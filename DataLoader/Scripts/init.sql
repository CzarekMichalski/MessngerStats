create table if not exists authors(
	id varchar primary key,
	name varchar not null,
	surname varchar not null
);

create table if not exists messages(
	id varchar primary key,
	author_id varchar not null,
	content varchar,
	send_time timestamptz,
	is_from_group bool,
	conversation_id varchar
);

create table if not exists photos(
	id varchar primary key,
	author_id varchar not null,
	send_time timestamptz,
	local_path varchar,
	conversation_id varchar
);

create table if not exists groups(
	id varchar primary key,
	participants_id varchar[],
	name varchar
);

create table if not exists conversations(
	id varchar primary key,
	participant_id varchar
);
