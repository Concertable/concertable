# Repository-per-microservice migration — Payment promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: reserved `C:\Users\tommy\source\repos\payment-next`; verify absence before cloning exactly once
- Branch: next proposed `Chore/payment-promotion-preparation`
- PR: none; no open `Concertable/payment-next` PR exists
- Dependency/package gates: implementation is unblocked from private `payment-next` `main` `e4da6e23f79bed9105e4a82f828c0608feee68a5`; final checkpoint 11 delivery is ordered and requires explicit authorization
- Last reconciled: **2026-08-31** from GitHub repository and PR state

## Current state

Private `Concertable/payment-next` exists with its extraction proof and no open PR. This reserved stream owns
only checkpoint-11 Payment repository preparation: Web, Workers, Contracts, Client, migrations, Stripe
tooling, images, Hosting/TestKit, AppHost, CI, publication setup, and repository evidence. It must not edit
RT3, Stage 4, Auth-next, Customer-next, Search-next, or shared execution ledgers.

The target currently has no `.github` workflows. Web, Workers, UnitTests, IntegrationTests, Contracts,
Client, and Hosting are package-clean; the whole solution is not. AppHost/ArchitectureTests retain foreign
Auth/B2B/AppHost.Shared source and database composition, and E2E Helpers retain foreign test source. The
README, root guidance inheritance, and package repository URLs still describe the old monorepo mirror.

State: **reserved to one Payment preparation owner; implementable, delivery-gated**. This merged ledger is
the atomic ownership claim for the exact checkout and branch above. Agents not explicitly dispatched to this
ledger treat the stream as owned and must not create a checkout or branch. No canonical rename, visibility change,
canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

The one agent explicitly dispatched to this ledger claims the reserved stream by verifying the exact checkout
path is still absent, cloning there exactly once, and recording the resulting worktree/branch in its first
substantive checkpoint. Fetch `origin/main`, verify exact head `e4da6e23f79bed9105e4a82f828c0608feee68a5`, and
create `Chore/payment-promotion-preparation`.

First land repository metadata/guidance corrections plus CI that supplies approved package-read credentials,
Release-builds Web and Workers, runs UnitTests and Docker-backed IntegrationTests, and packs Contracts,
Client, and Payment.Hosting. Do not claim whole-solution, AppHost, ArchitectureTests, or E2E closure. Later
slices own candidate-package consumer restore/publication; Web/Workers/migration images; owner-local migration
job/bundle and runtime-migration removal; RT3/Auth absorption for the standalone AppHost; package-clean
Hosting/TestKit and Stripe tooling; and repository settings/clean-clone evidence. Stage 4 alone moves system
E2E composition.

## Completed work

- Payment extraction mechanism was proven and pushed to private `Concertable/payment-next`.

## Verification

No promotion-preparation candidate has been verified in the target repository yet.

## Reviews

No promotion candidate exists. Review the first committed preparation slice before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Payment is an adapter service and owns the only live internal gRPC surface plus its Stripe HTTP webhook.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
- The target currently has no configured repository secret/variable names and package ACL is unproven; CI must prove private package restore without recording secret values.
