# Coding Challenge DevOps

SMT order management application built with ASP.NET Core, Blazor, Entity Framework Core, and SQLite.

## Prerequisites

Install:

- .NET SDK 10

Verify the .NET installation in PowerShell:

```powershell
dotnet --version
```

The project expects a .NET 10 SDK.

## Run the application locally

Open PowerShell in the repository root.

### 1. Restore dependencies

```powershell
cd .\Source
dotnet restore .\CodingChallenge.slnx
```

### 2. Start the API

Open a PowerShell terminal in `Source` and run:

```powershell
dotnet run --project .\src\CodingChallenge.Api\CodingChallenge.Api.csproj --launch-profile http
```

The API runs at:

```text
http://localhost:5220
```

The API uses the local SQLite database configured in `CodingChallenge.Api/appsettings.json`.

### 3. Start the Web application

Keep the API running and open a second PowerShell terminal.

From the repository root:

```powershell
cd .\Source
dotnet run --project .\src\CodingChallenge.Web\CodingChallenge.Web.csproj --launch-profile http
```

The Web application runs at:

```text
http://localhost:5001
```

Open that address in a browser.

## Demo accounts

Two demo users are configured for local development:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `admin123` | Admin |
| `operator` | `line42` | Operator |

## Run the automated tests

From `Source`:

```powershell
dotnet test .\tests\CodingChallenge.Tests\CodingChallenge.Tests.csproj
```

To build the complete solution first:

```powershell
dotnet build .\CodingChallenge.slnx --configuration Release
dotnet test .\tests\CodingChallenge.Tests\CodingChallenge.Tests.csproj --configuration Release --no-build
```

## Local URLs

| Service | URL |
| --- | --- |
| Web | http://localhost:5001 |
| API | http://localhost:5220 |

The Web application is configured to call the API at `http://localhost:5220`.

## Stop the application

Press:

```text
Ctrl+C
```

in each PowerShell terminal running the API or Web application.
