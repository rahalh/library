namespace Media.Test.IntegrationTests.Adapters.TestContainers
{
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Xunit;

    public class PgTestContainer : IAsyncLifetime
    {
        private readonly TestcontainerDatabase testContainers =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Port = 5431, Database = "media", Username = "postgres", Password = "root",
                })
                .WithBindMount("/tmp/scripts/", "/docker-entrypoint-initdb.d/")
                .Build();

        public string ConnectionString => this.testContainers.ConnectionString;

        public Task InitializeAsync() => this.testContainers.StartAsync();

        public Task DisposeAsync() => this.testContainers.DisposeAsync().AsTask();
    }
}
