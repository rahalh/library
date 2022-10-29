namespace Media.Test.IntegrationTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using API.Adapters;
    using API.Configuration;
    using API.Core;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
       private readonly TestcontainerDatabase pgContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Port = Random.Shared.Next(4000, 5000), Database = "media", Username = "postgres", Password = "root",
                })
                .WithBindMount(ToAbsolute("./IntegrationTests/Adapters/scripts/"), "/docker-entrypoint-initdb.d/")
                .Build();

        private readonly TestcontainerDatabase redisContainer =
            new TestcontainersBuilder<RedisTestcontainer>()
                .WithDatabase(new RedisTestcontainerConfiguration()
                {
                    Port = Random.Shared.Next(4000, 5000)
                })
                .Build();

        public string PgConnectionString => this.pgContainer.ConnectionString;
        public string RedisConnectionString => this.redisContainer.ConnectionString;

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
            {
                // services.AddSingleton<IMediaRepository, MediaPgRepository>();
                services.AddSingleton(_ => new RedisSettings(this.RedisConnectionString));
                services.AddSingleton(_ => new PostgresqlSettings(this.PgConnectionString));
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

        private static string ToAbsolute(string path) => Path.GetFullPath(path);
    }
}
