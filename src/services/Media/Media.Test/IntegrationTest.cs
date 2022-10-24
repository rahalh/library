namespace Media.Test
{
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;
    using Respawn;
    using Xunit;

    public class IntegrationTest : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Checkpoint checkpoint = new Checkpoint
        {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres,
            WithReseed = true
        };

        protected readonly ApiWebApplicationFactory factory;
        protected readonly HttpClient httpClient;
        protected readonly string pgConnectionString;

        public IntegrationTest(ApiWebApplicationFactory fixture)
        {
            this.factory = fixture;
            this.httpClient = this.factory.CreateClient();

            this.pgConnectionString = this.factory.Config.GetConnectionString("PostgreSQL");
            this.checkpoint.Reset(this.pgConnectionString).Wait();
        }
    }
}
