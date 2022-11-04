namespace Blob.Infrastructure.Tests.Adapters
{
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Domain;
    using Infrastructure.Adapters;
    using Shouldly;
    using TestContainerSetup;
    using Xunit;

    [Collection(nameof(LocalstackTestContainer))]
    public class DDBTest
    {
        private readonly string localstackUrl;

        public DDBTest(LocalstackTestContainer container) => this.localstackUrl = container.LocalstackUri;

        [Fact]
        public async Task SaveBlob()
        {
            // Arrange
            var repo = new DDBRepository(new DDBSettings("blob", this.localstackUrl));
            var blob = new Blob("id", "Application", "pdf", 1024, "domain", "prefix");

            // Act
            await repo.SaveAsync(blob, CancellationToken.None);
            var res = await repo.GetByIdAsync(blob.Id, CancellationToken.None);

            // Assert
            res.ShouldNotBeNull();
            res.ShouldBeOfType<Blob>();
            res.Id.ShouldBe(blob.Id);
            res.Name.ShouldBe(blob.Name);
            res.Size.ShouldBe(blob.Size);
            res.BlobType.ShouldBe(blob.BlobType);
            res.Extension.ShouldBe(blob.Extension);
            res.URL.ShouldBe(blob.URL);
        }

        [Fact]
        public async Task GetById_WhenInvalidId_ReturnsNull()
        {
            // Arrange
            var repo = new DDBRepository(new DDBSettings("blob", this.localstackUrl));

            // Act
            var res = await repo.GetByIdAsync("invalidId", CancellationToken.None);

            // Assert
            res.ShouldBeNull();
        }

        [Fact]
        public async Task GetById_WhenValidId_ReturnsBlob()
        {
            // Arrange
            var repo = new DDBRepository(new DDBSettings("blob", this.localstackUrl));
            var blob1 = new Blob("id1", "Application", "pdf", 1024, "domain", "prefix");
            var blob2 = new Blob("id2", "Application", "pdf", 1024, "domain", "prefix");
            await repo.SaveAsync(blob1, CancellationToken.None);
            await repo.SaveAsync(blob2, CancellationToken.None);

            // Act
            var res = await repo.GetByIdAsync("id2", CancellationToken.None);

            // Assert
            res.ShouldNotBeNull();
            res.ShouldBeOfType<Blob>();
            res.Id.ShouldBe(blob2.Id);
            res.Name.ShouldBe(blob2.Name);
            res.Size.ShouldBe(blob2.Size);
            res.BlobType.ShouldBe(blob2.BlobType);
            res.Extension.ShouldBe(blob2.Extension);
            res.URL.ShouldBe(blob2.URL);
        }

        [Fact]
        public async Task RemoveBlob_WhenValidId_PerformOp()
        {
            // Arrange
            var repo = new DDBRepository(new DDBSettings("blob", this.localstackUrl));
            var blob = new Blob("id", "Application", "pdf", 1024, "domain", "prefix");
            await repo.SaveAsync(blob, CancellationToken.None);

            // Act
            await repo.RemoveAsync(blob.Id, CancellationToken.None);
            var res = await repo.GetByIdAsync(blob.Id, CancellationToken.None);

            // Assert
            res.ShouldBeNull();
        }
    }
}
