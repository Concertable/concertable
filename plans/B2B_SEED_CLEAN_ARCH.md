# B2B Seed Clean Architecture Refactor

Rename and reorganize the B2B seed projects to match the codebase's existing module/factory conventions. Introduce static factories for spec → entity construction so that side matches `ApplicationFactory` / `BookingFactory` / `UserFactory` shape. Keep the source-of-truth concept: one canonical catalog of spec data, derived into both wire events (cross-boundary) and Domain entities (B2B-internal).

This is a self-contained doc — explains the current state, the problems, the design decisions, and the implementation order. No prior conversation needed.

---

## 1. Context — where we are today

After the prior `B2BSeedFixture` refactor (commit `57a60743`) plus the uncommitted spec-move-into-Specs/-subfolder edits, the state is:

```
Concertable.B2B.Seeding.Fixture/                  (cross-boundary; Customer + simulator + B2B)
  B2BSeedFixture.cs + Venues/Artists/Concerts partials
  Specs/VenueSeedSpec.cs, ArtistSeedSpec.cs, ConcertSeedSpec.cs
  SeedSpecMappers.cs                              spec → ChangedEvent (extensions)

Concertable.B2B.Seeding/                          (B2B-internal; Domain visible)
  SeedData.cs                                     composition root
  SeedSpecMappers.cs                              spec → DomainEntity (extensions)
  Extensions/, Fakers/                            existing helpers
```

`B2BSeedFixture` is a DI singleton holding `Venues` / `Artists` / `Concerts` (spec lists) plus a captured `Now`. `SeedData` is the B2B composition root holding materialized Domain entities + B2B-only entities (Contracts, Opportunities, Bookings, Applications, users) + named handles for tests (`ConfirmedBooking`, `PostedFlatFeeApp`, `VenueManager1`, etc.).

---

## 2. Problems we're addressing

### 2.1 Project naming — `Seeding.Fixture` vs `Seeding`

The two project names are one word apart and rhyme. At a glance you can't tell which is which without reading the suffix. `Fixture` also reads as an xUnit/test concept, which it isn't.

### 2.2 Class naming — `B2BSeedFixture` and `SeedData` both look like "the seed thing"

Both classes have `Seed` in the name, both are central singletons, both are referenced from SeedData/E2E setups. New readers can't immediately tell which holds specs (cross-boundary) and which holds materialized entities (B2B-internal).

### 2.3 `ToEntity` is factory-shaped work done as an extension method

The current `Concertable.B2B.Seeding/SeedSpecMappers.cs` has `ToEntity()` extensions doing real factory work:

- Call the Domain factory (`VenueEntity.Create(...)`)
- Set the Id via reflection (`.With(nameof(VenueEntity.Id), spec.VenueId)`)
- Apply a state transition (`.Approve()`, or `.Post()` conditionally for Concert)

This work belongs to a factory. Every other entity in the codebase has a static factory class for this: `ApplicationFactory`, `BookingFactory`, `OpportunityFactory`, `ContractFactory`, `UserFactory`. The seed-time construction of `VenueEntity` / `ArtistEntity` / `ConcertEntity` is the odd one out — done via spec extensions instead.

### 2.4 `ToChangedEvent` (the event side) has no factory-shaped work

By contrast, the spec → event conversion is **pure shape mapping**:

```csharp
public static VenueChangedEvent ToChangedEvent(this VenueSeedSpec spec) => new(
    spec.VenueId, spec.UserId, spec.Name, ...);
```

No factory call, no reflection, no state transitions. Just a record construction. There's no encapsulation to do.

This asymmetry between the two sides — one has real construction ceremony, the other doesn't — should be reflected honestly in the code: factory on the entity side, no factory on the event side.

---

## 3. Design decisions

### 3.1 Project rename

The renames need to be **consistent across every seeding project in the codebase**, not just B2B. Naming `Seed`/`Seeding` cleanly for one service and leaving the others as `Seeding.*` just shifts the inconsistency. Full matrix:

**Per-service seeding projects:**

| Before | After | Visibility | Holds |
|---|---|---|---|
| `Concertable.B2B.Seeding.Fixture` | `Concertable.B2B.Seed.Contracts` | Cross-boundary | `SeedCatalog`, spec records, `SeedSpecMappers` (events) |
| `Concertable.B2B.Seeding` | `Concertable.B2B.Seed.Infrastructure` | B2B-internal | `SeedData`, factories, existing extensions/fakers |
| `Concertable.B2B.Seeding.Simulator` | `Concertable.B2B.Seed.Simulator` | Worker host | Publishes the catalog's events on startup |
| `Concertable.Customer.Seeding` | `Concertable.Customer.Seed` | Customer-internal | `SeedData` (TestPassword const, SeedCustomer ref, CustomerIds list) |
| `Concertable.Payment.Seeding` | `Concertable.Payment.Seed` | Payment-internal | Payment seed data (audit before rename to confirm content) |

**Shared seeding projects (root-level):**

| Before | After | Holds |
|---|---|---|
| `Concertable.Seeding.Shared` | `Concertable.Seed.Shared` | Shared helpers across services |
| `Concertable.Seeding.Identity` | `Concertable.Seed.Identity` | `SeedUsers` (Guid generators for seed user identities) |
| `Concertable.Seeding.Infrastructure` | `Concertable.Seed.Infrastructure` | Shared seeding infrastructure (`IDevSeeder`, `SeedingScope`, etc.) |

**Naming rule applied:**
- `Seed` (noun) → the data, the contracts, the records
- All projects use `Seed` regardless of whether they hold data, factories, or process helpers — consistency wins over verb/noun precision
- The Customer / Payment per-service projects don't split into `.Contracts` / `.Infrastructure` because they have no cross-boundary content; flat `.Seed` is enough
- B2B is the only one that needs the split because it's the only seed that ships specs across the microservice boundary

The pair `Seed.Contracts` / `Seed.Infrastructure` mirrors the existing per-module convention (`Venue.Contracts` / `Venue.Infrastructure`, `Concert.Contracts` / `Concert.Infrastructure`, etc.). Reads immediately as "the contracts half of the seed module" vs "the internal infrastructure half." No rhyme problem, no `Fixture` baggage.

**Note:** the shared rename in particular has wide blast radius — `Concertable.Seeding.Identity.SeedUsers` is referenced from every seeder + the catalog. Worth doing as a separate commit ahead of the B2B-specific work so the larger churn is isolated.

### 3.2 Class rename

| Before | After |
|---|---|
| `B2BSeedFixture` | `SeedCatalog` |
| `SeedData` | unchanged |

`SeedCatalog` is self-describing — a catalog of seed specs. Distinct vocabulary from `SeedData` (Catalog vs Data are different words, not different suffixes on the same root). The full type `Concertable.B2B.Seed.Contracts.SeedCatalog` has the same "module name in class name" pattern as `Venue.Contracts.VenueChangedEvent` — codebase convention.

`SeedData` stays unchanged because:
- It's referenced across the codebase (tests, B2B seeders, DI registrations) — renaming touches many files for cosmetic gain
- The original confusion (both classes share `Seed` prefix and look like variants of the same thing) is resolved by renaming the catalog alone — `SeedCatalog` and `SeedData` are clearly distinct

### 3.3 Static factories on the entity side

Add three static classes in `Concertable.B2B.Seed.Infrastructure/Factories/`:

- `VenueFactory.cs`
- `ArtistFactory.cs`
- `ConcertFactory.cs`

Each is a static class with a single `Create(...)` method taking the entity's construction parameters as primitives (Id, UserId, Name, etc.). Same shape as `ApplicationFactory.Create(int artistId, int opportunityId)` or `BookingFactory.Complete(int id)`.

The factory **does not know about the spec record**. It takes plain primitives. `SeedData` unpacks the spec at the call site and passes the values to the factory.

```csharp
public static class VenueFactory
{
    public static VenueEntity Create(
        int id,
        Guid userId,
        string name,
        string about,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email)
    {
        var venue = VenueEntity
            .Create(userId, name, about, bannerUrl, avatar, location, address, email)
            .With(nameof(VenueEntity.Id), id);
        venue.Approve();
        return venue;
    }
}
```

`ArtistFactory.Create(int id, Guid userId, string name, ..., IEnumerable<Genre> genres)` — same shape, plus genres, no `.Approve()` (ArtistEntity doesn't have that step).

`ConcertFactory.Create(int id, int bookingId, int artistId, int venueId, DateRange period, string name, string about, IEnumerable<Genre> genres, decimal price, int totalTickets, DateTime? datePosted)` — `CreateDraft` + `.With(Id)` + `.With(Price)` + `.With(TotalTickets)` + conditional `.Post(...)` if `datePosted is not null`.

### 3.4 Event side stays as a thin extension

Keep `SeedSpecMappers.ToChangedEvent` in `Concertable.B2B.Seed.Contracts`. The asymmetry between this and the entity factories reflects a real difference: events are plain record constructions, entities are factory-built with reflection and state transitions. We don't add a factory class to wrap a one-line `new XChangedEvent(...)`.

This isn't "disjointed pattern" — it's the honest answer to "what does each side actually need":

| Side | Work to do | Shape |
|---|---|---|
| Spec → entity | Domain factory call + Id reflection + state transition | Static factory class |
| Spec → event | Record construction | Static extension method |

The simulator's call site stays compact: `v.ToChangedEvent()` per loop iteration.

### 3.5 `SeedData` consumes the factories

`SeedData` injects the three factories alongside `SeedCatalog`. Its three projection lines unpack each spec and call the factory:

```csharp
Venues = [.. catalog.Venues.Select(s => VenueFactory.Create(
    id: s.VenueId,
    userId: s.UserId,
    name: s.Name,
    about: s.About,
    bannerUrl: s.BannerUrl,
    avatar: s.Avatar,
    location: new Point(s.Longitude, s.Latitude) { SRID = 4326 },
    address: new Address(s.County, s.Town),
    email: s.Email))];
```

Same pattern for Artists. ConcertFactory takes the extra `bookingId` looked up from `Bookings`.

The spec is the **source** of those primitives; the factory is **what builds the entity**. The spec doesn't carry behavior; the factory doesn't know about specs.

---

## 4. Final shape

### 4.1 Project structure

```
Concertable.B2B.Seed.Contracts/                   (cross-boundary)
  SeedCatalog.cs                                  shell — ctor, Now
  SeedCatalog.Venues.cs                           partial — 35 VenueSeedSpec literals
  SeedCatalog.Artists.cs                          partial — 35 ArtistSeedSpec literals
  SeedCatalog.Concerts.cs                         partial — 47 ConcertSeedSpec literals (lazy)
  Specs/
    VenueSeedSpec.cs
    ArtistSeedSpec.cs
    ConcertSeedSpec.cs
  SeedSpecMappers.cs                              spec → ChangedEvent (extensions)

Concertable.B2B.Seed.Infrastructure/              (B2B-internal — references Domain)
  SeedData.cs                                     composition root (unchanged name)
  Factories/
    VenueFactory.cs                               static — primitives → VenueEntity
    ArtistFactory.cs
    ConcertFactory.cs
  Extensions/                                     existing (EntityReflectionExtensions)
  Fakers/                                         existing (LocationFaker, etc.)
```

### 4.2 `SeedCatalog` (the cross-boundary catalog)

```csharp
namespace Concertable.B2B.Seed.Contracts;

public sealed partial class SeedCatalog
{
    public DateTime Now { get; }

    public SeedCatalog(TimeProvider time)
    {
        this.Now = time.GetUtcNow().UtcDateTime;
    }
}
```

Plus the three partials with literal spec lists, identical to today's `B2BSeedFixture.Venues.cs` / `.Artists.cs` / `.Concerts.cs` — just the namespace and class name change.

### 4.3 `SeedSpecMappers` (cross-boundary event mappers)

```csharp
namespace Concertable.B2B.Seed.Contracts;

public static class SeedSpecMappers
{
    public static VenueChangedEvent ToChangedEvent(this VenueSeedSpec spec) => new(
        spec.VenueId, spec.UserId, spec.Name, spec.About, spec.Avatar, spec.BannerUrl,
        spec.County, spec.Town, spec.Latitude, spec.Longitude, spec.Email);

    public static ArtistChangedEvent ToChangedEvent(this ArtistSeedSpec spec) => new(
        spec.ArtistId, spec.UserId, spec.Name, spec.About, spec.Avatar, spec.BannerUrl,
        spec.County, spec.Town, spec.Latitude, spec.Longitude, spec.Email, spec.Genres);

    public static ConcertChangedEvent ToChangedEvent(this ConcertSeedSpec spec) => new(
        spec.ConcertId, spec.Name, spec.About, spec.Avatar, spec.BannerUrl,
        spec.TotalTickets, spec.AvailableTickets, spec.Price, spec.Period, spec.DatePosted,
        spec.ArtistId, spec.ArtistName, spec.VenueId, spec.VenueName,
        spec.Latitude, spec.Longitude, spec.Genres, spec.PayeeUserId);
}
```

Unchanged from today except for namespace.

### 4.4 Factories (B2B-internal)

`VenueFactory`:

```csharp
namespace Concertable.B2B.Seed.Infrastructure.Factories;

public static class VenueFactory
{
    public static VenueEntity Create(
        int id,
        Guid userId,
        string name,
        string about,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email)
    {
        var venue = VenueEntity
            .Create(userId, name, about, bannerUrl, avatar, location, address, email)
            .With(nameof(VenueEntity.Id), id);
        venue.Approve();
        return venue;
    }
}
```

`ArtistFactory`:

```csharp
namespace Concertable.B2B.Seed.Infrastructure.Factories;

public static class ArtistFactory
{
    public static ArtistEntity Create(
        int id,
        Guid userId,
        string name,
        string about,
        string bannerUrl,
        string avatar,
        Point location,
        Address address,
        string email,
        IEnumerable<Genre> genres)
        => ArtistEntity
            .Create(userId, name, about, bannerUrl, avatar, location, address, email, genres)
            .With(nameof(ArtistEntity.Id), id);
}
```

`ConcertFactory`:

```csharp
namespace Concertable.B2B.Seed.Infrastructure.Factories;

public static class ConcertFactory
{
    public static ConcertEntity Create(
        int id,
        int bookingId,
        int artistId,
        int venueId,
        DateRange period,
        string name,
        string about,
        IEnumerable<Genre> genres,
        decimal price,
        int totalTickets,
        DateTime? datePosted)
    {
        var concert = ConcertEntity
            .CreateDraft(bookingId, artistId, venueId, period, name, about, genres)
            .With(nameof(ConcertEntity.Id), id)
            .With(nameof(ConcertEntity.Price), price)
            .With(nameof(ConcertEntity.TotalTickets), totalTickets);
        if (datePosted is not null)
            concert.Post(concert.Name, concert.About, concert.Price, concert.TotalTickets, datePosted.Value);
        return concert;
    }
}
```

### 4.5 `SeedData` projection lines

Replace the three spec→entity projection lines:

```csharp
// Before
Artists  = [.. fixture.Artists.Select(s => s.ToEntity())];
Venues   = [.. fixture.Venues.Select(s => s.ToEntity())];
Concerts = [.. fixture.Concerts.Select(s => s.ToEntity(bookingId: Bookings[s.ConcertId - 1].Id))];

// After
Venues = [.. catalog.Venues.Select(s => VenueFactory.Create(
    id: s.VenueId, userId: s.UserId,
    name: s.Name, about: s.About,
    bannerUrl: s.BannerUrl, avatar: s.Avatar,
    location: new Point(s.Longitude, s.Latitude) { SRID = 4326 },
    address: new Address(s.County, s.Town),
    email: s.Email))];

Artists = [.. catalog.Artists.Select(s => ArtistFactory.Create(
    id: s.ArtistId, userId: s.UserId,
    name: s.Name, about: s.About,
    bannerUrl: s.BannerUrl, avatar: s.Avatar,
    location: new Point(s.Longitude, s.Latitude) { SRID = 4326 },
    address: new Address(s.County, s.Town),
    email: s.Email,
    genres: s.Genres))];

Concerts = [.. catalog.Concerts.Select(s => ConcertFactory.Create(
    id: s.ConcertId,
    bookingId: Bookings[s.ConcertId - 1].Id,
    artistId: s.ArtistId, venueId: s.VenueId,
    period: s.Period,
    name: s.Name, about: s.About,
    genres: s.Genres,
    price: s.Price,
    totalTickets: s.TotalTickets,
    datePosted: s.DatePosted))];
```

Ctor changes from `SeedData(B2BSeedFixture fixture)` to `SeedData(SeedCatalog catalog)` — the factories are static so they don't need to be injected.

### 4.6 Simulator and other consumers

`SeedEventPublishingService` keeps the same shape — just replaces `B2BSeedFixture` with `SeedCatalog`:

```csharp
public SeedEventPublishingService(
    IBusTransport transport,
    SeedCatalog catalog,
    IHostApplicationLifetime lifetime,
    ILogger<SeedEventPublishingService> logger)
{
    ...
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    foreach (var v in catalog.Venues)
        await transport.PublishAsync(v.ToChangedEvent(), Envelope(...), stoppingToken);
    ...
}
```

`ConcertProjectionHealthCheck` (Customer.Concert.Infrastructure) — same swap, `B2BSeedFixture` → `SeedCatalog`.

`Customer.E2ETests/AppFixture` — expose `SeedCatalog Catalog { get; }` (rename from `B2BSeed`), update DI registration.

`TicketPurchaseTests` — `fixture.Catalog.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId`.

---

## 5. Boundary check

Customer must never transitively reach B2B Domain.

```
Customer.E2ETests
  → references: Concertable.B2B.Seed.Contracts        ✓ (cross-boundary OK)
  → does NOT reference: Concertable.B2B.Seed.Infrastructure   ✓ (would pull Domain)

Customer.Web (via Customer.Concert.Infrastructure)
  → references: Concertable.B2B.Seed.Contracts        ✓
  → does NOT reference: Concertable.B2B.Seed.Infrastructure   ✓

Concertable.B2B.Seeding.Simulator
  → references: Concertable.B2B.Seed.Contracts        ✓
  → does NOT reference: Concertable.B2B.Seed.Infrastructure   ✓

Concertable.B2B.Web (B2B-internal)
  → references: Concertable.B2B.Seed.Contracts        ✓
  → references: Concertable.B2B.Seed.Infrastructure   ✓ (allowed — B2B owns Domain)
```

Grep check after the refactor (should return zero hits):

```
rg 'Concertable\.B2B\.Seed\.Infrastructure' \
   api/Concertable.Customer api/Concertable.B2B/Concertable.B2B.Seeding.Simulator
```

---

## 6. DI registrations

Per consuming host:

```csharp
// Concertable.B2B.Web/Program.cs (!Testing branch)
services.AddSingleton<SeedCatalog>();          // was AddSingleton<B2BSeedFixture>
services.AddScoped<SeedData>();                // unchanged

// Concertable.B2B.Seeding.Simulator/Program.cs
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SeedCatalog>();  // was AddSingleton<B2BSeedFixture>

// Concertable.Customer.Web/Program.cs
services.AddSingleton<SeedCatalog>();          // was AddSingleton<B2BSeedFixture>

// Concertable.B2B.E2ETests/AppFixture.cs (seed host)
services.AddSingleton(TimeProvider.System);
services.AddSingleton<SeedCatalog>();          // was AddSingleton<B2BSeedFixture>

// Concertable.Customer.E2ETests/AppFixture.cs (seed host)
services.AddSingleton(TimeProvider.System);
services.AddSingleton<SeedCatalog>();          // was AddSingleton<B2BSeedFixture>
```

Factories are static — no DI registration needed.

`AppFixture.B2BSeed` property in `Customer.E2ETests/AppFixture.cs` renames to `AppFixture.Catalog` (or `SeedCatalog`).

---

## 7. Implementation order

Recommended commit boundary: do the shared renames first (small, mechanical, wide reach), then the per-service B2B work as a second commit, then the Customer/Payment renames as a third — keeps each commit reviewable and bisectable.

### Phase 1 — shared project renames (separate commit)

0a. **Rename `Concertable.Seeding.Shared` → `Concertable.Seed.Shared`**
0b. **Rename `Concertable.Seeding.Identity` → `Concertable.Seed.Identity`**
0c. **Rename `Concertable.Seeding.Infrastructure` → `Concertable.Seed.Infrastructure`**
For each: rename folder, csproj, namespace in every .cs file, every `ProjectReference` in every consuming csproj. Build verify. Commit.

### Phase 2 — B2B seed clean-arch (this plan's main work)

1. **Rename project `Concertable.B2B.Seeding.Fixture` → `Concertable.B2B.Seed.Contracts`**
   - Rename the folder
   - Rename the csproj
   - Update `RootNamespace` and `AssemblyName` if present
   - Update every `ProjectReference` in consuming csprojs (B2B.Web, B2B.E2ETests, Customer.Web, Customer.E2ETests, Customer.Concert.Infrastructure, B2B.Seeding.Simulator, Concertable.B2B.Seeding)

2. **Rename class `B2BSeedFixture` → `SeedCatalog`**
   - Rename file `B2BSeedFixture.cs` → `SeedCatalog.cs`, and the three partials accordingly
   - Update namespace in all five files (.cs + 3 partials + Specs/*.cs + SeedSpecMappers.cs) from `Concertable.B2B.Seeding.Fixture` to `Concertable.B2B.Seed.Contracts` (and `Concertable.B2B.Seed.Contracts.Specs` for the spec records)
   - Update class name `B2BSeedFixture` → `SeedCatalog` in declarations and ctor signature

3. **Rename project `Concertable.B2B.Seeding` → `Concertable.B2B.Seed.Infrastructure`**
   - Rename folder, csproj, update RootNamespace if present
   - Update every `ProjectReference` in consuming csprojs (B2B.Web, B2B.E2ETests)

4. **Update namespace in all `Concertable.B2B.Seed.Infrastructure` files** from `Concertable.B2B.Seeding` to `Concertable.B2B.Seed.Infrastructure`. This is `SeedData.cs`, `SeedSpecMappers.cs`, all files under `Extensions/` and `Fakers/`.

5. **Add `Concertable.B2B.Seed.Infrastructure/Factories/` directory and three factory classes** (`VenueFactory.cs`, `ArtistFactory.cs`, `ConcertFactory.cs`). Bodies as in §4.4. Each in namespace `Concertable.B2B.Seed.Infrastructure.Factories`.

6. **Delete `Concertable.B2B.Seed.Infrastructure/SeedSpecMappers.cs`** (the `ToEntity` extensions). Its responsibility is now in the factories.

7. **Update `Concertable.B2B.Seed.Infrastructure/SeedData.cs`:**
   - Ctor parameter `B2BSeedFixture fixture` → `SeedCatalog catalog`
   - Three projection lines as in §4.5 — call factories directly with unpacked spec fields
   - All `fixture.X` → `catalog.X`
   - Add `using` for `Concertable.B2B.Seed.Infrastructure.Factories`

8. **Update consuming code outside the two renamed projects:**
   - `Concertable.B2B.Seeding.Simulator/SeedEventPublishingService.cs` — `B2BSeedFixture` → `SeedCatalog`, ctor parameter rename
   - `Concertable.B2B.Seeding.Simulator/Program.cs` — DI registration rename
   - `Concertable.B2B.Web/Program.cs` — DI registration, `using` updates
   - `Concertable.Customer.Web/Program.cs` — DI registration, `using` updates
   - `Concertable.Customer.Concert.Infrastructure/Data/ConcertProjectionHealthCheck.cs` — type swap
   - `Concertable.B2B.E2ETests/AppFixture.cs` — DI registration, `using` updates
   - `Concertable.Customer.E2ETests/AppFixture.cs` — DI registration, property rename `B2BSeed` → `Catalog`, `using` updates
   - `Concertable.Customer.E2ETests/Payments/TicketPurchaseTests.cs` — `fixture.B2BSeed` → `fixture.Catalog`

9. **Update `Concertable.B2B.Seeding.Simulator/CLAUDE.md`** — replace all `B2BSeedFixture` references with `SeedCatalog`, replace project name `Concertable.B2B.Seeding.Fixture` with `Concertable.B2B.Seed.Contracts`, update the file layout section to reflect the factory split.

10. **Build verify** each affected project sequentially (B2B.Web, Customer.Web, Simulator, both E2E test projects) before running tests.

### Phase 3 — Customer / Payment seed renames (third commit)

12. **Rename `Concertable.Customer.Seeding` → `Concertable.Customer.Seed`**
    - Folder rename, csproj rename
    - Update namespace in `SeedData.cs` from `Concertable.Customer.Seeding` to `Concertable.Customer.Seed`
    - Update every `ProjectReference` (Customer.Web, Customer.E2ETests, etc.)
    - Update `using Concertable.Customer.Seeding;` directives across the codebase

13. **Rename `Concertable.Payment.Seeding` → `Concertable.Payment.Seed`**
    - Same procedure as Customer; audit `SeedData` content first to confirm there's no equivalent of the B2B catalog/composition split needed
    - If Payment.Seeding has a single SeedData class, treat it like Customer (flat `.Seed`)

14. **Rename `Concertable.B2B.Seeding.Simulator` → `Concertable.B2B.Seed.Simulator`**
    - Folder rename, csproj rename
    - Update `Concertable.Customer.AppHost`'s `AddB2BSeedingSimulator<Projects.Concertable_B2B_Seeding_Simulator>(...)` reference (the generated `Projects.X` type matches the project filename)
    - Update CLAUDE.md

### Verification

15. **Build verify** the whole solution.

16. **Run regress** (`./e2e.ps1 regress`) to confirm no regression.

---

## 8. Verification

- All affected projects build with zero errors.
- Customer.Customer.E2ETests / Customer.Web do not reference `Concertable.B2B.Seed.Infrastructure` (boundary intact).
- `./e2e.ps1 regress` passes — all baseline-passing scenarios still pass.
- Spot check: in B2B's seed DB, venue 1 still has the canonical Test County values; in Customer's `[venue].[Venues]` projection, venue 2 still has the Redhill values.

### 8.1 Known flake to investigate, not panic on

The regress run on commit `57a60743` (the prior B2BSeedFixture refactor) failed B2B's **"Venue manager books artist on a door split"** scenario with SQL `0x80131904` TCP drops mid-test at the 30s mark. Other 6/7 B2B + 2/2 Customer scenarios passed cleanly on the first try. A `--no-build` retry against the same compiled DLL passed at 1:53s — confirming infra flake, not a refactor-induced regression.

If the same scenario fails again after this refactor lands, **before assuming the refactor broke something**:

1. Check the failure mode in `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/regress.last.log` — look for `Microsoft.Data.SqlClient.SqlException (0x80131904)` and TCP-drop signatures (`error: 0 - An established connection was aborted...`, `error: 0 - An existing connection was forcibly closed...`).
2. If those signatures match the prior flake, kill any lingering `testhost.exe` processes and retry the single scenario with `dotnet test ... --filter "DisplayName=Venue manager books artist on a door split" --no-build` to test against the existing build.
3. Only if it fails again with the same code-path failure (not infra) is it a real regression to investigate.

The underlying flake (SQL connection drops during Aspire container churn) is orthogonal to this refactor and worth a follow-up issue in its own right.

---

## 9. Out of scope

- Renaming `SeedData` — kept as-is; the catalog rename alone is enough to resolve the prior name-clash.
- Changing wire-event record shapes.
- Touching B2B-internal entities outside Venues/Artists/Concerts (Contracts, Opportunities, Bookings, Applications stay unchanged).
- Customer-side projection handler logic.
- The Customer.Seeding `UpcomingConcertId = 13` const (already unused dead code; leave alone).

---

## 10. Decisions locked in

| Decision | Choice | Rationale |
|---|---|---|
| Project naming | `Seed.Contracts` + `Seed.Infrastructure` | Matches per-module convention (`Venue.Contracts` / `Venue.Infrastructure`). No rhyme. No `Fixture` baggage. |
| Catalog class name | `SeedCatalog` | Self-describes via the codebase's "module name in class name" pattern (cf. `VenueChangedEvent`). Distinct word from `SeedData`. |
| Composition class name | `SeedData` unchanged | Established; renaming would touch many files for cosmetic gain; the catalog rename already resolves the prior clash. |
| Entity-side factory shape | Static class, primitive parameters | Matches `ApplicationFactory.Create(int artistId, int opportunityId)`. The factory doesn't know about specs — `SeedData` unpacks at the call site. |
| Event-side mapping | Static extension method on the spec | Pure record construction — no factory work to encapsulate. Asymmetry with entity side is honest. |
| Where factories live | `Concertable.B2B.Seed.Infrastructure/Factories/` | B2B-internal because they reference Domain entities. Sub-folder matches other modules' `Domain/Factories/` placement. |
| Spec records location | `Concertable.B2B.Seed.Contracts/Specs/` | Sub-folder under Contracts; namespace `Concertable.B2B.Seed.Contracts.Specs`. |
