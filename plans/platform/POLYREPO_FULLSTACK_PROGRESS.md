# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\FrontendBuildSeparation`
- Branch: `Feature/FrontendBuildSeparation`
- PR: review-fix PR [#319](https://github.com/Concertable/concertable/pull/319) open; Phase 1 PR [#301](https://github.com/Concertable/concertable/pull/301) merged
- Dependency/package gates: `@concertable/shared@0.1.0-alpha.0.2129` is published and restorable; Phase 2 must not start until the review-fix PR lands
- Last reconciled: 2026-08-03 after verified latest-main work-head push to PR #319

## Current state

Phases 0 and 1 are on `main` through PR #301. The existing local feature branch carried two later Phase 1 review-fix commits, `f57a4c504` and `0e3d8f5a6`, after its remote branch was deleted. `origin/main` at `92ee8483c` was merged into this fresh isolated worktree as `ffc7f7339`; a later platform-sync advance to `d0fa851fa` was merged as work head `1ba1bb1f0`. The conflict was resolved by preserving main's resume-plan/progress-ledger and worktree-identity rules while retaining the compatible E2E and review-policy changes. PR #319 is open with `full-e2e`; after fetch, the local, remote, and PR heads were all verified at `1ba1bb1f0e684c533ad3cecb7b7bc83ccdef3ca3`.

## Exact next action

Run `/code-review` for PR #319 against current `origin/main`. If the review is clear and required head checks pass, hand off `/merge`; Phase 2 remains blocked until PR #319 lands.

## Completed work

- Phase 0 registry/PAT setup is complete as recorded by `e0513bac0` and the plan.
- Phase 1 implementation and publication automation landed through PR #301 at feature head `7c9a64a3e`; GitHub merged it as `19be13d330` on 2026-08-02.
- Material Phase 1 commits include `7f8e75d57` (per-file ESM/declarations), `90f4baa8a` (versioning and publish automation), `369f39918` (Node/NodeNext-resolvable emitted imports), `ca1e398ed` (packed-artifact Node and Expo/Metro verification), and `5f9863654` (customer owner-package alignment without starting Phase 2).
- `f57a4c504` fixes the E2E-tier label lookup to fail closed when GitHub label retrieval fails.
- `0e3d8f5a6` makes full merge-queue E2E the strict default, preserves the no-duplicate-local-E2E workflow, and keeps findings on the reviewed branch unless they are proven independent.

## Verification

- `npm view @concertable/shared@0.1.0-alpha.0.2129 version --registry=https://npm.pkg.github.com` returned `0.1.0-alpha.0.2129` on 2026-08-03.
- PR #301's final merge-group run [30766521292](https://github.com/Concertable/concertable/actions/runs/30766521292) completed successfully; `build`, `e2e-api-tests`, `e2e-ui-tests`, and `ci-complete` all passed.
- On the post-merge review-fix tree, `git diff --check origin/main` passed.
- The changed `Classify changed files` shell block from `.github/workflows/test.yml` passed Git Bash syntax validation.
- A focused stubbed-label test proved the new failure path exits 1 and emits `Could not retrieve labels for PR #301`; a successful `full-e2e` label fetch exits 0.
- `git diff --name-only origin/main` before adding this ledger listed only `.github/workflows/test.yml`, `AGENTS.md`, `plans/AGENTS.md`, and `reviews/AGENTS.md`.
- After merging latest `origin/main` at `d0fa851fa`, the same diff, Git Bash syntax, fail-closed, and successful-label checks passed; the branch was zero commits behind and the PR diff remained the four review-fix files plus this ledger.

## Reviews

- Review: Phase 1 post-merge review of the work delivered by PR #301 at `7c9a64a3e`. The exact review artifact, original finding identifiers, and narrower review range are not present in git, GitHub PR comments, or the preserved orphaned directory, so they are not fabricated here.
- Finding reference unavailable — fail-open PR-label lookup: fixed by `f57a4c504`.
- Finding reference unavailable — E2E eligibility and reviewed-branch policy inconsistencies: fixed by `0e3d8f5a6`; compatible changes retained through the `origin/main` reconciliation.
- No open finding is evidenced. Delivery of the two fixed findings remains gated on the new review-fix PR.

## Decisions, discoveries, blockers, and deviations

- The dirty main checkout is unrelated and must not be edited.
- `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\FrontendBuildSeparationReview` exists but is not a registered git worktree and has no usable `.git` metadata. It was inspected read-only and must not be deleted.
- `origin/Feature/FrontendBuildSeparation` was gone at recovery time; the existing local branch is the authoritative source of the two review-fix commits.
- Main's newer plan lifecycle, progress-ledger checkpoint, resume-plan skill, and worktree-identity rules take precedence over obsolete plan-deletion wording from `0e3d8f5a6`.
- The review-fix PR requires full merge-queue E2E because it changes CI policy. Do not add `skip-e2e`; apply `full-e2e`.
- Phase 2 is deliberately blocked until the review-fix PR lands.

## Event log

### 2026-08-02 — Phase 1 merged and published

- Action: Delivered the universal shared frontend package and its publication/verification automation.
- Evidence: PR #301, head `7c9a64a3e`, merge `19be13d330`; final merge-group run 30766521292 passed build and both E2E layers.
- Outcome: Phases 0 and 1 are present on `main`; `@concertable/shared@0.1.0-alpha.0.2129` is published.
- Follow-up: Complete post-merge review fixes before Phase 2.

### 2026-08-03 — Phase 1 review findings fixed locally

- Action: Fixed fail-open label retrieval and aligned E2E/review-branch policy.
- Evidence: commits `f57a4c504` and `0e3d8f5a6`.
- Outcome: Both evidenced findings are fixed locally; the old remote branch had been deleted.
- Follow-up: Reconcile with current main and deliver the fixes in a new PR.

### 2026-08-03 — Reconstructed baseline and main reconciliation

- Action: Fetched `origin`, created the fresh isolated worktree, merged `origin/main` at `92ee8483c`, resolved the policy conflict, inspected the orphan read-only, reconstructed this ledger, and ran focused verification.
- Evidence: worktree/branch status; merge parents `0e3d8f5a6` and `92ee8483c`; four-file intended diff; Git Bash syntax and stubbed-label checks; package and GitHub run queries.
- Outcome: Main's resume/progress/worktree rules and the compatible review fixes coexist; verification is green.
- Follow-up: Push the reconciled work head, open the full-E2E review-fix PR, and do not start Phase 2.

### 2026-08-03 — Review-fix work head pushed

- Action: Pushed the reconciled work head and restored `origin/Feature/FrontendBuildSeparation` with upstream tracking.
- Evidence: the starting remote branch did not exist; pushed range `f57a4c504..ffc7f7339`; after fetch, local `HEAD` and `origin/Feature/FrontendBuildSeparation` both resolved to `ffc7f7339ae1cdb94a8381418eb020a24e88f2f9`; no open PR existed.
- Outcome: The verified review-fix work is published at `ffc7f7339`.
- Follow-up: Open the plain GitHub PR, apply `full-e2e`, and verify its head before stopping.

### 2026-08-03 — Review-fix PR opened; newer main observed

- Action: Opened plain GitHub PR #319 and applied `full-e2e`, then fetched the base and inspected its live state.
- Evidence: PR #319 was open at `db92ad7f47e484c2909185db09245dc2337064ed`, equal to the local and remote branch heads, with label `full-e2e`; initial `changes` and `instant-merge` checks passed while `build` was running. The subsequent fetch resolved `origin/main` to `d0fa851faad602b592e5886225941d58f6aeefc1`, two commits ahead through green platform-sync PR #318.
- Outcome: The PR exists with the required E2E tier but is behind the newest base.
- Follow-up: Merge the new main tip, verify, and update the PR head before handing off review/merge.

### 2026-08-03 — PR updated to latest main

- Action: Merged `origin/main` at `d0fa851fa`, re-ran focused verification, and pushed the updated work head to PR #319.
- Evidence: starting remote and PR head `db92ad7f47e484c2909185db09245dc2337064ed`; pushed work head `1ba1bb1f0e684c533ad3cecb7b7bc83ccdef3ca3`; after fetch, local `HEAD`, `origin/Feature/FrontendBuildSeparation`, and PR `headRefOid` all equalled `1ba1bb1f0`; PR label remained `full-e2e`; `HEAD..origin/main` count was zero.
- Outcome: PR #319 is current with the observed base and contains only the intended review fixes plus this ledger.
- Follow-up: Review PR #319, then merge it only after review and required checks are green; Phase 2 waits for the merge.
