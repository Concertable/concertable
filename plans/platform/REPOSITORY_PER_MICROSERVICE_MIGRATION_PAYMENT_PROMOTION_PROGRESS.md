# Repository-per-microservice migration — Payment promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: none; locate an existing private checkout before cloning once
- Branch: next proposed `Chore/payment-promotion-preparation`
- PR: none; no open `Concertable/payment-next` PR exists
- Dependency/package gates: implementation is unblocked from private `payment-next` `main` `e4da6e23f79bed9105e4a82f828c0608feee68a5`; final checkpoint 11 delivery is ordered and explicitly authorized
- Last reconciled: **2026-08-31** from GitHub repository and PR state

## Current state

Private `Concertable/payment-next` exists with its extraction proof and no open PR. This reserved stream owns
only checkpoint-11 Payment repository preparation: Web, Workers, Contracts, Client, migrations, Stripe
tooling, images, Hosting/TestKit, AppHost, CI, publication setup, and repository evidence. It must not edit
RT3, Stage 4, Auth-next, Customer-next, Search-next, or shared execution ledgers.

State: **ready and unowned; implementable, delivery-gated**. No canonical rename, visibility change,
canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

Assign one owner. Search for and reuse an existing private `payment-next` checkout; clone exactly once only
if none exists. Fetch `origin/main`, verify exact head `e4da6e23f79bed9105e4a82f828c0608feee68a5`, create
`Chore/payment-promotion-preparation`, audit checkpoint 11/10B, and implement the smallest repository-owned CI
slice that cleanly builds and tests Payment's standalone closure. Record later package/image publication,
migration, Hosting/TestKit, Stripe tooling, AppHost, ruleset, and clean-clone slices here.

## Completed work

- Payment extraction mechanism was proven and pushed to private `Concertable/payment-next`.

## Verification

No promotion-preparation candidate has been verified in the target repository yet.

## Reviews

No promotion candidate exists. Review the first committed preparation slice before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Payment is an adapter service and owns the only live internal gRPC surface plus its Stripe HTTP webhook.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
