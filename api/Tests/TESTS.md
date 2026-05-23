# Integration Tests

## Structure

Each microservice has its own testing infrastructure project that boots the service's real `Program`
using `WebApplicationFactory`, a Testcontainers SQL Server, and a `TestAuthHandler` that replaces
JWT Bearer validation.

| Infrastructure project | Boots | Test projects that use it |
|---|---|---|
| `Tests/Concertable.Testing.Integration` | `Concertable.B2B.Web` | Artist, Venue, User, Organization, Concert |
| `Tests/Concertable.Testing.Integration.Search` | `Concertable.Search.Web` | Search |
| `Tests/Concertable.Testing.Integration.Customer` | `Concertable.Customer.Web` | Customer.* (scaffold — no tests yet) |

## Running tests

### All B2B integration tests

```powershell
dotnet test Concertable.B2B/Modules/Artist/Tests/Concertable.Artist.IntegrationTests/Concertable.Artist.IntegrationTests.csproj
dotnet test Concertable.B2B/Modules/Venue/Tests/Concertable.Venue.IntegrationTests/Concertable.Venue.IntegrationTests.csproj
dotnet test Concertable.B2B/Modules/User/Tests/Concertable.User.IntegrationTests/Concertable.User.IntegrationTests.csproj
dotnet test Concertable.B2B/Modules/Organization/Tests/Concertable.Organization.IntegrationTests/Concertable.Organization.IntegrationTests.csproj
dotnet test Concertable.B2B/Modules/Concert/Tests/Concertable.Concert.IntegrationTests/Concertable.Concert.IntegrationTests.csproj
```

### Search integration tests

```powershell
dotnet test Concertable.Search/Tests/Concertable.Search.IntegrationTests/Concertable.Search.IntegrationTests.csproj
```

### All integration tests (one liner)

```powershell
@(
  "Concertable.B2B/Modules/Artist/Tests/Concertable.Artist.IntegrationTests/Concertable.Artist.IntegrationTests.csproj",
  "Concertable.B2B/Modules/Venue/Tests/Concertable.Venue.IntegrationTests/Concertable.Venue.IntegrationTests.csproj",
  "Concertable.B2B/Modules/User/Tests/Concertable.User.IntegrationTests/Concertable.User.IntegrationTests.csproj",
  "Concertable.B2B/Modules/Organization/Tests/Concertable.Organization.IntegrationTests/Concertable.Organization.IntegrationTests.csproj",
  "Concertable.B2B/Modules/Concert/Tests/Concertable.Concert.IntegrationTests/Concertable.Concert.IntegrationTests.csproj",
  "Concertable.Search/Tests/Concertable.Search.IntegrationTests/Concertable.Search.IntegrationTests.csproj"
) | ForEach-Object { dotnet test $_ }
```

### Unit tests

```powershell
dotnet test Tests/Concertable.Workers.UnitTests/Concertable.Workers.UnitTests.csproj
```

### UI E2E tests (requires running Aspire AppHost)

```powershell
dotnet test Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Concertable.E2ETests.Ui.csproj
```

## Key design decisions

- **Each microservice owns its fixture** — `ApiFixture` and `SqlFixture` in each
  `Testing.Integration.*` project are named identically but live in separate namespaces.
  Test projects import only their own fixture; there is no naming conflict.

- **Testcontainers** — a fresh SQL Server container starts per test run. `Respawn` resets
  data between tests without re-running migrations.

- **Authentication** — `TestAuthHandler` replaces JWT Bearer. Pass `X-Test-Sub` (user ID)
  and `X-Test-Role` headers to authenticate a request. No token required.

- **ASB receiver removed** — the `AzureServiceBusReceiver` hosted service is removed from
  the DI container in B2B and Customer fixtures (no real broker in tests). The outbox
  dispatcher and inbox are left running; a `MockBusTransport` is substituted so the
  dispatcher can drain the outbox without connecting to Azure.

- **Webhook simulation** — `MockWebhookSimulator` and `MockWebhookSimulatorFail` dispatch
  `PaymentSucceededEvent` / `PaymentFailedEvent` directly to `IIntegrationEventHandler`
  implementations in a new scope, bypassing HTTP entirely.

- **Search seeding** — `SearchTestSeeder` writes directly to the `[search].*` projection
  tables (one artist "Test Artist" with Rock genre, one venue "Test Venue", one posted
  concert with Rock genre and `Price > 0`).

## Adding new tests

1. Create a test class in the relevant module's `*.IntegrationTests` project.
2. Annotate with `[Collection("Integration")]` and inject `ApiFixture` via constructor.
3. Call `await fixture.ResetAsync()` in `InitializeAsync()`.
4. Use `fixture.CreateClient(user)` to get an authenticated `HttpClient`.
