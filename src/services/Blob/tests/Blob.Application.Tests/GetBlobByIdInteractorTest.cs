namespace Blob.Application.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Services;
    using Exceptions;
    using Interactors;
    using Moq;
    using Shouldly;
    using Xunit;

    // SUT
    public class GetBlobByIdInteractorTest
    {
        private readonly GetBlobByIdInteractor interactor;
        private readonly Mock<IBlobRepository> repo;

        public GetBlobByIdInteractorTest()
        {
            this.repo = new Mock<IBlobRepository>();
            this.interactor = new GetBlobByIdInteractor(this.repo.Object);
        }

        [Fact]
        public async Task WhenIdIsNotFound_ThrowsNotFoundException()
        {
            // Arrange
            this.repo.Setup(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult<Blob>(null));

            // Assert
            Should.Throw<NotFoundException>(async () => await this.interactor.HandleAsync(new GetBlobByIdRequest("validId"), CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdIsFound_ReturnBlobDTO()
        {
            var blob = new Blob("id", "applciation", "pdf", 1200, "domain", "");
            // Arrange
            this.repo.Setup(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(blob));

            // Act
            var res = await this.interactor.HandleAsync(new GetBlobByIdRequest("validId"), CancellationToken.None);

            // Assert
            res.Id.ShouldBe(blob.Id);
            res.Name.ShouldBe(blob.Name);
            res.Size.ShouldBe(blob.Size);
            res.BlobType.ShouldBe(blob.BlobType);
            res.Extension.ShouldBe(blob.Extension);
            res.URL.ShouldBe(blob.URL);
            res.CreateTime.ShouldBe(blob.CreateTime);
            res.UpdateTime.ShouldBe(blob.UpdateTime);
        }
    }
}
