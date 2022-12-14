namespace Media.Infrastructure.Tests.Adapters.TestContainers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Xunit;

    public class RedisPgTestContainer : IAsyncLifetime
    {
        private readonly TestcontainerDatabase pgContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Port = Random.Shared.Next(4000, 5000), Database = "media", Username = "postgres", Password = "root",
                })
                .WithBindMount(ToAbsolute("./Adapters/scripts/"), "/docker-entrypoint-initdb.d/")
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
