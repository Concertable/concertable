# Repository-per-microservice migration — Search promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: none; locate an existing private checkout before cloning once
- Branch: next proposed `Chore/search-promotion-preparation`
- PR: none; no open `Concertable/search-next` PR exists
- Dependency/package gates: implementation is unblocked from private `search-next` `main` `befe816afe8d1a75aeb7ba31a80e492e9d48c83b`; final checkpoint 12 delivery is ordered and explicitly authorized
- Last reconciled: **2026-08-31** from GitHub repository and PR state

## Current state

Private `Concertable/search-next` exists with its extraction proof and no open PR. This reserved stream owns
only checkpoint-12 Search repository preparation: Search runtime, migrations, Hosting/TestKit, standalone
AppHost, CI, package/image publication setup, seed convergence gates, and repository evidence. It must not
edit RT3, Stage 4, Auth-next, Customer-next, Payment-next, or shared execution ledgers.

State: **ready and unowned; implementable, delivery-gated**. Search must consume published Contracts and
producer simulator artifacts rather than another data service's runtime source. No canonical rename,
visibility change, canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

Assign one owner. Search for and reuse an existing private `search-next` checkout; clone exactly once only if
none exists. Fetch `origin/main`, verify exact head `befe816afe8d1a75aeb7ba31a80e492e9d48c83b`, create
`Chore/search-promotion-preparation`, audit checkpoint 12/10B, and implement the smallest repository-owned CI
slice that cleanly builds and tests Search's standalone closure. Record later publication/image, migration,
Hosting/TestKit, AppHost, seed convergence, ruleset, and clean-clone slices here.

## Completed work

- Search extraction proof was built and pushed to private `Concertable/search-next`.

## Verification

No promotion-preparation candidate has been verified in the target repository yet.

## Reviews

No promotion candidate exists. Review the first committed preparation slice before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Search is a data service: it consumes B2B/Customer events and simulator artifacts, never their runtime source or databases.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
