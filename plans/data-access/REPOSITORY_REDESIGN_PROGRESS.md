# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B #530 — https://github.com/Concertable/concertable/pull/530 (open). Scope: seam fix + composed repository facets + `IWriteRepository` rename, all this PR. (PR-A #522 merged; Customer IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-13 — Phase 2 composition is complete and verified in this commit. Branch includes current `origin/main` (`491890ec9`).

## Current state

PR-A (#522) and Customer IReadDbContext (#526) are merged + platform-sync green. **PR-B (#530): Phase 2 is complete; Phase 3 is next.** Shared `ReadRepository` and `BaseRepository` own read and write behavior once; the flat `Repository` facade composes both over the same tracked module context and contains only delegates. All three use explicit constructors.

Customer Concert grounds the context boundary: `ConcertModule` consumes only `IConcertReadRepository`; `ConcertReadRepository` directly inherits a read base and DI passes `ConcertReadDbContext`. Projection handlers separately use tracked `ConcertDbContext`. Combined repositories are tracked units of work and give one writable module-context instance to both composed facets; they do not hide a separate no-tracking context behind `IRepository`.

The shared `IReadDbContext.Query<TEntity>()` implementation belongs once on `DbContextBase` and returns
`Set<TEntity>()` directly; `DbSet<TEntity>` implicitly converts to `IQueryable<TEntity>`, so neither
`.AsQueryable()` nor a `ReadDbContextView` wrapper belongs in the final design. The migration is terminal
only when the duplicate Customer contract/read base and every transitional parallel abstraction are gone. Phase 2 removed those duplicates and added architecture tests that enforce the single shared read contract and implementation plus a query-only read capability surface.

**Phase 1 is complete.** `.github/workflows/test.yml` and
`scripts/{local-platform,integration,unit,e2e,test}.ps1` now enforce the seam. `local-platform.ps1` packs the 40 production
`IsPackable` projects at one unique MinVer override, emits a mapped local NuGet config, restores/builds/
tests consumers against that version, and verifies integration/E2E outputs contain exactly one
`Concertable.DataAccess.Infrastructure.dll` at that local version. CI packs once and shares the artifact
with build, carve, unit, integration, and any enabled E2E jobs; local unit/integration/E2E runners prepare
and consume the same feed. Publishing and committed service pins are unchanged.

**History (why the earlier revert mattered):** the reparent `Repository : ReadRepository` was binary-breaking — `context` moved off `BaseRepository`; feed-compiled consumers (`DealRepository → TenantScopedRepository → Repository`, `ConcertablePlatformVersion 0.1.0-alpha.0.955`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (`Seed.Infrastructure` source ProjectReference, higher MinVer wins) → `FieldAccessException`, 6 suites (run 31636765379). Phase 1 fixed that mixed-version test seam. The replacement design now removes duplicated behavior through composition instead of reparenting.

## Next Steps

1. Execute Phase 3's full repository-wide rename: `IBaseRepository`/`BaseRepository` to `IWriteRepository`/`WriteRepository`, including module aliases, keyless `SequenceRepository`, syncers, DI registrations, identifiers, tests, docs, and conventions. Preserve the composed Phase 2 architecture and flat `IRepository` API.
2. Run the whole-repository case-insensitive grep gate for `ibaserepository|baserepository`. The only permitted survivors are historical references in this plan and ledger; explicitly enumerate them or update the prose so every other occurrence is zero.
3. Prepare one fresh local platform version. Build `api/Concertable.slnx` in Release and run all dynamically discovered unit and integration projects against it; every integration output must contain exactly one DataAccess assembly at that version. Use the `subst` short path for integration and enter `integration-debug` for any red test.
4. Update the plan and ledger, commit the verified Phase 3 rename, and stop at the phase boundary.

## Completed work

- **PR-A** (#522, merged `da9d02c29`, sync green): Customer read-only no-tracking contexts — shared `ReadDbContext` base + `{Concert,Venue,Artist}ReadDbContext` (NoTracking, `SaveChanges` throws); read repos rebound off `Query`.
- **#526** (merged `6a3d66677`, sync green): `IReadDbContext` — read repos depend on a queryable-only interface (`IQueryable<T> Query<T>()`), no `DbSet`/`Add`/`SaveChanges` reachable; DI injects each concrete read context as `IReadDbContext` via a factory.
- **Phase 1 — seam fix:** the test and CI harnesses pack the source platform once, override every consumer pin to that version, and assert integration/E2E outputs contain exactly one `Concertable.DataAccess.Infrastructure.dll` at the expected version. The normal publish workflow and committed service pins are unchanged.
- **Phase 2 — composed repository facets:** shared `IReadDbContext` and read behavior moved to DataAccess; `Repository` delegates to the shared read/write facets over one tracked context; Customer Concert/Venue/Artist inherit the shared read facet with their dedicated no-tracking contexts; the duplicate Customer contract/read base are deleted. Focused behavior and architecture tests enforce the context and uniqueness invariants.

## Verification

- Phase 2 candidate in this commit: local platform `0.1.0-local.1786635601210` packed 40/40 projects; Release solution build succeeded with 0 errors; 23/23 dynamically discovered unit projects passed (1,048 tests); 16/16 dynamically discovered integration projects passed (406 tests), including the six historical proofs. Every integration output contained exactly one `Concertable.DataAccess.Infrastructure.dll` at `0.1.0-local.1786635601210+5197f63b74ad812ed01b81eea380d067fedeb40a`.
- Focused gates: DataAccess behavior tests 4/4 and repository architecture tests 2/2 passed. Plan graph and `git diff --check` are green.
- The deep worktree requires the documented `subst` short path for Customer integration tests because native `Microsoft.Data.SqlClient.SNI.dll` loading otherwise exceeds Windows path limits.

## Reviews

PR-B not yet reviewed.

## Decisions, discoveries, blockers, and deviations

- **Customer `IReadDbContext` was added (#526), reversing the earlier "no interface" note.** Tommy required it; Customer's read base was reworked so repositories bind the interface, not the concrete context. Phase 2 promoted that contract to shared DataAccess so the generic read behavior is shared.
- **The inherited-read/copied-write design was rejected after `d65293cc3`.** C#'s lack of class multiple inheritance does not justify duplicating behavior; `Repository` composes the read and write implementations and retains only flat one-line delegates.
- **Dedicated read repositories inherit; combined repositories compose.** Customer `ConcertReadRepository` is read-only and receives `ConcertReadDbContext`. A combined `IRepository` receives one tracked module context for both facets so read-mutate-save remains one unit of work. `TContext` in `WriteRepository` names the context used for writes; the context itself may also support tracked reads.
- **PR-B keeps `IBaseRepository`/`BaseRepository` — the plan's "delete them" is wrong.** `SequenceRepository<TSequence>` is keyless (`ISequence : ITenant`, not `IEntity<TKey>`), so it can only use the keyless write-only `BaseRepository`; `CollectionSyncer`/`OpportunitySyncer` also depend on the write-only `IBaseRepository`. So the write-only facet stays; the diamond dies by removing `GetAllAsync` from it instead.
- **Non-breaking analysis:** no consumer calls `IBaseRepository.GetAllAsync` (verified); `IRepository` still exposes `GetAllAsync` via `IReadRepository` (dropping `new` is source-compatible); `InsertAsync` is additive; `GetByIdAsync` stays `virtual` so the `ConcertReadRepository`/`CommissionBindingRepository` overrides survive the publish.
- **That analysis was incomplete and the PR is NOT non-breaking.** It reasoned only about *source* compatibility. Reparenting `Repository : ReadRepository` moves the inherited `context` field's declaring type off `BaseRepository` — a **binary** break. Feed-compiled consumers (`DealRepository : TenantScopedRepository : Repository`) emit `ldfld BaseRepository::context`; the integration host loads the source-built new base (via `Seed.Infrastructure`'s source ProjectReference winning on MinVer) → `FieldAccessException`. Confirmed by run 31636765379. Source-compatibility ≠ binary-compatibility for a published base whose consumers touch inherited fields.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.
- **Current-main graph drift was reconciled:** the dynamically discovered gate is now 23 unit and 16 integration projects, including Auth, B2B Conversations, Payment, and the new DataAccess unit project.
