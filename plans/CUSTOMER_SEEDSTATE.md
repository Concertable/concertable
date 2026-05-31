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
root `CLAUDE.md`, a seeder may only write what production writes directly — so Customer's
`SeedState` **must not** `context.Venues.Add(readModel)`.

Therefore Customer `SeedState` plays **two roles**, and for read models only the second applies:

1. **Seed source** — for tables Customer genuinely owns (Preferences; and Customer users /
   tickets / reviews if seeded). Build these B2B-style with factories.
2. **Reference registry** — named handles describing the **expected** projected read-model rows
   (venue 1, artist 2, concert "Upcoming FlatFee Show"), so tests can assert against them. The
   *rows themselves* are produced by replaying `SeedCatalog` events through the projection
   handlers, never inserted by `SeedState`.

The reference values are derived from the **same `SeedCatalog`** that drives the events, so the
handle and the actual projected row are guaranteed to match (single source of truth).

---

## 3. Target shape

`Concertable.Customer.Seed/SeedState.cs`:

```csharp
public sealed class SeedState
{
    public const string TestPassword = "Password11!";

    // Customer-owned personas (kept; widen to named handles)
    public SeedCustomer Customer { get; }            // Customer1 (primary)
    public SeedCustomer OtherCustomer { get; }       // Customer2
    public IReadOnlyList<SeedCustomer> Customers { get; }
    public IReadOnlyList<Guid> CustomerIds { get; }  // kept — PreferenceSeeder depends on it

    // Customer-owned entities Customer seeds directly (build via factories, B2B-style)
    public IReadOnlyList<PreferenceEntity> Preferences { get; }
    // (+ Customer User entities / Tickets / Reviews IF those become seeded — see §4)

    // Reference handles to the EXPECTED projected read-model rows (NOT inserted here).
    // Derived from SeedCatalog so they match what the handlers produce.
    public ConcertReference UpcomingFlatFeeConcert { get; }   // e.g. catalog.Concerts.First(c => c.Name == "Upcoming FlatFee Show")
    public IReadOnlyList<VenueReference> Venues { get; }
    public IReadOnlyList<ArtistReference> Artists { get; }
    public IReadOnlyList<ConcertReference> Concerts { get; }

    public SeedState(SeedCatalog catalog) { ... }
}
```

Where `XReference` is a lightweight projection of the spec (id + the fields tests assert on),
**not** the Customer `XReadModel` entity (Customer.Seed must not depend on the read-model Domain
internals any more than necessary — decide in §4 whether to expose the spec directly vs a
purpose-built reference type). Simplest: expose the relevant `SeedCatalog` specs directly
(`VenueSeedSpec` etc.) as the reference handles, since they already carry every asserted field
and `Customer.Seed` can reference `Concertable.B2B.Seed.Contracts` (cross-boundary OK).

Ctor takes `SeedCatalog` (register `AddSingleton<SeedCatalog>()` + `AddSingleton(TimeProvider.System)`
in Customer.Web / both fixtures — the E2E AppFixture already registers `SeedCatalog`).

---

## 4. Open decisions (need a human answer — do not guess)

1. **How are read models populated for integration tests?** Today `ApiFixture` direct-inserts
   them (convention violation, but isolated/no-bus). Options:
   - **(a) Convention-compliant replay (recommended):** add a Customer test seeder / fixture
     helper that takes `SeedCatalog`, calls `spec.ToChangedEvent()`, and invokes the real
     `VenueProjectionHandler/ArtistProjectionHandler/ConcertProjectionHandler` **in-process**
     (no bus). Read models populated the production way; `SeedState` exposes references. This
     unifies E2E and integration on one path and kills the §1.6 violation.
   - **(b) Keep direct inserts** in integration tests as a pragmatic test-only shortcut, and
     document the exception. Lower effort, keeps the violation.
   Decision: __________

2. **Does Customer seed its own `User` rows, and how?** B2B users are created by
   `CredentialRegisteredHandler` reacting to `CredentialRegisteredEvent` from Auth (NEVER direct
   insert). Customer users are the same pattern. So `SeedState` should **not** build Customer
   `UserEntity` directly — confirm whether E2E relies on Auth registration for customer users and
   whether integration tests need a convention-compliant path (replay `CredentialRegisteredEvent`).
   Decision: __________

3. **Tickets / Reviews:** should `SeedState` expose seeded tickets/reviews (Customer-owned,
   purchase-time snapshots)? If tests need them, build via factories B2B-style. If not, omit.
   Decision: __________

4. **Reference handle type:** expose `SeedCatalog` specs directly as the read-model references,
   or introduce Customer-side `XReference` DTOs? (Specs are simplest and already cross-boundary.)
   Decision: __________

---

## 5. SeedCustomers / SeedUsers ownership (flagged — decide here)

`api/Shared/Seed/Concertable.Seed.Identity/` currently holds **both**:
- `SeedUsers` — B2B-owned identities (artist/venue managers, admin Guids/emails).
- `SeedCustomers` — Customer-owned identities (customer Guids/emails).
- `EntityReflectionExtensions` — genuinely shared (used by every seed factory).

The data ones are mis-located: `SeedUsers` is B2B's, `SeedCustomers` is Customer's. They live in
shared **only** to coordinate cross-service identity: **Auth** registers these exact Guids, and
B2B/Customer/Payment all reference the same ones, so they must agree.

**Decision needed:** do we relocate the identity data to the owning services, or keep it shared?

- If we relocate: `SeedCustomers` → a Customer-owned seed-identity location, `SeedUsers` → a
  B2B-owned one. Then **Auth** (and any cross-service consumer) needs another way to obtain the
  canonical Guids — e.g. a thin cross-boundary identity contract, or Auth references the owning
  service's seed contracts. Spell out how coordination is preserved before moving anything.
- If we keep shared: at minimum split `Seed.Identity` conceptually so B2B vs Customer identity
  data is clearly delineated, and leave `EntityReflectionExtensions` as the shared remainder.

This is **out of scope for the SeedState body** but in scope for "do it right." Capture the
decision and, if relocating, sequence it as its own step with the Auth-coordination fix first.

Decision: __________

---

## 6. Implementation order (once §4/§5 decided)

1. Add `SeedState(SeedCatalog catalog)` ctor; register `SeedCatalog` + `TimeProvider` in
   `Customer.Web/Program.cs` (non-Testing branch) and both E2E/integration fixtures (E2E already
   has `SeedCatalog`).
2. Build Customer-owned entities via factories (start with `Preferences`; mirror B2B factory
   placement — put any new Customer seed factories in `Concertable.Customer.Seed/Factories/`).
3. Add read-model **reference handles** derived from `catalog` (specs or `XReference`).
4. Per §4(1): wire read-model population — replay path (recommended) or keep direct inserts.
5. Update `PreferenceDevSeeder`/`PreferenceTestSeeder` to consume the new `SeedState` shape
   (they already depend on `CustomerIds` — keep it).
6. Update `Customer.E2ETests/AppFixture` + `Testing.Integration.Customer/ApiFixture` to expose the
   richer `SeedState` and use the reference handles in assertions.
7. Build (`dotnet build api/Concertable.slnx`, 0 errors) + `./e2e.ps1 regress` green.

---

## 7. Boundary & convention checks

- `Concertable.Customer.Seed` may reference `Concertable.B2B.Seed.Contracts` (SeedCatalog/specs —
  cross-boundary OK) but **must not** reference `Concertable.B2B.Seed.Infrastructure` (B2B Domain).
- `SeedState` must **not** `Add` any `XReadModel` — read models come from handlers only.
- Re-read `api/docs/SEEDING_CONVENTIONS.md` in full before writing any seeder body.

---

## 8. Verification

- Solution builds with 0 errors.
- `./e2e.ps1 regress` passes (B2B 7/7, Customer 2/2 baseline) — run from the **repo root**
  (`e2e.ps1` pins `Set-Location $PSScriptRoot` + `[Environment]::CurrentDirectory`, so cwd is safe).
- Customer.Seed does not reference B2B.Seed.Infrastructure (grep).
- New integration/e2e tests can resolve `SeedState` from DI and assert against its handles.
