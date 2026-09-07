# Deal vocabulary split and DI-mapper collapse progress

- Plan: `plans/layering/DEAL_VOCABULARY_AND_MAPPER_COLLAPSE_PLAN.md`
- Roadmap: `plans/layering/LAYERING_ROADMAP.md`
- Roadmap item: `layering/deal-vocabulary-and-mapper-collapse`
- Worktree: none yet
- Branch: none yet — authored from `Docs/launch_seal-and-postgres-plans`, which does **not** own this work
- PR: none yet
- Dependency/package gates: `Riok.Mapperly` needs pinning in `api/Concertable.B2B/Directory.Packages.props`
  (Phase 2) and `api/Concertable.Payment/Directory.Packages.props` (Phase 4). It is already pinned at 4.3.1 and
  unused on `Chore/TestTierNaming`, `refactor/launch_operation-claims-and-attempts` and
  `refactor/launch_deal-lifecycle-modules-phase2` — reconcile rather than adding a second pin.
- Last reconciled: 2026-09-07 against a read-only survey of `main`-line source in the normal checkout

## Current state

Design complete, nothing implemented. No branch or worktree exists for this work.

## Decisions and discoveries that affect execution

- **`Deal.Domain` uses only two symbols from `Deal.Contracts`** — `DealType` and `PaymentMethod`, verified by
  grepping every usage in the project. No DTO usage exists today. The defect is that the assembly-wide
  reference *permits* it, not that it is currently exploited.
- **`Deal.Contracts` is not packable** (B2B `Directory.Build.props:39`), so the enums do not need duplicating
  into a domain copy and a wire copy. This was the expensive-looking option and it is not required.
- **`Concertable.Contracts` is the wrong home for now.** It already holds `Genre` and is conceptually right,
  but it is a published package and `plans/AGENTS.md` forbids a published-contract change landing in one PR.
- **Mapperly `MemberVisibility.All` is a dead end here** and is used nowhere in the plan. It bypasses the
  validating factory exactly as a public setter would, and riok/mapperly#1458 (private members across
  compilation units) is fixed only by #2139, merged 2026-02-04 and absent from the latest stable 4.3.1
  (published 2025-12-22). The `entity → DTO` direction never needed it.
- **`Apply` as an abstract member on `DealEntity` was designed and rejected.** It is the nicer dispatch and
  gives compiler-enforced exhaustiveness, but it only compiles because of the very `Domain → Contracts`
  reference Phase 1 deletes. Do not re-propose it after Phase 1 lands.
- **`DealService.Validate` is construct-and-discard and is load-bearing** —
  `OpportunityService.cs:183` reaches it through `DealModule.Validate`. The plan renames rather than removes
  it; keeping the guards in the entities keeps that cross-module path out of the blast radius.

## Reviews

None yet — no implementation exists.

## Next Steps

1. Create the worktree and branch from the current remote default:
   `./scripts/worktrees.ps1` per the `open-worktree` skill, branch `Refactor/layering_deal-vocabulary-and-mapper-collapse`.
2. Execute Phase 1 only: add `api/Concertable.B2B/src/Concertable.B2B.Vocabulary`, move `DealType` and
   `PaymentMethod` into it, repoint `Deal.Domain` and `Deal.Contracts`, fix every consuming `using`, and add the
   `*.Domain` → `*.Contracts` architecture test with the 11 remaining modules allowlisted.
3. Gate Phase 1 on a full `api/Concertable.slnx` build plus `Concertable.B2B.Deal.UnitTests`, then commit before
   starting Phase 2.

Phase 2 depends on nothing outside Phase 1 and may follow in the same branch.
