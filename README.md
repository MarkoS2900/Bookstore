# Bookstore API

A .NET 10 bookstore API built for the Quipu challenge. It manages authors and books, supports filtered and paginated book search, and uses OAuth 2.0 access tokens issued by a separate Duende IdentityServer application.

## 1. Overview

- Create and retrieve authors.
- Create, retrieve, update, and delete books.
- Search books by title and/or author name.
- Protect endpoints using `bookstore.read` and `bookstore.write` scopes.
- Persist data in SQL Server using Entity Framework Core.

Every book belongs to an existing author. The database starts empty, so create an author before creating a book.

## 2. Architecture

| Project | Responsibility |
|---|---|
| `Bookstore.Api` | REST API, validation, authorization, services, repositories, DTO mappings, exception handling, and EF Core migrations. |
| `Bookstore.Identity` | OAuth token issuance, client and scope configuration, and a minimal login page with an in-memory test user. |
| `Bookstore.Api.Tests` | xUnit unit and integration tests using Moq and SQLite in memory. |

The API follows the request flow:

**Controller → Service → Repository → EF Core → Database**

Controllers return response DTOs rather than database entities. Both applications use Serilog for console logging.

The solution file is `Bookstore.Api/Bookstore.Api.slnx`. Run commands from the repository root unless specified otherwise.

## 3. Prerequisites

- Docker Desktop with Linux containers and Docker Compose v2, or Docker Engine with the Compose plugin.
- .NET 10 SDK for building, running locally, and executing tests.
- Docker Desktop WSL integration enabled if running Docker commands from WSL.

## 4. Getting Started

### Run the complete stack with Docker

Create a `.env` file alongside `docker-compose.yml`:

```dotenv
DB_PASSWORD=Your_Strong_Development_Password!
```

Use a password that satisfies SQL Server complexity requirements. The `.env` file is excluded from Git and the Docker build context.

Start the stack:

```bash
docker compose config --quiet
docker compose up --build -d
docker compose ps
```

To follow application logs:

```bash
docker compose logs -f bookstore-api bookstore-identity
```

The first startup may take a few minutes while SQL Server initializes. Compose waits for the SQL Server health check before starting the API. In Development, the API applies migrations automatically on startup.

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| Swagger JSON | http://localhost:8080/swagger/v1/swagger.json |
| Identity discovery | http://localhost:8081/.well-known/openid-configuration |
| Token endpoint | http://localhost:8081/connect/token |
| SQL Server (host) | `localhost,1433` |

### Run with the .NET SDK

Create the `.env` file as described above, then start SQL Server:

```bash
docker compose up -d sqlserver
dotnet restore Bookstore.Api/Bookstore.Api.slnx
dotnet build Bookstore.Api/Bookstore.Api.slnx --no-restore
dotnet dev-certs https --trust
```

In the first terminal, start Identity:

```bash
dotnet run --project Bookstore.Identity --launch-profile https
```

In a second terminal, configure the API database connection and start the API using PowerShell:

```powershell
$env:ConnectionStrings__Bookstore = 'Server=localhost,1433;Database=BookstoreDb;User Id=sa;Password=Your_Strong_Development_Password!;Encrypt=True;TrustServerCertificate=True;Connect Timeout=30;'

dotnet run --project Bookstore.Api --launch-profile https
```

The applications do not load `.env` directly. Docker Compose reads it; native runs require environment variables or another local configuration override.

Native URLs:

- API Swagger: https://localhost:7032/swagger
- Identity: https://localhost:7264

Use the `https` launch profiles because the default OAuth configuration targets these addresses.

## 5. Authentication and Authorization

### Scopes

| Scope | Permissions |
|---|---|
| `bookstore.read` | Read authors and books; search books. |
| `bookstore.write` | Create authors and create, update, or delete books. |

A write scope does not automatically grant read access. Request both scopes for the complete evaluation flow.

### OAuth clients

| Client | Flow | Configuration |
|---|---|---|
| `bookstore-client` | Client Credentials | Confidential client; development secret `bookstore-dev-secret`. |
| `bookstore-swagger` | Authorization Code with PKCE | Public client; no client secret. |

### Obtain a token using PowerShell

With Docker running:

```powershell
$apiUrl = 'http://localhost:8080'
$identityUrl = 'http://localhost:8081'

$tokenResponse = Invoke-RestMethod -Method Post `
    -Uri "$identityUrl/connect/token" `
    -ContentType 'application/x-www-form-urlencoded' `
    -Body @{
        grant_type = 'client_credentials'
        client_id = 'bookstore-client'
        client_secret = 'bookstore-dev-secret'
        scope = 'bookstore.read bookstore.write'
    }

$headers = @{
    Authorization = "Bearer $($tokenResponse.access_token)"
}

Invoke-RestMethod -Uri "$apiUrl/api/v1/books" -Headers $headers
```

For native execution, change the URLs to `https://localhost:7032` and `https://localhost:7264`.

### Obtain a token using curl (Bash/WSL)

```bash
curl --fail-with-body -sS http://localhost:8081/connect/token \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode 'grant_type=client_credentials' \
  --data-urlencode 'client_id=bookstore-client' \
  --data-urlencode 'client_secret=bookstore-dev-secret' \
  --data-urlencode 'scope=bookstore.read bookstore.write'
```

Copy the `access_token` from the response:

```bash
export TOKEN='paste-your-access-token-here'

curl --fail-with-body -sS http://localhost:8080/api/v1/books \
  -H "Authorization: Bearer $TOKEN"
```

The Client Credentials flow does not require a username or browser login. Obtain a new token when the current one expires.

### Swagger authorization

1. Open http://localhost:8080/swagger (Docker) or https://localhost:7032/swagger (native).
2. Click **Authorize**.
3. Enter `bookstore-swagger` if prompted for a client ID. Leave the client secret empty.
4. Select `bookstore.read` and `bookstore.write`.
5. Sign in using:
   - Username: `testuser`
   - Password: `Password123!`
6. After the OAuth callback completes, use **Try it out** on an endpoint.

The test user is defined in `Bookstore.Identity/Users/TestUsers.cs`. There is no registration or persistent user database.

## 6. API Endpoints

All routes are under `/api/v1` and require a valid bearer token.

| Method | Route | Scope | Success |
|---|---|---|---|
| GET | `/authors` | `bookstore.read` | `200 OK` |
| GET | `/authors/{id}` | `bookstore.read` | `200 OK` |
| POST | `/authors` | `bookstore.write` | `201 Created` |
| GET | `/books` | `bookstore.read` | `200 OK` |
| GET | `/books/{id}` | `bookstore.read` | `200 OK` |
| POST | `/books` | `bookstore.write` | `201 Created` |
| PUT | `/books/{id}` | `bookstore.write` | `200 OK` |
| DELETE | `/books/{id}` | `bookstore.write` | `204 No Content` |
| GET | `/books/search` | `bookstore.read` | `200 OK` |

Successful create operations return the created resource and a `Location` header.

### Search and pagination

The search endpoint accepts the following query parameters:

| Parameter | Default | Description |
|---|---|---|
| `title` | — | Optional substring filter on book title. |
| `author` | — | Optional substring filter on author name. |
| `page` | `1` | Page number; must be positive. |
| `pageSize` | `10` | Results per page; must be between `1` and `100`. |

When both title and author are provided, filters are combined using AND. Search is case-sensitive according to the database collation. Omitting both filters returns all books, paginated and ordered by title.

Example:

```http
GET /api/v1/books/search?title=Harry&author=Rowling&page=1&pageSize=10
```

Example response:

```json
{
  "items": [
    {
      "bookId": 1,
      "authorId": 1,
      "authorName": "J. K. Rowling",
      "title": "Harry Potter",
      "subtitle": "Example subtitle"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

## 7. Database and Migrations

`BookstoreDbContext` contains `Authors` and `Books`.

- An author can have multiple books.
- Every book requires one author.
- Author deletion is restricted when related books exist.
- Author names and book titles have a maximum length of 100 characters.

Migrations are located in `Bookstore.Api/Migrations`.

In Development, the API automatically applies migrations on startup. In other environments, migrations must be applied separately before serving requests.

There is no automatic author or book seed data. Create an author before creating a book. The Identity test user is configured independently of the bookstore database.

### Apply migrations manually

With the .NET 10 SDK:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14

dotnet ef database update \
  --project Bookstore.Api \
  --startup-project Bookstore.Api
```

If `dotnet-ef` is already installed, use `dotnet tool update` instead.

Before running host commands, configure `ConnectionStrings__Bookstore` to use `localhost,1433`. The Compose hostname `sqlserver` is only accessible from within the Docker network.

### Create a migration

After changing the EF Core model:

```bash
dotnet ef migrations add DescribeYourChange \
  --project Bookstore.Api \
  --startup-project Bookstore.Api
```

## 8. Testing

Run the complete test suite:

```bash
dotnet test Bookstore.Api/Bookstore.Api.slnx
```

Run unit and integration tests separately:

```bash
dotnet test Bookstore.Api.Tests --filter "FullyQualifiedName~UnitTests"

dotnet test Bookstore.Api.Tests --filter "FullyQualifiedName~IntegrationTests"
```

### Test coverage

**Unit tests**
- Service-level business logic and validation.
- DTO/entity mapping behavior.

**Integration tests**
- Author and book endpoints.
- Request validation and missing resources.
- Search and pagination.
- Authentication and scope enforcement.
- Exception handling and Problem Details responses.

Integration tests use SQLite in memory and a test authentication handler. They do not require Docker, SQL Server, or a running Identity server.

They do not verify SQL Server migrations, real JWT signature validation, browser cookies, or the complete OAuth flow. Use the manual token and Swagger instructions above to verify those components.

On restricted Windows accounts, exception tests may fail because the default Windows Event Log provider cannot write to the system log. To disable it for the test process in PowerShell:

```powershell
$env:Logging__EventLog__LogLevel__Default = 'None'

dotnet test Bookstore.Api/Bookstore.Api.slnx

Remove-Item Env:Logging__EventLog__LogLevel__Default
```

## 9. Docker Operations

```bash
# Start or rebuild the stack
docker compose up --build -d

# Inspect containers
docker compose ps

# View application logs
docker compose logs --tail=100 bookstore-api bookstore-identity

# View SQL Server logs
docker compose logs --tail=100 sqlserver

# Stop containers while retaining them
docker compose stop

# Remove containers and network while retaining named volumes
docker compose down
```

Named volumes retain SQL data, Identity signing keys, and ASP.NET Core Data Protection keys.

**Do not use `docker compose down -v` unless you intend to delete the database and Identity key volumes.**

Changing `DB_PASSWORD` in `.env` does not change the existing `sa` password in a retained SQL Server volume.

### Common issues

- **SQL Server unhealthy or API exits during migrations:** Check SQL Server logs, password complexity, host port conflicts, and whether the password matches the retained database volume.
- **Swagger redirects back to login:** Rebuild Identity after cookie configuration changes, clear localhost cookies, and use the same hostname consistently.
- **Signing-key unprotect errors:** Identity signing keys may reference missing Data Protection keys. Restore the matching keys when available. Resetting obsolete development signing keys invalidates tokens signed by them; preserve the SQL volume.
- **Token endpoint unavailable immediately after startup:** Wait for Identity discovery to respond before requesting a token.
- **Native HTTPS certificate errors:** Trust the .NET development certificate on the machine/browser making the request.
- **Duende license warnings:** Development and evaluation use are supported without a production license. See [Duende licensing](https://docs.duendesoftware.com/general/licensing/) before production use.

## 10. Design Decisions and Limitations

- **OAuth flows:** Client Credentials supports scripts and service-to-service access. Authorization Code with PKCE enables interactive Swagger authentication without exposing a client secret in the browser. The deprecated Resource Owner Password Credentials flow is not implemented.
- **Authorization:** API policies are scope-based. The Identity test user's role claim is not used by API authorization. There is no per-user permission store.
- **Layering:** Services contain business validation, repositories isolate EF Core access, and explicit DTO mappings prevent database navigation properties from leaking into API responses.
- **Error handling:** Validation and missing-resource errors return specific responses. Unexpected exceptions return generic Problem Details while server logs retain diagnostic information. A `409 Conflict` handler exists, but books do not have a concurrency token for row-version-based conflict detection.
- **Data and API scope:** Books support CRUD operations; authors support create and read only. Standard list endpoints are unpaginated; pagination is available through book search. Orders, inventory, pricing, and author update/delete operations are outside the implemented scope.
- **Local development:** Docker uses HTTP, development credentials, SQL Server Developer edition, and startup migrations to simplify evaluation. Production deployment requires HTTPS, secure credential management, persistent Identity configuration, and a separate migration strategy.

