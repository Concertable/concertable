# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: `.worktrees/Refactor-launch_postgres-migration`
- Branch: `Refactor/launch_postgres-migration`
- PR: Phase 2 producer PR not opened; Phase 1 [#985](https://github.com/Concertable/concertable/pull/985)
  and generated platform sync [#988](https://github.com/Concertable/concertable/pull/988) are merged
- Dependency/package gates: Phase 2 adds a public API to `Concertable.DataAccess.Infrastructure`; publish
  and platform-sync must complete before the prepared service consumers can enter exact-head CI.
- Last reconciled: `2026-09-10` against `754f62e57` (`origin/main` at worktree creation)

## Current state

Phase 1 is delivered. Phase 2 is implemented and locally verified as a publish-first cut-over: the shared
DataAccess package owns `HasGeographyColumn`, all eight B2B, Customer, and Search mappings consume it, and
all initial migrations have been re-scaffolded. The package expansion must publish before the prepared
consumer commit can run exact-head PR CI against the real feed version.

## Next Steps

1. Commit, review, push, and run exact-head CI for the producer-only
   `Concertable.DataAccess.Infrastructure` package expansion.
2. After that PR publishes and the generated platform sync lands, rebase the prepared consumer commit onto
   current `main` and replace its temporary local-package proof with the published version.
3. Push the consumer PR and run exact-head CI for the complete affected SQL Server integration matrix and
   spatial-query coverage. Mark Phase 2 complete only when that consumer PR is delivered.

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
- Prepared the Phase 2 spatial seam: one `HasGeographyColumn` extension in
  `Concertable.DataAccess.Infrastructure` and eight migrated B2B, Customer, and Search mappings, with entity
  and query code unchanged on NetTopologySuite.
- Re-scaffolded all 24 initial-migration contexts. Messaging Outbox, Messaging Inbox, and Auth persisted
  grants were byte-identical and retained their IDs; the other 21 retained identical migration operations
  under new IDs while incorporating the already-published Phase 1 Inbox max-length snapshot metadata.

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
- Local package `0.1.0-local.1789064146595`: full solution build completed with 0 errors; four unrelated
  existing Auth/E2E warnings remained.
- Normalized comparison of all 21 regenerated migration bodies: 0 SQL Server operation differences;
  designers and snapshots differ only by the Phase 1 Inbox `HasMaxLength(450)` metadata.
- `Concertable.DataAccess.UnitTests`: 31 passed; `Concertable.Search.UnitTests`: 14 passed, including the
  geometry specification coverage.
- Exact-local-package architecture suites: B2B 22 passed, Customer 1 passed, Search 7 passed.

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
- NetTopologySuite remains the canonical CLR geometry model for both SQL Server spatial support and
  Npgsql/PostGIS; the provider changes its EF integration and database implementation, not entity/query
  geometry types.
- Phase 2 requires a publish-first package expansion even though the API addition is compatible: the eight
  changed consumers cannot compile against their currently pinned package because production projects do
  not source-swap `Concertable.DataAccess.Infrastructure`.

## Downstream handoffs

- Waiting plan: `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PROGRESS.md`.
  Gate: its Phase 4 SQL Server block-predicate helpers must be rewritten as Postgres policies plus a
  `BEFORE UPDATE` trigger during this plan's Phase 9 B2B cut-over. The seal plan is not blocked by this
  one and must not wait for it; this entry exists so the B2B cut-over does not silently drop the
  write-block when it regenerates B2B's migrations.
