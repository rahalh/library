namespace Media.API.Adapters
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Exceptions;
    using Media.API.Core;
    using Microsoft.Extensions.Configuration;
    using Npgsql;

    public class MediaPgRepository : IMediaRepository
    {
        private readonly string connectionString;

        public MediaPgRepository(IConfiguration config) =>
            this.connectionString = config.GetConnectionString("PostgreSQL");

        public async Task Save(Media media, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync(token);
            using var command = new NpgsqlCommand(@"
                insert into media(external_id, title, description, language_code, publish_date, media_type) values (@externalId, @title, @description, @languageCode, @publishDate, @mediaType)",
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
                total_views as TotalViews
                from media where external_id = @id";
            var res = await connection.QueryFirstOrDefaultAsync<Media>(new CommandDefinition(query, new {id},
                cancellationToken: token));
            if (res is null)
            {
                throw new NotFoundException();
            }

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
                total_views as TotalViews
                from media
                where @token is null or external_id >= @token
                order by total_views desc
                fetch first @size rows only";

            var res = await connection.QueryAsync<Media>(new CommandDefinition(query,
                new {token = parameters.Token, size = parameters.Size}, cancellationToken: token));
            return res.AsList();
        }

        public Task IncrementViewCount(string id, CancellationToken token) =>
            throw new System.NotImplementedException();

        public async Task IncremenTViewCount(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set total_views = total_views + 1 where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id}, cancellationToken: token));
        }

        public async Task SetContentURL(string id, string url, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var command = @"update media set content_url = @url where external_id = @id";
            await connection.ExecuteAsync(new CommandDefinition(command, new {id, url}, cancellationToken: token));
        }
    }
}
