# Repository-per-microservice migration — Auth-next promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\auth-next`
- Branch: `main`; create one repository-local preparation branch from fetched `origin/main`
- PR: none
- Dependency/package gates: implementation is unblocked from verified proof `198ca1e481dd056e008a0b5e6adb37651a072c1d`; final checkpoint 10 delivery remains ordered and requires explicit authorization
- Last reconciled: **2026-08-31** from the verified private Auth proof and merged monorepo PR #877

## Current state

Private `auth-next` contains Auth, Auth.Contracts, the AuthDb-owned Duende store, its migration executable,
Hosting, standalone AppHost, and focused verification. This ledger owns only checkpoint-10 repository
preparation. It does not own RT3, Stage 4, Customer, another service repository, or the umbrella ledger.

State: **implementable, delivery-gated**. No live migration, canonical rename, visibility change, production
publication/deployment, or monorepo source removal is authorized.

## Next Steps

Resume in the existing `auth-next` checkout from fetched private `origin/main`. Create a repository-local
preparation branch, audit checkpoint 10B, and implement the first independently shippable missing slice:
repository-owned CI for Auth, Auth.Contracts, AppHost, migration executable, architecture/composition tests,
and manifest verification. Preserve the AuthDb/no-B2BDb invariant and record later publication/image,
Hosting/TestKit, ruleset, and clean-clone slices here; do not modify sibling streams.

## Completed work

- PR #877 moved Duende persisted grants to AuthDb.
- Private `auth-next` proof `198ca1e481dd056e008a0b5e6adb37651a072c1d` builds Auth, Auth.Contracts, AppHost, and migration tooling standalone with no B2BDb resource.

## Verification

The proof passed Release builds, four operational-store migration tests, two architecture/composition tests,
and Aspire manifest publication. Repository-owned promotion CI has not yet run.

## Reviews

The private proof delta through `198ca1e481dd056e008a0b5e6adb37651a072c1d` was reviewed with no findings.
Review the first new preparation candidate before opening its PR.

## Decisions, discoveries, blockers, and deviations

- Auth owns both `AuthDbContext` and Duende's `PersistedGrantDbContext`; Auth.Contracts remains a sibling top-level build root.
- Reuse the existing private checkout and preserve history. Do not execute the live operational-store migration during preparation.
