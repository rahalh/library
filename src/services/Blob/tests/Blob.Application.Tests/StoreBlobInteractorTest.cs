namespace Blob.Application.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using Domain;
    using Domain.Services;
    using Exceptions;
    using Infrastructure.Configuration;
    using Interactors;
    using Moq;
    using Serilog.Core;
    using Services;
    using Shouldly;
    using Xunit;

    // SUT
    public class StoreBlobInteractorTest
    {
        private readonly StoreBlobInteractor interactor;
        private readonly Mock<IBlobRepository> repo;
        private readonly Mock<IFileStore> fileStore;
        private readonly Mock<IEventProducer> eventProducer;
        private readonly S3Settings s3Settings;

        public StoreBlobInteractorTest()
        {
            this.repo = new Mock<IBlobRepository>();
            this.fileStore = new Mock<IFileStore>();
            this.eventProducer = new Mock<IEventProducer>();
            this.s3Settings = new S3Settings() { BucketName = "bucket-name", StorageDomain = "storage-domain"};
            this.interactor = new StoreBlobInteractor(Logger.None, this.repo.Object, this.fileStore.Object, this.eventProducer.Object, this.s3Settings.StorageDomain, this.s3Settings.Prefix ?? string.Empty);
        }

        [Theory]
        [InlineData("application", "pdf", 10, false)]
        [InlineData("unsupported_content_type", "pdf", 10, true)]
        [InlineData("audio", "pdf", 10, true)]
        [InlineData("audio", "mp3", 2.30e+8, false)]
        [InlineData("audio", "mp3", 2.70e+8, true)]
        [InlineData("Application", "json", 2.4e+7, true)]
        [InlineData("Application", "pdf", 2.4e+7, false)]
        [InlineData("video", "mp4", 7e+9, false)]
        [InlineData("video", "mp4", 9e+9, true)]
        public async void WhenReqIsInvalid_ThrowsValidationException(string blobType, string extension, long size, bool throws)
        {
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", size, blobType, extension, content);

            if (!throws)
            {
                await Should.NotThrowAsync(async () => await this.interactor.HandleAsync(req, CancellationToken.None));
                return;
            }
            await Should.ThrowAsync<ValidationException>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));
        }

        [Fact]
        public async void WhenReqIsValidAndAllOperationsSucceed_ReturnsBlob()
        {
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", 12000, BlobTypes.Application.ToString(), "PDF", content);
            var blob = new Blob(req.Id, req.BlobType, req.Extension, req.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);
            var message = new BlobUploadedEventMessage(blob.Id, blob.URL);
            var @event = new Event<BlobUploadedEventMessage>(DateTime.Now, ProducedEvents.BlobUploaded, message);

            var res = await this.interactor.HandleAsync(req, CancellationToken.None);

            this.fileStore.Verify(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SaveAsync(It.Is<Blob>(y => y.Id == blob.Id), CancellationToken.None), Times.Once);
            this.eventProducer.Verify(x =>
                x.ProduceAsync(It.Is<Event<BlobUploadedEventMessage>>(y =>
                    y.EventType == @event.EventType && y.Content == message
                ), CancellationToken.None), Times.Once);

            res.ShouldNotBeNull();
            res.Id.ShouldBe(blob.Id);
            res.Name.ShouldBe(blob.Name);
            res.Size.ShouldBe(blob.Size);
            res.BlobType.ShouldBe(blob.BlobType);
            res.Extension.ShouldBe(blob.Extension);
            res.URL.ShouldBe(blob.URL);
        }

        [Fact]
        public async void WhenReqIsValidAndFileStoreOperationFails_ThrowsException()
        {
            // Arrange
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", 12000, BlobTypes.Application.ToString(), "PDF", content);
            var blob = new Blob(req.Id, req.BlobType, req.Extension, req.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);

            this.fileStore.Setup(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None)).Throws<Exception>();

            // Act + Assert
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            this.fileStore.Verify(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SaveAsync(It.IsAny<Blob>(), CancellationToken.None), Times.Never);
            this.eventProducer.Verify(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async void WhenReqIsValidAndRepoSaveOperationFails_ThrowsExceptionAndRollsBackChanges()
        {
            // Arrange
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", 12000, BlobTypes.Application.ToString(), "PDF", content);
            var blob = new Blob(req.Id, req.BlobType, req.Extension, req.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);

            this.repo.Setup(x => x.SaveAsync(It.IsAny<Blob>(), CancellationToken.None)).Throws<Exception>();

            // Act + Assert
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            this.fileStore.Verify(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SaveAsync(It.Is<Blob>(y => y.Id == req.Id), CancellationToken.None), Times.Once);
            this.eventProducer.Verify(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None), Times.Never);

            // check rollback ops
            this.fileStore.Verify(x => x.RemoveAsync(blob.Name, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void WhenReqIsValidAndEventPublishingFails_ThrowsExceptionAndRollsBackChanges()
        {
            // Arrange
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", 12000, BlobTypes.Application.ToString(), "PDF", content);
            var blob = new Blob(req.Id, req.BlobType, req.Extension, req.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);

            this.eventProducer.Setup(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None)).Throws<Exception>();

            // Act + Assert
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            this.fileStore.Verify(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SaveAsync(It.Is<Blob>(y => y.Id == req.Id), CancellationToken.None), Times.Once);
            this.eventProducer.Verify(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None), Times.Once);

            // check rollback ops
            this.fileStore.Verify(x => x.RemoveAsync(blob.Name, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.RemoveAsync(req.Id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void WhenReqIsValidAndRollbackFails_ThrowsException()
        {
            // Arrange
            var content = new MemoryStream();
            var req = new StoreBlobRequest("valid_id", 12000, BlobTypes.Application.ToString(), "PDF", content);
            var blob = new Blob(req.Id, req.BlobType, req.Extension, req.Size, this.s3Settings.StorageDomain, this.s3Settings.Prefix);

            this.eventProducer.Setup(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None)).Throws<Exception>();
            this.fileStore.Setup(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None)).Throws<Exception>();

            // Act + Assert
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            this.fileStore.Verify(x => x.StoreAsync(blob.Name, req.Content, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SaveAsync(It.Is<Blob>(y => y.Id == req.Id), CancellationToken.None), Times.Once);
            this.eventProducer.Verify(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None), Times.Once);

            // check rollback ops
            this.repo.Verify(x => x.RemoveAsync(req.Id, CancellationToken.None), Times.Once);
            this.fileStore.Verify(x => x.RemoveAsync(blob.Name, CancellationToken.None), Times.Once);
        }
    }
}
