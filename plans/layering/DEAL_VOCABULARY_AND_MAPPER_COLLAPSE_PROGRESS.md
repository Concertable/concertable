# Deal vocabulary split and DI-mapper collapse progress

- Plan: `plans/layering/DEAL_VOCABULARY_AND_MAPPER_COLLAPSE_PLAN.md`
- Roadmap: `plans/layering/LAYERING_ROADMAP.md`
- Roadmap item: `layering/deal-vocabulary-and-mapper-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-layering_deal-vocabulary-and-mapper-collapse`
- Branch: `Refactor/layering_deal-vocabulary-and-mapper-collapse`, based on `origin/main` @ `15ce7946f`
- PR: none yet
- Dependency/package gates: `Riok.Mapperly` needs pinning in `api/Concertable.B2B/Directory.Packages.props`
  (Phase 2) and `api/Concertable.Payment/Directory.Packages.props` (Phase 3). It is already pinned at 4.3.1 and
  unused on `Chore/TestTierNaming` and sibling branches — reconcile rather than adding a second pin.
- Last reconciled: 2026-09-07 against `origin/main` @ `15ce7946f` in this worktree

## Current state

**Phase 1 is complete and committed** (`3d4fa7ab4`). Phase 2 is the next action.

The plan was originally drafted against `Docs/launch_seal-and-postgres-plans`, whose Concert module predates
the Application/Booking/Opportunity split. Every file path and family roster in the plan has since been
re-verified against `origin/main` @ `15ce7946f`.

## Completed milestones

- **Phase 1** (`3d4fa7ab4`) — `Concertable.B2B.Vocabulary` added and registered in `api/Concertable.slnx`;
  `DealType`, `DealTypeNames` and `PaymentMethod` moved into it under namespace `Concertable.B2B.Vocabulary`;
  `Deal.Domain` repointed off `Deal.Contracts`; 51 files renamed off the old namespace;
  `LayeringArchitectureTests` added asserting the Domain→Contracts violations are exactly the ten modules
  still pending.

## Latest verification

- `Concertable.B2B.Deal.UnitTests` builds clean and passes **47/47**, which transitively proves
  Deal.Contracts, Deal.Domain, Deal.Application and Deal.Infrastructure compile.
- **`LayeringArchitectureTests` has never been executed.** `Concertable.B2B.ArchitectureTests` references the
  composition root, which does not build — see the blocker below. PR CI is its first real run; expect to fix
  the pending list there if it is wrong.

## Blocker on the full-solution gate (pre-existing, not caused by this branch)

`api/Concertable.slnx` does not build. `Concertable.B2B.Application.Infrastructure/Services/ApplicationCheckoutService.cs:112`
calls `IEscrowOperationsClient.AuthorizeAsync`, which the pinned `Concertable.Payment.Client` package does not
expose. **Verified pre-existing:** it reproduces at `origin/main` @ `15ce7946f` with this branch's changes
stashed. This is the recorded debt that PR CI never builds against the pinned packages (`1d11deec6`).

Consequence for this plan: the phase gates fall back to the smallest affected project plus its unit tests, and
anything requiring the composition root — the architecture suite included — is gated on PR CI rather than
locally.

## Decisions and discoveries that affect execution

- **Two of the original targets no longer exist on main.** `IPaymentAmountMapper`, `IDealTerms`,
  `IDealTermsRenderer` and `IDealTermsSerializer` are gone. The old Phase 3 was deleted; scope dropped from
  ~24 files to 17.
- **`DealTerms` (`Deal.Contracts/DealTerms.cs`) is the reference shape** — an abstract record whose four arms
  each override `Render()`, replacing a keyed family. Match it where an arm is a pure per-arm computation.
- **`Deal.Domain` uses only two symbols from `Deal.Contracts`** — `DealType` and `PaymentMethod`, namespace
  `Concertable.B2B.Deal.Contracts.Enums`. No DTO usage exists. The defect is that the assembly-wide reference
  *permits* it, not that it is currently exploited.
- **`Deal.Contracts` is not packable** (B2B `Directory.Build.props:39`), so the enums need no domain/wire
  duplication. This was the expensive-looking option and it is not required.
- **`Concertable.Contracts` is the wrong home for now.** It already holds `Genre` and is conceptually right,
  but it is a published package and `plans/AGENTS.md` forbids a published-contract change landing in one PR.
- **Mapperly `MemberVisibility.All` is a dead end here** and is used nowhere in the plan. It bypasses the
  validating factory exactly as a public setter would, and riok/mapperly#1458 (private members across
  compilation units) is fixed only by #2139, merged 2026-02-04 and absent from the latest stable 4.3.1
  (published 2025-12-22). The `entity → DTO` direction never needed it.
- **Nothing in this plan is waiting on a Mapperly release.** The rejection above is a design decision, not an
  availability one: a Mapperly 5 shipping `MemberVisibility.All` across assemblies would still not be used,
  because after Phase 1 `Deal.Domain` cannot reference `DealDto` at all. Do not revisit this plan when 5.0
  lands. The only deferred choice is the vocabulary's *location*, gated on the published-package rule and
  tracked as `layering/vocabulary-to-shared-contracts`.
- **`Apply` as an abstract member on `DealEntity` was designed and rejected.** `DealTerms.Render()` proves the
  shape works in this codebase, but `DealTerms` lives in Contracts and may see a DTO; `DealEntity` lives in
  Domain and, after Phase 1, may not. Do not re-propose it.
- **The surviving keyed families are not targets.** `IContractFactory` (Booking.Domain),
  `IDealPayeeResolver` and `ISettlementAmountResolver` (Concert.Application) carry real behaviour.

## Reviews

None yet — no implementation exists.

## Next Steps

Execute Phase 2 in this worktree:

1. Pin `Riok.Mapperly` in `api/Concertable.B2B/Directory.Packages.props` at the version
   `Chore/TestTierNaming` already uses (4.3.1), and add the `PackageReference` to
   `Concertable.B2B.Deal.Application`.
2. Add `DealMappers` (static, C# 14 extension members, `[Mapper]` with no visibility overrides,
   `[MapDerivedType]` over the four arms) covering `DealEntity → DealDto` only.
3. Add `DealFactory` with `Create(DealDto)` and `Apply(DealEntity, DealDto)` switches over
   `XDealEntity.Create`/`.Update`, the tuple pattern's default arm carrying the deal-type mismatch error that
   `DealUpdater.Apply` currently hand-writes.
4. Delete `IDealMapper`, `DealMapper` and the four arms; `IDealUpdater`, `DealUpdater` and the four arms.
   Repoint `DealService` (`Validate`, `CreateAsync`, `UpdateAsync`, `FindByIdAsync`, `GetByIdsAsync`).
5. Remove both `RequireAll` rows and the keyed registrations in
   `Deal.Infrastructure/Extensions/ServiceCollectionExtensions.cs:47-66`, then fix
   `DealStrategyArchitectureTests` and `DealStrategyFactoryTests` where they name deleted types.
6. Add the exhaustiveness test asserting both switches carry an arm per `DealType`.
7. Gate on `Concertable.B2B.Deal.UnitTests` building and passing. The full-solution build cannot be a gate
   until the `AuthorizeAsync` package break above clears.
