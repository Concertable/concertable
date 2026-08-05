# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\platform_polyrepo-fullstack` (off `origin/main` @ `b92bf0b49`). The old `Feature/FrontendBuildSeparation` worktree/branch is orphaned (PR #319 merged, remote branch deleted) and must not be reused.
- Branch: `Feature/platform_polyrepo-fullstack`
- PR: none yet (Phase 2). Review-fix PR [#319](https://github.com/Concertable/concertable/pull/319) **merged** (`5a84756de`, 2026-08-03); Phase 1 PR [#301](https://github.com/Concertable/concertable/pull/301) merged
- Dependency/package gates: `@concertable/shared@0.1.0-alpha.0.2129` is published and restorable; Phase 2 is now unblocked (review-fix PR #319 has landed). #319 touched only CI/docs (no `api/**`), so no platform-sync PR was triggered
- Last reconciled: 2026-08-05 — created the dedicated Phase 2 worktree and pulled the authoritative ledger onto its branch (the Phase-2-unblocked closeout had been committed onto the unrelated `Feature/SelfBillingAgreement` branch, so `origin/main` still held the pre-merge ledger)

## Current state

Phases 0 and 1 are on `main` through PR #301, and the Phase 1 review fixes are on `main` through PR #319, merged as `5a84756de` on 2026-08-03. `f57a4c504` is verified an ancestor of `origin/main`. The remote `Feature/FrontendBuildSeparation` branch was deleted on merge; the local worktree tip `ec7751f77` is an orphaned merge commit, now behind `origin/main` and no longer authoritative. No open platform-sync PR exists. Phase 1 (with review fixes) is fully terminal; Phase 2 is unblocked.

## Next Steps

**Progress (2026-08-05):** Tier 1 of 4 packaged — `@concertable/web` (web/shared) builds to `dist` green and is committed (`0fa7ce511`). Recipe proven: `package.json` with `./shared/*` dist exports + `publishConfig`; `tsconfig.json` (internal `@/*`→src, `lib` incl. `ES2022`, a `vite-env.d.ts` for `import.meta.env`); `tsconfig.build.json`; `tsc` + `tsc-alias`. **Next: replicate to `@concertable/mobile`, `@concertable/customer`, then `@concertable/b2b`** (b2b builds after web; mobile differs — RN types not DOM `lib`, no barrels so a single `./shared/*` wildcard, and the metro `watchFolders` + nativewind `global.css` input need retargeting to the package). Then step 2 (cutover), step 3 (publish automation), step 4 (gate).

Execute **Phase 2** in this worktree, following the Phase-1 `@concertable/shared` package as the template.

**Resolution model — decided 2026-08-05 (confirmed with Tommy): dist-only, build-first.** The tiers become installed packages consumed from `dist` in-monorepo (identical to `@concertable/shared` and the backend's consume-published-artifacts model); **no `source` export condition**. Add explicit pre-build ordering so the SPAs build against freshly-built tier `dist`.

Tier→package + exports root: `web/shared`→`@concertable/web` (`./shared/*`); `mobile/shared`→`@concertable/mobile` (`./shared/*`, no barrels — wildcards only); `web/b2b/shared`→`@concertable/b2b` (`./web/shared/*`); `customer/shared`→`@concertable/customer` (repoint existing exports src→dist, flip `private`, bump version). **Dep/build order:** `@concertable/shared`→{`@concertable/web`, `@concertable/mobile`, `@concertable/customer`}; `@concertable/b2b` also depends on `@concertable/web` (build web before b2b).

Steps: (1) scaffold each tier package (`package.json` exports→dist, `tsconfig.build.json`, internal alias via `tsc-alias`, `publishConfig`) + register in `app/package.json` workspaces; (2) rewrite every cross-tree alias import — in consumer surfaces **and inside the tiers themselves** — to a bare package specifier, then delete those aliases from every `tsconfig`/`vite`/`metro` config (keep each surface's own intra-package `@/*`→`./src/*`); (3) extend `version-fe-packages.mjs` / `verify-fe-package.mjs` / `publish-fe-packages.yml` to all tiers; (4) gate: grep-clean (no cross-tree source alias survives) + build all packages then four web builds + both mobile typechecks green. Follow Phase 2 in the plan.

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
- Phase 2 was deliberately blocked until the review-fix PR landed; PR #319 is now merged, so Phase 2 is unblocked.
- Ledger-drift caught at Phase 2 start: the Phase-2-unblocked closeout commits (`b11da1d38`, `2efd1647f`) were made while a prior session sat on the unrelated `Feature/SelfBillingAgreement` branch, so they never reached `origin/main` — the fresh worktree therefore started from the pre-merge ledger. Content was correct; recovered onto this branch via `git show`. Those stray doc commits are left on `SelfBillingAgreement` (per repo policy, doc commits riding a feature branch are not worth a force-push) and will reconcile when that PR merges. This is the shared-checkout hazard that motivated the dedicated `Feature/platform_polyrepo-fullstack` worktree.
- **In-monorepo tier resolution — dist-only, build-first (confirmed with Tommy 2026-08-05).** The load-bearing finding: there is no turbo/nx and no `source` export condition anywhere; `@concertable/shared` already resolves to built `dist` in-monorepo, so editing it needs a rebuild. Phase 2 extends that from 1 tier to 5. Chose to keep the tiers dist-only (consume the built artifact both in-monorepo and when carved), matching Phase 1 and the backend's consume-published-artifacts model, rather than add a `source`/`development` condition (which would make in-monorepo dev diverge from carved reality and hide "forgot to rebuild / dist broken" bugs). Mitigation: a one-command `build:packages` + extend the CI pre-build step. Alternative (source condition + retrofit `@concertable/shared`) explicitly considered and declined.
- **The tiers depend on each other, so cutover is not consumer-only.** `@concertable/b2b` (web/b2b/shared) imports `@/*` and `shared/*` that resolve to `@concertable/web` (web/shared), plus `@concertable/shared`; its own self-alias is `@b2b/*`. So the alias→package rewrite and the dep graph must cover intra-tier imports too, and `@concertable/web` must build before `@concertable/b2b`.

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

### 2026-08-04 — PR #319 confirmed merged; Phase 2 unblocked

- Action: Resumed to review/merge PR #319; found it already merged and reconciled the ledger to the terminal state.
- Evidence: `gh pr view 319` reports `state MERGED`, merge commit `5a84756de` at 2026-08-03T15:42Z; `git merge-base --is-ancestor 1152ee7cb origin/main` and `f57a4c504 origin/main` both pass; all five PR files (`.github/workflows/test.yml`, `AGENTS.md`, `plans/AGENTS.md`, `plans/POLYREPO_FULLSTACK_PROGRESS.md`, `reviews/AGENTS.md`) landed; `git ls-remote origin Feature/FrontendBuildSeparation` empty (branch deleted); no open `chore/platform-sync-*` PR.
- Outcome: Phase 1 including its review fixes is fully terminal on `main`. The worktree tip `ec7751f77` is orphaned and behind main. Phase 2 is unblocked.
- Follow-up: Begin Phase 2 from a fresh worktree/branch off current `origin/main`.

### 2026-08-05 — Dedicated Phase 2 worktree created; ledger reconciled onto its branch

- Action: Created the isolated worktree/branch `Feature/platform_polyrepo-fullstack` off `origin/main` (the resume prompt had been launched from the main checkout while it sat on the unrelated `Feature/SelfBillingAgreement` branch — no Phase 2 work was done there). Discovered the Phase-2-unblocked ledger closeout was stranded on `SelfBillingAgreement`; recovered the authoritative ledger onto this branch.
- Evidence: `git worktree add … -b Feature/platform_polyrepo-fullstack origin/main` at `b92bf0b49`; main checkout verified still on `Feature/SelfBillingAgreement` with only unrelated untracked paths (its HEAD advanced `3174d7f59 → 5070a8026` under another live session); `gh pr view 319` = MERGED `5a84756de`; branch-time platform-sync gate returned no open sync PR; `git rev-list --count origin/main..Feature/SelfBillingAgreement -- <ledger>` = 3 (two are the stray polyrepo closeout commits).
- Outcome: Phase 2 has a clean, isolated worktree; the branch ledger now reflects the true post-#319 state.
- Follow-up: Scope the four shared tiers against the Phase-1 `@concertable/shared` template and begin publishing + consumer cutover.

### 2026-08-05 — Resolution model confirmed; first tier packaged (`@concertable/web`)

- Action: Mapped the full Phase 2 cutover surface (6 consumer surfaces, 148 files, per-tier structure, build-order finding); confirmed the dist-only/build-first resolution model with Tommy; packaged `web/shared` as `@concertable/web` end-to-end and verified its `dist` build.
- Evidence: `@concertable/shared` dist prebuilt (exit 0); `@concertable/web` build real exit 0 after fixing 14 standalone-build errors (13× `import.meta.env` → added `vite-env.d.ts`; 1× `.at()` → ES2022 `lib`); `dist` emitted across all subdirs, `tsc-alias` left zero `@/` specifiers, external `@concertable/shared/*` specifiers intact; committed `0fa7ce511`; `node_modules` primed via `npm ci` (exit 0) and `npm install` registered the new workspace with a clean prior lockfile.
- Outcome: The tier-packaging recipe is proven and one of four tiers is done and committed on the isolated branch.
- Follow-up: Replicate the recipe to `@concertable/mobile`, `@concertable/customer`, `@concertable/b2b`, then the consumer/intra-tier import cutover, publish-automation extension, and the build gate.
