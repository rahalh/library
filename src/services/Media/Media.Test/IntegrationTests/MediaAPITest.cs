namespace Media.Test.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using API.Core;
    using API.Core.Interactors;
    using Helpers;
    using Newtonsoft.Json;
    using Shouldly;
    using Xunit;

    public class MediaAPITest : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
    {
        private readonly HttpClient httpClient;
        private readonly IntegrationTestFactory fixture;

        public MediaAPITest(IntegrationTestFactory fixture)
        {
            this.fixture = fixture;
            this.httpClient = fixture.CreateClient();
        }

        [Fact]
        public async Task ListMedia_WhenPaginationParamsMissing_ReturnsMediaList()
        {
            // Arrange
            // Act
            var resp = await this.httpClient.GetAsync("api/media");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var media = JsonConvert.DeserializeObject<ListMediaResponse>(bodyJSON);
            media.Items.Count().ShouldBe(4);
            media.Items.Select(x => x.UpdateTime).ShouldBeInOrder(SortDirection.Descending);
        }

        [Theory]
        [InlineData(2, 2)]
        [InlineData(5, 4)]
        [InlineData(0, 4)]
        [InlineData(-1, 4)]
        public async Task ListMedia_WhenPageSizeProvided_ReturnsMediaList(int requestedNumber, int actualNumber)
        {
            // Act
            var resp = await this.httpClient.GetAsync($"api/media?pageSize={requestedNumber}");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
            bodyJSON.ShouldNotBeNullOrEmpty();
            var media = JsonConvert.DeserializeObject<ListMediaResponse>(bodyJSON);
            media.Items.Count().ShouldBe(actualNumber);
            media.Items.Select(x => x.UpdateTime).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetMedia_WhenValidId_ReturnsMedia()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            var resp = await this.httpClient.GetAsync($"api/media/{id}");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var media = JsonConvert.DeserializeObject<Media>(bodyJSON);
            media.ExternalId.ShouldBe(id);
            media.TotalViews.ShouldBe(12);
        }

        [Fact]
        public async Task GetMedia_WhenInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = "invalidId";

            // Act
            var resp = await this.httpClient.GetAsync($"api/media/{id}");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // [Fact]
        // public async Task RemoveMedia_WhenValidId_DeletesMedia()
        // {
        //     // Arrange
        //     var id = "UPj6SSMvaKIuXwnY";
        //
        //     // Act
        //     var resp1 = await this.httpClient.DeleteAsync($"api/media/{id}");
        //     var bodyJSON1 = await resp1.Content.ReadAsStringAsync();
        //
        //     // var resp2 = await this.httpClient.GetAsync($"api/media/{id}");
        //     // var bodyJSON2 = await resp2.Content.ReadAsStringAsync();
        //
        //     // Assert
        //     resp1.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        //     bodyJSON1.ShouldBeEmpty();
        //     // resp2.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        //     // bodyJSON2.ShouldBeEmpty();
        // }

        [Fact]
        public async Task CreateMedia_WhenValidParams_ReturnsMedia()
        {
            // Arrange
            var media = new CreateMediaRequest("Title", "Description", "en", "book", DateTime.UtcNow);

            // Act
            var resp = await this.httpClient.PostAsJsonAsync("api/media", media);
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.Created);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var createdMedia = JsonConvert.DeserializeObject<Media>(bodyJSON);
            createdMedia.TotalViews.ShouldBe(0);
            createdMedia.Title.ShouldBe(media.Title);
            createdMedia.Description.ShouldBe(media.Description);
            createdMedia.MediaType.ShouldBe(media.MediaType, StringCompareShould.IgnoreCase);
            createdMedia.PublishDate.ShouldBe(media.PublishDate);
        }

        [Fact]
        public async Task CreateMedia_WhenInvalidMediaType_ReturnsMedia()
        {
            // Arrange
            var media = new CreateMediaRequest("Title", "Description", "en", "InvalidMedia", DateTime.UtcNow);

            // Act
            var resp = await this.httpClient.PostAsJsonAsync("api/media", media);

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        public async Task InitializeAsync() => await DBHelper.Reset(this.fixture.PgConnectionString);

        public async Task DisposeAsync() { }
    }
}
