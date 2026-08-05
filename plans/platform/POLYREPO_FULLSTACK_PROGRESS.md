# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\platform_polyrepo-fullstack` — reclaimed off `origin/main` for Phase 3 after Phase 2 merged (branch reset to `origin/main`).
- Branch: `Feature/platform_polyrepo-fullstack`
- PR: Phase 3a PR [#378](https://github.com/Concertable/concertable/pull/378) **OPEN** (head `4f3f65621`, base `main`, no skip labels, gate green). Phase 2 PR [#360](https://github.com/Concertable/concertable/pull/360) **MERGED** (`a3f9535`, 2026-08-05); Phase 1 PR [#301] + review-fix [#319] merged earlier. Docs convention PR [#364] (dispatch-prompt on real E2E merge failure) **open**.
- Dependency/package gates: **all five FE tiers published at `0.1.0-alpha.0.2401`** (`publish-fe-packages.yml` run [31009601005] green, including its from-feed verify of every tier). No `api/**` in the #360 diff → no backend platform-sync.
- Last reconciled: 2026-08-05 (resume) — re-synced the branch to `origin/main` (was 19 behind; merge `4f3f65621`, 0 behind now), re-ran the full FE gate green, **pushed Phase 3a and opened PR [#378](https://github.com/Concertable/concertable/pull/378)**. Phase 3b (carve-fe-web CI + `run_fe` gate) stays blocked until #378 merges + republishes.

## Current state

Phases 0, 1, and **2 are on `main`**. Phase 2 landed via PR #360 (merge `a3f9535`): all four remaining tiers packaged, all six consumer surfaces cut over to package imports, publish automation extended, and `publish-fe-packages.yml` published every tier to the feed at `0.1.0-alpha.0.2401`. Phase 2 is fully terminal.

**Phase 3a is PUSHED as PR [#378](https://github.com/Concertable/concertable/pull/378) (OPEN).** Two code commits: `d6ac4b123` (subpath rename — dropped the redundant `/shared` / `/web/shared` from every tier specifier so imports read `@concertable/<tier>/<path>`, uniform with `@concertable/shared`; exports maps + imports rewritten in lockstep, resolutions preserved by construction) and `c4775ebf1` (surfaces self-declare their full dependency closure — `@concertable/*` tiers + third-party libs + CSS `@import` assets, previously all masked by npm workspace hoisting — plus `app/scripts/carve-fe.mjs`, the feed-restore carve harness). Re-synced to `origin/main` (was 19 behind; clean merge `4f3f65621`, 0 behind) and re-ran the full FE gate green: `build:packages` exit 0, four web builds exit 0, both mobile `tsc --noEmit` 0 errors. PR head `4f3f65621`, base `main`, no skip labels; net diff is FE-only (0 `api/**`) so no backend platform-sync; broad package/workspace change → the CI classifier auto-runs full merge-queue E2E. `test.yml` deliberately untouched (carve-fe deferred).

## Next Steps

1. **Phase 3a PR [#378](https://github.com/Concertable/concertable/pull/378) is code-reviewed clean** (`reviews/Feature-platform_polyrepo-fullstack.md`, no findings) **and current with `main`; being merged via the queue** (full E2E auto-runs; no `skip-e2e`; no backend platform-sync). In flight: waiting on the PR's own checks (build/carve/unit/integration) to go green, then `gh pr merge 378 --merge --auto` and poll to MERGED. On merge, `publish-fe-packages.yml` republishes all five tiers with the new bare exports — **this republish is the gate that unblocks step 2.**
2. **After that PR merges/republishes**, add the `carve-fe-web` CI job (matrix over the 4 web surfaces, each calling `node scripts/carve-fe.mjs <surface>`) + a `run_fe` change-classifier gate (non-inert `^app/` change; keeps BE-only PRs off the slow npm carves) + `ci-complete` wiring, in `.github/workflows/test.yml`. **Deferred deliberately (publish-first):** a feed-restore carve on the rename PR itself installs the OLD published tiers (old `/shared` export keys) and can't resolve the renamed imports → guaranteed red. The job + gate were written and locally proven for `web/customer` before the rename (green `tsc -b` + vite 3637 modules); re-derive from git history / this ledger.
3. **Carved-web CSS `@source` content strategy** — carve BUILD is green, but `@concertable/web`'s `index.css` `@source` globs point at sibling-surface source (`../../customer/src`, `../../b2b/*`) that resolve only in-monorepo; a carved surface generates its own classes (Tailwind v4 auto-detect) but NOT the shared tiers' (their class strings live in `node_modules/@concertable/*/dist`, which auto-detect excludes). A single relative `@source` set can't serve both layouts (monorepo `app/{shared,web/shared,…}` ≠ node_modules `@concertable/*`). Needs a cross-context strategy proven by a carved vite build's generated CSS; tier change → effective on republish.
4. **Mobile metro/nativewind/tailwind retarget** off `../shared` onto `@concertable/mobile` (`watchFolders`, nativewind `input`, tailwind `content`), proven by `expo export` on the precompiled dist; then add mobile to the carve matrix.
5. **FE import-boundary rule** — no ESLint/dependency-cruiser toolchain in `app/` yet; the carve CI is the primary structural boundary today (BE parity: carve = structural gate, build-time guard = fast second layer). Standing up ESLint `no-restricted-imports` across surfaces is a separate sub-project.

Gate: carve-fe jobs green in CI (step 2, post-republish).

## Completed work

- **Phase 2 (this branch):** `@concertable/web` (`0fa7ce511`), `@concertable/mobile` (`5275b6664`),
  `@concertable/customer` src→dist (`c14895d97`), `@concertable/b2b` + its intra-tier import rewrite
  (`ab11c3977`); consumer cutover across all six surfaces + config alias removal + `build:packages` +
  `@concertable/web` `index.css` export + per-surface lucide ambient d.ts (`4d8fdbaa1`); publish
  automation extended to all five tiers with intra-dep pinning (`d974e724d`). Merge of `origin/main`:
  `47612a6d6`.
- Phase 0 registry/PAT setup is complete as recorded by `e0513bac0` and the plan.
- Phase 1 implementation and publication automation landed through PR #301 at feature head `7c9a64a3e`; GitHub merged it as `19be13d330` on 2026-08-02.
- Material Phase 1 commits include `7f8e75d57` (per-file ESM/declarations), `90f4baa8a` (versioning and publish automation), `369f39918` (Node/NodeNext-resolvable emitted imports), `ca1e398ed` (packed-artifact Node and Expo/Metro verification), and `5f9863654` (customer owner-package alignment without starting Phase 2).
- `f57a4c504` fixes the E2E-tier label lookup to fail closed when GitHub label retrieval fails.
- `0e3d8f5a6` makes full merge-queue E2E the strict default, preserves the no-duplicate-local-E2E workflow, and keeps findings on the reviewed branch unless they are proven independent.

## Verification

- **Phase 2 gate (2026-08-05, this branch):** `npm run build:packages` builds all five tiers to dist
  (exit 0). All four web builds green (`npm -w @concertable/web-{customer,venue,artist,business} run
  build` = `tsc -b && vite build`). Both mobile `tsc --noEmit` = 0 errors. Grep-clean: no `../shared/src`
  cross-tree alias in any surface tsconfig/vite config; no surviving `@/`→tier / `shared/` / `@b2b/`
  cross-tree import in any surface or tier source (the 19 `@b2b/` inside `@concertable/b2b` are its own
  intra-package self-alias). `npm install --package-lock-only` reports the lockfile in sync (CI `npm ci`).
- `version-fe-packages.mjs` computes one lockstep version across all five dirs and `--write` pins every
  intra-`@concertable` dep to it (verified `0.1.0-alpha.0.2373`, reverted). `verify-fe-package.mjs`
  passes node-profile on the packed `@concertable/shared` tarball (exit 0).
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
- **The web-surface `@/*` alias is a fallback, not a straight cross-tree map.** `web/customer` (and the b2b surfaces) map `@/components|features|hooks|lib|...` → the web tier *and* a generic `@/*` → `./src/*`, and the trees have overlapping feature names (`concerts`, `user`, `reviews`, …). A blanket rewrite would misroute own-src imports. Cutover resolved each import by checking whether the target file exists in the tier `src`; only then rewrite to the package. Result: 0 own-src imports misrouted (`leftAsOwnSrc: 0` — every rewritten specifier genuinely lived in the tier, matching TS's longest-prefix-wins).
- **CSS can't ride the tsc dist.** The tailwind entry `index.css` (imported as `@concertable/web/shared/index.css` by all four web surfaces) is emitted by no tsc build, so `@concertable/web` exports it directly from `./src/index.css` (added to `files`). Its `@source` globs are relative to that physical file and resolve in-monorepo; carve needs a different content strategy (deferred to Phase 3).
- **lucide-react-native prop augmentation is per-surface now.** `mobile/shared/src/types/lucide.d.ts` (adds `color`/`size`/`strokeWidth`/`className` to `LucideProps`) was previously in surface scope only because the surfaces `include`d the whole tier `src`. After the cutover drops that include, each mobile surface carries its own `lucide-env.d.ts` (its own icon usage needs it regardless of the tier — carve-correct), mirroring the existing per-surface generated `nativewind-env.d.ts`. The tier keeps its own copy for its own build.
- **Metro/nativewind/tailwind runtime configs left for Phase 3.** The Phase 2 gate is build + typecheck; the mobile app's metro `watchFolders`/nativewind `input`/tailwind `content` still point at `../shared` source. The app already resolves `@concertable/shared`/`customer` as symlinked packages the same way, so no in-monorepo runtime regression, but className/class-generation on the precompiled dist is unproven — a first-class Phase 3 item, not a silent gap.

## Event log

### 2026-08-05 — PR #378 code-reviewed clean; updated to current main; enqueuing

- Action: Ran `/code-review` on PR #378 (`6f825b3ee..22959ea5c`). Merged current `origin/main` first (was 2 behind — docs-only PR #379, 0 `app/` files, so the FE gate stands) and pushed `22959ea5c`. Reviewed `carve-fe.mjs`, `verify-fe-package.mjs`, all 11 tier/surface `package.json` (exports maps + closures), and the rename.
- Evidence: `reviews/Feature-platform_polyrepo-fullstack.md` — **no findings**. Diff is 100% frontend (backend lenses N/A); grep confirms 0 stale `/shared` specifiers; green gate proves imports resolve against renamed exports; `carve-fe.mjs` sound and intentionally not yet CI-wired.
- Outcome: #378 is review-clean and current with `main`. Enqueuing via the merge queue (full E2E, no skip-e2e).
- Follow-up: poll to MERGED; then follow `publish-fe-packages.yml` republish to green (unblocks Phase 3b). Worktree branch — no auto-teardown.

### 2026-08-05 — Phase 3a re-synced, re-gated, pushed; PR #378 opened

- Action: Resumed in the dedicated worktree. `git fetch` + merged `origin/main` (was 19 behind, all `api/**`/skills/docs — none touched `app/`; clean merge `4f3f65621`, 0 behind). Fresh `npm ci` in `app/` (first attempt hit the known AV-EPERM npm-cache flake; retried clean, exit 0). Re-ran the full FE gate, then pushed and opened the plain GitHub PR.
- Evidence: gate all exit 0 — `build:packages` (5 tiers), `npm -w @concertable/web-{customer,venue,artist,business} run build` (3708/4399/4389/15 modules), `tsc --noEmit` in `mobile/{customer,b2b}`; `GATE_FAIL=0`. Net diff vs `origin/main` = 206 files, all `app/**`/`plans/**` except a one-line dead-comment removal in `.github/workflows/mirror.yml`; **0 `api/**`**; `test.yml` untouched (carve-fe deferred); no `Skip-E2E` trailer in the commit range. PR #378 OPEN, base `main`, head `4f3f65621`, labels `[]`, 0 behind base.
- Outcome: Phase 3a is delivered as PR #378 with a green in-monorepo gate; full merge-queue E2E will run automatically; no backend platform-sync.
- Follow-up: review + merge #378; its `publish-fe-packages.yml` republish (bare exports) is the gate that unblocks Phase 3b (carve-fe-web CI + `run_fe` gate).

### 2026-08-05 — Phase 3a: tier-subpath rename + surface dep self-declaration + carve harness

- Action: A naming question surfaced the `/shared` subpath inconsistency (`@concertable/web/shared/*` vs bare `@concertable/shared/*`, and b2b's doubled `@concertable/b2b/web/shared/*`). With Tommy, chose to strip the redundant `/shared`/`/web/shared` from all tier specifiers (package names unchanged) — the package already means "shared <tier> platform," so the segment only leaked the monorepo dir layout. Scripted the repo-wide rewrite and rebuilt/verified. Separately drove the feed-restore carve for `web/customer` far enough to expose that surfaces relied on workspace hoisting for the tiers AND for third-party/CSS deps; declared each surface's full closure and wrote `app/scripts/carve-fe.mjs`.
- Evidence: commits `d6ac4b123` (rename: 191 source files + 4 exports maps, `grep` shows 0 leftover old specifiers) and `c4775ebf1` (carve-prep). Gate green — `build:packages` exit 0, four web builds OK, both mobile `tsc --noEmit` 0 errors. carve-fe.mjs proved `web/customer` reaches green `tsc -b` + a 3637-module vite transform restoring `@concertable/*` from the feed. Local `node_modules` corruption (AV-EPERM-aborted installs wiped `.bin`, left `typescript/lib` empty) repaired via a warm-cache `npm install`.
- Outcome: Phase 3a committed on-branch, verified in-monorepo, not pushed. The `carve-fe-web` CI job + `run_fe` gate were written and locally proven but reverted from this change (publish-first: a feed carve can't resolve the renamed imports until the tiers republish).
- Follow-up: push + plain GitHub PR (full merge-queue E2E); after it republishes, add carve-fe-web CI; then the carved CSS `@source` strategy, mobile metro/nativewind retarget, and the ESLint boundary rule.

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

### 2026-08-05 — Phase 2 merged and published; real E2E harness bug fixed en route

- Action: Opened PR #360, code-reviewed (no blocking findings), enqueued full-E2E. The merge-group `e2e-ui-tests` failed twice on fresh stacks — confirmed real, not flake. Root-caused by inspecting `test.yml`: the UI E2E harness serves the SPAs via `npm run dev` (Vite) but built only `@concertable/shared`, so the Phase-2 SPAs couldn't resolve `@concertable/web`/`b2b`/`customer` from dist → every scenario timed out at auth login. Fixed with a `build:web-packages` script wired into both UI E2E build steps; validated the venue Vite dev server resolves the tier dist. Synced to latest `origin/main`, dequeued the stuck auto-requeue via the GraphQL `dequeuePullRequest` mutation, re-enqueued.
- Evidence: PR #360 MERGED `a3f9535` at 13:18Z; winning merge-group run 31007161355 success; fix commit `21daabecd`; `publish-fe-packages.yml` run 31009601005 green, publishing `@concertable/{shared,web,mobile,customer,b2b}@0.1.0-alpha.0.2401` and passing its from-feed verify of every tier. No `api/**` in the diff → no backend sync. Also opened docs PR #364 for the dispatch-prompt convention.
- Outcome: Phase 2 fully terminal on `main`; all five FE tiers on the feed.
- Follow-up: Phase 3 (feed-restore carve CI + import-boundary rule + close the runtime/carve deferrals). Land docs PR #364.

### 2026-08-05 — Phase 2 completed end-to-end

- Action: Synced the branch to `origin/main` (was 32 behind; merged `47612a6d6`, resolved the ledger conflict in our favour), packaged the three remaining tiers (`@concertable/mobile`, `@concertable/customer` src→dist, `@concertable/b2b` incl. its intra-tier import rewrite), cut all six consumer surfaces over from path aliases to package specifiers (existence-checked resolver), stripped cross-tree aliases from every surface tsconfig/vite config, added `build:packages`, the `@concertable/web` `index.css` export, and per-surface lucide ambient d.ts, then extended the FE publish automation (version dep-pinning, package-driven verify, five-tier ordered publish workflow).
- Evidence: commits `5275b6664`, `c14895d97`, `ab11c3977`, `4d8fdbaa1`, `d974e724d` on `47612a6d6`. Gate green — `build:packages` exit 0, four web builds OK, both mobile `tsc --noEmit` 0 errors, grep-clean, lockfile in sync. Version script emits lockstep `0.1.0-alpha.0.2373` with intra-deps pinned (reverted); shared-tarball node verify exit 0.
- Outcome: Phase 2 is complete and committed on the branch; base merge clean; 0 behind `origin/main`.
- Follow-up: Push the branch; open the full-E2E Phase 2 PR when Tommy asks; then Phase 3 (feed-restore carve CI + import-boundary rule) which also closes the metro/nativewind/tailwind + carve-CSS runtime deferrals.

### 2026-08-05 — Resolution model confirmed; first tier packaged (`@concertable/web`)

- Action: Mapped the full Phase 2 cutover surface (6 consumer surfaces, 148 files, per-tier structure, build-order finding); confirmed the dist-only/build-first resolution model with Tommy; packaged `web/shared` as `@concertable/web` end-to-end and verified its `dist` build.
- Evidence: `@concertable/shared` dist prebuilt (exit 0); `@concertable/web` build real exit 0 after fixing 14 standalone-build errors (13× `import.meta.env` → added `vite-env.d.ts`; 1× `.at()` → ES2022 `lib`); `dist` emitted across all subdirs, `tsc-alias` left zero `@/` specifiers, external `@concertable/shared/*` specifiers intact; committed `0fa7ce511`; `node_modules` primed via `npm ci` (exit 0) and `npm install` registered the new workspace with a clean prior lockfile.
- Outcome: The tier-packaging recipe is proven and one of four tiers is done and committed on the isolated branch.
- Follow-up: Replicate the recipe to `@concertable/mobile`, `@concertable/customer`, `@concertable/b2b`, then the consumer/intra-tier import cutover, publish-automation extension, and the build gate.
