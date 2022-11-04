namespace Media.Infrastructure.Tests.Adapters.TestContainers
{
    using System;
    using System.IO;
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
                    Port = Random.Shared.Next(4000, 5000), Database = "media", Username = "postgres", Password = "root",
                })
                .WithBindMount(ToAbsolute("./Adapters/scripts/"), "/docker-entrypoint-initdb.d/")
                .WithCleanUp(true)
                .Build();

        public string ConnectionString => this.testContainers.ConnectionString;

        public Task InitializeAsync() => this.testContainers.StartAsync();

        public Task DisposeAsync() => this.testContainers.DisposeAsync().AsTask();

        private static string ToAbsolute(string path) => Path.GetFullPath(path);
    }
}
