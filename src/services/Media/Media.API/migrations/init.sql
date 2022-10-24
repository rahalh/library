CREATE DATABASE media;
\c media

CREATE TYPE media_enum AS ENUM(
	'MEDIA_BOOK',
	'MEDIA_PODCAST',
	'MEDIA_VIDEO'
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
	media_type		media_enum NOT NULL DEFAULT 'MEDIA_BOOK',
    publish_date 	date NOT NULL DEFAULT CURRENT_DATE,
	create_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	update_time 	timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
	total_views     bigint DEFAULT 0,
	content_url     text DEFAULT NULL,
	status          state_enum NOT NULL DEFAULT 'STATUS_PENDING',
	PRIMARY KEY(id, external_id)
);
