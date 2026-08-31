# Repository-per-microservice migration — Customer promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer-next`
- Branch: `main`; create one repository-local preparation branch from fetched `origin/main`
- PR: none
- Dependency/package gates: implementation is unblocked; final checkpoint 13 delivery is gated on canonical platform/system baselines, preceding service cutovers, and explicit authorization
- Last reconciled: **2026-08-31** from private `customer-next` `main` `e21ae9079ca2fdd3a0063a252f05499159d608ff`

## Current state

The reviewed private proof contains the Customer backend, web, mobile, and customer-only shared package and
builds standalone. This stream owns checkpoint-13 repository preparation, not RT3, Stage 4, Auth-next, the
umbrella migration ledger, or another service repository. The completed frontend-fold ledger remains the
evidence record for the extraction proof and is not reopened for promotion work.

State: **implementable, delivery-gated**. RT3, Stage 4, and earlier service promotions do not block
repository-local preparation. No rename, visibility change, canonical publication, live migration,
production deployment, or monorepo source removal is authorized.

## Next Steps

Fetch private `customer-next` `origin/main` in the existing checkout and create a repository-local
preparation branch. Audit checkpoint 13/10B against the extracted tree, then implement the first independently
shippable slice: Customer-owned CI covering clean restore plus backend Release build, shared/web tests and
build, mobile typecheck/export, and standalone AppHost/migration validation. Record missing publication,
Hosting/TestKit, Review/Seed Contracts, simulator, and repository-settings work as later slices in this ledger;
do not modify sibling streams.

## Completed work

- Full backend/frontend extraction proof reviewed, validated, and pushed at `e21ae9079ca2fdd3a0063a252f05499159d608ff`; see `REPOSITORY_PER_MICROSERVICE_MIGRATION_CUSTOMER_FRONTEND_PROGRESS.md`.

## Verification

The extraction head passed npm clean install, shared/web tests and builds, mobile typecheck/export, and the
51-project Release solution build. Promotion-specific CI and publication gates have not yet run.

## Reviews

No promotion candidate exists yet. Review the first committed preparation slice before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Implementation and final delivery use separate dependency graphs; only irreversible checkpoint-13 cutover actions wait for upstream canonical gates.
- Reuse the existing private checkout. Do not create a duplicate clone or rewrite private `main`.
