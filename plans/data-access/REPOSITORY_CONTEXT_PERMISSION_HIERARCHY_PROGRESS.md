# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (draft)
- Remote and PR work head: `40b9e7076a2844e908a7e07f1b902a651f800869`
- Dependency/package gates: Phase 1 remains an additive producer PR. After it merges, package publication and the generated platform-sync PR must be green before Phase 2 migrates consumers.
- Last reconciled: 2026-08-15 against fetched `origin/main` at `dee412ba8`; the correction is
  uncommitted on local head `6779d2be3`, which is 11 commits behind base.

## Current state

Phase 1's additive shared context capabilities, independent read context, context-free repository bases,
and compatibility surface are implemented on draft PR #561. Earlier exact-head CI checkpoints were
green, but the current review invalidated the Artist/Venue organisation-identity lookup design, so the
PR was returned to draft before merge.

The replacement design is implemented in the working tree:

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
- The earlier projection and naming correction is committed at `28de99489`. The then-current
  `origin/main` merged cleanly at `9ba02a024`, and the post-merge branch-local platform build and
  focused Conversations tests were green.
- The verified work range `86b352807..40b9e7076` is pushed; the remote-tracking branch and PR #561
  both equal the recorded work head. The checkpoint-transport commit must land before exact-head CI is evaluated.

## Next Steps

1. Commit the booking-event, scoped-test, and workflow-policy correction; merge current `origin/main`
   into the clean branch and complete focused verification.
2. Push the coherent checkpoint to draft PR #561 and require exact-head draft CI.
3. Keep PR #561 draft until explicit authorization to make it ready and merge. When authorized, use
   the repository merge workflow, follow additive package publication and the generated platform-sync
   PR to green, and close this source worktree with `-PlanManaged`.
4. Create the Phase 2 consumer-migration worktree from the resulting current `origin/main` and migrate
   consumers against the published additive platform version.

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

- Earlier exact-head GitHub Actions runs `31812660710`, `31835968326`, and `31838309184` completed
  successfully for their respective pushed heads; they do not validate the current working-tree correction.
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
- `git diff --check` - passed.
- `python .agents/hooks/plan_graph.py --root C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy` - 0 errors, 0 warnings.
- Current correction `git diff --check` - passed.
- Current correction plan graph - 0 errors, 0 warnings.
- The focused B2B build reached the expected pre-publication package gate: service contexts cannot
  consume the branch-local additive `ReadDbContext` from the published platform pin. Repacking the
  local platform could not complete after the worktree exhausted local disk space, so exact-head draft
  CI remains the required validation for this checkpoint.
- Post-merge `./scripts/local-platform.ps1 prepare` - succeeded; packed 40 packages at
  `0.1.0-local.1786751073698`.
- Post-merge B2B Web Release build against that local platform - succeeded with 0 errors and the
  existing `UserEntity.UserEntity()` warning.
- Post-merge Conversations unit tests - 9 passed, 0 failed, 0 skipped.
- Work-head push `86b352807..40b9e7076` - succeeded; remote-tracking and PR heads both verified equal
  `40b9e7076a2844e908a7e07f1b902a651f800869`.
- Exact-head draft CI is pending.

## Reviews

- Tommy approved the shared permission hierarchy and later approved the corrected context stance names.
- Prior formal and incremental reviews are recorded in `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`.
- Working-tree review of the corrected projection, event compatibility, migrations, context renames,
  module boundaries, and tests found no open issues. Exact-head draft CI remains required after the
  branch is current with `origin/main`.

## Decisions, discoveries, blockers, and deviations

- Customer Artist/Venue/Concert still require separate read and projection-write contexts; this B2B
  naming correction does not collapse those Customer stances.
- B2B tenant-independent contexts intentionally compose the full module model. Repository contracts and
  DTOs control what leaves the module; the context name does not imply marketplace visibility.
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
