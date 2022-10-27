namespace Media.Test.IntegrationTests.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using API.Adapters;
    using API.Config;
    using API.Core;
    using AutoFixture;
    using IntegrationTests.Helpers;
    using Shouldly;
    using TestContainers;
    using Xunit;

    // TODO parallel vs sequential execution
    public class MediaPgRepositoryTest : IClassFixture<PgRepoTestContainer>, IAsyncLifetime
    {
        private readonly string connectionString;
        private readonly MediaPgRepository repo;

        public MediaPgRepositoryTest(PgRepoTestContainer fixture)
        {
            this.connectionString = fixture.ConnectionString;
            this.repo = new MediaPgRepository(new PostgresqlSettings(this.connectionString));
        }

        [Fact]
        public async Task GetAllMedia_WhenFetch10Media_ReturnsListOf4Media()
        {
            var media = await this.repo.List(new PaginationParams(null, 10), CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<List<Media>>();
            media.Count.ShouldBe(4);
            media.Select(x => x.TotalViews).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetAllMedia_WhenFetchXMedia_ReturnsListOfXMedia()
        {
            var pageSize = 2;
            var media = await this.repo.List(new PaginationParams(null, pageSize), CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<List<Media>>();
            media.Count.ShouldBe(pageSize);
            media[0].ExternalId.ShouldBe("WfSPP636sByUECgl");
            media.Select(x => x.TotalViews).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetAllMedia_WhenFetchTokenMedia_ReturnsListOfMedia()
        {
            var id = "UPj6SSMvaKIuXwnY";
            var media = await this.repo.List(new PaginationParams(id, 3), CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<List<Media>>();
            media.Count.ShouldBe(2);
            media[0].ExternalId.ShouldBe(id);
            media.Select(x => x.TotalViews).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetById_WhenFetchValidId_ReturnsMedia()
        {
            var id = "UPj6SSMvaKIuXwnY";
            var media = await this.repo.FetchById(id, CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<Media>();
            media.ExternalId.ShouldBe(id);
        }

        [Fact]
        public async Task GetById_WhenFetchInValid_ReturnsNull()
        {
            var media = await this.repo.FetchById("invalid_id", CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task Remove_WhenValidId_ItemDeleted()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            await this.repo.Remove(id, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchById(id, CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task SetViewCount()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            var res = await this.repo.FetchById(id, CancellationToken.None);
            var count = res.TotalViews;
            // Act
            await this.repo.SetViewCount(id, ++count, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchById(id, CancellationToken.None);
            media.ShouldNotBeNull();
            media.TotalViews.ShouldBe(12);
        }

        [Fact]
        public async Task SetContentURL()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            var url = "url";

            // Act
            await this.repo.SetContentURL(id, url, CancellationToken.None);

            // Assert
            var media = await this.repo.FetchById(id, CancellationToken.None);
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
            await this.repo.Save(media, CancellationToken.None);

            // Assert
            var res = await this.repo.FetchById(media.ExternalId, CancellationToken.None);
            res.ShouldNotBeNull();
            res.ExternalId.ShouldBe(media.ExternalId);
            res.Title.ShouldBe(media.Title);
            res.Description.ShouldBe(media.Description);
            res.PublishDate.ShouldBe(media.PublishDate.Date);
            res.LanguageCode.ShouldBe(media.LanguageCode);
            res.MediaType.ShouldBe(media.MediaType);
        }

        public async Task InitializeAsync() => await DBHelper.Reset(this.connectionString);

        public async Task DisposeAsync() { }
    }
}
