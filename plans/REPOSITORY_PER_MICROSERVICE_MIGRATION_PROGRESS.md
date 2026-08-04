# Repository-per-microservice migration progress

- Plan: `plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Plan\RepositoryPerMicroserviceMigration`
- Branch: `Plan/RepositoryPerMicroserviceMigration`
- PR: not opened (`gh pr list --head Plan/RepositoryPerMicroserviceMigration --state all` → empty)
- Dependency/package gates: none active. No checkpoint has been implemented, so nothing is published, pinned, or platform-synced against this plan.
- Last reconciled: 2026-08-04, reconstructed from git + PR evidence (this ledger did not previously exist).

## Current state

**Design-only. No implementation checkpoint has begun.** The plan document itself declares
*"awaiting Tommy's review. No implementation checkpoint is authorized by this document alone."*

The branch holds two docs-only commits on top of merge-base `d3c399ec8` (the plan's own stated
planning baseline):

- `91e92c445 docs(plan): design repository-per-microservice migration` — authored the plan; deleted
  the superseded `plans/POLYREPO.md` and `plans/SPLIT_TIME_E2E_STRATEGY.md`; touched
  `api/Concertable.B2B/TECH_DEBT.md` and `api/Concertable.Customer/TECH_DEBT.md` (1 line each).
- `3f8cb3494 docs(plan): resolve migration review blockers` — revised the plan (+55/-33) and
  `plans/DEPLOYMENT.md` to resolve design-review blockers.

Working tree is clean. No `api/**`, `app/**`, workflow, or migration code has changed on this branch.

The plan breaks the migration into **17 checkpoints (0–16)**, each a separate PR/merge boundary, most
with an explicit human-review hard stop. **None have started.**

Staleness to be aware of: the branch is **138 commits behind `origin/main`** (docs-only branch, so
this is expected and harmless while it stays design-only). The plan's inventory numbers were captured
against baseline `d3c399ec8` and may have drifted since — treat them as a baseline snapshot, not
current truth, and re-verify at Checkpoint 0.

## Next Steps

**Gated on Tommy's explicit approval of the plan design.** The plan is `awaiting Tommy's review`; its
execution rules forbid starting any checkpoint until Tommy names it and says to execute it now. Once
approved, the first executable step is **Checkpoint 0 — Baseline, permissions, and reproducible
inventory** (`concertable`), which itself ends in a hard-stop review before any target repo is created.
Prerequisite: this branch is 138 commits behind `origin/main` — sync before writing any Checkpoint 0
code so the inventory is captured against current `main`, not the stale `d3c399ec8` baseline.

## Completed work

| Item | Evidence |
|---|---|
| Plan authored | commit `91e92c445` (docs-only; adds `plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md`, removes `POLYREPO.md` + `SPLIT_TIME_E2E_STRATEGY.md`) |
| Design-review blockers resolved | commit `3f8cb3494` (plan +55/-33, `plans/DEPLOYMENT.md` updated) |

No implementation checkpoint (0–16) is complete.

## Verification

No build/test verification applies — the branch is docs-only. No `dotnet build`, unit, integration,
or E2E run is associated with this branch, and none is required for a design document.

## Reviews

A design review of the plan occurred (commit `3f8cb3494 docs(plan): resolve migration review
blockers` records the resolution). No review artifact for this plan exists under `reviews/`. Its
findings are recorded only as resolved via that commit; treat the specific findings as unknown beyond
"blockers resolved in the plan text."

No code review applies — there is no implementation to review yet.

## Decisions, discoveries, blockers, and deviations

- **Decision — nine canonical repositories:** five service repos (B2B, Customer, Payment, Search,
  Auth), two platform repos (`platform-dotnet`, `platform-web`), one `system` repo (full-stack
  AppHost, fleet manifest, IaC, deployment, black-box E2E), and `Concertable/.github`.
- **Decision — lockstep `ConcertablePlatformVersion` + platform-sync PR are retired** in favour of
  independently versioned producer trains + Renovate; breaking Contracts use expand/publish/migrate/
  contract, never a repo-wide forced bump.
- **Decision — B2B Workers ships as a container on native Azure Functions on Container Apps**, not
  Functions Consumption (Consumption cannot run the custom container). This supersedes the earlier
  deployment design on that point.
- **Discovery — deployable closures are already package-clean:** the only non-AppHost/non-test
  cross-area `ProjectReference` is `Concertable.Auth.Contracts -> Concertable.Messaging.Contracts` (a
  platform edge). All other cross-area edges live in AppHost/E2E code.
- **Discovery — Auth persisted-grant coupling:** Auth's Duende persisted grants currently live in
  `B2BDb` and must move to `AuthDb` before Auth extraction (Checkpoint 1).
- **Discovery — mirrors are stale:** the six generated mirrors' latest parity runs are red (all six
  differed from `main` on 2026-08-02); they are historical bootstrap inputs, not trusted cutover
  sources, and need a final refresh + independent history verification.
- **Discovery — target repos do not exist yet:** no `Concertable/config`, `/system`,
  `/platform-dotnet`, or `/platform-web`; the planning credential lacks `read:packages`, so package
  ACL verification is an explicit Checkpoint 0 preflight, not an assumption.
- **Blocker/gate — plan is awaiting Tommy's review;** no checkpoint is authorized until he names one.
- **Deviation from staleness:** branch is 138 commits behind `origin/main`; plan inventory reflects
  baseline `d3c399ec8` and may have drifted.

## Event log

### 2026-08-04 — Reconstructed baseline (this ledger created)

- Action: Created this progress ledger for a legacy plan that had none, via the `resume-plan`
  reconstruction path. Baseline is explicitly reconstructed from repository evidence, not fabricated
  history.
- Evidence: `git log origin/main..HEAD` = 2 docs commits (`91e92c445`, `3f8cb3494`); merge-base with
  `origin/main` = `d3c399ec8`; `git status` clean; `gh pr list --head Plan/RepositoryPerMicroserviceMigration
  --state all` = empty; `git rev-list --count HEAD..origin/main` = 138; no `reviews/` artifact for this
  plan; plan document header states "awaiting Tommy's review."
- Outcome: Ledger records design-only status, no implementation, no PR, no verification, no active
  package/platform gate.
- Follow-up: Await Tommy's approval to begin Checkpoint 0; sync the branch with `origin/main` before
  any Checkpoint 0 implementation.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Plan\RepositoryPerMicroserviceMigration
Read AGENTS.md, plans/AGENTS.md, plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md, and plans/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
