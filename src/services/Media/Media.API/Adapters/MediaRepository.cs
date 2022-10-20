namespace Media.API.Adapters
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Exceptions;
    using Media.API.Core;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using StackExchange.Redis;
    using CommandFlags = StackExchange.Redis.CommandFlags;

    public class MediaRepository : IMediaRepository
    {
        private readonly string connectionString;
        private readonly ConnectionMultiplexer redis;

        public MediaRepository(IConfiguration config)
        {
            this.connectionString = config.GetConnectionString("Default");

            var redisHost = config.GetSection("Redis").GetValue<string>("Host");
            var redisPort = config.GetSection("Redis").GetValue<string>("Port");
            var database = config.GetSection("Redis").GetValue<string>("Database");
            this.redis = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},defaultDatabase={database}");
        }

        public async Task Save(Media media, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync(token);
            using var command = new NpgsqlCommand(@"
                insert into media(external_id, title, description, language_code, publish_date, media_type) values (@externalId, @title, @description, @languageCode, @publishDate, @mediaType)",
                connection);
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new("externalId", media.ExternalID), new("title", media.Title),
                new("description", media.Description), new("languageCode", media.LanguageCode),
                new("publishDate", media.PublishDate),
                new("mediaType", media.MediaType) {DataTypeName = "media_enum"},
            });
            try
            {
                await command.ExecuteNonQueryAsync(token);
            }
            catch (NpgsqlException ex)
            {
                if (ex.ErrorCode == 23505)
                {
                    throw new EntityExistsException();
                }

                throw;
            }
        }

        public async Task<Media> FetchByID(string id, CancellationToken token)
        {
            var mem = this.redis.GetDatabase();
            var mediaJSON = await mem.StringGetAsync($"media:{id}");
            if (!mediaJSON.IsNull)
            {
                return JsonSerializer.Deserialize<Media>(mediaJSON);
            }

            await using var connection = new NpgsqlConnection(this.connectionString);
            var query = @"
                select
                external_id as externalId,
                title,
                description,
                language_code as languageCode,
                publish_date as publishDate,
                media_type as MediaType,
                create_time as CreateTime,
                update_time as UpdateTime,
                total_views as TotalViews
                from media where external_id = @id";
            var res = await connection.QueryFirstOrDefaultAsync<Media>(new CommandDefinition(query, new {id},
                cancellationToken: token));
            if (res is null)
            {
                throw new NotFoundException();
            }

            this.redis.GetDatabase().StringSet($"media:{id}", JsonSerializer.Serialize(res),
                flags: CommandFlags.FireAndForget);
            return res;
        }

        public async Task Remove(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"delete from media where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id}, cancellationToken: token));
            this.redis.GetDatabase().KeyDelete($"media:{id}", flags: CommandFlags.FireAndForget);
        }

        public async Task<List<Media>> List(PaginationParams parameters, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var query = @"
                select
                external_id as externalId,
                title,
                description,
                language_code as languageCode,
                publish_date as publishDate,
                media_type as MediaType,
                create_time as CreateTime,
                update_time as UpdateTime,
                total_views as TotalViews
                from media
                where @token is null or external_id >= @token
                order by total_views desc
                fetch first @size rows only";

            var res = await connection.QueryAsync<Media>(new CommandDefinition(query,
                new {token = parameters.Token, size = parameters.Size}, cancellationToken: token));
            return res.AsList();
        }

        public async Task IncrementViewCount(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set total_views = total_views + 1 where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id}, cancellationToken: token));

            var mediaJson = await this.redis.GetDatabase().StringGetAsync($"media:{id}");
            if (!mediaJson.IsNull)
            {
                var media = JsonSerializer.Deserialize<Media>(mediaJson);
                media.TotalViews++;
                this.redis.GetDatabase().StringSet($"media:{id}", JsonSerializer.Serialize(media));
            }
        }
    }
}
