# Integration Test Conventions

Conventions for `*.IntegrationTests` projects and their `*.IntegrationTests.Fixtures` — tests that boot
the service's real `Program` and exercise it over HTTP against a real database. For pure in-memory
domain/service tests see [`UNIT_CONVENTIONS.md`](./UNIT_CONVENTIONS.md); for browser scenarios see
[`E2E_CONVENTIONS.md`](./E2E_CONVENTIONS.md). General C# style is in
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md).

## Structure

Each microservice has its own testing infrastructure project that boots the service's real `Program`
using `WebApplicationFactory`, a Testcontainers SQL Server, and a `TestAuthHandler` that replaces
JWT Bearer validation.

| Infrastructure project | Boots | Test projects that use it |
|---|---|---|
| `Tests/Concertable.Testing.Integration` | `Concertable.B2B.Web` | Artist, Venue, User, Tenant, Concert |
| `Tests/Concertable.Testing.Integration.Search` | `Concertable.Search.Web` | Search |
| `Tests/Concertable.Testing.Integration.Customer` | `Concertable.Customer.Web` | Customer.* (scaffold — no tests yet) |

## Key design decisions

- **Each microservice owns its fixture** — `ApiFixture` and `SqlFixture` in each
  `Testing.Integration.*` project are named identically but live in separate namespaces.
  Test projects import only their own fixture; there is no naming conflict.

- **Testcontainers** — a fresh SQL Server container starts per test run. `Respawn` resets
  data between tests without re-running migrations.

- **Authentication** — `TestAuthHandler` replaces JWT Bearer. Pass `X-Test-Sub` (user ID)
  and optionally `X-Test-Email` headers to authenticate a request. No token or role claim is required.

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

(Production seeding rules — never seed event-driven data — live in
[`SEEDING_CONVENTIONS.md`](./SEEDING_CONVENTIONS.md) and apply to `ITestSeeder` too.)

## Adding new tests

1. Create a test class in the relevant module's `*.IntegrationTests` project.
2. Annotate with `[Collection("Integration")]` and inject `ApiFixture` via constructor.
3. Call `await fixture.ResetAsync()` in `InitializeAsync()`.
4. Use `fixture.CreateClient(user)` to get an authenticated `HttpClient`.

Assertions use Shouldly (`ShouldBe`) — on a failing status the message carries URL + status + response
body.

## Grouping a large test class

**Naming.** Test files are `<Resource><Qualifier>ApiTests` — the resource/controller first
(`Application`, `Artist`), then any qualifier, then the fixed `ApiTests` suffix. The `ApiTests` suffix
is never dropped and the resource always leads, so a Versus slice of the Application controller is
`ApplicationVersusApiTests`, never `VersusApplicationTests`. `Api` (not `Endpoints` or `Controller`)
because these drive the real HTTP surface through the `*.Api` assembly, not a controller class in
isolation.

**Regions handle one axis of variation; a file split handles the second.** Most controllers vary on a
single axis — their endpoints — so they get **one file, a `#region` per endpoint**, named for the
**method/endpoint under test** (`#region Create`, `#region GetDetailsById`,
`#region GetVatCalculationAsync`), or for the behaviour where a cluster isn't a single method
(`#region Cancel from PaymentFailed`). That is the default and the common case (e.g. `ArtistApiTests`).
Group with `#region` — **never** `// ---- X ----` comment dividers.

A controller that varies on **two** axes is a matrix, not a list, and a single regioned file can't
express it — one axis becomes the regions and the other becomes repeated sub-blocks inside every
region. The `Application` controller is the standing example: endpoint (`/checkout`, `/accept`,
`/apply`) × `DealType` (each deal type is a different lifecycle state machine, so the *same* endpoint
behaves differently per type). For that shape:

- **Primary axis → file split.** Split on the axis where behaviour genuinely forks — here `DealType`,
  one file per value: `ApplicationVersusApiTests`, `ApplicationFlatFeeApiTests`, … The qualifier in the
  file name *is* the primary-axis value.
- **Secondary axis → `#region`s inside each file** — `#region Accept`, `#region AcceptCheckout` within
  `ApplicationVersusApiTests`.
- **Cross-cutting behaviour that belongs to no single value → its own file**, not duplicated across
  every value file — e.g. `ApplicationCancelApiTests` / `ApplicationWithdrawRejectApiTests` hold
  Cancel/Withdraw flows shared across all deal types.

Regioning is for navigation, not a licence to sprawl. On a single-axis class, when it outgrows
comfortable regioning, split it rather than pile on more regions.

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

The `integration-debug` skill runs the full suite with per-test server-side `ILogger` output and
captured mock state (notifications, emails, Stripe).
