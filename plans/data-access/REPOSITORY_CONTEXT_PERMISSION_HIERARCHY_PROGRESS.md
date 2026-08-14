# Repository and DbContext permission hierarchy progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchy`
- PR: not opened
- Dependency/package gates: design approved; Phase 1 is implemented locally; delivery will use additive publish -> consumer migration -> contraction publish with a green platform sync after each package merge
- Last reconciled: 2026-08-14 against fetched `origin/main` at `429581025`, the repo-wide census, and the verified Phase 1 working tree

## Current state

The prior repository redesign is merged and its plan closed. Current `main` contains
`IReadDbContext`, `IReadRepository`, `IWriteRepository`, and composite `IRepository`, but the shared
implementation uses context-generic write/combined bases plus private read/write facet subclasses.

Tommy approved the target design. Phase 1 is implemented and verified in the working tree: the shared
context capabilities, read-only EF base, context-free write/combined repositories, and additive
`ReadRepository.Context` migration property are present while every legacy published type and field
remains available. The roadmap, plan, ledger, and implementation are not yet committed.

## Next Steps

1. Commit the coherent Phase 1 additive package checkpoint with this ledger state.
2. Push the work head and verify the remote branch head, then open a draft GitHub PR.
3. Record the PR and verified remote head in this ledger, push the checkpoint transport commit, and verify local/remote/PR head equality.
4. Let exact-head draft PR CI validate the full build, service carves, unit, and integration matrices before merge review.

## Completed work

- Created an isolated planning worktree from fetched `origin/main` and completed the initial repository/context census.
- Drafted the roadmap item, plan, and progress ledger.
- Tommy approved the context interfaces, independent repository implementations, service migration matrix, and staged package cutover.
- Implemented Phase 1's additive shared permission surface and retained the legacy arities plus protected read-context field for package compatibility.

## Verification

- `dotnet build api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-restore --disable-build-servers` - succeeded with 0 warnings and 0 errors.
- `dotnet test api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj --no-build --no-restore --disable-build-servers` - 12 passed, 0 failed, 0 skipped.
- `python .agents/hooks/plan_graph.py --root C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-data-access-repository-permission-hierarchy` - 0 errors, 0 warnings.
- `git diff --check` - passed.

## Reviews

- Tommy design review approved.
- Working-tree self-review found and corrected the `ReadRepository` protected-field binary compatibility edge.
- Formal branch code/documentation review remains pending before merge.

## Decisions, discoveries, blockers, and deviations

- Customer Artist/Venue/Concert each require both `XReadDbContext` and `XDbContext`: the first serves
  customer reads; the second writes local replicas only from integration events. Customer still exposes
  no write repository for those B2B-owned aggregates.
- B2B public contexts are physically read-only but currently inherit the writable shared base; the new
  shared `ReadDbContext` corrects that inheritance.
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

## Recent transitions

- The initial discussion explored and rejected context generics on `ReadRepository`, nested facet
  composition, removal of the standalone `WriteRepository`, invented combined-context names, and
  collapsing Customer read/projection contexts. The plan records the resolved hierarchy so those paths
  are not repeated.
