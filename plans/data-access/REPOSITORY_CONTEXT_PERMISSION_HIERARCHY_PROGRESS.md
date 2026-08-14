# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (draft)
- Dependency/package gates: BUG2 is fixed, exact-head CI is green at `580426684`, and follow-up incremental review is clean. PR #561 remains draft until this clean-review checkpoint is pushed and its exact-head CI is green; it can then be marked ready. Merge still requires explicit authorization, then additive package publication and the generated platform-sync PR must land green before Phase 2 resumes from current `origin/main`.
- Last reconciled: 2026-08-14 against fetched `origin/main` at `fc196ba99`; verified local, remote-tracking, and PR head `580426684`; working tree contains the clean follow-up review and ledger checkpoint

## Current state

The prior repository redesign is merged and its plan closed. Current `main` contains
`IReadDbContext`, `IReadRepository`, `IWriteRepository`, and composite `IRepository`, but the shared
implementation uses context-generic write/combined bases plus private read/write facet subclasses.

Tommy approved the target design. Phase 1's shared context capabilities, read-only EF base,
context-free write/combined repositories, and additive `ReadRepository.Context` migration property
are committed and pushed at `8ab4402d9`. Every legacy published type and field remains available.
The pushed work head and `origin/Refactor/DataAccessRepositoryPermissionHierarchy` were verified equal
at `8ab4402d9b5a2ff1adf613fcfdd143da887df423`; draft PR #561 targets `main`.

Draft CI run `31798505833` exposed six compiler errors because the new shared `ReadDbContext` collided
with Customer's redundant generic `ReadDbContext` intermediary. The correction moves its generic
configuration-provider/default-schema behavior, plus B2B `PublicDbContext`'s equivalent behavior, into
the shared DataAccess base. Both service intermediaries are deleted; the six concrete Customer/B2B
module read contexts now derive the shared base directly.

That reparenting exposed `PublicOpportunityRepository` inheriting a writable-context-constrained base.
It now has an independent read-only implementation over `ConcertDbContext`; the public and
regular repositories share only the `ActiveForVenue` query extension. The correction is committed at
`b850ea4b1` and passed its focused local gates. The corrected work and clean-review checkpoint range
`94d7664ad..3245c4fd9` was pushed to draft PR #561.

The branch merged fetched `origin/main` at `a2747a90f` without conflicts, producing work head
`a579be6989bff37b3e1d1f13589b63ba0e166e3b`. Local HEAD, the remote-tracking branch, and PR #561's
head were verified equal at that SHA. Exact-head CI run `31812660710` completed successfully across
the source-platform pack, full solution build, all service carves, unit tests, and integration tests.

The green checkpoint was recorded and pushed at `2a99965a3b5f13e456bd365e3e25b0bb16077c19`.
Review then paused on an unresolved B2B context-stance naming concern. Tommy approved the resolution:
keep two physical contexts, rename the tenant-independent read-only `PublicXDbContext` types to
`XDbContext`, and rename the tenant-aware tracked/write `XDbContext` types to `TenantXDbContext`.
`IReadDbContext` supplies the read capability; B2B does not add a third `XReadDbContext` without a
genuinely distinct restricted/projection model. `AdminVenueDbContext` retains its explicit admin
tracked/write stance.

The approved correction is implemented and pushed at `272d17a78` across Artist, Venue, and Concert. Tenant-independent
contexts now use the module `XDbContext` names, tenant-aware tracked/write contexts use
`TenantXDbContext`, and migration ownership remains with the tenant contexts without schema changes.
Artist/Venue organisation identity and booking existence now use purpose-specific internal contracts;
only genuine marketplace repositories retain `Public`.

## Next Steps

1. Commit and push the clean BUG2 follow-up review through the two-leg plan checkpoint protocol.
2. Require exact-head draft CI for that checkpoint, then mark PR #561 ready when green.
3. Wait for explicit authorization to merge PR #561. When authorized, merge it through the repository
   merge workflow, follow additive package publication and the generated platform-sync PR to green,
   and close this source worktree with `-PlanManaged`.
4. Create the Phase 2 consumer-migration worktree from the resulting current `origin/main` and migrate
   consumers against the published additive platform version.

## Completed work

- Created an isolated planning worktree from fetched `origin/main` and completed the initial repository/context census.
- Drafted the roadmap item, plan, and progress ledger.
- Tommy approved the context interfaces, independent repository implementations, service migration matrix, and staged package cutover.
- Implemented and committed Phase 1's additive shared permission surface at `8ab4402d9`; retained the legacy arities plus protected read-context field for package compatibility and opened draft PR #561.
- Removed the redundant Customer `ReadDbContext` and B2B `PublicDbContext` intermediaries; all six
  concrete module read contexts now derive the shared DataAccess `ReadDbContext` directly.
- Split `PublicOpportunityRepository` from the writable generic opportunity base and moved the shared
  active-for-venue predicate to a query extension.
- Investigated B2B tenancy stances from the concrete contexts, configuration providers, selective query
  filters, DI, repositories/services/tests, EF model-caching rules, and the commits that introduced the
  split. Confirmed separate physical contexts are correct and recorded Tommy's approved naming scheme.
- Implemented the approved Artist/Venue/Concert context names across DI, unit-of-work/interceptor
  bindings, design-time factories, migration metadata, repositories, services, tests, and documentation
  without adding a migration or third context.
- Split Artist/Venue organisation identity lookups and booking existence into purpose-specific internal
  contracts while retaining `Public` only on marketplace repository contracts.
- Committed and pushed the coherent correction at `272d17a78`; verified the pushed range
  `2a99965a3..272d17a78` and exact equality of local, remote-tracking, and PR heads.
- Fixed BUG2 with focused Artist/Venue organisation-identity lookup coverage and pushed the work range
  `350ae02a1..860d6fac8`; local, remote-tracking, and PR heads were verified equal.

## Verification

- `dotnet build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-restore --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `dotnet test api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-build --no-restore --disable-build-servers` - 12 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 prepare` - succeeded; packed 40 branch-local packages at version `0.1.0-local.1786712598892`.
- `./scripts/local-platform.ps1 build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `./scripts/local-platform.ps1 build api/Concertable.Customer/tests/Concertable.Customer.IntegrationTests.Fixtures/Concertable.Customer.IntegrationTests.Fixtures.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `./scripts/local-platform.ps1 build api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/Concertable.B2B.IntegrationTests.Fixtures.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors after the public opportunity repository split.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj --configuration Release --disable-build-servers` - 133 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 build api/Concertable.slnx --configuration Release` - inconclusive locally; exceeded the 20-minute command ceiling without emitting a compiler error. Exact-head draft CI owns the complete solution matrix.
- `dotnet build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-restore --disable-build-servers` after merging current `origin/main` - inconclusive locally; exceeded the 3-minute command ceiling after compiling the changed DataAccess assemblies without emitting a compiler error.
- GitHub Actions CI run `31812660710` at work head `a579be698` - succeeded; source-platform pack, full solution build, all service carves, unit tests, and integration tests passed.
- `python .agents/hooks/plan_graph.py --root C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy` - 0 errors, 0 warnings.
- `git diff --check` - passed.
- `./scripts/local-platform.ps1 prepare` - succeeded; packed 40 branch-local packages at version `0.1.0-local.1786734284588`.
- `./scripts/local-platform.ps1 build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj --configuration Release --disable-build-servers` - succeeded with 0 errors and the existing `UserEntity.UserEntity()` warning.
- `./scripts/local-platform.ps1 test api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` - 12 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Artist/Tests/Concertable.B2B.Artist.UnitTests/Concertable.B2B.Artist.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` - 5 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Venue/Tests/Concertable.B2B.Venue.UnitTests/Concertable.B2B.Venue.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` - 5 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` - 134 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 build api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concertable.B2B.Concert.IntegrationTests.csproj --configuration Release --no-restore --disable-build-servers` - inconclusive locally; exceeded the four-minute command ceiling without emitting a compiler error. Exact-head draft CI owns the integration-test compile matrix.
- Repository-wide stale B2B context/repository name grep - no code matches; plan and ledger wording updated to the implemented names.
- Push checkpoint `2a99965a3..272d17a78` - succeeded; local, remote-tracking, and PR heads verified equal at `272d17a78202433161bdd314230d6e289454bd4a`.
- GitHub Actions CI run `31835968326` at exact PR head `350ae02a1` - succeeded; source-platform pack, full solution build, service carves, unit tests, and integration tests passed.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Artist/Tests/Concertable.B2B.Artist.UnitTests/Concertable.B2B.Artist.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` after BUG2 - 6 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Venue/Tests/Concertable.B2B.Venue.UnitTests/Concertable.B2B.Venue.UnitTests.csproj --configuration Release --no-restore --disable-build-servers` after BUG2 - 6 passed, 0 failed, 0 skipped.
- Push checkpoint `350ae02a1..860d6fac8` - succeeded; local, remote-tracking, and PR heads verified equal at `860d6fac8ab4dfe980562ed776f353ffe53d46f6`.
- GitHub Actions CI run `31838309184` at exact PR head `580426684` - the first attempt had one external `mcr.microsoft.com` connection reset while Payment Testcontainers pulled an image; the one-time failed-job rerun succeeded on the unchanged SHA.

## Reviews

- Tommy design review approved.
- Working-tree self-review found and corrected the `ReadRepository` protected-field binary compatibility edge.
- Formal review of `429581025..94d7664ad` found BUG1 after exact-head CI exposed the namespace collision; the working-tree correction resolves it in `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`.
- Incremental review of `94d7664ad..b850ea4b1` found no new issues; the review watermark is current at
  the corrected code head and BUG1 is resolved.
- Incremental review of `b850ea4b1..350ae02a1` found BUG2: the new Artist/Venue organisation-identity
  collaborators lacked focused wiring coverage. The fix is committed and pushed at `860d6fac8`.
  Security review of the merged Auth, Payment, Contracts, and configuration paths found no issues.
- Follow-up incremental review of `350ae02a1..580426684` found no new issues. BUG2 is resolved, and the
  review and security watermarks are current at `580426684`.

## Decisions, discoveries, blockers, and deviations

- Customer Artist/Venue/Concert each require both `XReadDbContext` and `XDbContext`: the first serves
  customer reads; the second writes local replicas only from integration events. Customer still exposes
  no write repository for those B2B-owned aggregates.
- B2B tenant-independent contexts derive the shared read-only base directly; no B2B intermediary context
  or writable context constraint remains in the public opportunity repository path.
- The approved B2B names are `XDbContext` for the tenant-independent no-tracking/save-rejecting stance
  and `TenantXDbContext` for the `ITenantContext`-carrying tracked/write stance. `AdminVenueDbContext`
  remains the tenant-independent administrative write stance. `Global` is redundant, and `Read` does
  not name a third physical stance because `XDbContext` already implements `IReadDbContext`.
- The tenant contexts apply selective filters, not blanket module filtering: Artist filters
  `ArtistEntity`; Venue filters `VenueEntity` and `VenueImageEntity`; Concert filters
  `ApplicationEntity`, `BookingEntity`, `ContractEntity`, `InvoiceEntity`, and
  `SelfBillingAgreementEntity`.
- Tenant-independent contexts intentionally compose the full module model. Repository contracts and
  DTOs control what leaves the module; marketplace reads and internal cross-tenant facts may safely
  share the same read-only physical context.
- `IBookingExistence` is an internal cross-tenant existence fact, not a public repository.
  Artist/Venue organisation-identity lookup methods are likewise internal facts and are split from the
  otherwise genuine marketplace repositories.
- Generic read-context plumbing belongs in shared DataAccess, not in Customer or B2B. The shared
  `ReadDbContext` owns configuration-provider/default-schema composition, no-tracking, query access,
  and save rejection; services own only their meaningful physical module contexts.
- `SequenceRepository` inherits the write base only to reuse `AddAsync`, but its contract is a custom
  allocator rather than `IWriteRepository`; it will use `ConcertDbContext` directly instead.
- Many bespoke B2B and Payment repositories use typed sets or EF-specific APIs. Shared bases will not
  retain `TContext`; only those concrete repositories retain their exact context privately.
- Search, Messaging, and framework contexts must be classified rather than mechanically forced into
  entity CRUD repositories.
- The prior atomic published-base reparent caused a runtime `FieldAccessException`; delivery must be
  additive expansion, published consumer migration, then contraction.
- `ReadRepository<TEntity, TKey>` cannot gain a replacement arity, so Phase 1 adds `Context` while
  retaining the published protected `context` field; Phase 2 migrates derived source and Phase 3
  removes the field only after source/package grep gates are clean.
