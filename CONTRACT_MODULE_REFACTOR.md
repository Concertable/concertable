# Contract Module Extraction — Implementation Plan

---

## ▶️ RESUME HERE (post-compact, 2026-04-25 — Step 13 CLOSED, refactor complete)

**Steps 0–13 are DONE. Build green, full integration suite green (122/122).** Contract
module extraction is finished. Read this block before resuming any follow-up.

### Step 13 final shape

Full integration suite passed after fixing one bug surfaced by the new Contract seeder.

**Bug:** `ContractTestSeeder` originally added 10 mixed-subtype contracts via a single
`AddRange` + `SaveChangesAsync`, then `ConcertTestSeeder` referenced contracts by literal
`contractId: 1..10`. EF Core 10 reorders TPT inserts by entity type when batching, so the
auto-IDENTITY ids did NOT come out in array order — `Contracts.Id = 5` ended up being a
FlatFee row instead of the expected DoorSplit. Cascading effect: dispatcher resolved the
WRONG `ContractType` for DoorSplitApp/VersusApp/FlatFeeApp → wrong workflow strategy →
either no draft concert created (→ GET 404) or no Stripe call (→ webhook throw). All 3
accept tests failed for this single root cause.

**Fix (named-seed pattern, matches how every other seed entity is referenced):**
- `Concertable.Core.csproj` — added `Concertable.Contract.Domain` ProjectReference (Core
  already references the other module Domains; same escape-hatch pattern).
- `Concertable.Seeding/GlobalUsings.cs` — added `Concertable.Contract.Domain` global using.
- `SeedData.cs` — added 10 typed contract properties (`FlatFeeAppContract`,
  `ConfirmedAppContract`, `AwaitingPaymentAppContract`, `VersusAppContract`,
  `DoorSplitAppContract`, `VenueHireAppContract`, `PostedFlatFeeAppContract`,
  `PostedDoorSplitAppContract`, `PostedVersusAppContract`, `PostedVenueHireAppContract`).
- `ContractTestSeeder.cs` — assigns each contract onto `seed.<App>Contract` before
  `AddRange`, so the real EF-assigned id is captured post-`SaveChangesAsync`. Also
  injects `SeedData`.
- `ConcertTestSeeder.cs` — replaced literal `contractId: 1..10` with
  `seed.<App>Contract.Id` references.

The Step-10 footnote claiming "auto-IDENTITY id generation gives ids 1..N matching
insertion order" was wrong for TPT — superseded by this fix. **Do NOT** rely on
insertion-order id assignment for any future TPT seeder; always reference `.Id`
post-save.

**Test infra additions (kept):**
- `Concertable.Tests.Common/HttpClientExtensions.cs` — new `GetAssertAsync<T>` helper
  that surfaces response body on non-2xx (was throwing generic `HttpRequestException`
  with no body, which made debugging the original 404 harder than it needed to be).
- 3 accept-test files (`OpportunityApplication{DoorSplit,Versus,FlatFee}ApiTests.cs`)
  switched their post-Accept GET from `GetAsync<T>` to `GetAssertAsync<T>`.

**ContractDevSeeder NOT touched** — the dev seeder writes 59 contracts in one batch
with the same TPT-reorder hazard. Dev-mode tests don't exist yet to surface the
problem, but the seeder's id-coupled comments (`contract 50 = DoorSplit @70%` etc.)
will silently lie until either (a) the dev seeder is migrated to the named-seed
pattern, or (b) the assertions are rewritten to look up by type rather than id.
Track this if/when E2E tests start failing on contract-type-specific behavior.

### Step 12 final shape

### Step 12 final shape

All 7 module migration folders deleted and re-scaffolded `InitialCreate` in dependency order:
Shared → Identity → Artist → Venue → Concert → **Contract** → AppDb. Timestamps fall in that
exact order (`11:48:53` Shared → `11:52:03` AppDb). Files now live at:

- `Concertable.Data.Infrastructure/Data/Migrations/20260425114853_InitialCreate.{cs,Designer.cs}` + `SharedDbContextModelSnapshot.cs`
- `Modules/Identity/Concertable.Identity.Infrastructure/Data/Migrations/20260425114930_InitialCreate.{cs,Designer.cs}`
- `Modules/Artist/Concertable.Artist.Infrastructure/Data/Migrations/20260425114956_InitialCreate.{cs,Designer.cs}`
- `Modules/Venue/Concertable.Venue.Infrastructure/Data/Migrations/20260425115036_InitialCreate.{cs,Designer.cs}`
- `Modules/Concert/Concertable.Concert.Infrastructure/Data/Migrations/20260425115103_InitialCreate.{cs,Designer.cs}`
- `Modules/Contract/Concertable.Contract.Infrastructure/Data/Migrations/20260425115132_InitialCreate.{cs,Designer.cs}` (NEW — first Contract migration)
- `Concertable.Infrastructure/Migrations/20260425115203_InitialCreate.{cs,Designer.cs}` + `ApplicationDbContextModelSnapshot.cs`

**Concert migration** confirms post-Step-9 schema:
- `Opportunities.ContractId` column present (`int NOT NULL`), no FK constraint declared
  (cross-context — principal `Contracts.Id` lives in Contract migration; matches
  `feedback_cross_context_fk.md` rule that module-context FKs to other modules' principals
  drop the SQL FK but keep the CLR property/index).
- `IX_Opportunities_ContractId` is a UNIQUE index (1:1 enforcement per
  `OpportunityEntityConfiguration` line 23).
- **Zero** `Contracts` / `FlatFeeContracts` / `DoorSplitContracts` / `VersusContracts` /
  `VenueHireContracts` table creates — Concert no longer owns those.

**Contract migration** (NEW) confirms post-Step-8 ownership:
- `EnsureSchema("contract")`.
- 5 tables (`Contracts` base + 4 subtypes) all in `contract` schema, TPT-style.
- `Contracts.Id` is `SqlServer:Identity` (1, 1) — auto-IDENTITY drives Step-10 seeder
  insertion-order matching for the `contractId: 1..N` references in
  `ConcertDevSeeder` / `ConcertTestSeeder`.
- Subtype tables (`PK_DoorSplitContracts` etc.) use `Id` as both PK and FK back to
  `Contracts.Id` (intra-context, full FK constraint declared).

**AppDb migration** stayed Contract-free — only creates `Messages`, `Preferences`,
`StripeEvents`, `Transactions`, `GenrePreferences`, `SettlementTransactions`,
`TicketTransactions`. Cross-schema FKs to `Users` (identity) and `Genres` (dbo/shared) ride
through cleanly because AppDb migrates last.

**Build expectation:** `dotnet build Concertable.sln` after re-scaffold — 0 errors, 91
warnings (decimal-precision noise + duplicate-using cleanup, all pre-existing). Tests not yet
run — Step 13.

### Pre-Step-12 state (for context)

### Step 11 final shape

IVT audit covered 4 AssemblyInfo files (Contract.Application, Contract.Infrastructure,
Concert.Application, Concert.Infrastructure). Method: greppped each suspected legacy host for
actual usage of internals before removing.

**Removed (no actual consumer):**
- `Contract.Application/AssemblyInfo.cs` — entire TEMPORARY block (`Concertable.Application`,
  `.Infrastructure`, `.Workers`, `.Web`). None of those assemblies reference any Contract.Application
  internal type post-Step-9 — IContractModule (public, in Contracts) covers all cross-module reads.
- `Contract.Infrastructure/AssemblyInfo.cs` — `Concertable.Workers` and `Concertable.Web`. Both
  only consume the public `AddContractModule()` extension, not internals.
- `Concert.Infrastructure/AssemblyInfo.cs` — `Concertable.Workers` (only refs public
  `Concert.Infrastructure.Extensions`), `Concertable.Infrastructure` (no usage), and
  `Concertable.Search.Infrastructure` (no usage).

**Kept (still actively consumed):**
- `Contract.Application/AssemblyInfo.cs` — Contract.Infrastructure, Contract.Api, sibling test
  projects, DynamicProxyGenAssembly2, Concert.Infrastructure (ride-along EF config registration).
- `Contract.Infrastructure/AssemblyInfo.cs` — Contract.Api, integration/unit test assemblies,
  DynamicProxyGenAssembly2 (strong-named).
- `Concert.Application/AssemblyInfo.cs` — ALL TEMPORARY entries kept; comments updated to drop
  stale `IContractStrategyResolver` reference and reflect post-Step-9 reality. `Concertable.Infrastructure`
  still hosts Payment/Ticket services consuming `IConcertRepository`/`IContractLookup`/`ITicketPaymentStrategy`;
  `Concertable.Workers` still hosts `ConcertFinishedFunction` consuming `IConcertRepository`/`ICompletionDispatcher`;
  `Concertable.Web` still keyed-registers `ITicketPaymentStrategy` impls + injects `ICompletionDispatcher`
  in `E2EEndpointExtensions`.
- `Concert.Infrastructure/AssemblyInfo.cs` — `Concertable.Web` kept (injects internal `WebhookService`);
  comment updated to reflect that's the sole reason it lingers.

**No build break** — confirmed with `dotnet build`, 0 errors.

### Steps closed at this point (Step 11)

- **Step 12** — Migration re-scaffold. **DONE 2026-04-25.** See "Step 12 final shape" block above.
- **Step 13** — Full test suite pass. Add `DynamicProxyGenAssembly2` IVT entries to any module
  whose internals get newly mocked (per `feedback_castle_proxy_ivt.md`). PENDING.

### Step 10 final shape

`ContractDevSeeder` + `ContractTestSeeder` live at
`api/Modules/Contract/Concertable.Contract.Infrastructure/Data/Seeders/`. Both `Order = 3` and inject
`ContractDbContext` (the dedicated context from Step 8). To make Contract seed BEFORE Concert,
Concert seeders bumped from `Order = 3` → `Order = 4`. (Plan-file footnote about `Order = 3.5`
turned out wrong — `IModuleSeeder.Order` is `int`, and the right ordering is just Contract=3,
Concert=4.)

- `ContractDevSeeder` writes 59 contracts via subtype factories (`FlatFee/DoorSplit/Versus/VenueHire ContractEntity.Create(...)`).
  Type/value mix lines up with Concert dev seeder's named applications:
  contract 21 = VenueHire (PostedVenueHire), 31 = FlatFee (PostedFlatFee),
  50 = DoorSplit @70% (FinishedDoorSplit — E2E expects £14 share),
  51 = Versus £100 + 70% (FinishedVersus — E2E expects £114 share),
  53 = DoorSplit (PostedDoorSplit), 54 = Versus (PostedVersus),
  58 = FlatFee (UpcomingFlatFee), 59 = VenueHire (UpcomingVenueHire).
- `ContractTestSeeder` writes 10 contracts matching Concert test seeder slots:
  1–3 FlatFee, 4 Versus, 5 DoorSplit, 6 VenueHire, 7 FlatFee, 8 DoorSplit, 9 Versus, 10 VenueHire.
- Auto-IDENTITY id generation gives ids 1..N matching insertion order, so the placeholder
  `contractId: <int>` references already in `ConcertDevSeeder` / `ConcertTestSeeder` are
  correct — no rewrite needed there beyond removing one stale TODO comment.
- `AddContractDevSeeder()` / `AddContractTestSeeder()` extensions added to
  `Concertable.Contract.Infrastructure/Extensions/ServiceCollectionExtensions.cs`. Wired in
  `Concertable.Web/Program.cs` (between Venue and Concert dev seeders) and
  `Concertable.Web.IntegrationTests/Infrastructure/ApiFixture.cs` (between Venue and Concert test
  seeders).
- `Concertable.Contract.Infrastructure.csproj` gained ProjectReference to
  `Concertable.Seeding` (for `SeedIfEmptyAsync` extension + `IDevSeeder`/`ITestSeeder` via
  Concertable.Application transitive).

### Steps still pending

### Step 9 final shape (executed across Phase A → B-redux → B-bonus → C)

**Cross-module write surface:** `IContractModule` is the unified facade — `GetByIdAsync` +
`CreateAsync` + `UpdateAsync` + `DeleteAsync` (decision α). `IContractService` (Contract.Application,
internal) is the workhorse; `ContractModule` (Contract.Infrastructure, internal sealed) is a
thin pass-through.

**Strategy interfaces DO NOT take `IContract` param** — reverted. Earlier lock had every
strategy method taking `IContract`, but ~half the impls (`SettleAsync` everywhere, both
`ITicketPaymentStrategy.PayAsync`, FlatFee/VenueHire `FinishAsync`, DoorSplit/Versus
`InitiateAsync`) ignored it. Leaky-param noise. Reverted to clean signatures.

**`IContractLookup` (Concert.Application, internal, request-scoped memoizing helper) is the
dedup point.** Three methods: `GetByApplicationIdAsync` / `GetByBookingIdAsync` /
`GetByConcertIdAsync` → `Task<IContract>`. Casted (not generic — generic would force
dispatcher to write awkward `<IContract>`). Impl in `Concert.Infrastructure/Services/ContractLookup.cs`
injects all 3 anchor repos + `IOpportunityRepository` (for opportunityId → contractId hop)
+ `IContractModule` (for the actual fetch). Internal `Dictionary<int, IContract>` cache keyed
by `contractId`. Both dispatcher AND strategy inject `IContractLookup` — second call hits memory.
**One contract DB fetch per request.**

**Dispatchers shrank to 2 deps:** `IContractLookup` + `IXStrategyFactory`. Down from 4. No
inline anchor → opportunity → contract chain in dispatcher code anymore. Renamed: `IAcceptDispatcher`
→ `IAcceptanceDispatcher`, `IFinishedDispatcher` → `ICompletionDispatcher` (`FinishedAsync` →
`FinishAsync`). `ISettlementDispatcher` kept name. Legacy `TicketPaymentDispatcher` (in
`Concertable.Infrastructure`) follows the same pattern with its own `ITicketPaymentStrategyFactory`
(in `Concertable.Infrastructure/Factories/`, will move to Payment when extracted).

**4 workflow strategy impls** (`FlatFee/DoorSplit/Versus/VenueHire ConcertWorkflow`):
- Drop `IContractRepository` injection entirely
- FlatFee/VenueHire `InitiateAsync` casts via `(FlatFeeContract)await contractLookup.GetByApplicationIdAsync(applicationId)` (and similar for VenueHire). Methods that don't need terms don't reference contract at all.
- DoorSplit/Versus `FinishAsync` similarly cast via `(DoorSplitContract)await contractLookup.GetByConcertIdAsync(concertId)`.
- 2 ticket payment strategies (`Artist`/`Venue TicketPaymentService`) keep their existing shape — they don't reference contract terms.

**`OpportunityService.Create/Update` use raw `using TransactionScope`** for cross-DbContext
atomicity (Concert + Contract). One inline usage, no abstraction — discussed wrapping in
`IAtomicScope` and rejected as ceremony with no payoff. MSDTC promotion is a known production-
hardening risk; eventual replacement is the **Outbox pattern** (own scope: contract Id strategy
change, worker infra, idempotency — deferred).

**Schema-level redesign (Direction B + B-split, locked):**
- `Opportunities.ContractId → Contracts.Id` is the only FK direction.
- `Contracts.OpportunityId` column DROPPED. `ContractEntity.OpportunityId` field DROPPED.
- Subtype factories drop the `int opportunityId` first param.
- Dedicated `ContractDbContext` (Step 8) stays. Default schema `"contract"`.
- `OpportunityDeletedDomainEvent` flow DEFERRED (`OpportunityEntity.Delete()` doesn't exist
  yet; pattern locked for when delete is needed).

**`OpportunityDto.Contract` DROPPED + `OpportunityDto.ContractId` ADDED.** Frontend hits
`GET /api/contract/{id}`. `OpportunityApplicationDto.ContractType` DROPPED.

**Tests landed:**
- 5 orphan mapper tests + legacy `ContractServiceTests` DELETED (orphan code from Phase A).
- 3 dispatcher tests + `TicketPaymentDispatcherTests` rewritten to mock `IContractLookup` + factory.
- 4 workflow `ApplicationServiceCompleteTests` rewritten — drop `IContractRepository`/`IContractModule`
  mocks, add `IContractLookup` mock, set up `GetByConcertIdAsync` / `GetByApplicationIdAsync`.
- `ConcertFinishedFunctionTests` updated to mock `ICompletionDispatcher.FinishAsync`. AAA
  comments preserved (project convention).

### Memory pointers

- `MEMORY.md` — index.
- `project_contract_module_facade.md` — Step 9 close-out details + historical context.
- `feedback_request_scoped_lookup.md` — `IContractLookup` pattern (reusable for future module pairs).
- `feedback_aaa_test_comments.md` — preserve AAA markers when rewriting tests.
- `feedback_module_boundaries.md` — CLAUDE.md cross-module rules.
- `feedback_no_ef_in_facade.md` — `IXModule` impls don't inline EF.
- `feedback_module_facade_surface.md` — Module.Api pattern.
- `project_concert_migration_reset.md` — Step 12 scaffold order convention.

### Build + test status: 0 errors, integration suite 122/122 green.

---

## ▶️ Historical RESUME (mid-Step-9, pre-close-out — kept for context)

**You are in the middle of Step 9.** Steps 0–8 are done. Step 9 has been re-architected mid-flight.
Read this whole RESUME block before touching code.

### The locked design (Direction B + B-split)

**Single FK, single direction, Contract is satellite.**

- `Opportunities.ContractId → Contracts.Id` is the **only** FK between them.
- `Contracts.OpportunityId` column **does not exist** in the new design (it does in code today
  — see "Phase B-redux" below for the undo work).
- `ContractEntity` has **zero outward references**. It's a pure terms-of-deal record. Nothing
  in `Contract.Domain` knows about Opportunity, Concert, or anything else.
- 1:1 enforced by `HasIndex(o => o.ContractId).IsUnique()` on `OpportunityEntityConfiguration`
  (Concert side).
- **Create flow** (no chicken-and-egg): save Contract first → get `contractId` → save Opportunity
  with `ContractId = contractId`. Insert order is unambiguous because the FK is one-way.
- **Atomicity** via `TransactionScope` at `OpportunityService` boundary. Two physical DbContexts
  (ConcertDbContext + ContractDbContext from Step 8) enroll automatically.
- **Delete cleanup via event bus.** `OpportunityDeletedDomainEvent` → integration event →
  Contract module subscriber calls `IContractService.DeleteAsync(contractId)`. Eventual
  consistency. Orphan rows are harmless audit data if cleanup ever races.
- **Read flow** via `IContractModule.GetByIdAsync(int contractId)` (renamed from
  `GetByOpportunityAsync`). Dispatchers do **two PK hops**: `applicationRepo →
  GetOpportunityIdAsync(appId)` → `opportunityRepo.GetContractIdAsync(oppId)` →
  `contractModule.GetByIdAsync(contractId)`. Concert owns the relationship traversal because
  Concert owns the relationships.
- **`OpportunityDto.Contract` field DROPPED.** Frontend hits `GET /api/contract/{id}` keyed on
  `opportunity.ContractId`. Contract.Module no longer needs the `/opportunity/{id}` route either.

**Why this shape (1-line):** Contract.Module is a pure satellite — it knows nothing, just
serves terms when asked by Id. Most module-pure shape in the whole refactor.

### What's superseded from the original plan locks

- **§2.2** PK split — partially. PK split itself stays (`ContractEntity.Id` is its own
  auto-generated identity). But the explicit `OpportunityId` column on Contract is **gone**.
- **§3.4** `IContractModule.GetByOpportunityAsync(int)` — replaced by `GetByIdAsync(int)`.
- **§3.6** `IContractRepository.GetByOpportunityIdAsync` — deleted; use inherited
  `IIdRepository<ContractEntity>.GetByIdAsync(int)`.
- Step 9 dispatcher pattern — was one PK hop into Contract; now two PK hops (anchor → opp.id →
  contract.id), both inside Concert.

### What's still locked from earlier (DO NOT undo)

- §3.1 per-consumer factories (Concert + Payment own their own strategy resolution; no generic
  `IContractStrategyFactory<T>`).
- §3.3 dedicated `ContractDbContext` (Step 8 stays — confirmed B-split, no rollback).
- §3.7 five projects with `.Api`.
- TPT inheritance over the four contract subtypes (`Contracts` + `FlatFeeContracts` +
  `DoorSplitContracts` + `VenueHireContracts` + `VersusContracts`).
- Strategy interfaces accept `IContract` per method (cast subtype at top of method body).
- Dispatchers stay split + renamed noun-verb (`AcceptanceDispatcher` /
  `SettlementDispatcher` / `CompletionDispatcher`).

### Current build state (after Step 9 Phase A)

Solution build: **2 reported errors + ~6 cascading behind them.**

- 2 in `Concertable.Infrastructure/Services/Payment/TicketPaymentDispatcher.cs` —
  `IContractStrategyResolver<>` reference (deleted in Step 4; consumer left for Step 9).
- ~6 cascading in `Concert.Infrastructure/Services/{Accept,Settlement,Complete}/` dispatchers
  (same `IContractStrategyResolver<>` ref; gated by `Concertable.Infrastructure` failing first
  so dotnet doesn't surface them yet).

All other build errors from earlier (21 total) were cleared in Phase A — see "Phase A done"
section below for the mechanical cleanup detail.

### Phase A — DONE (mechanical compile unblock; do not re-do)

- Deleted from `Concert.Application`: `Mappers/{ContractMapper, FlatFeeContractMapper,
  DoorSplitContractMapper, VersusContractMapper, VenueHireContractMapper}.cs` +
  `Interfaces/IContractMapper.cs`.
- Removed `services.AddSingleton<IContractMapper, ContractMapper>()` from
  `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`.
- `Concertable.Data.Application.csproj`: gained ProjectRef → `Contract.Domain`;
  `IReadDbContext.cs`: gained `using Concertable.Contract.Domain;`.
  `Concertable.Data.Infrastructure/Data/ReadDbContext.cs`: same using.
- `Concertable.Seeding/Factories/OpportunityFactory.cs`: param `ContractEntity contract` →
  `int contractId`; reflection set switched from `nameof(OpportunityEntity.Contract)` →
  `nameof(OpportunityEntity.ContractId)`. (Note: this whole factory disappears under the new
  design — `OpportunityEntity.ContractId` stays, but seeders move to ContractDevSeeder/TestSeeder
  per Step 10. Phase A keeps it as an interim crutch.)
- `ConcertDevSeeder` (60 sites) + `ConcertTestSeeder` (10 sites): every
  `XContractEntity.Create(...)` arg replaced with `contractId: <sequential int>` placeholder
  via awk regex sub. **Runtime correctness is broken intentionally** until Step 10 reorders
  contract creation.
- `Concertable.Core.UnitTests`: gained ProjectRefs → `Contract.Abstractions` + `Contract.Domain`;
  `GlobalUsings.cs` adds `Contract.Domain`. `DoorSplit/VersusContractEntityTests.cs` updated
  to pass `opportunityId: 1` first arg (will need to drop that arg again under the redesign;
  see Phase B-redux item 1).
- `Web.IntegrationTests` `*ContractDto` → `*Contract` renames in
  `Controllers/Opportunity/{OpportunityRequestBuilders, OpportunityApiTests}.cs` (sed).
- `Concert.Application/Mappers/OpportunityMapper.cs`: ctor params dropped (was
  `IContractMapper`); `Contract = null!` in `ToDto` with TODO. (Under redesign: drop the
  field entirely from `OpportunityDto` — see Phase B-redux item 10.)
- `Concert.Application/Mappers/OpportunityApplicationMapper.cs`: replaced
  `application.Opportunity.Contract.ContractType` with `default` placeholder + TODO.
- `Concert.Infrastructure/Services/OpportunityService.cs`:
  `Create/CreateMultiple/UpdateAsync` stubbed `NotImplementedException` with Phase B TODO;
  `IContractMapper` field removed from ctor.

### Phase B-redux — UNDO + RESHAPE (execute next)

The undo work driven by the redesign — flips the FK direction, drops `OpportunityId` from
Contract, restructures the facade.

1. **`Contract.Domain/Entities/ContractEntity.cs`** — drop the `OpportunityId` property
   entirely. Subtype factory methods drop their first param:
   `FlatFeeContractEntity.Create(int opportunityId, decimal fee, PaymentMethod)` →
   `FlatFeeContractEntity.Create(decimal fee, PaymentMethod)`. Same for `DoorSplitContractEntity`,
   `VersusContractEntity`, `VenueHireContractEntity`. Update `Update` methods correspondingly
   (they don't currently take opportunityId, so likely no change there).
2. **`Contract.Infrastructure/Data/Configurations/ContractEntityConfiguration.cs`** — drop
   `Property(c => c.OpportunityId).IsRequired()` + `HasIndex(c => c.OpportunityId).IsUnique()`.
   Keep the `ToTable("Contracts", Schema.Name)` + `UseTptMappingStrategy()` + the four subtype
   `ToTable` configs.
3. **`Concert.Infrastructure/Data/Configurations/OpportunityEntityConfiguration.cs`** — keep
   `Property(o => o.ContractId).IsRequired()` + `HasIndex(o => o.ContractId).IsUnique()` (the
   only direction now). Verify already in place from Step 5.
4. **`Contract.Application/Interfaces/IContractRepository.cs`** — drop
   `GetByOpportunityIdAsync(int, CancellationToken)`. Inherits `IIdRepository<ContractEntity>`
   which already provides `GetByIdAsync(int)`. Add `AddAsync(ContractEntity)` /
   `UpdateAsync(ContractEntity)` / `DeleteAsync(int contractId)` if not inherited (check
   `IBaseRepository`/`IIdRepository` definitions in `Concertable.Shared.Domain/IIdRepository.cs`).
5. **`Contract.Infrastructure/Repositories/ContractRepository.cs`** — drop the
   `GetByOpportunityIdAsync` impl. Body shrinks to ctor + inherited methods. Concrete file
   may end up with no body methods at all — that's fine.
6. **`Contract.Abstractions/IContractModule.cs`** — rename method:
   `Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default)`
   → `Task<IContract?> GetByIdAsync(int contractId, CancellationToken ct = default)`.
7. **`Contract.Infrastructure/ContractModule.cs`** — rename + impl:
   `await contractRepository.GetByIdAsync(contractId)` → map → return.
8. **`Contract.Application/Interfaces/IContractService.cs`** — extend:
   ```csharp
   Task<IContract> GetByIdAsync(int contractId);          // renamed from GetByOpportunityIdAsync
   Task<int> CreateAsync(IContract contract);             // returns new contractId
   Task UpdateAsync(int contractId, IContract contract);
   Task DeleteAsync(int contractId);
   ```
9. **`Contract.Application/Services/ContractService.cs`** — implement the new methods.
   Calls `IContractRepository.AddAsync/UpdateAsync/DeleteAsync` + maps via `IContractMapper`.
   Need a new `IContractMapper.ToEntity(IContract)` companion to the existing `ToContract`.
10. **`Contract.Application/Interfaces/IContractMapper.cs`** + `Mappers/ContractMapper.cs` —
    add `ContractEntity ToEntity(IContract dto)` method. Switch on `dto.ContractType` →
    `XContractEntity.Create(...)` (subtype factories now drop the opportunityId param per
    item 1 above).
11. **`Contract.Api/Controllers/ContractController.cs`** — route changes:
    `GET api/contract/opportunity/{id}` → `GET api/contract/{id}`. (Optionally add
    POST/PUT/DELETE if the frontend needs direct contract endpoints — currently OpportunityController
    is the entry point so probably not needed.)
12. **`api/Tests/Concertable.Core.UnitTests/Entities/Contracts/{DoorSplitContractEntityTests,
    VersusContractEntityTests}.cs`** — drop the `opportunityId: 1` arg I added in Phase A
    (subtypes' factories no longer take it).
13. **Concert side: `OpportunityEntity.ContractId` field stays.** No change to Concert.Domain.
    The existing `int ContractId` property is the right shape.
14. **`Concert.Application/Interfaces/IOpportunityRepository.cs`** + impl — add
    `Task<int> GetContractIdAsync(int opportunityId)` projection method. Single-column
    indexed lookup. Used by Concert dispatchers (Phase C).

### Phase B-bonus — Create/Update/Delete flow rewire

15. **`Concert.Infrastructure/Services/OpportunityService.cs`** — replace the Phase A
    `NotImplementedException` stubs with real impls using `TransactionScope`:
    ```csharp
    public async Task<OpportunityDto> CreateAsync(OpportunityRequest request)
    {
        // stripe validation + venue lookup unchanged
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var contractId = await contractService.CreateAsync(request.Contract);
        var opp = OpportunityEntity.Create(venueId, period, contractId, request.GenreIds);
        await opportunityRepository.AddAsync(opp);
        await opportunityRepository.SaveChangesAsync();
        scope.Complete();
        return mapper.ToDto(opp);
    }
    ```
    Same shape for `CreateMultipleAsync` (single scope, loop) and `UpdateAsync`
    (`contractService.UpdateAsync(opp.ContractId, request.Contract)` instead of `CreateAsync`).
    Inject `IContractService` (cross-module facade injection — confirm it's already public; it
    is, since it lives on `Contract.Application` but the *interface* at the public boundary is
    `IContractService` in Application... actually `IContractService` is `internal` —
    PROBLEM, see decision below).
16. **DECISION SURFACE (resolve when executing):** `IContractService` is currently `internal` to
    `Contract.Application`. Concert.Infrastructure injecting it directly = cross-module reference
    to another module's Application. **Per CLAUDE.md rule 1, only Contracts/Abstractions is a
    foreign-callable boundary.** Two options:
    - **(α)** Promote `CreateAsync` / `UpdateAsync` / `DeleteAsync` / `GetByIdAsync` onto
      `IContractModule` (in `Contract.Abstractions`, public). The facade gains
      write-side methods. Slight expansion of the cross-module surface but consistent with
      "the facade is the cross-module wire shape."
    - **(β)** Keep `IContractService` internal; add a thin `ContractWriteFacade` to
      `Contract.Abstractions` that delegates. Two interfaces, narrower surface per consumer.
    - **Recommendation: (α).** `IContractModule` becomes the unified public surface:
      `GetByIdAsync` + `CreateAsync` + `UpdateAsync` + `DeleteAsync`. `IContractService`
      becomes the controller's internal facade.
17. **Domain event for delete cleanup** —
    `Concert.Domain/Events/OpportunityDeletedDomainEvent.cs` (NEW) raised by
    `OpportunityEntity.Delete()` (or wherever opp deletion happens; check current code) carrying
    `int ContractId`. Handler in `Concert.Infrastructure/Events/`:
    `OpportunityDeletedDomainEventHandler` translates to integration event
    `OpportunityDeletedEvent` (carrying `int ContractId`) on `IIntegrationEventBus`.
    Subscriber in `Contract.Infrastructure/Handlers/`: `OpportunityDeletedEventHandler` calls
    `IContractService.DeleteAsync(@event.ContractId)`.
    Per `feedback_domain_events_for_integration.md` pattern — `IEventRaiser` + entity raises
    domain event, handler → integration event, foreign module subscribes.
18. **`OpportunityDto`** in `Concert.Application/DTOs/OpportunityDtos.cs` — drop
    `required IContract Contract` field. Update `OpportunityMapper.ToDto` to remove `Contract = null!`.
19. **`OpportunityApplicationDto`** in `Concert.Application/DTOs/OpportunityApplicationDtos.cs` —
    review: it has a `ContractType` field (Phase A set to `default`). Either drop the field
    or fetch via `IContractModule.GetByIdAsync` in the mapper (mapper goes async + Scoped).
    Lean: drop. Frontend can fetch contract by id if it needs the type.
20. **`OpportunityRequest`** in `Concert.Application/Requests/OpportunityRequests.cs` — KEEP
    `required IContract Contract` field. It's the input shape; service splits it on the way down.
21. **`Web.IntegrationTests`** — tests asserting `OpportunityDto.Contract` need rework
    (probably fetch via contract endpoint or removed). Tests that POST `OpportunityRequest`
    with embedded contract continue working.

### Phase C — Step 9 plan §dispatcher rewrites (final stretch)

22. **Concert repos: 3 projection methods (in addition to the `OpportunityRepository.GetContractIdAsync`
    from item 14):**
    - `IOpportunityApplicationRepository.GetOpportunityIdAsync(int applicationId)` — direct
      column read (`OpportunityApplicationEntity.OpportunityId` already exists).
    - `IConcertBookingRepository.GetOpportunityIdAsync(int bookingId)` — projects through
      `Booking.Application.OpportunityId` nav.
    - `IConcertRepository.GetOpportunityIdAsync(int concertId)` — projects through
      `Concert.Booking.Application.OpportunityId` nav chain.
    Each is a 1-column SQL projection.
    *Optionally:* fold the `→ contractId` step into one combined call per repo
    (`GetContractIdByApplicationIdAsync` etc.) — fewer round trips. Either shape works.
23. **`Concert.Application/Interfaces/IConcertWorkflowStrategy.cs`** — interface change:
    ```csharp
    internal interface IConcertWorkflowStrategy : IContractStrategy
    {
        Task<IAcceptOutcome> InitiateAsync(int applicationId, IContract contract, string? paymentMethodId = null);
        Task SettleAsync(int bookingId, IContract contract);
        Task<IFinishOutcome> FinishAsync(int concertId, IContract contract);  // renamed from FinishedAsync
    }
    ```
24. **4 strategy impls** under `Concert.Infrastructure/Services/Application/`:
    `FlatFeeConcertWorkflow`, `DoorSplitConcertWorkflow`, `VersusConcertWorkflow`,
    `VenueHireConcertWorkflow`. Each:
    - Drop `IContractRepository` injection entirely.
    - Drop `contractRepository.GetByXIdAsync<TSubtype>(...)` lookups (those methods are gone
      already from Step 4/Step 6).
    - Add `IContract contract` param to each method.
    - Cast to expected subtype at top of method body:
      `var terms = (FlatFeeContract)contract;` then use `terms.Fee`, `terms.PaymentMethod`, etc.
    - Existing `applicationRepository.GetArtistAndVenueByIdAsync` etc. stay — those are
      Concert-internal lookups, not Contract concerns.
25. **NEW: `Concert.Application/Interfaces/IConcertWorkflowStrategyFactory.cs`** —
    ```csharp
    internal interface IConcertWorkflowStrategyFactory
    {
        IConcertWorkflowStrategy Create(ContractType type);
    }
    ```
26. **NEW: `Concert.Infrastructure/Services/Application/ConcertWorkflowStrategyFactory.cs`** —
    ```csharp
    internal sealed class ConcertWorkflowStrategyFactory(IServiceProvider sp) : IConcertWorkflowStrategyFactory
    {
        public IConcertWorkflowStrategy Create(ContractType type)
            => sp.GetRequiredKeyedService<IConcertWorkflowStrategy>(type);
    }
    ```
27. **DI registration** in `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
    `services.AddScoped<IConcertWorkflowStrategyFactory, ConcertWorkflowStrategyFactory>();`
    The keyed strategy registrations (lines 71–74) stay.
28. **Rename and rewrite 3 dispatchers** under `Concert.Infrastructure/Services/`:
    - `Accept/AcceptDispatcher.cs` → `Accept/AcceptanceDispatcher.cs` (class + interface
      renames; method `AcceptAsync` keeps name).
    - `Settlement/SettlementDispatcher.cs` keeps name; method `SettleAsync` keeps name.
    - `Complete/FinishedDispatcher.cs` → `Complete/CompletionDispatcher.cs` (class + interface
      renames; method `FinishedAsync` → `FinishAsync`).
    All three rewrite to the same shape:
    ```csharp
    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var opportunityId = await applicationRepo.GetOpportunityIdAsync(applicationId)
            ?? throw new NotFoundException($"Application {applicationId} not found");
        var contractId = await opportunityRepo.GetContractIdAsync(opportunityId);
        var contract = await contractModule.GetByIdAsync(contractId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
        return await strategyFactory.Create(contract.ContractType)
            .InitiateAsync(applicationId, contract, paymentMethodId);
    }
    ```
    Same shape for SettleAsync (bookingId anchor) and FinishAsync (concertId anchor).
29. **Update interface files** in `Concert.Application/Interfaces/`:
    `IAcceptDispatcher.cs` → `IAcceptanceDispatcher.cs` (rename file + interface);
    `IFinishedDispatcher.cs` → `ICompletionDispatcher.cs` (rename file + interface; method
    `FinishedAsync` → `FinishAsync`); `ISettlementDispatcher.cs` unchanged.
30. **DI rename** in `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
    `services.AddScoped<IAcceptDispatcher, AcceptDispatcher>()` →
    `services.AddScoped<IAcceptanceDispatcher, AcceptanceDispatcher>()`. Same for
    `IFinishedDispatcher` → `ICompletionDispatcher`.
31. **Consumer rewrites** — anywhere `IAcceptDispatcher` / `IFinishedDispatcher` / `FinishedAsync`
    is referenced. Likely `Concertable.Web/E2EEndpointExtensions.cs` (E2E), `Concert.Api`
    controllers, `Concertable.Web/Controllers/DevController.cs` (TEMPORARY per IVT comment).
    grep for `AcceptDispatcher`, `FinishedDispatcher`, `FinishedAsync` and update.
32. **Legacy `TicketPaymentDispatcher`** at `Concertable.Infrastructure/Services/Payment/TicketPaymentDispatcher.cs` —
    same pattern with `ITicketPaymentStrategyFactory` (NEW, in `Concertable.Application/Interfaces/Payment/`)
    + `TicketPaymentStrategyFactory` impl (NEW, in `Concertable.Infrastructure/Factories/`).
    Inject `IConcertRepository` for the concertId → opportunityId hop, then
    `IOpportunityRepository.GetContractIdAsync` for the second hop, then `IContractModule.GetByIdAsync`,
    then `factory.Create(contract.ContractType).PayAsync(concertId, quantity, paymentMethodId, price, contract)`.
    Note: `ITicketPaymentStrategy.PayAsync` interface needs the `IContract contract` param added too.
33. **Stripe validation** at `Concertable.Infrastructure/Validators/StripeValidationFactory.cs` —
    review; likely already keyed-DI shape (`stripeValidationFactory.Create(contract.ContractType).ValidateAsync()`)
    so no rewrite needed — just confirm it doesn't reference `IContractStrategyResolver<>`.
34. **Tests** — rename `AcceptDispatcherTests` → `AcceptanceDispatcherTests`,
    `FinishedDispatcherTests` → `CompletionDispatcherTests` under
    `Concertable.Infrastructure.UnitTests/Services/`. Update mocks: replace
    `IContractStrategyResolver<IConcertWorkflowStrategy>` mock with mocks of
    `IOpportunityApplicationRepository.GetOpportunityIdAsync` +
    `IOpportunityRepository.GetContractIdAsync` + `IContractModule.GetByIdAsync` +
    `IConcertWorkflowStrategyFactory.Create`. Same shape for the other dispatcher tests.

### Step 10+ (after Step 9 lands)

- **Step 10** — `ContractDevSeeder` + `ContractTestSeeder` in `Contract.Infrastructure/Data/Seeders/`.
  Concert seeders create Opportunities; Contract seeders create Contracts; orchestrate via
  Order. Decide whether contract seeding goes BEFORE opportunity seeding (preferred — saves
  contract first, references it from opportunity) or as a separate stage that updates
  `Opportunities.ContractId` after both are seeded. Replace the `contractId: <int>` placeholders
  in ConcertDevSeeder/TestSeeder with real wiring.
- **Step 11** — Remove TEMPORARY IVT grants. Audit `Concert.Application/AssemblyInfo.cs`
  + `Concert.Infrastructure/AssemblyInfo.cs` for `IVT("Concertable.Infrastructure")` /
  `IVT("Concertable.Application")` / `IVT("Concertable.Web")` / `IVT("Concertable.Workers")`
  TEMPORARY entries. Drop those that are no longer needed after Step 9 + Step 10 land.
- **Step 12** — Migration re-scaffold. Delete every module's `Migrations/` folder. Rescaffold
  `InitialCreate` per context: Shared → Identity → Artist → Venue → Concert → **Contract** →
  AppDb. Verify the new schema (one FK direction `Opportunities.ContractId → Contracts.Id`).
  TPT layout for Contracts unchanged.
- **Step 13** — Full test suite pass. IVT for any newly-mocked internals
  (`DynamicProxyGenAssembly2` long-form per `feedback_castle_proxy_ivt.md`).

### Memory pointers (read these for context)

- `MEMORY.md` — index.
- `project_contract_module_facade.md` — Contract extraction memory; Direction-B redesign
  recorded.
- `project_modular_monolith.md` — overall MM stage progress; Concert is fully extracted, Contract
  is the active extraction.
- `feedback_module_boundaries.md` — CLAUDE.md cross-module rules.
- `feedback_no_ef_in_facade.md` — `IXModule` impls don't inline EF (delegate to repo).
- `feedback_module_services_save_own_context.md` — module services use their own
  `xRepository.SaveChangesAsync()`, not `IUnitOfWork`.
- `feedback_domain_events_for_integration.md` — domain event → integration event pattern;
  use this for the OpportunityDeleted → Contract cleanup flow.
- `feedback_module_facade_surface.md` — Module.Api pattern; controllers internal +
  `InternalControllerFeatureProvider`.
- `project_concert_migration_reset.md` — Step 12 scaffold order convention.

### Build expectation after Phase B-redux + Phase B-bonus + Phase C all land

Solution build should be **0 errors**. Tests likely have failures pending Step 10 (seeders)
and Step 13 (mock IVT) but compilation should be clean.

---

> **PROGRESS (2026-04-25):**
> - ✅ Step 0 — Discovery sweep complete (Appendix A filled).
> - ✅ Step 1 — 5 Contract projects scaffolded under `api/Modules/Contract/`. IVT from
>   `Concert.Infrastructure → Contract.Infrastructure` wired (ride-along ConcertDbContext access).
>   Build green.
> - ✅ Step 2 — `ContractType` + `PaymentMethod` moved from `Concert.Domain/Enums/` to
>   `Contract.Abstractions/`. Namespace `Concertable.Contract.Abstractions`. Concert.Domain
>   gained Contract.Abstractions project ref + global using; Concert.Application/Infrastructure/Api
>   + legacy Application/Infrastructure/Web/Workers/Core + 3 test projects gained
>   `global using Concertable.Contract.Abstractions;`. Full solution build green (0 errors).
> - ✅ Step 3 — `ContractEntity` hierarchy (5 files) moved to `Contract.Domain/Entities/` with
>   explicit `int OpportunityId` + factories taking `opportunityId` (PK split). `IContract` + 4
>   polymorphic DTOs (`FlatFeeContract`, `DoorSplitContract`, `VersusContract`, `VenueHireContract`
>   — `Dto` suffix dropped) moved to `Contract.Abstractions/`, made `public`. `OpportunityEntity.Contract`
>   nav dropped → `int ContractId`. `.Include(o => o.Contract)` stripped from 2 repos. Both
>   Domain projects build green, no mutual refs.
> - ✅ Step 4 — `IContractRepository` shrunk to single
>   `GetByOpportunityIdAsync(int, CancellationToken)` method, relocated to
>   `Contract.Application/Interfaces/`. `IContractStrategy` marker promoted to
>   `Contract.Abstractions/` (public empty). Deleted: `IContractStrategyFactory<T>`,
>   `IContractStrategyResolver<T>`, their impls under `Concertable.Infrastructure/Factories/`,
>   their 2 test files, plus their DI registrations from
>   `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`.
> - ✅ Step 5 — `ContractEntityConfiguration.cs` (single file, base + 4 subtypes TPT) moved to
>   `Contract.Application/Data/Configurations/`. FK rewrite: dropped nav-based HasOne/WithOne
>   chain; now `.Property(c => c.OpportunityId).IsRequired()` +
>   `.HasIndex(c => c.OpportunityId).IsUnique()`. Mirror `ContractId` config added on
>   `OpportunityEntityConfiguration`. `Contract.Application.csproj` gained
>   `Microsoft.EntityFrameworkCore.Relational`. `Concert.Infrastructure.csproj` gained ref to
>   `Contract.Application`. `ConcertConfigurationProvider` imports the new namespace.
> - ✅ Step 6 — `ContractRepository` impl moved to `Contract.Infrastructure/Repositories/`,
>   body shrunk to single `context.Contracts.FirstOrDefaultAsync(c => c.OpportunityId == ...)`.
>   `AddContractModule()` extension created at `Contract.Infrastructure/Extensions/` registering
>   the repo. DI line removed from Concert.
> - ✅ Step 6.5 — `IContractService` + `ContractService` (primary-ctor `IContractModule`
>   consumer, throws on null) moved to `Contract.Application/Interfaces/` + `/Services/`.
>   `ContractController` + copy of `InternalControllerFeatureProvider` moved to
>   `Contract.Api/Controllers/` + `/Extensions/`. `AddContractApi()` extension created wiring
>   controllers + `IContractService`. `IContractModule` interface stub in `Contract.Abstractions/`
>   (impl lands Step 7). Web `Program.cs` + Workers `ServiceCollectionExtensions.cs` call
>   `AddContractApi()` / `AddContractModule()`. Project refs added: Web→Contract.Api,
>   Workers→Contract.Infrastructure.
> - ✅ Step 7 — `IContractMapper` (internal) + `ContractMapper` impl (single `ToContract`
>   method, switch on concrete `ContractEntity` subtype → matching `IContract` DTO) added to
>   `Contract.Application/Interfaces/` + `/Mappers/`. `ContractModule` impl added to
>   `Contract.Infrastructure/` (primary-ctor `IContractRepository` + `IContractMapper`,
>   delegates `GetByOpportunityAsync` to repo + maps result; no inline EF per
>   `feedback_no_ef_in_facade.md`). All three registered in `AddContractModule()`.
>   Contract.Infrastructure builds green; pre-existing 18 cascade errors remain in
>   Concert.Application mappers / `Data.Application/IReadDbContext.cs` / Seeding
>   `OpportunityFactory.cs` (Step 9 work).
> - ✅ Step 8 — **§3.3 ride-along REVERSED** (2026-04-25 at user request):
>   `ContractDbContext` created at `Contract.Infrastructure/Data/ContractDbContext.cs`
>   inheriting `DbContextBase`, default schema `"contract"` (new `Contract.Infrastructure/Schema.cs`),
>   ctor-injects `ContractConfigurationProvider`. New `ContractConfigurationProvider`
>   (internal, `IEntityTypeConfigurationProvider`) at `Contract.Infrastructure/Data/`
>   applying `ContractEntityConfiguration` + 4 subtype configs. `ContractEntityConfiguration`
>   moved from `Contract.Application/Data/Configurations/` (deleted) to
>   `Contract.Infrastructure/Data/Configurations/` with `Schema.Name` on every `ToTable`.
>   `ContractRepository` switched from `ConcertDbContext` to `ContractDbContext`.
>   `AddContractModule()` now takes `IConfiguration`, registers `ContractDbContext` with
>   AuditInterceptor + DomainEventDispatchInterceptor + connection string +
>   `ContractConfigurationProvider` (singleton + as `IEntityTypeConfigurationProvider`).
>   `AddContractApi()` updated to take + forward `IConfiguration`.
>   **Cleaned up**: `Contract.Application.csproj` drops `Microsoft.EntityFrameworkCore` +
>   `…Relational` packages (cycle-avoidance hack retired); `Contract.Infrastructure.csproj`
>   drops `Concert.Infrastructure` ProjectReference (no longer needs ConcertDbContext);
>   `Concert.Infrastructure.csproj` drops `Contract.Application` ProjectReference (no longer
>   applies Contract configs); `Concert.Infrastructure/AssemblyInfo.cs` drops the
>   `Concertable.Contract.Infrastructure` IVT; `ConcertDbContext` drops 5 Contract DbSets;
>   `ConcertConfigurationProvider` drops 5 Contract config applies + namespace using.
>   `ApplicationDbContext` keeps the 5 `Entity<ContractEntity>().ToTable(t => t.ExcludeFromMigrations())`
>   lines — `ContractConfigurationProvider` is registered as `IEntityTypeConfigurationProvider`
>   so AppDb's loop applies it (table name + `"contract"` schema set there;
>   ExcludeFromMigrations layered on top — same pattern as Concert/Venue/Artist tables).
>   ReadDbContext picks up the same provider via DI; existing `IQueryable<ContractEntity>` +
>   subtype properties resolve once the IReadDbContext.cs cascade fix lands in Step 9.
>   Web `Program.cs` and Workers `ServiceCollectionExtensions.cs` updated to forward
>   `IConfiguration` to AddContractApi/AddContractModule. Migration scaffold order at
>   Step 12 becomes: Shared → Identity → Artist → Venue → Concert → **Contract** → AppDb.
> ## 🔁 REDESIGN LOCK 2026-04-25 (Step 9 architectural rethink)
>
> **Aggregate shape redesigned to single-direction FK.** Triggered by Step 9 Phase A surfacing
> the Opportunity↔Contract circular FK (both `Opportunities.ContractId` and `Contracts.OpportunityId`
> NOT NULL unique-indexed). Resolution: **Contract has no outward references at all.**
>
> - **`Opportunities.ContractId → Contracts.Id` is the ONLY FK.** Direction is one-way.
> - **`Contracts.OpportunityId` column DROPPED.** `ContractEntity.OpportunityId` field DROPPED.
>   Subtype factory methods drop the `int opportunityId` first param.
> - **Contract module is fully independent — knows about nothing.** Other modules reference it;
>   it references no one. Pure terms-of-deal record.
> - **1:1 still enforced** via unique index on `Opportunities.ContractId`.
> - **Create flow (no chicken-and-egg):** save Contract FIRST → get `contractId` → save Opportunity
>   with `ContractId = contractId`. Single direction; insert order unambiguous; no nullable
>   placeholder.
> - **Atomicity via TransactionScope** at `OpportunityService` boundary (B-split locked — dedicated
>   `ContractDbContext` from Step 8 stays; Step 8 NOT undone).
> - **Delete flow via event bus.** `OpportunityDeletedDomainEvent` → integration event → Contract
>   module subscriber calls `IContractService.DeleteAsync(contractId)`. Eventual consistency for
>   cleanup; orphan rows are harmless audit data if they ever land.
> - **Read flow via `IContractModule.GetByIdAsync(int contractId)`.** Replaces `GetByOpportunityAsync`.
>   Dispatcher pattern shifts to two-PK-hop: `applicationRepo.GetOpportunityIdAsync(appId)` →
>   `opportunityRepo.GetContractIdAsync(oppId)` → `contractModule.GetByIdAsync(contractId)`.
>   Concert owns the relationship traversal because Concert owns the relationships.
> - **`OpportunityDto.Contract` field DROPPED.** Frontend hits `GET /api/contract/{id}` keyed on
>   `opportunity.ContractId`. Cross-module decoupling per `feedback_restricted_projections.md`.
>
> **Lock-points superseded:**
> - **§2.2 PK split** — partially superseded. `ContractEntity.Id` still its own auto-generated
>   identity (PK split kept for SQL clarity), but explicit `OpportunityId` column on Contract is
>   GONE. `Opportunities.ContractId` is the one and only FK between them.
> - **§2.2 "both navs dropped"** — still holds: `OpportunityEntity.Contract` nav stays dropped,
>   `ContractEntity.Opportunity` nav stays dropped. But `ContractEntity.OpportunityId` field
>   NEWLY dropped under this redesign.
> - **§3.4 `IContractModule` shape** — superseded. Was `GetByOpportunityAsync(int)`; now
>   `GetByIdAsync(int)`. Single PK lookup. The earlier reasoning that "Contract owns the FK
>   so Contract knows the OpportunityId" no longer applies — Contract owns no FK.
> - **§3.6 `IContractRepository.GetByOpportunityIdAsync`** — superseded. Replaced by inherited
>   `IIdRepository.GetByIdAsync(int)`. Method deleted.
>
> **Lock-points unchanged:**
> - §3.1 per-consumer factories (Concert/Payment own their own strategy resolution).
> - §3.3 dedicated `ContractDbContext` (Step 8 stays — confirmed B-split, no rollback).
> - §3.7 five projects with `.Api`.
> - TPT inheritance over the four contract subtypes.
> - Strategy interfaces accept `IContract` per method (cast subtype at top).
> - Dispatchers stay split, renamed noun-verb (Acceptance/Settlement/Completion).
>
> ## Step 9 Phase A status (compile unblock — DONE)
>
>   **Phase A applied (mechanical compile fixes, build 21 → 2 errors + ~6 cascading behind):**
>   - Deleted 5 orphan mappers + `IContractMapper.cs` from Concert.Application/Mappers + Interfaces.
>   - Removed `services.AddSingleton<IContractMapper, ContractMapper>()` from
>     `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`.
>   - `Data.Application.csproj` gains ProjectRef → Contract.Domain;
>     `IReadDbContext.cs` gains `using Concertable.Contract.Domain;`.
>     `Data.Infrastructure/Data/ReadDbContext.cs` gains the same using (transitively brought via
>     Data.Application ref).
>   - `Concertable.Seeding/Factories/OpportunityFactory.cs` rewritten: `int contractId` instead
>     of `ContractEntity contract`; sets `nameof(OpportunityEntity.ContractId)` instead of nav.
>   - `ConcertDevSeeder` + `ConcertTestSeeder`: every `XContractEntity.Create(...)` call replaced
>     with `contractId: <sequential int>` placeholder (60 sites total in DevSeeder, 10 in TestSeeder).
>     Step 10 reorders properly so contracts seed alongside opportunities.
>   - `Core.UnitTests` gains ProjectRefs → Contract.Abstractions + Contract.Domain;
>     `GlobalUsings.cs` adds `Contract.Domain`. `DoorSplit/VersusContractEntityTests` updated to
>     pass `opportunityId: 1` first arg + dropped stale `Concertable.Core.Enums` import.
>   - `Web.IntegrationTests` `*ContractDto` → `*Contract` renames in
>     `OpportunityRequestBuilders.cs` + `OpportunityApiTests.cs` (sed).
>   - `OpportunityMapper.ToDto`: drops `IContractMapper` dep and the `Contract` assignment;
>     sets `Contract = null!` with TODO Step 9-followup. Class no longer takes ctor params.
>   - `OpportunityApplicationMapper.ToDto`: replaces `application.Opportunity.Contract.ContractType`
>     with `default` (ContractType placeholder) + TODO.
>   - `OpportunityService.Create/CreateMultiple/UpdateAsync` stubbed to throw
>     `NotImplementedException` with TODO pointing at Phase B work; `IContractMapper` field
>     removed from ctor injection.
>
>   **Remaining errors (≈6 cascading once TicketPaymentDispatcher compiles):**
>   `IContractStrategyResolver<>` references in `Concertable.Infrastructure/Services/Payment/TicketPaymentDispatcher.cs`
>   (2) + `Concert.Infrastructure/Services/{Accept,Settlement,Complete}/` dispatchers (3 files,
>   ~6 errors) — the legacy resolver was deleted in Step 4 but consumers were left for Step 9.
>   Concert dispatchers don't surface yet because Concertable.Infrastructure compiles first
>   and gates them. Phase C handles all of them.
>
>   **🛑 PHASE B / PHASE C — pending two architectural decisions:**
>
>   **Decision 1 — Opportunity ↔ Contract circular FK (write flow).**
>   Both `Opportunities.ContractId` and `Contracts.OpportunityId` are NOT NULL with unique
>   indexes after Step 3 nav drops; create flow needs a resolution.
>   - (a) **Make `Opportunities.ContractId` nullable.** Save Opportunity → save Contract with
>     `OpportunityId` → optionally backfill ContractId on Opportunity (or never — Contract is
>     reachable via the OpportunityId index without it). Cleanest at runtime; schema change at
>     Step 12 scaffold time. **Recommended.**
>   - (b) **Two-phase save in transaction.** Opp w/ ContractId=0 placeholder → Contract → update
>     Opp.ContractId. Briefly invalid FK; schema unchanged.
>   - (c) **`IContractService.CreateAsync(int opportunityId, IContract dto)` orchestrates (b)
>     internally.** OpportunityService gets a clean single call.
>
>   **Decision 2 — `OpportunityDto.Contract` (read flow).**
>   Currently `required IContract Contract` on the response DTO; structurally orphaned now.
>   - (i) **Drop the field.** Frontend hits `GET /api/contract/opportunity/{id}` separately.
>     Cross-module break per CLAUDE.md spirit + `feedback_restricted_projections.md`. **Recommended.**
>   - (ii) **Keep it, populate via `IContractModule.GetByOpportunityAsync` in mapper.** Mapper
>     becomes async + Scoped (not Singleton).
>   - (iii) **Status quo (TODO stub).** `null!` placeholder; not a final answer.
>
>   **Phase C — the actual plan §Step 9 work** (locked once Decisions 1+2 land):
>   - 3 Concert repos add `GetOpportunityIdAsync(int)` projections (Application, Booking, Concert).
>   - `IConcertWorkflowStrategy` adds `IContract` param to every method;
>     `FinishedAsync` → `FinishAsync`; 4 impls cast subtype at top.
>   - `IConcertWorkflowStrategyFactory` + impl (5-line `GetRequiredKeyedService`).
>   - 3 Concert dispatchers renamed (`Accept` → `Acceptance`, `Finished` → `Completion`,
>     `Settlement` keeps name) + rewritten: anchor-id hop → `contractModule.GetByOpportunityAsync`
>     → `factory.Create(contract.ContractType)` → `strategy.<Phase>Async(anchorId, contract)`.
>   - Legacy `TicketPaymentDispatcher` mirrors the pattern with `ITicketPaymentStrategyFactory`.
>   - Stripe validation factory verified.
>   - DI registrations updated; old `IContractStrategyFactory<>` / `IContractStrategyResolver<>`
>     already deleted — confirm no DI registrations remain.
>   - Tests: rename `AcceptDispatcherTests` → `AcceptanceDispatcherTests`,
>     `FinishedDispatcherTests` → `CompletionDispatcherTests`. Mock the new factory + facade
>     instead of the deleted resolver.
>
> - ⏭ **Awaiting decisions 1+2 to proceed** — recommended (a) + (i).
>
> ## ✅ DECISION RESOLVED 2026-04-25 — Direction B + B-split
>
> Both decisions superseded by the redesign above. Contract has no `OpportunityId`; FK is
> one-way `Opportunities.ContractId → Contracts.Id`; create flow saves Contract first then
> Opportunity (no chicken-and-egg); dedicated `ContractDbContext` stays; TransactionScope
> handles atomicity; delete cleanup via event bus.
>
> **Phase B-redux work (post-decision):**
> 1. Drop `OpportunityId` from `ContractEntity` + 4 subtypes; subtype factories drop the
>    first param (`Create(int opportunityId, ...)` → `Create(...)`).
> 2. Drop `OpportunityId` config from `ContractEntityConfiguration` (in
>    `Contract.Infrastructure/Data/Configurations/`); drop the unique index.
> 3. Drop `IContractRepository.GetByOpportunityIdAsync`; impl simplifies to inherited
>    `IIdRepository<ContractEntity>.GetByIdAsync(int)`. Add `AddAsync` / `UpdateAsync` /
>    `DeleteAsync` if not already inherited.
> 4. Rename `IContractModule.GetByOpportunityAsync(int)` → `GetByIdAsync(int)` in Abstractions;
>    update `ContractModule` impl.
> 5. Rename `IContractService.GetByOpportunityIdAsync(int)` → `GetByIdAsync(int)` in
>    Contract.Application/Interfaces. Add `CreateAsync(IContract)` returning new contract id,
>    `UpdateAsync(int contractId, IContract)`, `DeleteAsync(int contractId)`. Implement in
>    `ContractService` (calls `IContractRepository.AddAsync/UpdateAsync/DeleteAsync` + maps via
>    `IContractMapper`; new `IContractMapper.ToEntity(IContract)` companion to `ToContract`).
> 6. `ContractController` route: `GET api/contract/opportunity/{id}` → `GET api/contract/{id}`.
>    Add `POST` / `PUT` / `DELETE` if external callers need them (otherwise creation/update flows
>    through Concert's OpportunityController only).
> 7. `OpportunityEntity` keeps `ContractId` field. Concert.Domain unchanged from current shape.
> 8. **Phase C work (Step 9 plan §dispatcher rewrites)** — adapted to new shape:
>    - Add `GetContractIdByApplicationIdAsync(int)` / `GetContractIdByBookingIdAsync(int)` /
>      `GetContractIdByConcertIdAsync(int)` to the 3 Concert repos. (Single-step projections —
>      anchor → opportunityId hop combined with ContractId read.) Or split into the two-hop
>      pattern (`GetOpportunityIdAsync` + `OpportunityRepository.GetContractIdAsync`); single
>      combined method is fewer round-trips.
>    - `IConcertWorkflowStrategy` adds `IContract` param to every method;
>      `FinishedAsync` → `FinishAsync`; 4 impls cast subtype at top.
>    - `IConcertWorkflowStrategyFactory` + 5-line `GetRequiredKeyedService` impl.
>    - 3 dispatchers renamed (Accept→Acceptance, Finished→Completion) + rewritten:
>      `repo.GetContractIdByXIdAsync(anchorId)` → `contractModule.GetByIdAsync(contractId)` →
>      `factory.Create(contract.ContractType).<Phase>Async(anchorId, contract)`.
>    - Legacy `TicketPaymentDispatcher` mirrors the pattern (Concert repo hop +
>      `ITicketPaymentStrategyFactory`).
>    - Stripe validation factory verified (likely already keyed-DI shape).
> 9. **Phase B-bonus** (NEW — was implicit in Direction A but explicit here):
>    - `OpportunityService.CreateAsync` rewires:
>      ```
>      using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
>      var contractId = await contractService.CreateAsync(request.Contract);
>      var opp = OpportunityEntity.Create(venueId, period, contractId, request.GenreIds);
>      await opportunityRepository.AddAsync(opp);
>      await opportunityRepository.SaveChangesAsync();
>      scope.Complete();
>      ```
>    - `OpportunityService.UpdateAsync` similar but updates the existing contract via
>      `contractService.UpdateAsync(opp.ContractId, request.Contract)`.
>    - `OpportunityService.CreateMultipleAsync` similar pattern in a loop within one scope.
>    - `OpportunityCreatedDomainEvent` raised by `OpportunityEntity.Create`. NOT load-bearing
>      for the create flow (TransactionScope handles atomicity); used elsewhere if needed.
>    - `OpportunityDeletedDomainEvent` → integration event handler in Contract.Infrastructure
>      calls `IContractService.DeleteAsync(opportunity.ContractId)`. Eventual consistency.
> 10. `OpportunityDto`: drop `Contract` field. Update `OpportunityMapper.ToDto` (already returns
>     `null!` placeholder from Phase A — replace with field deletion).
> 11. `OpportunityRequest`: KEEP `Contract` field (it's the input shape — frontend submits an
>     opportunity with embedded contract terms; service splits it on the way down).
> 12. `OpportunityApplicationMapper`: drop the `application.Opportunity.Contract.ContractType`
>     reference fully (was Phase A `default` placeholder). Either drop the field from the DTO
>     or fetch via `IContractModule` per consumer.
> 13. `Concertable.Web.IntegrationTests`: tests using `OpportunityDto.Contract` need updating
>     to fetch contract via separate request, OR be rewritten to assert via the contract
>     endpoint directly.
>
> **STATUS: DRAFT — significant redesign 2026-04-24 (afternoon). Working document, still iterating.**
>
> This plan was rewritten after a fresh architectural review. The earlier "Shape B" generic-facade
> design (`IContractModule.ResolveFor…Async<T>`) was rejected: it made Contract.Infrastructure the
> dispatch hub for other modules' strategies by reaching into the DI container via `IServiceProvider`.
> Contract hosting an abstraction it does not consume, for the benefit of other modules, is the
> wrong module. The marker survives as a compile-time restriction, but resolution moves out.
>
> Prerequisite to: `PAYMENT_MODULE_REFACTOR.md` (currently PAUSED).
>
> **Locked (2026-04-24):**
> - **§3.1 Strategy resolution — per-consumer factories.** Contract does **not** host strategy
>   resolution. Each module that owns a strategy family writes a tiny internal factory
>   (`IConcertWorkflowStrategyFactory`, later `ITicketPaymentStrategyFactory`,
>   `IStripeValidationStrategyFactory`) that wraps
>   `IServiceProvider.GetRequiredKeyedService<TStrategy>(ContractType)`. The factory is the
>   ONLY place `IServiceProvider` appears in each module — bounded service-locator contained
>   in one 5-line class. No generic methods on `IContractModule`. No `IContractStrategyFactory<T>` /
>   `IContractStrategyResolver<T>` in `Contract.Abstractions`.
> - **§3.2 `PaymentMethod` enum — `Contract.Abstractions`, public.** Term of the deal, co-located
>   with `ContractType`.
> - **§3.4 `IContractModule` shape — Opportunity-anchored, two methods only.**
>   `GetTypeForOpportunityAsync(int)` and `GetByOpportunityAsync(int)` → `IContract?`. Contract
>   only exposes what it owns (FK `ContractEntity.OpportunityId`). No `*ForApplication` /
>   `*ForBooking` / `*ForConcert` methods on the facade — those would mean Contract.Infrastructure
>   SQL joining across Concert-owned tables. Concert does its own `applicationId → opportunityId`
>   / `bookingId → opportunityId` / `concertId → opportunityId` hop against its own repos, then
>   calls the facade. Two indexed PK lookups instead of one three-table join; boundary win worth it.
> - **§3.5 `ContractType` enum — `Contract.Abstractions`, public.** Cross-module primitive.
> - **§2.2 Cross-module Domain navs DROPPED both directions.** `ContractEntity.Opportunity` nav
>   removed (plain `int OpportunityId` only). `OpportunityEntity.Contract` nav removed (plain
>   `int ContractId` only). No `Contract.Domain → Concert.Domain` project ref. No
>   `Concert.Domain → Contract.Domain` project ref. Clean extraction, not rule-3 carryover.
> - **`IContract` interface + FlatFee/DoorSplit/VenueHire/Versus DTOs move to `Contract.Abstractions`,
>   public.** Currently at `Concert.Application/Interfaces/IContract.cs` + 4 DTOs in
>   `Concert.Application/DTOs/`. The interface carries `[JsonDerivedType]` discriminators — it's
>   a polymorphic wire shape, cross-module by nature.
> - **`IContractStrategy` marker — `Contract.Abstractions`, public.** Compile-time restriction:
>   Concert's `IConcertWorkflowStrategy : IContractStrategy`, Payment's
>   `ITicketPaymentStrategy : IContractStrategy`, `IStripeValidationStrategy : IContractStrategy`.
>   Declares "this family is keyed by `ContractType`." Not consumed by `IContractModule` at all.
> - **Dispatchers stay split, renamed noun-verb.** `IAcceptanceDispatcher` (`AcceptAsync`),
>   `ISettlementDispatcher` (`SettleAsync`), `ICompletionDispatcher` (`FinishAsync` — was
>   `FinishedAsync`, imperative mood). Three narrow interfaces for ISP —
>   `SettlementWebhookHandler` injecting `ISettlementDispatcher` cannot accidentally trigger
>   accept or finish flows. Each does one anchor-id hop → `contractModule.GetByOpportunityAsync`
>   → `factory.Create(contract.Type)` → `strategy.<Phase>Async(anchorId, contract)`. ~8 lines
>   each; per-phase duplication not worth abstracting. No `Result<T>` wrap carveout on
>   `CompletionDispatcher` — dispatcher signatures stay consistent; any wrap belongs at the caller.
> - **Single contract fetch per dispatch.** Dispatchers call `GetByOpportunityAsync` once and
>   pass the `IContract` through to the strategy as a method parameter. No second lookup inside
>   the strategy, no `GetTypeForOpportunityAsync` pre-check. One PK read in the Contract context
>   per workflow invocation.
> - **`IContract` gains `ContractType Type { get; }`.** Each subtype DTO implements it as a
>   hard-coded expression-body (`=> ContractType.FlatFee`, etc.). Factory switches on
>   `contract.Type` to resolve the keyed strategy. Polymorphic JSON serialisation is unaffected —
>   the `[JsonDerivedType]` discriminator already exists.
> - **Strategy interface takes `IContract` per method.** `IConcertWorkflowStrategy`:
>   `InitiateAsync(int applicationId, IContract contract, string? paymentMethodId = null)` /
>   `SettleAsync(int bookingId, IContract contract)` /
>   `FinishAsync(int concertId, IContract contract)`. Each strategy casts to its expected subtype
>   at the top of the method body (`var terms = (FlatFeeContract)contract;`) — safe by
>   construction (keyed registration + `contract.Type` switch guarantee the match). Contract
>   appears three times in the interface declaration but is passed exactly once per runtime
>   invocation; the three methods are three independent entry points each declaring the data
>   they need, not redundancy. Accepted over (a) strategy-fetches-internally (two round trips),
>   (b) factory + `ActivatorUtilities` + transient strategies (switch statement, ceremony), and
>   (c) generic `IConcertWorkflowStrategy<TContract>` (pushes the cast into glue code rather
>   than eliminating it).
> - **`IContractModule` has one method.** `GetByOpportunityAsync(int)` → `IContract?`.
>   `GetTypeForOpportunityAsync` dropped — the dispatcher needs the data anyway, so a
>   type-only accessor has no caller.
> - **Settlement stays in Concert.** Stripe webhook is a trigger; the business meaning ("booking
>   has been paid — settle it") is Concert's booking lifecycle. Payment owns money-movement
>   primitives (Stripe API, Transactions, refunds, ticket payment orchestration), not booking
>   state transitions. The webhook *endpoint* (signature validation, event extraction) may live in
>   Payment when Payment extracts — that's a Payment-plan seam question, not this plan's.
>
> **Locked (2026-04-24, afternoon — second pass):**
> - **Module name: `Concertable.Contract.Abstractions` (NOT `.Contracts`).** The "Contracts"
>   layer name in every other module (Identity/Artist/Venue/Concert) collides with this module's
>   own name (`Contract`). Contract gets `.Abstractions` first; the rest rename in a sweep at
>   the end of MM extraction. (Cleanup ticket from §1's earlier note, accelerated.)
> - **TPT, not TPH.** Discovery confirmed `ContractEntityConfiguration` already uses
>   `.UseTptMappingStrategy()` — five tables (`Contracts`, `FlatFeeContracts`,
>   `DoorSplitContracts`, `VenueHireContracts`, `VersusContracts`), no discriminator column,
>   subtype routing via per-subtype FK on `ContractEntity.Id`. Plan keeps TPT (no schema
>   migration of inheritance strategy on top of the extraction).
> - **Contract entities + repo + config are already in Concert module, not legacy.** Discovery
>   showed `ContractRepository` and `ContractEntityConfiguration` live in
>   `Concert.Infrastructure/`, and `ContractType` + `PaymentMethod` enums live in
>   `Concert.Domain/Enums/`. Plan §2.1 / Step 2 / Step 6 path references that pointed to
>   `Concertable.Core/Enums/` or `Concertable.Infrastructure/` are stale; the moves are
>   Concert-module → Contract-module, not legacy → Contract-module.
> - **Contract.Domain split: explicit PK + explicit `OpportunityId` FK.** Discovery surfaced
>   that the existing schema reuses `ContractEntity.Id` as the FK to Opportunity (1:1 PK
>   sharing via `.HasForeignKey<ContractEntity>(c => c.Id)`). Splitting cleans this up:
>   `ContractEntity.Id` becomes its own auto-generated identity; explicit `int OpportunityId`
>   column carries the FK. Standard EF/SQL pattern, removes the PK-as-FK semantic alias.
>   Cost-free since migrations rescaffold in Step 12 anyway. `ContractEntity.Create(...)`
>   takes `int opportunityId` as a parameter; subtype factory methods pass it through.
> - **§3.3 `ContractDbContext` — RIDE-ALONG on `ConcertDbContext`.** ⚠ **REVERSED 2026-04-25
>   at user request.** Step 8 created a dedicated `ContractDbContext` in `Contract.Infrastructure`
>   (default schema `"contract"`); ride-along scaffolding (Concert.Infrastructure → Contract.Application
>   ref, IVT for ConcertDbContext, EF dep on Contract.Application) was retired. Original
>   ride-along reasoning preserved below for context. Contract entities (TPH base
>   + 4 subtypes) are configured on `ConcertDbContext`, not on a dedicated `ContractDbContext`.
>   Contract aggregate is tightly wedded to Opportunity (1:1 via `OpportunityId` FK); a separate
>   context for one TPH hierarchy buys little and adds a migration / Respawner / cross-context
>   FK seam for no boundary win. `Concert.Infrastructure` references `Contract.Domain` to apply
>   `ContractEntityConfiguration` (allowed under CLAUDE.md rule 3 — Domain ref is the accepted
>   escape hatch for entity types). `Contract.Application` / `Contract.Infrastructure` still
>   exist; the repo injects `ConcertDbContext` (visible because Concert grants
>   `InternalsVisibleTo("Concertable.Contract.Infrastructure")`).
> - **§3.7 Module layout — FIVE projects, `.Api` INCLUDED.** `IContractService` (currently
>   `Concert.Application/Interfaces/IContractService.cs`) and `ContractController` (currently
>   `Concert.Api/Controllers/ContractController.cs`) move to `Contract.Application` /
>   `Contract.Api` respectively. The HTTP endpoint exists today and is contract-owned, so the
>   project goes in at extraction.
>
> **Still to walk through (standard-pattern calls, likely quick):**
> - §3.6 `InternalsVisibleTo` — follow Concert/Venue/Identity precedent.
>
> **Future cleanup (non-blocking, note for `MM_NORTH_STAR.md`):** the `Contracts` layer name
> collides with the `Contract` module name. Candidate rename: `Module.Abstractions`. Cross-
> cutting find/replace across every module — not worth doing mid-extraction.

---

## 1. Motivation

During Payment extraction discovery (Step 0, 2026-04-24) the `IContractStrategy` abstraction
surfaced as a shared concern:

- `ContractType` enum lives in `Concertable.Core/Enums/` (shared primitive giveaway)
- `IContractStrategy` marker + `IContractStrategyFactory<T>` + `IContractStrategyResolver<T>`
  live in `Concert.Application` today, but are consumed by three distinct strategy families:
  - `IConcertWorkflowStrategy` (Concert-owned: FlatFee/DoorSplit/VenueHire/Versus workflows)
  - `ITicketPaymentStrategy` (Payment-owned: Venue/Artist ticket payment services)
  - `IStripeValidationStrategy` (Payment-owned: Account/Customer validators)
- `IContractRepository.GetTypeBy{Concert,Application,Booking}IdAsync` joins Contract rows
  to Concert, Application, and Booking tables — Contract is a distinct aggregate, not a
  Concert aggregate member
- Contract entities (`FlatFeeContractEntity`, `DoorSplitContractEntity`, `VenueHireContractEntity`,
  `VersusContractEntity`) were parked in `Concert.Domain/Entities/Contracts/` during Concert
  extraction (2026-04-23) as a holding pattern

**Conclusion:** Contract is its own bounded concept. Multiple modules read contract type and
resolve strategies keyed by it — but resolution is the *consumer's* concern, not Contract's.
Contract exposes terms (the DTOs) and type identity (`ContractType` for the key); consumers own
their own strategy families and their own keyed DI wiring. Extracting Contract before Payment
means Payment inherits a clean boundary rather than Concert's anti-pattern.

---

## 2. Starting state (discovery, 2026-04-24)

### 2.1 Existing files

**Entities — currently in `api/Modules/Concert/Concertable.Concert.Domain/Entities/Contracts/`:**
- `ContractEntity.cs` — abstract **TPT** base (NOT TPH — see banner); has `Id`, `PaymentMethod`,
  abstract `ContractType`, nav `OpportunityEntity Opportunity`. PK is reused as FK to
  Opportunity (`.HasForeignKey<ContractEntity>(c => c.Id)`) — split during extraction (banner).
- `FlatFeeContractEntity.cs`
- `DoorSplitContractEntity.cs`
- `VenueHireContractEntity.cs`
- `VersusContractEntity.cs`

**Interfaces — currently in `api/Modules/Concert/Concertable.Concert.Application/Interfaces/`:**
- `IContract.cs` — polymorphic wire interface with `[JsonDerivedType]` discriminators
- `IContractStrategy.cs` — empty marker (`internal`)
- `IContractStrategyFactory.cs` — `internal interface IContractStrategyFactory<T> where T : IContractStrategy { T Create(ContractType); }`
- `IContractStrategyResolver.cs` — `internal interface IContractStrategyResolver<T> where T : IContractStrategy { ResolveForConcertAsync, ResolveForApplicationAsync, ResolveForBookingAsync }`
- `IContractRepository.cs` — `internal interface IContractRepository : IIdRepository<ContractEntity>` with 6 methods
- `IConcertWorkflowStrategy.cs` — `internal interface IConcertWorkflowStrategy : IContractStrategy { InitiateAsync, SettleAsync, FinishedAsync }`
- `IAcceptDispatcher.cs`, `ISettlementDispatcher.cs`, `IFinishedDispatcher.cs` — each one method, each internal

**DTOs — currently in `api/Modules/Concert/Concertable.Concert.Application/DTOs/`:**
- `FlatFeeContract.cs`, `DoorSplitContract.cs`, `VenueHireContract.cs`, `VersusContract.cs` (implementing `IContract`)

**Impls — currently in `api/Concertable.Infrastructure/Factories/`:**
- `ContractStrategyFactory.cs` — `internal class ContractStrategyFactory<T> : IContractStrategyFactory<T> where T : IContractStrategy` — thin `GetRequiredKeyedService<T>(contractType)` wrapper
- `ContractStrategyResolver.cs` — `internal class ContractStrategyResolver<T>` — injects `IContractRepository` + factory, delegates

**Dispatcher impls — `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/`:**
- `Accept/AcceptDispatcher.cs`
- `Settlement/SettlementDispatcher.cs`
- `Complete/FinishedDispatcher.cs`
- Each injects `IContractStrategyResolver<IConcertWorkflowStrategy>`, resolves, delegates to one strategy method.

**Repo impl — already in `api/Modules/Concert/Concertable.Concert.Infrastructure/Repositories/ContractRepository.cs`.**
Discovery confirmed: NOT in legacy `Concertable.Infrastructure/` — the Concert extraction already
brought it across. Step 6 is a Concert→Contract relocation, not legacy→Contract.

**Enums — currently in `api/Modules/Concert/Concertable.Concert.Domain/Enums/`** (NOT
`Concertable.Core/Enums/` — discovery confirmed neither file exists in Core):
- `ContractType.cs` (FlatFee, DoorSplit, VenueHire, Versus)
- `PaymentMethod.cs`

**EF configs — currently in `api/Modules/Concert/Concertable.Concert.Infrastructure/Data/Configurations/ContractEntityConfiguration.cs`**
(NOT in legacy `Data.Infrastructure/Data/Configurations/` — Concert extraction already
relocated them). File contains TPT config + 4 subtype configs (`FlatFeeContractEntityConfiguration`,
`DoorSplitContractEntityConfiguration`, `VersusContractEntityConfiguration`,
`VenueHireContractEntityConfiguration`). Each subtype gets its own table.

**Tests:**
- `api/Tests/Concertable.Infrastructure.UnitTests/Factories/ContractStrategyFactoryTests.cs`
- `api/Tests/Concertable.Infrastructure.UnitTests/Factories/ContractStrategyResolverTests.cs`
- Dispatcher tests under `api/Tests/Concertable.Infrastructure.UnitTests/Services/{Accept,Settlement,Complete}/`

### 2.2 Cross-module couplings at extraction start

| Coupling | Direction | Resolution |
|---|---|---|
| `ContractEntity.Opportunity` CLR nav → `OpportunityEntity` | Contract → Concert | **Dropped.** `ContractEntity` gains an explicit `int OpportunityId` column (currently the FK is encoded as PK reuse via `.HasForeignKey<ContractEntity>(c => c.Id)` — split per banner second-pass lock). No CLR nav. `Contract.Domain` has NO project ref to `Concert.Domain`. |
| `OpportunityEntity.Contract` CLR nav → `ContractEntity` | Concert → Contract | **Dropped.** `OpportunityEntity.ContractId` (int) replaces the nav. `OpportunityEntity.Create(int venueId, DateRange period, int contractId, ...)`. `Concert.Domain` has NO project ref to `Contract.Domain`. Concert reads terms via `IContractModule.GetByOpportunityAsync`. |
| `OpportunityApplicationEntity.Contract` nav (if exists) | Concert → Contract | **Dropped.** If a nav exists today it becomes a plain int FK or is removed entirely (application already has `OpportunityId`; contract is reachable via opportunity). Confirm in Step 0. |
| Concert dispatchers resolve `IContractStrategyResolver<IConcertWorkflowStrategy>` | Concert → Contract | Replaced by Concert-internal `IConcertWorkflowStrategyFactory` (§3.1 locked). Dispatchers inject the factory, not a cross-module resolver. |
| Payment services resolve `IContractStrategyResolver<ITicketPaymentStrategy>` + `IContractStrategyFactory<IStripeValidationStrategy>` | Payment → Contract | When Payment extracts: Payment-internal `ITicketPaymentStrategyFactory` + `IStripeValidationStrategyFactory`, same shape. For now (Contract extraction in isolation) the legacy Payment impls in `Concertable.Infrastructure` temporarily adopt the same non-generic factory pattern and retire when Payment extracts. |
| `IContractRepository` consumers (Concert workflow services that read `FlatFeeContractEntity.Fee` etc.) | Concert → Contract | Replaced by `IContractModule.GetByOpportunityAsync(opportunityId) → IContract?`. Caller hands the `IContract` to its strategy opaquely — no subtype pattern-matching; the strategy already knows which subtype it is (it's keyed by `ContractType`). |

**Aggregate ownership call (2026-04-24):** `OpportunityEntity`, `OpportunityApplicationEntity`,
`BookingEntity`, `ConcertEntity`, `TicketEntity` all **stay in Concert**. Opportunity is the
entry point into Concert's booking pipeline, not a Contract aggregate. Contract owns the
*terms*; Concert owns the *posting, application, booking, show*.

**Cross-module nav removal — no rule-3 carryover this time.** Both Domain-to-Domain project
references are removed at extraction, not deferred. Two aggregates referencing each other by
id only; orchestration binds them at creation time via the service layer. No EF navs cross
the module boundary. Runtime cost: one extra indexed PK lookup per Concert workflow operation
that needs contract terms (Concert resolves `opportunityId` from its own aggregate, then calls
the facade). Negligible — both queries hit primary keys. Labor cost: cataloguing every
`.Include(x => x.Contract)` and `opportunity.Contract.*` access site during Step 0 discovery,
so Step 3/9 rewrites them in one pass.

---

## 3. Architectural decisions

### 3.1 Strategy resolution — per-consumer factories, Contract hosts no resolution

**LOCKED (2026-04-24).**

Contract does not host strategy resolution. Each consuming module writes a tiny internal
factory per strategy family it owns:

```csharp
// Concert.Application (internal)
internal interface IConcertWorkflowStrategyFactory
{
    IConcertWorkflowStrategy Create(ContractType type);
}

// Concert.Infrastructure (internal)
internal sealed class ConcertWorkflowStrategyFactory(IServiceProvider sp) : IConcertWorkflowStrategyFactory
{
    public IConcertWorkflowStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<IConcertWorkflowStrategy>(type);
}
```

The factory is the **only** place `IServiceProvider` appears — 5-line class, one job.
Services and dispatchers inject `IConcertWorkflowStrategyFactory`, never `IServiceProvider`.

Dispatchers become:

```csharp
internal sealed class SettlementDispatcher(
    IContractModule contractModule,
    IBookingRepository bookingRepo,
    IConcertWorkflowStrategyFactory strategyFactory) : ISettlementDispatcher
{
    public async Task SettleAsync(int bookingId)
    {
        var opportunityId = await bookingRepo.GetOpportunityIdAsync(bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found");
        var contract = await contractModule.GetByOpportunityAsync(opportunityId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
        await strategyFactory.Create(contract.Type).SettleAsync(bookingId, contract);
    }
}
```

One PK hop for `opportunityId`, one PK fetch for the full `IContract`, then delegate. Strategy
receives the contract it needs as a method parameter — no second lookup inside the strategy.

`AcceptanceDispatcher` uses `IOpportunityApplicationRepository.GetOpportunityIdAsync(applicationId)`
and calls `strategy.InitiateAsync(applicationId, contract, paymentMethodId)`.
`CompletionDispatcher` uses `IConcertRepository.GetOpportunityIdAsync(concertId)` (or the existing
`ConcertEntity.OpportunityId` path — confirm in Step 0) and calls `strategy.FinishAsync(concertId, contract)`.

**Why this is the MM-clean answer:**

- Contract exposes only data (`ContractType` + `IContract`). It does not resolve anything.
  No cross-module DI reach. If Contract extracted to a microservice, the two facade methods
  translate directly (`GET /contracts/by-opportunity/{id}/type`, `GET /contracts/by-opportunity/{id}`).
- Each module's `IXStrategyFactory` is internal, typed to its own strategy interface, and
  owns its own `IServiceProvider` indirection. Bounded service-locator in one tiny class per
  family — the standard pattern for keyed DI with runtime keys.
- `IContractStrategy` marker survives in `Contract.Abstractions` as a compile-time restriction:
  "this strategy family is keyed by `ContractType`." Concert's `IConcertWorkflowStrategy :
  IContractStrategy`, Payment's two strategies likewise. No runtime role; not consumed by
  `IContractModule`.
- Per-module factory duplication is trivial (three 5-line classes at most, across Concert +
  Payment) and internal. Unlike the earlier Shape B, nothing leaks cross-module.

**Rejected alternatives (retained for audit):**

- **Generic `ResolveFor…Async<T>` methods on `IContractModule`** (Shape B, earlier lock).
  Made Contract.Infrastructure inject `IServiceProvider` and reach into the container to
  resolve Concert/Payment strategies. Contract hosting an abstraction for others' benefit
  it does not itself consume. Fails the microservice litmus test — generic `T` doesn't
  cross an HTTP boundary.
- **Generic `IContractStrategyFactory<T>` / `IContractStrategyResolver<T>` as open generics
  in `Contract.Abstractions`.** Same anti-pattern in different clothing — exposes Contract's
  DI mechanism as public API surface.
- **One fat `IConcertWorkflowService` with all three lifecycle methods (Accept/Settle/Finish)
  replacing the three dispatchers.** Rejected — loses ISP narrowing. `SettlementWebhookHandler`
  injecting that service could accidentally invoke accept/finish; the one-method dispatcher
  interfaces prevent that structurally.
- **`IServiceProvider` injected directly into dispatchers/services.** Service-locator.
  The tiny factory class is the bounded form that keeps the locator in one place and lets
  everything else inject a typed dependency.

### 3.2 `PaymentMethod` enum placement

**LOCKED: `Contract.Abstractions`.** Term of the deal, co-located with `ContractType`. Payment
references `Contract.Abstractions` to consume it.

### 3.3 `ContractDbContext` vs ride along with Concert

**LOCKED (2026-04-24, second pass): RIDE-ALONG on `ConcertDbContext`.**

Contract entities (`ContractEntity` TPH base + `FlatFee` / `DoorSplit` / `VenueHire` / `Versus`
subtypes) are configured on `ConcertDbContext`, not on a dedicated `ContractDbContext`.

**Why ride-along:**
- Contract is 1:1 with Opportunity via `OpportunityId` FK — there is no scenario where a Contract
  exists without an Opportunity, and Opportunity is Concert-owned. The aggregate boundary that
  matters at the Domain level (Contract owns its terms; Concert owns the posting/application/
  booking/show) is preserved by §2.2 (no Domain-to-Domain navs) and §3.4 (`IContractModule`
  facade). The DbContext seam adds nothing on top of that.
- Avoids a Respawner / migration history / cross-context FK seam for one TPH hierarchy.
- Contract still gets its own four-project module structure (Contracts/Domain/Application/
  Infrastructure/Api per §3.7) — the boundary is at the project + facade level, not the
  DbContext level.

**Concert.Infrastructure references `Contract.Domain`** to apply `ContractEntityConfiguration`.
This is allowed under CLAUDE.md rule 3 — Domain ref is the accepted escape hatch for entity
types in the modular monolith.

**`ContractRepository` (in `Contract.Infrastructure`) injects `ConcertDbContext`.** Concert
grants `InternalsVisibleTo("Concertable.Contract.Infrastructure")` so Contract's repo can see
the internal `ConcertDbContext` type.

**No CLR navs across the Contract/Concert boundary** (per §2.2 — locks unaffected). Cross-
aggregate access goes via `IContractModule.GetByOpportunityAsync` even though both contexts
are now the same physical DbContext — the facade is the architectural seam, not the context.

**Migration order unchanged from prior modules:** Shared → Identity → Artist → Venue → Concert
→ AppDb. Contract entities are scaffolded as part of Concert's `InitialCreate`. Payment slots
between Concert and AppDb when it extracts.

**Future evolution:** if/when Contract grows independent persistence concerns (versioning,
addenda, separate audit trail), promoting to a dedicated `ContractDbContext` is a focused
follow-up — change `ContractRepository`'s injected context, scaffold a new context, move the
config. The facade callers see no change.

### 3.4 `IContractModule` shape

**LOCKED (2026-04-24).** One method, Opportunity-anchored:

```csharp
public interface IContractModule
{
    Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default);
}
```

**`IContract` gains `ContractType Type { get; }`** so the dispatcher's factory call
(`factory.Create(contract.Type)`) has the key it needs from the same object. Each subtype DTO
implements the property with a hard-coded expression body.

**Why only Opportunity-anchored:** Contract's only upstream FK is `ContractEntity.OpportunityId`.
Exposing `*ForApplication` / `*ForBooking` / `*ForConcert` would force `Contract.Infrastructure`
SQL to JOIN across Concert-owned tables (`OpportunityApplications`, `Bookings`, `Concerts`) —
a boundary leak going the opposite direction from the one this extraction exists to fix.

Concert owns Application/Booking/Concert aggregates; resolving `opportunityId` from any of
them is a cheap indexed PK lookup using a repo Concert already has. Two queries on PKs
instead of one three-table join — measurable in microseconds, unmeasurable to users.

**Why no `GetTypeForOpportunityAsync`:** the dispatcher needs the full contract anyway (to
pass into the strategy as a method parameter, per §3.1). A type-only accessor would either
force a second DB round trip or go unused. Callers who only need the type read `.Type` on
the returned `IContract`.

**`IContract` is the return type, not per-subtype read models.** `IContract` is already a
polymorphic wire shape (`[JsonDerivedType]` + four DTOs). The dispatcher hands it to the
strategy; the strategy casts to its expected subtype once at the top of the method
(`var terms = (FlatFeeContract)contract;`) — safe by construction because keyed registration
and the `contract.Type` switch in the factory guarantee the match. **No subtype pattern-
matching at call sites outside strategies** — that's what the strategy pattern exists to prevent.

For callers that just render contract terms to the FE (controllers returning `IContract` in
a response body), JSON polymorphism handles subtype routing at serialization time. Still no
branching.

**Rejected alternatives:**
- Per-anchor methods (`*ForApplication/ForBooking/ForConcert`) — boundary leak described above.
- Per-subtype read-model records (`FlatFeeContractReadModel` etc.) on the facade — duplicates
  the DTO shape `IContract` already provides, forces 3 methods × 4 subtypes = 12-method facade.
- Keeping `GetTypeForOpportunityAsync` + `GetByOpportunityAsync` both — second method is unused
  once the dispatcher fetches the full contract, and the split invited the earlier two-round-
  trip design.

### 3.5 `ContractType` enum placement

**LOCKED: `Contract.Abstractions`.** Cross-module primitive keyed on by every consumer.

### 3.6 `InternalsVisibleTo` / test visibility

Standard pattern from prior extractions. Follow Concert/Venue/Identity precedent. Noted here
so it's not forgotten.

### 3.7 Module layout

**LOCKED (2026-04-24, second pass): FIVE projects, `.Api` INCLUDED.**

- `Concertable.Contract.Abstractions` — `ContractType`, `PaymentMethod`, `IContract` + 4 polymorphic DTOs, `IContractStrategy` marker, `IContractModule`
- `Concertable.Contract.Domain` — `ContractEntity` + 4 TPH subtypes, domain events (if any)
- `Concertable.Contract.Application` — `IContractRepository`, `IContractService`, `ContractEntityConfiguration` (TPH config — referenced by `Concert.Infrastructure` per §3.3 ride-along), mappers
- `Concertable.Contract.Infrastructure` — `ContractRepository` (injects `ConcertDbContext` per §3.3), `ContractModule` facade impl, `AddContractModule()`. **No `ContractDbContext`** — Contract rides Concert's per §3.3.
- `Concertable.Contract.Api` — `ContractController` (moved from `Concert.Api/Controllers/`). Internal class + `InternalControllerFeatureProvider` per Module.Api pattern (Identity/Artist/Search precedent).

**`.Api` rationale:** the contract HTTP endpoint exists today —
`Concert.Api/Controllers/ContractController.cs` exposes
`GET api/contract/opportunity/{opportunityId}` backed by
`Concert.Application/Interfaces/IContractService.cs` (`GetByOpportunityIdAsync(int) → IContract`).
Both move to Contract on extraction:
- `IContractService` → `Contract.Application/Interfaces/` (stays `internal`)
- `ContractController` → `Contract.Api/Controllers/` (stays `internal`)
- Controller injects `IContractService`; service injects `IContractModule` (or `IContractRepository`
  + mapper directly — confirm in Step 0 / Step 7). Likely the service just delegates to the facade.

**Note on `ContractEntityConfiguration` placement:** the EF config lives in `Contract.Application`
(not `Contract.Infrastructure`) so `Concert.Infrastructure` can reference it via the
`Application` project to apply on `ConcertDbContext`. Alternative: put the config in
`Contract.Infrastructure` and have `Concert.Infrastructure` reference that instead. **Default:
`Contract.Application`** — keeps the dependency direction shallow (Infrastructure → Application
of a sibling, not Infrastructure → Infrastructure). Confirm during Step 5.

---

## 4. Implementation steps

### Step 0 — Discovery sweep (before any code changes)

Capture complete picture in Appendix A (to be filled during Step 0):

- Every injection of `IContractRepository` (location + method called)
- Every injection of `IContractStrategyFactory<T>` (Concert + Payment workflows)
- Every injection of `IContractStrategyResolver<T>` (Concert + Payment)
- Every `[FromKeyedServices(ContractType.*)]` attribute site
- Full DI snapshot: every Contract-related registration in
  `Web/Extensions/ServiceCollectionExtensions.cs` + `Workers/ServiceCollectionExtensions.cs` +
  Concert's `AddConcertModule()`
- `ContractConfigurations.cs` full body (TPH discriminator shape, FK definitions)
- `ContractRepository` impl location + implementation
- `IContractService` impl location (`Concert.Application` or `Concert.Infrastructure`) + every injection site
- `ContractController` injection sites + every endpoint exposed
- Any `ConcertDbContext.DbSet<ContractEntity>` or subtype DbSets
- `OpportunityEntity.Contract` CLR nav (confirm exists, all read sites via `opportunity.Contract.*`
  and `.Include(x => x.Contract)`)
- `OpportunityApplicationEntity.Contract` reverse nav (if exists)
- `OpportunityApplicationEntity.OpportunityId` / `BookingEntity.OpportunityId` /
  `ConcertEntity.OpportunityId` — confirm each carries the FK we'll use for the local hop
- Confirm each Concert repo (`IOpportunityApplicationRepository`, `IBookingRepository`,
  `IConcertRepository`) exposes or can cheaply add `GetOpportunityIdAsync(int id)` projection
- Concrete field shapes for each contract subtype (`FlatFee.Fee`, `DoorSplit.VenueSplit` /
  `ArtistSplit`, `VenueHire.Fee`, `Versus.*`) — confirm the existing `IContract` DTOs already
  carry them (they should — they're the wire shape)
- Any tests referencing `IContractStrategy` / `IContractStrategyFactory` /
  `IContractStrategyResolver` / `ContractRepository` that will need IVT grants

**Output:** Appendix A in this document filled before Step 1.

### Step 1 — Scaffold 5 Contract projects

Under `api/Modules/Contract/`:
- `Concertable.Contract.Abstractions`
- `Concertable.Contract.Domain`
- `Concertable.Contract.Application`
- `Concertable.Contract.Infrastructure`
- `Concertable.Contract.Api`

Add all five to `Concertable.sln` under `Modules/Contract` folder (watch for the
`dotnet sln --solution-folder` duplicate-parent bug — see
`feedback_sln_solution_folder_duplicate.md`).

Project refs:
- `Contract.Domain → Contract.Abstractions, Shared.Domain`
- `Contract.Application → Contract.Domain, Contract.Abstractions, Shared.Application, Data.Application` *(EF Core SqlServer ref so `ContractEntityConfiguration` can use Fluent API; same pattern as Identity/Venue/Concert configs)*
- `Contract.Infrastructure → Contract.Application, Contract.Domain, Contract.Abstractions, Data.Application, Shared.Infrastructure, **Concert.Infrastructure**` *(for `ConcertDbContext` injection per §3.3 ride-along — gated by `InternalsVisibleTo`)*
- `Contract.Api → Contract.Application, Contract.Abstractions, Shared.Application` *(controller injects internal `IContractService` from Application; matches Identity/Artist/Venue/Concert.Api shape)*
- `Contract.Abstractions → Shared.Domain` (usually none needed)

**Explicit non-refs:** `Contract.Domain` does NOT reference `Concert.Domain`. `Contract.Application`
does NOT reference any Concert project (configs apply on a foreign context but the config class
itself has no Concert ref). `Contract.Api` does NOT reference any Concert project.

**New ref direction (per §3.3 ride-along):** `Concert.Infrastructure → Contract.Domain` (to apply
`ContractEntityConfiguration` on `ConcertDbContext`) and `Concert.Infrastructure → Contract.Application`
(if the config lives there, per §3.7 default). `Contract.Infrastructure → Concert.Infrastructure`
(to inject `ConcertDbContext`). Both are Domain/Infrastructure-direction refs allowed under
CLAUDE.md rule 3.

`AssemblyInfo.cs` placements:
- `Contract.Application/AssemblyInfo.cs`:
  - `InternalsVisibleTo("Concertable.Contract.Infrastructure")`
  - `InternalsVisibleTo("Concertable.Contract.Api")` *(for controller → service)*
  - `InternalsVisibleTo("Concertable.Concert.Infrastructure")` *(for config application — confirm scope in Step 5; may only need to expose the config class itself, which can be public if narrower IVT is awkward)*
  - TEMPORARY `InternalsVisibleTo("Concertable.Infrastructure")` — retires in Step 11
  - TEMPORARY `InternalsVisibleTo("Concertable.Application")` — retires in Step 11
  - `InternalsVisibleTo("DynamicProxyGenAssembly2")` long-form if any existing tests mock internals
- `Contract.Infrastructure/AssemblyInfo.cs`:
  - `InternalsVisibleTo("Concertable.Web.IntegrationTests")` if needed
- `Concert.Infrastructure/AssemblyInfo.cs` (existing — add):
  - `InternalsVisibleTo("Concertable.Contract.Infrastructure")` *(so `ContractRepository` can inject `ConcertDbContext`)*

### Step 2 — Move `ContractType` + `PaymentMethod` to `Contract.Abstractions`

- Move `api/Modules/Concert/Concertable.Concert.Domain/Enums/ContractType.cs` →
  `Concertable.Contract.Abstractions/ContractType.cs`
- Move `api/Modules/Concert/Concertable.Concert.Domain/Enums/PaymentMethod.cs` →
  `Concertable.Contract.Abstractions/PaymentMethod.cs`
- Namespace: `Concertable.Contract.Abstractions` (top-level, no `.Enums` subdir)
- Add project refs to `Concertable.Contract.Abstractions` from every consuming assembly that
  previously read these via `Concert.Domain` (or its global usings). Discovery showed ~24
  files reference `ContractType.` — most live in Concert.Application / Concert.Infrastructure /
  Web / legacy Concertable.Application / legacy Concertable.Infrastructure / test projects.
- Add `global using Concertable.Contract.Abstractions;` to those assemblies. Mirror Venue/Artist
  extraction pattern.
- Rebuild — fix namespace errors as they surface.

### Step 3 — Move `ContractEntity` hierarchy + `IContract` DTOs

**Entities** — move from `api/Modules/Concert/Concertable.Concert.Domain/Entities/Contracts/`
to `Concertable.Contract.Domain/Entities/`:
- `ContractEntity.cs` (abstract TPH base)
- `FlatFeeContractEntity.cs`
- `DoorSplitContractEntity.cs`
- `VenueHireContractEntity.cs`
- `VersusContractEntity.cs`

Namespace: `Concertable.Contract.Domain.Entities`.

**Drop** `ContractEntity.Opportunity` CLR nav. **Add explicit `int OpportunityId`** property
(currently absent — FK was encoded as PK reuse). `ContractEntity.Create(...)` /
`ContractEntity` constructor takes `int opportunityId` and assigns it. Subtype factory methods
(`FlatFeeContractEntity.Create(...)`, etc.) accept and forward `opportunityId`.

**Confirm no ref** `Contract.Domain → Concert.Domain`.

**DTOs** — move from `api/Modules/Concert/Concertable.Concert.Application/DTOs/ContractDtos.cs`
(discovery: all four DTOs live in a single file, named `FlatFeeContractDto` /
`DoorSplitContractDto` / `VersusContractDto` / `VenueHireContractDto`) and
`api/Modules/Concert/Concertable.Concert.Application/Interfaces/IContract.cs` to
`Concertable.Contract.Abstractions/`:
- `IContract.cs` (public). Keep the four `[JsonDerivedType]` attributes; rename type-args from
  the `Dto` suffix variants if we drop the suffix on extraction (see below).
- `FlatFeeContract.cs`, `DoorSplitContract.cs`, `VenueHireContract.cs`, `VersusContract.cs`
  (public). **Rename without `Dto` suffix on the move** — these are the public wire shape, not
  internal DTOs. Mirrors the IContract naming. Update `[JsonDerivedType(typeof(...))]` lines to
  match.

Namespace: `Concertable.Contract.Abstractions`.

**`IContract` already has `ContractType ContractType { get; }`** (discovery confirmed each DTO
implements it via `=> ContractType.FlatFee` etc.). Plan §3.1 / §3.4 referred to this as `Type`;
**keep the existing `ContractType` property name** (better, avoids a needless rename and matches
what readers/serializers see today). Factory call becomes `factory.Create(contract.ContractType)`.
Each subtype keeps its hard-coded expression body:

```csharp
public sealed record FlatFeeContract : IContract
{
    public int Id { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ContractType ContractType => ContractType.FlatFee;
    public decimal Fee { get; set; }
}
```

Ditto `DoorSplit`, `VenueHire`, `Versus`. Factory switches on `contract.ContractType` in §3.1.

**On Concert side** — drop `OpportunityEntity.Contract` nav. Replace with `int ContractId`.
Update `OpportunityEntity.Create(int venueId, DateRange period, ContractEntity contract, ...)`
→ `OpportunityEntity.Create(int venueId, DateRange period, int contractId, ...)`. **Note**:
discovery confirmed Opportunity-side already takes `ContractEntity contract` and assigns
`Contract = contract`; rewrite to take `int contractId` and assign `ContractId = contractId`.
Remove `.Include(o => o.Contract)` from `OpportunityRepository.cs` (lines 21, 41) and
`OpportunityApplicationRepository.cs` (lines 23, 63) — replace call sites in Step 9 with
facade calls.

Verify no cycle by building both Domain projects. They should have zero mutual refs.

### Step 4 — Move `IContractRepository` + delete cross-module abstractions

**Move** to `Concertable.Contract.Application/Interfaces/`:
- `IContractRepository.cs` — stays **internal**. Shrink to:
  ```csharp
  internal interface IContractRepository : IIdRepository<ContractEntity>
  {
      Task<ContractEntity?> GetByOpportunityIdAsync(int opportunityId, CancellationToken ct = default);
      // + write methods if Contract owns any (confirm in Step 0; add as needed)
  }
  ```
  Discovery showed the existing interface has 6 methods — three generic `GetBy{Opportunity,Concert,Application}Id<T>(...)`
  variants and three `GetTypeBy{Concert,Application,Booking}Id(...)` non-generics. All three
  generic variants and the type-only methods are **deleted**:
  - `GetByConcertIdAsync<T>` / `GetByApplicationIdAsync<T>` join across Concert-owned tables
    (`Concerts`, `OpportunityApplications`) — boundary leak.
  - `GetTypeByApplicationIdAsync` / `GetTypeByBookingIdAsync` / `GetTypeByConcertIdAsync`
    likewise (and §3.4 facade has one method only).
  - `GetByOpportunityIdAsync<T>` collapses to a non-generic `GetByOpportunityIdAsync` returning
    base `ContractEntity?` — workflows no longer ask for typed subtypes (the dispatcher hands
    them an `IContract` per §3.1, and they cast to their expected subtype DTO at the top of
    each method).

**Move marker** to `Concertable.Contract.Abstractions/`:
- `IContractStrategy.cs` — now **public**, empty marker. Constraint-only usage.

**Delete** (per §3.1 decision):
- `api/Modules/Concert/Concertable.Concert.Application/Interfaces/IContractStrategyFactory.cs`
- `api/Modules/Concert/Concertable.Concert.Application/Interfaces/IContractStrategyResolver.cs`
- `api/Concertable.Infrastructure/Factories/ContractStrategyFactory.cs`
- `api/Concertable.Infrastructure/Factories/ContractStrategyResolver.cs`
- `api/Tests/Concertable.Infrastructure.UnitTests/Factories/ContractStrategyFactoryTests.cs`
- `api/Tests/Concertable.Infrastructure.UnitTests/Factories/ContractStrategyResolverTests.cs`

**Keep on strategy family interfaces** (per §3.1 marker role):
- `IConcertWorkflowStrategy : IContractStrategy` — unchanged.
- `ITicketPaymentStrategy : IContractStrategy` — unchanged.
- `IStripeValidationStrategy : IContractStrategy` — unchanged.

Every call site that previously injected `IContractStrategyFactory<T>` /
`IContractStrategyResolver<T>` compiles broken until Step 9 (those get rewired there).

### Step 5 — Configure `ContractEntity` on `ConcertDbContext` (ride-along per §3.3)

**No `ContractDbContext` is created.** Contract entities ride on `ConcertDbContext`.

Move TPT configs (5 files — base + 4 subtypes per discovery):
- Source: `api/Modules/Concert/Concertable.Concert.Infrastructure/Data/Configurations/ContractEntityConfiguration.cs`
  (single file containing `ContractEntityConfiguration` + `FlatFeeContractEntityConfiguration` +
  `DoorSplitContractEntityConfiguration` + `VersusContractEntityConfiguration` +
  `VenueHireContractEntityConfiguration`).
- Destination: `Contract.Application/Data/Configurations/`. Classes stay `internal`;
  `Concert.Infrastructure` gets IVT to `Contract.Application` (added in Step 1).
- Strategy: TPT (NOT TPH per banner). Keep `.UseTptMappingStrategy()` on the base config.
  Keep per-subtype `ToTable("FlatFeeContracts")` etc. mappings — discovery confirmed each
  subtype has its own table, no discriminator column.
- **Update FK config:** the base config currently sets
  `.HasOne(c => c.Opportunity).WithOne(o => o.Contract).HasForeignKey<ContractEntity>(c => c.Id)` —
  rewrite to remove the nav reference and key on the new `OpportunityId` column:
  ```csharp
  builder.Property(c => c.OpportunityId).IsRequired();
  builder.HasIndex(c => c.OpportunityId).IsUnique(); // 1:1
  // No HasOne/.WithOne — nav dropped both directions per §2.2
  ```
  Opportunity-side FK is configured on `OpportunityEntityConfiguration` (Concert.Infrastructure):
  `builder.Property(o => o.ContractId).IsRequired(); builder.HasIndex(o => o.ContractId).IsUnique();`
  No CLR nav, FK constraint declared as a model-level relationship by EF inference (or made
  explicit if EF doesn't pick it up — confirm at scaffold time).

Update `ConcertDbContext.OnModelCreating`:
- The five `ApplyConfiguration(new ...EntityConfiguration())` calls already present for Contract
  configs stay (Concert.Infrastructure references Contract.Application for the config types now).
- Keep existing Concert configs untouched.

Update `ConcertDbContext` DbSets:
- DbSets already exist (discovery: lines 22–26 of `ConcertDbContext.cs`). Keep them as-is —
  `Contracts`, `FlatFeeContracts`, `DoorSplitContracts`, `VersusContracts`, `VenueHireContracts`.
  TPT requires the subtype DbSets for direct subtype queries; no change needed.

`Contract.Infrastructure.csproj`: refs `Data.Application` (for `IIdRepository<>` etc.) and
`Concert.Infrastructure` (for `ConcertDbContext`). No EF Core SqlServer ref needed (the config
lives in `Contract.Application`, which carries the EF dep; the repo just uses `DbContext` API
through `ConcertDbContext`).

**No `AddContractModule()` DbContext registration** — `ConcertDbContext` is already registered
by `AddConcertModule()`. `AddContractModule()` only registers `IContractRepository`,
`IContractService`, `IContractModule`, `IContractMapper`, `ContractDevSeeder`, `ContractTestSeeder`.

### Step 6 — Relocate `ContractRepository` impl to `Contract.Infrastructure`

**Move** `ContractRepository.cs` from
`api/Modules/Concert/Concertable.Concert.Infrastructure/Repositories/` (NOT legacy
`Concertable.Infrastructure/` — Concert extraction already brought it across) →
`Contract.Infrastructure/Repositories/`.

Currently inherits `IdModuleRepository<ContractEntity, ConcertDbContext>` — keep that base type
(it's in `Data.Application`, generic over context). Continues to inject `ConcertDbContext`
through the base ctor (per §3.3 ride-along — not `ApplicationDbContext`, not a dedicated
`ContractDbContext`). Visibility works because `Concert.Infrastructure/AssemblyInfo.cs` grants
`InternalsVisibleTo("Concertable.Contract.Infrastructure")` (added in Step 1).

**Body shrinks to the single `GetByOpportunityIdAsync(int opportunityId)`** read method. With
the explicit `OpportunityId` column (banner second-pass split), the query simplifies from
`context.Opportunities.Where(o => o.Id == opportunityId).Select(o => o.Contract)` to
`context.Contracts.FirstOrDefaultAsync(c => c.OpportunityId == opportunityId)`. Plus any
writes Step 0 confirms Contract owns (none surfaced in discovery — opportunities are created
with their contracts, all from Concert's OpportunityService.Create flow which would translate
to a contract write driven by Concert).

Implement `SaveChangesAsync()` pattern per `feedback_module_service_saves_own_context.md` —
never `IUnitOfWork`. Save on `ConcertDbContext` since that's the injected context.

Register in `AddContractModule()`: `services.AddScoped<IContractRepository, ContractRepository>()`.

### Step 6.5 — Move `IContractService` + `ContractController` into Contract module

**Move `IContractService` + impl** from `Concert.Application/Interfaces/IContractService.cs`
(and impl wherever Step 0 finds it) → `Contract.Application/Interfaces/IContractService.cs` +
`Contract.Application/Services/ContractService.cs`. Stays `internal`. Likely shape after move:

```csharp
internal interface IContractService
{
    Task<IContract> GetByOpportunityIdAsync(int opportunityId);
}

internal sealed class ContractService(IContractModule contractModule) : IContractService
{
    public async Task<IContract> GetByOpportunityIdAsync(int opportunityId)
        => await contractModule.GetByOpportunityAsync(opportunityId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
}
```

The service is a thin wrapper that throws on missing — the controller surface returns `IContract`,
not `IContract?`, so the null-to-NotFound translation lives here. If Step 0 reveals a more
substantive service (validators, write methods), expand accordingly.

**Move `ContractController`** from `Concert.Api/Controllers/ContractController.cs` →
`Contract.Api/Controllers/ContractController.cs`. Stays `internal class`, follows the
`InternalControllerFeatureProvider` pattern from Identity/Artist/Venue/Concert.Api.

Register in `AddContractModule()`: `services.AddScoped<IContractService, ContractService>()`.

`Concertable.Web` references `Contract.Api` (ApplicationPart discovery) per CLAUDE.md
"composition host" rule. `Concert.Api` no longer references Contract — the controller is gone
from there.

### Step 7 — Scaffold `IContractModule` facade

`Contract.Abstractions/IContractModule.cs`:
```csharp
public interface IContractModule
{
    Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default);
}
```

`Contract.Infrastructure/ContractModule.cs`:
```csharp
internal sealed class ContractModule(IContractRepository contractRepository, IContractMapper mapper)
    : IContractModule
{
    public async Task<IContract?> GetByOpportunityAsync(int opportunityId, CancellationToken ct = default)
    {
        var entity = await contractRepository.GetByOpportunityIdAsync(opportunityId, ct);
        return entity is null ? null : mapper.ToContract(entity);
    }
}
```

`IContractMapper` (internal to `Contract.Application`) maps `ContractEntity` → `IContract`
by inspecting concrete type and producing the matching DTO (`FlatFeeContract` /
`DoorSplitContract` / etc.). Standard AutoMapper-free mapper pattern from prior modules.

Per `feedback_no_ef_in_facade.md`: **no inline EF queries** in the facade impl. Delegate to
the repo; mapping is internal to Contract.

Register in `AddContractModule()`: `services.AddScoped<IContractModule, ContractModule>()`,
`services.AddScoped<IContractMapper, ContractMapper>()`.

### Step 8 — Remove `Contract` from `ApplicationDbContext` (Concert keeps it per §3.3 ride-along)

- Remove `DbSet<ContractEntity>` (and any subtype DbSets) from `ApplicationDbContext`.
- Remove the legacy `ApplyConfiguration(new ContractConfigurations())` call (or equivalent)
  from `ApplicationDbContext.OnModelCreating`.
- **`ConcertDbContext` keeps the `Contracts` DbSet + applies `ContractEntityConfiguration`
  per Step 5.** No exclusion needed — Contract is genuinely owned by `ConcertDbContext` now.
- Add `modelBuilder.Entity<ContractEntity>().ToTable("Contracts", t => t.ExcludeFromMigrations())`
  on `ApplicationDbContext` if `ApplicationDbContext` still needs the entity in its model for
  cross-context FK awareness (likely yes, for `OpportunityEntity.ContractId` FK metadata —
  confirm in Step 0). Pattern per `project_shared_dbcontext.md`.
- Rescaffold `ApplicationDbContext` `InitialCreate` and `ConcertDbContext` `InitialCreate`.
  `ConcertDbContext` migration creates `Contracts` table (with TPH discriminator) alongside
  `Opportunities` and friends — single migration, single context, no cross-context FK seam.
  `ApplicationDbContext` migration no longer creates `Contracts`.

### Step 9 — Rewire consumers: per-module factory + dispatcher updates

**Concert.Application** gains:
```csharp
internal interface IConcertWorkflowStrategyFactory
{
    IConcertWorkflowStrategy Create(ContractType type);
}
```

**Concert.Infrastructure** implements:
```csharp
internal sealed class ConcertWorkflowStrategyFactory(IServiceProvider sp) : IConcertWorkflowStrategyFactory
{
    public IConcertWorkflowStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<IConcertWorkflowStrategy>(type);
}
```

Register in `AddConcertModule()`:
```csharp
services.AddScoped<IConcertWorkflowStrategyFactory, ConcertWorkflowStrategyFactory>();
```

**Rename and rewrite three Concert dispatchers** — same shape each, different anchor. Class
renames: `AcceptDispatcher` → `AcceptanceDispatcher`, `FinishedDispatcher` → `CompletionDispatcher`
(method `FinishedAsync` → `FinishAsync`). `SettlementDispatcher` keeps its name.

```csharp
// AcceptanceDispatcher
internal sealed class AcceptanceDispatcher(
    IContractModule contractModule,
    IOpportunityApplicationRepository applicationRepo,
    IConcertWorkflowStrategyFactory strategyFactory) : IAcceptanceDispatcher
{
    public async Task<IAcceptOutcome> AcceptAsync(int applicationId, string? paymentMethodId = null)
    {
        var opportunityId = await applicationRepo.GetOpportunityIdAsync(applicationId)
            ?? throw new NotFoundException($"Application {applicationId} not found");
        var contract = await contractModule.GetByOpportunityAsync(opportunityId)
            ?? throw new NotFoundException($"No contract for opportunity {opportunityId}");
        return await strategyFactory.Create(contract.Type)
            .InitiateAsync(applicationId, contract, paymentMethodId);
    }
}

// SettlementDispatcher — same shape, bookingRepo + SettleAsync(bookingId, contract)
// CompletionDispatcher — same shape, concertRepo + FinishAsync(concertId, contract).
//   No Result<T> wrap — dispatcher signatures stay consistent; any error wrap belongs at the caller.
```

**Strategy interface shape** (confirms the decision from §3.1):

```csharp
internal interface IConcertWorkflowStrategy : IContractStrategy
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, IContract contract, string? paymentMethodId = null);
    Task SettleAsync(int bookingId, IContract contract);
    Task FinishAsync(int concertId, IContract contract);
}
```

**Add `GetOpportunityIdAsync(int id)` projection methods** to the Concert repos that need them.
Discovery confirms:
- `OpportunityApplicationEntity` already carries `OpportunityId` as a direct property — `IOpportunityApplicationRepository`
  still gets a `GetOpportunityIdAsync(int applicationId)` projection (1-column DB hit, not a full
  entity load) for consistency with the dispatcher pattern.
- `BookingEntity` (and `ConcertBookingEntity` per discovery — confirm class name in Step 9) does
  NOT carry `OpportunityId` — reaches via nav `Application.OpportunityId`. **Must add**
  `IConcertBookingRepository.GetOpportunityIdAsync(int bookingId)` projecting through the nav.
- `ConcertEntity` does NOT carry `OpportunityId` — reaches via nav `Booking.Application.OpportunityId`.
  **Must add** `IConcertRepository.GetOpportunityIdAsync(int concertId)` projecting through the
  nav chain.

Each is a single-column projection on PK-anchored joins — cheap.

**Concert workflow strategies** (`FlatFeeConcertWorkflow`, etc.) that previously read
`FlatFeeContractEntity.Fee` directly via a Contract.Domain type:
- Implement `IConcertWorkflowStrategy` with the new signatures above. Each method takes
  `IContract contract` as a parameter (populated by the dispatcher's `GetByOpportunityAsync`).
- Cast to the expected subtype at the top of each method:
  ```csharp
  public async Task SettleAsync(int bookingId, IContract contract)
  {
      var terms = (FlatFeeContract)contract;
      // use terms.Fee, terms.PaymentMethod, etc.
  }
  ```
  Safe by construction — keyed DI registration + the `contract.Type` switch in the factory
  guarantee the subtype matches. Mis-wiring blows up immediately in any integration test.
- No Concert code references `Contract.Domain` types, anywhere. Concert.Domain ref to
  Contract.Domain is removed in Step 3.

**Payment consumers** (legacy, still in `Concertable.Infrastructure`) — apply the same factory
pattern now even though Payment hasn't extracted:
- Add `ITicketPaymentStrategyFactory` + `TicketPaymentStrategyFactory` (5-line wrapper) in
  `Concertable.Infrastructure`. Rewrite `TicketPaymentDispatcher` to inject the factory +
  `IContractModule` + appropriate Concert repo for the opportunityId hop.
- Same for `IStripeValidationStrategyFactory` if a resolver/dispatcher uses it today.
- These relocate to `Payment.Application` / `Payment.Infrastructure` when Payment extracts
  (separate plan). For this extraction they stay in legacy and just adopt the new shape.

**DI registrations** (Web + Workers):
- Remove every `services.AddScoped(typeof(IContractStrategyFactory<>), typeof(ContractStrategyFactory<>))`
- Remove every `services.AddScoped(typeof(IContractStrategyResolver<>), typeof(ContractStrategyResolver<>))`
- Keyed `IConcertWorkflowStrategy`, `ITicketPaymentStrategy`, `IStripeValidationStrategy`
  registrations stay — they're still the gate for resolution.

### Step 10 — `ContractDevSeeder` + `ContractTestSeeder`

`internal class ContractDevSeeder : IDevSeeder` + `ContractTestSeeder : ITestSeeder`.
Both `Order = 3.5` (between Concert at 3 and Payment at 4 — `Order` is `double` per
`project_seeding_architecture.md`).

- Move Contract seed logic from whatever currently owns it (`ConcertDevSeeder` /
  `ConcertTestSeeder` most likely) into these new seeders.
- **Inject `ConcertDbContext`** (per §3.3 ride-along — not a dedicated context). Write contract
  rows keyed to seeded opportunities (which the Concert seeder created first, at `Order = 3`).
- `ContractTestSeeder` seeds one of each contract type for predictable fixtures.
- Register in `AddContractModule()`: `services.AddScoped<IDevSeeder, ContractDevSeeder>()`,
  `services.AddScoped<ITestSeeder, ContractTestSeeder>()`.

### Step 11 — Remove TEMPORARY `InternalsVisibleTo` grants

After Steps 4–9 land, Concert.Application no longer touches Contract internals directly —
it consumes via `IContractModule`. Remove:
- `InternalsVisibleTo("Concertable.Infrastructure")`
- `InternalsVisibleTo("Concertable.Application")`

Keep only:
- `InternalsVisibleTo("Concertable.Contract.Infrastructure")`
- `InternalsVisibleTo("Concertable.Web.IntegrationTests")` if any integration test needs
  Contract internals (unlikely)
- `InternalsVisibleTo("DynamicProxyGenAssembly2")` long-form if tests mock `IContractRepository`

### Step 12 — Migration re-scaffold

Delete every module's `Migrations/` folder. Rescaffold `InitialCreate` in dependency order:

**Shared → Identity → Artist → Venue → Concert → AppDb**
*(Payment slots between Concert and AppDb once Payment extraction resumes. Contract has no
own context per §3.3 ride-along — its tables are part of `ConcertDbContext`'s migration.)*

Checks:
- `ConcertDbContext` migration: creates **both** `Opportunities` and `Contracts` tables in the
  same migration. `Contracts.OpportunityId → Opportunities.Id` FK is intra-context (single
  DbContext) — full FK constraint declared inside Concert's migration with no cross-context
  gymnastics. `Opportunities.ContractId → Contracts.Id` likewise intra-context.
- `ApplicationDbContext` migration: `Contracts` fully excluded (entity present in model only
  for FK metadata if needed; `ExcludeFromMigrations` keeps it out).
- `SharedDbContext` migration: `__EFMigrationsHistory` (default name — per
  `feedback_shared_migrations_history_table.md`).

### Step 13 — Full test suite + IVT for mocked internals

Run Core + Infra + Workers unit tests + full integration suite. Expected fallout:
- Any test mocking `IContractRepository` / `IContractModule` / `IConcertWorkflowStrategyFactory`
  needs `DynamicProxyGenAssembly2` long-form IVT on the declaring assembly (per
  `feedback_castle_proxy_ivt.md`).
- Integration tests asserting contract seed data: update to read via `IContractModule` /
  `ContractDbContext`.
- Dispatcher tests (rename `AcceptDispatcherTests` → `AcceptanceDispatcherTests`,
  `FinishedDispatcherTests` → `CompletionDispatcherTests`; `SettlementDispatcherTests`
  unchanged) — update to mock `IContractModule.GetByOpportunityAsync` (returning an
  `IContract` with a set `Type`) + the Concert repo's `GetOpportunityIdAsync` +
  `IConcertWorkflowStrategyFactory.Create`, instead of the deleted `IContractStrategyResolver<T>`.
- Payment strategy tests (still in legacy `Concertable.Infrastructure.UnitTests`) updated
  to mock the new per-module factory, not the deleted generic one.

Final pass: remove any dead `using` imports, dead `global usings`, and confirm Web +
Workers both build cleanly and test green.

---

## 5. Next session resume instructions

All decisions from the 2026-04-24 redesign are locked. §3.3 + §3.7 are proposed standard-
pattern calls awaiting a quick formal confirm before §4.

**On resume:**

1. Confirm §3.3 (dedicated `ContractDbContext`) and §3.7 (four projects, no `.Api`) — both
   should be quick yes/no.
2. Begin **Step 0 — Discovery sweep**. Output appends as Appendix A below (to be created).
   Agent-delegable: grep for `IContractRepository`, `IContractStrategyFactory`,
   `IContractStrategyResolver`, `[FromKeyedServices(ContractType.`, `ContractConfigurations`,
   `ContractType.` usages, and `.Include(x => x.Contract)` / `opportunity.Contract.*` access
   sites across Web + Workers + test projects.
3. Confirm `IOpportunityApplicationRepository` / `IBookingRepository` / `IConcertRepository`
   can expose `GetOpportunityIdAsync(int id)` cheaply (likely a 1-column projection or a field
   already present on the entity).
4. Proceed Step 1 → Step 13 in order. Each step should build green before moving on.

`PAYMENT_MODULE_REFACTOR.md` stays paused until this plan is complete through Step 13.
Resume Payment plan at its existing Step 1 (Appendix A there is already filled).

---

## Appendix A — Step 0 Discovery findings (2026-04-24)

Sweep complete. Key findings condensed (full report in conversation history):

### Confirmed locations (post-Concert-extraction state)
- **Entities**: `api/Modules/Concert/Concertable.Concert.Domain/Entities/Contracts/{ContractEntity,FlatFeeContractEntity,DoorSplitContractEntity,VenueHireContractEntity,VersusContractEntity}.cs`
- **Subtype fields**: `FlatFee.Fee` (decimal), `DoorSplit.ArtistDoorPercent` (decimal), `VenueHire.HireFee` (decimal), `Versus.Guarantee + ArtistDoorPercent` (both decimal)
- **OpportunityEntity**: line 11 has `public ContractEntity Contract { get; private set; }`; `Create(int venueId, DateRange period, ContractEntity contract, IEnumerable<int>? genreIds = null)` at line 17. `OpportunityApplicationEntity` has no Contract nav (reaches via `Opportunity.Contract`).
- **EF configs**: `api/Modules/Concert/Concertable.Concert.Infrastructure/Data/Configurations/ContractEntityConfiguration.cs` — single file, base + 4 subtypes. `.UseTptMappingStrategy()` at line 12. FK encoded as `.HasForeignKey<ContractEntity>(c => c.Id)` (PK reuse — splitting per banner second-pass lock).
- **DbSets in `ConcertDbContext`**: `Contracts`, `FlatFeeContracts`, `DoorSplitContracts`, `VersusContracts`, `VenueHireContracts` (lines 22–26).
- **Interfaces** (all in `Concert.Application/Interfaces/`): `IContract`, `IContractStrategy`, `IContractStrategyFactory<T>`, `IContractStrategyResolver<T>`, `IContractRepository` (6 methods), `IConcertWorkflowStrategy`, `IAcceptDispatcher`, `ISettlementDispatcher`, `IFinishedDispatcher`, `IContractService`, `ITicketPaymentStrategy`.
- **DTOs**: `Concert.Application/DTOs/ContractDtos.cs` (single file, 4 records: `FlatFeeContractDto` etc., all with `ContractType ContractType => ContractType.X` expression bodies).
- **Enums**: `Concert.Domain/Enums/{ContractType,PaymentMethod}.cs` (NOT in `Concertable.Core/Enums/` — that legacy location is empty for these enums).
- **Repo impl**: `Concert.Infrastructure/Repositories/ContractRepository.cs` (inherits `IdModuleRepository<ContractEntity, ConcertDbContext>`).
- **Service impl**: `Concert.Infrastructure/Services/ContractService.cs` (injects `IContractRepository` + `IContractMapper`).
- **Controller**: `Concert.Api/Controllers/ContractController.cs` — single endpoint `GET api/contract/opportunity/{opportunityId}`.
- **Strategy impls**: all under `Concert.Infrastructure/Services/Application/{FlatFee,DoorSplit,VenueHire,Versus}ConcertWorkflow.cs`. Each injects `IContractRepository` and calls `GetByApplicationIdAsync<T>` or `GetByConcertIdAsync<T>` with the typed subtype.
- **Dispatchers**: `Concert.Infrastructure/Services/{Accept/AcceptDispatcher,Settlement/SettlementDispatcher,Complete/FinishedDispatcher}.cs`. All three inject `IContractStrategyResolver<IConcertWorkflowStrategy>`. `IFinishedDispatcher.FinishedAsync` returns `Task<Result<IFinishOutcome>>` (the only `Result<T>` wrap among the three; plan §3.1 keeps signatures consistent across the three by NOT propagating the wrap to the renamed `CompletionDispatcher.FinishAsync`).
- **Strategy factory/resolver impls**: legacy `Concertable.Infrastructure/Factories/ContractStrategy{Factory,Resolver}.cs` — to be deleted in Step 4.
- **Payment-side strategies** (legacy):
  - `ITicketPaymentStrategy : IContractStrategy` — `Concert.Application/Interfaces/ITicketPaymentStrategy.cs` (internal). Single method: `PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price) → Task<Result<PaymentResponse>>`.
  - `IStripeValidationStrategy` — `Concertable.Application/Interfaces/Payment/IStripeValidationStrategy.cs` (**public** — asymmetric with `ITicketPaymentStrategy`). Single method: `ValidateAsync() → Task<bool>`. Does NOT inherit `IContractStrategy` today.
  - `TicketPaymentDispatcher` (`Concertable.Infrastructure/Services/Payment/`) injects `IContractStrategyResolver<ITicketPaymentStrategy>`.
  - Stripe validators: `Concertable.Infrastructure/Validators/{StripeAccountValidator,StripeCustomerValidator}.cs`.
- **Keyed registrations**:
  - `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` lines 72–75: 4 × `IConcertWorkflowStrategy` keyed on each `ContractType`.
  - `Web/Extensions/ServiceCollectionExtensions.cs` lines 95–98: 4 × `IStripeValidationStrategy` keyed on each `ContractType`.
- **`.Include(o => o.Contract)`** sites: `Concert.Infrastructure/Repositories/OpportunityRepository.cs` (lines 21, 41) + `OpportunityApplicationRepository.cs` (lines 23, 63). `ConcertRepository` does NOT use `.Include(c => c.Contract)` directly.
- **`.Contract.*` access**: `Concert.Infrastructure/Services/OpportunityService.cs` lines 36, 74, 109 — accesses `request.Contract.ContractType` (DTO field on a request, not entity nav). Workflow strategies don't use the nav (they go through repo).
- **Concert repo OpportunityId hop status**:
  - `OpportunityApplicationEntity.OpportunityId` — direct field. Add `GetOpportunityIdAsync(int)` projection for dispatcher symmetry.
  - `ConcertBookingEntity` (the concrete name) — no direct field; nav path `booking.Application.OpportunityId`. Add projection.
  - `ConcertEntity` — no direct field; nav path `concert.Booking.Application.OpportunityId`. Add projection.
- **Tests touching Contract**: 6 mapper tests, 3 dispatcher tests, 4 application-service-complete tests (per contract type), `ContractServiceTests`, `TicketPaymentDispatcherTests`, `ContractStrategy{Factory,Resolver}Tests`, two entity tests (`DoorSplit`, `Versus`), and integration tests at `Web.IntegrationTests/Controllers/Opportunity/`. `Concert.Application/AssemblyInfo.cs` already grants IVT to all relevant test assemblies + `DynamicProxyGenAssembly2` + `Concertable.Web` + `Concertable.Workers` + `Concertable.Infrastructure`.

### Surprises (resolved 2026-04-24)
1. **TPT, not TPH** — banner second-pass lock; configs already correct.
2. **Repo + configs already in Concert.Infrastructure** — Step 6 / Step 5 source paths corrected.
3. **Enums in Concert.Domain, not Concertable.Core** — Step 2 source path corrected.
4. **PK-as-FK on `ContractEntity.Id`** — splitting (banner second-pass lock). Adds explicit `OpportunityId` column.
5. **`IContractMapper : IContractStrategy`** — vestigial inheritance. Drop the marker base when relocating the mapper to `Contract.Application` in Step 7 (cleanup-when-touched).
6. **`IStripeValidationStrategy` is `public`** + does NOT inherit `IContractStrategy` — asymmetry with `ITicketPaymentStrategy`. Address during Payment extraction; not this plan's scope (Payment-side legacy adoption of per-module factory in Step 9 still works because keyed registration is what matters at the DI level — the marker is only a compile-time hint).
7. **DTO suffix `Dto`** on the four contract DTO records — drop on the move (Step 3) so the public wire shape names match `IContract`.
8. **`IContract.ContractType`** (existing) vs `IContract.Type` (proposed in plan §3.1/§3.4) — keep the existing `ContractType` property name; factory call is `factory.Create(contract.ContractType)`.

---
