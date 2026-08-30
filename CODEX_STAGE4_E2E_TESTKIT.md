# Stage 4: move cross-service E2E to a `fleet` TestKit

You are working alone with no memory of any prior conversation. This file is your complete brief.
Work independently; do not wait for further instructions unless you hit a genuine blocker.

## Context

`Concertable/concertable` is a .NET/React monorepo mid-migration to repository-per-microservice. Read
`plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` and its `_PROGRESS.md` ledger in this
checkout for full background before doing anything else.

The ledger's own words on this stage: **"Move cross-service E2E to `fleet` — 21 E2E cross-repository
edges, no TestKit exists yet. The largest stage; gates `fleet`, customer and b2b, but not auth or
payment."** It also says: *"Stages 3 and 4 run in parallel with the extractions, not in front of
them"* — you are not blocked by anything currently in flight, and other agents are concurrently folding
Customer's and B2B's frontends into their private staging repos; don't touch those paths or that work.

## What I actually want from you first

This stage has no TestKit today — that's the real gap, not a mechanical move. Before writing any test
code:

1. Find and enumerate the actual 21 cross-service E2E edges (search for however E2E test projects
   currently reference more than one service — likely `Concertable.*.E2ETests` projects or fixtures that
   spin up multiple services' hosts together). Confirm the count and list them; the ledger's number may
   be stale.
2. Understand what `fleet` is meant to be (see stage 8 in the same ledger: "Extract `platform-dotnet`,
   `platform-web`, `fleet`" — `fleet` is the future umbrella-host repository these E2E tests will run
   against once each service is its own repo).
3. Propose a TestKit design: how a cross-service E2E test declares which services it needs, how it boots
   them (still against `AddProject` in-repo today, since services aren't extracted yet — this has to work
   *now*, in the monorepo, without assuming extraction has happened), and how the same test structure
   will keep working once those services move to their own repos and `fleet` references them as packages/
   containers instead. Read `api/PlatformSourcePackages.targets` and the stage-3 image-mode work
   (`Plan/RepoSplit-Stage3-Hosting-rt2`, already merged/in-review as PR #870) for the precedent this stage
   is meant to follow at the AppHost tier — stage 4 is the equivalent move for E2E.

**Do not just start relocating test files.** Report the design first if it involves any non-trivial
choice (which I expect it will, given "no TestKit exists yet") — this is exploratory/design work, not a
mechanical carve like the backend extractions.

## Do not

- Do not touch `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md` — a separate process
  owns folding results into that shared ledger.
- Do not touch Customer's or B2B's frontend fold work (separate concurrent tasks, separate disposable
  clones outside this checkout).
- Do not touch Stage 3's remaining round-trips (rt2/rt3/rt4) — separate, sequential work.

## Stop and report

If you hit a design decision you're not confident is right, or the 21-edge count doesn't match what you
find, stop and describe exactly what you found and the options you're weighing. Otherwise report: the
actual edge count and list, the TestKit design you're proposing (or already partially built), and what
verification you ran.
