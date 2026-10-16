# RaceDay Event API

A REST API for managing road running, walking and cycling events in South Africa. Organizers create events and the distances people can enter, participants enrol, and organizers capture the results afterwards.

**Module:** PROG6212 Programming 2B (IIE), Portfolio of Evidence, Part 2
**Author:** Phloid Rikhotso

## Features

- Register and log in with an email and password. Passwords are hashed with BCrypt.
- Two roles: **Organizer** and **Participant**, chosen from a dropdown when registering.
- Organizers create events and add categories to them. A race can have several distances, and each distance has its own entry fee and participant limit.
- Participants browse open events, pick a category and enrol. They can cancel their own enrolment.
- Organizers see who enrolled in their events and capture each participant's finish time, position and result status.
- Participants view their own race history.

## Built with

- ASP.NET Core 8 (C#)
- SQL Server LocalDB, using `System.Data.SqlClient` with parameterised queries
- BCrypt.Net-Next for password hashing
- Swagger (Swashbuckle) for trying out the endpoints
- xUnit for the unit tests

## Project structure

```
race_day_event/
├── Controllers/    authenticator, users, events, categories, enrolments, results
├── Database/       classes that hold the SQL queries, plus raceday_event.sql
├── Models/         data classes for the tables
├── Docs/           final SQL script and the API endpoint plan (with the ERD)
├── TestProject1/   xUnit unit tests
└── Program.cs      application setup (Swagger and session)
```

## Getting started

### What you need

- .NET 8 SDK
- SQL Server LocalDB (installed with Visual Studio)

### 1. Get the code

```bash
git clone https://github.com/Phloidrikhotso/race_day_event.git
cd race_day_event
```

### 2. Create the database

The API connects to the LocalDB instance `api_both` and the database `raceday`.

```bash
sqllocaldb create api_both
sqllocaldb start api_both
sqlcmd -S "(localdb)\api_both" -i Docs/raceday_event.sql
```

The script creates the `raceday` database, all the tables, and the two roles (Organizer and Participant). You can also open `Docs/raceday_event.sql` in Visual Studio (SQL Server Object Explorer) and run it there.

The connection string is `Data source=(localdb)\api_both;database=raceday;`. It is stored in the classes in the `Database` folder, so change it there if you use a different instance name.

### 3. Run the API

```bash
dotnet run
```

Open Swagger at `https://localhost:7093/swagger` (or `http://localhost:5064/swagger`).

## How logging in works

`POST api/authenticator/login` starts a session and stores the user's id and role. The other endpoints read the session to find out who is calling. The session ends when you log out, or after 5 minutes of no activity. In Swagger the browser keeps the session cookie, so log out before you log in as a different user.

## Endpoints

### Authentication

| Method | Route | Who | What it does |
|---|---|---|---|
| POST | `/api/authenticator/register` | Anyone | Registers a user as an Organizer or Participant |
| POST | `/api/authenticator/login` | Anyone | Logs in with email and password |
| POST | `/api/authenticator/logout` | Logged in | Ends the session |

### Users

| Method | Route | Who | What it does |
|---|---|---|---|
| GET | `/api/users/me` | Logged in | Returns the logged-in user's profile |
| PUT | `/api/users/me` | Logged in | Updates the logged-in user's profile |

### Events

| Method | Route | Who | What it does |
|---|---|---|---|
| GET | `/api/events` | Anyone | Lists upcoming events that are open for enrolment |
| GET | `/api/events/mine` | Organizer | Lists the organizer's own events at any date or status |
| GET | `/api/events/{event_id}` | Anyone | Returns one event |
| POST | `/api/events` | Organizer | Creates an event |
| PUT | `/api/events/{event_id}` | Organizer (owner) | Updates an event |
| DELETE | `/api/events/{event_id}` | Organizer (owner) | Deletes an event |

### Categories (the distances of an event)

| Method | Route | Who | What it does |
|---|---|---|---|
| GET | `/api/events/{event_id}/categories` | Anyone | Lists the categories of an event |
| POST | `/api/events/{event_id}/categories` | Organizer (owner) | Adds a category with its own fee and distance |
| PUT | `/api/categories/{category_id}` | Organizer (owner) | Updates a category |
| DELETE | `/api/categories/{category_id}` | Organizer (owner) | Deletes a category |

### Enrolments

| Method | Route | Who | What it does |
|---|---|---|---|
| POST | `/api/events/{event_id}/enrolments` | Participant | Enrols in an event under a chosen category |
| GET | `/api/enrolments/me` | Participant | Lists the participant's own enrolments |
| GET | `/api/events/{event_id}/enrolments` | Organizer (owner) | Lists the participants enrolled in an event |
| DELETE | `/api/enrolments/{enrolment_id}` | Participant (owner) | Cancels the enrolment |

### Results

| Method | Route | Who | What it does |
|---|---|---|---|
| POST | `/api/enrolments/{enrolment_id}/result` | Organizer (owner) | Captures a result for an enrolment |
| PUT | `/api/enrolments/{enrolment_id}/result` | Organizer (owner) | Updates a captured result |
| GET | `/api/results/me` | Participant | Returns the participant's own results |
| GET | `/api/events/{event_id}/results` | Organizer (owner) | Returns all the results of an event |

"Owner" means the organizer who created the event.

### Response codes

Every endpoint returns a short text message. The status code tells you what happened:

| Code | Meaning |
|---|---|
| 200 | It worked (including deletes and cancellations) |
| 201 | Something was created |
| 400 | The input failed validation |
| 401 | Nobody is logged in |
| 403 | The user has the wrong role, or the item belongs to someone else |
| 404 | The event, category, enrolment or result does not exist |
| 409 | A conflict, for example already enrolled, category full, event not open, or duplicate category |

## Trying it out in Swagger

1. Register one **Organizer** and one **Participant**.
2. Log in as the Organizer and create an event with the status **Open**.
3. Add one or more categories to the event, each with its own distance and fee.
4. Log out, then log in as the Participant, and list the events and their categories.
5. Enrol using the `category_id` you want.
6. Log out, log in as the Organizer, and view the enrolments. Capture a result for the enrolment.
7. Log in as the Participant again and check `GET /api/results/me`.

Race type, event status, category name and role are dropdowns in Swagger.

## Running the tests

The unit tests check the rules that run before the database is reached: being logged in, having the right role, and valid input. They don't need SQL Server.

In Visual Studio, open **Test Explorer** from the Test menu and choose **Run All Tests**. From the command line:

```bash
dotnet test
```

## Documentation

The `Docs` folder holds:

- `raceday_event.sql`, the final database script
- the API endpoint plan, which also contains the ERD
