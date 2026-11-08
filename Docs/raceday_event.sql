--creating database
create database raceday;
--using database raceday_event
use raceday;

--creating table roles
create table roles
(
role_id int identity(100000,1) primary key,
role_name varchar(50) not null unique,
role_description varchar(250)
);

--creating table users
create table users
(
userID int identity(200000,1) primary key,
role_id int not null,
first_name varchar(100) not null,
last_name varchar(100) not null,
username varchar(100) not null unique,
email varchar(100) not null unique,
phone_number varchar(30),
password_hash varchar(255) not null,
profile_picture_url varchar(500) null,
date_created datetime default getdate(),
foreign key(role_id) references roles(role_id)
);



--creating table events
create table event
(
event_id int identity(300000,1) primary key,
organizer_id int not null,
event_name varchar(250) not null,
event_description varchar(500),
event_date datetime not null,
event_location varchar(250) not null,
distance decimal(8,3) not null,
event_type varchar(20) not null,
event_status varchar(25) not null default 'Planned',
banner_url varchar(500) null,
check (distance > 0),
check (event_type in ('Run', 'Walk', 'Cycle')),
check (event_status in ('Planned', 'Open', 'Closed', 'Cancelled', 'Completed')),
date_created datetime default getdate(),
foreign key(organizer_id) references users(userID)
);

--creating table categories
create table category
(
category_id int identity(400000,1) primary key,
event_id int not null,
category_name varchar(250) not null,
entry_fee decimal(10,2) not null default 0,
race_distance decimal(8,3) null,
max_participants int not null default 500,
constraint CK_category_entry_fee check (entry_fee >= 0),
constraint CK_category_race_distance check (race_distance is null or race_distance > 0),
constraint CK_category_max_participants check (max_participants > 0),
constraint FK_category_event
    foreign key(event_id) references event(event_id)
);

--creating table enrolments
create table enrolment
(
enrolment_id int identity(500000,1) primary key,
userID int not null,
event_id int not null,
category_id int not null,
enrolment_date datetime default getdate(),
enrolment_status varchar(25) not null default 'Confirmed',
check (enrolment_status in ('Confirmed', 'Cancelled', 'Pending')),
foreign key(userID) references users(userID),
foreign key(event_id) references event(event_id),
foreign key(category_id) references category(category_id)
);

--creating table results
create table results
(
result_id int identity(600000,1) primary key,
enrolment_id int not null unique,
finish_time time null,
overall_position int null,
category_position int null,
results_status varchar(25) not null default 'Finished',
constraint CK_results_status
    check (results_status in ('Finished', 'DNF', 'DSQ', 'DNS')),
constraint CK_overall_position
    check (overall_position is null or overall_position > 0),
constraint CK_category_position
    check (category_position is null or category_position > 0),
foreign key(enrolment_id) references enrolment(enrolment_id)
);


-- ROLES (identity starts at 100000)

insert into roles (role_name, role_description)
values
('Organizer', 'Can create and manage events, categories and participant results'),
('Participant', 'Can browse and enrol in events and view personal results');

select * from roles;


-- USERS (identity starts at 200000)



select * from users;


-- EVENTS (identity starts at 300000)



select * from event;


-- CATEGORIES (identity starts at 400000)


select * from category;


-- ENROLMENTS (identity starts at 500000)



select * from enrolment;


-- RESULTS (identity starts at 600000)


select * from results;
