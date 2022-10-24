namespace Media.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using API.Core;
    using Dapper;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Npgsql;
    using Xunit;

    public class ListMediaTest : IntegrationTest
    {
        public ListMediaTest(ApiWebApplicationFactory fixture) : base(fixture) {}

        [Fact]
        public async Task GET_list_media_wo_parameters()
        {
            var data = new List<Media>
            {
                new Media() {Title = "Title 8", Description = "test1", PublishDate = new DateTime()},
                new Media() {Title = "Title 7", Description = "test2", PublishDate = new DateTime()}
            };
            await this.InsertDB(data);

            var resp = await this.httpClient.GetAsync("api/media");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var forecast = JsonConvert.DeserializeObject<Media[]>(
                await resp.Content.ReadAsStringAsync()
            );
            forecast.Should().HaveCount(2);
        }

        private async Task InsertDB(List<Media> items)
        {
            await using var connection = new NpgsqlConnection(this.pgConnectionString);
            await connection.OpenAsync();
            foreach (var item in items)
            {
                using var command = new NpgsqlCommand(@"
                insert into media(external_id, title, description, language_code, publish_date, media_type) values (@externalId, @title, @description, @languageCode, @publishDate, @mediaType)",
                    connection);
                command.Parameters.AddRange(new NpgsqlParameter[]
                {
                    new("externalId", item.ExternalId),
                    new("title", item.Title),
                    new("description", item.Description),
                    new("languageCode", item.LanguageCode),
                    new("publishDate", item.PublishDate),
                    new("mediaType", item.MediaType) {DataTypeName = "media_enum"},
                });
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
