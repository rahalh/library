namespace Media.Infrastructure.Adapters;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Services;
using Polly;
using Polly.CircuitBreaker;
using Serilog;
using StackExchange.Redis;

public class MediaRedisRepository : IMediaRepository
{
    private readonly IMediaRepository repo;
    private readonly ILogger logger;

    private readonly CircuitBreakerPolicy circuitBreakerPolicy;
    private readonly IDatabase database;

    public MediaRedisRepository(ILogger logger, IMediaRepository repo, IConnectionMultiplexer connection)
    {
        this.repo = repo;
        this.database = connection.GetDatabase();
        this.logger = logger.ForContext<MediaRedisRepository>();

        this.circuitBreakerPolicy = Policy.Handle<Exception>()
            .CircuitBreaker(exceptionsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromMinutes(5),
                onBreak: (ex, duration) => this.logger.Warning(ex, $"Circuit open for duration {duration}"),
                onReset: () => this.logger.Information("Circuit closed and is allowing requests through"),
                onHalfOpen: () =>
                    this.logger.Information(
                        "Circuit is half-opened and will test the service with the next request"));
    }

    public async Task SaveAsync(Media media, CancellationToken token) => await this.repo.SaveAsync(media, token);

    public async Task<Media?> FetchByIdAsync(string id, CancellationToken token)
    {
        var data = await this.GetValueAsync<Media?>(this.GenerateKey(id), token);
        if (data is not null)
        {
            return data;
        }

        var media = await this.repo.FetchByIdAsync(id, token);
        return media;
    }

    public async Task RemoveAsync(string id, CancellationToken token)
    {
        await this.repo.RemoveAsync(id, token);
        try
        {
            await this.circuitBreakerPolicy.Execute(
                () => this.database.KeyDeleteAsync(this.GenerateKey(id))
            );
        }
        catch (Exception e)
        {
            this.logger.Warning(e, e.GetType().ToString());
        }
    }

    public async Task<IReadOnlyList<Media>> ListAsync(int pageSize, string? pageToken, CancellationToken token) =>
        await this.repo.ListAsync(pageSize, pageToken, token);

    public async Task SetViewCountAsync(string id, int count, CancellationToken token)
    {
        await this.repo.SetViewCountAsync(id, count, token);

        var data = await this.GetValueAsync<Media?>(this.GenerateKey(id), token);
        if (data is not null)
        {
            data.TotalViews = count;
            try
            {
                await this.circuitBreakerPolicy.Execute(
                    () => this.database.StringSetAsync(this.GenerateKey(id), JsonSerializer.Serialize(data),
                        TimeSpan.FromHours(1))
                );
            }
            catch (Exception e)
            {
                this.logger.Warning(e, e.GetType().ToString());
            }
        }
    }

    public async Task SetContentURLAsync(string id, string url, CancellationToken token)
    {
        await this.repo.SetContentURLAsync(id, url, token);
        try
        {
            await this.circuitBreakerPolicy.Execute(
                () => this.database.KeyDeleteAsync(this.GenerateKey(id))
            );
        }
        catch (Exception e)
        {
            this.logger.Warning(e, e.GetType().ToString());
        }
    }

    public async Task<bool> CheckExistsAsync(string id, CancellationToken token) =>
        await this.repo.CheckExistsAsync(id, token);

    private async Task<TObject?> GetValueAsync<TObject>(string key, CancellationToken token)
        where TObject : class?
    {
        try
        {
            var dataJSON = await this.circuitBreakerPolicy.Execute(() => this.database.StringGetAsync(key));
            return dataJSON.IsNull ? default : JsonSerializer.Deserialize<TObject>(dataJSON!);
        }
        catch (Exception ex)
        {
            this.logger.Warning(ex, ex.GetType().ToString());
            return default;
        }
    }

    private string GenerateKey(string id) => $"media:{id}";
}
