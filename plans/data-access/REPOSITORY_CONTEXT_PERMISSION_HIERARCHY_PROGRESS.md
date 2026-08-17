# Repository and DbContext permission hierarchy consumer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/rpc`
- Branch: `Refactor/RepositoryPermissionContraction`
- PR: [#632](https://github.com/Concertable/concertable/pull/632) (ready, `full-e2e`, currently dequeued)
- Starting head: `95305c7a909d48a703ab572c2a153fe74d2d4daa`
- Consumer PR: [#561](https://github.com/Concertable/concertable/pull/561) merged as `249dc8a9df8d9b81271cd2250a01ecf086e97586`.
- Dependency/package gate: satisfied. Additive producer PR #590 merged as `59fe60e978affe23bcaf53823151eab2acda8ba0`, published platform `0.1.0-alpha.0.1007`, and platform-sync PR #592 merged green as `38e3d8548f10f3ab7a4a951b7c4ce961ec21c863`. Current `origin/main` pins `0.1.0-alpha.0.1009`, which includes the additive DataAccess API.
- Consumer publication/sync gate: satisfied. Publication run `31976777846` passed and platform-sync
  PR [#623](https://github.com/Concertable/concertable/pull/623) merged green as
  `d5669a836c4d7fd9bb4d15e9c05f0a71f0e9f40c`.
- Follow-up gate: satisfied. PR [#626](https://github.com/Concertable/concertable/pull/626) merged as
  `a790708f3d0b087e4f8f2c9c8d0362d91a90a8bc`; publication run `31983176063` published
  `0.1.0-alpha.0.1044`, and platform-sync PR
  [#629](https://github.com/Concertable/concertable/pull/629) merged green as
  `95305c7a909d48a703ab572c2a153fe74d2d4daa`.
- Last reconciled: 2026-08-17 against `origin/main` at `92ea04166`.

## Current state

The repo-wide consumer migration and its signup/topology reliability follow-up are merged, published,
and platform-synced on main. Phase 3 contraction is implemented in the short-path worktree: every
source consumer now uses the final context-free repository arities, concrete-context access is declared
locally where specialized queries require it, unused module write aliases are removed, and the shared
legacy arities, facet implementation, protected read-context field, and compatibility-only tests are
gone. Static verification remained green after merging current main through `92ea04166`. Draft PR #632
now owns compilation and tests because the local .NET runner repeatedly stalled without producing
compiler output; its initial remote work head is `fa86fd8bb`.

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

The current code and security review found no implementation defect. Review finding `CV1` is resolved
at `86b702886`: `api/agents/CODE_CONVENTIONS.md` now documents the context-free repository arities,
capability interfaces, concrete-context-forwarding module aliases, and private concrete-context rule.
Incremental review of `21cd0aba8..86b702886` found no new issues.

All six B2B and Customer Artist/Venue/Concert read contexts now implement module-specific interfaces
that expose only named `IQueryable` roots. Production repositories and query services inject those
interfaces rather than concrete EF contexts, while the shared `ReadDbContext` still enforces no
tracking and rejects saves.

The first naming-correction CI run exposed a cancellation-test harness race: an untyped financial
completion could consume a pending acceptance command before the asynchronously dispatched refund.
The fixture now completes a requested command type, and refund workflows explicitly complete
`RefundEscrowCommand`. The branch has since been reconciled with the latest current main through
`b633d79aa`; the incoming range contains only the reviewed launch-roadmap gap sweep.

Full-E2E merge-group run `31974080294` was ejected after 31/32 B2B UI scenarios passed. Artist signup
timed out because the registration URL wait attached after the sign-up click and missed the navigation
load edge. The mandated fresh-stack rerun also exposed that B2B handles `SendEmailCommand` without
provisioning its service command queue. The missing queue is now provisioned and covered by the
topology contract test, and both signup flows attach their registration wait before clicking.

## Next Steps

1. Push the reviewed `CV1` fix and plan checkpoint, then require exact-head PR CI to pass.
2. Delete the fully resolved review work order through the address-review closeout, require the new
   exact head to pass, keep the `full-e2e` tier, and return PR #632 to the merge queue.
3. On merge, close the source worktree and follow package publication and platform sync to green,
   then complete the published-baseline plan closeout gates.

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
- The narrow read-context surface passed B2B Artist 17/17, Venue 18/18, and Concert 229/229 plus
  Customer Artist 2/2, Venue 2/2, and Concert 25/25 unit tests. B2B and Customer Web Release builds
  passed with zero errors before the constructor-only review fix; every constructor-touched module
  then rebuilt through its focused unit suite.
- Work head `b0b3d35af` was pushed from `ba3e4ddab`, then verified equal on the remote branch and PR #561.
- Current main through `b633d79aa` merged cleanly; the Release solution build passed with 0 errors and
  10 existing warnings.
- Reviewed current-main work head `4179700e2` was pushed from `11c99197f`, then verified equal on the
  remote branch and PR #561.
- Exact-head CI run `31973511659` passed on checkpoint head `80d492cd3`.
- Full-E2E merge-group run `31974080294` passed 31/32 B2B UI scenarios but failed artist signup at
  `SignUpSteps.ClickSignUpLink`; API E2E and the remaining hard-floor jobs passed.
- The automatically readmitted merge-group run `31975154334` passed full E2E; consumer PR #561 then
  merged as `249dc8a9d`, package publication run `31976777846` passed, and platform-sync PR #623 merged
  green as `d5669a836`.
- Follow-up exact-head CI run `31979493729` passed on `7361b99b1`; two newer main guidance commits were
  then merged through `27dd5f7b4`, and the focused topology suite remained green at 7/7.
- B2B topology tests passed 7/7 after adding the B2B `SendEmailCommand` queue contract.
- B2B and Customer UI E2E projects built in Release with 0 errors after moving both registration waits
  ahead of their sign-up clicks.
- Follow-up exact-head CI run `31980425847` and full-E2E merge-group run `31981377623` passed on PR
  #626 before it merged as `a790708f3`.
- Follow-up publication run `31983176063` published `0.1.0-alpha.0.1044`; platform-sync PR #629 passed
  exact-head and merge-group CI and merged green as `95305c7a9`.
- Fresh contraction worktree graph reported 0 errors and 0 warnings at `95305c7a9`.
- Phase 3 source contraction removed every legacy repository-arity, `base.context`, facet, and protected
  read-context-field match across `api/**/*.cs`; every production repository that uses a concrete
  `context` now owns an explicit concrete `DbContext` field, and `git diff --check` passed.
- Merged 22 newer `origin/main` commits through `92ea04166`; the post-merge legacy grep and concrete-
  context ownership gates remained clean, plan graph reported 0 errors and 0 warnings, and
  `git diff --check origin/main...HEAD` passed.
- Work head `fa86fd8bb` was pushed and verified equal to draft PR #632's remote `headRefOid`.
- Recovery checkpoint `21cd0aba8` was pushed and exact-head CI run `32004250451` passed its platform
  pack, solution build, all service carves, selected unit/integration matrices, and `ci-complete`.
- Initial code and security review of `92ea04166..21cd0aba8` found no implementation or security issues.
- Review finding `CV1` was fixed at `86b702886`; obsolete-arity and inherited-context greps, positive
  hierarchy greps, docs reachability, and `git diff --check` passed.
- Incremental review of `21cd0aba8..86b702886` found no new correctness, architecture, convention,
  security, or changed-path coverage issues.
- Reviewed fix head `86b702886` was pushed from `21cd0aba8`, then verified equal to the remote branch
  and PR #632 `headRefOid`.
- PR #632 was marked ready on unchanged remote head `21cd0aba8`; the published-package public-shape
  trigger selected `full-e2e`, with no contradictory skip label present.
- The final fetch found the remote head 0 commits behind current `origin/main`; PR #632 entered the
  merge queue at position 1 in `AWAITING_CHECKS` state on unchanged head `21cd0aba8`.
- Merge-group run `32009106399` passed the hard-floor build, carve, unit, integration, API E2E, and UI
  E2E gates. The PR timeline records removal from the queue at `2026-08-17T08:43:23Z` under the
  `tomjseery` actor; no CI check failed, and the PR is now open, clean, mergeable, and unqueued.
- Local restore/test and no-restore test attempts repeatedly stalled without compiler or test output;
  their orphaned processes were stopped and no failure result was inferred. Draft-PR CI is the required
  compile/test gate for the exact committed head.
- Docker health passed before the focused artist-signup rerun. The fresh local stack could not reach
  the scenario because the long plan-worktree path caused Windows to reject
  `Microsoft.Data.SqlClient.SNI.dll` with `0x800700CE`; exact-head and merge-group CI remain the valid
  scenario execution gates from normal runner paths.

## Reviews

- Review of contraction range `92ea04166..21cd0aba8` and clean incremental review of the `CV1` fix
  through `86b702886` are recorded in `reviews/Refactor-RepositoryPermissionContraction.md`; all
  findings are resolved.

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
- Incremental review of `3f3734a5c..f9cb45b8c` found and resolved `CV3`, captured primary constructors
  on the new query-backed base and touched read consumers. No other correctness, security, architecture,
  convention, seeding, or changed-path coverage issue was found. Review and security watermarks are
  current at `f9cb45b8c15ffea1e612efa80e6fbbb388770443`.
- Incremental review of `f9cb45b8c..b887f15c8` found no issues. The range contains only review and
  plan checkpoints plus the already-reviewed launch-roadmap gap sweep from current main; the review
  watermark is current at `b887f15c8fe3baec149033dcc99358d8cc6cb959`.
- Review of follow-up range `d5669a836..1a2da63ba` found no issues. The review is recorded in
  `reviews/Fix-SignupNavigationRace.md`; the diff changes no security-sensitive production path.
- Incremental review of `7361b99b1..27dd5f7b4` found no issues; the range contains only current-main
  guidance and its merge commit, with no overlap in the repaired paths.

## Decisions, discoveries, blockers, and deviations

- Consumer source may be prepared against an exact local producer package, but it could not merge
  until the same API existed in the published platform version. That gate is now satisfied.
- `Tenant` is the canonical backend identity term. `Organisation` remains presentation language only.
- B2B unqualified `XDbContext` is the normal tenant-bound tracked/write stance; `XReadDbContext` is the
  tenant-independent no-tracking/no-save stance; `VenueAdminDbContext` is the administrative writable
  exception. Customer read and projection-write contexts remain separate physical contexts.
- The plan requires three feature merges: additive package, consumers, then legacy contraction.
- The merge-group repair must remove the navigation race and missing B2B command queue; it must not
  inflate the 15-second browser timeout or bypass the failing signup step.
- The focused local rerun is environment-blocked by the plan worktree's Windows path length, not by
  Docker health or an application startup error. The source projects compile and the topology contract
  passes locally; remote full-E2E must prove the repaired scenario.
- GitHub automatically readmitted #561 after the first failed merge-group and landed the previously
  pushed head before the repair commits existed. The repair therefore ships as a current-main follow-up
  rather than mutating the already-merged consumer PR.
