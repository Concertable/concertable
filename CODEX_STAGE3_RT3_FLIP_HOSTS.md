# Stage 3 rt3: flip the hosts

You are working alone with no memory of any prior conversation. This file is your complete brief.
Work independently; do not wait for further instructions unless you hit a genuine blocker.

## Hard prerequisite — check this first, before anything else

This round-trip consumes packages published by Stage 3 **rt2** (PR #870, "the hosting seam" —
`AppHost.Shared` gains the container-resource primitive, each `*.Hosting` gains an image-mode overload,
`Search.Hosting`/`Frontend.Hosting` become packable). **Do not start real work until you've confirmed
rt2 is merged to `main` and its packages are actually published to the feed** — publish-before-consume is
the whole reason these round-trips are split. If rt2 isn't merged yet, stop here and report that instead
of guessing at package versions that don't exist yet.

## Context

`Concertable/concertable` is a .NET/React monorepo mid-migration to repository-per-microservice. Read
`plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` and its `_PROGRESS.md` ledger for full
background — Stage 3's "Next Steps" section spells out rt2/rt3/rt4 in order.

## What "done" looks like

- AppHost csprojs consume `*.Hosting` as **packages**, not project references — extend
  `api/PlatformSourcePackages.targets` from the test tier (where it already exists) to the AppHost tier.
  `EnforceServiceBoundary`'s exemption set is already exactly `.AppHost` + tests, so mirror that same
  condition rather than inventing a new one.
- Foreign deployables (an AppHost's `AddProject` on a **sibling service**, not its own) become
  `AddContainer` on a pinned image digest, using the primitive rt2 added to `AppHost.Shared` and the
  image-mode overloads rt2 added to each `*.Hosting`.
- This closes the **44 apphost cross-target edges** the ledger tracks (26 `AppHost → *.Hosting`-class,
  18 `AppHost → foreign deployable`-class — see the ledger's "Stage 3's measured shape" table for the
  exact breakdown).
- `python eng/repository-split/inventory.py --check` stays green throughout — this is the same gate rt1
  and rt2 both had to keep passing.

## Do not

- Do not start consuming packages that don't exist yet (see prerequisite above).
- Do not touch `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md` — a separate process
  owns folding results into that shared ledger.
- Do not touch the frontend fold work, Stage 4's TestKit work, or the Auth grants migration — all separate,
  concurrent, unrelated to this.

## Stop and report

If rt2 isn't merged/published yet, report that and stop. Otherwise report: which of the 44 edges you
closed, inventory-check result, and any edge that resisted the mechanical conversion (e.g. an AppHost
referencing something rt2's primitive doesn't cover).
