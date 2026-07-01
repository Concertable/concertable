# Integration Tests

## Structure

Each microservice has its own testing infrastructure project that boots the service's real `Program`
using `WebApplicationFactory`, a Testcontainers SQL Server, and a `TestAuthHandler` that replaces
JWT Bearer validation.

| Infrastructure project | Boots | Test projects that use it |
|---|---|---|
| `Tests/Concertable.Testing.Integration` | `Concertable.B2B.Web` | Artist, Venue, User, Tenant, Concert |
| `Tests/Concertable.Testing.Integration.Search` | `Concertable.Search.Web` | Search |
| `Tests/Concertable.Testing.Integration.Customer` | `Concertable.Customer.Web` | Customer.* (scaffold — no tests yet) |

## Running tests

### All B2B integration tests

```powershell
dotnet test Concertable.B2B/src/Modules/Artist/Tests/Concertable.Artist.IntegrationTests/Concertable.Artist.IntegrationTests.csproj
dotnet test Concertable.B2B/src/Modules/Venue/Tests/Concertable.Venue.IntegrationTests/Concertable.Venue.IntegrationTests.csproj
dotnet test Concertable.B2B/src/Modules/User/Tests/Concertable.User.IntegrationTests/Concertable.User.IntegrationTests.csproj
dotnet test Concertable.B2B/src/Modules/Tenant/Tests/Concertable.B2B.Tenant.IntegrationTests/Concertable.B2B.Tenant.IntegrationTests.csproj
dotnet test Concertable.B2B/src/Modules/Concert/Tests/Concertable.Concert.IntegrationTests/Concertable.Concert.IntegrationTests.csproj
```

### Search integration tests

```powershell
dotnet test Concertable.Search/tests/Concertable.Search.IntegrationTests/Concertable.Search.IntegrationTests.csproj
```

### All integration tests (one liner)

```powershell
@(
  "Concertable.B2B/src/Modules/Artist/Tests/Concertable.Artist.IntegrationTests/Concertable.Artist.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Venue/Tests/Concertable.Venue.IntegrationTests/Concertable.Venue.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/User/Tests/Concertable.User.IntegrationTests/Concertable.User.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Tenant/Tests/Concertable.B2B.Tenant.IntegrationTests/Concertable.B2B.Tenant.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Concert/Tests/Concertable.Concert.IntegrationTests/Concertable.Concert.IntegrationTests.csproj",
  "Concertable.Search/tests/Concertable.Search.IntegrationTests/Concertable.Search.IntegrationTests.csproj"
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

- **Search seeding** — `SearchProjectionTestSeeder : ITestSeeder` populates the `[search].*`
  projection tables from the canonical `Concertable.B2B.Seed.Contracts.SeedCatalog` (the same
  specs the dev/E2E simulator replays), mapping each spec through `ToChangedEvent()` and then
  field-for-field as the projection handlers do. Tests derive expectations from
  `fixture.Catalog`, never from invented literals.

## Seeding conventions

**Factory seeding pattern** — domain entities must never carry a `Seed` static factory; that leaks test/infra concerns into the domain. When a seeder needs a known ID and no domain events, add `Seed` to the entity's **Factory** class. The factory calls the real DDD constructor (invariants enforced), uses `EntityReflectionExtensions.With(...)` to stamp in the ID, then `ClearDomainEvents()` to suppress outbox publication. See `CredentialFactory.Seed` vs `CredentialFactory.Create`.

**Sentinel pattern for `SeedIfEmptyAsync`** — when a cross-service event handler can write to the same table before the seeder runs, don't guard on `AnyAsync()` (a race-created row skips the entire seed). Guard on a specific entity that only the seeder ever creates (e.g. admin user ID), so partial event-driven rows don't prevent a full seed.

## Adding new tests

1. Create a test class in the relevant module's `*.IntegrationTests` project.
2. Annotate with `[Collection("Integration")]` and inject `ApiFixture` via constructor.
3. Call `await fixture.ResetAsync()` in `InitializeAsync()`.
4. Use `fixture.CreateClient(user)` to get an authenticated `HttpClient`.
