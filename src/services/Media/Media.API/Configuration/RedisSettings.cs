namespace Media.API.Configuration
{
    using System;

    public class RedisSettings
    {
        public string ConnectionString { get; }

        public RedisSettings(string connectionString) => this.ConnectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException($"{nameof(this.ConnectionString)} is missing");
    }
}
