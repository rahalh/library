namespace Blob.Tests.UnitTests.Interactors
{
    using System;
    using System.Threading;
    using API.Core;
    using API.Core.Exceptions;
    using API.Core.Interactors;
    using Moq;
    using Serilog.Core;
    using Shouldly;
    using Xunit;

    public class DeleteBlobInteractorTest
    {
        private readonly DeleteBlobInteractor interactor;
        private readonly Mock<IBlobRepository> repo;
        private readonly Mock<IFileStore> fileStore;
        private readonly Mock<IEventProducer> eventProducer;

        public DeleteBlobInteractorTest()
        {
            this.repo = new Mock<IBlobRepository>();
            this.fileStore = new Mock<IFileStore>();
            this.eventProducer = new Mock<IEventProducer>();
            this.interactor = new DeleteBlobInteractor(Logger.None, this.repo.Object, this.fileStore.Object, this.eventProducer.Object);
        }

        [Fact]
        public async void DeleteBlob_WhenBlobExists_DeletesBlob()
        {
            // Arrange
            var sampleBlob = new Blob("Id", "Application", "PDF", 1200, "bucketName.eu-west", "");
            this.repo.Setup(x => x.GetByIdAsync(sampleBlob.Id, CancellationToken.None)).ReturnsAsync(sampleBlob);

            // Act
            await Should.NotThrowAsync(async () => await this.interactor.HandleAsync(new DeleteBlobRequest(sampleBlob.Id), CancellationToken.None));

            // Assert
            this.repo.Verify(x => x.RemoveAsync(sampleBlob.Id, CancellationToken.None), Times.Once);
            this.fileStore.Verify(x => x.RemoveAsync(sampleBlob.Name, CancellationToken.None), Times.Once);
            this.eventProducer.Verify(x =>
                x.ProduceAsync(It.Is<Event<BlobDeletedEventMessage>>(x =>
                    x.EventType == ProducedEvents.BlobRemoved && x.Content == new BlobDeletedEventMessage(sampleBlob.Id)
                ), CancellationToken.None), Times.Once
            );
        }

        [Fact]
        public async void DeleteBlob_WhenBlobDoesntExist_Throws()
        {
            var blob = new Blob("Id", "Application", "PDF", 1200, "bucketName.eu-west", "");
            this.repo.Setup(x => x.GetByIdAsync(blob.Id, CancellationToken.None)).ReturnsAsync((Blob) null);
            await Should.ThrowAsync<NotFoundException>(async () => await this.interactor.HandleAsync(new DeleteBlobRequest(blob.Id), CancellationToken.None));
        }

        [Fact]
        public async void WhenAnOperationFails_ThrowsException()
        {
            // Arrange
            var sampleBlob = new Blob("Id", "Application", "PDF", 1200, "bucketName.eu-west", "");
            this.repo.Setup(x => x.GetByIdAsync(sampleBlob.Id, CancellationToken.None)).ReturnsAsync(sampleBlob);
            this.fileStore.Setup(x => x.RemoveAsync(sampleBlob.Name, CancellationToken.None)).Throws<Exception>();

            // Act
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(new DeleteBlobRequest(sampleBlob.Id), CancellationToken.None));

            // Assert
            this.repo.Verify(x => x.RemoveAsync(sampleBlob.Id, CancellationToken.None), Times.Never);
            this.eventProducer.Verify(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None), Times.Never);
        }
    }
}
