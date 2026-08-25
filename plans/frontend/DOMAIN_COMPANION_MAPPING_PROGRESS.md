# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: not opened
- Dependency/package gates: platform-sync PR #780 is red because Payment still implements the old `IAuditable` timestamp types; its existing worktree has concurrent uncommitted consumer edits
- Last reconciled: 2026-08-25 against `origin/main` at `ac7ff7f17021a2aaf163171798cde6fff4c7a897`; the full frontend matrix last passed at integration head `588c4e36b24c3bedfa8a07d51af01d99ff8fbc1f`

## Current state

Phases 0 through 4 are complete. The Phase 5 source inventory and review fixes are complete, the
branch is current with `origin/main`, and no partial code changes remain. The companion/request
migrations, direct typed labels, RHF write boundaries, private editor/auth/search state, owned-absence
normalization, consent service, transport encoders, adapters, and presentation projections are at
their planned boundaries.

The full branch review through `4b69e66ce` found two remaining boundary-projection convention misses.
Both were fixed in `539c1a520`; its incremental review was clean. Every Phase 5 invariant and the
complete post-merge frontend matrix now pass. The post-`539c1a520` incremental review found only ledger
corrections, which are resolved. Platform-sync PR #780 blocks readiness and merge, not draft PR creation.

## Next Steps

Push the stable candidate and open its draft PR for exact-head CI. Keep it draft until platform-sync PR #780's
Payment `IAuditable` consumer failure is resolved without colliding with its existing uncommitted worktree
changes, #780 lands, and current `origin/main` is reconciled.

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
- Merged `origin/main` at `ac7ff7f17` without conflicts.

## Verification

- GitHub reports PRs #595, #600, and #637 merged; platform-sync PR #780 is red and no implementation
  PR exists. Its build fails with four `CS0738` errors because Payment's `EscrowEntity` and
  `TransactionEntity` expose `DateTime` audit properties while `IAuditable` now requires
  `DateTimeOffset`.
- Current baseline: `origin/main` at `ac7ff7f17021a2aaf163171798cde6fff4c7a897`, merged without conflicts.
- Tests passed: `@concertable/b2b` 5 files / 15 tests; `@concertable/shared` 10 / 23;
  `@concertable/web` 5 / 31; `@concertable/customer` 3 / 3 through its build preflight.
- B2B, shared, customer, all five web SPA, and both mobile TypeScript builds passed.
- Dependency-cruiser reported no violations; all 7 carve/boundary tests passed.
- B2B and Customer Android exports passed with 3,691 and 4,283 modules respectively.
- Phase 5 mapper/buffer/store/absence searches passed with only the three intended binary
  `ArrayBuffer` sites and private search-store facade/test sites allowlisted.
- `git diff --check` and `python .agents/hooks/plan_graph.py --root <worktree>` passed.

## Reviews

Full implementation review covered `70af43a..4b69e66` in
`reviews/Refactor-frontend_domain-companion-mapping.md`. CV1 and CV2 were fixed in `539c1a520`; NAT1 through
NAT3 corrected delivery-checkpoint wording. Incremental review through this commit is clean with no open
findings.

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
