namespace Media.Test.IntegrationTests.Adapters
{
    using System.Threading;
    using System.Threading.Tasks;
    using API.Adapters;
    using API.Config;
    using API.Core;
    using AutoFixture;
    using IntegrationTests.Helpers;
    using Serilog;
    using Shouldly;
    using StackExchange.Redis;
    using TestContainers;
    using Xunit;

    public class MediaRedisRepositoryTest : IClassFixture<RedisRepoTestContainer>, IAsyncLifetime
    {
        private readonly string pgConnectionString;

        private readonly MediaRedisRepository cache;

        public MediaRedisRepositoryTest(RedisRepoTestContainer fixture)
        {
            this.pgConnectionString = fixture.PgConnectionString;

            var config = ConfigurationOptions.Parse(fixture.RedisConnectionString);
            config.AllowAdmin = true;


            var repo = new MediaPgRepository(new PostgresqlSettings(fixture.PgConnectionString));
            this.cache = new MediaRedisRepository(Log.Logger, new RedisSettings(config.ToString()), repo);
        }

        [Fact]
        public async Task GetById_WhenFetchValidIdAndRedisIsUp_ReturnsMedia()
        {
            var id = "UPj6SSMvaKIuXwnY";
            var media = await this.cache.FetchById(id, CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<Media>();
            media.ExternalId.ShouldBe(id);
        }

        [Fact]
        public async Task GetById_WhenFetchValidIdAndRedisIsDown_ReturnsMedia()
        {
            var id = "UPj6SSMvaKIuXwnY";
            await this.cache.Connection.CloseAsync();
            var media = await this.cache.FetchById(id, CancellationToken.None);

            media.ShouldNotBeNull();
            media.ShouldBeOfType<Media>();
            media.ExternalId.ShouldBe(id);
        }

        [Fact]
        public async Task GetById_WhenFetchInValid_ReturnsNull()
        {
            var media = await this.cache.FetchById("invalid_id", CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task Remove_WhenValidId_ItemDeleted()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            await this.cache.Remove(id, CancellationToken.None);

            // Assert
            var media = await this.cache.FetchById(id, CancellationToken.None);
            media.ShouldBeNull();
        }

        [Fact]
        public async Task IncrementViewCount()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            await this.cache.SetViewCount(id, 12, CancellationToken.None);

            // Assert
            var media = await this.cache.FetchById(id, CancellationToken.None);
            media.ShouldNotBeNull();
            media.TotalViews.ShouldBe(12);
        }

        [Fact]
        public async Task IncrementViewCount_WhenRedisDown_PerformsOp()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            await this.cache.Connection.CloseAsync();

            // Act
            await this.cache.SetViewCount(id, 12, CancellationToken.None);

            // Assert
            var media = await this.cache.FetchById(id, CancellationToken.None);
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
            await this.cache.SetContentURL(id, url, CancellationToken.None);

            // Assert
            var media = await this.cache.FetchById(id, CancellationToken.None);
            media.ShouldNotBeNull();
            media.ContentURL.ShouldNotBeNull();
            media.ContentURL.ShouldBe(url);
        }

        [Fact]
        public async Task SetContentURL_WhenRedisDown_PerformsOp()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";
            var url = "url";
            await this.cache.Connection.CloseAsync();

            // Act
            await this.cache.SetContentURL(id, url, CancellationToken.None);

            // Assert
            var media = await this.cache.FetchById(id, CancellationToken.None);
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
            media.MediaType = "Book";
            media.LanguageCode = "en";

            // Act
            await this.cache.Save(media, CancellationToken.None);

            // Assert
            var res = await this.cache.FetchById(media.ExternalId, CancellationToken.None);
            res.ShouldNotBeNull();
            res.ExternalId.ShouldBe(media.ExternalId);
            res.Title.ShouldBe(media.Title);
            res.Description.ShouldBe(media.Description);
            res.PublishDate.ShouldBe(media.PublishDate.Date);
            res.LanguageCode.ShouldBe(media.LanguageCode);
            res.MediaType.ShouldBe(media.MediaType);
        }

        public async Task InitializeAsync()
        {
            await DBHelper.Reset(this.pgConnectionString);
            RedisHelper.Reset(this.cache.Connection);
        }

        public async Task DisposeAsync() { }
    }
}
