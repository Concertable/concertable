# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: [#561](https://github.com/Concertable/concertable/pull/561) (draft)
- Dependency/package gates: Phase 1 must pass exact-head draft CI before it can merge and publish the additive package surface
- Last reconciled: 2026-08-14 against fetched `origin/main` at `429581025`, local head `b850ea4b1`, PR head `94d7664ad`, and failed CI run `31798505833`

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
It now has an independent read-only implementation over `PublicConcertDbContext`; the public and
regular repositories share only the `ActiveForVenue` query extension. The correction is committed at
`b850ea4b1` and passed its focused local gates.

## Next Steps

1. Push the verified context-hierarchy correction and review checkpoint, then verify local/remote/PR
   head equality.
2. Follow exact-head draft CI and fix any remaining failures on the corrected package topology.
3. When Phase 1 CI is green, record its package-publication delivery gate and prepare the Phase 2
   consumer migration from current `origin/main` after this PR merges.

## Completed work

- Created an isolated planning worktree from fetched `origin/main` and completed the initial repository/context census.
- Drafted the roadmap item, plan, and progress ledger.
- Tommy approved the context interfaces, independent repository implementations, service migration matrix, and staged package cutover.
- Implemented and committed Phase 1's additive shared permission surface at `8ab4402d9`; retained the legacy arities plus protected read-context field for package compatibility and opened draft PR #561.
- Removed the redundant Customer `ReadDbContext` and B2B `PublicDbContext` intermediaries; all six
  concrete module read contexts now derive the shared DataAccess `ReadDbContext` directly.
- Split `PublicOpportunityRepository` from the writable generic opportunity base and moved the shared
  active-for-venue predicate to a query extension.

## Verification

- `dotnet build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-restore --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `dotnet test api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-build --no-restore --disable-build-servers` - 12 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 prepare` - succeeded; packed 40 branch-local packages at version `0.1.0-local.1786712598892`.
- `./scripts/local-platform.ps1 build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `./scripts/local-platform.ps1 build api/Concertable.Customer/tests/Concertable.Customer.IntegrationTests.Fixtures/Concertable.Customer.IntegrationTests.Fixtures.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `./scripts/local-platform.ps1 build api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/Concertable.B2B.IntegrationTests.Fixtures.csproj --configuration Release --disable-build-servers` - succeeded with 0 warnings and 0 errors after the public opportunity repository split.
- `./scripts/local-platform.ps1 test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj --configuration Release --disable-build-servers` - 133 passed, 0 failed, 0 skipped.
- `./scripts/local-platform.ps1 build api/Concertable.slnx --configuration Release` - inconclusive locally; exceeded the 20-minute command ceiling without emitting a compiler error. Exact-head draft CI owns the complete solution matrix.
- `python .agents/hooks/plan_graph.py --root C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy` - 0 errors, 0 warnings.
- `git diff --check` - passed.

## Reviews

- Tommy design review approved.
- Working-tree self-review found and corrected the `ReadRepository` protected-field binary compatibility edge.
- Formal review of `429581025..94d7664ad` found BUG1 after exact-head CI exposed the namespace collision; the working-tree correction resolves it in `reviews/Refactor-DataAccessRepositoryPermissionHierarchy.md`.
- Incremental review of `94d7664ad..b850ea4b1` found no new issues; the review watermark is current at
  the corrected code head and BUG1 is resolved.

## Decisions, discoveries, blockers, and deviations

- Customer Artist/Venue/Concert each require both `XReadDbContext` and `XDbContext`: the first serves
  customer reads; the second writes local replicas only from integration events. Customer still exposes
  no write repository for those B2B-owned aggregates.
- B2B public contexts now derive the shared read-only base directly; no B2B intermediary context or
  writable context constraint remains in the public opportunity repository path.
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
