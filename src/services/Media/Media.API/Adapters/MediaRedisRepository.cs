namespace Media.API.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using StackExchange.Redis;

    public class MediaRedisRepository : IMediaRepository
    {
        private readonly IMediaRepository repo;
        private readonly IDatabase redis;
        private readonly ILogger logger;

        // todo redis async op hangs, when no connection is available.
        public MediaRedisRepository(ILogger logger, IConfiguration config, IMediaRepository repo)
        {
            this.repo = repo;
            this.redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")).GetDatabase();
            this.logger = logger.ForContext<MediaRedisRepository>();
        }

        public async Task Save(Media media, CancellationToken token) => await this.repo.Save(media, token);

        public async Task<Media> FetchByID(string id, CancellationToken token)
        {
            var cachedData = await this.GetValueAsync<Media>(this.redis, $"media:{id}");
            if (cachedData is not null)
            {
                return cachedData;
            }

            var media = await this.repo.FetchByID(id, token);
            this.redis.StringSet($"media:{id}", JsonSerializer.Serialize(media), flags: CommandFlags.FireAndForget);
            return media;
        }

        public async Task Remove(string id, CancellationToken token)
        {
            await this.repo.Remove(id, token);
            this.redis.KeyDelete($"media:{id}", flags: CommandFlags.FireAndForget);
        }

        public async Task<List<Media>> List(PaginationParams parameters, CancellationToken token) =>
            await this.repo.List(parameters, token);

        public async Task IncrementViewCount(string id, CancellationToken token)
        {
            await this.repo.IncrementViewCount(id, token);

            var media = await this.GetValueAsync<Media>(this.redis, $"media:{id}");
            if (media is not null)
            {
                media.TotalViews++;
                this.redis.StringSet($"media:{id}", JsonSerializer.Serialize(media), flags: CommandFlags.FireAndForget);
            }
        }

        private async Task<TObject> GetValueAsync<TObject>(IDatabase cache, string key)
            where TObject : class
        {
            try
            {
                var data = await cache.StringGetAsync(key);
                return data.IsNull ? default : JsonSerializer.Deserialize<TObject>(data);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return default;
            }
        }
    }
}
