namespace Media.Test
{
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Npgsql;
    using Xunit;

    public sealed class TestContainer : IAsyncLifetime
    {
        private readonly TestcontainerDatabase testcontainers = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "db_test", Username = "postgres", Password = "postgres",
            })
            .Build();

        [Fact]
        public void ExecuteCommand()
        {
            using (var connection = new NpgsqlConnection(this.testcontainers.ConnectionString))
            {
                using (var command = new NpgsqlCommand())
                {
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = "SELECT 1";
                    command.ExecuteReader();
                }
            }
        }

        public Task InitializeAsync() => this.testcontainers.StartAsync();

        public Task DisposeAsync() => this.testcontainers.DisposeAsync().AsTask();
    }
}
