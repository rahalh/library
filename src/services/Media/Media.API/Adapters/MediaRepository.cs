namespace Media.API.Adapters
{
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using Media.API.Core;
    using Microsoft.Extensions.Configuration;
    using Npgsql;

    public class MediaRepository : IMediaRepository
    {
        private readonly string connectionString;

        public MediaRepository(IConfiguration config) => this.connectionString = config.GetConnectionString("Default");

        public async Task Save(Media media, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            await connection.OpenAsync(token);
            using var command = new NpgsqlCommand(@"insert into media(external_id, title, description, language_code, publish_date, media_type) values (@externalId, @title, @description, @languageCode, @publishDate, @mediaType)", connection);
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new ("externalId", media.ExternalID),
                new ("title", media.Title),
                new ("description", media.Description),
                new ("languageCode", media.LanguageCode),
                new ("publishDate", media.PublishDate),
                new ("mediaType", media.MediaType) { DataTypeName = "media_enum" },
            });
            await command.ExecuteNonQueryAsync(token);
            // TODO may throw conflict exception
        }

        public async Task<Media> FetchByID(string id, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(this.connectionString);
            var query = @"select external_id as externalId, title, description, language_code as languageCode, publish_date as publishDate, media_type as MediaType, create_time as CreateTime, update_time as UpdateTime from media where external_id = @id";
            var res = await connection.QueryFirstOrDefaultAsync<Media>(new CommandDefinition(query, new {id}, cancellationToken: token));
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
            await connection.ExecuteAsync(command, new {id});
        }
    }
}