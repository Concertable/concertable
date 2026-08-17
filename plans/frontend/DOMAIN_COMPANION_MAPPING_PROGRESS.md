# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: not created
- Branch: `Refactor/frontend_domain-companion-mapping` (reserved; not created)
- Plan worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-frontend-domain-companion-plan`
- Plan branch: `Docs/frontend_domain-companion-plan`
- Plan PR: not opened
- Dependency/package gates: PRs #595, #600, and #637 must be terminal before the implementation worktree is created; no package publication gate
- Last reconciled: 2026-08-17 against `origin/main` at `9205e82df4359df8ddf8dfdace07b4aa09b6d186`, open PR inventory, and registered worktrees

## Current state

No implementation exists. The repository currently has no frontend `Mapper` objects or `toX`
companions. The canonical problem is the anonymous Opportunity request projection, but the refreshed
inventory also found implicit read/write conversions in Organization, Review, Preference, Concert,
Artist, and Venue flows.

The design is resolved: retain interfaces, use a same-name exported `const` companion directly below
the owning types, use source-owned `toX` names, validate raw form buffers before mapping, and add no
mapping dependency. Transport encoding, third-party adaptation, presentation projection, exhaustive
registries, identity mappings, and trivial request bodies remain outside companions.

## Next Steps

Blocked: Open PRs #595, #600, and #637 overlap the target concert/messaging types or frontend guidance.
Blocked by: GitHub PRs #595, #600, and #637, owned by their current branches.
Unblock action: Let each PR reach merged or closed state; then fetch `origin/main`, confirm no open red platform-sync PR, refresh the plan's baseline inventory, and create `Refactor/frontend_domain-companion-mapping` from current `origin/main`.
Resume when: `gh pr view 595`, `600`, and `637` each report `MERGED` or `CLOSED`, the refreshed main worktree is clean for the named target paths, and the progress ledger records the new main SHA and any changed paths.

## Completed work

- Researched TypeScript/React mapping approaches and rejected a runtime library for this codebase.
- Selected the interface-plus-same-name-companion pattern with source-owned `toX` operations.
- Classified the current frontend transformations and specified an exact disposition for every
  retained or migrated site.
- Created the roadmap item, implementation plan, and operational ledger.

## Verification

- Baseline source: `origin/main` at `9205e82df4359df8ddf8dfdace07b4aa09b6d186`.
- Open-PR overlap checked for #595, #600, #617, #633, and #637; only #595, #600, and #637 gate the
  frontend implementation.
- Registered worktrees checked. `Refactor/OrganizationProfileRouteContraction` has committed
  Artist/Venue changes without an open PR and is explicitly not the implementation base.
- No implementation tests apply yet.

## Reviews

The plan artifacts require a docs review before their plan PR is merged. No implementation review
exists.

## Decisions, discoveries, blockers, and deviations

- `Opportunity.toRequest`, not `OpportunityRequest.from`, is the canonical spelling.
- Companions stay in feature `types.ts` for this migration. No threshold or speculative folder split
  is left for the implementer to decide.
- Zod remains the only added behaviour at form boundaries and is already installed; no new dependency
  is planned.
- The inventory includes implicit mappings where a read type is reused as a write body, even if no
  function is currently named mapper.
- Backend mappers are excluded.

## Resume prompt

Not emitted while `## Next Steps` carries the hard-blocker fields. The last dependency owner or a
fresh status check opens the gate and supplies the implementation pointer.
