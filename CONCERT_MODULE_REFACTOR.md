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

   **✅ Done 2026-04-22.**

   **Files created:**
   - `api/Modules/Concert/Concertable.Concert.Infrastructure/Data/ConcertDbContext.cs` — `internal class ConcertDbContext : DbContextBase`. 17 DbSets (14 owned + 2 read models + ArtistReadModelGenres join). `OnModelCreating` is a one-liner: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertDbContext).Assembly)` — picks up every `IEntityTypeConfiguration<>` in Concert.Infrastructure (`internal` works fine, EF reflection ignores access modifiers).
   - 11 configs in `api/Modules/Concert/Concertable.Concert.Infrastructure/Data/Configurations/` (one file per config, all `internal`):
     - **Pre-existing, moved out of `Data.Infrastructure/Data/Configurations/` (Concert owns these entities):** `ConcertEntityConfiguration`, `ConcertGenreEntityConfiguration`, `ConcertImageEntityConfiguration`, `TicketEntityConfiguration`, `OpportunityEntityConfiguration` (file also holds `OpportunityApplicationEntityConfiguration`), `ContractEntityConfiguration` (file also holds `FlatFee/DoorSplit/Versus/VenueHire ContractEntityConfiguration` — TPT inheritance). `Data.Infrastructure/Data/Configurations/OpportunityConfigurations.cs` + `ContractConfigurations.cs` deleted; Concert/Ticket configs stripped from `MiscEntityConfigurations.cs`.
     - **NEW (didn't exist before — entities used EF conventions previously):** `ConcertBookingEntityConfiguration` (1:1 Application↔Booking via ApplicationId, NoAction), `OpportunityGenreEntityConfiguration` (cascading FKs to Opportunity+Genre, mirrors ConcertGenre), `ReviewEntityConfiguration` (Ticket nav via TicketId Guid FK, NoAction), `ArtistReadModelConfiguration` + `ArtistReadModelGenreConfiguration` (same file: unique UserId index, cascading HasMany Genres, composite key on join), `VenueReadModelConfiguration` (unique UserId index + Location.HasColumnType("geography")).

   **ReadDbContext + ApplicationDbContext cross-assembly discovery:** Both used to call `ApplyConfigurationsFromAssembly(typeof(<self>).Assembly)` — but Concert configs no longer live in those assemblies. Fix: replace with an `AppDomain.CurrentDomain.GetAssemblies()` filter for `Concertable.*.Infrastructure` and call `ApplyConfigurationsFromAssembly` per matching assembly. Concert.Infrastructure is loaded into AppDomain via Web's transitive ref (and via DI registration once AddConcertModule lands at Step 11). No project-ref change required (Data.Infrastructure → Concert.Infrastructure would be a cycle). No registry. No public types. Configs stay `internal`. Same approach in `Concertable.Infrastructure/Data/ApplicationDbContext.cs` so AppDb keeps a valid model during the transition window until Step 13 strips its DbSets.

   **Mid-step corrections (key learning, codified in `feedback_module_owns_its_configs.md`):**
   - User flagged twice that Concert-owned configs sitting in `Concertable.Data.Infrastructure/Data/Configurations/` violates module ownership — first for the 5 NEW ones I appended to `MiscEntityConfigurations.cs`, then for the 6 pre-existing ones (`ConcertEntityConfiguration` etc.) I left there. Correct read: the legacy `Data.Infrastructure/Data/Configurations/` folder is pre-extraction carryover, not the target. Anything Concert owns moves to `Concert.Infrastructure/Data/Configurations/` now. CLAUDE.md rewritten to make this unambiguous.
   - User also pushed back on my initial 17-line `ApplyConfiguration` block in `OnModelCreating` and on a static-registry pattern I tried for cross-assembly discovery. Final shape uses `ApplyConfigurationsFromAssembly` for everything — one line in ConcertDbContext (its own assembly), AppDomain scan in ReadDbContext/AppDbContext (cross-assembly).

   **Note:** `OpportunityGenreEntity` + `ConcertGenreEntity` retain their EF Core `[PrimaryKey]` attributes — violates CLAUDE "Domain free of EF attributes beyond BCL `Schema`" rule, but pre-existing carryover. Cleanup deferred (low priority; doesn't block extraction).

   **csproj:**
   - `Concertable.Concert.Infrastructure.csproj` — added EF Core `SqlServer` 10.0.3 + `SqlServer.NetTopologySuite` 10.0.3 packages, `Data.Infrastructure` project ref. Bumped `DependencyInjection.Abstractions` 10.0.1 → 10.0.3 to resolve NU1605 downgrade warning-as-error from EF transitive ref.

   **Cross-context coexistence (transitional):**
   - `ApplicationDbContext.OnModelCreating` calls `ApplyConfigurationsFromAssembly(typeof(DbContextBase).Assembly)`, so it now picks up the 5 new configs automatically. The 3 Concert-owned ones (Booking, OpportunityGenre, Review) match the entities ApplicationDbContext already has DbSets for — additive only. The 3 read-model configs (Artist/Venue read models + join) get added to ApplicationDbContext's model too via the assembly scan, which would create new tables — **but ApplicationDbContext is going away in Step 13**, so this is short-term noise. PendingModelChangesWarning was already RED per memory; this changes nothing about test status.

   **What was NOT done (deferred):**
   - **IReadDbContext composition unchanged.** Plan §10j says "Verify `IReadDbContext`'s implementation knows about `ConcertDbContext` during Step 6" — but ReadDbContext is a separate DbContext that maps the same physical tables and applies configs from the same Data.Infrastructure assembly. As long as the configs live in Data.Infrastructure (rule), ReadDbContext's `Set<ConcertEntity>()` etc. continue to work whether the "owning" model is ApplicationDbContext or ConcertDbContext at the EF level. ArtistReadModel/VenueReadModel are Concert-private — not exposed via IReadDbContext (Search doesn't read them).
   - **No `AddConcertModule()` DI wiring** — that's Step 11 (after services move in Step 7).
   - **No migrations scaffolded** — Step 14.

   **Build:** still exactly the 10 intentional CS1061 errors from Step 3 (4 workflows + 2 ticket payment services in legacy `Concertable.Infrastructure`). Zero new errors. Tests still RED on PendingModelChangesWarning until Steps 13–14.

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

   ✅ **Done 2026-04-22.**

   **Events (Contracts, public records, `: IIntegrationEvent`):**
   - `ArtistChangedEvent(int ArtistId, Guid UserId, string Name, string? Avatar, string? BannerUrl, string? County, string? Town, string? Email, IReadOnlyCollection<int> GenreIds)` in
     `api/Modules/Artist/Concertable.Artist.Contracts/Events/ArtistChangedEvent.cs`.
   - `VenueChangedEvent(int VenueId, Guid UserId, string Name, string About, string? County, string? Town, double? Latitude, double? Longitude)` in
     `api/Modules/Venue/Concertable.Venue.Contracts/Events/VenueChangedEvent.cs`.
   - **Field-set deviation from plan draft:** the plan §6b draft listed only `(ArtistId, UserId, Name, Avatar)` / `(VenueId, UserId, Name, Avatar)`. Final shape carries every field the read models hold (per §2b read-model definitions), so the projection handler can fully populate without secondary lookups. `Genres` ride as `int[]` IDs (Concert's `ArtistReadModelGenre` join already references `Shared.Domain.GenreEntity` — the join row only needs `GenreId`). `VenueChangedEvent` carries `Lat/Lng` as primitives instead of an NTS `Point`, keeping `Venue.Contracts` NTS-free; the handler reconstructs the Point via a static `NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326)`.
   - **Single Changed event vs Created/Updated split:** went with one `Changed` event per aggregate. Projection handler is upsert-by-id (idempotent), so distinguishing create/update gains nothing here. If a future consumer needs the distinction it can be added without breaking this contract.

   **Publish points:**
   - `ArtistService.CreateAsync` + `ArtistService.UpdateAsync` — emit `ArtistChangedEvent` after `SaveChangesAsync`. New ctor dep `IIntegrationEventBus eventBus`. Event payload built via private `ToChangedEvent(ArtistEntity, IEnumerable<int> genreIds)` helper that pulls genre IDs from the request (request is the canonical source — entity's `ArtistGenres` collection isn't always loaded post-create).
   - `VenueService.CreateAsync` + `VenueService.UpdateAsync` — same shape. Private `ToChangedEvent(VenueEntity)` helper. `Latitude=Location?.Y, Longitude=Location?.X` (NTS Point convention: X=lng, Y=lat per OGC).

   **Handlers (Concert.Infrastructure/Handlers/, both `internal`):**
   - `ArtistReadModelProjectionHandler : IIntegrationEventHandler<ArtistChangedEvent>` — `Include(a => a.Genres).FirstOrDefaultAsync(a => a.Id == e.ArtistId)`; insert-or-update with explicit Id assignment (`ArtistReadModel.Id = e.ArtistId`). Genre sync via `desired.Except(current)` add + `current.Except(desired)` remove. `await db.SaveChangesAsync(ct)` per call.
   - `VenueReadModelProjectionHandler : IIntegrationEventHandler<VenueChangedEvent>` — `FirstOrDefaultAsync(v => v.Id == e.VenueId)`; insert-or-update; reconstruct Point from event Lat/Lng via static GeometryFactory (SRID 4326). No `IGeometryProvider` dep — Concert.Infrastructure can't reference `Concertable.Application` per MM rules; static NTS factory is self-contained.

   **DI registration (Concert.Infrastructure/Extensions/ServiceCollectionExtensions.AddConcertModule):** both handlers added as `services.AddScoped<IIntegrationEventHandler<XChangedEvent>, XReadModelProjectionHandler>()`. Same module-extension method that already registers `ReviewCreatedDomainEventHandler`. AddConcertModule isn't actually wired into Web/Workers yet (that's Step 11) — handlers don't fire until then.

   **csproj refs added** to `Concertable.Concert.Infrastructure.csproj`:
   - `..\..\Artist\Concertable.Artist.Contracts\Concertable.Artist.Contracts.csproj`
   - `..\..\Venue\Concertable.Venue.Contracts\Concertable.Venue.Contracts.csproj`

   These are the only cross-module refs Concert.Infrastructure needs for §6b (Contracts only — CLAUDE rule 1). Artist.Contracts and Venue.Contracts already only ref Shared.Domain + Shared.Contracts, so the dep graph stays clean.

   **Build:** still exactly 10 intentional CS1061 errors from Step 3 (4 workflows + 2 ticket payment services in legacy `Concertable.Infrastructure`, deferred to Step 7). Zero new errors. Tests still RED on PendingModelChangesWarning until Steps 13–14.

   **Deferred:**
   - **Backfill** for production — TBD at deployment per plan note. Dev/test seeders will populate read models directly in Step 10.
   - **Wiring through `AddConcertModule()`** — handlers are registered inside the extension but the extension itself isn't called yet. Step 11.
   - **CreateAsync/UpdateAsync test coverage** for the publish behaviour — covered indirectly by integration tests once the read-model tables exist (Step 14+).

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

   ✅ **Done 2026-04-22.**

   **Files moved (from `Concertable.Infrastructure/Services/...` to
   `Concert.Infrastructure/Services/...`, all `internal`):**
   - `Services/Concert/{ConcertService, ConcertDraftService, ContractService,
     OpportunityApplicationService, OpportunityService}` → `Concert.Infrastructure/Services/`.
   - `Services/Application/{DeferredConcertService, UpfrontConcertService,
     DoorSplitConcertWorkflow, FlatFeeConcertWorkflow, VenueHireConcertWorkflow,
     VersusConcertWorkflow}` → same path under Concert.Infrastructure.
   - `Services/{Accept/AcceptDispatcher, Complete/FinishedDispatcher,
     Settlement/SettlementDispatcher}` → same paths.
   - `Services/Webhook/{WebhookProcessor, WebhookQueue, WebhookService,
     TicketWebhookHandler, SettlementWebhookHandler}` → `Concert.Infrastructure/Services/Webhook/`.
   - `Services/Review/{ConcertReviewService, ArtistReviewService, VenueReviewService}`
     → same paths under Concert.Infrastructure (Review services flipped from `public` to
     `internal`; their `IXReviewRepository` interfaces stay public in legacy
     `Concertable.Application/Interfaces/` until the rating-pipeline rewrite — internal
     impl can implement public interface, fine).

   **Repositories moved (legacy `Repository<T>` → `IdModuleRepository<T, ConcertDbContext>`):**
   - `Repositories/Concert/{ConcertRepository, ConcertBookingRepository, ContractRepository,
     OpportunityRepository, OpportunityApplicationRepository}` → `Concert.Infrastructure/Repositories/`.
   - `Repositories/Review/{ConcertReviewRepository, ArtistReviewRepository,
     VenueReviewRepository}` → `Concert.Infrastructure/Repositories/Review/`. These
     don't fit `IdModuleRepository` (composite Ticket-PK semantics on `ReviewEntity`),
     so they keep direct `ConcertDbContext` injection — flipped from `public` to `internal`.

   **`IConcertBookingRepository.GetByConcertIdAsync` Include extension:** added
   `.Include(b => b.Application).ThenInclude(a => a.Artist)` and the existing
   `.ThenInclude(o => o.Venue)` so the booking returns the Artist/Venue read-model navs the
   workflow rewrites need. (`GetByIdAsync` already loaded both, but `GetByConcertIdAsync`
   was the call path used by Versus/DoorSplit workflows + payment ticket services.)

   **IManagerModule rewrites (10 callers from Step 3 deferred list):**
   - `Versus/DoorSplitConcertWorkflow.FinishedAsync(concertId)` — load booking via new
     `bookingRepository.GetByConcertIdAsync(concertId)`, then
     `managerModule.GetByIdAsync(booking.Application.Opportunity.Venue.UserId)` +
     `managerModule.GetByIdAsync(booking.Application.Artist.UserId)`. New ctor dep
     `IConcertBookingRepository` on both workflows.
   - `FlatFee/VenueHireConcertWorkflow.InitiateAsync(applicationId, ...)` — use existing
     `IOpportunityApplicationRepository.GetArtistAndVenueByIdAsync(id)` (returns
     `(ArtistReadModel, VenueReadModel)?`); call `managerModule.GetByIdAsync(venue.UserId)`
     and `managerModule.GetByIdAsync(artist.UserId)`. New ctor dep
     `IOpportunityApplicationRepository` (replaces unused `IConcertRepository`).
   - `VenueTicketPaymentService.PayAsync(concertId, ...)` (stays in legacy
     `Concertable.Infrastructure/Services/Payment/` per §3) — inject
     `IConcertBookingRepository` (now in `Concert.Infrastructure`, accessible to legacy
     `Concertable.Infrastructure` via existing `[InternalsVisibleTo("Concertable.Infrastructure")]`
     in `Concert.Application/AssemblyInfo.cs`); load booking, call
     `managerModule.GetByIdAsync(booking.Application.Opportunity.Venue.UserId)`.
   - `ArtistTicketPaymentService.PayAsync(concertId, ...)` — same shape, calls
     `managerModule.GetByIdAsync(booking.Application.Artist.UserId)`.
   - **Test mock rewrites** — `DoorSplit/VersusApplicationServiceCompleteTests` previously
     mocked the deleted `IManagerModule.GetXManagerByConcertIdAsync(int)`. Replaced with
     mocks for `IConcertBookingRepository.GetByConcertIdAsync` returning a hand-built
     `ConcertBookingEntity` (helper `BookingFactory.Create(artistUserId, venueUserId)`
     uses `Activator.CreateInstance(..., NonPublic)` to bypass private setters on
     `OpportunityApplicationEntity`/`OpportunityEntity`) plus `IManagerModule.GetByIdAsync`
     mocks keyed on UserId.
   - `FlatFee/VenueHireConcertWorkflowCompleteTests` — added the
     `IOpportunityApplicationRepository` ctor arg.

   **csproj refs added to `Concertable.Concert.Infrastructure.csproj`:**
   - `Concertable.Concert.Application` — needed for the now-internal interfaces
     (`IConcertService`, `IConcertRepository`, etc.).
   - `Concertable.Application` + `Concertable.Core` — payment/ticket/notification interfaces
     and `Core.Enums.MessageAction`/parameters still in legacy.
   - `Concertable.Identity.Contracts` + `Concertable.Search.Contracts` +
     `Concertable.Payment.Contracts` — cross-module Contracts (`ICurrentUser`,
     `IConcertHeaderModule`, payment shared types).
   - `Concertable.Search.Application` — TEMPORARY for `IRatingSpecification<T>`
     consumption in `ConcertRepository`. Removed during the rating-pipeline rewrite
     (MM_NORTH_STAR §5).
   - `Concertable.Infrastructure` — TEMPORARY for payment services
     (`ICustomerPaymentService`, keyed `IManagerPaymentService`, `IBackgroundTaskQueue`,
     `IWebhookStrategyFactory`, `Concertable.Infrastructure.Helpers.ToPaginationAsync`,
     `Concertable.Infrastructure.Mappers.QueryableConcertMappers/QueryableReviewMappers`).
     Replaced with `Concertable.Payment.Contracts` ref during Payment Stage 1.

   **`InternalsVisibleTo` grants (TEMPORARY) on `Concertable.Infrastructure/AssemblyInfo.cs`:**
   - `Concertable.Concert.Infrastructure`, `Concertable.Workers`, `Concertable.Web`,
     `Concertable.Infrastructure.UnitTests`, `Concertable.Web.IntegrationTests` —
     so legacy internal helpers (Queryable mappers, factories, dispatchers, background
     queue) remain accessible during the transition. Removed when Payment extracts and
     these helpers move into Concert.Infrastructure or out to Payment.Infrastructure.

   **`Concertable.Workers` integration:**
   - Added project ref to `Concertable.Concert.Infrastructure`.
   - `Workers/ServiceCollectionExtensions.cs` updated `using` block to point at the new
     namespaces (`Concertable.Concert.Infrastructure.{Repositories,Services.{Accept,
     Application,Complete,Settlement}}`) — kept `Concertable.Infrastructure.Services.Payment`
     for `ManagerPaymentService` (Payment-internal, doesn't move).
   - `Workers/Functions/ConcertFinishedFunction.cs` flipped `public class` → `internal class`
     (its ctor takes the now-internal `IConcertRepository` + `IFinishedDispatcher`). Workers
     only runs Functions internally; xUnit test compatibility handled via new
     `Concertable.Workers/AssemblyInfo.cs` granting
     `[InternalsVisibleTo("Concertable.Workers.UnitTests")]`.

   **`Concertable.Web` host changes:**
   - `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` `using` block updated to
     the new `Concertable.Concert.Infrastructure.*` namespaces (legacy
     `Repositories/Rating`, `Services/{Email,Geometry,Payment,Rating,Blob}` stay).
   - All Web Concert-touching controllers/handlers/mappers flipped `public` → `internal`:
     `Controllers/{Concert,Contract,Dev,Opportunity,OpportunityApplication}Controller`,
     `Handlers/{ConcertPostedHandler, IConcertPostedHandler}`,
     `Mappers/ConcertResponseMappers`. They inject the now-internal Concert services
     (CS0051 prevents public class wrapping internal types regardless of
     `InternalsVisibleTo`).
   - New `Web/Extensions/InternalControllerFeatureProvider.cs` (mirrors the per-Module.Api
     copies) registered into `AddControllers(...).ConfigureApplicationPartManager(...)` in
     `Program.cs` so the now-internal Web controllers are still discovered by MVC.

   **TEMPORARY public-flips in `Concert.Application` for tests** (xUnit `[Fact]` + `[Theory]`
   classes must be public, and their public method/property signatures can't carry internal
   types — public wrappers force a propagation). Each annotated with a `// TEMPORARY:`
   comment naming the consumer; flip back internal once the corresponding test moves out:
   - `IConcertWorkflowStrategy`, `IContractStrategy` — consumed by
     `ContractStrategyFactoryTests.ContractTypeStrategies` `TheoryData`.
   - `IAcceptOutcome`/`ImmediateAcceptOutcome`/`DeferredAcceptOutcome`,
     `IFinishOutcome`/`ImmediateFinishOutcome`/`DeferredFinishOutcome` — return types on
     the now-public `IConcertWorkflowStrategy` so they cascade.
   - `IContract`, `OpportunityRequest`, `UpdateConcertRequest` — consumed by
     `Web.IntegrationTests/Controllers/Opportunity/{OpportunityApiTests,
     OpportunityRequestBuilders}` and `Concert/ConcertRequestBuilders`.
   - `IConcertNotificationService` — `Web.IntegrationTests/Infrastructure/Mocks/IMockNotificationService`
     extends it as a public mock interface.

   **Concert.Infrastructure project skeleton in place:**
   - `AssemblyInfo.cs` with `InternalsVisibleTo` for `Concert.Api`,
     `Web.IntegrationTests`, `Infrastructure.UnitTests`, `Infrastructure.IntegrationTests`,
     `Workers.UnitTests`, plus TEMPORARY grants for `Workers`, `Web`,
     `Concertable.Infrastructure`.
   - `GlobalUsings.cs` consolidating `Concertable.Shared`, the cross-module Contracts,
     `Concert.{Domain,Application.{Interfaces,DTOs,Mappers,Requests,Responses,Validators}}`,
     and the legacy `Concertable.{Application.{Interfaces,DTOs,Mappers,Requests,Responses},Core.Parameters,Data.Infrastructure,Data.Infrastructure.Data}`.

   **Build:** `dotnet build Concertable.sln` — **0 errors**, 43 warnings (existing nullable-
   ref + transitive package warnings, all pre-existing). The 10 deferred CS1061 callers
   from Step 3 are gone — workflows + payment ticket services compile cleanly against the
   read-model UserId hop.

   **Tests still RED at runtime** on `PendingModelChangesWarning` until Steps 13–14 (no
   migration yet); compilation is green.

   **Deferred from Step 7:**
   - `OpportunityApplicationController.cs` is now `internal` but still injects `IOpportunityApplicationValidator`/`IArtistModule`/`IOpportunityService` directly — Step 9 moves it into `Concert.Api` where this is the standard shape.
   - `DevController` flipped to `internal` — kills the public `/api/dev/*` discovery path. If those routes are still used outside development, Step 9 needs to either keep DevController in Web (and route through `IConcertLifecycleModule` once that lands per §4b) or move into Concert.Api.
   - The `// TEMPORARY:` public-flips in Concert.Application carry follow-up tags so the cleanup is searchable post-extraction. None block subsequent steps.

8. **Extend `IConcertModule` + `ConcertModule`** stubs (they exist; add the empty interface
   per §4 and the stub impl).

   ✅ **Done 2026-04-22.** `IConcertModule` (empty, public) created in
   `Concert.Contracts/IConcertModule.cs`. `internal sealed class ConcertModule : IConcertModule`
   created in `Concert.Infrastructure/ConcertModule.cs`. Registered via
   `services.AddScoped<IConcertModule, ConcertModule>()` in `AddConcertModule()`. Build 0 errors.

9. **Create `Concert.Api`** with all 4–6 controllers (internal) +
   `InternalControllerFeatureProvider`. Inject internal services directly.

   ✅ **Done 2026-04-22.** Files moved from `Concertable.Web/` into
   `Concertable.Concert.Api/`:
   - `Controllers/` (5 files): `ConcertController`, `ContractController`,
     `OpportunityController`, `OpportunityApplicationController`, `DevController` — all
     `internal class`, namespace `Concertable.Concert.Api.Controllers`.
   - `Handlers/` (2 files): `IConcertPostedHandler` + `ConcertPostedHandler` — `internal`,
     namespace `Concertable.Concert.Api.Handlers`. Registration moved from
     `Concertable.Web/Extensions/ServiceCollectionExtensions.cs:181` to `AddConcertApi()`.
   - `Mappers/ConcertResponseMappers.cs` — `internal static`, namespace
     `Concertable.Concert.Api.Mappers`.
   - `Responses/ConcertResponses.cs` (5 records) — flipped `public record` → `internal record`,
     namespace `Concertable.Concert.Api.Responses`.
   - `Requests/ApplicationRequests.cs` (`AcceptApplicationRequest`) — flipped to `internal`,
     namespace `Concertable.Concert.Api.Requests`.

   `Extensions/InternalControllerFeatureProvider.cs` added (mirrors Venue.Api copy).
   `Extensions/ServiceCollectionExtensions.cs` exposes `public static AddConcertApi(this
   IServiceCollection)` which calls `AddConcertModule()`, registers `IConcertPostedHandler`,
   then `AddControllers().AddApplicationPart(typeof(ConcertController).Assembly)
   .ConfigureApplicationPartManager(apm => apm.FeatureProviders.Add(new
   InternalControllerFeatureProvider()))`.

   `GlobalUsings.cs` (Concert.Api) extended with `Concertable.Concert.Domain`,
   `Concert.Application.{Interfaces,DTOs,Requests,Responses}`. `AssemblyInfo.cs` added
   granting `InternalsVisibleTo("Concertable.Web.IntegrationTests")` so test files can see
   `internal record` response types. The 9 integration test files using
   `Concertable.Web.{Responses,Requests,Mappers,Handlers}` updated to
   `Concertable.Concert.Api.{Responses,Requests,Mappers,Handlers}` via sed.

   `Concertable.Web` host changes:
   - `Concertable.Web.csproj` adds `<ProjectReference>` to `Concertable.Concert.Api`. Kept
     `Concertable.Concert.Infrastructure` ref (still hosts the bulk of Concert DI in Web's
     `ServiceCollectionExtensions.cs` until Steps 11–12).
   - `Program.cs` swaps `using Concertable.Concert.Infrastructure.Extensions;` →
     `using Concertable.Concert.Api.Extensions;` and `services.AddConcertModule();` →
     `services.AddConcertApi();`. The `.ConfigureApplicationPartManager(apm =>
     apm.FeatureProviders.Add(new Concertable.Web.Extensions.InternalControllerFeatureProvider()))`
     entry deleted from the top-level `AddControllers()` chain.
   - `Web/Extensions/InternalControllerFeatureProvider.cs` deleted (no remaining internal
     controllers in Web — all 5 internal Web controllers were Concert).
   - `Web/Extensions/ServiceCollectionExtensions.cs` removed `using Concertable.Web.Handlers;`
     and replaced the `IConcertPostedHandler` registration line with a `// IConcertPostedHandler
     registered by AddConcertApi()` breadcrumb.
   - 11 files deleted from Web: 5 controllers, 2 handler files,
     `Mappers/ConcertResponseMappers.cs`, `Responses/ConcertResponses.cs`,
     `Requests/ApplicationRequests.cs`, `Extensions/InternalControllerFeatureProvider.cs`.

   **Build:** `dotnet build Concertable.sln` — **0 errors**. Tests still RED at runtime on
   `PendingModelChangesWarning` until Steps 13–14 (no migration yet); compilation green.

   **Deferred from Step 9:**
   - `OpportunityApplicationController` still injects `IOpportunityApplicationValidator`,
     `IArtistModule`, `ICurrentUser`, `IOpportunityService` directly. Per §10h
     ("don't narrow the surface") this is intentional — refactors are post-extraction.
   - `DevController` is now in `Concert.Api` and stays `internal`. The `[Authorize]/api/dev/{accept,complete}`
     routes route through `IAcceptDispatcher`/`IFinishedDispatcher` from
     `Concert.Application.Interfaces` (resolved via Concert.Application's
     `[InternalsVisibleTo("Concertable.Concert.Api")]` grant). The Concert.Application
     TEMPORARY IVT to `Concertable.Web` for dispatchers stays in place until E2EEndpointExtensions
     stops using `IFinishedDispatcher` directly (separate cleanup, see Stage 0 §3).

10. **Add `ConcertDevSeeder` + `ConcertTestSeeder`** in `Concert.Infrastructure/Seeding/`.
    Wire into `DevDbInitializer` and `TestDbInitializer` via `IDevSeeder`/`ITestSeeder`.
    **Precondition for Step 13.** Budget significant time — Concert has the largest seed
    surface of any module (concerts, opportunities, applications, contracts, tickets,
    reviews, transactions).

    ✅ **Done 2026-04-22.** Two seeders created in
    `Concert.Infrastructure/Data/Seeders/` (Order = 3, after Identity 0 / Artist 1 / Venue 2):

    - **`ConcertDevSeeder`** (internal, `IDevSeeder`) — injects `ConcertDbContext`,
      `SeedData`, `TimeProvider`. `MigrateAsync()` calls `context.Database.MigrateAsync(ct)`.
      `SeedAsync()` blocks moved verbatim from `DevDbInitializer.InitializeAsync` (lines
      93–588): 57 Opportunities + 53 OpportunityGenres + 81 OpportunityApplications (with
      embedded `ConcertBookingFactory.{Confirmed,Complete,AwaitingPayment}` and the full
      `seed.{Confirmed/Posted/AwaitingPayment/Finished/Upcoming}*App/Booking` carrier
      population) + 60 ConcertGenres + 99 Tickets + 33 Reviews. All `seed.*App` /
      `seed.*Booking` writes preserved so downstream consumers (`SettlementTransaction` /
      `TicketTransaction` seeding still in legacy DevDbInitializer; integration tests)
      keep working.
    - **`ConcertTestSeeder`** (internal, `ITestSeeder`) — injects same trio. `SeedAsync()`
      moved verbatim from `TestDbInitializer.InitializeAsync` (lines 65–130): 10
      Opportunities (using `seed.Venue.Id` from VenueTestSeeder + `seed.Rock.Id` from genre
      reference data) + 10 OpportunityApplications populating `seed.{FlatFee/Confirmed/
      AwaitingPayment/Versus/DoorSplit/VenueHire/Posted*}App/Booking`. Posted* bookings
      attach `ConcertGenreEntity { GenreId = seed.Rock.Id }` inline.

    **DI extensions** added to
    `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
    `AddConcertDevSeeder()` registers `IDevSeeder → ConcertDevSeeder`,
    `AddConcertTestSeeder()` registers `ITestSeeder → ConcertTestSeeder`. Mirrors
    Venue/Artist pattern.

    **Wiring**:
    - `Concertable.Web/Program.cs` — added
      `using Concertable.Concert.Infrastructure.Extensions;` and
      `services.AddConcertDevSeeder();` after `AddVenueDevSeeder()` inside the
      `IsEnvironment("Testing") == false` branch.
    - `Tests/Concertable.Web.IntegrationTests/Infrastructure/ApiFixture.cs` — added
      `using Concertable.Concert.Infrastructure.Extensions;` and
      `services.AddConcertTestSeeder();` after `AddVenueTestSeeder()` inside
      `ConfigureTestServices`.

    **Strip from legacy initializers**:
    - `DevDbInitializer.cs` — removed Opportunities + OpportunityGenres +
      OpportunityApplications + ConcertGenres + Tickets + Reviews blocks (~496 lines).
      Kept: seeder loop, Genres seeding, Preferences, GenrePreferences, **Settlement +
      TicketTransaction seeding** (Transactions are still owned by `ApplicationDbContext`
      until Payment extracts — Concert seeder running first ensures concert IDs 1–3 exist
      when transactions reference them). Trimmed unused usings: `Concertable.Core.Parameters`,
      `Concertable.Seeding.Fakers` was kept (`ILocationFaker` still injected for parity
      with prior shape and might be needed by future seeding here).
    - `TestDbInitializer.cs` — removed Opportunities + OpportunityApplications blocks
      (~66 lines). Kept: seeder loop + Genres seeding (reference data needed by
      ArtistTestSeeder which assigns genre IDs).

    **Build**: full solution `dotnet build` — **0 errors, 77 warnings** (all pre-existing
    NuGet vulnerabilities + 4 duplicate-using warnings carried from Step 9 sweep).
    Integration tests still RED at runtime on `PendingModelChangesWarning` — expected,
    gated until Steps 13–14.

11. **`AddConcertModule()`** DI extension with the full block from §6. Wire it up in
    `Concertable.Web/Program.cs`.

    ✅ **Done 2026-04-23.**

    **Signature change:** `AddConcertModule(this IServiceCollection services, IConfiguration configuration)`.
    Added `ConcertDbContext` registration with both interceptors. Full §6 block wired:
    - 7 core services (Concert/ConcertDraft/Opportunity/OpportunityApplication/Contract/Upfront/Deferred).
    - 3 keyed review services (Artist/Venue/Concert) — all moved to Concert.Infrastructure.
    - 4 dispatchers (Accept/Finished/Settlement + TicketPayment **TEMPORARY** legacy) + ApplicationAcceptHandler (**TEMPORARY** legacy).
    - 4 keyed workflow strategies (FlatFee/DoorSplit/Versus/VenueHire).
    - `IContractStrategyFactory<>` + `IContractStrategyResolver<>` (**TEMPORARY** legacy impls in Concertable.Infrastructure).
    - Webhook plumbing: Processor + Queue + keyed TicketWebhookHandler/SettlementWebhookHandler. (`IWebhookStrategyFactory` + `IWebhookService` stay in legacy Web `AddContracts/AddStripeServices`.)
    - 8 repositories: Concert/Opportunity/OpportunityApplication/Contract/ConcertBooking + 3 review repos.
    - Mappers singletons: ContractMapper/OpportunityMapper/OpportunityApplicationMapper.
    - `IConcertModule, ConcertModule` (already present, kept).
    - 3 event handlers (already present, kept).
    - `AddValidatorsFromAssemblyContaining<OpportunityDtoValidator>()` — added `FluentValidation.DependencyInjectionExtensions 11.11.0` package to Concert.Infrastructure.csproj.
    - `using Concertable.Infrastructure.Interfaces;` added for `IWebhookProcessor/IWebhookQueue/IWebhookStrategy`.
    - `global using Concertable.Core.Enums;` added to Concert.Infrastructure GlobalUsings.cs for `ReviewType`/`WebhookType`.

    `AddConcertApi(this IServiceCollection services, IConfiguration configuration)` updated to accept and forward `configuration` to `AddConcertModule()`.
    `Web/Program.cs` updated: `services.AddConcertApi(builder.Configuration)`.

12. **Remove Concert registrations from `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`**
    — all services, dispatchers, repositories, keyed workflow strategies. Keep the payment
    services (they stay until Payment extracts). **Also remove the duplicate Concert DI
    block from `api/Concertable.Workers/ServiceCollectionExtensions.cs`** (lines ~68–73 —
    `IFinishedDispatcher`, `ISettlementDispatcher`, four keyed `IConcertWorkflowStrategy`)
    and replace with a single `services.AddConcertModule(cfg)` call. Workers picks up a ref
    to `Concertable.Concert.Infrastructure` in its csproj.

    ✅ **Done 2026-04-23.**

    **Web `ServiceCollectionExtensions.cs`:**
    - Removed Concert-specific usings (Concert.Infrastructure.Services/Repositories namespaces). Kept `Concertable.Concert.Infrastructure.Services.Webhook` for `WebhookService` (still registered in `AddStripeServices()` for real-stripe toggle).
    - `AddServices()`: removed IConcertDraftService/IConcertService/IOpportunityApplicationService/IOpportunityService + 3 keyed IReviewService registrations. Added breadcrumb comments.
    - `AddRepositories()`: removed IConcertRepository/IOpportunityApplicationRepository/IConcertBookingRepository/IOpportunityRepository/IContractRepository + 3 review repo registrations.
    - `AddContracts()`: stripped to payment-only — removed IContractStrategyFactory/Resolver, IContractService, 3 mappers, UpfrontConcertService, DeferredConcertService, 3 dispatchers, ITicketPaymentDispatcher, IApplicationAcceptHandler, 4 keyed IConcertWorkflowStrategy, IWebhookProcessor/Queue, 2 keyed IWebhookStrategy. **Kept:** keyed IManagerPaymentService (×2), ICustomerPaymentService, 4 keyed ITicketPaymentStrategy, IWebhookStrategyFactory.

    **Workers `ServiceCollectionExtensions.cs`:**
    - Added `services.AddScoped<DomainEventDispatchInterceptor>()` to `AddInfrastructure()` (required by ConcertDbContext interceptor chain; Identity module also needs it at runtime).
    - Added `services.AddConcertModule(configuration)` after `AddIdentityModule(configuration)`.
    - Removed `AddRepositories()` method entirely (3 concert repos now in Concert module).
    - Simplified `AddServices()` to payment-only: IPaymentService/IStripeAccountService/IManagerPaymentService.
    - Added using `Concertable.Concert.Infrastructure.Extensions` + `Concertable.Data.Infrastructure.Data`.
    - `Workers/Program.cs`: removed `.AddRepositories()` from the service chain.

    **Build:** `dotnet build Concertable.sln` — **0 errors, 55 warnings** (all pre-existing nullable-ref + NU1900 NuGet warnings). Tests still RED at runtime on `PendingModelChangesWarning` until Steps 13–14.

13. **`ApplicationDbContext` cleanup** — remove all 14 Concert DbSets + their
    `ApplyConfiguration` calls. Scaffold the paired drop migration.

    ✅ **Done 2026-04-23.**

    **AppDb DbSet removal (`api/Concertable.Infrastructure/Data/ApplicationDbContext.cs`):**
    - Stripped 14 Concert DbSets (Concerts, ConcertGenres, ConcertImages, Opportunities,
      OpportunityGenres, OpportunityApplications, ConcertBookings, Reviews, Tickets,
      Contracts, FlatFeeContracts, DoorSplitContracts, VersusContracts, VenueHireContracts).
    - Kept 8 AppDb-owned DbSets: Genres, Messages, Transactions, TicketTransactions,
      SettlementTransactions, Preferences, GenrePreferences, StripeEvents.
    - AppDomain assembly scan in `OnModelCreating` filtered to **exclude**
      `Concertable.Concert.Infrastructure` (`&& n != "Concertable.Concert.Infrastructure"`)
      so Concert configs no longer apply to AppDb's model.

    **Cross-context CLR-nav strip (Payment → Concert):** `TicketTransactionEntity.Concert`
    (`ConcertEntity` nav) and `SettlementTransactionEntity.Booking` (`ConcertBookingEntity`
    nav) were the only thing pulling Concert.Domain into AppDb's model after the assembly
    filter — neither had a single production consumer (verified via grep). Both navs
    deleted from `Concertable.Core/Entities/{Ticket,Concert}TransactionEntity.cs` and the
    matching `HasOne(...)` Fluent calls dropped from
    `Concertable.Data.Infrastructure/Data/Configurations/TransactionConfigurations.cs`.
    `ConcertId` / `BookingId` int FK columns kept (the FK constraint goes with them — see
    drop migration below). Cleaner outcome than the §7 `ExcludeFromMigrations`-with-nav
    plan: Payment.Core stops referencing Concert.Domain entirely.

    **Rating-pipeline rewrite collapsed in (MM_NORTH_STAR §5):** `IRatingRepository`,
    `IRatingEnricher`, `RatingEnricher`, `ArtistRatingRepository`, `VenueRatingRepository`,
    `ConcertRatingRepository` all **deleted**. `IRatingRepository` had **zero production
    consumers** (the only injector was `RatingEnricher`, which itself had no callers and no
    DI registration). The `ArtistRatingProjection` / `VenueRatingProjection` event-driven
    projections already exist (Artist/Venue.Domain) and are already consumed via
    `QueryableArtistMappers.ToHeaderDto/ToSummaryDto/ToDto` and the Venue equivalents
    reading `rating.AverageRating` directly. Net: §5 done as a side-effect of Step 13.

    **Tickets stay in Concert (legacy repo moved + collapsed):** `TicketRepository` (was
    `Concertable.Infrastructure/Repositories/TicketRepository.cs`, public, hit
    `ApplicationDbContext.Tickets`) moved to
    `api/Modules/Concert/Concertable.Concert.Infrastructure/Repositories/TicketRepository.cs`
    as `internal`, switched to `GuidModuleRepository<TicketEntity, ConcertDbContext>`,
    registered in `AddConcertModule()`. Removed from Web's `AddRepositories()`.

    **E2E + integration test cleanup:**
    - `Concertable.Web/Extensions/E2EEndpointExtensions.cs` `/e2e/finish/{concertId}` and
      `/e2e/payment-intent/{applicationId}` switched to inject `IReadDbContext readDb` for
      the `ConcertBookings` query (still inject `ApplicationDbContext db` for
      `SettlementTransactions`).
    - `Tests/Concertable.Web.IntegrationTests/Infrastructure/ApiFixture.cs` adds
      `IReadDbContext ReadDbContext { get; }` alongside the existing `ApplicationDbContext
      DbContext`. Two failing tests
      (`OpportunityApplicationFlatFeeApiTests.Accept_ShouldNotCreateDraft_WhenPaymentFails`
      + the VenueHire equivalent) switched from `fixture.DbContext.Concerts` to
      `fixture.ReadDbContext.Concerts`.

    **Drop migration scaffolded:**
    `api/Concertable.Infrastructure/Migrations/20260423095729_DropConcertTables.cs`
    generated via `dotnet ef migrations add DropConcertTables`. Contains 14 `DropTable`
    + 2 `DropForeignKey` (FK_SettlementTransactions_ConcertBookings_BookingId,
    FK_TicketTransactions_Concerts_ConcertId — the cross-context FKs that came along
    with the now-deleted CLR navs). EF emitted "An operation was scaffolded that may
    result in the loss of data" — expected. Migration retires alongside the other
    InitialCreate migrations during the Step 14 close-out reset (per
    `project_concert_migration_reset.md`).

    **Build:** `dotnet build Concertable.sln` — **0 errors, 91 warnings** (warning growth
    from CLR-nav removal triggering nullable-ref churn on still-mapped entities;
    pre-existing nullable-ref + NU1900 NuGet warnings unchanged). Tests RED at runtime
    on `PendingModelChangesWarning` until Step 14 lands ConcertDbContext InitialCreate.

13b. **Per-module rating projections — finish what Artist/Venue started.**

    ✅ **Done 2026-04-23.**

    **Concert side — projection + handler:**
    - `Concert.Domain/ReadModels/ConcertRatingProjection.cs` (`{ ConcertId PK, AverageRating, ReviewCount }`).
    - `Concert.Infrastructure/Data/Configurations/ConcertRatingProjectionConfiguration.cs` (`internal`, table `ConcertRatingProjections`, `ValueGeneratedNever` on PK — mirrors `ArtistRatingProjectionConfiguration` exactly).
    - `ConcertDbContext.ConcertRatingProjections` DbSet added.
    - `Concert.Infrastructure/Handlers/ConcertReviewProjectionHandler : IIntegrationEventHandler<ReviewSubmittedEvent>` — upsert-by-ConcertId pattern, identical shape to `ArtistReviewProjectionHandler`. Registered in `AddConcertModule()` alongside the Artist/Venue handlers.

    **Per-context split, all internal to Concert (driven by user feedback that EF queries belong in repos, not facades):**
    - `Concert.Application/Interfaces/Reviews/IConcertReviewRepository` — concert-context: `GetByConcertAsync`, `GetSummaryByConcertAsync` (reads `ConcertRatingProjection`), `CanUserReviewConcertAsync`, `AddAsync`, `SaveChangesAsync`.
    - `Concert.Application/Interfaces/Reviews/IArtistReviewRepository` — artist-context: `GetByArtistAsync`, `CanUserReviewArtistAsync`. Renamed methods to be context-explicit (was generic `GetAsync`/`CanReviewAsync`).
    - `Concert.Application/Interfaces/Reviews/IVenueReviewRepository` — venue-context: same shape as artist.
    - `Concert.Application/Interfaces/Reviews/IReviewValidator` — single entry point for can-review checks across all 3 contexts; thin wrapper over the 3 repos.
    - `Concert.Application/Interfaces/Reviews/IConcertReviewService` — concert-context current-user operations: `GetAsync`, `GetSummaryAsync`, `CanCurrentUserReviewAsync`, `CreateAsync`. Replaces the keyed `IReviewService` factory.
    - `Concert.Application/Requests/CreateReviewRequest` + `Concert.Application/Validators/CreateReviewRequestValidator` — moved from legacy `Concertable.Application`, marked `internal`.
    - `Concert.Application/Mappers/ReviewMappers.ToDto` — moved from legacy. Marked `public` per user feedback (cross-module reviewers may need to project ReviewEntity → ReviewDto; ReviewEntity is already accessible via Concert.Domain reference per CLAUDE rule 3).
    - `Concert.Infrastructure/Mappers/QueryableReviewMappers.ToDto` — `internal`, only Concert.Infrastructure projects ReviewEntity to ReviewDto via this `IQueryable` extension.
    - `Concert.Infrastructure/Repositories/Review/{Concert,Artist,Venue}ReviewRepository.cs` — rewrites against the new internal interfaces. ArtistReviewRepository now actually implements `CanUserReviewArtistAsync` (was `throw new NotImplementedException()` in the legacy code — pre-existing bug fixed in passing). All inject `ConcertDbContext + TimeProvider`.
    - `Concert.Infrastructure/Services/Review/ConcertReviewService.cs` — collapses the 3 keyed `IReviewService` services into one. Wraps `IConcertReviewRepository + ITicketRepository + IReviewValidator + ICurrentUser`.
    - `Concert.Infrastructure/Validators/ReviewValidator.cs` — uniform validator delegating to the 3 repos. Migrated from legacy `Concertable.Infrastructure/Validators/ReviewValidator.cs` (public) and made `internal`.

    **Cross-module facade (`IConcertModule`) — minimal, delegates only:**
    ```csharp
    public interface IConcertModule
    {
        Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams);
        Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams);
        Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
        Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
    }
    ```
    `ConcertModule` impl injects `IArtistReviewRepository + IVenueReviewRepository + IReviewValidator` and is a one-line passthrough per method. **No EF in the facade** (early draft put queries inline; user pushed back hard, refactored to abstractions). Concert.Contracts.csproj gained `Concertable.Shared.Contracts` ProjectReference (needed for `IPagination<>`/`IPageParams`/`ReviewDto`).

    **Artist/Venue module review surfaces — own services + own controllers:**
    - `Artist.Application/Interfaces/IArtistReviewService` (`internal`) + `Artist.Infrastructure/Services/ArtistReviewService` impl. Reads `ArtistDbContext.ArtistRatingProjections` directly for `GetSummaryAsync`; delegates to `IConcertModule` for `GetAsync` (lists) and `CanCurrentUserReviewAsync` (which fills in `ICurrentUser.GetId()` then calls `IConcertModule.CanUserReviewArtistAsync`).
    - `Venue.Application/Interfaces/IVenueReviewService` + `Venue.Infrastructure/Services/VenueReviewService` — symmetric to Artist.
    - `Artist.Api/Controllers/ArtistReviewController` (`internal`, `[Route("api/Artist")]`) — `GET {id}/review-summary`, `GET {id}/reviews`, `GET {id}/can-review`.
    - `Venue.Api/Controllers/VenueReviewController` — same shape under `api/Venue`.
    - `Concert.Api/Controllers/ConcertReviewController` (`internal`, `[Route("api/Concert")]`) — `POST {id}/reviews`, `GET {id}/reviews`, `GET {id}/review-summary`, `GET {id}/can-review`. Per user feedback, **separate controllers per concern** rather than bolting endpoints onto the existing aggregate controllers (Artist/Venue/ConcertController).
    - All injected services registered in their respective `AddXModule()` extensions.

    **`ReviewDto`/`ReviewSummaryDto` moved to Shared.Contracts** (per user feedback) — was `Concertable.Application/DTOs/ReviewDtos.cs`, now `Concertable.Shared.Contracts/ReviewDtos.cs` in `Concertable.Shared` namespace. The `Concertable.Shared` global using is already in place across modules so the move is transparent.

    **Deleted files:**
    - `Concertable.Web/Controllers/ReviewController.cs` — entire 67-line controller, replaced by the 3 per-module controllers above.
    - `Concertable.Application/Interfaces/{IReviewService,IReviewServiceFactory,IReviewValidator,IConcertReviewRepository,IArtistReviewRepository,IVenueReviewRepository}.cs` (6 files).
    - `Concertable.Application/Mappers/ReviewMappers.cs`, `Concertable.Application/Requests/ReviewRequests.cs`, `Concertable.Application/Validators/ReviewValidators.cs`, `Concertable.Application/DTOs/ReviewDtos.cs` (4 files).
    - `Concertable.Infrastructure/Validators/ReviewValidator.cs`, `Concertable.Infrastructure/Factories/ReviewServiceFactory.cs`, `Concertable.Infrastructure/Mappers/QueryableReviewMappers.cs` (3 files).
    - `Concertable.Core/Enums/ReviewType.cs` — was the keyed-factory key, no longer needed.
    - `Concert.Infrastructure/Services/Review/{Artist,Venue}ReviewService.cs` — keyed siblings of ConcertReviewService.

    **Web DI cleanup:** `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` — dropped `IReviewServiceFactory, ReviewServiceFactory` registration from `AddServices()` and `IReviewValidator, ReviewValidator` from `AddServiceValidators()`. Both replaced by Concert module wiring.

    **Build:** `dotnet build Concertable.sln` — **0 errors, 72 warnings** (down from 91 — ~20 fewer because dead review-pipeline code retired). Tests still RED at runtime on `PendingModelChangesWarning` until Step 14 (ConcertRatingProjection lands in InitialCreate during the close-out reset).

    **Frontend lockstep:** ~10 fetch sites need URL updates to the new per-module routes per the table below. **Out of scope for this server-side commit** — flag at review time so frontend updates ship in the same PR.

    **Default URL choices locked in (per user — option a "per-module nesting"):**

13b-old. **Per-module rating projections — finish what Artist/Venue started.** Symmetry
    cleanup before InitialCreate migrates. Each rated aggregate (Artist, Venue, Concert)
    owns its own `XRatingProjection` table populated from `ReviewSubmittedEvent`. Concert
    is the missing one — Artist/Venue projections were added during their Stage 1
    extractions (cross-module fanout problem); Concert was skipped because
    `ConcertReviewRepository.GetSummaryAsync` was reading Concert-owned `Reviews` from
    inside Concert (no fanout). Now that the rating-pipeline rewrite collapsed in at
    Step 13, Concert needs the same projection for symmetry + so live aggregation dies
    completely.

    **Concert side:**
    - Add `ConcertRatingProjection(int ConcertId PK, double AverageRating, int ReviewCount)`
      to `Concert.Domain/ReadModels/` (alongside `ArtistReadModel`/`VenueReadModel`).
    - Add `DbSet<ConcertRatingProjection>` to `ConcertDbContext`.
    - Add `ConcertRatingProjectionConfiguration` (internal) — same shape as
      `ArtistRatingProjectionConfiguration` (PK on ConcertId, no FK constraint to Concerts
      since this is an event-sourced projection that survives source deletion).
    - Add `ConcertReviewProjectionHandler : IIntegrationEventHandler<ReviewSubmittedEvent>`
      in `Concert.Infrastructure/Handlers/` — upsert-by-ConcertId pattern, mirrors
      `ArtistReviewProjectionHandler` exactly. Register in `AddConcertModule()`.
    - Rewrite `ConcertReviewRepository.GetSummaryAsync(int id)` to read
      `ConcertDbContext.ConcertRatingProjections.FirstOrDefaultAsync(p => p.ConcertId == id)`,
      project to `ReviewSummaryDto(ReviewCount, AverageRating)`. No more live aggregation.

    **Three endpoint families to modularize, not just summary** — `ReviewController` has
    Artist/Venue/Concert variants of three things, all routed through Concert today:

    **Family A — Summary (the easy one — projection already exists for Artist + Venue):**
    - **Delete from Concert.Infrastructure:** `ArtistReviewService.GetSummaryAsync`,
      `VenueReviewService.GetSummaryAsync` paths (whole keyed services likely retire when
      Family B + C also move — see below).
    - **Add to Artist.Api:** `GET /api/Artist/{id}/review-summary` endpoint reading
      `ArtistDbContext.ArtistRatingProjections` (already populated via existing
      `ArtistReviewProjectionHandler`). Internal `IArtistReviewService` /
      `IArtistReviewRepository` (or just inline the repo call) in `Artist.Application` /
      `Artist.Infrastructure` — same module-internal shape Artist already uses elsewhere.
    - **Add to Venue.Api:** same for `GET /api/Venue/{id}/review-summary`.
    - **Add to Concert (read its own new projection):** `ConcertReviewRepository.GetSummaryAsync`
      reads `ConcertDbContext.ConcertRatingProjections`. Endpoint stays at
      `/api/Review/summary/concert/{id}` until ReviewController moves into Concert.Api.

    **Family B — Paginated review lists (NEEDS DESIGN CALL):**
    Today: `GET /api/Review/{venue|artist|concert}/{id}` returns `IPagination<ReviewDto>`.
    Wrinkle: the source `Reviews` table is Concert-owned; the row PER-REVIEW is what
    `ReviewDto` projects from. Artist/Venue modules can't read Concert's `Reviews` table
    directly (CLAUDE rule 2). Two viable shapes — **pick at impl time**:

    - **(B-i) Per-consumer projection (MM_NORTH_STAR §5 pattern):** `ArtistReview`
      projection table in Artist.Domain (`ReviewId`, `ArtistId`, `Stars`, `Details`,
      `ReviewerEmail`, `CreatedAt`); populated from `ReviewSubmittedEvent` alongside
      `ArtistRatingProjection`. Same for `VenueReview` in Venue.Domain. Lists become
      module-internal queries. Cleanest module boundary; biggest infra add (2 more
      projection tables + handler updates).
    - **(B-ii) Cross-module facade:** `IConcertModule.GetReviewsByArtistAsync(int artistId,
      PageParams)` + `GetReviewsByVenueAsync(int venueId, PageParams)`. Artist.Api /
      Venue.Api controllers thin-proxy through to Concert. Concert keeps the source-of-
      truth read. Less new infra; expands `IConcertModule` facade surface; data still
      crosses the boundary at request time (vs. at event time in B-i).

    **Default recommendation: B-ii** — these endpoints are not on the search/listing
    hot path (they fire on detail-page review-list scrolls), so the cross-module hop is
    cheap; per-consumer projections for cold-read endpoints is over-investment until we
    see a perf signal.

    **Family C — Can-review validation (similar wrinkle to B):**
    Today: `GET /api/Review/{concert|artist|venue}/can-review/{id}` returns `bool`.
    Source data is even more Concert-tangled — checks tickets/concerts/bookings/
    applications to determine "has this user actually attended this thing". All
    Concert-owned. Same two shapes as B:

    - **(C-i)** Per-consumer projection: `UserArtistAttendance` /
      `UserVenueAttendance` projection tables (sparse boolean matrix) populated from a
      `ConcertAttendedEvent` (would need to be introduced — fired when a ticket is
      validated/used or simply at booking time, depending on policy). Heavyweight.
    - **(C-ii)** Cross-module facade: `IConcertModule.CanUserReviewArtistAsync(Guid userId,
      int artistId)` + `CanUserReviewVenueAsync` + `CanUserReviewConcertAsync`. Concert
      keeps the validator; Artist.Api / Venue.Api controllers proxy through. Same
      cost/benefit as B-ii.

    **Default recommendation: C-ii** — same reasoning as B. Can-review is cold (one call
    per detail-page render). Don't invest in projections until the data shape forces it.

    **What gets deleted from Concert in 13b regardless of which path B+C take:**
    - `IArtistReviewRepository`, `IVenueReviewRepository` interfaces — Artist/Venue own
      their reads now (B-i: read their own projection; B-ii: facade is the only consumer
      and it lives on Concert side).
    - `ArtistReviewRepository`, `VenueReviewRepository` impls.
    - `ArtistReviewService`, `VenueReviewService` (the keyed `IReviewService` variants).
    - The `IReviewService` keyed factory collapses — either keep keyed-by-`ReviewType.Concert`
      only, or replace with a plain `IConcertReviewService` and retire the factory entirely.
    - `IReviewValidator.CanUserReviewArtist/Venue` methods if C-ii (they move to a Concert-
      internal method on `IConcertModule`); the bool-returning controller actions move to
      Artist.Api/Venue.Api as thin facade-callers.
    - `ReviewController.GetSummaryByArtistId`, `GetSummaryByVenueId`, `GetByVenueId`,
      `GetByArtistId`, `CanUserReviewArtistId`, `CanUserReviewVenueId` from
      `Concertable.Web/Controllers/` — only the Concert-suffixed actions + the single
      `POST /api/Review` (Create, already Concert-scoped) survive there.

    **URL shape (locked in) — symmetric per-module nesting for ALL three families:**
    Web's `ReviewController` is **deleted entirely** at the end of 13b — every endpoint
    relocates to its owning module's controller under a per-aggregate URL. Frontend
    updates ~10 fetch sites in lockstep with this commit.

    | Old route | New route | New owner |
    |---|---|---|
    | `POST /api/Review` (always Concert) | `POST /api/Concert/{id}/reviews` | Concert.Api |
    | `GET /api/Review/concert/summary/{id}` | `GET /api/Concert/{id}/review-summary` | Concert.Api |
    | `GET /api/Review/artist/summary/{id}` | `GET /api/Artist/{id}/review-summary` | Artist.Api |
    | `GET /api/Review/venue/summary/{id}` | `GET /api/Venue/{id}/review-summary` | Venue.Api |
    | `GET /api/Review/concert/{id}` (list) | `GET /api/Concert/{id}/reviews` | Concert.Api |
    | `GET /api/Review/artist/{id}` (list) | `GET /api/Artist/{id}/reviews` | Artist.Api |
    | `GET /api/Review/venue/{id}` (list) | `GET /api/Venue/{id}/reviews` | Venue.Api |
    | `GET /api/Review/concert/can-review/{concertId}` | `GET /api/Concert/{id}/can-review` | Concert.Api |
    | `GET /api/Review/artist/can-review/{artistId}` | `GET /api/Artist/{id}/can-review` | Artist.Api |
    | `GET /api/Review/venue/can-review/{venueId}` | `GET /api/Venue/{id}/can-review` | Venue.Api |

    **`Concertable.Web/Controllers/ReviewController.cs` deletes entirely** at end of 13b
    — every endpoint has a new home. Same for `IReviewServiceFactory` registration in
    Web (the factory itself is already proposed to retire in favour of plain
    `IConcertReviewService`). No extra facade surface needed for the Concert-side reads
    (Concert.Api controllers inject Concert internal services directly). Cross-module
    facade surface for B-ii / C-ii adds `IConcertModule.GetReviewsByArtistAsync` etc. as
    detailed above.

    **Cleanup:**
    - Delete `QueryableReviewMappers.ToSummaryDto` (in `Concertable.Infrastructure/Mappers/`)
      — only callers are the deleted Artist/Venue/Concert review repos.
    - Audit `IReviewService` keyed factory — likely collapses to `IConcertReviewService`
      (Concert is the only remaining keyed registration).

    **Out of scope for 13b** (stays for a Search rewrite later):
    - Search `ConcertHeaderRepository` / `ArtistHeaderRepository` / `VenueHeaderRepository`
      still call `IRatingSpecification<T>.ApplyAggregate(context.Reviews)` per page-load —
      same wasted compute the rating-pipeline rewrite was supposed to kill on the listing
      hot path. Switching them to the projections requires `IReadDbContext` to expose
      `IQueryable<ArtistRatingProjection>` / `IQueryable<VenueRatingProjection>` /
      `IQueryable<ConcertRatingProjection>` (Domain-leak via the read shim, fine per
      CLAUDE rule 3). Tracked separately under MM_NORTH_STAR §5; do not bundle into 13b.

    **Done before Step 14** so the new `ConcertRatingProjection` lands in `ConcertDbContext`
    `InitialCreate` rather than as a follow-up `AddConcertRatingProjection` migration that
    would get wiped by the Step 14 close-out reset anyway.

14. **Scaffold `ConcertDbContext` `InitialCreate` migration.**

    ✅ **Done 2026-04-23.**

    **Genre cross-context problem + SharedDbContext fix.** Concert has three join
    entities with `GenreEntity` navs (`ConcertGenreEntity`, `OpportunityGenreEntity`,
    `ArtistReadModelGenre`). First scaffold attempts emitted `table.ForeignKey(...
    principalTable: "Genres" ...)` constraints, which would fail at apply time because
    Concert migrations run before `ApplicationDbContext` in the dep order. Tried
    `modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations())`
    in three positions (before/after assembly scan + as a dedicated
    `IEntityTypeConfiguration<GenreEntity>`) — all suppressed the table CREATE but EF
    still emitted the FK constraints (works for Artist with one Genre nav; doesn't for
    Concert with three; suspected EF Core 10.0.3 quirk).

    **Resolution:** introduced `SharedDbContext` in `Concertable.Data.Infrastructure/Data/`
    that owns `Genres` (and other shared reference data). Migration order updated to
    Shared → Identity → Artist → Venue → Concert → AppDb so the Genres table exists
    before Concert applies. FK constraints to Genres are now legitimate cross-context
    references that satisfy at apply time. Each module DbContext (Artist, Venue,
    Concert) and `ApplicationDbContext` keeps a single
    `modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations())`
    line so its own migration doesn't try to recreate Genres.

    **Concert migration result:** `20260423121237_InitialCreate.cs` — 18 CreateTables
    (17 Concert-owned + ArtistReadModelGenres), no Genres CreateTable, FKs to Genres
    kept as legitimate cross-context references. Build green: 0 errors, 77 warnings.

    See `project_shared_dbcontext.md` for the codified pattern (was previously a
    "future idea" memory; promoted to current state).

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
