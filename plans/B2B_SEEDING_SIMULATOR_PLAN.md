# Customer-standalone projection data via a B2B seeding simulator (+ `api/Seeding/` reorg)

## Context

`Concertable.Customer.AppHost` runs Customer microservice standalone — Auth, Customer.Web, Search, Payment, SPAs — but **not** B2B (because Customer is a separate microservice, not a piece of a monolith; see root `ARCHITECTURE.md`). Customer's projection tables (`[concert].[Concerts]`, `[venue].[Venues]`, `[artist].[Artists]`) and Search's equivalents are populated by `IIntegrationEventHandler<XChangedEvent>` reacting to events real B2B publishes. With no B2B running, those tables stay empty.

This breaks two things in priority order:

1. **Dev experience.** A developer running `Concertable.Customer.AppHost` for manual iteration on Customer expects the Customer SPA to look like production — 35 venues, 35 artists, 47 concerts to browse. Empty projections = empty SPA = useless dev environment.
2. **Customer E2E suite.** Tests that need to find a concert, buy a ticket, leave a review, etc. all fail when the projection is empty.

Today's workaround is `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/ProjectionSeeder.cs`, which fakes three events from inside the E2E AppFixture. It's a test-only hack: doesn't help dev at all, forces the test fixture to register an ASB transport, and lives in the wrong layer.

Convention (`api/docs/SEEDING_CONVENTIONS.md`): **never seed projection data directly.** Read-model rows must come from the same event flow that runs in production. If the producing service isn't there, the fix is to stand up a publisher — never to insert read-model rows.

Two intertwined problems:

1. **No B2B → no events → no Customer projection data.** Solved by a `Concertable.B2B.Seeding.Simulator` Worker host that publishes B2B's full seed event set (~117 `XChangedEvent`s) on startup. Source of truth for those events is a new `Concertable.B2B.Seeding.Fixture` project that both B2B's own seeders and the simulator derive from — byte-for-byte sync, no drift.
2. **`api/Seeding/` is the wrong layout.** It bundles ownership-mixed projects: B2B-owned `Concertable.B2B.Seeding`, Customer-owned `Concertable.Customer.Seeding`, shared infra (`Concertable.Seeding.Shared`, `Concertable.Seeding.Identity`, `Concertable.Seeding.Infrastructure`), and two empty placeholders (`Concertable.Seeding`, `Concertable.Seeding.WellKnown`). The folder having a mix is what misled me into thinking the new simulator was just another `Seeding` project. Fixed by moving each project under its owner before adding new ones.

After the reorg `api/Seeding/` ceases to exist. Service-owned seeding lives under each service folder; shared seeding infra lives under `api/Shared/`.

## Per-AppHost behaviour (this is the actual contract)

Concertable has three Aspire AppHosts a dev can start. The seeding design must produce the same observable state from any of them. Same canonical data, same projected rows, regardless of entry point. Different services running, same data shape.

### 1. `Concertable.B2B.AppHost` — B2B standalone

What runs: B2B.Web, B2B.Workers, Auth, Payment, B2B SPAs.

What's seeded:
- B2B.Web runs its own `DevDbInitializer`, which runs B2B's `IDevSeeder`s (`VenueDevSeeder`, `ArtistDevSeeder`, `ConcertDevSeeder`, etc.). These read from `Concertable.B2B.Seeding.SeedData` and persist to B2B's DB.
- B2B's domain events fire as a side effect of seeding → outbox → ASB → in-process subscribers (B2B's own modules).
- Customer isn't running, so integration events published on ASB (`VenueChangedEvent` etc.) have no downstream consumer. That's fine — B2B doesn't depend on Customer existing, exactly like Customer doesn't depend on B2B existing.

The simulator is **not registered** here. B2B is the real producer — itself.

### 2. `Concertable.Customer.AppHost` — Customer standalone

What runs: Customer.Web, Auth, Search.Web, Search.Workers, Payment.Web, Payment.Workers, Customer SPAs.

What's seeded:
- B2B is **not running**. Without intervention, Customer/Search projection tables stay empty.
- The simulator (`Concertable.B2B.Seeding.Simulator`) is registered as an Aspire resource. It publishes the full ~117-event B2B seed set on startup and exits.
- Customer's existing `XProjectionHandler`s consume the events through the real ASB pipeline, upsert their projections.
- Customer's projections end up with the same rows that real B2B would have produced under the umbrella, because the simulator and real B2B both derive their events from `Concertable.B2B.Seeding.Fixture`. Byte-identical.

### 3. `Concertable.AppHost` (umbrella) — everything

What runs: every service from both AppHosts above, plus anything else.

What's seeded:
- Real B2B.Web runs, real seeders fire, real events published. Customer/Search projection handlers consume them as in production.
- The simulator is **not registered**. Double-publishing would be wasted cycles (consumers are idempotent so no corruption, but log noise + confusion).

### Invariant: same data from any entry point

Pick any of the three AppHosts → start it → query the resulting Customer DB (where Customer is running) and B2B DB (where B2B is running). The shared entities — venues 1–35, artists 1–35, concerts 1–47 — have identical rows. That's the contract: regardless of producer, downstream sees the same projection.

Enforced by the single-source-of-truth fixture: every venue/artist/concert event field is in `B2BSeedFixture`. Both real B2B and the simulator project from it via the same `FromSeedFixture` factories. Drift is impossible if the rule "no entity field is set outside the fixture for entities in the fixture" is upheld.

### Where simulator registration lives

Per-AppHost simulator registration:

| AppHost | Registers simulator? |
|---|---|
| `Concertable.AppHost` (umbrella) | No — real B2B publishes |
| `Concertable.Customer.AppHost` | **Yes** — Customer is the standalone consumer |
| `Concertable.B2B.AppHost` | No — B2B IS the producer |

If a future standalone consumer AppHost is added (e.g., a hypothetical `Concertable.Search.AppHost` that runs Search without B2B), it would register the simulator too. Same pattern.

## Cross-repo strategy

In the split-repo future:

- **Event contracts** (`Concertable.B2B.X.Contracts`) ship as private NuGet packages from B2B's repo (GitHub Packages / Azure Artifacts). Customer's csproj turns `ProjectReference` into `PackageReference`. Semver-owned by B2B.
- **B2B's seed fixture** (the small project holding the canonical event records) also ships from B2B's repo as a tiny private NuGet package. Customer.E2ETests references it for assertions.
- **The simulator itself** ships as a container image from B2B's repo (`ghcr.io/<org>/concertable-b2b-seeding-simulator:vX.Y.Z`). Customer.AppHost switches from `AddProject<Projects.X>()` to `builder.AddContainer(...)`. Container is the cleanest answer because Aspire's `AddProject<>` needs source in the build graph, which isn't available cross-repo.
- **Shared seeding infrastructure** (`Concertable.Seeding.*` interfaces, scope, EF interceptors, logging) ships from a shared/Kernel-style repo as NuGet packages consumed by every service.

Same C# changes today and tomorrow — only the csproj reference type changes. The ownership-based folder layout previews the split.

## Design decision: the simulator does NOT depend on `Concertable.B2B.Seeding`

`Concertable.B2B.Seeding.SeedData` holds B2B's internal Domain entities (`VenueEntity`, `ArtistEntity`, `ConcertEntity`). Importing it would transitively pull `Concertable.B2B.Venue.Domain`, `.Artist.Domain`, `.Concert.Domain` into the simulator — crossing a service boundary the simulator should never cross. In a split-repo world those Domain projects are private to B2B's repo and never published as packages.

The simulator's job is to look like B2B on the wire — publish well-shaped `XChangedEvent` records. That's a contract-level concern, not a Domain-level one. So the simulator references only:

- `Concertable.B2B.Venue.Contracts`
- `Concertable.B2B.Artist.Contracts`
- `Concertable.B2B.Concert.Contracts`
- `Concertable.B2B.Seeding.Fixture` (the new fixture)
- `Concertable.Messaging.AzureServiceBus`
- `Concertable.Messaging.Contracts`
- `Concertable.ServiceDefaults`

## Plan

### Phase 0 — revert this session's bad work

I previously violated the projection-seeding convention by adding direct-EF projection seeders. Files to restore to pre-session state:

- `api/Seeding/Concertable.Customer.Seeding/SeedData.cs` — drop the `VenueReadModel` / `ArtistReadModel` / `ConcertReadModel` properties; restore to the original shape (`SeedCustomer Customer`, `IReadOnlyList<Guid> CustomerIds`, `TestPassword`, `UpcomingConcertId`).
- `api/Seeding/Concertable.Customer.Seeding/Concertable.Customer.Seeding.csproj` — drop the three Customer Domain `ProjectReference`s I added.
- `api/Concertable.Customer/Modules/Venue/.../Concertable.Customer.Venue.Infrastructure.csproj` and `.../Artist/.../Concertable.Customer.Artist.Infrastructure.csproj` — drop the Seeding `ProjectReference`s I added.
- Delete the three bad seeders: `Venue.Infrastructure/Data/Seeders/VenueDevSeeder.cs`, `Artist.Infrastructure/Data/Seeders/ArtistDevSeeder.cs`, `Concert.Infrastructure/Data/Seeders/ConcertDevSeeder.cs`.
- `Concertable.Customer.Venue.Infrastructure.Extensions.ServiceCollectionExtensions` and the two siblings — drop `AddCustomerVenueDevSeeder` / `AddCustomerArtistDevSeeder` / `AddCustomerConcertDevSeeder`.
- `api/Concertable.Customer/Concertable.Customer.Web/Program.cs` — drop the three `services.AddCustomerXDevSeeder()` calls.
- `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/AppFixture.cs` — restore the `AddAzureServiceBusTransport(...)` block and the `await new ProjectionSeeder(host, Polling).SeedAsync();` calls.
- `git restore` of the deleted `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/ProjectionSeeder.cs`.

### Phase 1 — convention enforcement docs ✅ DONE

Three docs already written this session — they capture the design so future sessions don't relitigate it.

- ✅ Root `CLAUDE.md` — trimmed to a short reference to `ARCHITECTURE.md`.
- ✅ `ARCHITECTURE.md` (new, root) — microservice premise: services own their runtime, cross-service is Contracts-only, standalone is canonical.
- ✅ `api/docs/SEEDING_CONVENTIONS.md` — top-of-file banner + new "Standalone-service seeding" section linking to the simulator's CLAUDE.md.
- ✅ `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md` — full simulator design (~250 lines, 8 sections: what it is, why it exists, tight-sync principle, fixture details, how to add an entity, what NOT to do, cross-repo distribution, boundary grep commands).

### Phase 2 — `api/Seeding/` reorg

Inventory of `api/Seeding/` today and target home:

| Current path | Contents | Target |
|---|---|---|
| `Concertable.Seeding/` | empty folder, no csproj | **delete** |
| `Concertable.Seeding.WellKnown/` | empty csproj | **delete** (replaced by Phase 3's B2B seed fixture project under B2B) |
| `Concertable.Seeding.Shared/` | `IDbSeeder` / `IDevSeeder` / `ITestSeeder` interfaces, `SeedingScope`, `SeedingIdentityInterceptor`, DI/DbContext extension methods, `SeedIfEmptyAsync`, `Log.cs` | move shared bits to `api/Shared/Concertable.Seeding.Shared/`. Split out `Fakers/LocationFaker.cs` + `Fakers/ILocationFaker.cs` — they're B2B-only, move into `api/Concertable.B2B/Concertable.B2B.Seeding/Fakers/` and make them internal there. |
| `Concertable.Seeding.Identity/` | `SeedUsers`, `SeedCustomers` (Guid identities every service agrees on) | move to `api/Shared/Concertable.Seeding.Identity/`. Truly cross-service — Auth seeds users with these IDs, B2B/Customer/Payment look them up. |
| `Concertable.Seeding.Infrastructure/` | `SeedingDomainEventDispatchInterceptor` | move to `api/Shared/Concertable.Seeding.Infrastructure/`. EF interceptor every service-with-domain-events uses during seeding. |
| `Concertable.B2B.Seeding/` | B2B's `SeedData` (with B2B Domain dependencies) | move to `api/Concertable.B2B/Concertable.B2B.Seeding/`. Owned by B2B. Internally consumes the moved `LocationFaker`. |
| `Concertable.Customer.Seeding/` | Customer's `SeedData` (after Phase 0 revert: `SeedCustomer`, `CustomerIds`, `TestPassword`, `UpcomingConcertId`) | move to `api/Concertable.Customer/Concertable.Customer.Seeding/`. Drop `UpcomingConcertId` (Phase 3 — B2B owns the seed concert ID, not Customer). |

After Phase 2:

- `api/Seeding/` folder is deleted.
- `api/Shared/Concertable.Seeding.Shared/`, `api/Shared/Concertable.Seeding.Identity/`, `api/Shared/Concertable.Seeding.Infrastructure/` — three shared seeding-infra projects.
- `api/Concertable.B2B/Concertable.B2B.Seeding/` — B2B's seed data.
- `api/Concertable.Customer/Concertable.Customer.Seeding/` — Customer's seed data.

Csproj `ProjectReference` paths across the codebase need updating wherever they cross folders — likely 25–40 csproj edits but mechanical (find/replace `..\..\..\Seeding\Concertable.Seeding.Shared\` → `..\..\..\Shared\Concertable.Seeding.Shared\` etc.). No namespace changes needed (everything keeps its `Concertable.Seeding[.X]` namespace).

`Concertable.slnx` solution file needs the move-equivalent updates.

### Phase 3 — `Concertable.B2B.Seeding.Fixture` (canonical event records — full set)

New project at `api/Concertable.B2B/Concertable.B2B.Seeding.Fixture/`. Purpose: hold the **canonical event payloads** for every B2B entity that publishes an event downstream services consume — the single source of truth that B2B's own seeders and the simulator both derive from. No drift on any field for any entity.

Scope: every `VenueChangedEvent`, `ArtistChangedEvent`, `ConcertChangedEvent` B2B's seed would publish. Counted from current `Concertable.B2B.Seeding/SeedData.cs`:

- 35 venues (1 hardcoded "The Grand Venue" + 34 from the `venueData` array)
- 35 artists (the `bands` array)
- 47 concerts (the `Concerts` initializer)

= ~117 event records.

B2B-internal entities (Applications, Bookings, Contracts, Opportunities, Transactions, etc., ~95 more) **stay in `B2B.Seeding.SeedData`** — they're not in the fixture because they don't publish events Customer or Search subscribe to. The fixture is about wire-level sync, not B2B's whole internal model.

`Concertable.B2B.Seeding.Fixture.csproj` — net10.0, references:

- `Concertable.B2B.Venue.Contracts`
- `Concertable.B2B.Artist.Contracts`
- `Concertable.B2B.Concert.Contracts`
- `Concertable.Seeding.Identity` (for `SeedUsers.VenueManagerId/Email`, `SeedUsers.ArtistManagerId/Email`)

`B2BSeedFixture.cs` (with one full entry per list to show the shape; remaining entries follow the same pattern):

```csharp
using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Contracts;
using Concertable.Kernel;
using Concertable.Seeding.Identity;

namespace Concertable.B2B.Seeding.Fixture;

public static class B2BSeedFixture
{
    public static IReadOnlyList<VenueChangedEvent> Venues { get; } =
    [
        new VenueChangedEvent(
            VenueId:   1,
            UserId:    SeedUsers.VenueManagerId(1),
            Name:      "The Grand Venue",
            About:     "Test venue",
            Avatar:    "avatar.jpg",
            BannerUrl: "grandvenue.jpg",
            County:    "Test County",
            Town:      "Test Town",
            Latitude:  51.0,
            Longitude: 0.0,
            Email:     SeedUsers.VenueManagerEmail(1)),

        new VenueChangedEvent(
            VenueId:   2,
            UserId:    SeedUsers.VenueManagerId(2),
            Name:      "Redhill Hall",
            About:     "Test venue",
            Avatar:    "avatar.jpg",
            BannerUrl: "redhillhall.jpg",
            County:    "Greater London",
            Town:      "London",
            Latitude:  51.5074,
            Longitude: -0.1278,
            Email:     SeedUsers.VenueManagerEmail(2)),

        // ... 33 more. Each entry is a self-contained literal. The `venueData` array
        // + `Locations` iteration that current SeedData.cs uses is fully materialised here.
    ];

    public static IReadOnlyList<ArtistChangedEvent> Artists { get; } =
    [
        new ArtistChangedEvent(
            ArtistId:  1,
            UserId:    SeedUsers.ArtistManagerId(1),
            Name:      "The Rockers",
            About:     "Test artist",
            Avatar:    "avatar.jpg",
            BannerUrl: "rockers.jpg",
            County:    "Leicestershire",
            Town:      "Loughborough",
            Latitude:  52.7721,
            Longitude: -1.2062,
            Email:     SeedUsers.ArtistManagerEmail(1),
            Genres:    [Genre.Rock, Genre.Pop, Genre.Jazz]),

        // ... 34 more from the current `bands` array, each as a self-contained literal.
    ];

    // Time-relative fields make this a factory taking `now`. Static parts (ID, Name, ArtistId,
    // VenueId, etc.) are inline literals — no `opps[]` lookups, no index arithmetic.
    public static IReadOnlyList<ConcertChangedEvent> Concerts(DateTime now) =>
    [
        new ConcertChangedEvent(
            ConcertId:        13,
            Name:             "Upcoming FlatFee Show",
            About:            "Test concert",
            Avatar:           null,
            BannerUrl:        null,
            TotalTickets:     150,
            AvailableTickets: 150,
            Price:            20m,
            Period:           new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)),
            DatePosted:       now,
            ArtistId:         2,
            ArtistName:       "Indie Vibes",
            VenueId:          1,
            VenueName:        "The Grand Venue",
            Latitude:         51.0,
            Longitude:        0.0,
            Genres:           [Genre.Rock, Genre.Indie],
            PayeeUserId:      SeedUsers.VenueManagerId(1)),

        // ... 46 more from the current `Concerts` initializer, each as a self-contained literal.
    ];

    // Convenience accessor used by the readiness-gate waiter and Customer E2E tests.
    public static ConcertChangedEvent UpcomingConcert(DateTime now) =>
        Concerts(now).First(c => c.ConcertId == 13);

    // Stable accessor for assertions where the time-relative shape doesn't matter.
    public const int UpcomingConcertId = 13;
}
```

Where the current `SeedData.cs` computes values via index lookups (`opps[5].VenueId`, `Locations[locIndex++ % Locations.Length]`, `Bookings[12].Id`), the fixture **inlines the resolved values per entry**. No `opps[]`, no `Locations[]`, no index arithmetic. Each entity in the fixture is a self-contained literal — every field is written out explicitly. This is what materialising the canonical data means: the helpers go away once their outputs are baked into the fixture.

Consumers:

- `Concertable.B2B.Seeding.Simulator` — iterates and publishes all 117 events.
- `Concertable.B2B.Seeding` — derives its 35 venues + 35 artists + 47 concerts from these lists (see Phase 3b).
- `Concertable.Customer.E2ETests` — references the fixture to assert on any field of any seeded venue/artist/concert.

Move `UpcomingConcertId` out of `Concertable.Customer.Seeding/SeedData.cs`. Customer.E2ETests asserts via `B2BSeedFixture.Concerts(now)` or via a named accessor like `B2BSeedFixture.UpcomingConcert(now)` that returns the specific entry tests care about.

### Phase 3b — `Concertable.B2B.Seeding.SeedData` derives venues/artists/concerts from the fixture

To guarantee byte-for-byte sync, B2B's seeders must build their entities from the same source the simulator publishes from.

`Concertable.B2B.Seeding/Concertable.B2B.Seeding.csproj` — add `ProjectReference` to `Concertable.B2B.Seeding.Fixture`.

Add a static `FromSeedFixture(XChangedEvent, ...extras...)` method per faker/factory. Examples:

```csharp
// api/Concertable.B2B/Concertable.B2B.Seeding/Fakers/VenueFaker.cs
public static partial class VenueFaker
{
    public static VenueEntity FromSeedFixture(VenueChangedEvent e) =>
        Create(
            id:        e.VenueId,
            userId:    e.UserId,
            name:      e.Name,
            banner:    e.BannerUrl,
            avatar:    e.Avatar,
            location:  new Point(e.Longitude, e.Latitude) { SRID = 4326 },
            address:   new Address(e.County, e.Town),
            email:     e.Email);
}
```

```csharp
// api/Concertable.B2B/Concertable.B2B.Seeding/Fakers/ArtistFaker.cs
public static partial class ArtistFaker
{
    public static ArtistEntity FromSeedFixture(ArtistChangedEvent e) =>
        Create(
            id:        e.ArtistId,
            userId:    e.UserId,
            name:      e.Name,
            banner:    e.BannerUrl,
            avatar:    e.Avatar,
            location:  new Point(e.Longitude, e.Latitude) { SRID = 4326 },
            address:   new Address(e.County, e.Town),
            email:     e.Email,
            genres:    e.Genres.ToArray());
}
```

```csharp
// api/Concertable.B2B/Concertable.B2B.Seeding/Factories/ConcertFactory.cs (or wherever lives)
public static partial class ConcertFaker
{
    // The concert's bookingId isn't in the event (it's a B2B-internal coupling),
    // so the caller passes it alongside the fixture event.
    public static ConcertEntity FromSeedFixture(ConcertChangedEvent e, int bookingId) =>
        Post(
            id:           e.ConcertId,
            bookingId:    bookingId,
            name:         e.Name,
            price:        e.Price,
            totalTickets: e.TotalTickets,
            artistId:     e.ArtistId,
            venueId:      e.VenueId,
            period:       e.Period,
            datePosted:   e.DatePosted,
            genres:       e.Genres.ToArray());
}
```

`Concertable.B2B.Seeding/SeedData.cs` — the venue/artist/concert array literals **move out** entirely into `B2BSeedFixture` (Phase 3). The corresponding constructor blocks in `SeedData` become tiny projections:

```csharp
// before — ~150 lines of array literals + Locations[locIndex++ ...] iteration
// after:
Venues  = [.. B2BSeedFixture.Venues.Select(VenueFaker.FromSeedFixture)];
Venue   = Venues[0];

Artists = [.. B2BSeedFixture.Artists.Select(ArtistFaker.FromSeedFixture)];
Artist  = Artists[0];

Concerts = [.. B2BSeedFixture.Concerts(now).Select(c =>
    ConcertFaker.FromSeedFixture(c, bookingId: Bookings[BookingIndexFor(c.ConcertId)].Id))];
```

Where `BookingIndexFor(int concertId)` is a small private helper inside `SeedData.cs` that maps each fixture concert id to the right `Bookings[i]` index — that coupling is B2B-internal and stays here, doesn't leak into the fixture.

`SeedData.cs` then keeps everything *not* in the fixture: Contracts (66 entries), Opportunities (67), Bookings (~50), Applications (~95), the various exposed `XApp`/`XBooking`/`XContract` properties. These stay because they're B2B-internal, no events to sync.

### Phase 4 — `Concertable.B2B.Seeding.Simulator`

New project at `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/`. Worker host mirroring `api/Concertable.Search/Concertable.Search.Workers/Program.cs`.

`Concertable.B2B.Seeding.Simulator.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Shared\Concertable.ServiceDefaults\Concertable.ServiceDefaults.csproj" />
    <ProjectReference Include="..\Modules\Venue\Concertable.B2B.Venue.Contracts\Concertable.B2B.Venue.Contracts.csproj" />
    <ProjectReference Include="..\Modules\Artist\Concertable.B2B.Artist.Contracts\Concertable.B2B.Artist.Contracts.csproj" />
    <ProjectReference Include="..\Modules\Concert\Concertable.B2B.Concert.Contracts\Concertable.B2B.Concert.Contracts.csproj" />
    <ProjectReference Include="..\Concertable.B2B.Seeding.Fixture\Concertable.B2B.Seeding.Fixture.csproj" />
    <ProjectReference Include="..\..\Concertable.Messaging\Concertable.Messaging.AzureServiceBus\Concertable.Messaging.AzureServiceBus.csproj" />
    <ProjectReference Include="..\..\Concertable.Messaging\Concertable.Messaging.Contracts\Concertable.Messaging.Contracts.csproj" />
  </ItemGroup>
</Project>
```

`Program.cs`:

```csharp
using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Messaging.AzureServiceBus.Extensions;
using Concertable.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAzureServiceBusTransport(
    opts =>
    {
        opts.ConnectionString = builder.Configuration.GetConnectionString("asb") ?? "";
        opts.ServiceName = "concertable-b2b-seeding-simulator";
    },
    reg => reg
        .Publishes<VenueChangedEvent>()
        .Publishes<ArtistChangedEvent>()
        .Publishes<ConcertChangedEvent>());

builder.Services.AddHostedService<SeedEventPublishingService>();

var app = builder.Build();
app.Run();
```

`SeedEventPublishingService.cs`:

```csharp
using Concertable.B2B.Seeding.Fixture;
using Concertable.Messaging.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concertable.B2B.Seeding.Simulator;

internal sealed class SeedEventPublishingService : BackgroundService
{
    private readonly IBusTransport transport;
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<SeedEventPublishingService> logger;

    public SeedEventPublishingService(
        IBusTransport transport,
        IHostApplicationLifetime lifetime,
        ILogger<SeedEventPublishingService> logger)
    {
        this.transport = transport;
        this.lifetime = lifetime;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;

        foreach (var v in B2BSeedFixture.Venues)
            await transport.PublishAsync(v, Envelope(), stoppingToken);
        logger.LogInformation("Published {Count} venue events", B2BSeedFixture.Venues.Count);

        foreach (var a in B2BSeedFixture.Artists)
            await transport.PublishAsync(a, Envelope(), stoppingToken);
        logger.LogInformation("Published {Count} artist events", B2BSeedFixture.Artists.Count);

        var concerts = B2BSeedFixture.Concerts(now);
        foreach (var c in concerts)
            await transport.PublishAsync(c, Envelope(), stoppingToken);
        logger.LogInformation("Published {Count} concert events", concerts.Count);

        lifetime.StopApplication();
    }

    private static MessageEnvelope Envelope() =>
        new(Guid.NewGuid(), "concertable-b2b-seed", DateTimeOffset.UtcNow);
}
```

~117 events published in total. Calling `StopApplication()` after publishing transitions the Aspire resource to Exited cleanly. Idempotent at the consumer level: Customer/Search projection handlers upsert by entity ID; re-running the simulator does not corrupt projections.

The simulator's existing CLAUDE.md (from Phase 1) is the canonical design reference for anyone touching this project.

### Phase 5 — Aspire wiring

- `api/Concertable.AppHost.Shared/AppHostConstants.cs` (or wherever `ResourceNames` lives) — add `public const string B2BSeedingSimulator = "b2b-seeding-simulator";`.
- `api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs` — new extension method mirroring `AddSearchWorkers`:

  ```csharp
  public static IResourceBuilder<ProjectResource> AddB2BSeedingSimulator<TProject>(
      this IDistributedApplicationBuilder builder,
      IResourceBuilder<AzureServiceBusResource> asb)
      where TProject : IProjectMetadata, new()
      => builder.AddProject<TProject>(AppHostConstants.ResourceNames.B2BSeedingSimulator)
                .WithReference(asb)
                .WaitFor(asb);
  ```

- `api/Concertable.Customer/Concertable.Customer.AppHost/Program.cs` — `builder.AddB2BSeedingSimulator<Projects.Concertable_B2B_Seeding_Simulator>(asb);` after `customerWeb` is registered.
- `api/Concertable.Customer/Concertable.Customer.AppHost/Concertable.Customer.AppHost.csproj` — add `ProjectReference` to the simulator project (Aspire needs source in the build graph to generate the `Projects.X` type).
- `api/Concertable.AppHost/Program.cs` — **do not** register the simulator. Real B2B is running in the umbrella; double-publishing wastes cycles.

### Phase 6 — readiness gate via existing `IHealthWaiter` pattern (E2E-only registration)

Run 3 (see `plans/CURRENT_E2E_RESULTS.md`) flagged a chicken-and-egg deadlock when an event-driven `IHealthWaiter` is registered in a module that the Web host itself consumes:

- `Program.cs` blocks on `await initializer.InitializeAsync()` before `app.Run()`.
- The waiter inside `InitializeAsync` polls for rows that arrive only when the ASB consumer (an `IHostedService`) processes events.
- Hosted services don't start until `app.Run()`, which is unreachable while `InitializeAsync` blocks. Hard deadlock.

So the waiter class lives in `Concertable.Customer.Concert.Infrastructure`, but registration goes **only into the E2E `AppFixture`'s seed host**, not into `AddCustomerConcertModule`. Customer.Web in dev mode resolves an empty `IEnumerable<IHealthWaiter>`; `DevDbInitializer`'s wait loop is a no-op there. In dev there's nothing to wait for anyway: under the umbrella AppHost real B2B publishes events, under standalone `Concertable.Customer.AppHost` the simulator publishes them — neither needs Customer.Web to block on the projection landing.

New file `api/Concertable.Customer/Modules/Concert/Concertable.Customer.Concert.Infrastructure/Data/ConcertProjectionHealthWaiter.cs`:

```csharp
using Concertable.B2B.Seeding.Fixture;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertProjectionHealthWaiter : IHealthWaiter
{
    private readonly ConcertDbContext context;
    private readonly DbHealthWaiter waiter;

    public ConcertProjectionHealthWaiter(ConcertDbContext context, DbHealthWaiter waiter)
    {
        this.context = context;
        this.waiter = waiter;
    }

    public Task WaitForReadyAsync(TimeSpan timeout)
    {
        // Resolve the seed concert id once (UpcomingConcert is time-relative, but ConcertId is not).
        var seedConcertId = B2BSeedFixture.UpcomingConcert(DateTime.UtcNow).ConcertId;
        var filtered = context.Concerts.Where(c => c.Id == seedConcertId);
        return waiter.WaitForCountAsync(filtered, expectedCount: 1, timeout);
    }
}
```

Registration goes into `Concertable.Customer.E2ETests/AppFixture.cs`'s seed-host `ConfigureServices` only:

```csharp
services.TryAddSingleton<DbHealthWaiter>();
services.AddScoped<IHealthWaiter, ConcertProjectionHealthWaiter>();
```

**Do not** register inside `AddCustomerConcertModule` — that path is consumed by Customer.Web's prod DI, which would create the deadlock.

`DevDbInitializer` in Customer.Web already calls `await Task.WhenAll(waiters.Select(w => w.WaitForReadyAsync(...)))`. Empty `IEnumerable` in dev → instant pass; in E2E seed host the waiter is registered and reseed waits for the seed concert.

`Concertable.Customer.Concert.Infrastructure.csproj` gains a `ProjectReference` to `Concertable.B2B.Seeding.Fixture`.

### Phase 7 — E2E cleanup and waiter registration

Delete `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/ProjectionSeeder.cs` entirely.

`AppFixture.cs` changes — the seed-host `ConfigureServices` block becomes:

```csharp
// BEFORE
host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        // ...other registrations...
        services.AddOutbox(opt => opt.UseSqlServer(customerConnectionString), runDispatcher: false);
        services.AddInbox(opt => opt.UseSqlServer(customerConnectionString));
        services.AddSeedingInfrastructure();
        services.AddScoped<SeedData>();
        services.AddCustomerConcertModule(customerSeedConfig);
        services.AddCustomerPreferenceModule(customerSeedConfig);
        services.AddAzureServiceBusTransport(
            opts =>
            {
                opts.ConnectionString = asbConnectionString;
                opts.ServiceName = "concertable-e2e-seeder";
            },
            _ => { });
        services.AddCustomerPreferenceDevSeeder();
        services.AddScoped<IDbInitializer, CustomerDevDbInitializer>();
    })
    .Build();

await host.StartAsync();
await ReseedAsync();
await new ProjectionSeeder(host, Polling).SeedAsync();   // gone in AFTER
```

```csharp
// AFTER
host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        // ...other registrations...
        services.AddOutbox(opt => opt.UseSqlServer(customerConnectionString), runDispatcher: false);
        services.AddInbox(opt => opt.UseSqlServer(customerConnectionString));
        services.AddSeedingInfrastructure();
        services.AddScoped<SeedData>();
        services.AddCustomerConcertModule(customerSeedConfig);
        services.AddCustomerPreferenceModule(customerSeedConfig);
        services.AddCustomerPreferenceDevSeeder();
        services.AddScoped<IDbInitializer, CustomerDevDbInitializer>();

        // Readiness gate: block reseed until the seed concert arrives via the simulator's events.
        services.TryAddSingleton<DbHealthWaiter>();
        services.AddScoped<IHealthWaiter, ConcertProjectionHealthWaiter>();
    })
    .Build();

await host.StartAsync();
await ReseedAsync();
// No more ProjectionSeeder call — the simulator (Aspire resource) does the publishing;
// DevDbInitializer's IHealthWaiter loop gates reseed completion on the seed concert landing.
```

Also drop in the same file:

- `await app.GetConnectionStringAsync("asb")` line — not needed without the transport.
- `using Concertable.Messaging.AzureServiceBus.Extensions;` — no longer used.
- The duplicate `ProjectionSeeder` call inside `ResetAsync` (per-scenario reseed).

`Payments/TicketPurchaseTests.cs`:

```csharp
// before
using Concertable.Customer.Seeding;
...
ConcertId = SeedData.UpcomingConcertId,

// after
using Concertable.B2B.Seeding.Fixture;
...
ConcertId = B2BSeedFixture.UpcomingConcert(DateTime.UtcNow).ConcertId,
```

(or expose a const `B2BSeedFixture.UpcomingConcertId = 13` since `ConcertId` itself isn't time-relative; the assertion doesn't need to materialise the whole event.)

`Concertable.Customer.E2ETests.csproj` — add `ProjectReference` to `Concertable.B2B.Seeding.Fixture` if not already transitive through Customer.Concert.Infrastructure.

## Critical files

**New projects:**

- `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/{Program.cs, SeedEventPublishingService.cs, Concertable.B2B.Seeding.Simulator.csproj, appsettings.json, CLAUDE.md}` (CLAUDE.md already written in Phase 1).
- `api/Concertable.B2B/Concertable.B2B.Seeding.Fixture/{B2BSeedFixture.cs, Concertable.B2B.Seeding.Fixture.csproj}` — holds the canonical `XChangedEvent` records both producers derive from.
- `api/Concertable.Customer/Modules/Concert/Concertable.Customer.Concert.Infrastructure/Data/ConcertProjectionHealthWaiter.cs`.

**B2B.Seeding refactor (Phase 3b):**

- New `FromSeedFixture(XChangedEvent, ...)` factories on `Fakers/VenueFaker.cs`, `Fakers/ArtistFaker.cs`, and the seed-concert factory.
- `Concertable.B2B.Seeding/SeedData.cs` — venue / artist / concert blocks rewritten to project from the fixture; ~150 lines of array literals move out into the fixture.
- `Concertable.B2B.Seeding/Concertable.B2B.Seeding.csproj` — add ProjectReference to `Concertable.B2B.Seeding.Fixture`.

**Project moves (Phase 2):**

- `api/Seeding/Concertable.Seeding.Shared/` → `api/Shared/Concertable.Seeding.Shared/` (minus the Fakers/ subfolder).
- `api/Seeding/Concertable.Seeding.Identity/` → `api/Shared/Concertable.Seeding.Identity/`.
- `api/Seeding/Concertable.Seeding.Infrastructure/` → `api/Shared/Concertable.Seeding.Infrastructure/`.
- `api/Seeding/Concertable.B2B.Seeding/` → `api/Concertable.B2B/Concertable.B2B.Seeding/` (gains the moved `Fakers/`).
- `api/Seeding/Concertable.Customer.Seeding/` → `api/Concertable.Customer/Concertable.Customer.Seeding/`.
- `api/Seeding/Concertable.Seeding/` and `api/Seeding/Concertable.Seeding.WellKnown/` — deleted (empty).
- `api/Seeding/` folder — deleted after the above.

**Docs (Phase 1 — DONE):**

- ✅ Root `CLAUDE.md` — trimmed to a short reference to `ARCHITECTURE.md`.
- ✅ `ARCHITECTURE.md` (root, new) — microservice premise.
- ✅ `api/docs/SEEDING_CONVENTIONS.md` — banner + Standalone-service seeding section.
- ✅ `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md` — full simulator design reference.

**Modified:**

- `Concertable.slnx` — solution file references the new project paths.
- All csprojs across the codebase that have `ProjectReference` to moved projects (mechanical path updates, ~30–40 files).
- `api/Concertable.AppHost.Shared/{AppHostConstants.cs, DistributedApplicationBuilderExtensions.cs}` — new resource name + extension method.
- `api/Concertable.Customer/Concertable.Customer.AppHost/{Program.cs, Concertable.Customer.AppHost.csproj}` — register resource + project ref.
- `api/Concertable.Customer/Modules/Concert/Concertable.Customer.Concert.Infrastructure/Concertable.Customer.Concert.Infrastructure.csproj` — ref to `Concertable.B2B.Seeding.Fixture`.
- **No** change to `Concertable.Customer.Concert.Infrastructure.Extensions.ServiceCollectionExtensions` — the waiter registration goes into the E2E AppFixture only, not into `AddCustomerConcertModule`, to avoid the deadlock documented in Phase 6.
- `Concertable.Customer.Seeding/SeedData.cs` — drop `UpcomingConcertId`.
- `Concertable.Customer.E2ETests/{AppFixture.cs, Payments/TicketPurchaseTests.cs, Concertable.Customer.E2ETests.csproj}` — drop ASB + ProjectionSeeder, switch to fixture accessor.

**Deleted:**

- `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/ProjectionSeeder.cs`.
- Phase 0 reverts.
- The two empty placeholder seeding projects.

## Existing pieces being reused

- Event records `VenueChangedEvent`, `ArtistChangedEvent`, `ConcertChangedEvent` from `Concertable.B2B.{Venue,Artist,Concert}.Contracts.Events` — exact same types projection handlers consume.
- `IBusTransport.PublishAsync` and `MessageEnvelope` from `Concertable.Messaging.Contracts`.
- `AddAzureServiceBusTransport` registration pattern from `api/Concertable.Search/Concertable.Search.Workers/Program.cs:22-34` — copy shape, swap `SubscribeTo` for `Publishes`.
- `DbHealthWaiter` (`api/Concertable.DataAccess/Concertable.DataAccess.Infrastructure/DbHealthWaiter.cs`), `IHealthWaiter` (`api/Concertable.DataAccess/Concertable.DataAccess.Application/IHealthWaiter.cs`). `UserHealthWaiter` (`api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Data/UserHealthWaiter.cs`) is the reference impl.
- `DevDbInitializer` in both Customer.Web and B2B.Web — already awaits `IHealthWaiter`s. Same code runs dev + E2E. No new initializer.

## Verification

1. **Solution builds clean post-reorg.** `dotnet build api/Concertable.slnx` and `dotnet build api/Concertable.Customer/Concertable.Customer.slnx` and `dotnet build api/Concertable.B2B/Concertable.B2B.slnx` — zero errors. Catches any missed csproj path updates.
2. **Standalone Customer dev:** `dotnet run --project api/Concertable.Customer/Concertable.Customer.AppHost`. Customer SPA loads and shows the seeded concerts without any test fixture. Aspire dashboard shows `b2b-seeding-simulator` running briefly then exiting.
3. **Full E2E suite — no regression vs baseline.** Run `./e2e.ps1 run` (both B2B and Customer suites). Compare totals against [`plans/CURRENT_E2E_RESULTS.md`](./CURRENT_E2E_RESULTS.md):
   - Current baseline at `ec3a6723`: **9 passed, 21 failed** (B2B 7/23, Customer 2/7). All 21 failures are Stripe payment flows — not in scope for this plan to fix.
   - **Pass criterion:** post-change result must be ≥ 9 passing, and the failing set must be a strict subset of the baseline failing set (no previously-passing scenario regresses). Specifically, the 7 B2B passes and 2 Customer passes listed in CURRENT_E2E_RESULTS.md must still pass.
   - If anything from the baseline-passing list regresses, the change is incorrect — investigate before merging. Likely candidates: a missed csproj path update, the simulator not being registered, the fixture's resolved-literal values drifting from what current SeedData produces.
   - Update CURRENT_E2E_RESULTS.md with the post-change result and the new head SHA.
4. **Umbrella unaffected:** `dotnet run --project api/Concertable.AppHost`. Real B2B runs; the simulator is not registered. Existing seed flow unchanged.
5. **B2B AppHost standalone unaffected:** `dotnet run --project api/Concertable.B2B/Concertable.B2B.AppHost`. B2B's own seeders fire, real events published. Simulator not registered. Same observable B2B DB state as before this plan.
6. **Folder reorg check:** `ls api/Seeding/` returns "no such file or directory". `ls api/Shared/ | grep Seeding` returns three projects. `ls api/Concertable.B2B/ | grep Seeding` returns three projects (data + simulator + fixture). `ls api/Concertable.Customer/ | grep Seeding` returns one project.
7. **Convention check:** `grep -rn '\.Add(Range)?\s*(' --include='*.cs' api/ | grep -iE 'ReadModel' | grep -v 'obj/' | grep -v 'Migrations/'` returns zero hits.
8. **Customer.Seeding scope check:** `grep -rn 'Concertable\.B2B' api/Concertable.Customer/Concertable.Customer.Seeding/` returns zero — Customer.Seeding has no knowledge of B2B identifiers.
9. **Tight-sync check:** start the umbrella AppHost; query CustomerDb for venue id 1, artist id 2, concert id 13; record every column. Stop. Start the standalone Customer AppHost; query the same three rows; columns should be identical (modulo `DatePosted` because `UpcomingConcert` is time-relative — comparable by structure, not absolute value). Any drift means a field is being set somewhere other than `B2BSeedFixture`.

## Related docs

- Root [`ARCHITECTURE.md`](../ARCHITECTURE.md) — microservice premise.
- [`api/docs/SEEDING_CONVENTIONS.md`](../api/docs/SEEDING_CONVENTIONS.md) — banner + standalone-service seeding section.
- [`api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md`](../api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md) — full simulator design reference.
