# Concert Module Extraction — Implementation Plan

Successor to `VENUE_MODULE_REFACTOR.md` (Stage 1 complete 2026-04-22). Concert is the
biggest extraction yet: it absorbs ~13 entities, 4 keyed workflow strategies, 3 dispatchers,
5 repositories, and most of the nav chains Artist/Venue deferred. It's also the first
extraction where the facade story (`IConcertModule`) starts **empty** rather than driven by
existing cross-module callers — no module consumes Concert services today.

**Read this with `VENUE_MODULE_REFACTOR.md` and `ARTIST_MODULE_REFACTOR.md` open.** Anywhere
this doc says "as Venue §X" or "as Artist §X", those plans are canonical — don't re-derive.
`CONCERT_MODULE_REFACTOR_DRAFT.md` holds the original design notes around the Identity
cross-module query smell; those become concrete Stage 1 steps below.

Memory: `project_modular_monolith.md` tracks stage progress.

---

## Starting state (discovery, 2026-04-22)

A Concert module skeleton already exists at `api/Modules/Concert/` with three projects
(`Contracts`, `Domain`, `Infrastructure`). Content so far:

- `Concertable.Concert.Contracts/Events/ReviewSubmittedEvent.cs` — integration event consumed
  by Artist + Venue review projection handlers.
- `Concertable.Concert.Domain/Events/ReviewCreatedDomainEvent.cs` — raised by `ReviewEntity`.
- `Concertable.Concert.Infrastructure/Events/ReviewCreatedDomainEventHandler.cs` — translates
  the domain event to the integration event.
- `Concertable.Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — DI stub.

No `Application` project. No `Api` project. No `ConcertDbContext`. No controllers. All
Concert-domain code still lives in `Concertable.Core` / `.Application` / `.Infrastructure` /
`.Web`, and `ApplicationDbContext` still owns every Concert-related `DbSet`.

The skeleton is effectively a pre-claim on the namespace + the review-event plumbing that
Artist/Venue needed *before* Concert extracted. It's not a head-start on the work, but it is
the baseline — don't recreate the existing projects; extend them.

### What's Concert-scoped and moves

- `ConcertEntity`, `ConcertBookingEntity`, `ConcertGenreEntity`, `ConcertImageEntity`
- `OpportunityEntity`, `OpportunityApplicationEntity`, `OpportunityGenreEntity`
- `ContractEntity` (abstract TPT base) + `FlatFeeContractEntity`, `DoorSplitContractEntity`,
  `VersusContractEntity`, `VenueHireContractEntity`
- `TicketEntity`, `ReviewEntity`
- All EF configurations for the above (currently in
  `api/Concertable.Data.Infrastructure/Data/Configurations/` —
  `MiscEntityConfigurations.cs`, `OpportunityConfigurations.cs`, `ContractConfigurations.cs`,
  plus the Concert FK slices of `TransactionConfigurations.cs`)
- Enums: `ApplicationStatus`, `BookingStatus`, `ContractType`, `PaymentMethod`
- `DateRange` value object — **stays** in `Concertable.Shared.Domain` (already shared;
  Opportunity's `Period` owned property references it but other modules may reuse).

### What's Concert-adjacent but stays put (or leaves with another module)

- **Payment services** (`StripePaymentClient`, `OnSessionPaymentService`,
  `OffSessionPaymentService`, keyed `IManagerPaymentService`, `ICustomerPaymentService`,
  `IStripeAccountService`) — stay in `Concertable.Infrastructure/Services/Payment/`. Concert
  services inject them as legacy project refs during Stage 1; rewrites to `IPaymentModule`
  happen during the **Payment extraction**. Same pattern Venue used with its ticket payment
  service. See §3 (Payment coupling) below.
- **Messages** (`MessageEntity`, `MessageEntityConfiguration`) — not Concert-owned despite
  currently living in the same config file. Leave in `ApplicationDbContext` until the
  messaging/notifications module is scoped. Do **not** pull into `ConcertDbContext`.
- **Transactions** (`TicketTransactionEntity`, `SettlementTransactionEntity`) — belong to the
  future Payment module. Their configs have FKs *into* Concert (`ConcertId`, `BookingId`) but
  the entities themselves are Payment-scoped. Configs stay in `Concertable.Data.Infrastructure`;
  the FKs become plain primitives once Payment extracts.
- **Rating projection handlers** — `ArtistReviewProjectionHandler` /
  `VenueReviewProjectionHandler` already live in Artist/Venue Infrastructure. They consume
  `ReviewSubmittedEvent` from `Concert.Contracts` — which is the correct shape. No change.
- **`ArtistRatingRepository` / `VenueRatingRepository`** (legacy
  `api/Concertable.Infrastructure/Repositories/Rating/`) — read through `IReadDbContext.Reviews`.
  After Concert extracts, `Reviews` lives in `ConcertDbContext`. `IReadDbContext` still projects
  it via the cross-module read shim, so these repos keep working. Relocation / replacement is
  the rating-pipeline rewrite (MM_NORTH_STAR §5), not Stage 1.

### Pre-existing smells that Concert extraction fixes mid-flight

From `CONCERT_MODULE_REFACTOR_DRAFT.md`:

- `ArtistManagerRepository.GetByConcertIdAsync` and `GetByApplicationIdAsync` in
  `api/Modules/Identity/Concertable.Identity.Infrastructure/Repositories/` reach through
  `IReadDbContext.Concerts` to resolve an identity user from a concert id. Same shape exists
  on `VenueManagerRepository` (confirm during Step 3 of Stage 1). These methods disappear —
  callers in the new Concert module do the Concert→UserId hop locally, then call
  `IManagerModule.GetByIdAsync(userId)`. See §3 below.

---

## Stage 0 — Pre-extraction prep

Concert doesn't need a large Stage 0 (no shared abstraction to unwind, unlike Artist's
`IReviewable<TSelf>`). Prep items:

1. **Confirm no inbound Concert-service consumers remain.** Re-grep done 2026-04-22 turned
   up four hits the original audit missed. Decisions below:

   - **`Concertable.Workers` (Azure Functions host)** — `ConcertFinishedFunction` injects
     `IFinishedDispatcher` + `IConcertRepository`, and `Workers/ServiceCollectionExtensions.cs`
     re-registers the full keyed workflow-strategy + dispatcher block. Workers is a second
     composition root, not a module. **Resolution:** Workers stays at repo root and calls
     `AddConcertModule()` the same way Web does. Delete the duplicate Concert DI block from
     `Workers/ServiceCollectionExtensions.cs` as part of Step 12. Workers' csproj picks up a
     ref to `Concertable.Concert.Infrastructure` (+ any other modules it needs for the
     functions it hosts).
   - **`SettlementWebhookHandler`** (`api/Concertable.Infrastructure/Services/Webhook/`) —
     injects `ISettlementDispatcher`. If it stays in legacy Infrastructure, legacy Infra
     would reference Concert.Application internals (wrong direction). **Resolution:** move
     into `Concert.Infrastructure/Services/Webhook/` as part of Step 7. It's Concert's
     settlement entry point, triggered by a Stripe event — Payment's job is webhook plumbing,
     Settlement is Concert's follow-on. `ISettlementWebhookStrategy` registration in Web
     stays where it is; DI resolves cross-module fine.
   - **`DevController` + `E2EEndpointExtensions`** (`Concertable.Web`) — inject
     `IAcceptDispatcher`/`IFinishedDispatcher`. **Resolution:** grant
     `InternalsVisibleTo("Concertable.Web")` on `Concert.Application` as a temporary
     escape hatch. Annotate the grant in `AssemblyInfo.cs` as "for dev/E2E endpoints only —
     remove when those routes move into Concert.Api". Moving them is out of scope for
     Stage 1.
   - **Unit + integration test projects** — `Concertable.Infrastructure.UnitTests`,
     `Concertable.Infrastructure.IntegrationTests`, `Concertable.Workers.UnitTests` also
     reach Concert internals. **Resolution:** add all three to the
     `InternalsVisibleTo` list alongside `Concertable.Web.IntegrationTests` in
     `Concert.Application/AssemblyInfo.cs`.

2. **Snapshot the `IConcertWorkflowStrategy` + dispatcher wiring.** The keyed DI registration
   in `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` (lines ~265–304) is the
   most fragile part of this extraction. Copy the exact block into Step 11's `AddConcertModule()`
   before deleting from Web; don't re-derive from reading the types. **Workers mirrors the
   same keyed block** (`api/Concertable.Workers/ServiceCollectionExtensions.cs:68–73`) —
   that one gets deleted too in Step 12 and replaced with a single `AddConcertModule()` call.

   Snapshot captured 2026-04-22 — see appendix "DI snapshot" at bottom of this doc.

No files move during Stage 0.

---

## Stage 1 — Concert module extraction

### 1. Target architecture

```
api/Modules/Concert/
  Concertable.Concert.Contracts/       ← IConcertModule, integration events, public DTOs
  Concertable.Concert.Domain/          ← entities, domain events, enums, workflow-outcome marker types
  Concertable.Concert.Application/     ← services, repositories, dispatchers, workflow strategy ifaces, DTOs, validators, mappers
  Concertable.Concert.Infrastructure/  ← EF repos, workflow strategy impls, ConcertDbContext, handlers, AddConcertModule()
  Concertable.Concert.Api/             ← ConcertController, OpportunityController, OpportunityApplicationController, ContractController (all internal)
```

Project references:

- `Concert.Domain` → `Concertable.Shared.Domain`, `Concertable.Artist.Domain`,
  `Concertable.Venue.Domain` (for `OpportunityApplicationEntity.Artist` +
  `OpportunityEntity.Venue` navs — same tolerated escape-hatch pattern `Concertable.Core`
  uses today per CLAUDE.md rule 3). Retires when read-models replace entity refs.
- `Concert.Contracts` → `Concert.Domain`, `Concertable.Shared.Contracts`.
- `Concert.Application` → `Concert.Contracts`, `Concert.Domain`, `Identity.Contracts`,
  `Artist.Contracts`, `Venue.Contracts`, `Search.Contracts`, `Concertable.Shared.*`,
  `Concertable.Data.Application` (for `IReadDbContext`).
- `Concert.Infrastructure` → `Concert.Application`, `Concertable.Data.Infrastructure` (for
  `DbContextBase` + shared entity configs), `Concertable.Shared.Infrastructure`.
  **Temporary legacy ref:** `Concertable.Infrastructure` for payment services until Payment
  extracts. This is the only accepted cross-extraction-boundary ref in this plan — see §3.
- `Concert.Api` → `Concert.Application`, `Concert.Infrastructure`.
- `Concertable.Core` → `Concertable.Concert.Domain` — keeps `OpportunityApplicationEntity.Artist`
  and `OpportunityEntity.Venue` navs compilable during the transition. Dropped when Core is
  fully retired.
- `Concertable.Web` → `Concert.Api` + `Concert.Infrastructure` (ApplicationPart + DI).

### 2. Files to move

#### Entities → `Concert.Domain/`

From `api/Concertable.Core/Entities/`:
- `ConcertEntity.cs`
- `ConcertBookingEntity.cs`
- `ConcertGenreEntity.cs`
- `ConcertImageEntity.cs`
- `OpportunityEntity.cs`
- `OpportunityApplicationEntity.cs`
- `OpportunityGenreEntity.cs`
- `TicketEntity.cs`
- `ReviewEntity.cs`

From `api/Concertable.Core/Entities/Contracts/`:
- `ContractEntity.cs` (abstract)
- `FlatFeeContractEntity.cs`
- `DoorSplitContractEntity.cs`
- `VersusContractEntity.cs`
- `VenueHireContractEntity.cs`

From `api/Concertable.Core/Enums/`:
- `ApplicationStatus.cs`, `BookingStatus.cs`, `ContractType.cs`, `PaymentMethod.cs`

Namespace: `Concertable.Concert.Domain.*` from the start. Do **not** leave
`Concertable.Core.Entities` on moved files (Identity lesson, `IDENTITY_COMPLETION.md §4`).

**Nav properties — keep as cross-module Domain refs (CLAUDE.md rule 3 escape hatch):**
- `OpportunityApplicationEntity.Artist` → Artist module. `Concert.Domain → Artist.Domain` ref.
- `OpportunityEntity.Venue` → Venue module. `Concert.Domain → Venue.Domain` ref.
- These mirror the refs `Concertable.Core → Artist.Domain`/`Venue.Domain` that already exist
  today. They retire together when read-models replace entity refs (MM_NORTH_STAR §Corollary 5),
  not during Concert Stage 1.

**Nav properties to drop:**
- `TicketEntity.User` — drop nav, keep `UserId` (Guid). Identity lookups via `IManagerModule`.
- `ConcertEntity.LocationExpression` — **DELETE.** Stale code. Concert now has its own
  `Location` column (Location Refactor Plan landed Location denormalization onto Concert
  directly). The expression `c => c.Booking.Application.Opportunity.Venue.Location` is dead
  weight — replace every usage with `c.Location`.

**Nav properties to keep (intra-Concert):**
- `ConcertEntity.Booking`, `ConcertEntity.Tickets`, `ConcertEntity.ConcertGenres`,
  `ConcertEntity.Images`
- `ConcertBookingEntity.Application`, `ConcertBookingEntity.Concert`
- `OpportunityEntity.Applications`, `OpportunityEntity.OpportunityGenres`,
  `OpportunityEntity.Contract`
- `OpportunityApplicationEntity.Opportunity`, `OpportunityApplicationEntity.Booking`
- `TicketEntity.Concert`, `ReviewEntity.Ticket`
- `ContractEntity.Opportunity`

#### Domain events → `Concert.Domain/Events/`

Already landed:
- `ReviewCreatedDomainEvent.cs` (present)

New (introduce only if a Stage 1 handler needs them — do **not** speculatively add):
- None required for Stage 1. Integration-event surface (`ConcertCreated`, `BookingConfirmed`,
  `OpportunityApplicationAccepted`) is deferred to the Payment extraction, when another module
  actually needs to react.

#### Application → `Concert.Application/`

**Interfaces** (from `api/Concertable.Application/Interfaces/Concert/` and
`Interfaces/Repositories/`):
- `IConcertService`, `IConcertDraftService`
- `IOpportunityService`
- `IOpportunityApplicationService`
- `IContractService`
- `IUpfrontConcertService`, `IDeferredConcertService`
- `IAcceptDispatcher`, `IFinishedDispatcher`, `ISettlementDispatcher`,
  `ITicketPaymentDispatcher`
- `IContractStrategy`, `IConcertWorkflowStrategy`, `ITicketPaymentStrategy`
- `IConcertValidator`, `IOpportunityApplicationValidator`
- `IConcertRepository`, `IOpportunityRepository`, `IOpportunityApplicationRepository`,
  `IContractRepository`, `IConcertBookingRepository`, `ITicketRepository`,
  `IReviewRepository`

**DTOs / Requests / Responses** (from `api/Concertable.Application/DTOs/`, `Requests/`,
`Responses/`):
- `ConcertDtos.cs` (`ConcertDto`, `ConcertVenueDto`, `ConcertArtistDto`, `ConcertSummaryDto`,
  `ConcertSnapshot`)
- `OpportunityDtos.cs`, `OpportunityApplicationDtos.cs`, `ContractDtos.cs`
- `ConcertRequests.cs`, `OpportunityRequests.cs`
- `ConcertResponses.cs`, `WorkflowOutcomes.cs` (`IAcceptOutcome`/`ImmediateAcceptOutcome`/
  `DeferredAcceptOutcome`, `IFinishOutcome`/`ImmediateFinishOutcome`/`DeferredFinishOutcome`)

**Validators** (from `api/Concertable.Application/Validators/`):
- `ConcertValidators.cs`, `OpportunityValidators.cs`,
  `OpportunityApplicationRequestValidator` (if present), plus any ticket/review validators.

**Mappers** (from `api/Concertable.Application/Mappers/`):
- `ConcertMappers.cs`, `OpportunityMapper.cs`, `OpportunityApplicationMapper.cs`,
  `ContractMapper.cs` (+ per-type: `FlatFee/DoorSplit/Versus/VenueHire`),
  `ConcertTransactionMapper.cs`, `TicketMappers.cs`, `TicketTransactionMapper.cs`.

Namespace: `Concertable.Concert.Application.*`. Everything `internal`; add
`Concert.Application/AssemblyInfo.cs` with:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
// TEMPORARY: DevController + E2EEndpointExtensions inject IAcceptDispatcher/IFinishedDispatcher
// directly. Remove when those routes move into Concert.Api. See CONCERT_MODULE_REFACTOR.md §Stage 0.
[assembly: InternalsVisibleTo("Concertable.Web")]
```

Matches the Venue precedent at
`api/Modules/Venue/Concertable.Venue.Application/AssemblyInfo.cs`.

#### Infrastructure → `Concert.Infrastructure/`

**Services** (from `api/Concertable.Infrastructure/Services/`):
- `ConcertService`, `ConcertDraftService`
- `OpportunityService`
- `OpportunityApplicationService`
- `ContractService`
- `UpfrontConcertService`, `DeferredConcertService`
- `AcceptDispatcher`, `FinishedDispatcher`, `SettlementDispatcher`, `TicketPaymentDispatcher`
- `ApplicationAcceptHandler` (and any sibling handlers invoked by dispatchers)
- `SettlementWebhookHandler` (from `Services/Webhook/`) — implements `ISettlementWebhookStrategy`
  but injects `ISettlementDispatcher`; it's Concert's settlement entry point, not Payment's.
  Moves to `Concert.Infrastructure/Services/Webhook/` as part of Step 7. Stage 0 resolution.

**Workflow strategy implementations** (from
`api/Concertable.Infrastructure/Services/Application/`):
- `FlatFeeConcertWorkflow`, `VenueHireConcertWorkflow`,
  `DoorSplitConcertWorkflow`, `VersusConcertWorkflow`

**Repositories** (from `api/Concertable.Infrastructure/Repositories/Concert/` +
`Repositories/`):
- `ConcertRepository`, `OpportunityRepository`, `OpportunityApplicationRepository`,
  `ContractRepository`, `ConcertBookingRepository`
- `TicketRepository`, `ReviewRepository` (whichever Concert-owned review repo still lives in
  legacy Infrastructure — confirm at impl time)

All base-classed to `IntModuleRepository<TEntity, ConcertDbContext>` (int-keyed — Concert
entities use `int Id`).

**New files:**
- `Concert.Infrastructure/Data/ConcertDbContext.cs` — inherits `DbContextBase`. DbSets listed
  in §5 below.
- `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — `AddConcertModule()`.
  The existing `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` (which
  currently registers only the review handler) absorbs the full DI block from §6.

**EF entity configurations stay put** at
`api/Concertable.Data.Infrastructure/Data/Configurations/` per CLAUDE.md database rules:
- `MiscEntityConfigurations.cs` — `ConcertEntityConfiguration`,
  `ConcertGenreEntityConfiguration`, `ConcertImageEntityConfiguration`,
  `TicketEntityConfiguration` (+ `MessageEntityConfiguration` stays — not Concert).
- `OpportunityConfigurations.cs` — `OpportunityEntityConfiguration`,
  `OpportunityApplicationEntityConfiguration`, `ConcertBookingEntityConfiguration` (+
  `OpportunityGenreEntityConfiguration`).
- `ContractConfigurations.cs` — all five contract configs.
- `ReviewEntityConfiguration` (wherever it currently lives) stays.

`ConcertDbContext` applies them explicitly in `OnModelCreating` (same pattern as
`VenueDbContext`).

#### Controllers → `Concert.Api/`

From `api/Concertable.Web/Controllers/`:
- `ConcertController.cs`
- `OpportunityController.cs`
- `OpportunityApplicationController.cs`
- `ContractController.cs`
- `TicketController.cs` (if present — confirm at impl time)
- `ReviewController.cs` (if present — confirm at impl time)

Make all controllers `internal`; add `InternalControllerFeatureProvider` in `Concert.Api`
(Venue/Artist/Identity/Search precedent).

Routes preserved verbatim. Controllers inject internal Application services
(`IConcertService` etc.) via the `InternalsVisibleTo` grant in Step 2's `AssemblyInfo.cs`.
Do **not** route through `IConcertModule` — Api is same-module.

Notification/SignalR wiring (`IConcertNotificationService`, `IConcertPostedHandler`) moves
with `ConcertController`. Confirm those types are Concert-scoped during impl; if they
straddle modules, keep the hub itself in Web and inject an interface.

#### Seeding

- `api/Concertable.Seeding/Fakers/` — any `ConcertFaker`, `OpportunityFaker`,
  `ContractFaker`, `TicketFaker`, `ReviewFaker` stay in `Concertable.Seeding` (cross-module
  seed data, Venue/Artist precedent).
- **New:** `ConcertDevSeeder` + `ConcertTestSeeder` in `Concert.Infrastructure/Seeding/`.
  Precondition for Step 14 (ApplicationDbContext cleanup) — the per-module seeder pattern
  MUST land before DbSets can be removed from `ApplicationDbContext`. Artist/Venue both had
  to pull this into scope mid-flight; don't repeat that mistake. See
  `project_seeding_architecture.md`.

### 3. Cross-module coupling

#### Design direction (2026-04-22, post-Step-2 revision): owned event-driven read models (MM_NORTH_STAR §5)

**Superseding earlier drafts.** The draft plan tolerated cross-module EF nav props pointing
at `Artist.Domain` / `Venue.Domain` types. **Final decision:** Concert owns its own
`ArtistReadModel` + `VenueReadModel` tables, populated from Artist/Venue integration events.
Standard MM_NORTH_STAR §5 projection pattern. Concert.Domain drops its refs to Artist.Domain
+ Venue.Domain.

**The shape:**

- `ArtistReadModel` lives in `Concert.Domain`. Fields Concert actually reads: `Id`, `UserId`,
  `Name`, `Avatar` (confirm at impl time; trim aggressively).
- `VenueReadModel` lives in `Concert.Domain`. Same minimal field set.
- **Concert-owned tables** — `DbSet<ArtistReadModel>` + `DbSet<VenueReadModel>` on
  `ConcertDbContext`, Concert's `InitialCreate` migration (Step 14) creates
  `ArtistReadModels` + `VenueReadModels` as real tables. **NO `ExcludeFromMigrations`** —
  Concert owns these tables outright.
- **Populated by integration events** — `ArtistChangedIntegrationEvent` from
  `Artist.Contracts/Events/`, `VenueChangedIntegrationEvent` from `Venue.Contracts/Events/`.
  Artist/Venue services emit on create + update. Concert-side handlers
  (`ArtistReadModelProjectionHandler`, `VenueReadModelProjectionHandler`) upsert.
- **Concert.Domain has ZERO refs to Artist.Domain or Venue.Domain** — the read-model classes
  are Concert's own types. `Concert.Domain.csproj` drops both `ProjectReference` entries
  added in Step 2.
- **Nav prop retyping:** `OpportunityApplicationEntity.Artist : ArtistEntity` →
  `OpportunityApplicationEntity.Artist : ArtistReadModel` (same nav name, new type pointing at
  Concert's own table). Same for `OpportunityEntity.Venue` → `VenueReadModel`. `ArtistId` /
  `VenueId` FK columns now FK into Concert's own `ArtistReadModels` / `VenueReadModels` tables,
  not Artist's / Venue's tables.
- **Backfill** — `ConcertDevSeeder`/`ConcertTestSeeder` (Step 10) seed read-model rows from
  `SeedData`. Production backfill decided at deployment time.

**Why owned, not `ExcludeFromMigrations` over source tables:** Concert has write paths
(workflows, bookings, applications) that join against Artist/Venue data in transactional
contexts. An owned projection table keeps those joins within ConcertDbContext's transactional
scope, isolates Concert from Artist/Venue schema migrations, and preserves the "could extract
to microservice" property. `ExcludeFromMigrations` over Artist's `Artists` table would
re-introduce cross-module schema coupling inside Concert's workflows — that's what
MM_NORTH_STAR §5 explicitly rejects for consumer modules that own writes.

**Where `ExcludeFromMigrations` DOES apply — Search only.** Search is the structural
exception per MM_NORTH_STAR §5 ("Search is the exception"): it owns no write tables, no
workflows, no transactional joins. `SearchDbContext` can be pure `ExcludeFromMigrations`
mappings over source tables because there's no owned side to maintain. Not the pattern for
Concert.

**Step 2 partial revert required** (Step 2 shipped before this decision landed):
- Remove both `ProjectReference` lines (`Concert.Domain → Artist.Domain` + `→ Venue.Domain`)
  from `api/Modules/Concert/Concertable.Concert.Domain/Concertable.Concert.Domain.csproj`.
- `OpportunityApplicationEntity.Artist : ArtistEntity` → `: ArtistReadModel` (new class in
  Concert.Domain).
- `OpportunityEntity.Venue : VenueEntity` → `: VenueReadModel` (same).
- `using Concertable.Artist.Domain;` / `using Concertable.Venue.Domain;` lines inside
  `Concert.Domain/Entities/*.cs` get removed.

**What stays in §3 below** as historical context on `IReadDbContext` + Identity cross-module
methods — still applies; Step 7 rewiring reads `application.Artist.UserId` (where `Artist`
is Concert's owned `ArtistReadModel` instance, populated from Artist events) to feed
`managerModule.GetByIdAsync(userId)`. Two round trips (Concert's aggregate query + Identity
PK lookup), not three.

#### `IReadDbContext` usage — Concert extraction eliminates all non-Search consumers

After Stage 1 lands, the only remaining `IReadDbContext` consumer in the repo is **Search**
— which is the legitimate, long-term workaround until Search migrates to a dedicated
`SearchDbContext` with per-module-emitted view models (MM_NORTH_STAR §Corollary 5).

The three non-Search consumers that exist today all go away during this extraction:
- **Identity repos** (`ArtistManagerRepository.GetByConcertIdAsync`/`GetByApplicationIdAsync`,
  Venue equivalent) — foreign-noun smell from the original draft. Deleted in Step 3;
  callers fixed in Step 7.
- **Legacy `OpportunityRepository`** in `Concertable.Infrastructure` — moves into
  `Concert.Infrastructure` in Step 7 and reads its own `ConcertDbContext` instead.
- **Legacy `UserRepository`** cross-module reads — already Identity-internal, not a Concert
  concern but worth noting the shim usage shrinks to Search-only post-extraction.

**Follow-up (not Stage 1):** Search's header repositories currently chain through entity
navs (`a.User.*`, `v.Location`, `c.Booking.Application.Opportunity.Venue.*`). Location
chasing is already resolvable via `IHasLocation` — Artist/Venue/Concert all implement it
(Location Refactor Plan). Rewriting Search's location-filter specs to generic
`IQueryable<T> where T : IHasLocation` drops the nav chain substitution entirely. Not in
Concert Stage 1 scope — lift during the Search view-model migration.

#### Fix now (Concert outbound — rewrite as Contracts facade calls)

- `ConcertService`, `OpportunityService`, all four workflow strategies currently inject
  `IManagerModule` (Identity Contracts). Already correct shape — leaves as-is.
- `ConcertService` injects `ISearchModule` for index updates. Correct shape — leaves as-is.
- `OpportunityApplicationService` injects `IArtistModule` + `IVenueModule` for
  application-detail projections. Correct shape.
- **Delete `IManagerRepository.GetByConcertIdAsync` + `GetByApplicationIdAsync`** in
  `api/Modules/Identity/Concertable.Identity.Infrastructure/Repositories/ArtistManagerRepository.cs`
  + `VenueManagerRepository` + `UserRepository.GetByConcertIdAsync/GetByApplicationIdAsync` +
  `UserService` equivalents + the four `IManagerModule.GetXManagerByConcertId/ApplicationIdAsync`
  facade methods on `IdentityModule.cs`. `IManagerModule` shrinks to **`GetByIdAsync(Guid userId)`**
  only (no `GetManagerAsync`). `ArtistManagerRepository`/`VenueManagerRepository` drop
  the `IReadDbContext readDb` ctor param entirely (Identity stops reaching into Concert
  tables full stop). **Concert-side callers lookup `UserId` from the aggregate read-model
  nav** (see §3 Read models, below) — `application.ArtistReadModel.UserId`,
  `opportunity.VenueReadModel.UserId` — then call `managerModule.GetByIdAsync(userId)`.
  Total round trips per manager lookup: 2 (caller's existing aggregate query + Identity PK
  lookup) vs today's 3 (caller query + Identity's chain re-walk via `IReadDbContext` + PK
  lookup). Executes as callers migrate in Step 7, not a big-bang rewrite.

#### Defer to Payment extraction (Concert outbound — temporary legacy ref)

- `UpfrontConcertService`, `DeferredConcertService`, `CustomerPaymentService`,
  `TicketPaymentDispatcher` inject `IManagerPaymentService` (keyed `onSession`/`offSession`),
  `ICustomerPaymentService`, `IStripeAccountService`, `IStripeWebhookHandler` etc. These
  stay as **legacy project refs** — `Concert.Infrastructure` references
  `Concertable.Infrastructure` specifically for this. Rewrites to an `IPaymentModule` facade
  land during Payment extraction (mirrors Venue's handling of `VenueTicketPaymentService`).
  Flag this as the one accepted cross-extraction-boundary reference and gate removal on
  Payment Stage 1.

#### Inbound coupling — non-module hosts + one webhook

No Artist/Venue/Identity/Search code calls `IConcertService`/`IOpportunityService`/other
Concert services today. Search reads concert tables via `IReadDbContext`, not services. That
stays. When foreign *module* callers appear, they land on `IConcertModule` (Contracts) — not
direct Application refs.

Non-module consumers the Stage 0 audit found (see §Stage 0 for resolutions):
- `Concertable.Workers` — second composition host; calls `AddConcertModule()` post-extraction.
- `SettlementWebhookHandler` — moves into `Concert.Infrastructure` (it's a Concert entry point).
- `Concertable.Web.DevController` + `E2EEndpointExtensions` — temporary
  `InternalsVisibleTo("Concertable.Web")` grant on `Concert.Application` until those routes
  migrate into `Concert.Api`.

#### `ConcertEntity.LocationExpression` — delete

Concert has its own `Location` column (Location Refactor Plan). The
`c => c.Booking.Application.Opportunity.Venue.Location` expression is stale — replace with
`c.Location` at every call site. No denormalization work needed; it's already been done.

### 4. `IConcertModule` — Contracts surface

**Start empty.** No module consumes Concert services today. Ship the interface + registration
stub so the module shape is complete, but don't speculate on methods:

```csharp
namespace Concertable.Concert.Contracts;

public interface IConcertModule
{
    // Intentionally empty at Stage 1. Add cross-module lookups only when a
    // foreign caller proves need. See CLAUDE.md facade naming rule and
    // feedback_module_facade_surface.md.
}
```

`ConcertModule` implementation in `Concert.Infrastructure/` is a stub that matches. Expect
the first real method to land during **Payment extraction** — Payment likely needs
`GetBookingAsync(int bookingId)` or similar for settlement triggers.

Do not put `CreateAsync`/`UpdateAsync`/`PostAsync`/workflow dispatch on `IConcertModule`.
Those are controller-facing and exposed via internal `IConcertService` etc.

#### 4b. Likely additional Contracts facades (split by concern, not one fat interface)

Per CLAUDE.md ("A module can expose multiple facades — don't force everything through one fat
`IXModule`"), Concert has at least three cross-module entry points that warrant separate
facades in `Concert.Contracts`. None of these ship in Stage 1 — they crystallise during the
consuming extraction (mostly Payment) — but flag them now so we don't retroactively widen
`IConcertModule` into a god interface.

**Candidates:**

- **`IConcertLifecycleModule`** (or `IConcertWorkflowModule`) — cross-module triggers for the
  accept/settle/finish lifecycle. Backed internally by `AcceptDispatcher` /
  `SettlementDispatcher` / `FinishedDispatcher` which resolve the keyed
  `IConcertWorkflowStrategy` per `ContractType`. Likely methods:
  - `SettleAsync(int bookingId)` — Payment webhook handler invokes this after Stripe settlement
    clears (replaces the direct `ISettlementDispatcher` ref in today's `SettlementWebhookHandler`).
  - `FinishAsync(int concertId)` — `Concertable.Workers.ConcertFinishedFunction` invokes this
    on the hourly timer (replaces the direct `IFinishedDispatcher` ref). Works Workers stays
    a composition host but stops reaching into Concert internals.
  - `AcceptApplicationAsync(int applicationId, string? paymentMethodId)` — only if a foreign
    module triggers accept (today it's Concert's own controller; don't surface speculatively).

- **`IConcertTicketPaymentModule`** — backed by `TicketPaymentDispatcher`. Payment webhook or
  a future ticketing module calls this to drive the keyed `ITicketPaymentStrategy`
  (`VenueTicketPaymentService` / `ArtistTicketPaymentService`). Method: `PayAsync(concertId,
  quantity, paymentMethodId, price)` returning `Result<PaymentResponse>`. Only promote if
  the caller is outside Concert; today every caller is Concert-internal.

- **`IConcertModule`** — plain lookups. `GetBookingAsync(int bookingId)`,
  `GetConcertSummaryAsync(int concertId)` style reads. Mirrors Identity's `IIdentityModule`
  and Venue's `IVenueModule`.

**The dispatchers themselves stay `internal` to `Concert.Application`.** They are orchestrators,
not a stable cross-module surface — the facade is the method a foreign caller invokes, and the
dispatcher is the implementation it hides. `IConcertWorkflowStrategy` also stays internal (it's
keyed-DI plumbing, not a contract anyone else calls).

**Decision rule:** don't create any of these facades until a foreign caller proves need. The
current direct-dispatcher consumers (`SettlementWebhookHandler`, `ConcertFinishedFunction`,
`DevController`, `E2EEndpointExtensions`) all get the `InternalsVisibleTo` escape hatch in
Stage 1 (§Stage 0 resolutions). They rewrite to Contracts facades in follow-up work:
- `SettlementWebhookHandler` → `IConcertLifecycleModule.SettleAsync` during Payment extraction.
- `ConcertFinishedFunction` → same facade; Workers drops its direct `IFinishedDispatcher` ref.
- `DevController`/`E2EEndpointExtensions` → either migrate into `Concert.Api` (kills the
  `InternalsVisibleTo("Concertable.Web")` grant) or call through the facade when they
  survive as dev/test tooling.

### 5. `ConcertDbContext`

Inherits `DbContextBase`. Owns every DbSet the Concert module moves out of
`ApplicationDbContext`:

```csharp
internal class ConcertDbContext(DbContextOptions<ConcertDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<ConcertBookingEntity> ConcertBookings => Set<ConcertBookingEntity>();
    public DbSet<ConcertGenreEntity> ConcertGenres => Set<ConcertGenreEntity>();
    public DbSet<ConcertImageEntity> ConcertImages => Set<ConcertImageEntity>();
    public DbSet<OpportunityEntity> Opportunities => Set<OpportunityEntity>();
    public DbSet<OpportunityApplicationEntity> OpportunityApplications => Set<OpportunityApplicationEntity>();
    public DbSet<OpportunityGenreEntity> OpportunityGenres => Set<OpportunityGenreEntity>();
    public DbSet<ContractEntity> Contracts => Set<ContractEntity>();
    public DbSet<FlatFeeContractEntity> FlatFeeContracts => Set<FlatFeeContractEntity>();
    public DbSet<DoorSplitContractEntity> DoorSplitContracts => Set<DoorSplitContractEntity>();
    public DbSet<VersusContractEntity> VersusContracts => Set<VersusContractEntity>();
    public DbSet<VenueHireContractEntity> VenueHireContracts => Set<VenueHireContractEntity>();
    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConcertEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertBookingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertImageEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityApplicationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OpportunityGenreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ContractEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FlatFeeContractEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DoorSplitContractEntityConfiguration());
        modelBuilder.ApplyConfiguration(new VersusContractEntityConfiguration());
        modelBuilder.ApplyConfiguration(new VenueHireContractEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TicketEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewEntityConfiguration());
    }
}
```

**Cross-module FKs:**
- `OpportunityApplicationEntity.ArtistId` (int) + `Artist` nav kept (Domain escape hatch).
- `OpportunityEntity.VenueId` (int) + `Venue` nav kept (Domain escape hatch).
- `TicketEntity.UserId` (Guid) — plain primitive, no nav, no FK constraint.

Migration history table: **default `__EFMigrationsHistory`** (shared with
Identity/Artist/Venue/Application). See `feedback_shared_migrations_history_table.md`.

### 6. `AddConcertModule()` DI wiring

Target shape (quoting Venue's pattern, expanded for Concert's surface):

```csharp
public static IServiceCollection AddConcertModule(this IServiceCollection services, IConfiguration cfg)
{
    services.AddDbContext<ConcertDbContext>((sp, opts) =>
        opts.UseSqlServer(cfg.GetConnectionString("DefaultConnection"),
                sql => sql.UseNetTopologySuite())
            .AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>()));

    // Services
    services.AddScoped<IConcertService, ConcertService>();
    services.AddScoped<IConcertDraftService, ConcertDraftService>();
    services.AddScoped<IOpportunityService, OpportunityService>();
    services.AddScoped<IOpportunityApplicationService, OpportunityApplicationService>();
    services.AddScoped<IContractService, ContractService>();
    services.AddScoped<IUpfrontConcertService, UpfrontConcertService>();
    services.AddScoped<IDeferredConcertService, DeferredConcertService>();

    // Dispatchers
    services.AddScoped<IAcceptDispatcher, AcceptDispatcher>();
    services.AddScoped<IFinishedDispatcher, FinishedDispatcher>();
    services.AddScoped<ISettlementDispatcher, SettlementDispatcher>();
    services.AddScoped<ITicketPaymentDispatcher, TicketPaymentDispatcher>();
    services.AddScoped<IApplicationAcceptHandler, ApplicationAcceptHandler>();

    // Keyed workflow strategies (order matters — keys must match ContractType values exactly)
    services.AddKeyedScoped<IConcertWorkflowStrategy, FlatFeeConcertWorkflow>(ContractType.FlatFee);
    services.AddKeyedScoped<IConcertWorkflowStrategy, DoorSplitConcertWorkflow>(ContractType.DoorSplit);
    services.AddKeyedScoped<IConcertWorkflowStrategy, VersusConcertWorkflow>(ContractType.Versus);
    services.AddKeyedScoped<IConcertWorkflowStrategy, VenueHireConcertWorkflow>(ContractType.VenueHire);

    // Repositories
    services.AddScoped<IConcertRepository, ConcertRepository>();
    services.AddScoped<IOpportunityRepository, OpportunityRepository>();
    services.AddScoped<IOpportunityApplicationRepository, OpportunityApplicationRepository>();
    services.AddScoped<IContractRepository, ContractRepository>();
    services.AddScoped<IConcertBookingRepository, ConcertBookingRepository>();
    services.AddScoped<ITicketRepository, TicketRepository>();
    services.AddScoped<IReviewRepository, ReviewRepository>();

    // Module facade (empty for Stage 1 — see §4)
    services.AddScoped<IConcertModule, ConcertModule>();

    // Domain event → integration event translator (already present in the existing stub)
    services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();

    services.AddValidatorsFromAssemblyContaining<OpportunityDtoValidator>();

    return services;
}

public static IServiceCollection AddConcertDevSeeder(this IServiceCollection services)
{
    services.AddScoped<IDevSeeder, ConcertDevSeeder>();
    return services;
}

public static IServiceCollection AddConcertTestSeeder(this IServiceCollection services)
{
    services.AddScoped<ITestSeeder, ConcertTestSeeder>();
    return services;
}
```

Remove from `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` (lines ~180–304):
- All service registrations for `IConcertService`, `IConcertDraftService`,
  `IOpportunityService`, `IOpportunityApplicationService`, `IContractService`,
  `IUpfrontConcertService`, `IDeferredConcertService`.
- All dispatcher + `ApplicationAcceptHandler` registrations.
- All four keyed `IConcertWorkflowStrategy` registrations.
- All Concert/Opportunity/OpportunityApplication/Contract/ConcertBooking/Ticket/Review
  repository registrations.

**Keep in legacy Web** (these are payment-module scoped, not Concert):
- `IManagerPaymentService` keyed registrations (`onSession`/`offSession`).
- `ICustomerPaymentService`, `IStripeAccountService`, Stripe-client wiring.
- `IConcertNotificationService` (SignalR hub) — stays in Web until notifications/messaging
  module is scoped.

### 7. `ApplicationDbContext` cleanup + migration scaffolding

Remove from `ApplicationDbContext`:
- All 14 DbSets enumerated in §5.
- All `modelBuilder.ApplyConfiguration(...)` for those entity configs.

Keep `ExcludeFromMigrations` markers for the entities (where still referenced by foreign navs
like `OpportunityEntity.Venue` in Venue's context) until the drop migration runs cleanly.

Migration sequence (mirrors Venue §7):
1. **Scaffold `ConcertDbContext` `InitialCreate`** — creates all 14 tables + their indexes +
   TPT contract tables.
2. **Scaffold `ApplicationDbContext` migration that drops the 14 tables.** EF generates the
   `DropTable` calls automatically when the DbSets are removed.
3. **Runtime order:** Identity (0) → Artist (1) → Venue (2) → **Concert (3)** →
   `ApplicationDbContext` (last). `DevDbInitializer` + `TestDbInitializer` need
   `ConcertDbContext` added to their migration orchestration list.

#### Cross-context FK gotcha — **applies here**

`ConcertDbContext` will have FKs into:
- `Concertable.Shared.Domain.GenreEntity` via `ConcertGenreEntity.GenreId` and
  `OpportunityGenreEntity.GenreId`. `GenreEntity` lives in `ApplicationDbContext` (shared
  reference data). **Concert runs before ApplicationDbContext**, so these FKs fail. Strip
  `table.ForeignKey(...)` calls for `FK_ConcertGenres_Genres_GenreId` and
  `FK_OpportunityGenres_Genres_GenreId` from the Concert `InitialCreate` migration. Same
  pattern as `FK_ArtistGenres_Genres_GenreId` in Artist. See `feedback_cross_context_fk.md`.

`ConcertDbContext` will have FKs into Artist/Venue tables because the navs are kept
(§2). Those FKs appear in the Concert `InitialCreate` migration pointing at Artist/Venue
tables owned by other contexts. Concert runs after Identity (0)/Artist (1)/Venue (2) so the
principal tables exist — but per `feedback_cross_context_fk.md`, cross-context FKs are still
managed by stripping them from the module migration and relying on app-level referential
integrity. Strip `FK_OpportunityApplications_Artists_ArtistId` and
`FK_Opportunities_Venues_VenueId` from the Concert migration. Keep the CLR navs for
`.Include()` projection.

**Inbound FKs to Concert tables from `ApplicationDbContext`:**
- `TicketTransactionEntity.ConcertId` → `Concerts`
- `SettlementTransactionEntity.BookingId` → `ConcertBookings`

These live in `ApplicationDbContext`'s migration (AppDb runs last). Principal tables
(`Concerts`, `ConcertBookings`) exist by the time AppDb migrates. **Leave these FKs in place**
— same pattern Venue §7 followed. They become plain primitives when Payment extracts.

### 8. Integration tests

- Existing Concert-related tests live under
  `api/Tests/Concertable.Web.IntegrationTests/Controllers/` and similar. Keep folder
  locations (Artist/Venue precedent).
- `TestDbInitializer` needs updating to seed via `IConcertTestSeeder` instead of direct
  `context.Concerts.Add(...)` etc. This is non-trivial — Concert has the most seed data of
  any module. Budget time for it.
- **Respawner** — confirm `TablesToIgnore = ["__EFMigrationsHistory"]` still covers Concert's
  use of the default migration history table.
- Full integration suite must pass: Concert/Opportunity/OpportunityApplication/Contract/
  Ticket/Review controller tests, plus anything that indirectly exercises the workflow
  strategies (application acceptance, concert lifecycle, settlement).

### 9. Implementation order

This is the biggest extraction so far — **19 steps** with a few deliberate checkpoints
(original 17 + 2 new: Step 2b introduces the owned `ArtistReadModel`/`VenueReadModel` classes
and drops Concert.Domain's refs to Artist/Venue Domain; Step 6b wires integration events +
projection handlers).

1. **Scaffold `Concert.Application` + `Concert.Api` projects** under `api/Modules/Concert/`
   (Contracts, Domain, Infrastructure already exist). Add to `Concertable.sln`. **Hand-edit
   `Concertable.sln`** to reparent under the existing `Modules/Concert` folder — the
   `dotnet sln add --solution-folder` duplicate-folder bug from Venue Step 1 will recur. See
   `feedback_sln_solution_folder_duplicate.md`.

   ✅ **Done 2026-04-22.** Projects created:
   - `Concertable.Concert.Application` (GUID `{A8D9E6F2-4B3C-4A5E-B7C1-1F2E3D4C5B6A}`) — refs
     Contracts/Domain + Identity/Artist/Venue Contracts + Shared Contracts/Validation,
     FluentResults, FluentValidation. `AssemblyInfo.cs` grants `InternalsVisibleTo` for
     Concert.Infrastructure, Concert.Api, Web.IntegrationTests.
   - `Concertable.Concert.Api` (GUID `{B9E0F7A3-5C4D-5B6F-C8D2-2A3B4C5D6E7F}`) — refs
     Contracts, Application, Infrastructure.
   - `GlobalUsings.cs` (Api) trimmed to `Shared` + `Concert.Contracts` for now — Application
     namespace imports (`DTOs`/`Interfaces`/`Requests`) re-added in Step 5 once those
     namespaces exist. Sln hand-edit nested both under existing `Modules/Concert` folder
     (GUID `{510D9D4B-BCFF-D8B0-A69C-AC2EAD029501}`), no duplicate folder. Full solution
     builds 0 errors.

2. **Move entities + enums** → `Concert.Domain/`. Update namespaces. Drop `Ticket.User` nav
   (keep `UserId` Guid). Keep `OpportunityApplication.Artist` + `Opportunity.Venue` navs
   (cross-module Domain refs per §2). **Delete `ConcertEntity.LocationExpression`** and
   replace all callers with `c.Location` (Concert owns its own Location column now). Inverse
   navs elsewhere already dropped during Artist/Venue extractions — nothing more to flip.

   > **Nav-prop direction superseded by §3 Option C (2026-04-22).** Step 2 as-shipped
   > retained the `OpportunityApplication.Artist` / `Opportunity.Venue` navs pointing at
   > Artist.Domain/Venue.Domain — this was reverted by the post-Step-2 design decision to
   > adopt read models. See Step 2b below. Leave the navs in place for now — Step 2b does
   > the nav-type flip together with the read-model entity introduction.

2b. **Introduce `ArtistReadModel` + `VenueReadModel` in `Concert.Domain`** (NEW, post-Step-2
    design revision — owned event-driven projections per MM_NORTH_STAR §5). Purpose: remove
    Concert.Domain's refs to Artist.Domain + Venue.Domain by owning Concert's own projection
    tables, populated from Artist/Venue integration events.

    - Create `ArtistReadModel(int Id PK, Guid UserId, string Name, string? Avatar)` in
      `Concert.Domain/Entities/`. Confirm exact field list at impl time by grepping what
      Concert currently reads off `OpportunityApplication.Artist` — trim aggressively.
    - Create `VenueReadModel(int Id PK, Guid UserId, string Name, string? Avatar)` in
      `Concert.Domain/Entities/` — same exercise.
    - Flip entity nav types: `OpportunityApplicationEntity.Artist : ArtistEntity` →
      `: ArtistReadModel` (name unchanged, still `.Artist`, just a different type).
      `OpportunityEntity.Venue : VenueEntity` → `: VenueReadModel`. `ArtistId` / `VenueId`
      FK columns unchanged — they're the FK into Concert's own `ArtistReadModels` /
      `VenueReadModels` tables.
    - **Remove `Concert.Domain → Artist.Domain` + `Concert.Domain → Venue.Domain` project
      refs** from `Concertable.Concert.Domain.csproj` (Step 2 added them; Step 2b rips them
      out).
    - Remove `using Concertable.Artist.Domain;` / `using Concertable.Venue.Domain;` inside
      Concert.Domain source files.
    - Build must stay green. Callers accessing `application.Artist.Name` etc. keep working —
      the nav is still called `.Artist`, just typed to `ArtistReadModel` now.

    **These are Concert-owned tables.** Step 6 adds real `DbSet<ArtistReadModel>` +
    `DbSet<VenueReadModel>` on ConcertDbContext with normal configurations (no
    `ExcludeFromMigrations`). Concert's `InitialCreate` migration (Step 14) creates the
    physical `ArtistReadModels` + `VenueReadModels` tables. Step 6b wires integration events
    + handlers to populate them.

   ✅ **Done 2026-04-22.** Entities moved: 9 entities (Concert/Booking/Genre/Image,
   Opportunity/Application/Genre, Ticket, Review) + 5 contract types (base + 4 concrete) +
   4 enums (ApplicationStatus/BookingStatus/ContractType/PaymentMethod). All under flat
   `Concertable.Concert.Domain` namespace — **no `.Contracts` sub-namespace** (would
   collide semantically with `Concertable.Concert.Contracts` facade project; Domain is
   publicly accessible for now since read-models don't exist yet, per MM_NORTH_STAR §5).
   `Concert.Domain/Entities/Contracts/` folder retained on disk for organisation only.
   `Concert.Domain.csproj` refs `Artist.Domain` + `Venue.Domain` (surviving navs) + EF +
   NTS packages. `ConcertEntity` now owns `Point? Location` and implements `IHasLocation`.

   **ILocatable cleanup (bonus)** — deleted `ILocatable<TSelf>` interface entirely.
   `GeometrySpecification<TEntity>` + `IGeometrySpecification<TEntity>` now constrain on
   `IHasLocation` with direct `e => e.Location` predicate (no `Substitute` expression
   rewriting). `VenueEntity` + `ArtistEntity` had `ILocatable` + `LocationExpression`
   stripped; both implement `IHasLocation`. `GeometrySpecificationTests.TestEntity`
   simplified. `Concertable.Infrastructure.Expressions.ExpressionExtensions.Substitute` no
   longer used in production code (tests + helper still live; cleanup optional).

   **Drops + temporary stubs** (proper rewiring lands in Step 7 per plan):
   - `TicketEntity.User` nav removed (kept `UserId` Guid).
   - `TicketMappers.ToDto(ticket, string email)` — overload takes email string; resurrected
     (was dead, now called by `TicketService.GetUserUpcomingAsync`/`GetUserHistoryAsync`
     passing `currentUser.Email`). Proper fix: inject IIdentityModule user lookup in Step 7.
   - `ReviewMappers.ToDto(review, string email)` — same signature change;
     `ConcertReviewService.CreateAsync` passes `currentUser.Email`.
   - `QueryableReviewMappers.ToDto` SQL projection stubs `Email = string.Empty` — Step 7
     must join `ApplicationDbContext.Users` (or query-side `IIdentityModule` lookup) for
     the paginated review list; currently affects review listing UI.
   - `MiscEntityConfigurations.TicketEntityConfiguration` drops `HasOne(t => t.User)`
     Fluent mapping (nav no longer exists).
   - `TicketRepository.GetHistoryByUserIdAsync`/`GetUpcomingByUserIdAsync` drop
     `.Include(t => t.User)` chains.

   **Global usings propagated** (partial Step 4 work pulled forward to keep builds green):
   `global using Concertable.Concert.Domain` added to: Core, Application, Infrastructure,
   Seeding, Data.Application, Data.Infrastructure, Workers, Web, Search.Application,
   Search.Infrastructure, Search.Contracts, Identity.Infrastructure, Core.UnitTests,
   Infrastructure.UnitTests, Web.IntegrationTests, Web.E2ETests. Remaining Step 4 work:
   audit any explicit `using Concertable.Core.Entities;` that could be deleted + add
   missing global usings to other test projects if surfaced.

   **Bulk sed**: the 31 files with explicit `using Concertable.Core.Entities.Contracts;`
   had that import stripped entirely (dead — namespace no longer exists; types resolve
   via the `Concertable.Concert.Domain` global using).

   **NuGet bump**: `Concert.Infrastructure` csproj `Microsoft.Extensions.DependencyInjection.Abstractions`
   10.0.0 → 10.0.1 to satisfy `Concert.Domain`'s EFCore 10.0.1 transitive floor.

   **Build**: full solution 0 errors. Integration tests will fail with
   `PendingModelChangesWarning` until Steps 13–14 scaffold the paired
   `ApplicationDbContext` drop migration + `ConcertDbContext` `InitialCreate` —
   expected, plan accepts WIP state between steps. Don't push until Step 14+.

2b. ✅ **Done 2026-04-22.** Owned read models introduced in `Concert.Domain/ReadModels/`:
    - `ArtistReadModel(int Id PK, Guid UserId, string Name, string? Avatar, string? BannerUrl,
      string? County, string? Town, string? Email, ICollection<ArtistReadModelGenre> Genres)`.
    - `ArtistReadModelGenre(int ArtistReadModelId, int GenreId, ArtistReadModel Artist,
      GenreEntity Genre)` — genre join table owned by Concert.
    - `VenueReadModel(int Id PK, Guid UserId, string Name, string About, string? County,
      string? Town, Point? Location)`.
    - `OpportunityApplicationEntity.Artist : ArtistReadModel` (was `ArtistEntity`).
    - `OpportunityEntity.Venue : VenueReadModel` (was `VenueEntity`).
    - `Concert.Domain.csproj` drops both `Artist.Domain` + `Venue.Domain` project refs.
    - Callers updated: `ConcertMappers`, `OpportunityApplicationMapper`,
      `QueryableConcertMappers`, `QueryableConcertHeaderMappers` (Search), `GenreSelectors`
      (added `FromArtistReadModel`), all three repo `Include`/`ThenInclude` chains
      (`ConcertRepository`, `ConcertBookingRepository`, `OpportunityApplicationRepository`),
      `ConcertDraftService` (`.ArtistGenres` → `.Genres`), `OpportunityApplicationService`
      (dropped Artist/Venue Application DTOs + Mappers usings; `GetArtistAndVenueByIdAsync`
      now returns `(ArtistReadModel, VenueReadModel)?`), `IOpportunityApplicationRepository`
      + `IOpportunityApplicationService` interfaces updated to match.
    - `GenreSelectors.FromArtistReadModel` added alongside existing `FromArtist` (which
      stays typed to `ArtistEntity` for Search's `QueryableArtistHeaderMappers`).
    - Address null check `&& Venue.Address != null` removed from all LINQ where-clauses
      (County/Town are now flat nullable fields on `VenueReadModel`).
    - **Build**: full solution 0 errors. Integration tests remain RED on
      `PendingModelChangesWarning` (expected — `ArtistReadModels`, `ArtistReadModelGenres`,
      `VenueReadModels` tables don't exist yet; created in Step 14's `ConcertDbContext`
      `InitialCreate` migration). Don't push until Step 14+.

3. **Audit + delete cross-module methods on Identity repos.** Delete
   `GetByConcertIdAsync` + `GetByApplicationIdAsync` from `IManagerRepository` and both
   `ArtistManagerRepository` + `VenueManagerRepository`. **Also delete** the matching
   `IUserRepository`/`IUserService` variants and the four facade methods on
   `IdentityModule` (`GetVenueManagerByConcertIdAsync`, `GetArtistManagerByConcertIdAsync`,
   `GetVenueManagerByApplicationIdAsync`, `GetArtistManagerByApplicationIdAsync`).
   **`IManagerModule` shrinks to `GetByIdAsync(Guid userId)` only** (nullable return —
   callers assert if needed). `ArtistManagerRepository` / `VenueManagerRepository` drop the `IReadDbContext`
   ctor param — Identity stops touching cross-module read shim entirely for these lookups.
   Callers will temporarily not compile — that's the point; compiler surfaces the rewrite
   list for Step 7. Each caller rewrites to `managerModule.GetByIdAsync(entity.ArtistReadModel.UserId)`
   or the Venue equivalent, using the read-model nav introduced in Step 2b.

   ✅ **Done 2026-04-22.** Deleted methods:
   - `IManagerRepository<T>`: both cross-module methods removed; interface is now empty
     (inherits `IGuidRepository<T>` only).
   - `ArtistManagerRepository` + `VenueManagerRepository`: both methods deleted; `IReadDbContext
     readDb` ctor param dropped; `using Concertable.Data.Application;` removed.
   - `IUserRepository`: `GetIdByApplicationIdAsync`, `GetIdByConcertIdAsync`,
     `GetByApplicationIdAsync`, `GetByConcertIdAsync` removed.
   - `IUserService`: same 4 methods removed.
   - `UserRepository`: same 4 implementations deleted; `IReadDbContext` ctor param dropped.
   - `UserService`: same 4 implementations deleted; `using Concertable.Shared.Exceptions;`
     removed (NotFoundException no longer referenced).
   - `IdentityModule`: 4 facade methods removed; `IManagerRepository<ArtistManagerEntity>` +
     `IManagerRepository<VenueManagerEntity>` ctor params dropped; `using Concertable.Core.Entities;`
     removed.
   - `IManagerModule`: 4 cross-module methods removed; `GetByIdAsync(Guid userId)` is the sole
     remaining method — nullable return (`ManagerDto?`), no throw, callers assert. `GetManagerAsync`
     collapsed into `GetByIdAsync` (they were identical). 6 existing `GetManagerAsync` callers
     renamed to `GetByIdAsync`: `VenueService`, `ArtistService`, `StripeAccountController` (×2),
     `StripeCustomerValidator`, `StripeAccountValidator`. Nullable semantics preserved for the two
     validators that use null to distinguish manager vs customer role.
   - `ServiceCollectionExtensions` (Identity): dead `AddScoped<IManagerRepository<T>, ...>`
     registrations removed.

   **Rewrite list (Step 7)** — callers surfaced by the expected build break:
   - `VersusConcertWorkflow.cs:38-39` — `GetVenueManagerByConcertIdAsync` + `GetArtistManagerByConcertIdAsync`
   - `DoorSplitConcertWorkflow.cs:38-39` — same
   - `VenueHireConcertWorkflow.cs:29-30` — `GetArtistManagerByApplicationIdAsync` + `GetVenueManagerByApplicationIdAsync`
   - `FlatFeeConcertWorkflow.cs:29-30` — same
   - `VenueTicketPaymentService.cs:33` — `GetVenueManagerByConcertIdAsync`
   - `ArtistTicketPaymentService.cs:33` — `GetArtistManagerByConcertIdAsync`
   - `DoorSplitApplicationServiceCompleteTests.cs:36-37` + `VersusApplicationServiceCompleteTests.cs:36-37` — mocks

   **Build**: 10 errors in `Concertable.Infrastructure` (all CS1061 on deleted `IManagerModule`
   methods) — expected WIP state until Step 7 rewrites those callers. Zero new errors in any
   other project.

4. **Add `Concertable.Core → Concertable.Concert.Domain` project reference.** Propagate
   `global using Concertable.Concert.Domain` to all GlobalUsings hosts that currently import
   `Concertable.Core.Entities` for concert types. Fix explicit-using sites.

   ✅ **Done 2026-04-22.** Project ref + global usings were both done in earlier steps
   (Core.csproj already references Concert.Domain; 17 GlobalUsings hosts already have the
   global using from Step 2b). Step 4 was the explicit-using audit:
   - 134 files had `using Concertable.Core.Entities;`. 7 entity types remain in
     `Concertable.Core.Entities` (Transaction, TicketTransaction, ConcertTransaction,
     Preference, GenrePreference, Message, StripeEvent — Payment + ancillary types not yet
     extracted). 38 files genuinely reference one of those — kept.
   - **103 dead `using Concertable.Core.Entities;` lines stripped** (concert types now resolve
     via `Concertable.Concert.Domain` global using). Spans Application/Infrastructure/Web/
     Seeding/Search.Infrastructure/Identity.Infrastructure/Data.Infrastructure + 4 test files.
   - 31 files retain the import (legitimate — they reference one of the 7 surviving types).
   - `Workers.UnitTests` GlobalUsings checked — has no concert refs at all, no propagation
     needed.
   - **Build**: same 10 CS1061 errors from Step 3 (Step 7 rewrite list); zero new errors.

   **Post-Step-4 status (continuation pointer):**
   - 10 CS1061 errors in `Concertable.Infrastructure` are intentional and stay broken until
     **Step 7** — rewriting them in their current location would mean rewriting twice (the
     files all move to `Concert.Infrastructure` in Step 7, where the `IManagerModule.GetByIdAsync`
     calls go in cleanly with the read-model nav UserId hop).
   - `ConcertDbContext` lands in **Step 6** (after Step 5 moves Application). Step 6b wires
     `ArtistChangedIntegrationEvent`/`VenueChangedIntegrationEvent` + projection handlers to
     populate `ArtistReadModel`/`VenueReadModel`.
   - **Sequence: Step 5 → Step 6 → Step 6b → Step 7** (Infrastructure move + IManagerModule
     rewrite happens together in Step 7).

5. **Move Application layer** → `Concert.Application/` with `internal` visibility + correct
   namespace + `AssemblyInfo.cs`. Workflow-strategy interfaces
   (`IContractStrategy`/`IConcertWorkflowStrategy`/`ITicketPaymentStrategy`) + outcome types
   live here too.

   ✅ **Done 2026-04-22.** 47 files moved (interfaces, DTOs, requests, responses, validators,
   mappers — Concert/Opportunity/Contract surface only). All `internal`. **Ticket + Review
   surface deferred to Step 7** (TicketDto's `User : UserDto` coupling and
   ReviewDto/Email handling need IIdentityModule rewiring that lands with the Infra move).

   **Architectural side-moves (out of original §2 scope but unblocked Step 5):**
   - `Concertable.Payment.Contracts` project scaffolded at `api/Modules/Payment/`. Holds
     `PaymentResponse` (the base Stripe-response record used by both Concert immediate-pay
     paths AND legacy `IManagerPaymentService`/`ICustomerPaymentService`/`IPaymentService`).
     `Concertable.Application` + `Concert.Application` both reference it. Sln + nested-folder
     entries added (Payment folder GUID `{C0F1A8B4-...}`, Payment.Contracts GUID
     `{D1A2B9C5-...}`). Payment module will absorb the rest during Payment Stage 1.
   - `IPagination<T>` + `Pagination<T>` moved → `Concertable.Shared.Contracts` under namespace
     `Concertable.Shared` (legitimate primitive per CLAUDE.md rule 5; Search/Venue/Concert all
     consume it). `IPageParams` was already there. Files relocated + namespace renamed; ~38
     callsites picked it up automatically via global `using Concertable.Shared`.
   - `GenreMappers` (extension methods on `GenreEntity`/`GenreDto`) moved →
     `Concertable.Shared.Contracts` — was blocking Concert.Application's mappers since
     `GenreEntity` is in Shared.Domain and `GenreDto` is in Shared.Contracts already. Added
     `<Using Include="Concertable.Shared" />` to Shared.Contracts.csproj for global resolution.
   - `ConcertBookingParams` moved out of `Concertable.Core/Parameters/` →
     `Concert.Application/Requests/`, `internal`. Was Concert-scoped; only consumer is
     `ConcertBookingParamsValidator`.

   **Sln entries (Concert.Application + Concert.Api added; Step 1's Done note claimed these
   were in but they weren't):** GUIDs `{A8D9E6F2-4B3C-4A5E-B7C1-1F2E3D4C5B6A}` (Application),
   `{B9E0F7A3-5C4D-5B6F-C8D2-2A3B4C5D6E7F}` (Api). Both nested under existing
   `Modules/Concert` solution folder.

   **`AssemblyInfo.cs` InternalsVisibleTo grants** (Step 5 expanded list — each marked
   TEMPORARY with retiring step):
   ```csharp
   [assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]   // Step 7
   [assembly: InternalsVisibleTo("Concertable.Concert.Api")]              // Step 9
   [assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]     // permanent
   [assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")] // permanent
   [assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")] // permanent
   [assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]        // permanent
   // TEMPORARY: legacy Concertable.Infrastructure still hosts Concert service impls until Step 7.
   [assembly: InternalsVisibleTo("Concertable.Infrastructure")]
   // TEMPORARY: Concertable.Workers re-registers Concert dispatchers until Step 12.
   [assembly: InternalsVisibleTo("Concertable.Workers")]
   // TEMPORARY: DevController + E2EEndpointExtensions inject Concert dispatchers; remove when those move into Concert.Api (Step 9).
   [assembly: InternalsVisibleTo("Concertable.Web")]
   ```

   **Mid-Step accessibility cascade (key learning — codify in feedback memory):** Once
   Concert.Application interfaces became `internal`, every legacy Concert IMPL in
   `Concertable.Infrastructure` failed to build with CS0050/CS0051 ("constructor parameter X
   is less accessible than method") — because `public` impl classes can't take `internal`
   interfaces in their ctors/method signatures. The wrong fix was making interfaces `public`
   for test convenience (the user explicitly flagged this as an antipattern mid-step). The
   right fix: bulk-flip 31 legacy impl classes `public class` → `internal class`. Files
   touched: `ConcertService`, `ConcertDraftService`, `OpportunityService`,
   `OpportunityApplicationService`, `ContractService`, `UpfrontConcertService`,
   `DeferredConcertService`, 4 workflows, 4 dispatchers, `ApplicationAcceptHandler`,
   `TicketService`, `ArtistTicketPaymentService`, `VenueTicketPaymentService`,
   `SettlementWebhookHandler`, `ConcertRepository`, `ConcertBookingRepository`,
   `OpportunityRepository`, `OpportunityApplicationRepository`, `ContractRepository`,
   `ConcertValidator`, `OpportunityApplicationValidator`, `TicketValidator`,
   `ContractStrategyFactory`, `ContractStrategyResolver`, `QueryableConcertMappers`. DI is
   unaffected — `AddScoped<IFoo, Foo>()` works fine across `InternalsVisibleTo` boundaries.

   **Signature pre-change** (originally planned for Step 7, pulled into Step 5 because the
   interface couldn't otherwise compile inside Concert.Application without an Identity.Domain
   ref): `IOpportunityService.GetOwnerByIdAsync(int)` and `IOpportunityRepository.GetOwnerByIdAsync(int)`
   both rewritten from `Task<UserEntity>` → `Task<Guid?>`. The single legacy caller —
   `OpportunityApplicationService.ApplyAsync` — picks up `IManagerModule managerModule` ctor
   dep and does `managerModule.GetByIdAsync(opportunityOwnerId)` for the email lookup
   previously sourced from `UserEntity.Email`. `OpportunityRepository.GetOwnerByIdAsync` now
   reads `o.Venue.UserId` from its own context (no `IReadDbContext` ride-along).

   **Global usings added to keep callers compiling without per-file churn** (legacy hosts +
   tests get Concert.Application namespaces via global usings rather than ~100 individual
   `using` updates):
   - `Concertable.Infrastructure/GlobalUsings.cs` → all 6 Concert.Application namespaces
     (Interfaces/DTOs/Requests/Responses/Validators/Mappers) + `Payment.Contracts`
   - `Concertable.Web/GlobalUsings.cs` → same
   - `Concertable.Workers/GlobalUsings.cs` → same
   - `Concertable.Application/GlobalUsings.cs` → `Payment.Contracts` (legacy IPaymentService etc.)
   - Test global usings (`Web.IntegrationTests`, `Web.E2ETests`, `Infrastructure.UnitTests`)
     → all Concert.Application namespaces + Payment.Contracts
   - `Concert.Application/GlobalUsings.cs` → `Concertable.Application.Interfaces` (for
     `IIdRepository`/`IGuidRepository` whose namespace is awkwardly `Concertable.Application.Interfaces`
     even though their files live in Shared.Domain). Marked as a known awkward import; cleanup
     belongs to a separate Shared.Domain namespace-rename pass.

   **Bulk strip:** `using Concertable.Application.Interfaces.Concert;` deleted from ~30 files
   (namespace no longer exists post-move). Sed: `sed -i '/^using Concertable\.Application\.Interfaces\.Concert;$/d'`.

   **Files staying in legacy `Concertable.Application/` until Step 7** (Ticket/Review surface
   per scope agreement): `ITicketRepository`, `ITicketService`, `ITicketValidator`,
   `IConcertReviewRepository`, `IArtistReviewRepository`, `IVenueReviewRepository`
   (rating-pipeline rewrite, MM_NORTH_STAR §5), `IReviewService`, `IReviewServiceFactory`,
   `IReviewValidator`, `TicketDto` (in `SharedDtos.cs`), `TicketConcertDto` (kept in
   `SharedDtos.cs` after a brief move-and-revert when TicketDto's UserDto coupling surfaced),
   `ReviewDto`, `CreateReviewRequest`, `TicketValidators.cs`, `ReviewValidators.cs`,
   `TicketMappers.cs`, `ReviewMappers.cs`, `TicketTransactionMapper.cs`,
   `ConcertTransactionMapper.cs` (last two implement Payment-module `ITransactionMapper`).
   `PaymentResponses.cs` in legacy now contains only `TicketPaymentResponse` (the base
   `PaymentResponse` moved to `Payment.Contracts`).

   **Cross-module ref chain** (Stage 5 transitional shape — collapses in Step 7):
   - `Concertable.Application` → `Concertable.Payment.Contracts` ✓
   - `Concertable.Infrastructure` → `Concertable.Concert.Application` (via project ref) ✓
   - `Concertable.Application` does **not** reference `Concert.Application` (no callers need
     the move-targets — TicketMappers/SharedDtos all use TicketConcertDto from legacy now).
   - `Concert.Application.csproj` refs: Concert.Contracts, Concert.Domain, Identity.Contracts,
     Artist.Contracts, Venue.Contracts, Payment.Contracts, Shared.Contracts, Shared.Validation
     + FluentResults + FluentValidation packages.

   **Build:** same 10 intentional CS1061 errors from Step 3 (the `IManagerModule.GetXManagerByConcertIdAsync`
   /`GetXManagerByApplicationIdAsync` callers in 4 workflows + 2 ticket payment services + 2
   test mocks). Zero new errors. These get rewritten in Step 7 as the workflows/services move
   to `Concert.Infrastructure` and pick up the read-model UserId hop +
   `managerModule.GetByIdAsync(userId)`.

6. **Create `ConcertDbContext`** in `Concert.Infrastructure/Data/`. Apply all 14 entity
   configurations explicitly for Concert-owned types, **plus 2 more** for the Step 2b read
   models — `ArtistReadModelConfiguration` + `VenueReadModelConfiguration` (16 configs total).
   These are Concert-owned tables (no `ExcludeFromMigrations`) — Concert creates
   `ArtistReadModels` + `VenueReadModels` in its `InitialCreate` migration. Artist and Venue's
   source tables are not mapped in ConcertDbContext at all — Concert doesn't read them
   directly; the read models are Concert's own snapshot, populated from events (Step 6b).

6b. **Integration events + projection handlers.** Wire the read models to Artist/Venue write
    paths.

    - **Contracts** — add `ArtistChangedIntegrationEvent(int ArtistId, Guid UserId, string Name,
      string? Avatar) : IIntegrationEvent` in `Artist.Contracts/Events/`. Same shape for
      `VenueChangedIntegrationEvent(int VenueId, Guid UserId, string Name, string? Avatar)` in
      `Venue.Contracts/Events/`. (Check whether `ArtistCreated`/`ArtistUpdated` from
      MM_NORTH_STAR's event catalog fit better — prefer splitting Created vs Updated if
      semantically clearer.)
    - **Publish** — one-line emit in `ArtistService.CreateAsync`/`UpdateAsync` +
      `VenueService.CreateAsync`/`UpdateAsync` on successful save. Use the existing
      `IIntegrationEventBus` (already in `Concertable.Shared.Infrastructure`).
    - **Handlers** — `ArtistReadModelProjectionHandler : IIntegrationEventHandler<...>` in
      `Concert.Infrastructure/Handlers/`. Upsert-by-id into `ConcertDbContext.ArtistReadModels`;
      idempotent. Same for `VenueReadModelProjectionHandler`.
    - **Register** in `AddConcertModule()` (Step 11).
    - **Backfill** — `ConcertDevSeeder`/`ConcertTestSeeder` (Step 10) seed the read-model rows
      directly from `SeedData`. Production backfill TBD — either a startup task that reads
      Artist/Venue tables and upserts Concert's read models once, or a one-off migration
      script. Decide at deployment time.

7. **Move Infrastructure layer** → `Concert.Infrastructure/`:
   - All services + workflow strategies + dispatchers + repositories.
   - Rewrite callers of the deleted Identity repo methods (from Step 3) to
     `managerModule.GetByIdAsync(application.Artist.UserId)` /
     `opportunity.Venue.UserId` — the restricted-mapping nav (still named `.Artist` /
     `.Venue`, typed to `ArtistReadModel` / `VenueReadModel` after Step 2b) exposes UserId,
     Identity does one PK lookup. Two round trips instead of three.
   - Switch repository base classes to `IntModuleRepository<T, ConcertDbContext>`.
   - Leave payment-service injections pointing at `Concertable.Infrastructure` (temporary
     legacy ref — see §3).

8. **Extend `IConcertModule` + `ConcertModule`** stubs (they exist; add the empty interface
   per §4 and the stub impl).

9. **Create `Concert.Api`** with all 4–6 controllers (internal) +
   `InternalControllerFeatureProvider`. Inject internal services directly.

10. **Add `ConcertDevSeeder` + `ConcertTestSeeder`** in `Concert.Infrastructure/Seeding/`.
    Wire into `DevDbInitializer` and `TestDbInitializer` via `IDevSeeder`/`ITestSeeder`.
    **Precondition for Step 13.** Budget significant time — Concert has the largest seed
    surface of any module (concerts, opportunities, applications, contracts, tickets,
    reviews, transactions).

11. **`AddConcertModule()`** DI extension with the full block from §6. Wire it up in
    `Concertable.Web/Program.cs`.

12. **Remove Concert registrations from `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`**
    — all services, dispatchers, repositories, keyed workflow strategies. Keep the payment
    services (they stay until Payment extracts). **Also remove the duplicate Concert DI
    block from `api/Concertable.Workers/ServiceCollectionExtensions.cs`** (lines ~68–73 —
    `IFinishedDispatcher`, `ISettlementDispatcher`, four keyed `IConcertWorkflowStrategy`)
    and replace with a single `services.AddConcertModule(cfg)` call. Workers picks up a ref
    to `Concertable.Concert.Infrastructure` in its csproj.

13. **`ApplicationDbContext` cleanup** — remove all 14 Concert DbSets + their
    `ApplyConfiguration` calls. Scaffold the paired drop migration.

14. **Scaffold `ConcertDbContext` `InitialCreate` migration.** Inspect for outbound FKs:
    strip `FK_ConcertGenres_Genres_GenreId`, `FK_OpportunityGenres_Genres_GenreId`, and any
    Artist/Venue/Identity FKs (per §7 directional rule). Keep CLR navs where §3 option (a)
    is in effect.

15. **Global usings** — add `Concertable.Concert.Contracts` + `Concertable.Concert.Domain`
    to `Concertable.Infrastructure/GlobalUsings.cs` + `Concertable.Web/GlobalUsings.cs`
    where Concert types were previously pulled via `Concertable.Application.*` /
    `Concertable.Core.Entities`. **Remove** `Concertable.Application.Interfaces.Concert`
    and similar from legacy global using imports — Concert namespaces own those now.

16. **Run migrations + full test suite.** Expect failures in:
    - `VenueService`/`ArtistService` saving via wrong context (won't apply — those were
      fixed in Venue Stage 1). Keep watching for the pattern in any new Concert services
      that slip in `IUnitOfWork` — use `xRepository.SaveChangesAsync()` per
      `feedback_module_service_saves_own_context.md`.
    - Ambiguous `ConcertDto`/`OpportunityDto` references where two namespaces now export
      the same name during the transition.
    - Integration tests exercising the keyed workflow-strategy wiring. The key must match
      the `ContractType` enum value exactly — misordered registration is silent at startup,
      noisy at runtime.

17. **Fix regressions + close the stage.** Run the full 218+ test suite. Commit per step
    (or at the end of each logical block) — this extraction is large enough that a single
    WIP commit will be painful to rebase.

### 10. Known friction points

#### a. Keyed DI for workflow strategies

Four `IConcertWorkflowStrategy` implementations keyed by `ContractType`. Moving them is
mechanical, but **the key values must match the enum exactly** — otherwise `AcceptDispatcher`
silently fails to resolve at runtime. Copy the registration block verbatim from the old
Web extension; don't retype. Test coverage: application-acceptance integration tests must
exercise all four contract types.

#### b. TPT inheritance on `ContractEntity`

Five EF configurations (base + 4 concrete). The generated migration will produce 5 tables
with FKs linking concrete → base. Verify the generated migration matches the existing
`ApplicationDbContext` contract tables exactly — any drift here is a silent data-loss risk
during the drop/create migration pair.

#### c. `OpportunityEntity.Period` (owned `DateRange`)

`Period` is an owned type in `OpportunityEntityConfiguration`. EF materializes it as two
columns (`Period_Start`, `Period_End`). Generated migration must preserve column names
exactly — check before applying.

#### d. Payment service temp ref

`Concert.Infrastructure` → `Concertable.Infrastructure` is the one accepted
cross-extraction-boundary reference. Mark it clearly in the `.csproj` with a comment:

```xml
<!-- TEMPORARY: payment services stay in legacy Infrastructure until Payment extraction. -->
<!-- Replace with Concertable.Payment.Contracts ref during Payment Stage 1. -->
<ProjectReference Include="..\..\..\Concertable.Infrastructure\Concertable.Infrastructure.csproj" />
```

This is the kind of thing that silently becomes permanent if not flagged. Callout lives in
the csproj, in `MM_NORTH_STAR.md`, and in the Payment plan when it's written.

#### e. `ConcertEntity.LocationExpression` — delete on sight

Stale. Concert has its own `Location` column now. Every consumer using `LocationExpression`
(distance sorts in `ConcertRepository`, Search specs, `ConcertService` line ~130) flips to
`c.Location` directly. Grep all callers during Step 2; delete the expression in the same
commit. No fallback path, no option (a)/(b) — just gone.

#### f. `OpportunityApplicationValidator` Identity lookup

Currently uses `IManagerRepository.GetByApplicationIdAsync` (the cross-module smell). After
Step 3 deletes that method, validator rewrites to use `IOpportunityApplicationRepository`
+ `IManagerModule.GetByIdAsync`. Two queries instead of one; acceptable per draft doc's
"same round trips" analysis.

#### g. Seeding is a Stage 1 precondition, not a follow-up

Repeating from Venue §10e because Concert's seed surface is 3× larger: `ConcertDevSeeder` +
`ConcertTestSeeder` MUST land before Step 13. Budget time for it. Concert seeds reference
Artist/Venue/Identity seed ids — make sure order is preserved: Identity (0) → Artist (1) →
Venue (2) → Concert (3).

#### h. Controllers inject many services — don't narrow the surface

`OpportunityApplicationController` currently injects 5 dependencies
(`IOpportunityApplicationService`, `IOpportunityApplicationValidator`, `IArtistModule`,
`ICurrentUser`, `IOpportunityService`). Resist the urge to "clean this up" during the move —
that's out of scope. Carry the shape over verbatim. Refactors can happen post-extraction.

#### i. `SignalR` hub wiring

`ConcertController` injects `IConcertNotificationService` (SignalR). The hub class itself
stays in `Concertable.Web/Hubs/` until notifications/messaging is scoped. The interface
`IConcertNotificationService` may need to live in `Concert.Contracts` or `Shared.Contracts`
— confirm during impl. If it's only called from Concert, the interface can stay internal to
`Concert.Application` and the hub implementation stays in Web.

#### j. Search cross-module reads continue unchanged

Search's `ConcertHeaderRepository` etc. read through `IReadDbContext.Concerts` (and
related). `IReadDbContext` continues to project these `IQueryable<TEntity>`s — just against
`ConcertDbContext` now. Verify `IReadDbContext`'s implementation knows about
`ConcertDbContext` during Step 6. This is the only place the `ReadDbContext` composition
needs updating.

### 11. Explicitly out of scope

- **Payment extraction** — separate plan, triggered after Concert Stage 1 lands. Payment
  absorbs Stripe clients, `IManagerPaymentService` family, `ICustomerPaymentService`,
  `IStripeAccountService`, `TicketTransactionEntity`, `SettlementTransactionEntity`, and
  the webhook handlers.
- **Messaging/notifications extraction** — includes `MessageEntity`, `IConcertNotificationService`
  and its hub, email-notification services. Separate plan.
- **Rating-pipeline rewrite** — `ArtistRatingRepository` + `VenueRatingRepository` removal,
  projection-only rating reads. `MM_NORTH_STAR §5`.
- **Retiring cross-module Domain refs** (`Concert.Domain → Artist.Domain`/`Venue.Domain`).
  Retires with the read-model replacement for entity refs across all modules, not Concert
  Stage 1. MM_NORTH_STAR §Corollary 5.
- **Migrating Search off `IReadDbContext`** — Search's `SearchDbContext` + per-module view
  models is the target end state. Concert extraction gets Search to "the only remaining
  consumer," which is the prerequisite for that rewrite but not the rewrite itself.
- **Rewriting Search location-filter specs to `IQueryable<T> where T : IHasLocation`** —
  tidier than chasing nav chains for location. Part of the Search migration, not Concert.
- **Flipping concert entities to `internal`** — post-Payment extraction, when nothing outside
  Concert references them.
- **Integration-event surface** — `ConcertCreatedIntegrationEvent`,
  `BookingConfirmedIntegrationEvent`, `OpportunityApplicationAcceptedIntegrationEvent`, etc.
  These arrive when a foreign module (Payment) actually needs to react. Don't invent
  speculatively.
- **`IConcertModule` methods** — stays empty until a caller proves need.
- **Query handler promotion** for the two-round-trip Identity lookups (`GetManagerForConcertQueryHandler`
  etc.) — only promote once profiling shows the split matters. Default is the two-query split
  at the call site.

---

## Up next: Payment

After Concert Stage 1 lands, Payment is the logical next extraction. Triggers:

- Removes the single `Concert.Infrastructure → Concertable.Infrastructure` temp project ref
  (§10d).
- Absorbs `StripePaymentClient`, `OnSession`/`OffSession` services, keyed
  `IManagerPaymentService`, `ICustomerPaymentService`, `IStripeAccountService`,
  `TicketTransactionEntity`, `SettlementTransactionEntity`, webhook handlers.
- First extraction where integration events (`BookingConfirmedIntegrationEvent`?,
  `ConcertSettledIntegrationEvent`?) likely get introduced — Concert will consume them via
  handlers inside `Concert.Infrastructure`.
- `IPaymentModule` facade surface becomes concrete (methods Concert calls during
  settlement + ticket purchase).

No draft exists for Payment yet. Start one at `PAYMENT_MODULE_REFACTOR_DRAFT.md` when
Concert Stage 1 is nearing complete.

---

## Reference

- `VENUE_MODULE_REFACTOR.md` — the direct precedent this plan is a delta against.
- `ARTIST_MODULE_REFACTOR.md` — canonical 4-layer pattern; keyed-DI handling for rating
  specifications mirrors what workflow strategies need.
- `IDENTITY_MODULE_REFACTOR.md` + `IDENTITY_COMPLETION.md` — namespace lesson (don't leave
  `Concertable.Core.Entities` / `Concertable.Application.*` on moved files).
- `CONCERT_MODULE_REFACTOR_DRAFT.md` — original draft notes. §"Preferred fix option 1"
  becomes Step 3 + Step 7 above; §"Action items" is superseded by this plan.
- `CLAUDE.md` — non-negotiable module / DbContext / namespace rules.
- `MM_NORTH_STAR.md` — §Corollary 1/2/5 + bridge §1 (rating-pipeline rewrite,
  integration-event outbox, read-model replacement for Module.Domain refs).
- Memory: `project_modular_monolith.md`, `feedback_module_facade_surface.md`,
  `feedback_cross_context_fk.md`, `feedback_module_service_saves_own_context.md`,
  `feedback_shared_migrations_history_table.md`, `feedback_sln_solution_folder_duplicate.md`,
  `project_seeding_architecture.md`, `feedback_workers_is_composition_host.md`.

---

## Appendix A — DI snapshot (captured 2026-04-22)

This is the live state of Concert-adjacent DI across both composition hosts at the moment
Stage 0 completed. Step 11 (`AddConcertModule()`) copies the → rows verbatim; Step 12 deletes
them from both hosts. Do not re-derive — the keyed registrations must match enum values
exactly, and the `AddKeyedManagerPaymentService` helper is easy to misread.

Legend:
- **→** Moves into `AddConcertModule()`.
- **⊘** Stays in Web (not Concert-owned — notifications, shared infra, future modules).
- **⏳** Stays in Web for now; migrates during Payment extraction.

### `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`

`AddServices()` lines 182–212:
```
 ⊘ IConcertNotificationService → SignalRConcertNotificationService    (stays; notifications module future)
 ⊘ IConcertPostedHandler → ConcertPostedHandler                        (stays; notifications)
 ⊘ IApplicationNotificationService → SignalRApplicationNotificationService
 ⊘ ITicketNotificationService → SignalRTicketNotificationService
 ⊘ IMessageNotificationService → SignalRMessageNotificationService
 → IConcertDraftService → ConcertDraftService
 → IConcertService → ConcertService
 → IOpportunityApplicationService → OpportunityApplicationService
 ⊘ IMessageService → MessageService                                    (messaging module future)
 → IOpportunityService → OpportunityService
 → ITicketService → TicketService
 ⏳ ITransactionService → TransactionService                            (Payment module)
 ⏳ ITransactionMapper → TransactionMapper (singleton)                  (Payment)
 ⊘ IGenreService → GenreService                                        (shared reference data)
 → IReviewService keyed: Artist/Venue/Concert → ArtistReviewService/VenueReviewService/ConcertReviewService
 → IReviewServiceFactory → ReviewServiceFactory
 ⊘ IGeometryCalculator (singleton)                                     (shared)
 ⊘ IPdfService, QRCodeGenerator, IQrCodeService                        (shared infra)
 ⊘ IPreferenceService → PreferenceService                              (Identity adjacent? confirm)
 ⊘ UrlSettings, IUriService                                            (shared)
```

`AddServiceValidators()` 214–222:
```
 → IConcertValidator → ConcertValidator (singleton)
 → ITicketValidator → TicketValidator
 → IOpportunityApplicationValidator → OpportunityApplicationValidator
 → IReviewValidator → ReviewValidator
```

`AddRepositories()` 224–246:
```
 → IConcertRepository → ConcertRepository
 → IOpportunityApplicationRepository → OpportunityApplicationRepository
 → IConcertBookingRepository → ConcertBookingRepository
 ⊘ IMessageRepository → MessageRepository                              (messaging)
 → IOpportunityRepository → OpportunityRepository
 → IContractRepository → ContractRepository
 → ITicketRepository → TicketRepository
 ⏳ ITransactionRepository → TransactionRepository                      (Payment)
 ⊘ IGenreRepository → GenreRepository                                  (shared)
 → IArtistReviewRepository → ArtistReviewRepository                     (Review is Concert-owned)
 → IVenueReviewRepository → VenueReviewRepository
 → IConcertReviewRepository → ConcertReviewRepository
 ⊘ AddRatingRepositories (keyed Artist/Concert/Venue)                  (rating-pipeline rewrite, MM_NORTH_STAR §5)
 ⊘ IPreferenceRepository → PreferenceRepository
 ⏳ IStripeEventRepository → StripeEventRepository                      (Payment)
 ⏳ IUnitOfWork → UnitOfWork                                            (deprecated; services save own context)
 ⊘ IDapperRepository → DapperRepository                                (shared)
```

`AddContracts()` 265–304 — the most fragile block:
```
 → IContractStrategyFactory<> (open generic) → ContractStrategyFactory<>
 → IContractStrategyResolver<> (open generic) → ContractStrategyResolver<>
 → IContractService → ContractService
 → IContractMapper (singleton) → ContractMapper
 → IOpportunityMapper (singleton) → OpportunityMapper
 → IOpportunityApplicationMapper (singleton) → OpportunityApplicationMapper
 → IUpfrontConcertService → UpfrontConcertService
 → IDeferredConcertService → DeferredConcertService
 ⏳ AddKeyedManagerPaymentService("onSession")                          (Payment)
 ⏳ AddKeyedManagerPaymentService("offSession")                         (Payment; factory helper at lines 257–263)
 ⏳ ICustomerPaymentService → CustomerPaymentService                    (Payment)
 → ITicketPaymentDispatcher → TicketPaymentDispatcher
 → IApplicationAcceptHandler → ApplicationAcceptHandler
 → IAcceptDispatcher → AcceptDispatcher
 → IFinishedDispatcher → FinishedDispatcher
 → ISettlementDispatcher → SettlementDispatcher

 → ITicketPaymentStrategy keyed by ContractType:
     FlatFee   → VenueTicketPaymentService
     DoorSplit → VenueTicketPaymentService
     Versus    → VenueTicketPaymentService
     VenueHire → ArtistTicketPaymentService

 → IConcertWorkflowStrategy keyed by ContractType:
     FlatFee   → FlatFeeConcertWorkflow
     DoorSplit → DoorSplitConcertWorkflow
     Versus    → VersusConcertWorkflow
     VenueHire → VenueHireConcertWorkflow

 ⏳ IWebhookStrategyFactory → WebhookStrategyFactory                    (Payment webhook infra)
 ⏳ IWebhookProcessor → WebhookProcessor                                (Payment)
 ⏳ IWebhookQueue → WebhookQueue                                        (Payment)
 ⏳ IWebhookStrategy keyed WebhookType.Concert → TicketWebhookHandler   (Payment — ticket purchase webhook)
 → IWebhookStrategy keyed WebhookType.Settlement → SettlementWebhookHandler
       (per §2/§3: SettlementWebhookHandler moves to Concert.Infrastructure; the KEYED
        REGISTRATION itself stays in Web's webhook-strategy block for now because the
        factory/processor stay there. Just update the `using` in Web to point at the
        Concert.Infrastructure namespace once the handler moves. When Payment extracts
        and takes the webhook infra with it, Concert exposes its settlement handler via
        IConcertModule or the handler re-homes again.)
```

`AddStripeServices()` 102–135 — **entirely** stays until Payment extracts:
```
 ⏳ StripeAccountValidator/StripeCustomerValidator                      (Payment)
 ⏳ IStripeValidator, IStripeValidationFactory, keyed IStripeValidationStrategy  (Payment)
 ⏳ Stripe.* services + IStripeAccountService + IStripePaymentClient    (Payment)
 ⏳ IPaymentService keyed on/off Session + IWebhookService               (Payment)
```

### `Concertable.Workers/ServiceCollectionExtensions.cs`

`AddRepositories()` 50–57:
```
 → IConcertRepository (duplicate of Web — remove when Workers switches to AddConcertModule)
 → IOpportunityApplicationRepository (duplicate)
 → IContractRepository (duplicate)
```

`AddServices()` 59–76:
```
 ⏳ IPaymentService → PaymentService                                    (Payment)
 ⏳ IStripeAccountService → StripeAccountService                        (Payment)
 ⏳ IManagerPaymentService → ManagerPaymentService  ← NOTE: non-keyed here, DIFFERENT from Web's keyed pair (Payment)
 → IContractStrategyFactory<>/Resolver<>            (open generic — duplicate; covered by AddConcertModule)
 → IFinishedDispatcher → FinishedDispatcher        (duplicate)
 → ISettlementDispatcher → SettlementDispatcher    (duplicate)
 → IConcertWorkflowStrategy keyed ×4                (duplicate of Web's block; identical key↔impl mapping)
```

### Delete verification for Step 12

Post-extraction, Web's `AddContracts()` retains: `AddKeyedManagerPaymentService` pair,
`ICustomerPaymentService`, webhook factory/processor/queue, `TicketWebhookHandler`.
Everything else in 265–304 moves.

Post-extraction, Workers' `AddServices()` collapses to:
```csharp
services.AddConcertModule(configuration);
// Payment-module legacy registrations remain until Payment extraction:
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IStripeAccountService, StripeAccountService>();
services.AddScoped<IManagerPaymentService, ManagerPaymentService>();
```
`AddRepositories()` deletes entirely (all three now ship via `AddConcertModule()`).
