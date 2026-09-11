# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: none; the Phase 3 delivery worktree is closed
- Branch: next proposed `Refactor/launch_postgres-test-harness-seam`
- PR: Phase 3 consumer [#1019](https://github.com/Concertable/concertable/pull/1019) merged as
  `723886449d6498c11d418087820d2385c4e4acdd`
- Dependency/package gates: delivered. `Concertable.Seed.Shared` and
  `Concertable.Testing.Integration` `0.2.0-alpha.0.5` are published and consumed by the monorepo.
- Last reconciled: `2026-09-12` against merge commit `723886449`

## Current state

Phases 1-3 are delivered. Shared mappings and fixture APIs are provider-neutral while every service still
runs on SQL Server. Auth preserves explicit Duende operational-store identity values through the shared
identity-window helper; ordinary SQL remains ordinary SQL. B2B Booking and Concert fixtures use the shared
provider-aware temporary check-constraint helpers. No service provider cut-over has begun.

## Next Steps

Implement Phase 4 from current `origin/main` in a fresh worktree: make each service's Testcontainers image
and Respawn `DbAdapter` selectable through one test-harness seam with SQL Server as the default, add focused
tests for the selection behavior, and run the complete SQL Server integration matrix. Do not cut any service
over to Postgres during Phase 4.

## Completed work

- Phase 1 removed explicit SQL Server string types, preserved the SQL Server schema, landed in
  [#985](https://github.com/Concertable/concertable/pull/985), and completed package synchronization through
  [#988](https://github.com/Concertable/concertable/pull/988).
- Phase 2 introduced the shared `HasGeographyColumn` seam, published it through monorepo producer
  [#997](https://github.com/Concertable/concertable/pull/997) and
  [platform-dotnet#2](https://github.com/Concertable/platform-dotnet/pull/2), then migrated all eight spatial
  mappings and re-scaffolded initial migrations in [#1007](https://github.com/Concertable/concertable/pull/1007).
- Phase 3 added provider-aware identity and temporary check-constraint APIs through monorepo producer
  [#1017](https://github.com/Concertable/concertable/pull/1017) and canonical producer
  [platform-dotnet#3](https://github.com/Concertable/platform-dotnet/pull/3), published platform train
  `0.2.0-alpha.0.5`, and migrated the Auth and B2B consumers in
  [#1019](https://github.com/Concertable/concertable/pull/1019).

## Verification

- Platform producer exact-head run
  [34630847352](https://github.com/Concertable/platform-dotnet/actions/runs/34630847352) passed; publish run
  [34631131950](https://github.com/Concertable/platform-dotnet/actions/runs/34631131950) published
  `0.2.0-alpha.0.5`.
- Against one exact local platform closure, Auth passed 54/54 integration tests and all 12 B2B integration
  projects passed; the changed Booking and Concert projects passed 24/24 and 72/72. Feed-only `.5` builds for
  the Auth fixture and both B2B fixture projects completed with 0 errors.
- Consumer exact-head run
  [34643905173](https://github.com/Concertable/concertable/actions/runs/34643905173) passed with 89 successful
  jobs and 5 intentional skips.
- The first merge-group run's lone browser timeout did not reproduce: the exact scenario passed 1/1 on a
  fresh isolated local stack without a code change. Replacement merge-group run
  [34653730142](https://github.com/Concertable/concertable/actions/runs/34653730142) passed the complete queue
  gate, including B2B API and browser E2E.
- Post-merge package run
  [34655090916](https://github.com/Concertable/concertable/actions/runs/34655090916), image run
  [34655090898](https://github.com/Concertable/concertable/actions/runs/34655090898), and main CI run
  [34655091011](https://github.com/Concertable/concertable/actions/runs/34655091011) passed at exact merge
  commit `723886449d6498c11d418087820d2385c4e4acdd`.

## Reviews

- Phase 3 consumer canonical full and incremental reviews approved commit
  `09ed3bed386212040382fe2d9579e38d586be210` with no findings. The spent review work order was removed after
  merge.

## Decisions, discoveries, blockers, and deviations

- There is no production data, so no dual-provider abstraction, ETL, backfill, or compatibility window is
  warranted. Each service owns its database and can cut over independently.
- Phases 1-4 keep SQL Server as the active provider. Provider-neutral seams centralize the temporary
  differences needed by later service cut-overs; they do not introduce runtime dual-provider support.
- Only inserts that preserve explicit identity values use the identity-window helper. Ordinary seed and
  fixture SQL remains unwrapped.
- NetTopologySuite remains the canonical CLR geometry model for both SQL Server and PostGIS.
- The B2B concurrency-token choice between Npgsql `xmin` and an explicit `bigint` remains owned by Phase 9;
  it must not be pulled into the Phase 4 harness seam.
- Deep Windows worktree paths can exceed `Microsoft.Data.SqlClient.SNI.dll`'s native loader limit. Use a
  temporary short drive mapping when a local SQL Server integration run fails at native load before test
  execution.

## Downstream handoffs

- Waiting plan: `plans/launch/DEAL_CONFIGURATION_PROGRESS.md`.
  Gate: after Phase 9's B2B provider cut-over is delivered, reconcile its Phase 2 hybrid `jsonb` revision
  persistence against the landed mappings.
- Waiting plan: `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PROGRESS.md`.
  Gate: rewrite its Phase 4 SQL Server block-predicate helpers as Postgres policies plus a `BEFORE UPDATE`
  trigger during this plan's Phase 9 B2B cut-over.
