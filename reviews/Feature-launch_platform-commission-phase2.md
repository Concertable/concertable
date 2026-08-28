# Code review — Feature/launch_platform-commission-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** complete
**Reviewed up to commit:** pending the incremental commit — see the incremental pass below _(2026-08-28)_
**Judgment:** approved

## Review pass — 2026-08-28 — full

**Candidate base:** `10a1aa0bb5ab083ab8d01f423af50961708036f2`
**Candidate head:** `bfd51184859520f081e9c62a5f64c9c73dd39ff7`
**Candidate branch:** `Feature/launch_platform-commission-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `51 paths` (`api/Concertable.B2B/src/Modules/Concert` + `app/web/b2b/shared/src/features/concerts/types.ts` + the two `plans/launch` docs)
**Work-order path:** `reviews/Feature-launch_platform-commission-phase2.md`
**Work-order mode:** `new`
**Pass judgment:** approved

Two commits — Phase 2 steps 1–2: keyed pure gross calculators (`c68878297`), then the revenue-share
settlement aggregate + commission binding (`bfd511848`). Lenses run: native/general (correctness,
reuse, simplification, efficiency, error handling), persistence + multitenancy ownership, module
boundaries, keyed-strategies conventions, C# style/naming, result-carriers/errors, changed-behaviour
test impact, TypeScript style. Routed skills re-checked against the frozen diff.

Most of this candidate was reviewed interactively with Tommy during authoring — that resolved the
design questions (nullable-on-`ConcertEntity` → extracted `RevenueShareSettlementEntity`; `CanDeclare`
+ two coupled response nullables → the `ISettlementDeclaration` union; two `ReviewedGross*` columns →
the `SettlementReview` value object; 3 correlated subqueries → 1; dead `IConcertReadDbContext`
member removed; `DeferredBooking` guard on `Declare`). This pass is the independent re-check.

### Findings

- [x] **TEST1 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/Mappers/RevenueShareSettlementMapper.cs:20`
  The settlement-view mappers are the core new read-model logic and were only partially covered.
  `ConcertDealStrategyFactoryTests` verifies *which* leaf resolves per `DealType`, and
  `ConcertDoorRevenueApiTests` exercises `Undeclared → Declared` end-to-end, but nothing asserted that
  `RevenueShareSettlementMapper.ToSettlement` maps a row to the right `ISettlementDeclaration` case —
  in particular `Reviewed` (unreachable from integration until `FreezeReviewedGross` is wired) and the
  `Undeclared.WindowOpen` computation — nor that `FixedSettlementMapper` returns `FixedSettlement` with
  the gross from `ISettlementGrossCalculator`.
  Fixed: added `SettlementMapperTests` — `FixedSettlementMapper` gross for FlatFee/VenueHire, and
  `RevenueShareSettlementMapper` for every declaration state (`Undeclared` window open/closed,
  `Declared`, `Reviewed`) including the frozen-review passthrough and the ticket-sales minor units.

- [x] **DOC1 — LOW — multitenancy roster** — `api/Concertable.B2B/CODE_PATTERNS.md:31`
  `RevenueShareSettlements` is a new `IVenueArtistTenantScoped` entity deliberately left **unfiltered**
  (the completion sweep — `GetTotalRevenueByConcertIdAsync`, `DoorRevenueOutstandingSpecification` —
  reads it across tenants host-side, like `Concert`; writes are interceptor-guarded). The
  `multitenancy` skill routes reviewers to this roster's "Which entities are filtered" section, which
  did not list it.
  Fixed: added `RevenueShareSettlement` to the unfiltered-by-design bullet with its reason, and noted
  `SettlementGrossCalculator` / `SettlementMapper` as new keyed facades in the strategy-families list.

## Checked and clean

- **Multitenancy** — `RevenueShareSettlementEntity` carries the owner ids and is interceptor-write-
  guarded but not query-filtered; every direct read (`GetByConcertIdAsync`) sits behind the
  `VenueForbidden` guard in `DeclareDoorRevenueAsync`, and the manager-details path reaches it only
  through the already-Booking-scoped concert query. Matches the `ConcertEntity` stance.
- **Persistence** — `RevenueShareSettlementRepository` binds the module `Repository<T>` alias (not a
  tenant-scoped base), matching `ConcertRepository`. `OwnsOne(e => e.Review)` → nullable
  `Review_GrossMinor` / `Review_ReviewedAtUtc`, null together. `InsertAsync` persists on its own
  (codebase convention, cf. `BookingService`), so first-declaration saves without an explicit
  `SaveChangesAsync`; `Redeclare` mutates a tracked entity and calls `SaveChangesAsync`. Schema/table
  name is a `Schema.Tables` constant. Migration re-scaffolded, not additive.
- **Keyed strategies** — `ISettlementMapper` / `ISettlementGrossCalculator` follow the established
  shape: module-local `IConcertDealStrategyFactory<T>`, named facade, per-`DealType` leaves declared
  vertically, `RequireAll<T>()`. No `DealType` branch leaked into `ConcertService` or the mappers.
  Lifetime widening (`Singleton → Scoped`) on the settlement leaves is consistent and validated by the
  `ValidateScopes = true` factory tests.
- **Module boundaries** — `ISettlement` / `ISettlementDeclaration` are `internal` responses in
  `Concert.Application/Responses/` beside the sibling `IPaymentAmount`. No Result/Option in the HTTP
  DTOs. `MyDetailsResponse` is the existing owner `Response` type; adding `Settlement` is in-bounds.
- **Correctness** — the `row switch` in `RevenueShareSettlementMapper` is compiler-exhaustive over the
  three real states with no catch-all or null-forgiving. `BindCommission` rejects rebinding to a
  different id. `DeclareDoorRevenueAsync` keeps all five pre-existing guards. `DoorRevenueOutstanding`
  negation semantics preserved (`ended && booked && (not-deferred || has-settlement)`).
- **C# style / naming** — private fields unprefixed, `this.`-qualified; `SettlementReview` is a
  positional value-object record in `Domain/ValueObjects/` matching `ESignature` / `InvoiceParty`; new
  Api mapper uses a C# 14 `extension()` block.
- **TypeScript** — the removed `MyConcert.doorRevenue` field was declared but never rendered; the
  declare action link and POST request flow are unchanged. The `settlement` union type is deferred to
  step 5, which renders it.
- **Efficiency note (not a finding)** — `ConcertService.GetDetailsAsync` /
  `GetDetailsByApplicationIdAsync` add one cross-module `dealResolver.ResolveByConcertIdAsync` call
  (third round trip) per manager concert-details load. Needed by both settlement-view branches, the
  page is not hot, and there is no clean denormalization; accepted.

## Incremental verification — 2026-08-28

`1cd46f0a8` closes TEST1 and DOC1 — test and documentation only, no runtime change. 274 Concert unit
tests green (+8 in `SettlementMapperTests`). No new findings.

## Review pass — 2026-08-28 — incremental (author + Tommy)

**Candidate head:** post-`6ae3c5797` working tree
**Pass judgment:** approved

Tommy raised five further issues on the settlement model; all fixed in the same session:

- [x] **PLACE1 — LOW — placement** — `ManagerConcertDetailsProjection` / `RevenueShareSettlementRowProjection`
  were in `Application/DTOs/ConcertDtos.cs`. Projections aren't DTOs (`csharp-naming`). Moved to
  `Application/Projections/ManagerConcertDetailsProjection.cs`, matching the existing
  `OpportunityApplicationProjection` precedent.
- [x] **PLACE2 — LOW — placement** — `QueryableSettlementMappers.ToManagerDetails` is `ToDetails` plus a
  join, a concert-details query mapper. Folded into `QueryableConcertMappers`; `QueryableSettlementMappers.cs`
  deleted.
- [x] **CONV1 — MEDIUM — exception convention** — Concert domain entities threw `InvalidOperationException`
  for invariant breaches; the rest of B2B (Admin/Artist/Venue/Tenant/Conversations) throws
  `Concertable.Kernel.DomainException`. Converted all 11 sites (`ApplicationEntity`, `BookingEntity`,
  `ConcertEntity`, `ContractEntity`, `InvoiceEntity`, `RevenueShareSettlementEntity`) plus
  `RevenueShareSettlementAmount`'s settlement guard. Rule added to `ARCHITECTURE.md`; enforced by
  `DomainInvariantExceptionTests` (source scan — currently zero offenders).
- [x] **API1 — LOW — sentinel** — `FlatFee`/`VenueHire` gross callers passed `Money.Zero(Currency.Gbp)`
  as a "no takings" sentinel, reading as a real zero. `ISettlementGrossCalculator.CalculateGross` now
  takes `Money? eligibleTakings = null`; fixed-fee callers omit it, and `RevenueShare(...)` throws
  `DomainException` if a revenue-share formula is called without takings.
- [x] **API2 — LOW — coupling** — `ISettlementMapper.ToSettlement` took the whole
  `ManagerConcertDetailsProjection` (a query shape). Now takes `(DealDto, ConcertDetails,
  RevenueShareSettlementRowProjection?, DateTime)` — the pieces it reads.

Verified: 276 Concert unit tests green (+2); `Concert.Api` / `Concert.IntegrationTests` / `B2B.AppHost`
build clean.

## CI-only validation (recorded, not a finding)

The Concert integration/E2E suite cannot run from this worktree (`Microsoft.Data.SqlClient.SNI`
`0x800700CE` — Windows MAX_PATH on the `.worktrees/…` path). The merge queue must confirm:
`QueryableSettlementMappers.ToManagerDetails` EF translation (nested `ConcertDetails` projection +
correlated `RevenueShareSettlements` subquery), the `OwnsOne` projection (`s.Review` inside `.Select`),
and `GetTotalRevenueByConcertIdAsync`'s revised subquery. 266 (pre-fix) Concert unit tests + Concert.Api
/ Concert.IntegrationTests / B2B.AppHost builds green locally (0 warnings).
