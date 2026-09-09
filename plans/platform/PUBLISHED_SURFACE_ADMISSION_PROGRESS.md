# Published surface admission progress

- Plan: `plans/platform/PUBLISHED_SURFACE_ADMISSION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/surface-admission`
- Worktree: none yet
- Branch: none yet
- PR: none yet
- Dependency/package gates: none for Phase 1. Phase 1's table is a hard input to
  `PLATFORM_RELEASE_TRAINS_PLAN.md` Phase 3.
- Last reconciled: 2026-09-09 against `origin/main` `081e149a2e0a4544e1c781adc5e322a6ee0e4f85`.

## Current state

Authored, not started. No verdicts assigned.

Inventory at the reconciled SHA: 55 `IsPackable` projects. Enumerate with
`grep -rl '<IsPackable>true</IsPackable>' api --include='*.csproj' | xargs -n1 basename | sed 's/\.csproj$//' | sort`
rather than trusting any copy of the list, which will rot.

## Completed milestones

None.

## Latest verification

None. Nothing implemented.

## Reviews

None recorded — nothing implemented to review.

## Decisions and discoveries

- The admission rule already exists in `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` and must be cited
  from there, never restated here.
- Roughly 13 of the 55 packages are `*.Contracts`; the remaining 42 are framework, adapters, test
  infrastructure and hosting. The framework half is what makes services move in lockstep.
- `api/Concertable.B2B/Directory.Packages.props` consumes `Concertable.Auth.Hosting`,
  `Concertable.Payment.Hosting` and `Concertable.Payment.TestKit`. Treat this as an open question for
  Phase 1, not as an established violation — the `*.Hosting` composition-time policy may permit it.
- Vendoring is a legitimate verdict, not a fallback. Do not let a DRY argument silently remove it from
  the option set for the `Shared.*` adapters.

## Downstream handoffs

- `plans/platform/PLATFORM_RELEASE_TRAINS_PROGRESS.md` — its Phase 3 cannot start until this plan's
  Phase 1 table assigns every package a train. Update and compact that ledger in the same session that
  lands the table.

## Next Steps

Produce the Phase 1 verdict table:

1. Enumerate the `IsPackable` projects with the command above; assert the count matches the rows you
   write.
2. For each, resolve its real consumers from the `Directory.Packages.props` of every service folder
   under `api/` — consumer count is evidence, not judgement, and the admission rule turns on "at least
   two legitimate product consumers".
3. Apply the admission rule from `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`, then the cost test,
   and record verdict + train property + post-cut owning repository + reason per row.
4. Answer the three named questions explicitly — the 12 `Shared.*` adapters, the eight
   `Testing.*`/`*.TestKit` packages, and B2B's sibling `*.Hosting`/`*.TestKit` consumption.
5. Land the table in the plan, then update
   `plans/platform/PLATFORM_RELEASE_TRAINS_PROGRESS.md` to record that its Phase 3 input exists.
