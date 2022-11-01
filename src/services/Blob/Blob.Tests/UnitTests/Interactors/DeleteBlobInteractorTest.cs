namespace Blob.Tests.UnitTests.Interactors
{
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
            var blob = new Blob("Id", "Application", "PDF", 1200, "bucketName.eu-west", "");
            this.repo.Setup(x => x.GetByIdAsync(blob.Id, CancellationToken.None)).ReturnsAsync(blob);

            await Should.NotThrowAsync(async () => await this.interactor.HandleAsync(new DeleteBlobRequest(blob.Id), CancellationToken.None));
        }

        [Fact]
        public async void DeleteBlob_WhenBlobDoesntExist_Throws()
        {
            var blob = new Blob("Id", "Application", "PDF", 1200, "bucketName.eu-west", "");
            this.repo.Setup(x => x.GetByIdAsync(blob.Id, CancellationToken.None)).ReturnsAsync((Blob) null);
            await Should.ThrowAsync<NotFoundException>(async () => await this.interactor.HandleAsync(new DeleteBlobRequest(blob.Id), CancellationToken.None));
        }
    }
}
