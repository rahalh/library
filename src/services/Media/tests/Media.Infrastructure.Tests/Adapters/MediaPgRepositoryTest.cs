namespace Media.Infrastructure.Tests.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Configuration;
    using Domain;
    using Helpers;
    using Infrastructure.Adapters;
    using Shouldly;
    using TestContainers;
    using Xunit;

    public class MediaPgRepositoryTest : IClassFixture<PgTestContainer>, IAsyncLifetime
    {
        private readonly string connectionString;
        private readonly MediaPgRepository repo;

        public MediaPgRepositoryTest(PgTestContainer fixture)
        {
            this.connectionString = fixture.ConnectionString;
            this.repo = new MediaPgRepository(new PostgresqlSettings {ConnectionString = this.connectionString});
        }

        [Theory]
        [InlineData(10, null, new[] {"WfSPP636sByUECgl", "jZeM577fgcSrulKc", "UPj6SSMvaKIuXwnY", "Bg7-4rPtC-Kl2fGh"})]
        [InlineData(2, null, new[] {"WfSPP636sByUECgl", "jZeM577fgcSrulKc"})]
        [InlineData(2, "UPj6SSMvaKIuXwnY", new[] {"UPj6SSMvaKIuXwnY", "Bg7-4rPtC-Kl2fGh"})]
        [InlineData(3, "Bg7-4rPtC-Kl2fGh", new[] {"Bg7-4rPtC-Kl2fGh"})]
        public async Task FetchMedia_ReturnsListOfMedia(int pageSize, string? pageToken, string[] expectedIds)
        {
            var res = await this.repo.ListAsync(pageSize, pageToken, CancellationToken.None);

            res.ShouldNotBeNull();
            res.ShouldBeOfType<List<Media>>();
            res.Select(x => x.ExternalId).ShouldBe(expectedIds);
            res.Select(x => x.UpdateTime).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetById_WhenIdExists_ReturnsMedia()
        {
            var id = "UPj6SSMvaKIuXwnY";
            var media = await this.repo.FetchByIdAsync(id, CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<Media>();
            media.ExternalId.ShouldBe(id);
        }

        [Fact]
        public async Task GetById_WhenIdNotFound_ReturnsNull()
        {
            var media = await this.repo.FetchByIdAsync("invalid_id", CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task Remove_WhenIdExists_ItemDeleted()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            await this.repo.RemoveAsync(id, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchByIdAsync(id, CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task SetViewCount()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            var res = await this.repo.FetchByIdAsync(id, CancellationToken.None);
            var count = res!.TotalViews;
            // Act
            await this.repo.SetViewCountAsync(id, ++count, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchByIdAsync(id, CancellationToken.None);
            media.ShouldNotBeNull();
            media.TotalViews.ShouldBe(count);
        }

        [Fact]
        public async Task SetContentURL()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            var url = "https://domain.com";

            // Act
            await this.repo.SetContentURLAsync(id, url, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchByIdAsync(id, CancellationToken.None);
            media.ShouldNotBeNull();
            media.ContentURL.ShouldNotBeNull();
            media.ContentURL.ShouldBe(url);
        }

        [Fact]
        public async Task SaveMedia()
        {
            // Arrange
            var fixture = new Fixture();
            var media = fixture.Create<Media>();
            media.MediaType = "BOOK";
            media.LanguageCode = "en";

            // Act
            await this.repo.SaveAsync(media, CancellationToken.None);

            // Assert
            var res = await this.repo.FetchByIdAsync(media.ExternalId, CancellationToken.None);
            res.ShouldNotBeNull();
            res.ExternalId.ShouldBe(media.ExternalId);
            res.Title.ShouldBe(media.Title);
            res.Description.ShouldBe(media.Description);
            res.PublishDate.ShouldBe(media.PublishDate.Date);
            res.LanguageCode.ShouldBe(media.LanguageCode);
            res.MediaType.ShouldBe(media.MediaType);
        }

        [Fact]
        public async Task CheckExists_WhenDoesntExist_ReturnsFalse() {
            // Arrange
            var id = "invalidId";

            // Act
            var exists = await this.repo.CheckExistsAsync(id, CancellationToken.None);

            // Assert
            exists.ShouldBe(false);
        }

        [Fact]
        public async Task CheckExists_WhenExists_ReturnsTrue() {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            var exists = await this.repo.CheckExistsAsync(id, CancellationToken.None);

            // Assert
            exists.ShouldBe(true);
        }

        public async Task InitializeAsync() => await DBHelper.Reset(this.connectionString);

        public async Task DisposeAsync() { }
    }
}
