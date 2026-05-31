# Customer SeedState — full composition root + read-model reference handles

Make `Concertable.Customer`'s seed object a rich composition root in the same shape as
B2B's `SeedState`, so the upcoming Customer **integration** and **E2E** tests have named,
deterministic handles to the data — exactly like B2B tests use `SeedState.ConfirmedBooking`,
`SeedState.VenueManager1`, etc.

This is a **self-contained plan**. Execute it cold after `/clear`. It explains the current
state, the hard constraint that makes Customer different from B2B, the target shape, the
exact files, and the open decisions that need a human answer before/while executing.

> Naming note: the seed composition-root class is now called **`SeedState`** in every service
> (renamed from `SeedData`). The cross-boundary spec list is **`SeedCatalog`**
> (`Concertable.B2B.Seed.Contracts`). Keep that vocabulary.

> ⚠ PREREQUISITE — fixture moved/renamed: `plans/TEST_PROJECT_RELOCATION.md` relocates and renames
> the Customer integration fixture. Do that plan's **Customer slice first**. After it,
> `Concertable.Testing.Integration.Customer` (at `api/Tests/`) no longer exists — it is now
> **`Concertable.Customer.IntegrationTests.Fixtures`** at
> `api/Concertable.Customer/Tests/Concertable.Customer.IntegrationTests.Fixtures/`. Everywhere
> below that says `Testing.Integration.Customer/ApiFixture` (§1.1, §1.6, §6 step 6), read the new
> name/location. Also: per that plan's Decision C2, `MockCustomerPaymentClient` now lives in this
> fixture (moved out of shared `Concertable.Testing.Integration`).

---

## 0. Prior state — what already landed

A large seed refactor already shipped on `Refactor/Microservices`:

- All seeding projects renamed `Concertable[.X].Seeding(.*)` → `Concertable[.X].Seed(.*)`.
- B2B split into `Concertable.B2B.Seed.Contracts` (cross-boundary: `SeedCatalog` + specs +
  `ToChangedEvent()` mappers) and `Concertable.B2B.Seed.Infrastructure` (B2B-internal:
  `SeedState`, all seed factories under `Factories/`).
- All seed-only entity factories (`Venue/Artist/Concert/Application/Booking/Opportunity/
  Contract*/User`) consolidated into `Concertable.B2B.Seed.Infrastructure/Factories/`.
- Shared seed projects grouped under `api/Shared/Seed/`:
  `Concertable.Seed.Identity` (zero-dep: `SeedUsers`, `SeedCustomers`, `EntityReflectionExtensions`),
  `Concertable.Seed.Shared` (EF/DI seeding infra), `Concertable.Seed.Infrastructure`
  (domain-event dispatch interceptor).
- Namespaces aligned to project+folder convention everywhere.
- `SeedData` → `SeedState` across B2B, Customer, and the Search integration test.

Customer's `SeedState` was **not** redesigned in that work — it is still minimal. That is this plan.

---

## 0a. Decisions locked (2026-05-31)

All §4 and §5 open decisions resolved + two new structural ones added (A, B). Execute against these:

| ID | Decision |
|---|---|
| §4 #1 | **E2E / dev: unchanged** — already convention-compliant via B2B Seed Simulator → bus → projection handlers. **Integration tests: direct-insert via new `XProjectionTestSeeder`s driven by the same `SeedCatalog`.** Documented exception to the read-models-via-handlers rule, scoped to integration tests only. Match between simulator output and direct insert is guaranteed because both consume the same catalog data. |
| §4 #2 | **Mirror B2B's `UserTestSeeder` precedent** — Customer's `UserTestSeeder` direct-inserts `seedState.Customers` (typed `UserEntity` list). Pragmatic test-only shortcut; production users still come from `CredentialRegisteredHandler` via Auth. |
| §4 #3 | **Yes — full `SeedState` includes Tickets and Reviews.** Build via Customer-owned factories same way B2B builds Bookings / Applications / Concerts. Expose typed named handles (`UpcomingFlatFeeTicket`, `ConfirmedConcertReview`, etc.). |
| §4 #4 | **Use `SeedCatalog` specs directly as reference handles** — no custom `XReference` DTOs. `Customer.Seed.Infrastructure` references `Concertable.B2B.Seed.Contracts` (cross-boundary OK). |
| §5 | **Keep identity data in shared `Concertable.Seed.Identity`.** Flatten `SeedCustomers` to B2B-`SeedUsers` shape (raw `Guid` / `string` accessors). Auth and all consumers continue to reference the shared lib — no relocation. |
| A (new) | **Drop the `SeedCustomer` record entirely.** `SeedCustomers` becomes a flat static class with `CustomerId(int n)` / `CustomerEmail(int n)` accessors mirroring `SeedUsers`. `SeedState` exposes typed `UserEntity Customer1 / Customer2 / Customer3` built via `UserFactory.FromRegistration`. Anywhere that took a `SeedCustomer` switches to taking the `UserEntity` from `SeedState`. |
| B (new) | **Rename project** `Concertable.Customer.Seed` → `Concertable.Customer.Seed.Infrastructure` (mirrors B2B's `Concertable.B2B.Seed.Infrastructure`). Namespace follows. **No `.Contracts` project** — Customer never publishes specs cross-service (per #4). |

---

## 1. Current Customer state (verified)

### 1.1 `Concertable.Customer.Seed/SeedState.cs` (today — minimal)

```csharp
namespace Concertable.Customer.Seed;

public sealed class SeedState
{
    public const string TestPassword = "Password11!";
    public SeedCustomer Customer { get; }            // = SeedCustomers.Customer1
    public IReadOnlyList<Guid> CustomerIds { get; }  // [Customer1.Id, Customer2.Id, Customer3.Id]

    public SeedState()
    {
        Customer = SeedCustomers.Customer1;
        CustomerIds = [.. SeedCustomers.All.Select(c => c.Id)];
    }
}
```

Referenced by: `Customer.Web`, `Customer.Concert.Infrastructure`, `Customer.Preference.Infrastructure`,
`Customer.E2ETests`, `Customer.E2ETests.Ui`, `Payment.Infrastructure`, `Testing.Integration.Customer`.

### 1.2 Customer modules (8)

Artist, Concert, Preference, Profile, Review, Ticket, User, Venue — each with
Domain / Application / Infrastructure / Contracts.

### 1.3 Read-model entities (projections — NOT directly seedable in prod)

| Entity | Path | Shape (key fields) |
|---|---|---|
| `VenueReadModel` (class `VenueEntity`) | `Modules/Venue/Concertable.Customer.Venue.Domain/Entities/VenueEntity.cs` | Id, UserId, Name, About, Avatar, BannerUrl, County, Town, Latitude, Longitude, Email, AverageRating, ReviewCount |
| `ArtistReadModel` (class `ArtistEntity`) | `Modules/Artist/Concertable.Customer.Artist.Domain/Entities/ArtistEntity.cs` | …same + `Genres` (ArtistGenreReadModel collection) |
| `ConcertReadModel` (class `ConcertEntity`) | `Modules/Concert/Concertable.Customer.Concert.Domain/Entities/ConcertEntity.cs` | Id, Name, About, BannerUrl?, Avatar?, TotalTickets, AvailableTickets, Price, Period (DateRange), DatePosted?, ArtistId, ArtistName, VenueId, VenueName, AverageRating, ReviewCount, `Genres` (ConcertGenreReadModel collection) |

> Confirm exact class names while executing — the file is `XEntity.cs` but the concept is the
> read model. Match whatever the projection handler instantiates.

### 1.4 Projection handlers (the ONLY production writers of those read models)

- `Modules/Venue/.../Infrastructure/Handlers/VenueProjectionHandler.cs` ← `VenueChangedEvent`
- `Modules/Artist/.../Infrastructure/Handlers/ArtistProjectionHandler.cs` ← `ArtistChangedEvent`
- `Modules/Concert/.../Infrastructure/Handlers/ConcertProjectionHandler.cs` ← `ConcertChangedEvent`

Each is idempotent (inbox-checked), calls `XReadModel.Create()`/`.Update()`, syncs genre
children, and `SaveChangesAsync()`. Events come from `Concertable.B2B.{Venue,Artist,Concert}.Contracts.Events`.

### 1.5 What Customer seeds directly today

- `PreferenceDevSeeder` (IDevSeeder, Order 7) — 3 `PreferenceEntity` (one per `SeedState.CustomerIds`).
- `PreferenceTestSeeder` (ITestSeeder, Order 7) — 1 `PreferenceEntity` for `SeedState.Customer`.
- No seeders in User / Ticket / Review / Profile yet.

### 1.6 How read models get populated in each context

- **E2E / dev:** `Concertable.Customer.AppHost` registers the **B2B Seed Simulator**
  (`builder.AddB2BSeedingSimulator<Projects.Concertable_B2B_Seed_Simulator>(asb)`), which
  publishes every `SeedCatalog` spec as an `XChangedEvent`; Customer's projection handlers
  populate the read models. Convention-compliant: identical code path to production.
- **Integration tests (`api/Tests/Concertable.Testing.Integration.Customer`):** today the
  `ApiFixture` **bypasses the handlers** and inserts read-model rows directly
  (`SeedVenueAsync` → `context.Venues.Add(...)`, etc.), with a mocked bus. ⚠️ This is the one
  spot that violates the "read models only via handlers" rule. See §4 decision.

### 1.7 B2B model to mirror (`Concertable.B2B.Seed.Infrastructure/SeedState.cs`)

`SeedState(SeedCatalog catalog)` builds **owned domain entities** via factories
(`catalog.Venues.Select(s => VenueFactory.Create(...))`, plus Contracts/Bookings/Applications)
and exposes both **collections** (`Venues`, `Concerts`, …) and **named handles**
(`ConfirmedBooking`, `VenueManager1`, `PostedFlatFeeApp`, …) for tests.

---

## 2. The hard constraint (why Customer ≠ B2B)

B2B `SeedState` is a **seed source**: it builds domain entities that the B2B seeders insert.
B2B *owns* those tables and production writes them directly, so seeding them directly is legal.

Customer's `Venue/Artist/Concert` are **read-model projections**. Production writes them **only**
via the projection handlers reacting to B2B events. Per `api/docs/SEEDING_CONVENTIONS.md` and
root `CLAUDE.md`, a seeder may only write what production writes directly — so in E2E / dev /
prod, Customer's read models are populated through the handler path (driven by the B2B Seed
Simulator in dev/E2E).

Therefore Customer `SeedState` plays **two roles**:

1. **Seed source** — for tables Customer genuinely owns: Customer Users, Preferences, Tickets,
   Reviews. Build these B2B-style with factories.
2. **Reference registry** — named handles to the **expected** projected read-model rows (venue 1,
   artist 2, concert "Upcoming FlatFee Show"). Handles are `SeedCatalog` specs directly, not
   the read-model entities. The actual DB rows come from:
   - **E2E / dev:** simulator publishes specs as `XChangedEvent` → projection handlers populate
     read models (convention-compliant, production code path).
   - **Integration tests (documented exception):** `XProjectionTestSeeder`s read the same specs
     and direct-insert read-model rows. Bypasses bus + handler for speed/isolation. Match with
     simulator output guaranteed because both consume the same `SeedCatalog`.

The reference values are derived from the **same `SeedCatalog`** that drives the events, so the
handle and the actual row are guaranteed to match (single source of truth across all three
consumers: handles, simulator, integration test seeders).

---

## 3. Target shape

`api/Concertable.Customer/Concertable.Customer.Seed.Infrastructure/SeedState.cs` (note rename per
decision B):

```csharp
namespace Concertable.Customer.Seed.Infrastructure;

public sealed class SeedState
{
    public const string TestPassword = "Password11!";

    // Customer users — typed entities, B2B style (decision A)
    public UserEntity Customer1 { get; }
    public UserEntity Customer2 { get; }
    public UserEntity Customer3 { get; }
    public IReadOnlyList<UserEntity> Customers { get; }

    // Customer-owned entities (Preferences/Tickets/Reviews built via Customer-side factories)
    public IReadOnlyList<PreferenceEntity> Preferences { get; }
    public IReadOnlyList<TicketEntity> Tickets { get; }
    public IReadOnlyList<ReviewEntity> Reviews { get; }

    // Named ticket / review handles (B2B-style)
    public TicketEntity UpcomingFlatFeeTicket { get; }
    public TicketEntity PastDoorSplitTicket { get; }
    public ReviewEntity ConfirmedConcertReview { get; }

    // Read-model reference handles — SeedCatalog specs directly (decision §4 #4).
    // NOT inserted here; rows come from simulator (E2E/dev) or XProjectionTestSeeder (integration).
    public VenueSeedSpec Venue { get; }
    public IReadOnlyList<VenueSeedSpec> Venues { get; }
    public ArtistSeedSpec Artist { get; }
    public IReadOnlyList<ArtistSeedSpec> Artists { get; }
    public ConcertSeedSpec UpcomingFlatFeeConcert { get; }
    public ConcertSeedSpec PastDoorSplitConcert { get; }
    public IReadOnlyList<ConcertSeedSpec> Concerts { get; }

    public SeedState(SeedCatalog catalog) { ... }
}
```

Ctor takes `SeedCatalog`. Register `AddSingleton<SeedCatalog>()` + `AddSingleton(TimeProvider.System)`
+ `AddScoped<SeedState>()` in `Customer.Web/Program.cs` (non-Testing branch) and both fixtures
(E2E AppFixture already registers `SeedCatalog`).

`SeedCustomers` (shared, flattened per decision A):

```csharp
namespace Concertable.Seed.Identity;

public static class SeedCustomers
{
    public const int CustomerCount = 3;
    public static Guid CustomerId(int n) => new($"c0000000-0000-0000-0000-{n:D12}");
    public static string CustomerEmail(int n) => $"customer{n}@test.com";
}
```

The `SeedCustomer` record is **deleted**. Any signature that took `SeedCustomer` now takes the
typed `UserEntity` from `SeedState` (or in shared library context, the raw `Guid` from
`SeedCustomers.CustomerId(n)`).

---

## 4. Decisions (locked — see §0a for the table)

All open decisions resolved on 2026-05-31. See the table in §0a for the locked answers. Originals
preserved here for context.

1. ~~How are read models populated for integration tests?~~ **Locked:** integration uses new
   `XProjectionTestSeeder`s driven by `SeedCatalog` (direct-insert, documented exception). E2E /
   dev keep the simulator → handler path (no change).
2. ~~Does Customer seed its own User rows?~~ **Locked:** yes, mirror B2B's `UserTestSeeder` —
   direct-insert `seedState.Customers`. Production unaffected.
3. ~~Tickets / Reviews?~~ **Locked:** yes, full SeedState exposes them via factories with named
   handles.
4. ~~Reference handle type?~~ **Locked:** `SeedCatalog` specs directly, no `XReference` DTOs.

---

## 5. SeedCustomers / SeedUsers ownership (locked)

**Locked:** **keep identity data in shared `Concertable.Seed.Identity`.** Flatten `SeedCustomers`
to B2B-`SeedUsers` shape (raw `Guid` / `string` accessors — see §3). Drop the `SeedCustomer`
record. Auth and every other cross-service consumer continues to reference the shared lib — no
relocation, no relocation-coordination work needed.

This is the simplest answer that preserves cross-service identity coordination while removing the
asymmetry that gave Customer a record-shape and B2B a flat-shape.

---

## 6. Implementation order

Do steps in order — each leaves the build green so failures localise.

1. **Flatten `SeedCustomers` (decision A).** Delete the `SeedCustomer` record. Rewrite
   `api/Shared/Seed/Concertable.Seed.Identity/SeedCustomers.cs` to flat shape (see §3). Update
   every caller that took `SeedCustomer` — pass `Guid` (for shared lib code) or the typed
   `UserEntity` (for service code via `SeedState`). Today's callers: `Customer.Web`,
   `Customer.Concert.Infrastructure`, `Customer.Preference.Infrastructure`, `Customer.E2ETests`,
   `Customer.E2ETests.Ui`, `Payment.Infrastructure`, `Customer.IntegrationTests.Fixtures`.
2. **Rename project (decision B).** `Concertable.Customer.Seed` →
   `Concertable.Customer.Seed.Infrastructure`. Update the csproj filename, folder, namespace, and
   every `<ProjectReference>` that pointed at it. Add it to `Concertable.Customer.slnx`. No
   `.Contracts` project — Customer doesn't publish specs.
3. **Add Customer seed factories.** Mirror B2B layout:
   `Concertable.Customer.Seed.Infrastructure/Factories/{UserFactory, PreferenceFactory,
   TicketFactory, ReviewFactory}.cs`. `UserFactory.FromRegistration(Guid, string)` for Customer
   users, plus factories for Preferences/Tickets/Reviews.
4. **Rewrite `SeedState`** per §3. Ctor takes `SeedCatalog`, builds Customer1/2/3 via
   `UserFactory`, builds Preferences/Tickets/Reviews via the new factories, exposes spec-typed
   reference handles for Venues/Artists/Concerts. Register `SeedCatalog`, `TimeProvider.System`,
   `SeedState` in `Customer.Web/Program.cs` (non-Testing) + both fixtures.
5. **Update existing Customer seeders.** `PreferenceDevSeeder` and `PreferenceTestSeeder` consume
   `SeedState.Preferences` (currently they reach for `CustomerIds`). Add new `UserTestSeeder`
   (mirrors B2B — direct-inserts `seedState.Customers`), `TicketTestSeeder`, `ReviewTestSeeder`.
6. **Add `XProjectionTestSeeder`s** (decision §4 #1, integration-only). One per read-model module:
   - `VenueProjectionTestSeeder` in `Customer.Venue.Infrastructure/Data/Seeders/`
   - `ArtistProjectionTestSeeder` in `Customer.Artist.Infrastructure/Data/Seeders/`
   - `ConcertProjectionTestSeeder` in `Customer.Concert.Infrastructure/Data/Seeders/`
   Each takes `XDbContext` + `SeedCatalog`, iterates `catalog.X`, calls `XReadModel.Create(...)`
   with the spec's fields, saves. **No dev variant** — dev uses the simulator path.
7. **Update `Customer.IntegrationTests.Fixtures/ApiFixture`**:
   - Register `SeedCatalog`, `SeedState`, all `ITestSeeder`s in `ConfigureTestServices`.
   - On `ResetAsync()`: run all `ITestSeeder`s in order; expose `SeedState` via property.
   - **Delete** the `SeedUserAsync` / `SeedVenueAsync` / `SeedArtistAsync` / `SeedConcertAsync` /
     `SeedTicketAsync` helpers.
   - Existing integration test files that called those helpers move to `fixture.SeedState.X` named
     handles.
8. **Update `Customer.E2ETests/AppFixture`** to expose the richer `SeedState` for E2E tests that
   want named handles. Simulator path is unchanged.
9. **Document the integration-test exception** in `api/docs/SEEDING_CONVENTIONS.md` —
   `XProjectionTestSeeder`s are exempt from "read models only via handlers" because they're
   driven from the same `SeedCatalog` that drives the simulator, so match is guaranteed.
10. **Verify:** `dotnet build api/Concertable.slnx` → 0 errors. `./integration.ps1 customer` →
    all green. `./e2e.ps1 regress` → green.

---

## 7. Boundary & convention checks

- `Concertable.Customer.Seed.Infrastructure` may reference `Concertable.B2B.Seed.Contracts`
  (SeedCatalog / specs — cross-boundary OK) but **must not** reference
  `Concertable.B2B.Seed.Infrastructure` (B2B Domain).
- `SeedState` must **not** `Add` any `XReadModel` — that's the simulator + handlers in dev/E2E,
  and `XProjectionTestSeeder`s in integration tests. `SeedState` only **describes** what those
  rows look like (via the spec handles).
- The integration-test direct-insert exception is documented in `SEEDING_CONVENTIONS.md` and is
  scoped to `XProjectionTestSeeder`s only. Test code that wants a read-model row reaches into
  `seedState.Venue` etc.; it never calls `db.Venues.Add()` itself.
- Re-read `api/docs/SEEDING_CONVENTIONS.md` in full before writing any seeder body.

---

## 8. Verification

- `dotnet build api/Concertable.slnx` → 0 errors.
- `./integration.ps1 customer` → all green.
- `./e2e.ps1 regress` passes (B2B 7/7, Customer 2/2 baseline) — run from the **repo root**
  (`e2e.ps1` pins `Set-Location $PSScriptRoot` + `[Environment]::CurrentDirectory`).
- `Concertable.Customer.Seed.Infrastructure` does not reference `Concertable.B2B.Seed.Infrastructure`
  (grep).
- `SeedCustomer` record no longer exists in the codebase (grep — should be zero matches).
- Integration / E2E tests resolve `SeedState` from DI and assert against its named handles
  (`seedState.UpcomingFlatFeeConcert`, `seedState.Customer1`, etc.).

---

## 9. Reference snippets

Locked code shapes for the work. Match these — don't reinvent.

### 9.1 Flattened `SeedCustomers`

```csharp
namespace Concertable.Seed.Identity;

public static class SeedCustomers
{
    public const int CustomerCount = 3;
    public static Guid CustomerId(int n) => new($"c0000000-0000-0000-0000-{n:D12}");
    public static string CustomerEmail(int n) => $"customer{n}@test.com";
}
```

### 9.2 `SeedState` (partial — composition root shape)

```csharp
namespace Concertable.Customer.Seed.Infrastructure;

public sealed class SeedState
{
    public const string TestPassword = "Password11!";

    public UserEntity Customer1 { get; }
    public UserEntity Customer2 { get; }
    public UserEntity Customer3 { get; }
    public IReadOnlyList<UserEntity> Customers { get; }

    public IReadOnlyList<PreferenceEntity> Preferences { get; }
    public IReadOnlyList<TicketEntity> Tickets { get; }
    public IReadOnlyList<ReviewEntity> Reviews { get; }

    public TicketEntity UpcomingFlatFeeTicket { get; }
    public TicketEntity PastDoorSplitTicket { get; }
    public ReviewEntity ConfirmedConcertReview { get; }

    public VenueSeedSpec Venue { get; }
    public IReadOnlyList<VenueSeedSpec> Venues { get; }
    public ArtistSeedSpec Artist { get; }
    public IReadOnlyList<ArtistSeedSpec> Artists { get; }
    public ConcertSeedSpec UpcomingFlatFeeConcert { get; }
    public ConcertSeedSpec PastDoorSplitConcert { get; }
    public IReadOnlyList<ConcertSeedSpec> Concerts { get; }

    public SeedState(SeedCatalog catalog)
    {
        Customer1 = UserFactory.FromRegistration(SeedCustomers.CustomerId(1), SeedCustomers.CustomerEmail(1));
        Customer2 = UserFactory.FromRegistration(SeedCustomers.CustomerId(2), SeedCustomers.CustomerEmail(2));
        Customer3 = UserFactory.FromRegistration(SeedCustomers.CustomerId(3), SeedCustomers.CustomerEmail(3));
        Customers = [Customer1, Customer2, Customer3];

        Preferences = Customers.Select(c => PreferenceFactory.CreateDefault(c.Id)).ToList();

        Venues = catalog.Venues;
        Venue = catalog.Venues[0];
        Artists = catalog.Artists;
        Artist = catalog.Artists[0];
        Concerts = catalog.Concerts;
        UpcomingFlatFeeConcert = catalog.Concerts.First(c => c.Name == "Upcoming FlatFee Show");
        PastDoorSplitConcert   = catalog.Concerts.First(c => c.Name == "Past DoorSplit Show");

        UpcomingFlatFeeTicket = TicketFactory.CreateForUpcoming(Customer1.Id, UpcomingFlatFeeConcert);
        PastDoorSplitTicket   = TicketFactory.CreateForPast(Customer1.Id, PastDoorSplitConcert);
        Tickets = [UpcomingFlatFeeTicket, PastDoorSplitTicket];

        ConfirmedConcertReview = ReviewFactory.Create(Customer1.Id, PastDoorSplitConcert);
        Reviews = [ConfirmedConcertReview];
    }
}
```

(Exact handle list — Tickets, Reviews, Concerts — settles during step 4; the named cases above
are illustrative.)

### 9.3 Projection test seeder (one per read-model module)

```csharp
namespace Concertable.Customer.Venue.Infrastructure.Data.Seeders;

internal class VenueProjectionTestSeeder : ITestSeeder
{
    public int Order => 1;

    private readonly VenueDbContext context;
    private readonly SeedCatalog catalog;

    public VenueProjectionTestSeeder(VenueDbContext context, SeedCatalog catalog)
    {
        this.context = context;
        this.catalog = catalog;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            foreach (var spec in catalog.Venues)
            {
                context.Venues.Add(VenueReadModel.Create(
                    venueId: spec.VenueId, userId: spec.UserId,
                    name: spec.Name, about: spec.About,
                    avatar: spec.Avatar, bannerUrl: spec.BannerUrl,
                    county: spec.County, town: spec.Town,
                    latitude: spec.Latitude, longitude: spec.Longitude,
                    email: spec.Email));
            }
            await context.SaveChangesAsync(ct);
        });
    }
}
```

Same shape for `ArtistProjectionTestSeeder` and `ConcertProjectionTestSeeder`. Field-to-field
maps spec → `XReadModel.Create(...)`.

### 9.4 `ApiFixture` wiring

```csharp
builder.ConfigureTestServices(services =>
{
    // ... existing logging / auth / messaging mocks ...

    services.AddSingleton(TimeProvider.System);
    services.AddSingleton<SeedCatalog>();
    services.AddScoped<SeedState>();

    services.AddScoped<ITestSeeder, UserTestSeeder>();
    services.AddScoped<ITestSeeder, PreferenceTestSeeder>();
    services.AddScoped<ITestSeeder, TicketTestSeeder>();
    services.AddScoped<ITestSeeder, ReviewTestSeeder>();
    services.AddScoped<ITestSeeder, VenueProjectionTestSeeder>();
    services.AddScoped<ITestSeeder, ArtistProjectionTestSeeder>();
    services.AddScoped<ITestSeeder, ConcertProjectionTestSeeder>();
});
```

```csharp
public SeedState SeedState { get; private set; } = null!;

public async Task ResetAsync()
{
    await sqlFixture.ResetAsync();
    NotificationClient.Reset();

    scope?.Dispose();
    scope = factory.Services.CreateScope();
    SeedState = scope.ServiceProvider.GetRequiredService<SeedState>();

    foreach (var seeder in scope.ServiceProvider.GetServices<ITestSeeder>().OrderBy(s => s.Order))
        await seeder.SeedAsync();
}
```

All of `SeedUserAsync` / `SeedVenueAsync` / `SeedArtistAsync` / `SeedConcertAsync` /
`SeedTicketAsync` on the fixture are **deleted**. Tests get the full catalog by default and
reach into `fixture.SeedState.X` for named handles — same shape as B2B integration tests.
