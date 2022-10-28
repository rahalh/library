namespace Media.API.Config
{
    using System;

    public class PostgresqlSettings
    {
        public string ConnectionString { get; }

        public PostgresqlSettings(string connectionString) => this.ConnectionString = string.IsNullOrEmpty(connectionString) ? throw new ArgumentNullException($"{nameof(ConnectionString)} is missing") : connectionString;
    }
}
