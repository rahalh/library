namespace Media.Test
{
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
       private readonly TestcontainerDatabase pgContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Port = 5431, Database = "media", Username = "postgres", Password = "root",
                })
                .WithBindMount("/tmp/scripts/", "/docker-entrypoint-initdb.d/")
                .Build();

        private readonly TestcontainerDatabase redisContainer =
            new TestcontainersBuilder<RedisTestcontainer>()
                .WithDatabase(new RedisTestcontainerConfiguration()
                {
                    Port = 6380
                })
                .Build();

        public string PgConnectionString => this.pgContainer.ConnectionString;
        public string RedisConnectionString => this.redisContainer.ConnectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureAppConfiguration(config =>
            {
                var c = new ConfigurationBuilder()
                    .AddJsonFile("integrationsettings.json")
                    .Build();
                config.AddConfiguration(c);
            });

        public async Task InitializeAsync()
        {
            await this.pgContainer.StartAsync();
            await this.redisContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await this.pgContainer.DisposeAsync().AsTask();
            await this.redisContainer.DisposeAsync().AsTask();
        }
    }
}
