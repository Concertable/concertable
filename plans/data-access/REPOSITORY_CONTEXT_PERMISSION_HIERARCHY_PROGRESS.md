# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (ready, open)
- Remote and PR head: `e39b3759c64ad8fc7e4b9a9fdc415512a1143d11`; reviewed local fix head:
  `a36851c8477432fbd7c110dc3786316c83633be7`. Push and replacement exact-head CI are pending.
- Tommy explicitly authorized `/merge` on 2026-08-15. The published contract/package shape requires
  the `full-e2e` merge-queue tier; queue admission is the next delivery action.
- `origin/main` advanced by six documentation-only commits during CI. It merged cleanly at
  `495fd7900`; the merged range is reviewed and the branch is current with base. The required local
  solution build exceeded ten minutes without a compiler diagnostic, so exact-head CI remains the
  authoritative build gate before queue admission.
- Plan-managed work-head push succeeded for `fd2c51386..495fd7900`; local, remote-tracking, and draft
  PR heads were all verified at `495fd7900`.
- Exact-head CI passed on `a2c2cbd33`. PR #561 is ready with the mandatory `full-e2e` label; auto-merge
  was enabled through the protected merge queue. GitHub admitted the verified PR head at queue position
  4 while earlier merge groups ran. Its own merge group is now `AWAITING_CHECKS` at queue position 3:
  run `31888159066`, merge head `a0ceaae9f`, based on `c0af85731`.
- Merge-group API E2E passed on run `31888159066`; UI E2E is queued behind the remaining hard-floor
  matrix jobs.
- Merge-group UI E2E failed on run `31888159066`: 30/31 scenarios passed, while `A venue's members
  share one inbox with independent read state` could not find a mailbox message labeled `The Rockers`.
  No HTTP 4xx/5xx, browser-console, on-screen, or service error was logged. The failure is in the
  participant-profile rendering path changed by this branch; the failed run was not retried.
- GitHub ejected and unlocked PR #561. The failure screenshot confirmed the API rendered the seeded
  artist as `Unknown`. The E2E reseeding host registered the outbox with its dispatcher disabled but
  omitted the in-process event dispatch used by the real B2B host, so seeded Artist/Venue events never
  populated Conversations' participant projection. The local fix adds that missing production-aligned
  registration; it does not direct-seed the projection or add a runtime cross-module fallback.
- The fix is committed at `016bd25fb`, incrementally reviewed with no findings, and current
  `origin/main` at `9516a2a2b` merged cleanly at `f80bd66c5`. The updated plan graph passes with zero
  errors and zero warnings.
- The reviewed transport checkpoint is committed at `e39b3759c`; local code is ready to push over PR
  head `a2c2cbd33` and then requires replacement exact-head CI.
- The first push of `e39b3759c` was rejected without changing the remote because GitHub still marked
  #561 as merge-queued after the failed group despite clearing its auto-merge request. The exact stale
  queue entry was removed through GitHub's dequeue mutation; the same reviewed SHA then pushed
  successfully and local, remote-tracking, and PR heads were verified equal.
- Replacement exact-head CI run `31891846110` passed on `e39b3759c`: local platform pack, full
  solution build, all backend carves, selected unit and integration matrices, and `ci-complete` are
  green. PR-level E2E jobs skipped as expected; the `full-e2e` label reserves them for the merge group.
- The branch remains current with `origin/main` at `9516a2a2b`. GitHub re-admitted exact PR head
  `e39b3759c` with the `full-e2e` label. Replacement merge-group run `31892616154` passed API E2E and
  all 31 B2B UI scenarios, proving the seeded participant projection fix. It then failed an unrelated
  Customer sign-up scenario after 5/6 Customer scenarios passed: `RunAndWaitForNavigationAsync` at
  `SignUpSteps.cs:39` received `net::ERR_ABORTED; maybe frame was detached?`. The failed run must not be
  retried. Its Playwright trace shows the Customer SPA issued two `/connect/authorize` requests about
  16 ms apart with different PKCE state values; the second navigation aborted the first login page.
  All three Strict-Mode SPA login routes now guard `signinRedirect` with a mount-persistent ref so one
  route activation starts one OIDC navigation.
- The OIDC navigation fix is committed at `a36851c84`, incrementally reviewed with no findings, and
  passes the full four-app web boundary build.
- `origin/main` advanced by four Tenant invitation-outbox commits while the queue ran. They merged
  cleanly at `8992a36cd`; incremental review found no overlap or new issue.
- Dependency/package gates: Phase 1 remains an additive producer PR. After it merges, package publication and the generated platform-sync PR must be green before Phase 2 migrates consumers.
- Last reconciled: 2026-08-15 against fetched `origin/main` at `863e0c3af`; the branch is current with
  base after merge commit `8992a36cd`.

## Current state

Phase 1's additive shared context capabilities, independent read context, context-free repository bases,
and compatibility surface are implemented on draft PR #561. A later review invalidated the original
Artist/Venue organisation-identity lookup design, so the PR was returned to draft and corrected before
merge. The repository-naming correction described below is committed and pushed on the current branch;
exact-head CI and follow-up review still gate readiness.

The replacement design is implemented. The focused follow-up review fixes are committed, reviewed,
and green locally and under exact-head draft CI. PR #561 is clean and remains draft with auto-merge off.

- `Tenant` is the canonical Domain/Application/Infrastructure/Contracts term. In the current model it
  is the business account, membership boundary, legal/VAT/Stripe identity, and settlement identity;
  there is no separate organisation aggregate or identifier.
- Artist and Venue are tenant-owned marketplace profiles. `ArtistOrgIdentity`, `VenueOrgIdentity`, both
  lookup abstractions/implementations, and their module/service facade methods are removed.
- Conversations owns a `ParticipantProfile` projection keyed by `TenantId`, maintained from
  `ArtistChangedEvent` and `VenueChangedEvent`. Message rendering reads that local projection instead
  of synchronously reaching into Artist or Venue. `VenueChangedEvent.TenantId` is an additive property
  so existing positional consumers remain source/binary compatible.
- Tenant-bound tracked/write context names are module-first: `ArtistTenantDbContext`,
  `VenueTenantDbContext`, and `ConcertTenantDbContext`. Their tenant-independent read-only counterparts
  remain `ArtistDbContext`, `VenueDbContext`, and `ConcertDbContext`.
- The initial-migration script targets the migration-owning tenant contexts. The Conversations initial
  migration is regenerated with `ParticipantProfiles`; the context renames only update factory,
  snapshot, and migration metadata names and do not change their schemas.
- B2B guidance now makes Tenant/organisation vocabulary and the participant projection boundary
  explicit so the concepts are not mixed again.
- `IBookingExistence` and `BookingExistence` are removed. Escrow payment processors resolve the
  booking's application through `IBookingRepository`, acknowledge a stale missing-booking event
  without throwing, and pass a valid application id into `EscrowExecutor`.
- Normal PR readiness no longer enables delivery: the repo-wide ready-event auto-merge workflow and
  the docs instant-merge workflow are removed. Explicit `/merge` or `/merge-docs` authorization owns
  delivery; platform-sync retains only its bot-PR-scoped automation.
- Integration-test guidance now requires the Kernel `IScoped<T>` scope-root abstraction for a single
  scoped dependency or event-handler collection instead of hand-written `CreateScope()` blocks.
- The booking, scoped-test, and merge-policy correction is committed at `f8609b6a9`; current
  `origin/main` merged without conflicts at `906c3da13`.
- Exact-head CI exposed missing Payment contract imports in the new escrow integration test. The
  focused compiler correction is committed at `af7586cf7`.
- The corrected checkpoint is pushed at `d8c36cf05`; PR #561 remains open and draft with auto-merge
  disabled.
- The remaining `Public`-qualified repository persistence names were inconsistent with the corrected
  context stance. Their interfaces, implementations, files, DI registrations, and service fields are now named
  `ArtistReadRepository`, `VenueReadRepository`, `ConcertReadRepository`, and
  `OpportunityReadRepository`; marketplace audience remains at the API contract rather than in
  persistence type names. The correction is committed and pushed at `391bd2517`.
- The earlier projection and naming correction is committed at `28de99489`. The then-current
  `origin/main` merged cleanly at `9ba02a024`, and the post-merge branch-local platform build and
  focused Conversations tests were green.
- Exact-head draft CI run `31881386510` passed on pushed head `3044fb74c` with the full build, service
  carves, selected unit and integration matrices, and `ci-complete` green.
- Current `origin/main` merged cleanly at `beb0bd91d`.
- Follow-up review of `580426684..beb0bd91d` found two medium issues: captured primary constructors in
  the new participant projection handlers and missing assertions for sender profile rendering. Both
  are fixed at `5c6ab849f`; the focused Conversations unit tests pass 11/11 and incremental review of
  `beb0bd91d..5c6ab849f` found no further issues.
- Plan-managed work-head push succeeded for `3044fb74c..df469d372`; local, remote-tracking, and draft
  PR heads were all verified at `df469d372`.

## Next Steps

1. Commit the current-main review checkpoint, push it through exact-head CI, then re-enqueue `full-e2e` and follow a new merge group
   to a terminal result without retrying a failed run.
2. On green, close the source worktree, sync main,
   then follow package publication and the generated platform-sync PR to green and merged.
3. From a fresh close-out worktree, reconcile the terminal delivery evidence, delete this plan and
   ledger together, tick the roadmap item, and land the docs-only close-out.

## Completed work

- Created the plan-managed worktree, completed the repository/context census, and obtained design approval.
- Implemented and pushed Phase 1's additive shared permission surface and compatibility seam.
- Removed redundant service-specific read-context bases and moved their shared behavior into the
  published DataAccess `ReadDbContext`.
- Split the public opportunity read path from the tenant-bound writable repository path.
- Renamed Artist/Venue/Concert physical context stances consistently across DI, repositories, services,
  interceptors, seeders, tests, design-time factories, snapshots, and migration metadata.
- Rejected and removed the intermediate Artist/Venue organisation-identity lookup design before merge.
- Implemented the Conversations-owned participant projection and additive Venue event enrichment.

## Verification

- `./scripts/local-platform.ps1 build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj --configuration Release --disable-build-servers --maxcpucount:1` - succeeded with 0 errors and the existing `UserEntity.UserEntity()` warning.
- Focused Conversations unit-test project build - succeeded with 0 errors and the existing `UserEntity.UserEntity()` warning.
- `dotnet ef migrations add InitialCreate --no-build --configuration Release --context ConversationsDbContext ...` against the branch-local platform build - succeeded; the generated initial migration contains `conversations.ParticipantProfiles` keyed by `TenantId`.
- Conversations unit tests - 9 passed, 0 failed, 0 skipped.
- Artist unit tests - 5 passed, 0 failed, 0 skipped.
- Venue unit tests - 5 passed, 0 failed, 0 skipped.
- Concert unit tests - 134 passed, 0 failed, 0 skipped.
- Repository-wide code/docs grep found no stale Artist/Venue organisation-identity lookup or old B2B
  context names.
- Mechanical rename comparison confirmed all nine context, factory, and snapshot renames differ only by
  the approved identifier replacement.
- Post-merge `git diff origin/main...HEAD --check` - passed.
- Post-merge plan graph - 0 errors, 0 warnings.
- Exact-head CI run `31865873337` exposed missing Payment contract imports in
  `EscrowPaymentProcessorTests`; `af7586cf7` added the two existing contract imports.
- Exact-head draft CI run `31868276444` on `d8c36cf05` - passed: local platform pack, full solution
  build, service carves, selected unit and integration matrices, and `ci-complete` all green.
- Current `./scripts/local-platform.ps1 prepare` - succeeded; packed 40 exact-branch packages at
  `0.1.0-local.1786789942005`.
- Artist, Venue, and Concert Infrastructure Release builds against that exact package set - succeeded
  with 0 warnings and 0 errors.
- Current Artist unit tests - 5 passed, 0 failed, 0 skipped.
- Current Venue unit tests - 5 passed, 0 failed, 0 skipped.
- Current Concert unit tests - 134 passed, 0 failed, 0 skipped.
- Whole-repository identifier and filename greps found zero old repository-name survivors.
- The whole B2B Web build against the exact local package exceeded the five-minute local command cap
  without a compiler diagnostic; the three directly affected infrastructure builds are green and
  exact-head draft CI remains the authoritative full-build gate.
- Post-merge B2B Web Release build against that local platform - succeeded with 0 errors and the
  existing `UserEntity.UserEntity()` warning.
- Post-merge Conversations unit tests - 9 passed, 0 failed, 0 skipped.
- Current `git diff --check` - passed.
- Current plan graph - 0 errors, 0 warnings.
- Exact-head draft CI run `31881386510` on `3044fb74c` - passed: local platform pack, full solution
  build, service carves, selected unit and integration matrices, and `ci-complete` all green.
- Current focused Conversations unit tests after the review fixes - 11 passed, 0 failed, 0 skipped.
- Exact-head draft CI run `31883621520` on `fd2c51386` - passed: local platform pack, full solution
  build, service carves, selected unit and integration matrices, and `ci-complete` all green.
- Post-current-main local `dotnet build api/Concertable.slnx --configuration Release
  --disable-build-servers --maxcpucount:1` - stopped after exceeding ten minutes with no compiler
  diagnostic; exact-head PR CI is required before queue admission.
- Exact-head draft CI run `31885137119` on `a2c2cbd33` - passed: local platform pack, full solution
  build, service carves, selected unit and integration matrices, and `ci-complete` all green.
- Merge-group API E2E passed and UI E2E ran 30/31 scenarios green on run `31888159066`; the sole
  failure exposed the missing in-process event dispatcher in the E2E reseeding host.
- Messaging unit tests after the fix - 41 passed, 0 failed, 0 skipped.
- The mandatory local Docker HTTP round-trip health check timed out, so no local browser rerun was
  started. The affected E2E project build reached branch package consumers but failed because the
  local package set predates this branch's additive `ReadDbContext`; exact-head CI must rebuild the
  local platform before the replacement merge-group run.
- Replacement exact-head CI run `31891846110` on `e39b3759c` - passed: local platform pack, full
  solution build, all backend carves, selected unit and integration matrices, and `ci-complete` green.
- Replacement merge-group run `31892616154` - API E2E passed; B2B UI E2E passed 31/31 including the
  formerly failing mailbox scenario; Customer UI E2E passed 5/6 and failed `New customer registers and
  signs in` on an aborted login-to-sign-up navigation.
- The failed Customer trace confirms two concurrent OIDC authorize navigations, not an Auth service or
  HTTP failure. Customer/Venue/Artist login routes now start `signinRedirect` at most once per mount.
- The mandatory Docker HTTP health gate timed out again, so no local browser run was started.
- Fresh lockfile install and shared web package build - succeeded; shared package tests passed 6/6.
- Customer, Venue, Artist, and Business production builds after the auth fix - all succeeded.

## Reviews

- Tommy approved the shared permission hierarchy and later approved the corrected context stance names.
- Prior formal and incremental reviews are recorded in `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`.
- Follow-up review of `580426684..beb0bd91d` covered correctness, security, microservice isolation,
  module boundaries, seeding, C# conventions, and test coverage. `CV1` and `BUG3` are fixed at
  `5c6ab849f`; incremental review of the committed fix range found no further issues.

## Decisions, discoveries, blockers, and deviations

- Customer Artist/Venue/Concert still require separate read and projection-write contexts; this B2B
  naming correction does not collapse those Customer stances.
- B2B tenant-independent contexts intentionally compose the full module model. Repository contracts and
  DTOs control what leaves the module; the context name does not imply marketplace visibility.
- Persistence types describe capability and stance, not API audience: B2B tenant-independent readers
  use `XReadRepository`; `Public` remains valid only at genuine HTTP/presentation boundaries.
- `IBookingExistence` was not a distinct domain capability: it duplicated one unfiltered booking query
  solely to decorate an exception. Worker scopes already bypass tenant filters, and booking creation
  commits before Payment is called, so `IBookingRepository.GetApplicationIdByIdAsync` is sufficient.
- A missing booking id on an escrow payment event is acknowledged and logged at the event boundary;
  it is not translated into a domain or HTTP-shaped exception.
- PR readiness is review state, not merge authorization. Ambient workflows must not merge or enable
  auto-merge for normal PR lifecycle events; only explicit delivery workflows may do so.
- Consumer-owned event-fed projections are the durable cross-module answer for response display data.
  Provider modules do not publish response-shaped identity records or expose synchronous lookup facades.
- `Organisation` remains valid presentation language in HTTP DTOs/routes/UI copy, but backend domain and
  cross-module contracts use `Tenant` unless a future model establishes genuinely different lifecycle
  or cardinality.
- The complete initial-migration script was interrupted during its repeated host builds; the temporary
  deletion it left was restored. The changed Conversations context was then rebuilt against the
  branch-local platform and scaffolded successfully in isolation.
