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

insert into users
(role_id, first_name, last_name, username, email, phone_number, password_hash)
values
(100000, 'Florida', 'Naidoo', 'florida.organizer', 'florida.organizer@raceday.co.za', '0787654567', 'hashed_password_1'),
(100000, 'Katlego', 'Sithole', 'katlego.organizer', 'katlego.organizer@raceday.co.za', '0823432334', 'hashed_password_2'),
(100001, 'Akani', 'Baloyi', 'akani.participant', 'akani@gmail.com', '0675296554', 'hashed_password_3'),
(100001, 'Nyiko', 'Mathebula', 'nyiko.participant', 'nyiko@gmail.com', '0732109965', 'hashed_password_4'),
(100001, 'Thandi', 'Barclays', 'thandi.participant', 'thandi@gmail.com', '0765291978', 'hashed_password_5');

select * from users;


-- EVENTS (identity starts at 300000)

insert into event
(organizer_id, event_name, event_description, event_date, event_location, distance, event_type, event_status)
values
(200000, 'Two Oceans Marathon', 'Scenic marathon event along the Cape Peninsula', '2027-04-17', 'Cape Town, Western Cape', 56.000, 'Run', 'Open'),
(200001, 'Park Community Fun Run', 'Family-friendly community running event', '2027-05-02', 'Johannesburg, Gauteng', 10.000, 'Run', 'Planned'),
(200000, 'Johannesburg Charity Walk', 'Community charity walking event', '2027-06-12', 'Johannesburg, Gauteng', 5.000, 'Walk', 'Planned');

select * from event;


-- CATEGORIES (identity starts at 400000)

insert into category
(event_id, category_name, entry_fee, race_distance, max_participants)
values
(300000, 'Ultra Marathon - Senior', 900.00, 56.000, 11000),
(300000, 'Half Marathon', 550.00, 21.100, 8000),
(300001, '10km Fun Run', 150.00, 10.000, 2000),
(300001, '5km Fun Run', 100.00, 5.000, 3000),
(300002, 'Open Walk', 100.00, 5.000, 2000);

select * from category;


-- ENROLMENTS (identity starts at 500000)

insert into enrolment
(userID, event_id, category_id, enrolment_status)
values
(200002, 300000, 400000, 'Confirmed'),
(200003, 300000, 400001, 'Confirmed'),
(200004, 300001, 400002, 'Confirmed'),
(200002, 300001, 400003, 'Cancelled'),
(200003, 300002, 400004, 'Confirmed');

select * from enrolment;


-- RESULTS (identity starts at 600000)

insert into results
(enrolment_id, finish_time, overall_position, category_position, results_status)
values
(500000, '05:12:40', 980, 610, 'Finished'),
(500001, '02:05:15', 1450, 720, 'Finished'),
(500002, null, null, null, 'DNS');

select * from results;