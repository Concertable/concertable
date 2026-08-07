# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-platform_polyrepo_mobile-carve` — created from current `origin/main` @ `59bdd7a8a` for the definitive Phase 3 mobile carve gate.
- Branch: `Feature/platform_polyrepo_mobile-carve` @ work head `098d4c3a5`; contains the transferred ledger recovery history plus the two mobile `carve-fe` matrix entries against the now-published fixed tier. Local ledger-only checkpoints after `098d4c3a5` describe delivery transitions.
- PR: **mobile-retarget PR [#413](https://github.com/Concertable/concertable/pull/413) MERGED as `62646f4cdd6933a695fc790e1329588fce3f928a`** — reviewed source head `a8296d51b`; full merge-group run [31195035539](https://github.com/Concertable/concertable/actions/runs/31195035539) completed successfully. Prior: carved-web CSS **[#405](https://github.com/Concertable/concertable/pull/405) MERGED** (`d9c62e2c5`) + [#411]; Phase 3b [#389] (`1cbeb2175`) + [#398]; Phase 3a [#378] (`fba490e25`); Phase 2 [#360] (`a3f9535`); Phase 1 [#301]+[#319].
- Dependency/package gates: **#413 FE publication DONE** — successful descendant run [31197751649](https://github.com/Concertable/concertable/actions/runs/31197751649) published and feed-verified `@concertable/mobile@0.1.0-alpha.0.2571` with the brand assets. This unblocks the follow-up mobile carve gate. No `api/**` → no backend platform-sync.
- Last reconciled: 2026-08-07 — work head `098d4c3a5` is committed and PR-preflight green: proper feature branch, clean tree, 0 behind current `origin/main`, no existing PR, no open platform-sync gate, and no package cut-over. Next: push the work head with the plan checkpoint transport, open the self-validating PR, and follow both mobile carve jobs.

## Current state

Phases 0, 1, 2, **3a, and 3b are on `main`**. Phase 3a (PR #378, merge `fba490e25`) landed the tier-subpath rename (`@concertable/<tier>/<path>`, no `/shared`) + surface dependency-closure self-declaration + the `carve-fe.mjs` harness. Phase 3b (PR #389, merge `1cbeb2175`) landed the FE carve **gate**: `test.yml` now carries the `run_fe` classifier + the `carve-fe-web` matrix job (4 web surfaces feed-restore-and-build standalone) + its `ci-complete` wiring. Phases 0–3b are fully terminal.

**Carved-web CSS `@source` strategy — MERGED (#405, `d9c62e2c5`) + republished. TERMINAL.** `app/web/shared/src/index.css` adds two dist-scanning `@source` globs (`../dist/**/*.js` for the `@concertable/web` tier — same offset from the file in both layouts; `../../b2b/dist/**/*.js` for `@concertable/b2b`). The existing sibling-`src` globs are kept: each is inert in the layout it doesn't belong to, and Tailwind silently ignores an `@source` matching nothing, so both sets coexist. Only `@concertable/{web,b2b}` carry web class strings (shared/customer tier dists have 0 classNames — logic-only). Proven by a local carved-layout vite build (tiers packed into `node_modules`): the tier canary classes go from **absent (baseline) → present (fixed)** for customer (`@concertable/web`) and venue (`@concertable/web` + `@concertable/b2b`).

**Mobile retarget + asset fix — IMPLEMENTED on `Feature/platform_polyrepo_mobile-retarget` (`1d29804a9`), PR #413 OPEN (publish-first).** (a) Both `app/mobile/{customer,b2b}` `metro.config.js` (`watchFolders` + NativeWind `input`) and `tailwind.config.js` (`content`) now `require.resolve("@concertable/mobile/…")` + scan the package's compiled `dist/**/*.js` instead of the `../shared` sibling; `App.tsx` imports `@concertable/mobile/global.css`. `@concertable/mobile` is the only className-bearing mobile tier (208 vs 0). (b) **Pre-existing tier bug fixed:** `@concertable/mobile`'s `Logo` `require`d `assets/brand/logo*.png` that the package never shipped (`files` lacked `assets`) and that physically lived outside the package (`app/mobile/assets/`), so the path resolved nowhere in-monorepo OR carved — the mobile app had never actually been bundled, only `tsc`'d. Moved `brand/` into the tier (`app/mobile/shared/assets/`, making the existing `../../../assets/brand` path correct) + `files` += `assets`; app-icon assets (icon/splash/adaptive/favicon) stay at `app/mobile/assets/` (surface `app.json`). (c) `carve-fe.mjs`'s mobile branch runs `expo export` after `tsc --noEmit`; the carve job is renamed `carve-fe-web` → `carve-fe`. **Mobile is deliberately OUT of the `carve-fe` matrix here** (the carve restores the tier from the feed, so the asset fix must republish first).

**Phase 3 remaining:** (1) the mobile carve **gate** follow-up (below); (2) the ESLint import-boundary rule.

## Next Steps

**1. Push, open, and follow the mobile carve gate PR.** Push work head `098d4c3a5` first, verify the remote ref, transport the resulting ledger checkpoint, and open the plain GitHub PR. Follow `carve-fe (mobile/customer)` and `carve-fe (mobile/b2b)` to green; if either exposes another latent bundle bug, fix the real bug without weakening the gate (surface bug on this branch; published-tier bug through another publish-first cycle). No E2E label is set at PR creation; the merge workflow owns the full-E2E decision. (Also logged in `app/mobile/TECH_DEBT.md`.)

**2. FE import-boundary rule** — no ESLint/dependency-cruiser toolchain in `app/` yet; the carve CI is the primary structural boundary today (BE parity: carve = structural gate, build-time guard = fast second layer). Standing up ESLint `no-restricted-imports` across surfaces is a separate sub-project.

Then Phase 4 (FE platform-sync) and Phase 5 (produce full-stack repos, D-A/D-B) per the plan.
Gate: each item ends with its own green carve/build proof on its PR.

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

- **Mobile carve matrix implementation (2026-08-07):** `.github/workflows/test.yml` now lists all six
  surfaces, including `mobile/customer` and `mobile/b2b`; the existing classifier self-triggers
  `run_fe=true` when `test.yml` changes; `git diff --check` passes. The local shell has neither PyYAML
  nor Node on `PATH`, so the PR's own workflow parse plus its two new matrix jobs are the authoritative
  executable proof.

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

### 2026-08-07 — mobile-carve PR preflight green

- Action: Refreshed `origin`, checked branch identity, tree state, base drift, existing PRs, platform-sync gates, and the net branch paths before publication.
- Evidence: branch `Feature/platform_polyrepo_mobile-carve`; `origin/main...098d4c3a5` = `0 behind / 11 ahead`; working tree clean; no existing PR; no open `chore/platform-sync-*`; net paths are only `.github/workflows/test.yml` and the active ledger; no `api/**` or published-package cut-over.
- Outcome: preflight is GREEN. Work head `098d4c3a5` is clear to push and open as a plain GitHub PR.
- Follow-up: push the work head, verify remote equality, transport the push checkpoint, then create the PR.

### 2026-08-07 — mobile carve matrix enabled locally

- Action: Removed the now-stale publish-first deferral text and added `mobile/customer` plus `mobile/b2b` to the existing `carve-fe` surface matrix.
- Evidence: matrix assertion finds the exact six-surface list; the unchanged classifier contains its explicit `.github/workflows/test.yml` self-trigger; `git diff --check` passes. `carve-fe.mjs` already maps both mobile surfaces and runs `tsc --noEmit` followed by `expo export --platform android`.
- Outcome: the definitive feed-restored mobile bundling gate is implemented locally; the PR will execute both new matrix jobs against `@concertable/mobile@0.1.0-alpha.0.2571`.
- Follow-up: commit, push, open the PR, and follow both mobile carves to green without weakening the gate.

### 2026-08-07 — superseded closeout worktree removed

- Action: Verified the closeout worktree was clean and contained only the active ledger tail, then removed it and deleted its local branch.
- Evidence: closeout `status --porcelain` empty; `origin/main..HEAD` path set = `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; worktree path no longer exists; `Docs/platform_polyrepo_mobile-retarget_closeout` deleted.
- Outcome: `Feature/platform_polyrepo_mobile-carve` is the sole recovery anchor and is ready for implementation.
- Follow-up: add the two mobile matrix entries and deliver the self-validating carve PR.

### 2026-08-07 — recovery transferred to fresh mobile-carve worktree

- Action: Confirmed no open red platform-sync PR, created `Feature/platform_polyrepo_mobile-carve` from current `origin/main` @ `59bdd7a8a`, and cherry-picked the eight ledger-only closeout commits in order.
- Evidence: every transferred commit changes only `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; source closeout and destination feature ledger blobs match exactly at `e99282b00f954541a89ae0215a074b433ae89705`; new feature worktree is clean before this identity edit.
- Outcome: the fresh feature worktree is the recovery anchor and is ready for the two-line CI matrix change; no runtime/source change was transferred.
- Follow-up: remove the superseded closeout worktree/branch, then implement and deliver the mobile carve matrix entries.

### 2026-08-07 — `@concertable/mobile@0.1.0-alpha.0.2571` published and feed-verified

- Action: Located the first successful frontend-package publication whose head contains #413's merge, then inspected its job and package logs for the mobile tier and feed-verification result.
- Evidence: run [31197751649](https://github.com/Concertable/concertable/actions/runs/31197751649) at `59bdd7a8a` is a descendant of #413 merge `62646f4cd`, completed `success`, and published all five tiers at `0.1.0-alpha.0.2571`; the log records `+ @concertable/mobile@0.1.0-alpha.0.2571` and the subsequent `verify-fe-package.mjs "@concertable/mobile@$VERSION" @concertable/mobile --metro-only` feed verification inside the successful job.
- Outcome: the fixed mobile tier, including its packaged brand assets, is live on the GitHub npm feed; the publish-first prerequisite for mobile carve CI is terminal green. The closeout worktree now lives at the shorter writable `.worktrees\Docs-polyrepo-mobile-closeout` path.
- Follow-up: start the fresh mobile-carve branch/worktree and add `mobile/customer` plus `mobile/b2b` to the `carve-fe` matrix.

### 2026-08-07 — merged mobile-retarget worktree and branch removed

- Action: Verified the source worktree was clean and that every commit after reviewed PR head `a8296d51b` changed only the active ledger, then removed the merged worktree and deleted the local source branch.
- Evidence: source `status --porcelain` empty; tail path set = `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; `git worktree remove --force` succeeded; local `Feature/platform_polyrepo_mobile-retarget` deleted. The remote-delete command reported the ref already absent; `git fetch --prune`, `git ls-remote --heads`, worktree listing, and branch listing confirm no source worktree or local/remote source branch remains.
- Outcome: the closeout worktree is the sole recovery anchor for the remaining FE publication gate.
- Follow-up: identify the `publish-fe-packages` run for merge `62646f4cd` and follow it to terminal success.

### 2026-08-07 — post-merge recovery state transferred to the closeout worktree

- Action: Fetched current `origin/main`, created `Docs/platform_polyrepo_mobile-retarget_closeout` from `59bdd7a8a` at the shorter `C:\tmp\Concertable-polyrepo-mobile-closeout` path, and cherry-picked the five observation checkpoints after reviewed source head `a8296d51b` in order.
- Evidence: every source-tail commit (`e479a2e7c`, `74aac0b3a`, `a611d4b7a`, `8a0ee4458`, `d26dd0f11`) changes only `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; the final source and closeout ledger blobs are identical (`23adc093ce0eb1e053cfe2585352a3afb86c554b`); `62646f4cd` is an ancestor of current `origin/main`; closeout worktree is clean before this identity edit.
- Outcome: recovery ownership has moved to the clean docs closeout branch; no runtime/source change was transferred.
- Follow-up: remove the merged feature worktree and both source-branch refs, then monitor the FE package publication from this closeout worktree.

### 2026-08-07 — #413 merged through the full-E2E merge queue

- Action: Monitored the exact merge-group formation for #413 to a terminal result without retrying or mutating the queue.
- Evidence: merge-group run [31195035539](https://github.com/Concertable/concertable/actions/runs/31195035539), branch `gh-readonly-queue/main/pr-413-68f6383487538cf631787d9afc1c03f107c0af2a`, completed `success`; the next PR poll reported `MERGED` with merge commit `62646f4cdd6933a695fc790e1329588fce3f928a`.
- Outcome: the mobile retarget and `@concertable/mobile` brand-asset packaging fix are on `main`, with full queue E2E green.
- Follow-up: move the observation tail to a docs closeout worktree, delete the merged feature worktree/branch, and follow the resulting FE package publication to success.

### 2026-08-07 — #413 admitted to the merge queue at position 1

- Action: Issued the enable-only queue command after the explicit-method form was rejected, then verified admission directly through GraphQL.
- Evidence: `gh pr merge 413 --auto` reported `already queued to merge`; the authoritative follow-up query reports PR `OPEN`, source head `a8296d51b`, `mergeQueueEntry.state=AWAITING_CHECKS`, `position=1`, and no source-head change. A transient 401 affected only the same command's follow-up display; the independent GraphQL retry succeeded.
- Outcome: the one-time re-evaluation remedy worked and #413 is now in the full-E2E merge queue.
- Follow-up: monitor the specific merge-group run to merge or a terminal failure; never retry a failing run.

### 2026-08-07 — #413 auto-merge disable succeeded; explicit-method re-enable rejected

- Action: Confirmed the source range has no `Skip-E2E`/`Skip-E2E-UI` trailers, then applied the sanctioned one-time disable/re-enable sequence.
- Evidence: `SKIP_TRAILERS=none`; `gh pr merge 413 --disable-auto` succeeded; `gh pr merge 413 --merge --auto` returned `The merge strategy for main is set by the merge queue`; the follow-up PR query shows `OPEN/CLEAN`, head `a8296d51b`, `autoMergeRequest=null`.
- Outcome: the reviewed source head is unchanged and green, but auto-merge is disabled because the current CLI/repository combination rejects an explicit method when the queue owns it. This is a command-shape failure, not a check retry or queue failure.
- Follow-up: issue the enable-only command without an explicit merge method, then verify the GraphQL queue entry.

### 2026-08-07 — #413 green but never admitted to the merge queue

- Action: Queried the merge-queue entry and recent merge-group runs for #413 after confirming its terminal green PR-head checks.
- Evidence: PR remains `OPEN/CLEAN` at `a8296d51b`; auto-merge was enabled at `2026-08-07T14:27:09Z`; GraphQL reports no `mergeQueueEntry`; the recent `merge_group` run list contains no branch for `pr-413-`; therefore no hidden merge-group failure exists.
- Outcome: GitHub's sustained green-but-unadmitted re-evaluation glitch is confirmed. The sanctioned one-time remedy is to disable and re-enable auto-merge; this is not a retry of a failed check.
- Follow-up: re-assert auto-merge once and verify admission before waiting for the queue result.

### 2026-08-07 — PR #413 review-clean, current, and PR-head green; auto-merge enabled

- Action: Resolved the existing mobile-retarget worktree instead of creating a duplicate, fetched current remote state, confirmed the code review, and reconciled the PR's complete head-check set before the merge-queue gate.
- Evidence: worktree clean; local/remote source head and PR `headRefOid` all `a8296d51b`; `HEAD..origin/main` = 0; code review `529dba9dd..ce49f788` in `reviews/Feature-platform_polyrepo_mobile-retarget.md` reports no issues, with `a8296d51b` adding only that review artifact; PR #413 is `OPEN/CLEAN`, auto-merge enabled, no labels, and every build/web-carve/backend-carve/unit/integration/`ci-complete` check is `SUCCESS`. PR-level E2E checks are `SKIPPED` as designed because full E2E runs on the merge group.
- Outcome: reviewed source head `a8296d51b` is ready for full merge-queue execution. This local checkpoint is observation-only and must not be pushed to the source PR.
- Follow-up: verify queue admission and merge-group results; on merge, transfer the observation tail to a docs closeout worktree before watching the FE publication gate.

### 2026-08-07 — carve-fe exposed a pre-existing mobile-bundling bug; asset fix + publish-first restructure

- Action: The mobile carve (added to the `carve-fe` matrix, run in CI on #413) failed. First failure: `App.tsx` imported `../shared/global.css` (fixed → `@concertable/mobile/global.css`, `c44a0b9a5`). Second failure (after that): `@concertable/mobile/dist/components/ui/Logo.js` `require`d `../../../assets/brand/logo-long.png` — but the tier shipped no `assets/` (`files` lacked it) and the brand images physically lived at `app/mobile/assets/` (outside the tier). That path resolves nowhere in-monorepo either → the mobile app had **never been bundled**, only `tsc`'d. Fixed by `git mv app/mobile/assets/brand → app/mobile/shared/assets/brand` (existing `../../../assets/brand` path now correct) + `files` += `assets` (`1d29804a9`). Restructured #413 **publish-first**: removed `mobile/{customer,b2b}` from the `carve-fe` matrix, because the carve restores the tier from the feed and the asset fix isn't effective until republish.
- Evidence: CI run 31188407992 mobile/customer carve bundled **4325 modules** (proving the retargeted metro/NativeWind/tailwind config resolves `@concertable/mobile` from the feed) before failing on the Logo asset. `@concertable/mobile` `files` now `["dist","global.css","nativewind-env.d.ts","assets"]`. App-icon assets (icon/splash/adaptive/favicon) left at `app/mobile/assets/` (surface `app.json` refs `../assets/*` unchanged). Logged the "mobile never bundled → more latent bugs likely" debt in `app/mobile/TECH_DEBT.md`. Local verification remains impossible (Windows AV blocks heavy `app/` npm).
- Outcome: #413 is a coherent publish-first PR — retarget + tier asset fix; web-only carve matrix so it can merge + republish the fixed tier.
- Follow-up: merge #413 → republish `@concertable/mobile` (with assets) → follow-up PR re-adds `mobile/{customer,b2b}` to the `carve-fe` matrix; chase any further latent bundle bugs the now-runnable carve surfaces.

### 2026-08-07 — Mobile metro/nativewind/tailwind retarget implemented + carve-wired; PR #413 open

- Action: Reconciled a stale ledger (its `## Next Steps` preamble still read "carved-web CSS PR being opened" — actually terminal: #405 merged `d9c62e2c5`, republished, closeout #411 merged 09:33Z). Created worktree `Feature/platform_polyrepo_mobile-retarget` off `origin/main` @ `529dba9dd` and did Phase 3 item 1: retargeted both mobile surfaces' `metro.config.js` (`watchFolders` + NativeWind `input` via `require.resolve("@concertable/mobile/…")`) and `tailwind.config.js` (`content` → `@concertable/mobile/dist/**/*.js`, path-normalized to POSIX for fast-glob); upgraded `carve-fe.mjs`'s mobile branch to `tsc --noEmit` + `expo export`; renamed the CI carve job `carve-fe-web` → `carve-fe` and added `mobile/{customer,b2b}` to its matrix + `ci-complete`.
- Evidence: commit `92d48f6db` (6 files). Empirically scoped: `@concertable/mobile` src has 208 `className=` across 39 files; `@concertable/customer` and `@concertable/shared` src have 0 — so `@concertable/mobile` is the only className-bearing mobile tier and the `../shared`→package swap is complete. All 5 edited files pass `node --check`; `test.yml` passes `yaml.safe_load`; no stale `carve-fe-web` refs remain; `run_fe` (line 110/111) fires on both the `app/` config changes and the `test.yml` change, so #413 runs the full `carve-fe` matrix incl. the two mobile surfaces. PR #413 OPEN, base `main`, no skip labels (full E2E — CI-workflow + multi-surface bundler-config change = broad blast radius).
- Environment blocker: could **not** run the local `expo export`/carve pre-proof. Every heavy `app/` npm `install`/`rm` here is stalled or killed by Windows AV file-locking (repeated `ENOTEMPTY` on rmdir during reconcile, `Permission denied` on `mv node_modules`, and long installs terminated) — the exact condition the ledger documents for Phase 3a/3b ("local carve couldn't finish in the shell window — so CI is the validation"). Correctness rests on: the change mirrors the proven web #405 fix; `@concertable/mobile` exports `./package.json`, `./global.css`, and `./*`→`dist/*.js`, so both `require.resolve` calls resolve in-monorepo (symlink→`app/mobile/shared`, identical to the old `../shared`) and when carved (`node_modules/@concertable/mobile`); className string literals survive tsc (`jsx: react-jsx`), the same mechanism the web dist-scan relies on.
- Outcome: mobile retarget implemented, committed, pushed; PR #413 open. The Linux `carve-fe` mobile `expo export` job is the definitive proof.
- Follow-up: follow #413 to merged + green `carve-fe`; if the mobile carve is red, debug on-branch (fix the config/harness, never weaken the gate) and push. Then Phase 3 item 2 (ESLint import-boundary) on a fresh branch, then Phase 4/5.

### 2026-08-07 — Carved-web CSS PR #405 MERGED (rode out a GitHub Actions outage) + republished

- Action: Opened PR #405, code-reviewed clean (`reviews/Feature-platform_polyrepo_carved-css.md`, no findings), enabled auto-merge. Hit the GitHub auto-merge **re-eval glitch** (#3 — green but never admitted ~45 min) → one-time re-assert (disable+enable) admitted it to the queue. The merge_group then failed **4 times**; root-caused via job logs to a **GitHub Actions platform outage** (`Failed to resolve action download info. Error: Service Unavailable`/`Internal Server Error`, ~15:28–16:25Z) that killed jobs at startup across unrelated backend + FE areas (the `changes` gate, `carve-auth`, `e2e-api-tests`, Kernel/Shared.Api unit tests) — NOT the change (a web-CSS `@source` edit cannot fail backend C#/carve/api-e2e, and `carve-fe-web` builds against the feed's old tiers). Did not blind-retry; the queue kept re-forming and, once GitHub recovered, a clean group merged.
- Evidence: `#405 MERGED d9c62e2c5` at 2026-08-06T16:42:23Z, on `origin/main`. `publish-fe-packages` run 31120764923 (16:42Z) = success → all 5 FE tiers republished with the fixed `index.css`. The 4 failed merge_group runs (31115677323 / 31117452350 / 31118122016 / 31119279785) all show the identical Service-Unavailable action-download signature. Two intermediate confirm-loop "CI FAILED" reports were false positives from a `gh run list` filter (corrupted by `2>&1`) miscounting other PRs' failures — corrected by polling the specific run id.
- Outcome: carved-web CSS fix live on `main` and effective in carved surfaces (republished). Feature worktree pruned; this closeout lands via the docs (no-E2E) path.
- Follow-up: Phase 3 remaining — mobile metro/nativewind retarget (+ mobile carve), then the ESLint import-boundary rule; each on a fresh branch off `origin/main`.

### 2026-08-06 — Carved-web CSS `@source` strategy implemented + carve-proved

- Action: Resumed from a stale ledger (Next Steps step 1 read "review + merge #389") and found it already terminal — #389 MERGED (`1cbeb2175`), closeout #398 MERGED. The `platform_polyrepo-fullstack` worktree was concurrently **pruned** mid-session (its `Docs/polyrepo_3b-closeout` branch had merged — expected cleanup, briefly looked alarming as its contents shifted). Reconciled to post-#398/#402 `origin/main`, created `Feature/platform_polyrepo_carved-css` off it for the real current Next Step. Diagnosed the mechanism empirically in a locally-reproduced carved layout (tiers `npm pack`ed into `node_modules`, other deps resolved up to `app/node_modules`, real `vite build`): `@config` content is **silently ignored** by `@tailwindcss/vite` v4.3; a bare-package `@import` of a partial **fails** (exports map blocks the subpath); a direct `@source` into the tier dist **works**; a non-matching `@source` glob is **silently ignored**. Chose the minimal additive fix.
- Evidence: `app/web/shared/src/index.css` keeps the monorepo `src` globs and adds `@source "../dist/**/*.js"` (`@concertable/web` — `../dist` is the same offset from the file at `app/web/shared/src` and at `node_modules/@concertable/web/src`) + `@source "../../b2b/dist/**/*.js"` (`@concertable/b2b`, carved path). Carved before/after per surface: customer `@concertable/web` canaries (`animate-pulse`, `aspect-square`) absent→PRESENT (css 21106→102156 B); venue `@concertable/web` + `@concertable/b2b` canaries (`bg-background`, `border-2`, `border-current`) absent→PRESENT (22729→103628 B). Only `@concertable/{web,b2b}` dists carry classNames (`@concertable/{shared,customer}` = 0 — logic-only tiers). In-monorepo regression gate: all four web builds green (`GATE_FAIL=0`); the added globs are inert/dup in-monorepo. Working-tree diff = `index.css` only; throwaway proof harness deleted.
- Environment note: the initial `npm ci` was **AV-EPERM-killed**, leaving several packages partially extracted (`recharts`, `react-native-gesture-handler`, `@gorhom/bottom-sheet` missing their `.d.ts`). `recharts` was repaired (unblocked the web builds). The `@concertable/mobile` tier build / mobile typecheck still fail on the RN partials — an **install artifact, not a regression** (a web-tier CSS change cannot affect mobile, which does not consume `@concertable/web`); the merge-queue runs a clean install.
- Outcome: carved-web CSS strategy implemented + carve-proved on-branch and committed. PR being opened.
- Follow-up: open the PR (full E2E; `publish-fe-packages` republishes on merge → fix effective in carved surfaces), follow to merged + green republish; then Next Steps (mobile retarget, ESLint boundary).

### 2026-08-06 — Phase 3b PR #389 MERGED (rode out re-eval glitch + UI-E2E flake)

- Action: After enabling auto-merge, the PR sat `OPEN/CLEAN` but unadmitted for ~9 min → the GitHub auto-merge re-eval glitch (#3), not a failure. One-time re-assert (`gh pr merge 389 --disable-auto` then `--merge --auto`) → `queue=QUEUED`. First merge-queue run (31089695254) FAILED on `e2e-ui-tests` ("Run B2B UI E2E tests") with Playwright "Timeout waiting for event Response" (30s/60s). Classified **flake**: the suite booted fine (not the startup-death signature) and #389's diff is `test.yml`-only — incapable of causing a UI-scenario Response timeout. Did NOT blind-retry; the queue had already auto-re-formed a fresh group (31089986474), which is the legitimate fresh-stack tiebreaker. It passed clean → PR merged.
- Evidence: `#389 MERGED (1cbeb2175)` at 2026-08-06; failed run 31089695254 failed jobs = `e2e-ui-tests` + `ci-complete` (aggregator); fresh run 31089986474 conclusion=success; `git branch -r --contains 1cbeb2175` shows `origin/main`. Post-merge: latest `publish-fe-packages` run is still #378's [31045651556] (no FE republish — test.yml-only); no ours-owned platform-sync (0 `api/**`); `chore/platform-sync-0.1.0-alpha.0.833` #396 is another PR's api sync (green-so-far), not ours.
- Outcome: Phases 0–3b terminal on `main`. The FE carve gate (`run_fe` + `carve-fe-web` + `ci-complete`) is live and self-proven.
- Follow-up: remaining Phase 3 items (carved-web CSS `@source`, mobile retarget, ESLint boundary) on fresh branches off `origin/main`.

### 2026-08-06 — Phase 3b PR #389 re-synced to current main; reviewing + enqueuing

- Action: Resumed in the dedicated worktree. #389's four `carve-fe-web` surfaces + build/unit/integration were already green (E2E auto-skipped as a CI+doc-only diff). Branch was 13 behind `origin/main`; per the pre-auto-merge currency rule, `git merge origin/main --no-edit` (clean — main never touched `test.yml`), 0 behind. Verified the change survived: net 3-dot diff = `test.yml`+ledger only, `carve-fe-web`/`run_fe` intact, `python -c yaml.safe_load` OK.
- Evidence: merge commit; `git rev-list --count HEAD..origin/main` = 0; `origin/main` @ `6586122b8`; the merge pulled 13 commits (money-value-type refactor, merge-review-gate hook, review-authz) — all `api/**`/`.claude`/docs, none touching the FE packages or `test.yml`.
- E2E-tier decision: **no skip-e2e, no full-e2e — the classifier already runs full E2E in the queue.** Read the classifier: `test.yml` isn't in `$INERT`, so it's a non-inert, non-package "code change" (line 104) → `run_e2e`/`run_e2e_ui` stay true; line 149 only suppresses E2E on non-`merge_group` events, so the "skipping" seen via `gh pr checks` is PR-level, and the merge queue runs full API+UI E2E. Adding `full-e2e` would be redundant; `skip-e2e` would be wrong. (Unlike #319, which changed E2E *policy*, this changes *carve* policy only — but here the classifier makes the point moot.)
- Review (Lens A/F, CI-only diff so B–E N/A): `$INERT` in scope at run_fe (defined line 91); run_fe logic correct (non-inert `^app/` OR carve-mechanism self-change); carve-fe-web job well-formed (`needs:[changes]`, `if run_fe`, `packages: read` + `GITHUB_TOKEN`, matrix over the 4 web surfaces); `ci-complete` (`if: always()`, fails only on failure/cancelled) correctly tolerates a **skipped** carve-fe-web on BE-only PRs, same as the existing conditional carve-* jobs. **No findings.**
- Outcome: #389 current with `main`, change intact. Next: `/code-review` (merge-review-gate hook now requires `reviews/Feature-platform_polyrepo_carve-fe-ci.md` stamped at HEAD), then `gh pr merge 389 --merge --auto` + poll to MERGED.

### 2026-08-05 — Phase 3a MERGED + republished green; Phase 3b (carve-fe CI) authored

- Action: Enqueued #378; the merge queue found a mid-air collision with cookie-consent PR #377 (`mergeState=DIRTY`, all four web `main.tsx`). Merged `origin/main`, resolved the four conflicts (kept #377's ConsentProvider/CookieConsentBanner/ManageCookiesButton wiring on the bare specifiers), reconciled the lockfile (`--package-lock-only`, already consistent), re-ran the full FE gate green, pushed. Re-enqueued; hit the GitHub auto-merge re-eval glitch once (CLEAN-but-unqueued ~6 min) → re-asserted auto-merge → QUEUED position 1 → merged. Then authored Phase 3b on a fresh branch.
- Evidence: #378 MERGED `fba490e25` 20:46Z; two merge_group formations completed success (E2E green, base-churn re-formed the group). `publish-fe-packages.yml` run 31045651556 = success (5 tiers republished, bare exports, from-feed verify passed). Phase 3b: `test.yml` +40 lines (run_fe output + logic, carve-fe-web matrix job, ci-complete wiring); YAML parses; run_fe unit-tested on 7 file-sets; local carve couldn't finish in the 9-min shell window (heavy npm install) so CI is the validation.
- Outcome: Phases 0–3a terminal on `main` with tiers republished. Phase 3b authored + ledger-checkpointed, ready to push + PR.
- Follow-up: push `Feature/platform_polyrepo_carve-fe-ci`, open the plain PR, review + merge; carve-fe-web self-validates on it. Then Next Steps 2–4 (carved CSS `@source`, mobile retarget, ESLint boundary).

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
