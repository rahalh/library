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

    public class DeleteBlobSagaInteractorTest
    {
        private readonly DeleteBlobSagaInteractor interactor;
        private readonly Mock<IBlobRepository> repo;
        private readonly Mock<IFileStore> fileStore;

        public DeleteBlobSagaInteractorTest()
        {
            this.repo = new Mock<IBlobRepository>();
            this.fileStore = new Mock<IFileStore>();
            this.interactor = new DeleteBlobSagaInteractor(Logger.None, this.repo.Object, this.fileStore.Object);
        }

        [Fact]
        public async void WhenIdNotFound_ThrowsNotFoundException()
        {
            // Arrange
            this.repo.Setup(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync((Blob) null);

            // Assert
            await Should.ThrowAsync<NotFoundException>(async () => await this.interactor.HandleAsync(new DeleteBlobSagaRequest("id"), CancellationToken.None));
        }

        [Fact]
        public async void WhenIdIsFound_DeletesBlob()
        {
            // Arrange
            var blob = new Blob("id", "application", "pdf", 12000, "domain", "");
            this.repo.Setup(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(blob);

            // Act
            await this.interactor.HandleAsync(new DeleteBlobSagaRequest(blob.Id), CancellationToken.None);

            // Assert
            this.repo.Verify(x => x.RemoveAsync(blob.Id, CancellationToken.None), Times.Once);
            this.fileStore.Verify(x => x.RemoveAsync(blob.Name, CancellationToken.None), Times.Once);
        }
    }
}
