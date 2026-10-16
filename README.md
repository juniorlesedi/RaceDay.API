# RaceDay API

## Project Overview
RaceDay API is an ASP.NET Core Web API developed in C# for managing race events, participants, organisers, categories, registrations and race results.

## Technologies Used
- C#
- ASP.NET Core Web API
- .NET 8
- SQL Server
- Entity Framework Core
- Swagger / OpenAPI
- xUnit unit testing
- GitHub Actions for continuous integration

## Main Features
- Participant and organiser registration and login
- Password hashing and session-based authentication
- Event creation, viewing, updating and deletion
- Category management
- Participant event registrations and cancellations
- Race result management
- User profile management
- Role-based access restrictions
- API documentation through Swagger

## Database Setup
1. Install SQL Server and SQL Server Management Studio (SSMS).
2. Create or restore the RaceDay database using the supplied database schema.
3. Update the `RaceDayConnection` connection string in `appsettings.json` to match your SQL Server instance.
4. Ensure the database tables and columns match the application models.

**Important:** This project uses an existing database. Do not run database creation or migrations against an existing database without checking the schema first.

## Running the API
1. Open the solution in Visual Studio 2022.
2. Restore the NuGet packages.
3. Confirm that the SQL Server connection string is correct.
4. Set `RaceDay.API` as the startup project.
5. Run the application.
6. Open the Swagger URL displayed by the application to test the endpoints.

## Running Unit Tests
Run the following command from the solution folder:

```bash
dotnet test
```

The unit tests check API behaviour, including unauthenticated access to protected operations.

## Continuous Integration
GitHub Actions automatically restores dependencies, builds the solution and runs the unit tests when code is pushed or a pull request is opened.

## API Endpoints
- `POST /api/Auth/register` — Register an account
- `POST /api/Auth/login` — Log in
- `POST /api/Auth/logout` — Log out
- `GET /api/Auth/me` — View the current session
- `/api/Events` — Manage race events
- `/api/Categories` — Manage event categories
- `/api/Registrations` — Manage participant registrations
- `/api/Results` — Manage race results
- `/api/Profile` — Update a profile

Refer to Swagger for the available HTTP methods, request bodies and response codes.

## Demonstration Video
YouTube link: [Insert your demonstration video link here]

## Repository
GitHub: [Insert your repository URL here]
