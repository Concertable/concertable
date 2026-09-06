# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: not created
- Branch: not created
- PR: not opened
- Dependency/package gates: Phase 1 changes the published `Concertable.Messaging` and
  `Concertable.DataAccess.Infrastructure` packages and must publish plus platform-sync before any
  service cut-over consumes it. No other phase has a package gate.
- Last reconciled: `2026-09-05` against `origin/main`

## Current state

Design approved by Tommy on 2026-09-05. No implementation has started.

The provider-specific surface has been measured against `main` rather than estimated, and is recorded in
the plan's §3 table. The decisive constraint is that **there is no production data**: the migration
happens before launch, so this is a provider swap with no dual-write window, no ETL, and no
compatibility layer.

Phases 1-4 are provider-neutral refactors that ship on SQL Server and are implementable today. Phases
5-9 flip one service at a time, which is possible because each service owns its own database,
connection string, and Aspire resource.

## Next Steps

Create a fresh worktree from current `origin/main` and implement Phase 1:

1. In `Concertable.Messaging` outbox and inbox entity configurations and in `DataAccess`
   `DbContextBase`, replace `HasColumnType("nvarchar(450)")` with `HasMaxLength(450)` and
   `HasColumnType("nvarchar(max)")` with an unbounded string property.
2. Re-scaffold initial migrations from `api/` via `./initial-migrations.ps1` and diff the generated SQL
   Server schema against the previous one — it must be identical. A schema difference means the mapping
   is not equivalent and must be corrected before proceeding.
3. Run the affected service integration suites on SQL Server; all must stay green.
4. Publish the two packages and land the generated platform sync before starting any service cut-over.

Do not begin a service cut-over in this worktree.

## Completed work

- Inventoried the provider-specific surface against `main`: 8 `SET IDENTITY_INSERT`, 2
  `sys.check_constraints` queries, 8 `geography` columns, 5 `nvarchar` column types, the
  `IsRowVersion()` concurrency token with 5 implementers, and 50 migration files.
- Established that the concurrency token is B2B-local rather than shared, which removes it from the
  shared prep phases and confines it to the B2B cut-over.
- Selected Search as the pilot service: three spatial read models, two migrations, no write surface.
- Recorded the decision to phase per service rather than as one flag day, on the evidence that each
  service owns its own database.

## Verification

- Migration file counts per service from `find api -path "*/Migrations/*.cs" -not -name "*ModelSnapshot.cs"`:
  Auth 4, B2B 24, Customer 14, Messaging 4, Payment 2, Search 2 — 50 total.
- `HasColumnType` inventory confirms 8 `geography` and 5 `nvarchar` sites outside migrations.
- SQL-Server-specific syntax in raw SQL is limited to `SET IDENTITY_INSERT` (8) and
  `sys.check_constraints` (2); no `MERGE`, `NOLOCK`, `GETUTCDATE`, `NEWSEQUENTIALID`, or query hints
  were found.
- `IConcurrencyVersioned` and `ConcurrencyVersionExtensions` are both under
  `api/Concertable.B2B/src/Concertable.B2B.DataAccess/`, confirming the token is B2B-scoped.

## Reviews

- None recorded; no implementation commit exists yet.

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
