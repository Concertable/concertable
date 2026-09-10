# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_postgres-migration`
- Branch: `Refactor/launch_postgres-migration`
- PR: not opened
- Dependency/package gates: Phase 1 changes the published `Concertable.Messaging` and
  `Concertable.DataAccess.Infrastructure` packages and must publish plus platform-sync before any
  service cut-over consumes it. No other phase has a package gate.
- Last reconciled: `2026-09-09` against `d145503f6` (`origin/main`)

## Current state

Phase 1 is implemented and locally green on SQL Server. The five shared messaging mappings now use
provider-neutral length configuration, and only the two Messaging-owned initial migrations regenerated.
Every other owner context was byte-identical and retained its migration ID.

## Next Steps

Push the reviewed Phase 1 candidate and open a draft PR so exact-head CI runs the full affected SQL
Server integration matrix. Resolve every CI finding, merge the approved candidate, and own publication of `Concertable.Messaging` and
`Concertable.DataAccess.Infrastructure` plus the causally generated platform-sync PR to green/merged.

Do not begin a service cut-over in this worktree.

## Completed work

- Phase 1 replaced the five explicit SQL Server string types with provider-neutral EF configuration and
  re-scaffolded the two Messaging initial migrations without changing their SQL Server schema (`this commit`).
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
  integration matrix remains a PR CI gate.

## Reviews

- Canonical review approved with no findings; work order:
  `reviews/Refactor-launch_postgres-migration.md`.

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
