namespace Media.Infrastructure.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Application;
    using Configuration;
    using Domain;
    using Domain.Services;
    using Serilog;
    using StackExchange.Redis;

    // optional cache => ignores errors
    // If the Redis becomes unavailable, the services need to continue to operate as if uncached
    public class MediaRedisRepository : IMediaRepository
    {
        private readonly IMediaRepository repo;
        private readonly ILogger logger;

        private static string GenerateKey(string id) => $"media:{id}";

        private readonly Lazy<ConnectionMultiplexer> lazyConnection;

        public ConnectionMultiplexer Connection => this.lazyConnection.Value;

        public IDatabase Redis => this.Connection.GetDatabase();

        // todo redis async op hangs, when no connection is available.
        public MediaRedisRepository(ILogger logger, RedisSettings settings, IMediaRepository repo)
        {
            this.repo = repo;
            this.lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(settings.ConnectionString));
            this.logger = logger.ForContext<MediaRedisRepository>();
        }

        public async Task SaveAsync(Media media, CancellationToken token) => await this.repo.SaveAsync(media, token);

        public async Task<Media?> FetchByIdAsync(string id, CancellationToken token)
        {
            var cachedData = await this.GetValueAsync<Media>(this.Redis, GenerateKey(id));
            if (cachedData is not null)
            {
                return cachedData;
            }

            var media = await this.repo.FetchByIdAsync(id, token);
            async Task action() => await this.Redis.StringSetAsync(GenerateKey(id), JsonSerializer.Serialize(media), TimeSpan.FromHours(1));
            await this.RunWithErrorHandler(action);
            return media;
        }

        public async Task RemoveAsync(string id, CancellationToken token)
        {
            await this.repo.RemoveAsync(id, token);
            async Task action() => await this.Redis.KeyDeleteAsync(GenerateKey(id));
            await this.RunWithErrorHandler(action);
        }

        public async Task<IReadOnlyList<Media>> ListAsync(int pageSize, string? pageToken, CancellationToken token) =>
            await this.repo.ListAsync(pageSize, pageToken, token);

        public async Task SetViewCountAsync(string id, int count, CancellationToken token)
        {
            await this.repo.SetViewCountAsync(id, count, token);

            var media = await this.GetValueAsync<Media>(this.Redis, GenerateKey(id));
            if (media is not null)
            {
                media.TotalViews = count;
                async Task action() => await this.Redis.StringSetAsync(GenerateKey(id), JsonSerializer.Serialize(media), TimeSpan.FromHours(1));
                // todo consider using fire and forget flag
                await this.RunWithErrorHandler(action);
            }
        }

        public async Task SetContentURLAsync(string id, string url, CancellationToken token)
        {
            await this.repo.SetContentURLAsync(id, url, token);

            var media = await this.GetValueAsync<Media>(this.Redis, GenerateKey(id));
            if (media is not null)
            {
                media.ContentURL = url;

                async Task action() => await this.Redis.StringSetAsync(GenerateKey(id), JsonSerializer.Serialize(media), TimeSpan.FromHours(1));
                await this.RunWithErrorHandler(action);
            }
        }

        public async Task<bool> CheckExistsAsync(string id, CancellationToken token) => await this.repo.CheckExistsAsync(id, token);

        private async Task<TObject?> GetValueAsync<TObject>(IDatabase cache, string key)
            where TObject : class
        {
            try
            {
                var data = await cache.StringGetAsync(key);
                return data.IsNull ? default : JsonSerializer.Deserialize<TObject>(data);
            }
            catch (Exception ex)
            {
                this.logger.Warning(ex, ex.Message);
                return default;
            }
        }

        private async Task RunWithErrorHandler(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                this.logger.Warning(ex, ex.Message);
            }
        }
    }
}
