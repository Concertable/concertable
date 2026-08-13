# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/data-access_base-unify`
- Branch: `Refactor/data-access_base-unify`
- PR: PR-B #530 — https://github.com/Concertable/concertable/pull/530 (open). Scope: seam fix + composed repository facets + `IWriteRepository` rename, all this PR. (PR-A #522 merged; Customer IReadDbContext #526 merged.)
- Dependency/package gates: PR-B is publish-first (ships `Concertable.DataAccess.*`) → on merge, publish + a `chore/platform-sync-*` PR rebuild every consumer against the new package. That sync PR is the real cross-consumer test.
- Last reconciled: 2026-08-13 — work-head push `190674ea5..276e191e4` is verified at local, remote-tracking, and PR #530 head; exact-head PR CI is running. Phase 5 delivery is in progress.

## Current state

PR-A (#522) and Customer IReadDbContext (#526) are merged + platform-sync green. **PR-B (#530): Phases 1–4 are locally complete, current `origin/main` is merged, review is clean through `f133bbefb`, and candidate `276e191e4` is published to the PR. Exact-head PR CI is running.** Shared `ReadRepository` and `WriteRepository` own read and write behavior once; the flat `Repository` facade composes both over the same tracked module context and contains only delegates. All three use explicit constructors.

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

**History (why the earlier revert mattered):** the reparent `Repository : ReadRepository` was binary-breaking — `context` moved off the former write base; feed-compiled consumers (`DealRepository → TenantScopedRepository → Repository`, `ConcertablePlatformVersion 0.1.0-alpha.0.955`) emit `ldfld` against the former write base's `context` field; the integration host loads the source-built new base (`Seed.Infrastructure` source ProjectReference, higher MinVer wins) → `FieldAccessException`, 6 suites (run 31636765379). Phase 1 fixed that mixed-version test seam. The replacement design now removes duplicated behavior through composition instead of reparenting.

## Next Steps

Complete Phase 5 delivery: require exact-head PR CI green on candidate `276e191e4`, then merge PR-B #530 through `/merge` with `skip-e2e` (no positive trigger). Own `publish-packages` and the resulting `chore/platform-sync-*` PR to a green merge, then close out the plan from a fresh docs worktree.

## Completed work

- **PR-A** (#522, merged `da9d02c29`, sync green): Customer read-only no-tracking contexts — shared `ReadDbContext` base + `{Concert,Venue,Artist}ReadDbContext` (NoTracking, `SaveChanges` throws); read repos rebound off `Query`.
- **#526** (merged `6a3d66677`, sync green): `IReadDbContext` — read repos depend on a queryable-only interface (`IQueryable<T> Query<T>()`), no `DbSet`/`Add`/`SaveChanges` reachable; DI injects each concrete read context as `IReadDbContext` via a factory.
- **Phase 1 — seam fix:** the test and CI harnesses pack the source platform once, override every consumer pin to that version, and assert integration/E2E outputs contain exactly one `Concertable.DataAccess.Infrastructure.dll` at the expected version. The normal publish workflow and committed service pins are unchanged.
- **Phase 2 — composed repository facets:** shared `IReadDbContext` and read behavior moved to DataAccess; `Repository` delegates to the shared read/write facets over one tracked context; Customer Concert/Venue/Artist inherit the shared read facet with their dedicated no-tracking contexts; the duplicate Customer contract/read base are deleted. Focused behavior and architecture tests enforce the context and uniqueness invariants.
- **Phase 3 — write-facet rename:** the public write contract and shared implementation are `IWriteRepository`/`WriteRepository`; module aliases, keyless sequence persistence, syncers, identifiers, tests, conventions, plan material, and the existing review artifact use the new vocabulary. The whole-repository case-insensitive content and path grep for the legacy names is zero.
- **Phase 4 — verification:** local platform `0.1.0-local.1786642862582` packed 40/40 projects; the Release solution build succeeded with 0 errors; 23/23 unit projects passed 1,075 tests; 16/16 integration projects passed 407 tests. Every integration output contained exactly one DataAccess assembly at `0.1.0-local.1786642862582+dddff8c7902d7ebce546270f030ef36d4b56f20b`.
- **Phase 5 — work-head push:** starting remote/PR head `190674ea5`; pushed range `190674ea5..276e191e4`; fetched remote-tracking and PR `headRefOid` both verified equal to work head `276e191e4`. Exact-head CI is running.

## Verification

- Plan-managed work push: local `HEAD`, `origin/Refactor/data-access_base-unify`, and PR #530 `headRefOid` all equalled `276e191e4fd9b1e245d25869d3063561b77731f4` after fetch; the prior remote/PR head was `190674ea520feca593ab9327b051325c87202054`.
- Post-merge candidate `c143c15c6`: `git rev-list --count HEAD..origin/main` returned 0; repository-wide case-insensitive legacy-name content/path grep returned zero outside the plan/ledger historical allowlist; `git diff --check origin/main...HEAD` passed; plan graph reported 0 errors and 0 warnings; `Concertable.DataAccess.UnitTests` passed 4/4 in Release.
- Review fix `6914b9baf`: `scripts/unit.ps1 list` reported the added DataAccess group and `git diff --check` passed.
- Current-main candidate `f133bbefb`: repository-wide legacy-name content/path grep returned zero outside the plan/ledger historical allowlist; `git diff --check origin/main...HEAD` passed; plan graph reported 0 errors and 0 warnings; `Concertable.DataAccess.UnitTests` passed 4/4 in Release.
- Phase 3/4 candidate in this commit: local platform `0.1.0-local.1786642862582` packed 40/40 projects; Release solution build succeeded with 0 errors; 23/23 dynamically discovered unit projects passed (1,075 tests); 16/16 dynamically discovered integration projects passed (407 tests). Every integration output contained exactly one `Concertable.DataAccess.Infrastructure.dll` at `0.1.0-local.1786642862582+dddff8c7902d7ebce546270f030ef36d4b56f20b`.
- Focused DataAccess behavior/architecture tests passed 4/4. The whole-repository legacy-name content/path grep, plan graph, and `git diff --check` are green.
- The initial Release restore exposed B2B's stale Reunion alpha-1 pin after the freshly packed Payment client required alpha 3. `integration-debug` traced it to upstream commit `7738f954e`; applying that exact one-line alignment made the full build and both test matrices green.
- The deep worktree requires the documented `subst` short path for Customer integration tests because native `Microsoft.Data.SqlClient.SNI.dll` loading otherwise exceeds Windows path limits.

## Reviews

Full PR-B `/review` (low/high-confidence threshold) covered `2dfe09cc9..6914b9baf` (20 commits) in `reviews/Refactor-data-access_base-unify.md`, including the security-sensitive workflow/package lens. One low native finding (DataAccess missing from `unit.ps1 list`/help) was fixed in `6914b9baf` and verified. Incremental review `6914b9baf..f133bbefb` covered the latest platform-sync merge and found no issues; no open findings remain.

## Decisions, discoveries, blockers, and deviations

- **Customer `IReadDbContext` was added (#526), reversing the earlier "no interface" note.** Tommy required it; Customer's read base was reworked so repositories bind the interface, not the concrete context. Phase 2 promoted that contract to shared DataAccess so the generic read behavior is shared.
- **The inherited-read/copied-write design was rejected after `d65293cc3`.** C#'s lack of class multiple inheritance does not justify duplicating behavior; `Repository` composes the read and write implementations and retains only flat one-line delegates.
- **Dedicated read repositories inherit; combined repositories compose.** Customer `ConcertReadRepository` is read-only and receives `ConcertReadDbContext`. A combined `IRepository` receives one tracked module context for both facets so read-mutate-save remains one unit of work. `TContext` in `WriteRepository` names the context used for writes; the context itself may also support tracked reads.
- **PR-B keeps the write-only facet — the plan's "delete them" is wrong.** `SequenceRepository<TSequence>` is keyless (`ISequence : ITenant`, not `IEntity<TKey>`), so it can only use the keyless write-only `WriteRepository`; `CollectionSyncer`/`OpportunitySyncer` also depend on the write-only `IWriteRepository`. So the write-only facet stays; the diamond dies by removing `GetAllAsync` from it instead.
- **Non-breaking analysis:** no consumer calls the former write-facet `GetAllAsync` (verified); `IRepository` still exposes `GetAllAsync` via `IReadRepository` (dropping `new` is source-compatible); `InsertAsync` is additive; `GetByIdAsync` stays `virtual` so the `ConcertReadRepository`/`CommissionBindingRepository` overrides survive the publish.
- **That analysis was incomplete and the PR is NOT non-breaking.** It reasoned only about *source* compatibility. Reparenting `Repository : ReadRepository` moves the inherited `context` field's declaring type off the former write base — a **binary** break. Feed-compiled consumers (`DealRepository : TenantScopedRepository : Repository`) emit `ldfld` against the former write base's `context` field; the integration host loads the source-built new base (via `Seed.Infrastructure`'s source ProjectReference winning on MinVer) → `FieldAccessException`. Confirmed by run 31636765379. Source-compatibility ≠ binary-compatibility for a published base whose consumers touch inherited fields.
- **Tech debt logged** (`api/Concertable.DataAccess/TECH_DEBT.md`): seal `GetByIdAsync` (remove `virtual`); and the pending duplicate-aware insert must hoist as a distinct name (e.g. `TryInsertAsync`) now that plain `InsertAsync` exists.
- **Current-main graph drift was reconciled:** the dynamically discovered gate is now 23 unit and 16 integration projects, including Auth, B2B Conversations, Payment, and the new DataAccess unit project.
