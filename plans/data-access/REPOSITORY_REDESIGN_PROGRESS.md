# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B #530 — https://github.com/Concertable/concertable/pull/530 (open). Scope expanded (2026-08-12): seam fix + reparent + `IWriteRepository` rename, all this PR. Plan is execution-ready for handoff. (PR-A #522 merged; IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-13 — Phase 1 is complete and verified on current `origin/main` (`98fa02b9e`). The current CI graph contains 40 packable platform projects, 19 unit projects, and 13 integration projects.

## Current state

PR-A (#522) and IReadDbContext (#526) are merged + platform-sync green. **PR-B (#530): the binary-breaking reparent remains reverted until Phase 1 is verified.** `Repository<T,TContext,TKey> : BaseRepository` again, with the 3 read members re-declared; `context` stays on `BaseRepository` so feed-compiled consumers keep resolving. All additive wins retained: `InsertAsync`, dead B2B/Payment `ReadRepository<T>` alias deletion, `GetAllAsync` removed from `IBaseRepository` (kills the diamond), `Query` removed.

**Phase 1 is complete.** `.github/workflows/test.yml` and
`scripts/{local-platform,integration,unit,e2e,test}.ps1` now enforce the seam. `local-platform.ps1` packs the 40 production
`IsPackable` projects at one unique MinVer override, emits a mapped local NuGet config, restores/builds/
tests consumers against that version, and verifies integration/E2E outputs contain exactly one
`Concertable.DataAccess.Infrastructure.dll` at that local version. CI packs once and shares the artifact
with build, carve, unit, integration, and any enabled E2E jobs; local unit/integration/E2E runners prepare
and consume the same feed. Publishing and committed service pins are unchanged.

**History (why the revert):** the reparent `Repository : ReadRepository` was binary-breaking — `context` moved off `BaseRepository`; feed-compiled consumers (`DealRepository → TenantScopedRepository → Repository`, `ConcertablePlatformVersion 0.1.0-alpha.0.955`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (`Seed.Infrastructure` source ProjectReference, higher MinVer wins) → `FieldAccessException`, 6 suites (run 31636765379). Reverting keeps `context` on `BaseRepository`; the lost "reads defined once" is cosmetic (the real win, context-enforced no-tracking, shipped in PR-A/#526).

## Next Steps

1. Implement Phase 2: reparent `Repository<T,TContext,TKey>` onto `ReadRepository<T,TContext,TKey>`.
2. Move the write methods and `InsertAsync` onto `Repository`; remove its re-declared read methods so reads are inherited once.
3. Run the six proof suites named in the plan against a newly prepared local platform feed, then the full build, unit, and integration gates. Diagnose any red integration test through `integration-debug`.
4. Update the plan and ledger, commit Phase 2, and stop at the phase boundary.

## Completed work

- **PR-A** (#522, merged `da9d02c29`, sync green): Customer read-only no-tracking contexts — shared `ReadDbContext` base + `{Concert,Venue,Artist}ReadDbContext` (NoTracking, `SaveChanges` throws); read repos rebound off `Query`.
- **#526** (merged `6a3d66677`, sync green): `IReadDbContext` — read repos depend on a queryable-only interface (`IQueryable<T> Query<T>()`), no `DbSet`/`Add`/`SaveChanges` reachable; DI injects each concrete read context as `IReadDbContext` via a factory.
- **Phase 1 — seam fix:** the test and CI harnesses pack the source platform once, override every consumer pin to that version, and assert integration/E2E outputs contain exactly one `Concertable.DataAccess.Infrastructure.dll` at the expected version. The normal publish workflow and committed service pins are unchanged.

## Verification

- Local platform `0.1.0-local.1786575969629`: 40/40 production `IsPackable` projects packed at one MinVer override.
- Unit: 19/19 dynamically discovered projects green, 974/974 tests.
- Integration: 13/13 projects green, 390/390 tests; every project verified exactly one `Concertable.DataAccess.Infrastructure.dll` at the local platform version. Four Customer projects required the repository's documented `subst` short-path workaround for Windows `MAX_PATH`; their first failures were `Microsoft.Data.SqlClient.SNI.dll` load errors, not product failures.
- Release solution build against the local feed after the final `origin/main` merge: 0 errors, 4 existing warnings.
- Static checks: plan graph, PowerShell parse, and `git diff --check` green.

## Reviews

PR-B not yet reviewed.

## Decisions, discoveries, blockers, and deviations

- **`IReadDbContext` was added (#526), reversing the earlier "no interface" note.** Tommy required it; the shared read base was reworked so repos bind the interface, not the concrete context.
- **PR-B keeps `IBaseRepository`/`BaseRepository` — the plan's "delete them" is wrong.** `SequenceRepository<TSequence>` is keyless (`ISequence : ITenant`, not `IEntity<TKey>`), so it can only use the keyless write-only `BaseRepository`; `CollectionSyncer`/`OpportunitySyncer` also depend on the write-only `IBaseRepository`. So the write-only facet stays; the diamond dies by removing `GetAllAsync` from it instead.
- **Non-breaking analysis:** no consumer calls `IBaseRepository.GetAllAsync` (verified); `IRepository` still exposes `GetAllAsync` via `IReadRepository` (dropping `new` is source-compatible); `InsertAsync` is additive; `GetByIdAsync` stays `virtual` so the `ConcertReadRepository`/`CommissionBindingRepository` overrides survive the publish.
- **That analysis was incomplete and the PR is NOT non-breaking.** It reasoned only about *source* compatibility. Reparenting `Repository : ReadRepository` moves the inherited `context` field's declaring type off `BaseRepository` — a **binary** break. Feed-compiled consumers (`DealRepository : TenantScopedRepository : Repository`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (via `Seed.Infrastructure`'s source ProjectReference winning on MinVer) → `FieldAccessException`. Confirmed by run 31636765379. Source-compatibility ≠ binary-compatibility for a published base whose consumers touch inherited fields.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.
- **Current-main graph drift was reconciled:** Auth, B2B Conversations, and Payment are included in the local integration runner; its 13-project set now matches CI's dynamic discovery.
