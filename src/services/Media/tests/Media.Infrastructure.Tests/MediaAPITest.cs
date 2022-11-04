namespace Media.Infrastructure.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Interactors;
    using Domain;
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

        [Theory]
        [InlineData(0, null, 4)]
        [InlineData(null, null, 4)]
        [InlineData(-1, null, 4)]
        [InlineData(null, "Bg7-4rPtC-Kl2fGh", 1)]
        [InlineData(2, "WfSPP636sByUECgl", 2)]
        [InlineData(5, null, 4)]
        // todo shouldn't be allowed
        // [InlineData(5, "", 4)]
        public async Task ListMediaEndpoint_ReturnsListOfItems(int? pageSize, string? token, int itemsCount)
        {
            // Arrange
            var query = new StringBuilder();
            if (pageSize is not null)
            {
                query.Append($"pageSize={pageSize}");
            }

            if (token is not null)
            {
                query.Append($"&pageToken={token}");
            }

            // Act
            var resp = await this.httpClient.GetAsync($"api/media?{query.ToString()}");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var media = JsonConvert.DeserializeObject<ListMediaResponse>(bodyJSON)!;
            media.Items.Count().ShouldBe(itemsCount);
            media.Items.Select(x => x.UpdateTime).ShouldBeInOrder(SortDirection.Descending);
        }

        [Fact]
        public async Task GetMediaEndpoint_WhenValidId_ReturnsMedia()
        {
            // Arrange
            var id = "UPj6SSMvaKIuXwnY";

            // Act
            var resp = await this.httpClient.GetAsync($"/api/media/{id}");
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var media = JsonConvert.DeserializeObject<Media>(bodyJSON)!;
            media.ExternalId.ShouldBe(id);
            media.TotalViews.ShouldBe(12);
        }

        [Fact]
        public async Task GetMediaEndpoint_WhenInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = "invalidId";

            // Act
            var resp = await this.httpClient.GetAsync($"api/media/{id}");

            // Assert
            resp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // todo use Kafkadrop's REST API
        // [Fact]
        // public async Task RemoveMediaEndpoint_WhenValidId_DeletesMedia()
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

        [Theory]
        [InlineData("Title", "Description", "en", "book", "2022-01-13T16:25:35", true)]
        [InlineData("Title", "Description", "en", "invalid_media_type", "2022-04-11T09:16:35", false)]
        [InlineData("Title", "Description", "en", "book", null, false)]
        [InlineData("Title", "Description", "", "book", "2022-04-11T09:16:35", false)]
        [InlineData("Title", null, "en", "book", "2022-04-11T09:16:35", false)]
        public async Task CreateMediaEndpoint_WhenParamsAreValid_ReturnsNewMedia(string title, string desc, string languageCode, string mediaType, string? publishDate, bool valid)
        {
            // Act
            var resp = await this.httpClient.PostAsJsonAsync("api/media", new
            {
                title, description = desc, languageCode, mediaType, publishDate
            });
            var bodyJSON = await resp.Content.ReadAsStringAsync();

            // Assert
            if (!valid)
            {
                resp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
                return;
            }
            resp.StatusCode.ShouldBe(HttpStatusCode.Created);
            bodyJSON.ShouldNotBeNullOrEmpty();

            var createdMedia = JsonConvert.DeserializeObject<Media>(bodyJSON)!;
            createdMedia.TotalViews.ShouldBe(0);
            createdMedia.Title.ShouldBe(title);
            createdMedia.Description.ShouldBe(desc);
            createdMedia.MediaType.ShouldBe(mediaType, StringCompareShould.IgnoreCase);
            createdMedia.PublishDate.ShouldBe(DateTime.Parse(publishDate));
        }

        public async Task InitializeAsync() => await DBHelper.Reset(this.fixture.PgConnectionString);

        public async Task DisposeAsync() { }
    }
}
