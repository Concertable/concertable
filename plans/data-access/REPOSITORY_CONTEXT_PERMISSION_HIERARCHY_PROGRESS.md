# Repository and DbContext permission hierarchy consumer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (open; exact-head CI pending)
- Verified work head: `9e270e8337a2d14c07e87b08569ce027a7b004c2`
- Starting remote head: `dc1f55591ca589a5516bca6c6513e7a095beed1c`
- Pushed range: `dc1f55591ca589a5516bca6c6513e7a095beed1c..9e270e8337a2d14c07e87b08569ce027a7b004c2`
- Remote and PR head: `9e270e8337a2d14c07e87b08569ce027a7b004c2` (verified equal after push)
- Dependency/package gate: satisfied. Additive producer PR #590 merged as `59fe60e978affe23bcaf53823151eab2acda8ba0`, published platform `0.1.0-alpha.0.1007`, and platform-sync PR #592 merged green as `38e3d8548f10f3ab7a4a951b7c4ce961ec21c863`. Current `origin/main` pins `0.1.0-alpha.0.1009`, which includes the additive DataAccess API.
- Last reconciled: 2026-08-15 against PR #561, fetched `origin/main` at `e8242eb43ee922ed34699ccbccdf29e473448b0d`, producer publication, and platform-sync evidence.

## Current state

The repo-wide consumer migration is implemented, committed, reviewed, and delivery-ready on #561.
It includes the Customer, B2B, and Payment repository
and context migrations, the B2B context-stance naming correction, and the Conversations-owned
participant projection required to preserve the module boundary.

The branch also carries two merge-queue fixes discovered while validating this work: the E2E reseeding
host now dispatches seeded participant events in process, and the three Strict Mode SPA login routes
start at most one OIDC redirect per mount.

The producer/package gate is open. Current `origin/main` is reconciled in the preserved consumer
worktree. The three Customer read-context conflicts retain the consumer migration to the published
shared `ReadDbContext`; the plan artifacts retain the compact versions from `main` with current
delivery evidence. Package-bound B2B, Customer, and Payment builds and the focused tests are green.
The compound reconciliation work head is verified on the remote branch and PR.

## Next Steps

1. Push this checkpoint-only transport commit and verify local, remote-tracking, and PR heads are equal.
2. Require green exact-head PR CI, normalize to `full-e2e`, enqueue, and follow the new merge-group run
   to a terminal result without retrying a failure.
3. On merge, close the source worktree and follow publication plus the generated platform-sync PR to
   green before starting the legacy shared-package contraction.

## Completed work

- Implemented and pushed the consumer migration and compatibility-preserving hierarchy on #561.
- Replaced persistence-level `Public*Repository` names with capability-based `*ReadRepository` names;
  the old public repositories were read-only and never supported Artist/Venue updates.
- Replaced synchronous cross-module participant identity lookups with a Conversations-owned event-fed
  projection.
- Fixed the two merge-queue E2E defects at `016bd25fb` and `a36851c84`.
- Merged, published, and platform-synced the additive DataAccess producer through PRs #590 and #592.

## Verification

- Exact-head CI run `31895752976` passed on `dc1f55591`: local platform pack, full backend build and
  carves, frontend carves/boundaries, selected unit/integration matrices, and `ci-complete`.
- Merge-group run `31892616154` passed API E2E and all 31 B2B UI scenarios; its Customer failure was
  diagnosed from the trace and fixed by the OIDC redirect guards.
- Shared web packages passed with 6/6 shared and 16/16 B2B shared tests after the latest main merge.
- Customer, Venue, Artist, and Business production builds passed after the OIDC fix.
- Producer exact-head CI run `31899830109` and full-E2E merge-group run `31900417169` passed.
- Producer package publication run `31902042481` published `0.1.0-alpha.0.1007`; platform-sync PR #592
  passed exact-head and merge-group CI and merged green.
- Package-bound Release builds against platform `0.1.0-alpha.0.1009`: B2B Web and Customer Web passed
  with zero errors and their existing sealed-constructor warnings; Payment Web passed with zero
  warnings and zero errors.
- Focused B2B DataAccess/Conversations, Customer Artist/Venue/Concert, and Payment unit-test projects
  all passed.
- Current merge diff checks passed; plan graph reported 0 errors and 0 warnings.

## Reviews

- Formal and incremental reviews are recorded in
  `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`; all findings are resolved through
  reviewed work head `9e270e8337a2d14c07e87b08569ce027a7b004c2`.
- Incremental review of `dc1f55591..c3afdb4b2` found no issues across the native, security,
  architecture, convention, seeding, and test-coverage lenses. Review and security watermarks are
  current at `c3afdb4b2fd137cbf406dfeb7174d9c082968c4d`.

## Decisions, discoveries, blockers, and deviations

- Consumer source may be prepared against an exact local producer package, but it could not merge
  until the same API existed in the published platform version. That gate is now satisfied.
- `Tenant` is the canonical backend identity term. `Organisation` remains presentation language only.
- B2B `XDbContext` is tenant-independent and read-only; `XTenantDbContext` is tenant-bound, tracked,
  and writable. Customer read and projection-write contexts remain separate physical contexts.
- The plan requires three feature merges: additive package, consumers, then legacy contraction.
