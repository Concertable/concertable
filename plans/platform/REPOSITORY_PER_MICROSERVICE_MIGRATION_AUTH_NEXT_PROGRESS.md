# Repository-per-microservice migration — Auth promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Repository: retained `Concertable/auth` (repository id `1351336992`)
- Worktree: `C:\Users\tommy\source\repos\auth`
- Branch: `main` at `5dfac11de3ddae3477e71262a4e69036c6b14e31`
- PR: latest preparation PR [`Concertable/auth#5`](https://github.com/Concertable/auth/pull/5) merged and green
- Dependency/package gates: repository preparation is terminal; checkpoint 10 delivery remains ordered behind
  the foundation/M1 cutover and requires its normal authorization gate
- Last reconciled: **2026-09-05** from the retained repository identity, current main, and terminal PR #5

## Current state

Private `Concertable/auth` retains its repository identity and contains Auth, Auth.Contracts, the AuthDb-owned
Duende store, its migration executable, Hosting, standalone AppHost, repository-owned CI, image/package
candidate workflows, and focused verification. This ledger owns only checkpoint-10 repository preparation.
It does not own another service repository, the system compatibility set, or foundation/M1.

State: **repository preparation terminal; delivery-gated**. No repository rename/replacement, live migration,
visibility change, production publication/deployment, or monorepo source removal is authorized by this ledger.

## Next Steps

Do not start another preparation slice. After foundation/M1 is terminal and checkpoint 10 is reached, fetch the
retained `Concertable/auth` main, reconcile the exact cutover source/artifacts, run a fresh current-head review,
and execute the ordered publication/system-qualification steps. Preserve the AuthDb/no-B2BDb invariant and do
not modify sibling streams.

## Completed work

- PR #877 moved Duende persisted grants to AuthDb.
- Private `auth-next` proof `198ca1e481dd056e008a0b5e6adb37651a072c1d` builds Auth, Auth.Contracts, AppHost, and migration tooling standalone with no B2BDb resource.
- Retained-repository PRs #1–#5 delivered standards wiring, standalone CI, package readiness, image readiness,
  and release-candidate verification to `Concertable/auth` main.

## Verification

The original proof passed Release builds, four operational-store migration tests, two architecture/composition
tests, and Aspire manifest publication. PR #5 exact-head CI passed build/test/manifest, image build/scan,
release-candidate preparation, and the required CI gate before merge to current main.

## Reviews

The private proof delta through `198ca1e481dd056e008a0b5e6adb37651a072c1d` was reviewed with no findings.
Run a fresh current-head review before checkpoint-10 delivery changes; the merged preparation PRs and their
checks remain the durable intervening evidence.

## Decisions, discoveries, blockers, and deviations

- Auth owns both `AuthDbContext` and Duende's `PersistedGrantDbContext`; Auth.Contracts remains a sibling top-level build root.
- `Concertable/auth` is the retained target; `auth-next` is historical proof vocabulary, not an actionable
  repository identity.
- Reuse the existing private checkout and preserve history. Do not execute the live operational-store migration during preparation.
