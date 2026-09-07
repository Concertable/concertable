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

Plan authored and reconciled against real `main`. No implementation yet. Phase 1 is the next action.

The plan was originally drafted against `Docs/launch_seal-and-postgres-plans`, whose Concert module predates
the Application/Booking/Opportunity split. Every file path and family roster in the plan has since been
re-verified against `origin/main` @ `15ce7946f`.

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

Execute Phase 1 in this worktree:

1. Add `api/Concertable.B2B/src/Concertable.B2B.Vocabulary` (`net10.0`, no package references) and register it
   in `api/Concertable.slnx`.
2. Move `DealType`, `DealTypeNames` and `PaymentMethod` from
   `Modules/Deal/Concertable.B2B.Deal.Contracts/Enums/` into it under namespace `Concertable.B2B.Vocabulary`.
3. Repoint `Deal.Domain` (drop `Deal.Contracts`, add `Vocabulary`) and `Deal.Contracts` (add `Vocabulary`), then
   fix the `global using Concertable.B2B.Deal.Contracts.Enums;` line in each of the 20 `GlobalUsings.cs` files
   that carry it, plus any file-scoped `using` of that namespace.
4. Add the `*.Domain` → `*.Contracts` architecture test with the remaining B2B modules allowlisted.
5. Gate on a full `api/Concertable.slnx` build plus `Concertable.B2B.Deal.UnitTests`, then commit before
   starting Phase 2.
