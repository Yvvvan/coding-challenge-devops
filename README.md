# Coding Challenge DevOps

SMT order management application built with ASP.NET Core, Blazor, Entity Framework Core, and SQLite.

## Running the application

There are two ways to run the application locally:

- Manual setup with the .NET SDK
- Docker Compose

## Option 1: Manual setup with .NET

### Requirements

Install the .NET 10 SDK.

Open PowerShell as Administrator and run:

```powershell
winget install Microsoft.DotNet.SDK.10 --source winget
```

After installation, open a new PowerShell window and verify the installed SDK:

```powershell
dotnet --version
```

### 1. Restore dependencies

Open PowerShell in the repository root:

```powershell
cd .\Source
dotnet restore .\CodingChallenge.slnx
```

### 2. Start the API

In the same PowerShell terminal:

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

### Stop the manually started application

Press:

```text
Ctrl+C
```

in each PowerShell terminal running the API or Web application.

## Option 2: Docker Compose

Docker Compose starts the Web application, API, and persistent SQLite storage together.

### Requirements

Install Docker Desktop with Docker Compose support.

Verify Docker in PowerShell:

```powershell
docker --version
docker compose version
```

### Start the application

From the repository root:

```powershell
docker compose up --build
```

Or run it in the background:

```powershell
docker compose up --build -d
```

Open the Web application at:

```text
http://localhost:5001
```

The API is available at:

```text
http://localhost:5220
```

The Web container communicates with the API container through the internal Docker network. SQLite remains embedded in the API and its database file is persisted in a named Docker volume.

When using Docker Compose, the .NET SDK does not need to be installed on the host because the SDK and runtime are provided by the Docker images.

### Stop the Docker application

Stop the containers while keeping the persisted data:

```powershell
docker compose down
```

To also delete the persisted SQLite database and start from a clean state:

```powershell
docker compose down -v
```

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
