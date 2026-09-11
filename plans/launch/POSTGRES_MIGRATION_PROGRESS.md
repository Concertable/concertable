# Postgres migration progress

- Plan: `plans/launch/POSTGRES_MIGRATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/postgres-migration`
- Worktree: `.worktrees/Refactor-launch_postgres-migration`
- Branch: `Refactor/launch_postgres-migration`
- PR: Phase 3 consumer not opened; monorepo producer [#1017](https://github.com/Concertable/concertable/pull/1017)
  and canonical platform producer [Concertable/platform-dotnet#3](https://github.com/Concertable/platform-dotnet/pull/3)
  are merged
- Dependency/package gates: delivered. `Concertable.Seed.Shared` and `Concertable.Testing.Integration`
  `0.2.0-alpha.0.5` are published, and the prepared consumer advances every monorepo platform pin to that
  exact train.
- Last reconciled: `2026-09-11` against `9370ade8a` (`origin/main`)

## Current state

Phases 1 and 2 are delivered. The Phase 3 package APIs are merged in both the transitional monorepo source and
their canonical `Concertable/platform-dotnet` owner, and platform train `0.2.0-alpha.0.5` is published. The
consumer is rebased onto current main, advances all platform pins to `.5`, removes the remaining Auth and B2B
fixture SQL Server catalog/identity syntax, and passes the complete Auth and B2B SQL Server integration matrix.

## Next Steps

Complete the Phase 3 consumer:

1. Run the final current-pin SQL Server integration matrix and repository package/plan gates.
2. Review the rebased consumer candidate, push `Refactor/launch_postgres-migration`, and open its PR.
3. Own exact-head and merge-group validation to green, merge the PR, and record the Phase 4 handoff.

Do not begin a service cut-over during Phase 3.

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
- Added the producer-side Phase 2 spatial seam: one `HasGeographyColumn` extension in
  `Concertable.DataAccess.Infrastructure`.
- On the separate prepared consumer branch, migrated eight B2B, Customer, and Search mappings with entity
  and query code unchanged on NetTopologySuite, then re-scaffolded all 24 initial-migration contexts.
  Messaging Outbox, Messaging Inbox, and Auth persisted grants were byte-identical and retained their IDs;
  the other 21 retained identical migration operations under new IDs while incorporating the
  already-published Phase 1 Inbox max-length snapshot metadata.
- Delivered the eight consumer mappings and re-scaffolded initial migrations in
  [#1007](https://github.com/Concertable/concertable/pull/1007), completing Phase 2.
- Added the Phase 3 provider dispatch in `Concertable.Seed.Shared` and the shared identity-window and
  temporary check-constraint API in `Concertable.Testing.Integration`.
- Delivered the transitional monorepo package source through #1017 and the canonical package source through
  `Concertable/platform-dotnet#3`, then published platform train `0.2.0-alpha.0.5`.
- Rebased the prepared consumer onto current main and advanced all eight monorepo platform-train pins to the
  exact published `.5` release.

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
- Prepared-consumer proof with local package `0.1.0-local.1789064146595`: full solution build completed with
  0 errors; four unrelated existing Auth/E2E warnings remained.
- Normalized comparison of all 21 regenerated migration bodies: 0 SQL Server operation differences;
  designers and snapshots differ only by the Phase 1 Inbox `HasMaxLength(450)` metadata.
- Producer `Concertable.DataAccess.UnitTests`: 32 passed, including direct geography-column metadata
  coverage; prepared-consumer `Concertable.Search.UnitTests`: 14 passed, including geometry specification
  coverage.
- Exact-local-package architecture suites: B2B 22 passed, Customer 1 passed, Search 7 passed.
- Producer PR #997 and platform PR #2 published `Concertable.DataAccess.Infrastructure`
  `0.2.0-alpha.0.4`; release-train PR #1004 made that published pin independently consumable and passed
  exact-head and merge-group validation before landing as `d6f986c25`.
- Rebased the prepared consumer commit onto `d6f986c25`; all eight production spatial mappings use
  `HasGeographyColumn`, leaving the cross-provider `geography` relational semantic in one shared seam.
- Published-feed restore passed, and the full Release solution build completed in 11m32s with 0 errors;
  the five warnings are existing Auth EF-version, nullable, and generated UI warnings.
- Rebased-consumer architecture suites passed: B2B 22, Customer 1, Search 7. Search unit tests passed 14,
  including geometry specification coverage.
- Consumer exact-head run `34597464789` passed the complete affected SQL Server integration matrix.
- Merge-group run `34598736363` passed the protected build plus B2B and Customer API/browser E2E and landed
  #1007 as `f7e31c26c`.
- Post-merge package run `34600419582`, image run `34600419579`, and main CI run `34600419772` passed at
  the exact merge commit; package verification restored the newly published service closure from a fresh consumer.
- Phase 3 producer: `Concertable.Seed.Shared.UnitTests` 20 passed; `Concertable.Testing.Integration` Release
  build passed with 0 warnings and 0 errors.
- Phase 3 local package `9999.0.0-local.1789143141361` packed the complete 58-package closure. SHA-256:
  `Concertable.Seed.Shared` `E33488E9A69A0A112889CB0481A41E05C53C4D31D50F7D6CE8968ADE249F6D8D`;
  `Concertable.Testing.Integration` `24FBD6114B64922366826A71A88D64EEB08D5E185901086991B9706F02ECDA13`.
- The prepared Auth consumer built with 0 errors and its operational-store migration tests passed 4/4,
  including identity preservation. The prepared B2B solution built with 0 errors; existing warnings remain.
- Prepared-consumer SQL Server integration tests passed against that exact package: Booking 3/3 and Concert
  13/13. Each run verified exactly one `Concertable.DataAccess.Infrastructure.dll` at the local package version.
- The prepared Auth and B2B fixture inventory now contains 0 raw `SET IDENTITY_INSERT` and 0
  `sys.check_constraints` occurrences outside migrations and build outputs.
- Platform PR #3 exact-head CI run `34630847352` passed both package build/test and build-law consumer jobs;
  merge commit `3136a4ee1` is published by run `34631131950` as `0.2.0-alpha.0.5`.
- Feed-only `.5` builds passed for the Auth fixture and the B2B Booking and Concert integration projects with
  0 warnings and 0 errors.
- The final current-pin Auth integration matrix passed 54/54 against local platform closure
  `9999.0.0-local.1789151042614`, including all four operational-store migration cases.
- The final current-pin B2B integration matrix passed all 12/12 projects against that same closure. The changed
  Booking and Concert fixture projects passed 24/24 and 72/72 respectively; every project verified exactly one
  `Concertable.DataAccess.Infrastructure.dll` at the local platform version.

## Reviews

- Canonical review approved with no findings through Phase 1 head `4eb4be854`; its spent work order was
  removed after PR #985 merged.
- Phase 2 consumer review found one documentation overclaim about active-provider dispatch. The plan now
  records the actual cross-provider `geography` seam, and native/correctness, test-impact/reliability, and
  documentation/security lenses approved candidate `3b7c1aca8`; review evidence landed in `b09511d1f`.
- Phase 3 monorepo producer review approved `b437240e9`; the platform review approved byte-identical package
  source at `ec79f388d`. The rebased consumer awaits its final canonical pass.

## Decisions, discoveries, blockers, and deviations

- No production data exists, so no dual-provider abstraction, ETL, or backfill is warranted. A
  compatibility layer would be permanent cost for a temporary problem.
- Per-service cut-over is possible and preferred; each service has its own database and Aspire resource.
- Prep phases must leave the SQL Server schema byte-identical. A schema diff in Phase 1 is a defect, not
  an acceptable side effect.
- The existing B2B concurrency token choice — Npgsql `uint` row-version mapping to `xmin` versus an
  explicitly maintained `bigint` — remains deferred to Phase 9, with rationale required. Modern
  PostgreSQL freezing preserves the original `xmin`; the previous contrary statement was incorrect.
  Business/configuration revision identity remains separate from either optimistic concurrency token.
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
- Phase 3 has the same publish-first shape: service test projects bind the published shared-test packages,
  so the provider APIs land and publish before Auth and B2B migrate their fixture call sites.
- Deep Windows worktree paths exceeded `Microsoft.Data.SqlClient.SNI.dll`'s native loader limit during the B2B
  diagnostic run. The identical tests passed through a temporary short drive mapping; this changes no repository
  or delivery behavior.
- The organization Renovate policy holds a new dependency release for three days. Phase 3 therefore advances
  the exact already-published `.5` platform train in its consumer PR rather than waiting for routine automation.
- `scripts/worktrees.ps1 close -PlanManaged` rejected the producer ledger's repository-relative worktree path
  even though `plans/AGENTS.md` prescribes that ledger form. Closing without `-PlanManaged` remained safe and
  succeeded; the contract mismatch is recorded in root technical debt.

## Downstream handoffs

- Waiting plan: `plans/launch/DEAL_CONFIGURATION_PROGRESS.md`.
  Gate: after Phase 9's B2B provider cut-over is delivered, reconcile that plan's Phase 2 hybrid
  `jsonb` revision persistence against the landed mappings. Its finite-language Phase 1 can proceed
  earlier; this provider migration must not wait for the configuration refactor, tenant entitlements
  or a builder. The new configuration aggregates use their own explicit `bigint` edit token.

- Waiting plan: `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PROGRESS.md`.
  Gate: its Phase 4 SQL Server block-predicate helpers must be rewritten as Postgres policies plus a
  `BEFORE UPDATE` trigger during this plan's Phase 9 B2B cut-over. The seal plan is not blocked by this
  one and must not wait for it; this entry exists so the B2B cut-over does not silently drop the
  write-block when it regenerates B2B's migrations.
