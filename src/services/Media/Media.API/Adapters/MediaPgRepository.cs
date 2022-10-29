namespace Media.API.Adapters
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Dapper;
    using Exceptions;
    using Media.API.Core;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using NpgsqlTypes;

    public class MediaPgRepository : IMediaRepository
    {
        private readonly string connectionString;

        public MediaPgRepository(PostgresqlSettings settings) => this.connectionString = settings.ConnectionString;

        public async Task Save(Media media, CancellationToken token)
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
                    throw new EntityExistsException();
                }

                throw;
            }
        }

        public async Task<Media> FetchById(string id, CancellationToken token)
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

        public async Task Remove(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"delete from media where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id}, cancellationToken: token));
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

        public async Task SetViewCount(string id, int viewCount, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set total_views = @viewCount where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id, viewCount}, cancellationToken: token));
        }

        public async Task SetContentURL(string id, string url, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set content_url = @url where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id, url}, cancellationToken: token));
        }
    }
}
