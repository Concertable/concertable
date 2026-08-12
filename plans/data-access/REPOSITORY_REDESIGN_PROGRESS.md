# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B #530 — https://github.com/Concertable/concertable/pull/530 (open). Scope expanded (2026-08-12): seam fix + reparent + `IWriteRepository` rename, all this PR. Plan is execution-ready for handoff. (PR-A #522 merged; IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-12 — scope decided (fix the seam, don't defer); full 5-phase execution plan written. Tree currently at `535a3ebd7` (reparent reverted = binary-safe fallback); seam fix not yet built.

## Current state

PR-A (#522) and IReadDbContext (#526) are merged + platform-sync green. **PR-B (#530): the binary-breaking reparent has been reverted (direction 1).** `Repository<T,TContext,TKey> : BaseRepository` again, with the 3 read members re-declared; `context` stays on `BaseRepository` so feed-compiled consumers keep resolving. All additive wins retained: `InsertAsync`, dead B2B/Payment `ReadRepository<T>` alias deletion, `GetAllAsync` removed from `IBaseRepository` (kills the diamond), `Query` removed. `api/Concertable.slnx` builds 0 errors. Not yet pushed.

**History (why the revert):** the reparent `Repository : ReadRepository` was binary-breaking — `context` moved off `BaseRepository`; feed-compiled consumers (`DealRepository → TenantScopedRepository → Repository`, `ConcertablePlatformVersion 0.1.0-alpha.0.955`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (`Seed.Infrastructure` source ProjectReference, higher MinVer wins) → `FieldAccessException`, 6 suites (run 31636765379). Reverting keeps `context` on `BaseRepository`; the lost "reads defined once" is cosmetic (the real win, context-enforced no-tracking, shipped in PR-A/#526).

## Next Steps

**Decided (2026-08-12): fix the seam properly and do it all in this PR** — no deferral. Full mechanism +
5 phases in `REPOSITORY_REDESIGN_PLAN.md` → "The seam fix" and "Execution plan". Execute in order:

1. **Phase 1 — Seam fix (local platform-pack + pin override).** Make the integration build compile AND
   run every consumer against the ONE source-built platform (carve-safe: consumers stay PackageReference).
   Land first.
2. **Phase 2 — Restore the reparent** (`Repository : ReadRepository`). This is the exact change that
   failed as run 31636765379 — it going green is the proof the seam fix works (the 6 suites: B2B
   Artist/Concert/User/Venue, Customer User/Concert).
3. **Phase 3 — Rename** `IBaseRepository`/`BaseRepository` → `IWriteRepository`/`WriteRepository`, full
   grep-gate (`grep -rniE "ibaserepository|baserepository"` → zero, every tier/casing), all consumers
   migrated in-PR.
4. **Phase 4 — Verify:** local pack → `dotnet build api/Concertable.slnx -p:ConcertablePlatformVersion=<local>`
   0 errors; integration suites green via `e2e-*` skills (Docker health pre-flight); a red suite → the
   matching debug skill, not a report.
5. **Phase 5 — Deliver:** push (queue tier `skip-e2e`), own the `chore/platform-sync-*` PR to merged
   (consumers already migrated → green), close out per `plans/AGENTS.md`.

**Current tree state:** the reparent is currently REVERTED to `Repository : BaseRepository` (commit
`535a3ebd7`) so PR-B is green *without* the seam fix. Phase 2 re-applies the reparent once Phase 1 lands.
If executing, either start from here (reparent reverted, seam fix not yet built) or note that
`535a3ebd7` is the binary-safe fallback if the seam fix proves infeasible.

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
- **That analysis was incomplete and the PR is NOT non-breaking.** It reasoned only about *source* compatibility. Reparenting `Repository : ReadRepository` moves the inherited `context` field's declaring type off `BaseRepository` — a **binary** break. Feed-compiled consumers (`DealRepository : TenantScopedRepository : Repository`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (via `Seed.Infrastructure`'s source ProjectReference winning on MinVer) → `FieldAccessException`. Confirmed by run 31636765379. Source-compatibility ≠ binary-compatibility for a published base whose consumers touch inherited fields.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify
Read @plans/data-access/REPOSITORY_REDESIGN_PLAN.md and @plans/data-access/REPOSITORY_REDESIGN_PROGRESS.md and do what its `## Next Steps` says.
```
