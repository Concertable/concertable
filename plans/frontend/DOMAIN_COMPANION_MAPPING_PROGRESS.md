# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: not opened
- Dependency/package gates: none; PRs #595, #600, and #637 are merged and no open platform-sync PR exists
- Last reconciled: 2026-08-24 after merging `origin/main` at `acf729372e46fc8a03f706a77f8e68931a899efd` into branch head `ce15ef0758a8d69fe788b9bd26d47b60afc7a8fe`

## Current state

Phases 0 through 4 are complete. The Phase 5 source inventory and review fixes are complete, the
branch is current with `origin/main`, and no partial code changes remain. The companion/request
migrations, direct typed labels, RHF write boundaries, private editor/auth/search state, owned-absence
normalization, consent service, transport encoders, adapters, and presentation projections are at
their planned boundaries.

The full branch review through `4b69e66ce` found two remaining boundary-projection convention misses.
Both were fixed in `539c1a520`; focused tests/builds passed and the incremental review was clean. The
post-merge plan checkpoint and final exact-head verification/review remain before PR delivery.

## Next Steps

1. Run the Phase 4 invariant searches and the complete frontend verification matrix on the merged head.
2. Incrementally review every commit after `539c1a520` and leave no open findings.
3. Push the stable candidate, open its PR, and require exact-head CI before merge readiness.

## Completed work

- Researched TypeScript/React mapping approaches and rejected a runtime library for this codebase.
- Selected the interface-plus-same-name-companion pattern with source-owned `toX` operations.
- Classified the current frontend transformations and specified an exact disposition for every
  retained or migrated site.
- Created the roadmap item, implementation plan, and operational ledger in `d09f09f23`.
- Resolved all five docs-review findings in `18fec1752` and `959dbb516`.
- Cleared the dependency gate and completed Phases 1 through 4 in `e9a8fe7c9`, `a5ba13de2`,
  `70bfc8ac3`, and `cce96a5e7`.
- Closed the remaining inventory and review fixes through `539c1a520`.
- Merged current `origin/main` without conflicts in `ce15ef075`.

## Verification

- GitHub reports PRs #595, #600, and #637 merged; no open platform-sync PR or implementation PR exists.
- Current baseline: `origin/main` at `acf729372e46fc8a03f706a77f8e68931a899efd`, merged without conflicts.
- `@concertable/shared`: 10 test files / 23 tests passed; build passed at `539c1a520`.
- `@concertable/web`: 5 test files / 31 tests passed; build passed at `539c1a520`.
- `python .agents/hooks/plan_graph.py --root <worktree>`: 0 errors, 0 warnings before this checkpoint.
- The complete post-merge Phase 4 matrix has not yet run.

## Reviews

Full implementation review covered `70af43a..4b69e66` in
`reviews/Refactor-frontend_domain-companion-mapping.md`. CV1 and CV2 were fixed in `539c1a520`; the
incremental review through that commit found no new issues. The merge/checkpoint delta still needs an
incremental pass before delivery.

## Decisions, discoveries, blockers, and deviations

- `Opportunity.toRequest`, not `OpportunityRequest.from`, is the canonical spelling.
- Companions stay in feature `types.ts` for this migration. No threshold or speculative folder split
  is left for the implementer to decide.
- Zod remains the only added behaviour at form boundaries and is already installed; no new dependency
  is planned.
- The inventory includes implicit mappings where a read type is reused as a write body, even if no
  function is currently named mapper.
- Backend mappers are excluded.
- Transport encoders remain API-private; third-party adapters and presentation projections remain
  boundary-local rather than becoming domain companions.
- Current `write-boundary` guidance requires RHF/Zod to produce the request directly; the superseded
  parse-then-buffer-mapper shape was not restored.
- Local E2E is not part of this refactor's pre-PR gate.
