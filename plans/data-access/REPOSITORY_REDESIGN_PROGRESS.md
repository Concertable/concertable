# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B #530 — https://github.com/Concertable/concertable/pull/530 (open). Scope expanded (2026-08-12): seam fix + reparent + `IWriteRepository` rename, all this PR. Plan is execution-ready for handoff. (PR-A #522 merged; IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-13 — Phase 2 is complete as `d65293cc3` and includes current `origin/main` (`3a5df8b18`); the post-verification drift was docs-only and did not change the 40 packable platform projects, 22 unit projects, or 16 integration projects.

## Current state

PR-A (#522) and IReadDbContext (#526) are merged + platform-sync green. **PR-B (#530): Phase 2 is complete locally.** `Repository<T,TContext,TKey>` now inherits `ReadRepository<T,TContext,TKey>`, inherits the read members once, and owns the duplicated write members plus `InsertAsync`. The keyless write-only `BaseRepository` remains unchanged for `SequenceRepository` and direct `IBaseRepository` consumers. Phase 3's `IWriteRepository`/`WriteRepository` rename remains outstanding.

**Phase 1 is complete.** `.github/workflows/test.yml` and
`scripts/{local-platform,integration,unit,e2e,test}.ps1` now enforce the seam. `local-platform.ps1` packs the 40 production
`IsPackable` projects at one unique MinVer override, emits a mapped local NuGet config, restores/builds/
tests consumers against that version, and verifies integration/E2E outputs contain exactly one
`Concertable.DataAccess.Infrastructure.dll` at that local version. CI packs once and shares the artifact
with build, carve, unit, integration, and any enabled E2E jobs; local unit/integration/E2E runners prepare
and consume the same feed. Publishing and committed service pins are unchanged.

**History (why the revert):** the reparent `Repository : ReadRepository` was binary-breaking — `context` moved off `BaseRepository`; feed-compiled consumers (`DealRepository → TenantScopedRepository → Repository`, `ConcertablePlatformVersion 0.1.0-alpha.0.955`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (`Seed.Infrastructure` source ProjectReference, higher MinVer wins) → `FieldAccessException`, 6 suites (run 31636765379). Reverting keeps `context` on `BaseRepository`; the lost "reads defined once" is cosmetic (the real win, context-enforced no-tracking, shipped in PR-A/#526).

## Next Steps

1. Implement Phase 3: rename `IBaseRepository`/`BaseRepository` to `IWriteRepository`/`WriteRepository` across the shared platform and every consumer, including identifiers and module aliases.
2. Run the whole-repository case-insensitive grep gate for `ibaserepository|baserepository`; only the plan and ledger's explicitly historical text may remain, and every other occurrence must be removed.
3. Prepare a new local platform feed, run the full Release solution build plus all dynamically discovered unit and integration projects, and verify every integration output contains exactly one DataAccess assembly at that local version. Diagnose any red integration test through `integration-debug`.
4. Update the plan and ledger, commit Phase 3, and stop at the phase boundary.

## Completed work

- **PR-A** (#522, merged `da9d02c29`, sync green): Customer read-only no-tracking contexts — shared `ReadDbContext` base + `{Concert,Venue,Artist}ReadDbContext` (NoTracking, `SaveChanges` throws); read repos rebound off `Query`.
- **#526** (merged `6a3d66677`, sync green): `IReadDbContext` — read repos depend on a queryable-only interface (`IQueryable<T> Query<T>()`), no `DbSet`/`Add`/`SaveChanges` reachable; DI injects each concrete read context as `IReadDbContext` via a factory.
- **Phase 1 — seam fix:** the test and CI harnesses pack the source platform once, override every consumer pin to that version, and assert integration/E2E outputs contain exactly one `Concertable.DataAccess.Infrastructure.dll` at the expected version. The normal publish workflow and committed service pins are unchanged.
- **Phase 2 — repository reparent (`d65293cc3`):** `Repository<T,TContext,TKey>` inherits `ReadRepository<T,TContext,TKey>`, reads are defined once, and the concrete repository base carries the write operations required by `IRepository`.

## Verification

- Local platform `0.1.0-local.1786608449216`: 40/40 production `IsPackable` projects packed at one MinVer override.
- Six historical proof projects: B2B Artist 17/17, Concert 144/144, User 3/3, Venue 25/25; Customer User 6/6 and Concert 11/11. Each output contained exactly one DataAccess assembly at the local platform version.
- Release solution build against the local feed: 0 errors, 8 existing warnings.
- Unit: 22/22 dynamically discovered projects green, 1,038/1,038 tests.
- Integration: 16/16 dynamically discovered projects green, 404/404 tests; every output contained exactly one `Concertable.DataAccess.Infrastructure.dll` at the local platform version.
- The deep worktree reproduced the known Customer `Microsoft.Data.SqlClient.SNI.dll` native-load failure; all final integration evidence was collected through the `R:` `subst` short path. Two B2B Concert fixture-reset SQL timeouts passed individually on fresh stacks and the complete project then passed 144/144.
- Static checks: plan graph and `git diff --check` green.

## Reviews

PR-B not yet reviewed.

## Decisions, discoveries, blockers, and deviations

- **`IReadDbContext` was added (#526), reversing the earlier "no interface" note.** Tommy required it; the shared read base was reworked so repos bind the interface, not the concrete context.
- **PR-B keeps `IBaseRepository`/`BaseRepository` — the plan's "delete them" is wrong.** `SequenceRepository<TSequence>` is keyless (`ISequence : ITenant`, not `IEntity<TKey>`), so it can only use the keyless write-only `BaseRepository`; `CollectionSyncer`/`OpportunitySyncer` also depend on the write-only `IBaseRepository`. So the write-only facet stays; the diamond dies by removing `GetAllAsync` from it instead.
- **Non-breaking analysis:** no consumer calls `IBaseRepository.GetAllAsync` (verified); `IRepository` still exposes `GetAllAsync` via `IReadRepository` (dropping `new` is source-compatible); `InsertAsync` is additive; `GetByIdAsync` stays `virtual` so the `ConcertReadRepository`/`CommissionBindingRepository` overrides survive the publish.
- **That analysis was incomplete and the PR is NOT non-breaking.** It reasoned only about *source* compatibility. Reparenting `Repository : ReadRepository` moves the inherited `context` field's declaring type off `BaseRepository` — a **binary** break. Feed-compiled consumers (`DealRepository : TenantScopedRepository : Repository`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (via `Seed.Infrastructure`'s source ProjectReference winning on MinVer) → `FieldAccessException`. Confirmed by run 31636765379. Source-compatibility ≠ binary-compatibility for a published base whose consumers touch inherited fields.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.
- **Current-main graph drift was reconciled:** Auth, B2B Conversations, and Payment are included in the local integration runner; its 13-project set now matches CI's dynamic discovery.
