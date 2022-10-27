namespace Media.Test.IntegrationTests.Adapters.Helpers
{
    using StackExchange.Redis;

    public static class RedisHelper
    {
        public static void Reset(ConnectionMultiplexer connection)
        {
            var server = connection.GetServers()[0];
            server.FlushDatabase();
        }
    }
}
