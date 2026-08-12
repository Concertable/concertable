# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_repository-redesign`
- Branch: `Refactor/data-access_repository-redesign`
- PR: #522 — https://github.com/Concertable/concertable/pull/522 (PR-A, open)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.Infrastructure`); PR-A is Customer-internal, no publish cycle. PR-B blocked until PR-A (#522) merges.
- Last reconciled: 2026-08-12, PR-A pushed + opened

## Current state

PR-A (Customer read-only no-tracking contexts) is committed (`4439cb8e7`), pushed, and open as #522. Locally verified (build+unit+integration green).

Enforcement lands via a shared Customer read-only base `ReadDbContext` (in `Concertable.Customer.DataAccess.Infrastructure`, mirroring B2B's `PublicDbContext`: composes the module's `IEntityTypeConfigurationProvider`, `HasDefaultSchema`, seals `SaveChanges`/`SaveChangesAsync` to throw). Each read repo binds a per-module `{Concert,Venue,Artist}ReadDbContext : ReadDbContext` registered `.UseQueryTrackingBehavior(NoTracking)` with no interceptors. Read contexts expose their sets as `IQueryable<T>` (not `DbSet<T>`) so the read surface can't stage writes — a hardening beyond the plan, agreed with Tommy. The 3 concrete read repos drop the `Query` helper for the (now no-tracking) named sets.

PR-B not started.

## Next Steps

Drive PR-A (#522) to merge via `/merge`. Once merged, start PR-B in this same worktree/ledger (sync to current `origin/main` first): fold `IBaseRepository` into `IRepository` (kill the `new GetAllAsync` diamond), `Repository : ReadRepository`, delete `BaseRepository`, remove `Query` (rewrite base `GetAll`/`GetById` off it — first confirm zero remaining `Query`/`IBaseRepository` consumers repo-wide), add `InsertAsync` (`AddAsync`+`SaveChangesAsync`, returns entity), delete the dead B2B/Payment `ReadRepository<T>` aliases. Ships in `Concertable.DataAccess.Infrastructure` → publish → follow the `chore/platform-sync-*` PR to green (migrate any red consumer in that sync PR).

## Completed work

- PR-A implemented: shared `ReadDbContext` base + package ref on `Concertable.Customer.DataAccess.Infrastructure`; `{Concert,Venue,Artist}ReadDbContext`; rebound module-local `ReadRepository<T>` aliases + the 3 concrete read repos to the read contexts; DI registrations. (uncommitted)

## Verification

- `dotnet build Concertable.Customer/Concertable.Customer.slnx` — 0 errors.
- `Concertable.Customer.Concert.UnitTests` — 21/21 passed.
- `Concertable.Customer.Concert.IntegrationTests` — 11/11 passed (Docker healthy via `scripts/docker-health.ps1`); boots full module DI, proving `ConcertReadDbContext`/`ConcertReadRepository` resolve.
- Venue/Artist have no test projects (projection-only) — build is their gate.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- No separate `IReadDbContext` interface for repos to bind: the shared base `ReadRepository<T,TContext,K>` is constrained `TContext : DbContextBase` and calls `Set<T>()`, so an interface can't be the bound context. Read-only surface is expressed via `IQueryable<T>` properties on the concrete read context instead.
- `CustomerDb` connection name: a constant exists (`CustomerConstants.Database` in `Concertable.Customer.Hosting`), but every module-level DI uses the literal `"CustomerDb"` — Hosting is a layer modules don't reference. New read-context registrations use the literal to match their neighbors; unifying onto a shared constant (rehomed to a module-referenceable project) is a separate cleanup, not in this PR.
- PR-A does not touch shared published packages (`api/Concertable.{Kernel,DataAccess,...}` source), so no publish/platform-sync — consistent with the plan.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_repository-redesign
Read @plans/data-access/REPOSITORY_REDESIGN_PLAN.md and @plans/data-access/REPOSITORY_REDESIGN_PROGRESS.md and do what its `## Next Steps` says.
```
