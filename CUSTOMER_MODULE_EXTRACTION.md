# Extract the Customer domain from B2B into the Customer service

## Context

"Customer" is currently smeared across three places in the codebase:

- **`Concertable.B2B/Modules/Customer/`** — a 5-project module owning customer
  concert-discovery *preferences* (`PreferenceEntity` = notification radius +
  preferred `Genre`s, `GenrePreferenceEntity`). It has its own `CustomerDbContext`
  (`customer` schema) but sits inside the B2B monolith on `B2BDb`. B2B has no
  business owning customer-domain data.
- **`Concertable.B2B/Modules/User/`** — holds `Role.Customer` and customer
  credentials. Temporary; `AUTH_IDENTITY_REFACTOR.md` moves credentials out.
- **`Concertable.Customer/`** — the actual consumer marketplace (its own modular
  monolith on `CustomerDb`). Its `Profile` module (`CustomerProfileEntity`, just a
  `Sub` GUID) is the real customer-account home but is currently a near-empty stub
  whose `Domain`/`Application`/`Api` projects are empty scaffolds.

The single coupling B2B has *into* the preference store is
`ICustomerModule.GetUserIdsByLocationAndGenresAsync`, called by B2B's
`ConcertService.PostAsync` to find customers to notify when a concert is posted. A
B2B service reaching into a customer-domain store to run a customer-domain query is
the leak to remove.

**Intended outcome:** all customer-domain data lives only in the
`Concertable.Customer` service. Its `Profile` module is promoted into a proper
**`User` module** — symmetric with B2B's User module — owning the customer
`UserEntity`, a customer-side `Role` enum (`Customer`, `Admin`), and the preference
data. B2B stops knowing customers exist. "Notify interested customers" becomes a
Customer-side reaction to the `ConcertChangedEvent` B2B already publishes.

**End-state architecture:**

```
            Auth  (identity service: owns credentials, issues tokens, role-agnostic)
              │ publishes *RegisteredEvent
       ┌──────┴───────┐
      B2B            Customer
    (B2BDb)         (CustomerDb)
    business app    consumer marketplace
    User module     User module  (← promoted from "Profile")
     Role: Venue/    Role: Customer, Admin
     Artist/Admin    + Preference data
    Venue/Artist/    Concert(read-proj)/Ticket/Review
    Concert/...
```

Two independent modular monoliths. Each owns its own users and its own `Role` enum.
They never share a DB and never call each other synchronously — only Azure Service
Bus events.

This pairs with `AUTH_IDENTITY_REFACTOR.md`: this doc moves customer *domain data*;
the Auth refactor moves customer *credentials*. **Sequencing:** start after Auth
Phase 2 is committed. Phases 1–5 here are independent of Auth Phase 3; only the
customer `role` *token claim* depends on the Auth refactor's claims-provider seam —
cross-referenced where relevant.

This is a large, multi-phase change. Phases land and verify independently.

---

## Phase 1 — Rename the `Profile` module → `User` module

Pure rename, no behaviour change. The `Profile` module is the Customer service's
user/account module, mis-named; `Profile.Domain`/`.Application`/`.Api` are empty
scaffolds — all code is in `Profile.Infrastructure`.

- Rename projects `Concertable.Customer.Profile.{Domain,Application,Infrastructure,Api}`
  → `Concertable.Customer.User.*`; folder `Modules/Profile/` → `Modules/User/`.
- `ProfileDbContext` → `UserDbContext`; schema `profile` → `user` (match B2B's User
  module schema convention — confirm at implementation).
- `CustomerProfileEntity` → `UserEntity`; `CustomerProfiles` DbSet/table → `Users`.
- `CustomerProfileCreationHandler` → `UserRegistrationHandler` (still consumes
  `CustomerRegisteredEvent`, still inbox-idempotent).
- Update `Schema.cs`, the `*ConfigurationProvider`, `ServiceCollectionExtensions`
  (`AddCustomerProfileModule` → `AddCustomerUserModule`), namespaces.
- Update `Concertable.Customer.slnx`, `Concertable.Customer.Web` project references +
  `Program.cs` composition, and `initial-migrations.ps1`
  (`ProfileDbContext` → `UserDbContext`).
- Re-scaffold the module's `InitialCreate`.

*Verification:* solution builds; `CustomerRegisteredEvent` still creates the customer
row (now `UserEntity`) via the inbox.

---

## Phase 2 — Customer `Role` enum + enriched `UserEntity`

- **New `Concertable.Customer.User.Contracts` project** (symmetric with B2B's
  `Concertable.User.Contracts` and the existing `Concertable.Customer.Review.Contracts`).
  Holds:
  - `Role` enum — `Customer`, `Admin` (the marketplace's own taxonomy, fully separate
    from B2B's `VenueManager`/`ArtistManager`/`Admin`).
  - `ICustomerUserModule` — the cross-module facade (populated in Phase 3).
- `UserEntity` gains: `Role` (default `Customer`), `Email` (denormalized from
  `CustomerRegisteredEvent`), `Location` (NetTopologySuite `Point`, nullable — the
  customer's home, needed for radius matching), optionally `Address`.
- `UserRegistrationHandler` sets `Email` + `Role.Customer` from the event.
- Customer-service authorization policies (`Customer`, `Admin`) registered in
  `Customer.Web` — gating customer vs marketplace-admin endpoints.

Note: the `role` *token claim* for customers is emitted by Auth. Establishing the
enum + policies here is independent; wiring Auth to emit customer roles through the
`IProfileClaimsProvider` seam is tracked in `AUTH_IDENTITY_REFACTOR.md`.

*Verification:* builds; a newly-registered customer has a `UserEntity` with
`Role.Customer` and `Email`; the re-scaffolded `InitialCreate` reflects the new
columns.

---

## Phase 3 — Move the preference domain into the `User` module

Bring the preference code out of `Concertable.B2B/Modules/Customer/` into the
Customer service's `User` module (preferences are a customer-account concern — one
per customer, unique on `UserId`).

- **Domain** (`Concertable.Customer.User.Domain`): `PreferenceEntity`,
  `GenrePreferenceEntity` (verbatim; `Genre` enum already shared via Kernel).
- **Application** (`...User.Application`): `IPreferenceService`,
  `IPreferenceRepository`, `PreferenceDto`, `CreatePreferenceRequest`,
  `CreatePreferenceRequestValidator`, `PreferenceMappers`.
- **Infrastructure** (`...User.Infrastructure`): `PreferenceService`,
  `PreferenceRepository`, EF configs; `Preferences`/`GenrePreferences` become DbSets
  on `UserDbContext` (`user` schema).
- **Api** (`...User.Api`): `PreferenceController` — wire the (currently empty)
  `User.Api` project into `Customer.Web`; carry the `InternalControllerFeatureProvider`.
- **Rewire `PreferenceService`** — drop the B2B `IUserModule` dependency.
  `GetUserIdsByLocationAndGenresAsync` now joins `Preferences` → `Users` locally and
  filters by `IGeometryCalculator.IsWithinRadius` using `UserEntity.Location` +
  `PreferenceEntity.RadiusKm`. Keeps `ICurrentUser` (already available —
  `Customer.Web` calls `AddCurrentUser()`).
- **`ICustomerUserModule`** (in `...User.Contracts`) exposes
  `GetUserIdsByLocationAndGenresAsync` for the Concert module (Phase 4).
- `UserEntity.Location` is set by the customer — recommend bundling it into the
  preference create/update request initially (the only feature that needs it),
  rather than a separate endpoint.
- Register `IGeometryCalculator` + the geographic geometry provider in `Customer.Web`
  (B2B registers these at host level — replicate).
- Re-scaffold `UserDbContext`'s `InitialCreate` (now includes
  `Preferences`/`GenrePreferences`).

*Verification:* preference CRUD works through the Customer service's API;
`GetUserIdsByLocationAndGenresAsync` returns correct matches with no cross-service
call.

---

## Phase 4 — Event-driven "notify interested customers"

B2B already publishes `ConcertChangedEvent` (carries `Latitude`, `Longitude`,
`Genres`, `DatePosted`); the Customer service already subscribes
(`customer-concert-changed`).

- New handler `ConcertPostedNotificationHandler` in the Customer `Concert` module,
  `IIntegrationEventHandler<ConcertChangedEvent>` — a second in-process consumer of
  the existing subscription (the inbox keys on `MessageId` + `ConsumerName`, so two
  handlers for one event is fine; no new ASB subscription needed).
- On a concert's *posted* transition (`DatePosted` non-null) and not-yet-notified:
  call `ICustomerUserModule.GetUserIdsByLocationAndGenresAsync(Latitude, Longitude,
  Genres)`, then `INotificationClient.SendAsync(userId, "ConcertPosted", payload)`
  per matched customer (`AddNotificationClient()` already wired in `Customer.Web`).
- Idempotent notify-once: a `NotifiedSubscribers` flag on the local `ConcertEntity`
  projection (the Concert module owns it); subsequent `ConcertChangedEvent`s skip.

*Verification:* posting a concert in B2B → matched customers receive a
`ConcertPosted` SignalR push from the Customer service; re-posting / editing the
concert does not re-notify.

---

## Phase 5 — Sever B2B and delete `Modules/Customer/`

- `Concertable.Concert.Infrastructure/Services/ConcertService.cs` — `PostAsync` stops
  injecting/calling `ICustomerModule`; `ConcertPostResponse` loses `UserIds`.
- Delete B2B's `ConcertPostedHandler` SignalR fan-out and
  `ConcertNotifier.ConcertPostedAsync` (notification now happens Customer-side).
- Drop the `Concertable.Concert.Infrastructure.csproj` →
  `Concertable.Customer.Contracts` project reference.
- **Delete `Concertable.B2B/Modules/Customer/`** (all 5 projects).
- Remove `AddCustomerApi`/`AddCustomerModule`/`AddCustomerDevSeeder` from `B2B.Web`
  and `B2B.Workers`; remove `AddCustomerTestSeeder` from
  `Concertable.Testing.Integration/ApiFixture.cs`.
- Remove the Customer projects from `Concertable.slnx` and `Concertable.B2B.slnx`.
- Delete the stale, unreferenced `api/Modules/Customer/` duplicate (3 orphan csprojs,
  no source — confirmed not in any `.slnx`).

*Verification:* B2B builds with no Customer module; posting a concert performs no
preference lookup in B2B.

---

## Phase 6 — Seeding, migrations, end-to-end

- The Customer service has **no seeders today**. Add `IDevSeeder`/`ITestSeeder` for
  the `User` module — seed customer `UserEntity` rows (+ sample preferences) so
  customer login and preference flows are exercisable. (B2B's old
  `CustomerDevSeeder`/`CustomerTestSeeder` are the reference; B2B's `SeedData`
  carrier does not cross service boundaries — the Customer service needs its own.)
- Re-scaffold every affected `InitialCreate` via `initial-migrations.ps1` (no
  additive migrations); nuke + recreate `CustomerDb` and `B2BDb`.
- Full `AppHost` run: register a customer → `UserEntity` appears in the Customer
  service via the inbox; set preferences; post a concert in B2B → matched customer
  gets the SignalR notification; customer login + UI E2E pass.

Note: customer login still authenticates against B2B's credential store until the
Auth refactor's Phase 3 — unchanged by this doc.

---

## Critical files

- `api/Concertable.Customer/Modules/Profile/` → renamed to `Modules/User/`, projects
  `Concertable.Customer.User.*` (P1).
- `.../User.Infrastructure/Data/UserDbContext.cs`,
  `.../User.Infrastructure/Events/UserRegistrationHandler.cs` (P1/P2).
- `.../User.Domain/` — `UserEntity.cs`, `PreferenceEntity.cs`,
  `GenrePreferenceEntity.cs` (P2/P3).
- **New** `Concertable.Customer.User.Contracts` — `Role` enum, `ICustomerUserModule`
  (P2/P3).
- `.../User.Api/Controllers/PreferenceController.cs` (P3).
- **New** `api/Concertable.Customer/Modules/Concert/.../Handlers/ConcertPostedNotificationHandler.cs`
  (P4).
- `api/Concertable.B2B/Modules/Concert/Concertable.Concert.Infrastructure/Services/ConcertService.cs`
  — drop `ICustomerModule` (P5).
- `api/Concertable.B2B/Modules/Customer/` — **deleted** (P5).
- `api/Modules/Customer/` — stale duplicate, **deleted** (P5).
- `Concertable.Customer.Web/Program.cs` — module composition, geometry, controllers
  (P1/P3/P4).
- `api/initial-migrations.ps1` — `ProfileDbContext` → `UserDbContext`, drop B2B
  `CustomerDbContext` (P1/P3/P6).
- `Concertable.Customer.slnx`, `Concertable.slnx`, `Concertable.B2B.slnx` — project
  membership (P1/P5).

## Conventions in effect

No additive migrations (re-scaffold via `initial-migrations.ps1`). No primary
constructors for services/repos/handlers/validators — explicit ctor,
`private readonly` fields, `this.field = param`, no `_` prefix. No comments in
DI/infra/config code, no fix-narrating comments. `.Web` = microservice host,
`.Api` = module layer; `.Contracts` for cross-module shapes. `.slnx` for solution
membership. Cross-module calls via `Contracts` only. Integration events raised from
domain events via `IPreCommitDomainEventHandler` + `IBus`, never service-layer
`eventBus.PublishAsync`. Read-model/projection entities set
`Property(x => x.Id).ValueGeneratedNever()`.
