namespace Media.Test
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class ListMediaTest : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly HttpClient httpClient;

        public ListMediaTest(ApiWebApplicationFactory api)
        {
            this.httpClient = api.CreateClient();
        }

        [Fact]
        public async Task GET_list_media_wo_parameters()
        {
            var media = await this.httpClient.GetAsync("api/media");
            media.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
