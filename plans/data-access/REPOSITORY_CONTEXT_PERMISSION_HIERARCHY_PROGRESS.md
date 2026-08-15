# Repository and DbContext permission hierarchy consumer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (open; delivery-gated)
- Remote and PR head: `dc1f55591ca589a5516bca6c6513e7a095beed1c`
- Dependency/package gates: the additive DataAccess producer owned by
  `REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PRODUCER_PROGRESS.md` must merge, publish a newer platform
  package, and finish its generated platform sync before #561 can be revalidated and merged.
- Last reconciled: 2026-08-15 against PR #561, fetched `origin/main`, and the package-boundary build.

## Current state

The repo-wide consumer migration is implemented, committed, reviewed, and delivery-ready on #561
against the exact branch-local producer package. It includes the Customer, B2B, and Payment repository
and context migrations, the B2B context-stance naming correction, and the Conversations-owned
participant projection required to preserve the module boundary.

The branch also carries two merge-queue fixes discovered while validating this work: the E2E reseeding
host now dispatches seeded participant events in process, and the three Strict Mode SPA login routes
start at most one OIDC redirect per mount.

After `origin/main` advanced to platform version `0.1.0-alpha.0.1002`, a normal package-bound solution
build failed because that published DataAccess version does not yet contain `ReadDbContext`. This is the
expected producer/consumer delivery boundary, not a consumer source defect. The local source worktree
currently has an unpushed current-main merge commit and observation-only plan/review edits; preserve it
until the producer gate opens, then reconcile from the newly published baseline before pushing.

## Next Steps

Blocked: PR #561 cannot become merge-ready against the currently published DataAccess package.
Blocked by: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PRODUCER_PROGRESS.md`
Unblock action: merge the additive DataAccess producer, confirm package publication advanced, and follow its generated platform-sync PR to green and merged.
Resume when: `origin/main` pins a published `Concertable.DataAccess.Application` and `Concertable.DataAccess.Infrastructure` version containing `IDbContext`, `IWriteDbContext`, `ReadDbContext`, and the context-free repository arities.

## Completed work

- Implemented and pushed the consumer migration and compatibility-preserving hierarchy on #561.
- Replaced persistence-level `Public*Repository` names with capability-based `*ReadRepository` names;
  the old public repositories were read-only and never supported Artist/Venue updates.
- Replaced synchronous cross-module participant identity lookups with a Conversations-owned event-fed
  projection.
- Fixed the two merge-queue E2E defects at `016bd25fb` and `a36851c84`.

## Verification

- Exact-head CI run `31895752976` passed on `dc1f55591`: local platform pack, full backend build and
  carves, frontend carves/boundaries, selected unit/integration matrices, and `ci-complete`.
- Merge-group run `31892616154` passed API E2E and all 31 B2B UI scenarios; its Customer failure was
  diagnosed from the trace and fixed by the OIDC redirect guards.
- Shared web packages passed with 6/6 shared and 16/16 B2B shared tests after the latest main merge.
- Customer, Venue, Artist, and Business production builds passed after the OIDC fix.
- A normal solution build after platform pin `0.1.0-alpha.0.1002` failed only where consumers referenced
  the not-yet-published additive DataAccess API, confirming the delivery gate.

## Reviews

- Formal and incremental reviews are recorded in
  `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`; all findings are resolved through
  reviewed head `dc1f55591ca589a5516bca6c6513e7a095beed1c`.

## Decisions, discoveries, blockers, and deviations

- Consumer source may be prepared against an exact local producer package, but it cannot merge until
  the same API exists in the real published platform version.
- `Tenant` is the canonical backend identity term. `Organisation` remains presentation language only.
- B2B `XDbContext` is tenant-independent and read-only; `XTenantDbContext` is tenant-bound, tracked,
  and writable. Customer read and projection-write contexts remain separate physical contexts.
- The plan requires three feature merges: additive package, consumers, then legacy contraction.

