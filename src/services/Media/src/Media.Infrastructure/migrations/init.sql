create database media;
\c media

CREATE TYPE media_enum AS ENUM(
	'BOOK',
	'PODCAST',
	'VIDEO'
);

-- Using ISO 639-1 Language code
CREATE TABLE IF NOT EXISTS media(
	id	 	        bigserial NOT NULL,
    external_id 	varchar(128) NOT NULL UNIQUE,
    title 			varchar(255) NOT NULL UNIQUE,
    description 	text DEFAULT NULL,
    language_code   varchar(5) NOT NULL DEFAULT 'en',
	media_type		media_enum NOT NULL DEFAULT 'BOOK',
    publish_date 	date NOT NULL DEFAULT CURRENT_DATE,
	create_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	update_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	total_views     bigint DEFAULT 0,
	content_url     text DEFAULT NULL,
	PRIMARY KEY(id, external_id)
);

CREATE INDEX idx_create_time on media(create_time);

CREATE INDEX idx_external_id on media(external_id);
