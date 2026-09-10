# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: not created for the next phase; the Phase 1 worktree is retired
- Branch: not created for the next phase
- PR: Phase 1 [#985](https://github.com/Concertable/concertable/pull/985) merged; generated
  platform sync [#988](https://github.com/Concertable/concertable/pull/988) merged
- Dependency/package gates: Phase 1 changes the published `Concertable.Messaging` and
  `Concertable.DataAccess.Infrastructure` packages and must publish plus platform-sync before any
  service cut-over consumes it. No other phase has a package gate.
- Last reconciled: `2026-09-10` against `a8205ffc5` (`origin/main`)

## Current state

Phase 1 is delivered. The five shared messaging mappings use provider-neutral length configuration, the
re-scaffolded SQL Server schema is equivalent, exact-head CI and the merge queue are green, and package
version `0.1.0-alpha.0.1366` is published and consumed through the merged platform-sync PR.

## Next Steps

Create a fresh worktree from current `origin/main` and implement Phase 2:

1. Replace the eight explicit `HasColumnType("geography")` mappings with the plan's single shared spatial
   configuration extension while keeping entity and query code on NetTopologySuite.
2. Re-scaffold every initial migration and prove the generated SQL Server schemas remain equivalent.
3. Run the focused local checks, then use exact-head PR CI for the complete affected SQL Server integration
   matrix and spatial-query coverage.

Do not begin a service cut-over during Phase 2.

## Completed work

- Phase 1 replaced the five explicit SQL Server string types with provider-neutral EF configuration,
  re-scaffolded the two Messaging initial migrations without changing their SQL Server schema, landed in
  [#985](https://github.com/Concertable/concertable/pull/985), published `0.1.0-alpha.0.1366`, and completed
  consumer synchronization through [#988](https://github.com/Concertable/concertable/pull/988).
- Inventoried the provider-specific surface against `main`: 8 `SET IDENTITY_INSERT`, 2
  `sys.check_constraints` queries, 8 `geography` columns, 5 `nvarchar` column types, the
  `IsRowVersion()` concurrency token with 5 implementers, and 50 migration files.
- Established that the concurrency token is B2B-local rather than shared, which removes it from the
  shared prep phases and confines it to the B2B cut-over.
- Selected Search as the pilot service: three spatial read models, two migrations, no write surface.
- Recorded the decision to phase per service rather than as one flag day, on the evidence that each
  service owns its own database.

## Verification

- `./initial-migrations.ps1`: all owner contexts scaffolded successfully; every non-Messaging context was
  byte-identical, and normalized `CreateTable` comparisons prove both regenerated Messaging schemas equivalent.
- `Concertable.Messaging.UnitTests`: 45 passed.
- `Concertable.DataAccess.UnitTests`: 31 passed.
- `Concertable.DataAccess.IntegrationTests`: 19 passed on SQLite; the exact-head SQL Server service
  integration matrix passed in PR CI run `34421517660` (93 jobs).
- Merge-group run `34451099204` passed and landed Phase 1 as `45e41f648`.
- Publish run `34452515712` published and restored the complete 58-package closure at
  `0.1.0-alpha.0.1366`; generated sync PR #988 and its cascade-guarded follow-on publication both passed.

## Reviews

- Canonical review approved with no findings through Phase 1 head `4eb4be854`; its spent work order was
  removed after PR #985 merged.

## Decisions, discoveries, blockers, and deviations

- No production data exists, so no dual-provider abstraction, ETL, or backfill is warranted. A
  compatibility layer would be permanent cost for a temporary problem.
- Per-service cut-over is possible and preferred; each service has its own database and Aspire resource.
- Prep phases must leave the SQL Server schema byte-identical. A schema diff in Phase 1 is a defect, not
  an acceptable side effect.
- The concurrency token choice — Npgsql `UseXminAsConcurrencyToken()` versus a hand-maintained `bigint` —
  is deliberately deferred to Phase 9 and must be recorded with its rationale before implementation.
  `xmin` needs no schema change but is rewritten by `VACUUM FREEZE`.
- Postgres RLS `USING` on UPDATE filters rather than errors, so the seal enforcement plan's proof test
  must assert on the row rather than the exception type. That constraint is already written into
  `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PLAN.md` Phase 4.
- Migration cost compounds with schema churn: 50 migration files and 8 spatial columns today, growing
  with every feature. This is the argument for starting prep now rather than at the launch deadline.

## Downstream handoffs

- Waiting plan: `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PROGRESS.md`.
  Gate: its Phase 4 SQL Server block-predicate helpers must be rewritten as Postgres policies plus a
  `BEFORE UPDATE` trigger during this plan's Phase 9 B2B cut-over. The seal plan is not blocked by this
  one and must not wait for it; this entry exists so the B2B cut-over does not silently drop the
  write-block when it regenerates B2B's migrations.
