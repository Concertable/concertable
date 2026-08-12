# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B not yet opened. (PR-A #522 merged; IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-12, PR-B implemented + full-slnx build green

## Current state

PR-A and the IReadDbContext follow-up are both **merged + platform-sync green**. PR-B (base unify + `InsertAsync`) is implemented on this branch, `dotnet build api/Concertable.slnx` green; not yet reviewed/committed/opened.

PR-B changes (shared `Concertable.DataAccess`): `IBaseRepository` loses `GetAllAsync` (that member forced the diamond) and gains `InsertAsync`; `IRepository` drops its `new GetAllAsync`; `ReadRepository` drops the `Query`/`AsNoTracking` helper (reads are `context.Set<T>()`, tracking decided by the bound context); `Repository : ReadRepository` (reads live once) re-declaring the trivial writes; the dead B2B/Payment `ReadRepository<T>` aliases are deleted.

## Next Steps

Review PR-B (`/review`), commit, open the PR, drive it to merge (publish-first). Then **own the platform-sync**: the `chore/platform-sync-*` PR rebuilds all consumers against the republished package — if it goes red, migrate the failing consumer(s) IN that sync PR (legal once published) and push. When the sync is green, the whole plan is terminal → close out (delete plan + ledger together) via `/merge-docs`.

## Completed work

- **PR-A** (#522, merged `da9d02c29`, sync green): Customer read-only no-tracking contexts — shared `ReadDbContext` base + `{Concert,Venue,Artist}ReadDbContext` (NoTracking, `SaveChanges` throws); read repos rebound off `Query`.
- **#526** (merged `6a3d66677`, sync green): `IReadDbContext` — read repos depend on a queryable-only interface (`IQueryable<T> Query<T>()`), no `DbSet`/`Add`/`SaveChanges` reachable; DI injects each concrete read context as `IReadDbContext` via a factory.

## Verification

- `dotnet build api/Concertable.slnx` — 0 errors (DataAccess source + all consumers vs the published package + the alias cleanup).
- Consumers compile against the *published* DataAccess package, so my source change is exercised against them only at platform-sync; local gate is the build + the reasoned non-breaking analysis below.

## Reviews

PR-B not yet reviewed.

## Decisions, discoveries, blockers, and deviations

- **`IReadDbContext` was added (#526), reversing the earlier "no interface" note.** Tommy required it; the shared read base was reworked so repos bind the interface, not the concrete context.
- **PR-B keeps `IBaseRepository`/`BaseRepository` — the plan's "delete them" is wrong.** `SequenceRepository<TSequence>` is keyless (`ISequence : ITenant`, not `IEntity<TKey>`), so it can only use the keyless write-only `BaseRepository`; `CollectionSyncer`/`OpportunitySyncer` also depend on the write-only `IBaseRepository`. So the write-only facet stays; the diamond dies by removing `GetAllAsync` from it instead.
- **Non-breaking analysis:** no consumer calls `IBaseRepository.GetAllAsync` (verified); `IRepository` still exposes `GetAllAsync` via `IReadRepository` (dropping `new` is source-compatible); `InsertAsync` is additive; `GetByIdAsync` stays `virtual` so the `ConcertReadRepository`/`CommissionBindingRepository` overrides survive the publish.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify
Read @plans/data-access/REPOSITORY_REDESIGN_PLAN.md and @plans/data-access/REPOSITORY_REDESIGN_PROGRESS.md and do what its `## Next Steps` says.
```
