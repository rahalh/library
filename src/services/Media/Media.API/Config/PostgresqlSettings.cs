namespace Media.API.Config
{
    using System;

    public class PostgresqlSettings
    {
        public string ConnectionString { get; }

        public PostgresqlSettings(string connectionString) => this.ConnectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException("PostgreSQL connectionstring is missing");
    }
}
