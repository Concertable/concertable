# Published surface admission progress

- Plan: `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/surface-admission`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-PublishedSurfaceAdmission`
- Branch: `Docs/PublishedSurfaceAdmission`
- PR: none yet
- Dependency/package gates: none for Phase 1. Phase 1's table is a hard input to
  `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3.
- Last reconciled: 2026-09-11 against `origin/main` `d01a2a4857c10be8e1e9ca27f3b2e18543d2c38e`.

## Current state

Phase 1 is implemented locally. Every packable project has one binding verdict, train property or explicit
unpublished marker, post-cut owner, consumer count and reason. The table admits 35 platform or service-owned
packages and removes 23 packages from the future published surface through `vendor` verdicts.

Inventory at the reconciled SHA: 58 `IsPackable` projects, 16 of them `*.Contracts`. Enumerate with
`grep -rl '<IsPackable>true</IsPackable>' api --include='*.csproj' | xargs -n1 basename | sed 's/\.csproj$//' | sort`
rather than trusting any copy of the list, which will rot.

## Completed milestones

- Phase 1 binding verdict table completed for all 58 packable projects.
- The three required disposition groups are explicit: 13 `Shared.*` adapters, five `Testing.*` plus three
  `*.TestKit` packages, and sibling `*.Hosting`/`*.TestKit` consumption.

## Latest verification

- Table rows: 58; unique package IDs: 58; current `IsPackable` projects: 58.
- Consumer evidence includes both direct `PackageReference` edges and source-form `ProjectReference` edges.

## Reviews

Review pending for the completed Phase 1 table.

## Decisions and discoveries

- The admission rule already exists in `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` and must be cited
  from there, never restated here.
- 16 of the 58 packages are `*.Contracts`; the remaining 42 are framework, adapters, test
  infrastructure and hosting. The framework half is what makes services move in lockstep.
- `api/Concertable.B2B/Directory.Packages.props` consumes `Concertable.Auth.Hosting`,
  `Concertable.Payment.Hosting` and `Concertable.Payment.TestKit`. Treat this as an open question for
  Phase 1, not as an established violation — the `*.Hosting` composition-time policy may permit it.
  The related split-pin drift (`Concertable.Shared` pinning `Payment.Hosting` to the platform train) is
  recorded in `api/TECH_DEBT.md`; do not re-derive it here.
- Vendoring is a legitimate verdict, not a fallback. Do not let a DRY argument silently remove it from
  the option set for the `Shared.*` adapters.
- The live inventory has 13 `Shared.*` adapter packages, not the plan's stale count of 12. All 13 are
  vendored by capability family because their thin provider seams cost less to duplicate than coordinate.
- Internal-only B2B contracts, `Messaging.Application`, `Seed.Infrastructure`, `Concertable.Grpc`, and
  `Concertable.Testing.Unit` leave the published surface because they have fewer than two legitimate
  cross-repository consumers.
- Generic testing primitives remain platform-owned; the topology-bearing E2E harness moves to the System
  train; service TestKits remain on their owner trains for black-box system tests.
- Sibling Hosting and TestKit consumption is legitimate only at AppHost composition and test boundaries.

## Downstream handoffs

- `plans/platform/PLATFORM_RELEASE_TRAINS_PROGRESS.md` — its Phase 3 cannot start until this plan's
  Phase 1 table assigns every package a train. Update and compact that ledger in the same session that
  lands the table.

## Next Steps

1. Review and land the Phase 1 verdict table.
2. Execute each `vendor` class through its own follow-on plan; do not delete packages in this docs PR.
3. Use the admitted train membership to implement the consumer-property split in
   `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3.
