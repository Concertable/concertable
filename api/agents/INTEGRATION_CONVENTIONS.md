# Integration Test Conventions

Conventions for `*.IntegrationTests` projects and their `*.IntegrationTests.Fixtures` — tests that boot
the service's real `Program` and exercise it over HTTP against a real database. For pure in-memory
domain/service tests see [`UNIT_CONVENTIONS.md`](./UNIT_CONVENTIONS.md); for browser scenarios see
[`E2E_CONVENTIONS.md`](./E2E_CONVENTIONS.md). General C# style is in
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md).

## Structure

Each microservice owns a `Concertable.<Service>.IntegrationTests.Fixtures` project holding its `ApiFixture`,
which boots the service's real `Program` via `WebApplicationFactory`. The shared, service-agnostic pieces
live in `Concertable.Testing.Integration` (unit-level helpers in `Concertable.Testing`), referenced by every
fixture: `SqlFixture` (Testcontainers SQL + Respawn), `TestAuthHandler`, the shared mocks, and the setup
extensions below.

| Service fixture | Boots |
|---|---|
| `Concertable.Auth.IntegrationTests.Fixtures` | `Concertable.Auth` (Razor Pages + Duende) |
| `Concertable.B2B.IntegrationTests.Fixtures` | `Concertable.B2B.Web` |
| `Concertable.Customer.IntegrationTests.Fixtures` | `Concertable.Customer.Web` |
| `Concertable.Search.IntegrationTests.Fixtures` | `Concertable.Search.Web` |

## Shared setup — anything common lives in `Concertable.Testing`

A fixture must not re-hand-roll setup another service already has. Anything shared across two or more
integration suites goes in `Concertable.Testing.Integration` (or `Concertable.Testing` for unit-level
helpers) and is composed via extension methods / constants — never copy-pasted per fixture. When you catch
yourself copying a setup step into a second fixture, lift it into the shared lib instead.

- `Environments.Integration` / `.E2E` (set) and `env.IsIntegration()` / `env.IsE2E()` (check) — extension members
  in `Concertable.Kernel` hung onto the framework's `Environments` / `IHostEnvironment`; never a raw env literal.
- `services.AddTestAuthentication()` — makes `TestAuthHandler` the default scheme.
- `services.AddXunitLogging(accessor)` — routes host logs to the current xunit test output.
- `services.RemoveAzureServiceBus()` — drops the ASB receiver(s) and swaps `IBusTransport` for a no-op
  `MockBusTransport`; omit it in a service with no bus (e.g. Search).
- `IntegrationDbInitializer` — the shared `IDbInitializer` that migrates inbox/outbox then migrates + seeds every
  registered `ITestSeeder`; register via `services.AddScoped<IDbInitializer, IntegrationDbInitializer>()` alongside
  the service's own seeders.

Only genuinely service-specific wiring (Duende/Razor, Payment-in-process, Stripe fakes, per-service mocks and
seeders) stays in the service's own fixture.

## Key design decisions

- **Each microservice owns its fixture** — the `ApiFixture` in each `…IntegrationTests.Fixtures` project is
  named identically but lives in its own namespace, so test projects import only their own.

- **Testcontainers** — a fresh SQL Server container starts per test run. `Respawn` resets
  data between tests without re-running migrations.

- **Authentication** — via `AddTestAuthentication()`; pass `X-Test-Sub` (user ID) and optionally
  `X-Test-Email` headers to authenticate a request. No token is required, and no `role` claim is emitted.

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

## Scoped services and event handlers

An integration test is a scope root. Resolve `Concertable.Kernel.DependencyInjection.IScoped<T>` from
`fixture.Services` and use `RunAsync` whenever the test needs one scoped `DbContext`, repository,
service, or handler collection. Do not hand-write `CreateScope()` / `CreateAsyncScope()` for those
cases. A manual scope is reserved for a test that must coordinate multiple distinct services in the
same scoped lifetime and has no narrower scope-root aggregate to resolve.

Dispatch integration events through
`IScoped<IEnumerable<IIntegrationEventHandler<TEvent>>>` and invoke every registered handler inside
that one scope, matching the in-process message pipeline. Use the Kernel abstraction for new code; the
copy in `Concertable.DataAccess` is a temporary compatibility surface for unmigrated consumers.

Do not use `IScoped<T>` from code that already runs inside an existing request or fixture-provided
scope. Resolve the dependency from that ambient scope instead so its `DbContext` and transaction stay
shared.

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
