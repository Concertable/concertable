# B2B Seed Spec Refactor

Replace `Concertable.B2B.Seeding.Fixture`'s `XChangedEvent` literal lists with `XSeedSpec` types that convert to both the wire event and the B2B Domain entity. Pure-literal seed data; no private compression scaffolding; honest naming.

This document is self-contained — it explains the architecture context, the problems with the current shape, the design alternatives considered, why each decision was made, and the exact implementation order. Read top to bottom; no prior conversation context required.

---

## 1. Architecture context

Concertable is a multi-microservice system. The relevant projects:

| Project | Role |
|---|---|
| `Concertable.B2B.Venue.Contracts` (and `.Artist`, `.Concert`) | Cross-service wire types: integration events (`VenueChangedEvent`) and DTOs. Visible to everyone. **Cannot reference B2B Domain.** |
| `Concertable.B2B.<Module>.Domain` | B2B-internal entities (`VenueEntity`, `ArtistEntity`, etc.). Visible only to B2B. |
| `Concertable.B2B.Seeding` | **B2B-internal seed code.** `SeedData.cs`, fakers, factories. Pulls in B2B Domain. Only B2B references it. |
| `Concertable.B2B.Seeding.Fixture` | **Shared seed data.** Contracts-only — no Domain, no EF, no fakers. Ships as a private NuGet. Referenced by B2B's seeding, the simulator, and Customer E2E tests. |
| `Concertable.B2B.Seeding.Simulator` | A Worker host registered in `Concertable.Customer.AppHost` only. Publishes B2B's full seed event set on startup so Customer's projection handlers populate when real B2B isn't running. |
| `Concertable.Customer.E2ETests` | Customer's integration tests. References the fixture to know what seed data exists. |

**Naming subtlety.** `.Seeding` and `.Seeding.Fixture` both have "Seeding" in the name but do different things:
- `.Seeding` = B2B-only behaviour (fakers, factories, the master `SeedData.cs`).
- `.Seeding.Fixture` = shareable data only. Must not pull in B2B Domain or EF.

The boundary exists because Customer cannot have a build-time dependency on B2B's source. Any shared seed data must therefore live in a contracts-only place.

### How production publishes events

In production:
1. A user action (e.g., a venue manager registers via the B2B UI) causes B2B to create a `VenueEntity`.
2. The entity raises a `VenueChangedDomainEvent`.
3. B2B's outbox publishes `VenueChangedEvent` (the integration event) to the bus.
4. Customer's `VenueProjectionHandler` receives the event and writes a `VenueReadModel`.

Entity-first, event-derived. Natural direction.

### How seeding currently bridges the boundary

At seed time we want the same outcome (Customer's projection populated with seed venues) but we can't run real registration flows. The obvious "have B2B's seeder hand-write `VenueEntity` literals" doesn't work because Customer / the simulator cannot reference `VenueEntity`. So the seed data has to be expressed in something both sides can see — the cross-service event contract.

Today this means the fixture exports `IReadOnlyList<VenueChangedEvent>` literals. B2B's `SeedData.cs` projects them into entities via fakers; the simulator publishes them to the bus verbatim.

This inverts the production direction. At seed time, the event is the source; the entity is derived. The current shape works, but the naming lies about the role — those literals are seed data wearing event clothing. Nothing raised them.

---

## 2. Why the current fixture is dodgy

The current `B2BSeedFixture.cs` has three problems beyond the naming.

### 2.1 Pretends seed data is events

Literal `new VenueChangedEvent(...)` entries with no entity behind them. The wire-event type does double duty as a seed carrier. Mental model is constantly fighting reality: "this is an event that wasn't raised by anything."

### 2.2 Compresses via private scaffolding

The file has:
- 5 private record types (`LocationData`, `VenueData`, `BandData`, `OppSpec`, `ConcertSpec`) acting as row formats.
- 4 data arrays (`Locations[15]`, `Bands[35]`, `VenueRows[34]`, `Opportunities[67]`).
- 2 magic index sets (`VenueHireOppIndices = [9, 15, 20, 27, 36, 42, 47, 51, 58, 62, 65]`, `FiveHourOppIndices = [31]`) — to add a `VenueHire` concert you edit a `ConcertSpec` row AND add its index to a `HashSet<int>` 100 lines away.
- 3 `Build*` methods that loop and synthesise events from rows, with index-cycling location assignment (`Locations[locIndex++ % Locations.Length]`).

The cycling is the worst offender. Venue 2 ("Redhill Hall") ends up "in Birmingham" because `locIndex = Bands.Length = 35` and `35 % 15 == 5` and `Locations[5]` happens to be Birmingham. Nobody chose that mapping. Adding a city to `Locations` silently mutates every seed venue/artist town.

There are also hidden duplicate-venue concerts — `ConcertRows[14..17]` all reference `OppIndex=0` so all four start at Venue 1 on the same day, but you can't see it from the row.

### 2.3 Contradicts its own docs

`Concertable.B2B.Seeding.Simulator/CLAUDE.md` claims the fixture is "35 explicit literals, one per seed venue / artist" with "no helpers, no index lookups." The actual file does exactly what the doc forbids. Either the doc or the code is wrong. This refactor makes the code match the doc.

---

## 3. Design alternatives considered

### 3.1 Status quo (private compression specs + Build methods)

Today's shape. ~320 lines via compression. Rejected because of §2.

### 3.2 Fully literal `XChangedEvent` lists

Replace the compression with explicit `new VenueChangedEvent(VenueId: 2, Name: "Redhill Hall", …)` per row. Solves §2.2 and §2.3. Doesn't solve §2.1 — still calls seed data "ChangedEvents."

### 3.3 `XSeedSpec` types (chosen)

A new shared type per cross-service entity. Lives in Contracts alongside the matching event. Carries the same fields. Converts to the event via `.ToChangedEvent()` on the type. Converts to the B2B entity via `.ToEntity()` extension method in B2B's seeding project.

Solves all three problems. Costs: 3 new tiny types + 3 new extension classes. Benefit: naming matches reality — the fixture exports "seed specs," not "events nobody raised."

### 3.4 Why not just delete the seed and let real users register?

Kills dev experience and E2E tests. The simulator exists precisely so Customer can run standalone without B2B.

### 3.5 Why not move seed data into a shared kernel both sides reference?

That kernel would have to contain `VenueEntity` for B2B to use it, which reintroduces the coupling the microservice split exists to remove. Customer would transitively depend on B2B Domain. Architecturally wrong.

### 3.6 Why not codegen events from entity literals?

Adds tooling for a problem 30 lines of test code already solves (see §8). Not worth it.

---

## 4. The chosen design

### 4.1 `XSeedSpec` types

One per cross-service entity. Lives in the matching contracts project (`Concertable.B2B.Venue.Contracts/Seed/VenueSeedSpec.cs`, etc.). Record with init-only properties. Single conversion method `ToChangedEvent()`.

```csharp
// Concertable.B2B.Venue.Contracts/Seed/VenueSeedSpec.cs
public sealed record VenueSeedSpec
{
    public required int VenueId { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string About { get; init; }
    public required string Avatar { get; init; }
    public required string BannerUrl { get; init; }
    public required string County { get; init; }
    public required string Town { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required string Email { get; init; }

    public VenueChangedEvent ToChangedEvent() => new(
        VenueId, UserId, Name, About, Avatar, BannerUrl,
        County, Town, Latitude, Longitude, Email);
}
```

`ArtistSeedSpec` is the same shape plus `Genres`. `ConcertSeedSpec` is the same shape plus `Period`, `DatePosted`, `Avatar?`, `BannerUrl?`, `TotalTickets`, `AvailableTickets`, `Price`, `ArtistId`, `ArtistName`, `VenueId`, `VenueName`, `Latitude`, `Longitude`, `Genres`, `PayeeUserId`.

### 4.2 Entity conversion as B2B-internal extension

`ToEntity()` cannot live on the spec itself — that would require Contracts to reference `Concertable.B2B.<Module>.Domain`, which breaks the boundary. Instead, the entity-side conversion is an extension method in `Concertable.B2B.Seeding/Fakers/`:

```csharp
// Concertable.B2B.Seeding/Fakers/VenueSeedExtensions.cs (B2B-internal)
public static class VenueSeedExtensions
{
    public static VenueEntity ToEntity(this VenueSeedSpec spec) => new()
    {
        Id = spec.VenueId,
        UserId = spec.UserId,
        Name = spec.Name,
        About = spec.About,
        Avatar = spec.Avatar,
        BannerUrl = spec.BannerUrl,
        County = spec.County,
        Town = spec.Town,
        Location = new Point(spec.Longitude, spec.Latitude) { SRID = 4326 },
        Email = spec.Email,
    };
}
```

The asymmetry — `ToChangedEvent()` on the type vs `ToEntity()` as extension — is **architecturally correct**:

```csharp
// In B2B code (sees both):
var entity = spec.ToEntity();          // ✓ extension visible
var @event = spec.ToChangedEvent();    // ✓ method on type

// In Customer / Simulator code (sees only ToChangedEvent):
var @event = spec.ToChangedEvent();    // ✓
var entity = spec.ToEntity();          // ✗ won't compile — extension not visible
```

Customer should not be able to construct a B2B entity. The compiler enforces the boundary; the call-site syntax mirrors the architecture.

Existing `Concertable.B2B.Seeding/Fakers/VenueFaker.cs`, `ArtistFaker.cs`, `ConcertFaker.cs` (with `FromSeedFixture` methods) are deleted — replaced by the extensions.

### 4.3 Fixture becomes a DI singleton with `TimeProvider`

Today the fixture exposes static lists and a `Concerts(DateTime now)` method that callers pass `now` into. Two problems:
- Ugly to thread `DateTime` through every call site.
- Easy to mess up within-process consistency — if two callers pass different `now` values, their concerts disagree on dates.

Fix: make the fixture a singleton. Inject `TimeProvider`. Capture `now` once in the constructor. All accesses use that single captured anchor. No `DateTime` parameter anywhere.

```csharp
// B2BSeedFixture.cs (shell)
namespace Concertable.B2B.Seeding.Fixture;

public sealed partial class B2BSeedFixture
{
    private readonly DateTime now;

    public B2BSeedFixture(TimeProvider timeProvider)
    {
        this.now = timeProvider.GetUtcNow().UtcDateTime;
    }
}
```

**Why singleton + TimeProvider works for time sync:**

- **Within one process** (the actual concern): constructor captures `now` once; every property access serves the same coherent snapshot. All 47 concerts derive from the same `now`. The simulator currently does this manually (`var now = DateTime.UtcNow;` at the top of `ExecuteAsync`); singleton makes it automatic.
- **Cross-process** is not a concern in this codebase. Two reasons:
  1. Real B2B (umbrella `Concertable.AppHost`) and the simulator (standalone `Concertable.Customer.AppHost`) are **mutually exclusive consumers** — only one runs at a time. The simulator is explicitly NOT registered in the umbrella per its own CLAUDE.md.
  2. Customer's E2E tests only read `B2BSeedFixture.UpcomingConcertId` and never assert on `DatePosted` / `Period.Start` (verified in `TicketPurchaseTests.cs:22,30` and `AppFixture.cs:201`). No test compares a concert's date across processes.

If a future test ever asserts on exact dates across processes, the fix is to pass a coordinated `DateTime` rather than `TimeProvider.System` into the singleton. Not needed today.

### 4.4 Fixture split across three partial files

Fully literal lists run ~500–900 lines per entity type. Single file would be ~2000 lines and miserable to navigate. Split:

```
B2BSeedFixture.cs            shell (ctor + this.now)
B2BSeedFixture.Venues.cs     public IReadOnlyList<VenueSeedSpec> Venues { get; } = [literals]
B2BSeedFixture.Artists.cs    public IReadOnlyList<ArtistSeedSpec> Artists { get; } = [literals]
B2BSeedFixture.Concerts.cs   public IReadOnlyList<ConcertSeedSpec> Concerts (lazy-cached, uses this.now)
```

Consumers see one type with three properties. Each file is one entity type's literals.

### 4.5 Delete `UpcomingConcertId` const

Currently the fixture exposes `public const int UpcomingConcertId = 13` plus a helper `UpcomingConcert(now)`. This is a label, not data. It exists so tests can identify "the upcoming concert" by ID.

Concert 13's `Name` is literally `"Upcoming FlatFee Show"`. Tests can filter intrinsically:

```csharp
fixture.Concerts.First(c => c.Name == "Upcoming FlatFee Show")
```

The const goes away. The fixture becomes 100% data, zero labels. Test labels live in the tests.

---

## 5. File layout

```
Concertable.B2B.Venue.Contracts/
  Events/VenueChangedEvent.cs            (existing, unchanged)
  Seed/VenueSeedSpec.cs                  NEW

Concertable.B2B.Artist.Contracts/
  Events/ArtistChangedEvent.cs           (existing, unchanged)
  Seed/ArtistSeedSpec.cs                 NEW

Concertable.B2B.Concert.Contracts/
  Events/ConcertChangedEvent.cs          (existing, unchanged)
  Seed/ConcertSeedSpec.cs                NEW

Concertable.B2B.Seeding.Fixture/         (shareable NuGet, contracts-only)
  B2BSeedFixture.cs                      shell — ctor, `now` field
  B2BSeedFixture.Venues.cs               IReadOnlyList<VenueSeedSpec>
  B2BSeedFixture.Artists.cs              IReadOnlyList<ArtistSeedSpec>
  B2BSeedFixture.Concerts.cs             IReadOnlyList<ConcertSeedSpec>

Concertable.B2B.Seeding/                 (B2B-internal)
  Fakers/VenueSeedExtensions.cs          NEW — .ToEntity()
  Fakers/ArtistSeedExtensions.cs         NEW — .ToEntity()
  Fakers/ConcertSeedExtensions.cs        NEW — .ToEntity() (takes extra bookingId)
  Fakers/VenueFaker.cs                   DELETE
  Fakers/ArtistFaker.cs                  DELETE
  Fakers/ConcertFaker.cs                 DELETE
  SeedData.cs                            update three projection lines + take fixture via ctor
```

The Fixture project deletes everything currently inside `B2BSeedFixture.cs` — every private record, every array, every magic index set, every Build method, every helper (`EmailFromVenueName`, `EmailFromArtistName`, `BuildVenue1`, etc.). What remains is shell + three partials of literals.

---

## 6. Why this isn't duplication

A natural concern: doesn't this "duplicate" data between the seed spec and the B2B entity?

It does not. There's one source (the seed spec literals in the fixture) and two derivations:

```
VenueSeedSpec (canonical)
        │
        ├──► spec.ToEntity()        ──► VenueEntity     (B2B's seed DB)
        │
        └──► spec.ToChangedEvent()  ──► published event ──► Customer projection handler ──► VenueReadModel (Customer's seed DB)
```

Changing Venue 2's town to "Croydon":
1. Edit one literal in `B2BSeedFixture.Venues.cs`.
2. Next seed run, `VenueSeedExtensions.ToEntity` reads the new value → B2B's `VenueEntity` has Croydon.
3. Simulator publishes the new event → Customer's `VenueReadModel` has Croydon.

One edit. Both consumers update. **B2B's `SeedData.cs` is not touched.** It cannot be touched for venue value changes — `SeedData.Venues` is computed (`[.. fixture.Venues.Select(s => s.ToEntity())]`), not declared. There's no `new VenueEntity { Name = "Redhill Hall", ... }` literal anywhere in SeedData. Grep for `"Redhill Hall"` and you find one hit: the fixture.

The mapping function (`ToEntity()`) is the only place the spec→entity field correspondence is encoded. That's three small extension classes, one method each. A roundtrip test (§8) pins drift.

---

## 7. SeedData's role after the refactor

`Concertable.B2B.Seeding/SeedData.cs` becomes a focused **composition root** for B2B's seed graph:

- **Projects fixture data into entities** for the three event-bearing types:
  ```csharp
  Venues   = [.. fixture.Venues.Select(s => s.ToEntity())];
  Artists  = [.. fixture.Artists.Select(s => s.ToEntity())];
  Concerts = [.. fixture.Concerts.Select(s => s.ToEntity(bookingId: Bookings[s.ConcertId - 1].Id))];
  ```
- **Holds B2B-internal entities as literals** — `Contracts`, `Opportunities`, `Bookings`, `Applications`, user entities. These are not published to any other service, so they live here. The current shape stays.
- **Wires cross-references** between fixture-sourced things and B2B-internal things (e.g., a `Booking` references a `Concert.ConcertId` that came from the fixture; an `Application` references both).
- **Exposes named shortcuts** — `FlatFeeApp`, `ConfirmedBooking`, `VenueManager1`, etc. — single-instance handles to specific test scenarios.

---

## 8. Consumer updates

### 8.1 DI registration per consumer

Each host that constructs `B2BSeedFixture` registers:
```csharp
services.AddSingleton(TimeProvider.System);
services.AddSingleton<B2BSeedFixture>();
```

Hosts that need this: B2B's host(s) that run `SeedData`, `Concertable.B2B.Seeding.Simulator`, `Concertable.Customer.E2ETests` (via `AppFixture`).

### 8.2 `Concertable.B2B.Seeding/SeedData.cs`

```csharp
// Before
Artists  = [.. B2BSeedFixture.Artists.Select(ArtistFaker.FromSeedFixture)];
Venues   = [.. B2BSeedFixture.Venues.Select(VenueFaker.FromSeedFixture)];
Concerts = [.. B2BSeedFixture.Concerts(now)
    .Select(e => ConcertFaker.FromSeedFixture(e, bookingId: Bookings[e.ConcertId - 1].Id))];

// After (B2BSeedFixture is now injected via ctor)
Artists  = [.. fixture.Artists.Select(s => s.ToEntity())];
Venues   = [.. fixture.Venues.Select(s => s.ToEntity())];
Concerts = [.. fixture.Concerts.Select(s => s.ToEntity(bookingId: Bookings[s.ConcertId - 1].Id))];
```

`ConcertSeedExtensions.ToEntity` takes an extra `bookingId` parameter — the only piece of B2B-internal context the spec can't carry (since `Booking` is B2B-internal and unknown to Customer).

### 8.3 `Concertable.B2B.Seeding.Simulator/SeedEventPublishingService.cs`

```csharp
// Before
var now = DateTime.UtcNow;
foreach (var v in B2BSeedFixture.Venues)
    await transport.PublishAsync(v, Envelope(...), stoppingToken);
foreach (var a in B2BSeedFixture.Artists)
    await transport.PublishAsync(a, Envelope(...), stoppingToken);
var concerts = B2BSeedFixture.Concerts(now);
foreach (var c in concerts)
    await transport.PublishAsync(c, Envelope(...), stoppingToken);

// After (B2BSeedFixture injected)
foreach (var v in fixture.Venues)
    await transport.PublishAsync(v.ToChangedEvent(), Envelope(...), stoppingToken);
foreach (var a in fixture.Artists)
    await transport.PublishAsync(a.ToChangedEvent(), Envelope(...), stoppingToken);
foreach (var c in fixture.Concerts)
    await transport.PublishAsync(c.ToChangedEvent(), Envelope(...), stoppingToken);
```

The local `var now = DateTime.UtcNow;` line goes — `now` is captured inside the singleton at construction.

### 8.4 Customer E2E tests

`AppFixture` registers `B2BSeedFixture` as a singleton and exposes it (e.g., as `AppFixture.B2BSeed`) so tests can filter.

```csharp
// TicketPurchaseTests.cs:22
// Before
ConcertId = B2BSeedFixture.UpcomingConcertId,
// After
ConcertId = fixture.B2BSeed.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId,

// TicketPurchaseTests.cs:30
// Before
tickets => tickets is not null && tickets.Any(t => t.Concert.Id == B2BSeedFixture.UpcomingConcertId),
// After
tickets => tickets is not null && tickets.Any(t => t.Concert.Id == fixture.B2BSeed.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId),

// AppFixture.cs:201 (WaitForSeedProjectionAsync)
// Before
cmd.Parameters.AddWithValue("@id", B2BSeedFixture.UpcomingConcertId);
// After
var seedConcertId = b2bFixture.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId;
cmd.Parameters.AddWithValue("@id", seedConcertId);
```

---

## 9. Decisions locked in

| Decision | Choice | Rationale |
|---|---|---|
| Spec naming | `VenueSeedSpec` / `ArtistSeedSpec` / `ConcertSeedSpec` | "Spec" suffix is fine here because the role is different from the rejected private compression specs — these are top-level, explicit, single-shape-per-entity contracts. |
| Wire-event conversion | Instance method `ToChangedEvent()` on the spec | Contract-side conversion lives on the type. |
| Entity conversion | Extension method `ToEntity()` in `Concertable.B2B.Seeding/Fakers/` | B2B Domain cannot leak into Contracts. Extension is the cleanest way to keep call-site syntax symmetric (`spec.ToEntity()` reads like `spec.ToChangedEvent()`). |
| Fixture lifetime | Singleton, `TimeProvider` injected, `now` captured in ctor | Within-process consistency is automatic; eliminates the ugly `DateTime now` parameter at call sites; cross-process drift is not a concern in this codebase. |
| Fixture file split | Three partials (`Venues.cs`, `Artists.cs`, `Concerts.cs`) + shell | Single file would be ~2000 lines. |
| `Concerts` shape | Lazy-cached getter (`this.concerts ??= [...]`) | Auto-property initialisers cannot reference `this.now`. Lazy cache builds the 47 concert literals once on first access. |
| `UpcomingConcertId` const | DELETE | It's a label, not data. Tests filter by `Name` instead. Fixture becomes 100% data. |
| Locations | Real-world per named venue | The accidental cycling that put Redhill Hall in Birmingham is fixed as part of going literal. Redhill Hall → Surrey/Redhill, Camden Electric Ballroom → Greater London/London, Manchester Night & Day Café → Greater Manchester/Manchester, etc. |
| Emails | Preserved byte-identical | Venue 1 = `SeedUsers.VenueManagerEmail(1)`; the other 34 venues = `"{slug}@test.com"` derived from name. Inconsistency is real but out of scope for this refactor. |
| Duplicate-venue concerts | Preserved | Several concerts in the current data resolve to the same `(VenueId, start time)` pair — e.g., `ConcertRows[14..17]` all at Venue 1, now−60d. Whether intentional or not, preserve to avoid breaking tests. Fix in a focused follow-up if desired. |
| Existing `XChangedEvent` records | Unchanged | Stay as positional records in their contracts projects. No wire change. |
| Old `XFaker.FromSeedFixture` methods | Deleted | Replaced by `.ToEntity()` extensions. |

---

## 10. Out of scope

- Changing the wire-event record shape. Events stay as positional records.
- Customer / Search projection handler logic.
- B2B's internal seed data outside Venues / Artists / Concerts. `Contracts`, `Opportunities`, `Bookings`, `Applications`, user entities stay as today.
- The `Venue 1` email inconsistency (mix of `SeedUsers.VenueManagerEmail(1)` and name-derived emails).
- Whether duplicate-venue concerts should be spread.
- B2B's `CredentialRegisteredHandler` user-creation flow.
- Cross-process date sync (not a concern; see §4.3).

---

## 11. Implementation order

1. **Add the three `XSeedSpec` records** in their contracts projects (`Concertable.B2B.Venue.Contracts/Seed/VenueSeedSpec.cs`, etc.).
2. **Add the three `XSeedExtensions` classes** in `Concertable.B2B.Seeding/Fakers/`. Body of each `ToEntity()` mirrors the existing `FromSeedFixture` in the old fakers.
3. **Rewrite `B2BSeedFixture.cs`** — shell + three partial files. Replace every existing private record, array, magic index set, helper, and Build method with literal `XSeedSpec` lists (35 venues + 35 artists + 47 concerts). Real-world locations per named venue. Emails preserved (Venue 1 uses `SeedUsers.VenueManagerEmail(1)`; the rest use `"{slug}@test.com"`). Singleton with `TimeProvider`; `Concerts` is a lazy-cached getter using `this.now`.
4. **Update `Concertable.B2B.Seeding/SeedData.cs`** — three projection lines (`Artists`, `Venues`, `Concerts`); ctor takes `B2BSeedFixture`.
5. **Update `Concertable.B2B.Seeding.Simulator/SeedEventPublishingService.cs`** — inject `B2BSeedFixture`; iterate `.ToChangedEvent()`; drop the local `var now = DateTime.UtcNow;`.
6. **Update `Concertable.Customer.E2ETests/AppFixture.cs`** — register `B2BSeedFixture` singleton; expose it (e.g., `B2BSeed`); swap `UpcomingConcertId` references in `WaitForSeedProjectionAsync`.
7. **Update `Concertable.Customer.E2ETests/Payments/TicketPurchaseTests.cs`** — swap the two `UpcomingConcertId` references for `Name`-filtered lookups via `fixture.B2BSeed.Concerts`.
8. **Delete** the three old `XFaker.FromSeedFixture` methods (whole files: `VenueFaker.cs`, `ArtistFaker.cs`, `ConcertFaker.cs` in `Concertable.B2B.Seeding/Fakers/`).
9. **Add DI registrations** (`AddSingleton<B2BSeedFixture>` + `AddSingleton(TimeProvider.System)`) in each consuming host.
10. **Update `Concertable.B2B.Seeding.Simulator/CLAUDE.md`** — the existing claim about "explicit literals, no helpers, no index lookups" finally matches the code; update any stale references to `XChangedEvent` lists or `UpcomingConcertId`.

---

## 12. Roundtrip test (recommended follow-up, not required by this refactor)

The fixture being the single source means B2B's entity is faithful by construction — but a regression test pins it explicitly. One test per entity type:

```csharp
[Fact]
public void VenueSeed_RoundTrip_Through_Entity_Matches_Spec()
{
    var fixture = new B2BSeedFixture(TimeProvider.System);
    foreach (var spec in fixture.Venues)
    {
        var entity = spec.ToEntity();
        var roundtripped = entity.ToChangedEvent();  // or whatever B2B's prod path is
        roundtripped.Should().BeEquivalentTo(spec.ToChangedEvent());
    }
}
```

Catches drift on:
- Add-a-field to the event without updating the spec, the literal, or the extension.
- Extension method stops mapping a field.
- B2B's production projection diverges from the seed projection.

Lives in `Concertable.B2B.Seeding.Tests/` or equivalent. Not strictly required to ship this refactor, but cheap insurance against the spec→entity field correspondence quietly rotting.


## Verifying the change

Run `./e2e.ps1 regress` (or invoke the `e2e-ui-regress` skill -- "regress the tests" / "is it still green") after each substantive edit. It runs only the baseline-passing scenarios from `api/Tests/Concertable.E2ETests/E2E_BASELINE.md` (~3-6 min) and fails fast if any previously-passing scenario regresses. For full discovery (newly-passing or newly-failing scenarios), use the `e2e-ui-debug` skill (~25-30 min).
