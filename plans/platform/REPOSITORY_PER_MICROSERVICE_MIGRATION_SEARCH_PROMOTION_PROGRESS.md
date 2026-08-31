# Repository-per-microservice migration — Search promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: reserved `C:\Users\tommy\source\repos\search-next`; verify absence before cloning exactly once
- Branch: next proposed `Chore/search-promotion-preparation`
- PR: none; no open `Concertable/search-next` PR exists
- Dependency/package gates: implementation is unblocked from private `search-next` `main` `befe816afe8d1a75aeb7ba31a80e492e9d48c83b`; final checkpoint 12 delivery is ordered and requires explicit authorization
- Last reconciled: **2026-08-31** from GitHub repository and PR state

## Current state

Private `Concertable/search-next` exists with its extraction proof and no open PR. This reserved stream owns
only checkpoint-12 Search repository preparation: Search runtime, migrations, Hosting/TestKit, standalone
AppHost, CI, package/image publication setup, seed convergence gates, and repository evidence. It must not
edit RT3, Stage 4, Auth-next, Customer-next, Payment-next, or shared execution ledgers.

The target currently has no `.github` workflows. Web, Workers, UnitTests, and IntegrationTests are
package-clean; the whole solution is not. AppHost retains foreign Auth/B2B/AppHost.Shared source and foreign
database resources, ArchitectureTests consume that AppHost, and the inherited E2E Helper retains foreign
source. Search.Hosting exists but is non-packable; no TestKit or migration job/bundle exists.

State: **reserved to one Search preparation owner; implementable, delivery-gated**. This merged ledger is
the atomic ownership claim for the exact checkout and branch above. Agents not explicitly dispatched to this
ledger treat the stream as owned and must not create a checkout or branch. Search must consume published
Contracts and producer simulator artifacts rather than another data service's runtime source. No canonical rename,
visibility change, canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

The one agent explicitly dispatched to this ledger claims the reserved stream by verifying the exact checkout
path is still absent, cloning there exactly once, and recording the resulting worktree/branch in its first
substantive checkpoint. Fetch `origin/main`, verify exact head `befe816afe8d1a75aeb7ba31a80e492e9d48c83b`, and
create `Chore/search-promotion-preparation`.

First land the stale mirror-README correction plus CI that supplies `GITHUB_PACKAGES_TOKEN`, Release-builds
Web and Workers, and runs UnitTests and Docker-backed IntegrationTests. Do not claim whole-solution,
AppHost, ArchitectureTests, or E2E closure. Later slices consume RT3's exact hosting/image seam; coordinate
Stage 4's removal of the inherited full-stack E2E Helper and define a Search-owned TestKit; add an owner-local
migration job/bundle and remove runtime migration; publish Web/Workers/migration images and required
Hosting/TestKit packages; prove clean-clone/ruleset evidence; and prove standalone convergence from B2B-owned
simulator events, including rating events, without directly depending on Customer runtime or simulator.

## Completed work

- Search extraction proof was built and pushed to private `Concertable/search-next`.

## Verification

No promotion-preparation candidate has been verified in the target repository yet.

## Reviews

No promotion candidate exists. Review the first committed preparation slice before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Search is a data service: its current projection inputs, including rating updates, are B2B-owned events; it consumes B2B simulator artifacts, never B2B/Customer runtime source or databases.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
