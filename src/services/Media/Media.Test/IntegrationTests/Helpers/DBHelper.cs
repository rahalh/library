namespace Media.Test.IntegrationTests.Helpers
{
    using System.Threading.Tasks;
    using Dapper;
    using Npgsql;

    public class DBHelper
    {
        private static readonly string SeedQuery = @"INSERT INTO media(external_id, title, description, total_views, create_time, update_time)
                VALUES (
	                'Bg7-4rPtC-Kl2fGh',
	                'Building Microservices: Designing Fine-Grained Systems',
	                'In this book, Sam Newman explains the whole process of microservices developing. Microservices are widely used and should be the first choice for API developing.',
	                10,
	                '2022-10-26 12:51:19',
	                '2022-10-26 12:55:19'
                ),
                (
	                'UPj6SSMvaKIuXwnY',
	                'Design, Build, Ship: Faster, Safer Software Delivery',
	                'What''s the best way to get code from your laptop into a production environment? With this highly actionable guide, architects, developers, engineers, and others in the IT space will learn everything.',
	                11,
	                '2022-10-26 12:51:19',
	                '2022-10-26 12:56:19'
                ), (
	                'jZeM577fgcSrulKc',
	                'I Heard You Paint Houses: Frank Sheeran The Irishman and Closing the Case on Jimmy Hoffa',
                    'I Heard You Paint Houses: Frank The Irishman Sheeran and Closing the Case on Jimmy Hoffa is a 2004 work of narrative nonfiction written by former homicide prosecutor, investigator and defense attorney Charles Brandt that chronicles the life of Frank Sheeran, an alleged mafia hitman who confesses the crimes he committed working for the Bufalino crime family.',
                    120,
                    '2022-10-26 12:51:19',
                    '2022-10-26 12:57:19'
                ), (
                    'WfSPP636sByUECgl',
                    'Monolith to Microservices: Evolutionary Patterns to Transform Your Monolith',
                    'How do you detangle a monolithic system and migrate it to a microservice architecture? How do you do it while maintaining business-as-usual?',
                    200,
                    '2022-10-26 12:51:19',
                    '2022-10-26 12:58:19'
            );";

        public static async Task Setup(string connectionString)
        {
            var query = @"
                CREATE TYPE media_enum AS ENUM(
	                'Book',
	                'Podcast',
	                'Video'
                );

                CREATE TYPE state_enum AS ENUM(
                    'STATUS_PENDING',
                    'STATUS_DONE'
                );

                -- Using ISO 639-1 Language code
                CREATE TABLE IF NOT EXISTS media(
	                id	 	        bigserial NOT NULL,
                    external_id 	varchar(128) NOT NULL UNIQUE,
                    title 			varchar(255) NOT NULL UNIQUE,
                    description 	text DEFAULT NULL,
                    language_code   varchar(5) NOT NULL DEFAULT 'en',
	                media_type		media_enum NOT NULL DEFAULT 'Book',
                    publish_date 	date NOT NULL DEFAULT CURRENT_DATE,
	                create_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	                update_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	                total_views     bigint DEFAULT 0,
	                content_url     text DEFAULT NULL,
	                status          state_enum NOT NULL DEFAULT 'STATUS_PENDING',
	                PRIMARY KEY(id, external_id)
                );

                CREATE INDEX idx_create_time on media(create_time);

                CREATE INDEX idx_external_id on media(external_id);
            ";

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.ExecuteAsync(query);
        }

        public static async Task Seed(string connectionString)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.ExecuteAsync(SeedQuery);
        }

        public static async Task Reset(string connectionString)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.ExecuteAsync(@"delete from media");
            await Seed(connectionString);
        }
    }
}
