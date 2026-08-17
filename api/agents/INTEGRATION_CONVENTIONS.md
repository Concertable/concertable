# Integration tests — Concertable's fixtures and shared harness

The standard is the `integration-testing` skill: what makes a test an integration test, the fixture-per-
service shape, lifting anything shared into the shared library, `IScoped<T>` instead of hand-rolled scopes,
`<Resource><Qualifier>ApiTests` naming, and region-vs-file-split grouping. Production seeding rules are the
`seeding` skill plus [`SEEDING_CONVENTIONS.md`](./SEEDING_CONVENTIONS.md), and they bind `ITestSeeder` too.

This file is the inventory of what exists here.

## One fixture project per service

| Fixture project | Boots |
|---|---|
| `Concertable.Auth.IntegrationTests.Fixtures` | `Concertable.Auth` (Razor Pages + Duende) |
| `Concertable.B2B.IntegrationTests.Fixtures` | `Concertable.B2B.Web` |
| `Concertable.Customer.IntegrationTests.Fixtures` | `Concertable.Customer.Web` |
| `Concertable.Search.IntegrationTests.Fixtures` | `Concertable.Search.Web` |

Each holds that service's `ApiFixture` — identically named, in its own namespace. Only genuinely
service-specific wiring stays there: Duende/Razor, Payment-in-process, Stripe fakes, per-service mocks and
seeders.

## The shared members every fixture composes

In `Concertable.Testing.Integration` (unit-level helpers in `Concertable.Testing`):

- `SqlFixture` — Testcontainers SQL + Respawn.
- `TestAuthHandler`, via `services.AddTestAuthentication()`. Authenticate a request with `X-Test-Sub` and
  optionally `X-Test-Email`. No token, no `role` claim.
- `Environments.Integration` / `.E2E` to set, `env.IsIntegration()` / `env.IsE2E()` to check — extension
  members in `Concertable.Kernel` hung onto the framework's `Environments`/`IHostEnvironment`. Never a raw
  environment literal.
- `services.AddXunitLogging(accessor)` — routes host logs to the current xunit output.
- `services.RemoveAzureServiceBus()` — drops the ASB receivers and swaps `IBusTransport` for
  `MockBusTransport`. Omit it in a service with no bus (Search).
- `IntegrationDbInitializer` — migrates inbox/outbox, then migrates and seeds every registered
  `ITestSeeder`. Register as `services.AddScoped<IDbInitializer, IntegrationDbInitializer>()`.
- `MockWebhookSimulator` / `MockWebhookSimulatorFail` — dispatch `PaymentSucceededEvent`/`PaymentFailedEvent`
  straight to the registered `IIntegrationEventHandler` implementations in a new scope, bypassing HTTP.

Assertions use Shouldly; on a failing status the message carries URL, status and response body.
Dispatch integration events through `IScoped<IEnumerable<IIntegrationEventHandler<TEvent>>>` — use the
Kernel abstraction; the copy in `Concertable.DataAccess` is a temporary compatibility surface.

Derive expectations from `fixture.Catalog`, never invented literals.

## Running

```powershell
# All B2B + Search integration suites
@(
  "Concertable.B2B/src/Modules/Artist/Tests/Concertable.B2B.Artist.IntegrationTests/Concertable.B2B.Artist.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Venue/Tests/Concertable.B2B.Venue.IntegrationTests/Concertable.B2B.Venue.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/User/Tests/Concertable.B2B.User.IntegrationTests/Concertable.B2B.User.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Tenant/Tests/Concertable.B2B.Tenant.IntegrationTests/Concertable.B2B.Tenant.IntegrationTests.csproj",
  "Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concertable.B2B.Concert.IntegrationTests.csproj",
  "Concertable.Search/tests/Concertable.Search.IntegrationTests/Concertable.Search.IntegrationTests.csproj"
) | ForEach-Object { dotnet test $_ }
```

The `integration-debug` skill runs the full suite with per-test server-side `ILogger` output and captured
mock state (notifications, emails, Stripe).

## The two Concertable-specific seeding shapes tests rely on

- **Factory seeding.** A domain entity never carries a `Seed` static — that leaks test concerns into the
  domain. Add `Seed` to the entity's **Factory**, which calls the real DDD constructor, stamps the id with
  `EntityReflectionExtensions.With(...)`, then `ClearDomainEvents()` to suppress outbox publication. See
  `CredentialFactory.Seed` versus `CredentialFactory.Create`.
- **Sentinel guard for `SeedIfEmptyAsync`.** Where a cross-service event handler can write the same table
  before the seeder runs, guarding on `AnyAsync()` lets one race-created row skip the whole seed. Guard on
  an entity only the seeder ever creates (an admin user id).
