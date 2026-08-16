# Repository and DbContext permission hierarchy consumer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (open; replacement exact-head CI pending)
- Verified work head: `61b67199f57935c20151959f438e0a8768e13e15`
- Starting remote head: `b65fce300a93626ef3fc47a45b5a37fa3f97bf54`
- Pushed range: `b65fce300a93626ef3fc47a45b5a37fa3f97bf54..61b67199f57935c20151959f438e0a8768e13e15`
- Remote and PR head: `61b67199f57935c20151959f438e0a8768e13e15` (verified after work-head push)
- Dependency/package gate: satisfied. Additive producer PR #590 merged as `59fe60e978affe23bcaf53823151eab2acda8ba0`, published platform `0.1.0-alpha.0.1007`, and platform-sync PR #592 merged green as `38e3d8548f10f3ab7a4a951b7c4ce961ec21c863`. Current `origin/main` pins `0.1.0-alpha.0.1009`, which includes the additive DataAccess API.
- Last reconciled: 2026-08-16 against PR #561 and `origin/main` at `07624709d873dd0aecc934e59bbc45f78b0c844b`.

## Current state

The repo-wide consumer migration is implemented and reconciled locally with current main for #561.
It includes the Customer, B2B, and Payment repository
and context migrations, the B2B context-stance naming correction, and the Conversations-owned
participant projection required to preserve the module boundary.

The branch also carries two merge-queue fixes discovered while validating this work: the E2E reseeding
host now dispatches seeded participant events in process, and the three Strict Mode SPA login routes
start at most one OIDC redirect per mount.

The producer/package gate is open. The current-main merge preserves the typed-result service APIs,
capability-named `*ReadRepository` persistence surface, and Conversations-owned participant projection.
B2B now uses unqualified `XDbContext`/`XRepository` for the normal active-tenant unit of work,
`XReadDbContext`/`XReadRepository` for tenant-independent structurally read-only access, and
`VenueAdminDbContext`/`VenueAdminRepository` for the administrative write exception. No source or
filename contains the superseded concrete context/repository identifiers or a `Public*` persistence name.

The consistency sweep also covers reusable and pre-existing administrative persistence types:
`VenueArtistTenantScopedDbContext` matches the scoped repository/entity vocabulary, and Conversations
uses aggregate-first `ConversationsAdminDbContext`, `MessageAdminRepository`, and
`ContentReportAdminRepository` names with matching interfaces.

The standard repository-contract audit is also complete. Venue admin, Customer User, and Payment
PayoutAccount now inherit the combined repository interface and implementation bases instead of
hand-redeclaring generic CRUD. Payment FinancialOperation remains bespoke because it has no repository
identity contract and participates in a separate unit of work.

The first naming-correction CI run exposed a cancellation-test harness race: an untyped financial
completion could consume a pending acceptance command before the asynchronously dispatched refund.
The fixture now completes a requested command type, and refund workflows explicitly complete
`RefundEscrowCommand`. The branch has since been reconciled with the latest current main.

## Next Steps

1. Require green exact-head PR CI.
2. Await explicit merge authorization; then normalize to `full-e2e`, enqueue, and follow the merge-group,
   publication, and generated platform-sync gates to green before starting the legacy contraction.

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
- Current-main reconciliation: `initial-migrations.ps1` completed for every context; B2B Web Release
  build passed with 0 warnings and 0 errors; Artist, Venue, Concert, and Conversations unit tests passed
  17/17, 18/18, 219/219, and 34/34; source/filename persistence-name grep returned zero; plan graph
  reported 0 errors and 0 warnings; `git diff --cached origin/main --check` passed.
- Current-main work-head push succeeded for `899ced299..3b0d66643`; its checkpoint transport produced
  `1037165132e38f4fca8eddd991804ba097eba58d`, and exact-head CI run `31949481048` passed.
- Reconciled 31 newer `origin/main` commits and corrected the B2B persistence names without changing
  the context separation: normal tenant contexts/repositories are unqualified, alternate read-only
  contexts/repositories use `Read`, and the Venue admin exception uses aggregate-first `VenueAdmin`.
- Current candidate verification: B2B Web Release build passed with 0 errors; Artist, Venue, and Concert
  unit suites passed 17/17, 18/18, and 221/221; EF design-time resolution and pending-model checks passed
  for `ArtistDbContext`, `VenueDbContext`, and `ConcertDbContext`; plan graph and docs reachability reported
  0 errors and 0 warnings; old concrete-identifier and public persistence-filename gates returned zero.
  The full all-context migration script exceeded its 20-minute command budget without output, so its
  affected-context invariant was verified directly with EF instead.
- Naming-correction head `b29e5422f` reached exact-head CI run `31954686429`; all jobs passed except
  B2B Concert integration, where two cancellation tests deterministically found no refund command.
- The two exact failing tests passed after command-typed completion. The complete changed classes then
  passed 11/11 and 4/4 before current-main reconciliation.
- Merged 26 newer current-main commits through `07624709d`; B2B Web Release rebuilt with 0 errors, and
  the combined `ApplicationCancelApiTests` plus `ConcertCancelApiTests` scope passed 15/15.
- The completed B2B persistence-name sweep rebuilt B2B Web Release with 0 warnings and 0 errors;
  B2B DataAccess and Conversations unit suites passed 2/2 and 34/34; the old-name gate returned zero
  across source, guidance, plans, and review references.
- Exact-head CI run `31960086019` passed on checkpoint head `b65fce300`.
- The standard repository follow-up passed B2B Venue 18/18, Customer User 15/15, and Payment 272/272
  unit tests. B2B Web, Customer Web, and Payment Web Release builds passed with zero errors; the direct
  CRUD redeclaration audit now returns only the intentionally bespoke FinancialOperation contract.

## Reviews

- Formal and incremental reviews are recorded in
  `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`; all findings are resolved.
- Incremental review of `dc1f55591..c3afdb4b2` found no issues across the native, security,
  architecture, convention, seeding, and test-coverage lenses. Review and security watermarks are
  current at `c3afdb4b2fd137cbf406dfeb7174d9c082968c4d`.
- Incremental review of `c3afdb4b2..ff8354a15` found and resolved `BUG4`, the untyped payment-command
  completion race. No other correctness, security, architecture, convention, seeding, or changed-path
  coverage issue was found. Review and security watermarks are current at
  `ff8354a15ec6254a630b420c3e0c1f8a47da7ca9`.
- Incremental review of `ff8354a15..3f3734a5c` found and resolved `CV2`, the three standard repository
  contracts that still redeclared base CRUD. No other correctness, security, architecture, convention,
  seeding, or changed-path coverage issue was found. Review and security watermarks are current at
  `3f3734a5c7104b9d83cd4347e0ab571d15df69b6`.

## Decisions, discoveries, blockers, and deviations

- Consumer source may be prepared against an exact local producer package, but it could not merge
  until the same API existed in the published platform version. That gate is now satisfied.
- `Tenant` is the canonical backend identity term. `Organisation` remains presentation language only.
- B2B unqualified `XDbContext` is the normal tenant-bound tracked/write stance; `XReadDbContext` is the
  tenant-independent no-tracking/no-save stance; `VenueAdminDbContext` is the administrative writable
  exception. Customer read and projection-write contexts remain separate physical contexts.
- The plan requires three feature merges: additive package, consumers, then legacy contraction.
