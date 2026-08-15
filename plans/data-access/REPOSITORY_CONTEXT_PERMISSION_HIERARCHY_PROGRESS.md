# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (draft)
- Verified work and PR head: `df469d372c77dbfde67142f8e69ea31f6bdb3997`; pushed from remote head
  `3044fb74c2eb6b238a80c5bc443ec6442163d037` through `df469d372` and verified against the
  remote-tracking ref and draft PR head. The checkpoint-transport commit and its exact-head CI remain.
- Dependency/package gates: Phase 1 remains an additive producer PR. After it merges, package publication and the generated platform-sync PR must be green before Phase 2 migrates consumers.
- Last reconciled: 2026-08-15 against fetched `origin/main` at `520761dd4`; the branch is current with
  base after merge commit `beb0bd91d`.

## Current state

Phase 1's additive shared context capabilities, independent read context, context-free repository bases,
and compatibility surface are implemented on draft PR #561. A later review invalidated the original
Artist/Venue organisation-identity lookup design, so the PR was returned to draft and corrected before
merge. The repository-naming correction described below is committed and pushed on the current branch;
exact-head CI and follow-up review still gate readiness.

The replacement design is implemented. Exact-head CI passed on the pushed checkpoint; the focused
follow-up review fixes are committed, reviewed, and green under the Conversations unit-test project.

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

1. Transport this push checkpoint, verify local/remote/PR head equality, then require exact-head draft CI.
2. Keep PR [#561](https://github.com/Concertable/concertable/pull/561) draft until those gates are green
   and Tommy explicitly authorizes `/merge`; when asking for that permission, always include this
   clickable PR link.

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
