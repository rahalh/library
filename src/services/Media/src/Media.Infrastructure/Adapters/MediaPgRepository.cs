namespace Media.Infrastructure.Adapters
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application;
    using Configuration;
    using Dapper;
    using Domain;
    using Domain.Services;
    using Exceptions;
    using Npgsql;

    public class MediaPgRepository : IMediaRepository
    {
        private readonly string connectionString;

        public MediaPgRepository(PostgresqlSettings settings) => this.connectionString = settings.ConnectionString;

        public async Task SaveAsync(Media media, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync(token);
            using var command = new NpgsqlCommand(@"
                insert into media(
                    external_id,
                    title,
                    description,
                    language_code,
                    publish_date,
                    media_type
                ) values
                (
                    @externalId,
                    @title,
                    @description,
                    @languageCode,
                    @publishDate,
                    @mediaType
                )",
                connection);
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new("externalId", media.ExternalId), new("title", media.Title),
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
                    throw new ConflictException($"An entity with ExternalId = '{media.ExternalId}' already exists");
                }
                throw;
            }
        }

        public async Task<Media> FetchByIdAsync(string id, CancellationToken token)
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
                total_views as TotalViews,
                content_url as ContentURL
                from media where external_id = @id";
            var res = await connection.QueryFirstOrDefaultAsync<Media>(new CommandDefinition(query, new {id},
                cancellationToken: token));
            return res;
        }

        public async Task RemoveAsync(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"delete from media where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id}, cancellationToken: token));
        }

        public async Task<IReadOnlyList<Media>> ListAsync(PaginationParams parameters, CancellationToken token)
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
                total_views as TotalViews,
                content_url as ContentURL
                from media
                where @token is null or update_time <= (select update_time from media where external_id = @token)
                order by update_time desc
                fetch first @size rows only";

            var res = await connection.QueryAsync<Media>(new CommandDefinition(query,
                new {parameters.Token, parameters.Size}, cancellationToken: token));
            return res.AsList();
        }

        public async Task SetViewCountAsync(string id, int viewCount, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set total_views = @viewCount where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id, viewCount}, cancellationToken: token));
        }

        public async Task SetContentURLAsync(string id, string url, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set content_url = @url where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id, url}, cancellationToken: token));
        }

        public async Task<bool> CheckExistsAsync(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"select count(*) from media where external_id = @id";
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(command, new {id}, cancellationToken: token));
        }
    }
}
