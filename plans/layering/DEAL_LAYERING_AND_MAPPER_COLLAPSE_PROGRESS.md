# Deal layering and DI-mapper collapse progress

- Plan: `plans/layering/DEAL_LAYERING_AND_MAPPER_COLLAPSE_PLAN.md`
- Roadmap: `plans/layering/LAYERING_ROADMAP.md`
- Roadmap item: `layering/deal-layering-and-mapper-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-layering_deal-vocabulary-and-mapper-collapse`
- Branch: `Refactor/layering_deal-vocabulary-and-mapper-collapse`, based on `origin/main` @ `15ce7946f`
  (the branch and worktree keep the original name; renaming them is blocked while commits are unpushed, and
  the PR title is what matters)
- PR: none yet
- Dependency/package gates: `Riok.Mapperly` 4.3.1 was already pinned in `api/Concertable.B2B/Directory.Packages.props`
  and is now referenced by `Deal.Application`. Phase 3 needs the same pin in
  `api/Concertable.Payment/Directory.Packages.props`.
- Last reconciled: 2026-09-07 against `origin/main` @ `15ce7946f` in this worktree

## Current state

**Phase 1 complete. Phase 2 half done** — the `IDealMapper` family is gone; `IDealUpdater` is untouched
pending a design decision (see Next Steps).

The plan was originally drafted against `Docs/launch_seal-and-postgres-plans`, whose Concert module predates
the Application/Booking/Opportunity split. Every file path and family roster in the plan has since been
re-verified against `origin/main` @ `15ce7946f`.

## Completed milestones

- **Phase 1** — `DealType` moved into `Concertable.B2B.Deal.Domain`;
  `PaymentMethod` moved into the new service-local `Concertable.B2B.Enums`, registered in
  `api/Concertable.slnx`; `Deal.Domain` repointed off `Deal.Contracts` onto `Concertable.B2B.Enums`;
  `Deal.Contracts` takes a reference on `Deal.Domain` for `DealType`; 51 files repointed off the old
  namespace; `LayeringArchitectureTests` added asserting the Domain→Contracts violations are exactly the ten
  modules still pending.

  The first cut of this (`3d4fa7ab4`) put both enums in one service-level `Concertable.B2B.Vocabulary`
  project. Rejected: `DealType` is Deal's own domain vocabulary and belongs in `Deal.Domain`, and the name
  was not carrying its weight. Do not reintroduce a single shared home for both enums.

- **Phase 2a** — `IDealMapper`, the `DealMapper` facade and its four arms deleted (6 files). Replaced by
  `Mappers/DealMapper.cs` (Mapperly `[Mapper]`, `[MapDerivedType]` over the four pairs, generating `ToDto`
  and `ToDtos`). `ToEntity` is an abstract member on `DealDto` overridden one line per arm, so the DTO→entity
  switch is gone too and no extension class survives.
  `DealService` drops the `IDealMapper` dependency; the keyed `IDealMapper` registrations and the matching
  rows in `DealStrategyArchitectureTests` and `DealStrategyFactoryTests` are gone. New `DealMapperTests`
  covers both directions per `DealType`, which is also the exhaustiveness guard the `RequireAll` row used
  to provide.

  `Riok.Mapperly` was already pinned at 4.3.1 in `api/Concertable.B2B/Directory.Packages.props`; only the
  `PackageReference` on `Deal.Application` was needed. `[MapperIgnoreSource(nameof(DealEntity.TenantId))]`
  declares the one deliberate source-only member — tenancy is ambient and absent from the DTO.

## Latest verification

- `Concertable.B2B.Deal.UnitTests` builds clean and passes **58/58**, which transitively proves
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
  tracked as `layering/enums-to-shared-contracts`.
- **`Apply` as an abstract member on `DealEntity` was designed and rejected.** `DealTerms.Render()` proves the
  shape works in this codebase, but `DealTerms` lives in Contracts and may see a DTO; `DealEntity` lives in
  Domain and, after Phase 1, may not. Do not re-propose it.
- **The surviving keyed families are not targets.** `IContractFactory` (Booking.Domain),
  `IDealPayeeResolver` and `ISettlementAmountResolver` (Concert.Application) carry real behaviour.

## Reviews

None yet.

## Next Steps

**Blocked on a design decision, not on work.** `IDealUpdater` is the only remaining Deal family: the
interface in `Deal.Application/Interfaces`, the `DealUpdater` facade and four arms in
`Deal.Infrastructure/Services/Updaters`, plus the keyed registrations and `RequireAll<IDealUpdater>()`.

The straight translation is a tuple-pattern switch over `(existing, deal)` calling `entity.Update(...)`,
which Tommy has flagged as too verbose given both sides always vary together. Agree the shape before
writing it; the mechanical work behind it is under an hour.

Once that is settled: delete the six `IDealUpdater` files, repoint `DealService.UpdateAsync`, drop the
keyed registrations and the remaining `DealStrategyFactoryTests`/`DealStrategyArchitectureTests` rows,
then gate on `Concertable.B2B.Deal.UnitTests`.

Phase 3 (Payment `ITransactionMapper`, B2B `IUserMapper`) is independent and can proceed in parallel.
