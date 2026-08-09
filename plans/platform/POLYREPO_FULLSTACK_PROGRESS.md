# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-platform_polyrepo_import-boundary` — dedicated Phase 3 import-boundary checkout, current with `origin/main` at `c72b058af` through merge `26d84f69d`.
- Branch: `Feature/platform_polyrepo_import-boundary` — direct owner of the remaining Phase 3 import-boundary work.
- PR: **import-boundary PR [#428](https://github.com/Concertable/concertable/pull/428) OPEN** at verified remote head `149f7a4db`, base `main`, not draft, no labels. Replacement run 31308852277 completed 45/46 jobs green but its final B2B Venue integration runner remains stuck; the same exact head passed the Venue module locally 25/25 in 1.4 minutes. This diagnostic checkpoint is the sole local tail. Prior mobile-carve PR [#416](https://github.com/Concertable/concertable/pull/416) merged as `83a3f49a1`; publish-first mobile-retarget PR [#413](https://github.com/Concertable/concertable/pull/413) merged as `62646f4cd` and republished `@concertable/mobile@0.1.0-alpha.0.2571`; carved-web CSS [#405] (`d9c62e2c5`); Phase 3b [#389] (`1cbeb2175`); Phase 3a [#378] (`fba490e25`); Phase 2 [#360] (`a3f9535`); Phase 1 [#301]+[#319].
- Dependency/package gates: **#413 FE publication DONE** — successful descendant run [31197751649](https://github.com/Concertable/concertable/actions/runs/31197751649) published and feed-verified `@concertable/mobile@0.1.0-alpha.0.2571` with the brand assets. This unblocks the follow-up mobile carve gate. No `api/**` → no backend platform-sync.
- Last reconciled: 2026-08-09 — exact-head run 31308852277 reached 45 completed green jobs, then its B2B Venue integration job remained in-progress abnormally long. A short detached worktree at `149f7a4db` ran `scripts/integration.ps1 venue` green (25/25, 1.4 minutes) and was removed; this proves a runner-specific hang rather than a branch regression.

## Current state

Phases 0, 1, 2, **3a, and 3b are on `main`**. Phase 3a (PR #378, merge `fba490e25`) landed the tier-subpath rename (`@concertable/<tier>/<path>`, no `/shared`) + surface dependency-closure self-declaration + the `carve-fe.mjs` harness. Phase 3b (PR #389, merge `1cbeb2175`) landed the FE carve **gate**: `test.yml` now carries the `run_fe` classifier + the `carve-fe-web` matrix job (4 web surfaces feed-restore-and-build standalone) + its `ci-complete` wiring. Phases 0–3b are fully terminal.

**Carved-web CSS `@source` strategy — MERGED (#405, `d9c62e2c5`) + republished. TERMINAL.** `app/web/shared/src/index.css` adds two dist-scanning `@source` globs (`../dist/**/*.js` for the `@concertable/web` tier — same offset from the file in both layouts; `../../b2b/dist/**/*.js` for `@concertable/b2b`). The existing sibling-`src` globs are kept: each is inert in the layout it doesn't belong to, and Tailwind silently ignores an `@source` matching nothing, so both sets coexist. Only `@concertable/{web,b2b}` carry web class strings (shared/customer tier dists have 0 classNames — logic-only). Proven by a local carved-layout vite build (tiers packed into `node_modules`): the tier canary classes go from **absent (baseline) → present (fixed)** for customer (`@concertable/web`) and venue (`@concertable/web` + `@concertable/b2b`).

**Mobile retarget + asset fix — MERGED (#413, `62646f4cd`) + republished.** (a) Both `app/mobile/{customer,b2b}` `metro.config.js` (`watchFolders` + NativeWind `input`) and `tailwind.config.js` (`content`) now `require.resolve("@concertable/mobile/…")` + scan the package's compiled `dist/**/*.js` instead of the `../shared` sibling; `App.tsx` imports `@concertable/mobile/global.css`. `@concertable/mobile` is the only className-bearing mobile tier (208 vs 0). (b) **Pre-existing tier bug fixed:** `@concertable/mobile`'s `Logo` `require`d `assets/brand/logo*.png` that the package never shipped (`files` lacked `assets`) and that physically lived outside the package (`app/mobile/assets/`), so the path resolved nowhere in-monorepo OR carved — the mobile app had never actually been bundled, only `tsc`'d. Moved `brand/` into the tier (`app/mobile/shared/assets/`, making the existing `../../../assets/brand` path correct) + `files` += `assets`; app-icon assets (icon/splash/adaptive/favicon) stay at `app/mobile/assets/` (surface `app.json`). (c) `carve-fe.mjs`'s mobile branch runs `expo export` after `tsc --noEmit`; the carve job is renamed `carve-fe-web` → `carve-fe`. The published fixed tier is feed-verified at `@concertable/mobile@0.1.0-alpha.0.2571`.

**Mobile carve gate — MERGED (#416, `83a3f49a1`).** Both `mobile/customer` and `mobile/b2b` restored
the published tiers from the feed, type-checked, and completed `expo export` on the PR head and in the
successful full-E2E merge-group run 31204805838. The resolved mobile bundling entry has been removed
from `app/mobile/TECH_DEBT.md`.

**Phase 3 import-boundary implementation is locally complete and review-fixed.** `dependency-cruiser` now enforces that each of the 11 frontend workspaces can reach another workspace only through its published `@concertable/*` package. The runner loads every workspace's own tsconfig, so relative and alias-based source reaches are both covered. A negative test injects one cross-surface and one tier-source import and requires both violations; CI runs that proof plus the clean scan as the independent `fe-boundaries` job, and `ci-complete` requires it. Review exposed that directly spawning the npm `.cmd` shim fails with `EINVAL` on Windows; the runner now invokes dependency-cruiser's JavaScript entrypoint through Node. Delivery remains live until the updated reviewed head passes replacement checks and merges through the full-E2E queue.

## Next Steps

**1. Finish and land import-boundary PR #428.** Commit and push this exact-head Venue diagnostic checkpoint, verify equality, and require the resulting independent replacement run; do not cancel or retry stuck run 31308852277. Recheck currency immediately at terminal green and enqueue full E2E if current. Phase 3 becomes terminal only after the merge is recorded and its recovery state is transferred to a closeout worktree.

Then Phase 4 (FE platform-sync) and Phase 5 (produce full-stack repos, D-A/D-B) per the plan.
Gate: each item ends with its own green carve/build proof on its PR.

## Completed work

- **Phase 3 import-boundary implementation (local):** workspace-wide `dependency-cruiser` rule, per-workspace tsconfig runner, two-violation negative proof, Node-20-compatible locked tool version, and required `fe-boundaries` CI job aggregated by `ci-complete`.

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

- **Import-boundary gate (2026-08-08):** clean official Node 20 container; workflow YAML parsed; `npm run test:boundaries` passed and proved exactly two `not-to-foreign-workspace` violations; `npm run lint:boundaries` passed all 11 tsconfig-aware scans with zero violations (53/85/77/2 web-surface modules, 28/18 mobile-surface modules, and 83/203/98/64/17 tier modules).
- **Review-fix gate (2026-08-08):** after merging `origin/main`, `dotnet build api/Concertable.slnx` succeeded with 0 errors. A clean `node:20-bookworm` container installed from `package-lock.json`; `npm run test:boundaries` passed and proved both violations; `npm run lint:boundaries` passed all 11 workspaces with zero violations. The host `npm ci` remained subject to the documented Windows antivirus partial-extraction failure and was not used as product evidence.
- **Current-main gate (2026-08-08):** after merging base `0514fe25b`, `dotnet build api/Concertable.slnx` succeeded with 0 errors. The merge changed no PR-owned frontend boundary or workflow path, so the existing clean Node 20 boundary proof remains applicable.
- **Second current-main gate (2026-08-09):** after merging base `cf4737b4f` as `b9425e5da`, `dotnet build api/Concertable.slnx --no-restore` succeeded with 0 errors (six existing nullable-context warnings). The merge imports only the platform-version sync and skill documentation; it changes no PR-owned frontend boundary or workflow path, so the existing clean Node 20 boundary proof remains applicable.
- **Third current-main gate (2026-08-09):** after merging base `9a54efd58` as `92e5be5df`, the initial parallel `dotnet build api/Concertable.slnx --no-restore` hit a transient Windows `CS0016` invalid output-handle failure in `Concertable.Payment.Api`; the single-threaded retry passed with 0 errors. The merge changes only techdebt command/plugin metadata, so the existing clean Node 20 boundary proof remains applicable.
- **Midnight integration blocker fix (2026-08-09):** CI run 31284847017 failed 11 `ContractApiTests` after the test host crossed UTC midnight: `SeedCatalog` retained the prior day's captured clock while `OpportunityRequestBuilders` recomputed `DateTime.UtcNow.AddMonths(1)`, colliding with seeded concert 45. Isolated fix PR #440 made every generated opportunity use the fixture seed clock. `scripts/integration.ps1 concert` passed B2B Concert 144/144 and Customer Concert 11/11; PR-head run 31287014734 and merge-group runs 31287569394/31287815716 passed. Publication/restore run 31288225192 and sync PR #442 completed green.
- **Post-blocker current-main gate (2026-08-09):** after merging `origin/main` `c72b058af` as `26d84f69d`, `dotnet build api/Concertable.slnx --no-restore --maxcpucount:1` passed with 0 errors. In a clean disposable `node:20-bookworm` container, `npm ci`, `npm run test:boundaries`, and `npm run lint:boundaries` passed; all 11 workspace scans reported zero violations (53/85/77/2 web surfaces, 28/18 mobile surfaces, and 85/203/98/64/17 tier modules).
- **Exact-head Venue diagnostic (2026-08-09):** after replacement run 31308852277 stalled with 45/46 jobs green and only the B2B Venue integration job still live, a short detached worktree at exact PR head `149f7a4db` ran `scripts/integration.ps1 venue`. Docker/Testcontainers started normally and all 25 Venue integration tests passed in 1.4 minutes with clean teardown. The diagnostic worktree was removed; the GitHub runner was left untouched for its own terminal classification.
- **Standing frontend gate (2026-08-08):** clean official Node 20 container; `npm ci` green; `npm run build:packages` built all five tiers; all four web builds green (3713 customer, 4404 venue, 4394 artist, 1757 business modules); `tsc --noEmit` green for `mobile/customer` and `mobile/b2b`; `git diff --check` green.

- **Mobile carve matrix implementation (2026-08-07):** `.github/workflows/test.yml` now lists all six
  surfaces, including `mobile/customer` and `mobile/b2b`; the existing classifier self-triggers
  `run_fe=true` when `test.yml` changes; `git diff --check` passes. The local shell has neither PyYAML
  nor Node on `PATH`, so the PR's own workflow parse plus its two new matrix jobs are the authoritative
  executable proof. On PR #416 run 31202906691, both `carve-fe (mobile/customer)` and
  `carve-fe (mobile/b2b)` completed successfully at remote head `f0fbd4e6a`.

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

- **Import-boundary full code/security review:** `reviews/Feature-platform_polyrepo_import-boundary.md`, range `9a18371a0..8a80bd3a` plus the cross-platform runner fix. Finding `NAT1` (MEDIUM correctness) identified direct `.cmd` spawning as Windows-incompatible; fixed by invoking dependency-cruiser's JavaScript entrypoint through `process.execPath` and verified in the clean Node 20 container. No other correctness, workflow-security, architecture-boundary, convention, or changed-behaviour coverage findings remain.
- **Import-boundary incremental review:** range `da3b75a7..f353a70b` (2 commits), covering only the review artifact and plan-ledger delivery checkpoints. No findings; watermark advanced to `f353a70b6841f03c6339a7b2590dd7126b480499`.
- **Import-boundary current-main incremental review:** range `f353a70b..0e4009ff`; branch-authored delta is plan/review checkpoints only and merge `0e4009ff8` imports already-landed main without changing the PR net boundary/workflow diff. No findings; watermark advanced to `0e4009ff81950c283396e07fe5f75ef31d409530`.
- **Import-boundary second current-main incremental review:** range `0e4009ff..b9425e5d`; branch-authored delta is plan/review checkpoints only and merge `b9425e5da` imports already-landed platform pins and skill docs without changing the PR net boundary/workflow diff. No findings; watermark advanced to `b9425e5da6d1f752804accc166dc34727ae084fe`.
- **Import-boundary third current-main incremental review:** range `b9425e5d..92e5be5d`; branch-authored delta is plan delivery checkpoints only and merge `92e5be5df` imports already-landed techdebt command/plugin metadata without changing the PR net boundary/workflow diff. No findings; watermark advanced to `92e5be5df58d0f2deaba600387fba1b8b1cfaaab`.
- **Import-boundary post-blocker current-main incremental review:** range `92e5be5d..26d84f69`; branch-authored delta is delivery-ledger checkpoints only and merge `26d84f69d` imports already-landed reviewed work without changing the PR net boundary/workflow diff. No findings; watermark advanced to `26d84f69dcb6d5615a1a4fe30c34dd22fc70d982`.

- **Mobile carve gate full code review:** `reviews/Feature-platform_polyrepo_mobile-carve.md`, range
  `59bdd7a8a..e64245e51` (18 commits), watermark `e64245e5192ccbccb30a5fd54d687ca05170c321`.
  No findings; the changed runtime path is CI-only, both new matrix keys are implemented by the existing
  harness, and PR #416 proved both feed-restored Expo exports. Backend lenses are N/A (no `api/**`).

- Review: Phase 1 post-merge review of the work delivered by PR #301 at `7c9a64a3e`. The exact review artifact, original finding identifiers, and narrower review range are not present in git, GitHub PR comments, or the preserved orphaned directory, so they are not fabricated here.
- Finding reference unavailable — fail-open PR-label lookup: fixed by `f57a4c504`.
- Finding reference unavailable — E2E eligibility and reviewed-branch policy inconsistencies: fixed by `0e3d8f5a6`; compatible changes retained through the `origin/main` reconciliation.
- No open finding is evidenced. Delivery of the two fixed findings remains gated on the new review-fix PR.

## Decisions, discoveries, blockers, and deviations

- **Architecture enforcement:** selected `dependency-cruiser` over a style linter or bespoke import parser because the rule is a resolved dependency/ownership invariant across TypeScript and JavaScript modules. `preserveSymlinks` keeps legitimate workspace package imports under `node_modules`; the single rule rejects direct paths between all 11 workspace roots. The runner supplies absolute per-workspace tsconfig paths so each surface's own alias map participates in resolution.
- **Tool version:** pinned `dependency-cruiser ^17.4.3`, the newest release compatible with CI's Node 20 (`^20.12||^22||>=24`). Current `18.1.1` requires Node 22 and cannot be used without a repository-wide runtime upgrade.

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

### 2026-08-09 — exact-head Venue runner hang disproved locally

- Action: Investigated an abnormally long final job in replacement run 31308852277 without cancelling or retrying it. Created a short detached worktree at exact head `149f7a4db`, ran the affected module through the integration-debug workflow, then removed the clean diagnostic worktree.
- Evidence: GitHub completed 45/46 jobs green and left only B2B Venue integration running; the local exact-head `scripts/integration.ps1 venue` run passed 25/25 in 1.4 minutes with healthy Testcontainers startup and teardown.
- Outcome: the outstanding GitHub job is a runner-specific hang, not a code regression. This evidence checkpoint creates an independently classified replacement head without changing product code or manually retrying the hung run.
- Follow-up: commit/push this checkpoint, require its complete replacement run, then perform the final currency check and enqueue full E2E.

### 2026-08-09 — post-blocker reviewed head pushed and verified

- Action: Published the reviewed branch after the blocker fix and 64-commit current-main reconciliation.
- Evidence: pushed `1e91e4ed6..15b74a2cf`; after fetch, local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equalled `15b74a2cf6e44f886cfc55e8823c81329fd9eaf8`.
- Outcome: the reviewed implementation is published; this transport checkpoint is the sole local tail.
- Follow-up: commit and push the checkpoint, verify equality, then require a complete replacement gate before the final currency check and full-E2E queue admission.

### 2026-08-09 — post-blocker current main merged, gated, and reviewed

- Action: Merged the 64-commit base advance accumulated while the integration blocker was fixed, then rebuilt and re-proved the boundary gate against the merged frontend lockfile/shared-tier changes.
- Evidence: merge `26d84f69dcb6d5615a1a4fe30c34dd22fc70d982` contains base `c72b058afe43742854b765838bf43f179e7ed92a`; `origin/main...HEAD` = 0 behind / 25 ahead. Full-solution build passed with 0 errors. Clean Node 20 `npm ci` + negative proof + all 11 clean scans passed. The PR net diff remains `.github/workflows/test.yml`, dependency-cruiser config/runner/test, `app/package*.json`, and plan/review artifacts.
- Outcome: the branch is current, builds, boundary-proves, and is review-clean. Its replacement head is local only.
- Follow-up: publish through the compound protocol, require replacement checks, then perform the final currency check and enqueue full E2E.

### 2026-08-09 — midnight integration blocker fixed, published, and synced

- Action: Diagnosed PR #428 replacement run 31284847017 instead of retrying it. Eleven B2B `ContractApiTests` all failed just after UTC midnight with `400 "You already have a concert on this day"`; the immediately preceding same-code head had passed before midnight. Created isolated short-path branch/worktree `Fix/IntegrationMidnightClock`, made generated opportunity dates share the fixture's captured seed clock, verified and reviewed it, then landed the dependency through its own PR and platform-sync lifecycle.
- Evidence: root cause was the host-captured `SeedCatalog.Now` from August 8 plus seeded concert 45 at +32 days versus request-builder `DateTime.UtcNow.AddMonths(1)` after the clock rolled to August 9—both resolved to September 9. Targeted integration verification passed B2B Concert 144/144 and Customer Concert 11/11. PR #440 merged as `2eb8bc4764ee1303dc77ced9149b1e7a5f093583`; package publish/restore run 31288225192 succeeded; platform-sync PR #442 merged as `ab5bea7aff3153bc5095d07c6b918a8aeeae286a`. The clean fix worktree/branch was removed.
- Outcome: the unrelated main-line test defect is fixed and fully synced; PR #428 can resume. Main advanced to `c72b058af` during the dependency lifecycle.
- Follow-up: merge current main into #428, rebuild/review, republish, and require a fresh exact-head gate before queue admission.

### 2026-08-09 — third current-main reviewed head pushed and verified

- Action: Published the reviewed branch after the third current-main merge/build/review checkpoint.
- Evidence: pushed `270676f25..c928d34c4`; after fetch, local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equalled `c928d34c4f0c40198ddbb45a2328792276f0d45a`.
- Outcome: the reviewed implementation is published; this transport checkpoint is the sole local tail.
- Follow-up: commit and push the checkpoint, verify equality, then require a complete replacement gate before the final currency check and full-E2E queue admission.

### 2026-08-09 — third current-main merge built and reviewed

- Action: Merged the docs/meta base that advanced during replacement CI, rebuilt the full solution, and incrementally reviewed the resulting range.
- Evidence: merge `92e5be5df58d0f2deaba600387fba1b8b1cfaaab` contains base `9a54efd58636ee1a97aa27a86b166d575d07c327`; `origin/main...HEAD` = 0 behind / 21 ahead. The parallel build hit transient Windows `CS0016` output-handle failure; `dotnet build api/Concertable.slnx --no-restore --maxcpucount:1` then succeeded with 0 errors. Incremental review `b9425e5d..92e5be5d` has no findings.
- Outcome: the branch is current, builds, and is review-clean. Its replacement head is local only.
- Follow-up: publish through the compound protocol, require replacement checks, then recheck currency and enqueue full E2E.

### 2026-08-09 — replacement green; third base drift blocks queueing

- Action: Followed replacement run 31283793063 to terminal green, then performed the mandatory immediate fetch and base-currency check.
- Evidence: at PR head `270676f2583eb95a67c076cb6219521788117079`, `fe-boundaries`, all six `carve-fe` jobs, build, all unit/integration jobs, `instant-merge`, and `ci-complete` passed. `origin/main...HEAD` then reported 6 behind / 19 ahead; new base `9a54efd58` contains only techdebt command/plugin documentation and metadata changes from PRs #436/#437.
- Outcome: the complete PR gate is green, but the head cannot enter the merge queue while stale.
- Follow-up: merge current main, rebuild/review, and publish another exact replacement head.

### 2026-08-09 — second current-main reviewed head pushed and verified

- Action: Published the reviewed branch after the second current-main merge/build/review checkpoint.
- Evidence: pushed `8a8450014..c876796f3`; after fetch, local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equalled `c876796f3f9ecda0531bdf996c1ae7209aa2e626`.
- Outcome: the reviewed implementation is published; this transport checkpoint is the sole local tail.
- Follow-up: commit and push the checkpoint, verify equality, then require a complete replacement gate before the final currency check and full-E2E queue admission.

### 2026-08-09 — second current-main merge built and reviewed

- Action: Merged the base that advanced during replacement CI, rebuilt the full solution, and incrementally reviewed the resulting range.
- Evidence: merge `b9425e5da6d1f752804accc166dc34727ae084fe` contains base `cf4737b4fa438b34394491fb07951675f2417d1f`; `origin/main...HEAD` = 0 behind / 17 ahead; `dotnet build api/Concertable.slnx --no-restore` succeeded with 0 errors. Incremental review `0e4009ff..b9425e5d` has no findings; the imported main changes are platform pins and skill docs, with no PR-owned boundary/workflow path changed.
- Outcome: the branch is current, builds, and is review-clean. Its replacement head is local only.
- Follow-up: commit and push through the compound protocol, require replacement checks, then recheck currency and enqueue full E2E.

### 2026-08-08 — replacement green; second base drift blocks queueing

- Action: Followed the delayed replacement run to terminal green, then immediately refreshed and checked base currency.
- Evidence: run 31281728324 at PR head `8a84500141ee4a54d3e5e6692d5ccb60701248ed` passed `fe-boundaries`, all six `carve-fe` jobs, build, all unit/integration jobs, and `ci-complete`. `origin/main...8a8450014` then reported 6 behind / 15 ahead; new base `cf4737b4f` includes platform-sync #434 and docs/skill PR #435.
- Outcome: the PR head is fully green but cannot be admitted while stale under the repository currency gate.
- Follow-up: checkpoint the second drift, merge current main, rebuild/review, and publish the replacement head.

### 2026-08-08 — current-main reviewed head pushed and verified

- Action: Pushed the complete branch after merging current main, rebuilding, and incrementally reviewing it.
- Evidence: pushed `b483f8ac4..2024e110f`; local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equal `2024e110f9fdcb568c800578daea34e73a8d1d73` after fetch.
- Outcome: the current reviewed implementation is published and replacement CI is running. This checkpoint is the sole local tail.
- Follow-up: transport this checkpoint, verify equality, wait for replacement checks, then recheck base currency and enqueue.

### 2026-08-08 — current main merged, built, and incrementally reviewed

- Action: Merged `origin/main` after the final currency gate blocked queue admission, rebuilt the full solution, and incrementally reviewed the resulting range.
- Evidence: merge `0e4009ff81950c283396e07fe5f75ef31d409530` contains base `0514fe25b`; `origin/main...HEAD` = 0 behind / 13 ahead; `dotnet build api/Concertable.slnx` succeeded with 0 errors. The merge output contains no PR-owned boundary/workflow path. Incremental review `f353a70b..0e4009ff` has no findings.
- Outcome: the branch is current, builds, and is review-clean. Its replacement head is local only.
- Follow-up: commit and push through the compound protocol, require replacement checks, then recheck currency and enqueue full E2E.

### 2026-08-08 — queue admission blocked by new base drift

- Action: Performed the mandatory final base-currency check after the final PR-head gate passed, then inspected the intervening commits and net paths.
- Evidence: `origin/main` advanced to `0514fe25b`; `origin/main...b483f8ac4` reports 16 behind / 10 ahead. The new base includes platform-sync #433, runtime PR #431, and review tooling/docs changes; PR #428 remains `OPEN/CLEAN` at `b483f8ac4`, with no queue entry and no prior `pr-428-*` merge-group run.
- Outcome: the green PR head cannot be queued while stale. The local post-PR tail still changes only this ledger, so the branch is safe to update through the compound protocol.
- Follow-up: checkpoint the stale-base finding, merge current main, rebuild and verify, incrementally review, then push the replacement head.

### 2026-08-08 — final PR head gate terminal green

- Action: Reconciled the replacement run triggered by the transported review checkpoint through every terminal job.
- Evidence: PR #428 head `b483f8ac4394959f459fcb1f6cd29a2b596fc953`; run 31280806389 passed `fe-boundaries`, all six `carve-fe` surfaces, build, all unit/integration jobs, and `ci-complete`. PR-level API/UI E2E jobs skipped as designed; labels remain empty, so full E2E is selected for the merge group.
- Outcome: the exact remote head is fully green and ready for queue admission. This observation checkpoint is local-only.
- Follow-up: verify the checkpoint-only tail and current queue/merge-group state, enqueue the exact remote head, and monitor it to a terminal outcome.

### 2026-08-08 — incremental-review head pushed and verified

- Action: Pushed the review/check checkpoint and reconciled the delivery refs.
- Evidence: pushed `f353a70b6..7f49fbcdd`; local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equal `7f49fbcddd61cd312969abac5e2f14cf28a2378a` after fetch.
- Outcome: the PR now contains the complete review record; only inert replacement checks remain before queue admission. This checkpoint is the sole local tail.
- Follow-up: transport this checkpoint, verify equality, reconcile inert checks, then enqueue full E2E.

### 2026-08-08 — replacement PR gate green; meta tail incrementally reviewed

- Action: Followed the complete replacement check set to terminal green at the transported PR head, then incrementally reviewed the two commits after the code-review watermark.
- Evidence: PR run 31280106976 at `f353a70b6`: `fe-boundaries`, six `carve-fe` jobs, build, all unit/integration jobs, and `ci-complete` passed; PR-level E2E skipped as designed. Incremental review `da3b75a7..f353a70b` contains only `reviews/Feature-platform_polyrepo_import-boundary.md` and `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; no findings.
- Outcome: the exact remote source head is fully green and the full branch history is reviewed. The local incremental-review checkpoint is the only unpublished tail.
- Follow-up: commit and push the review checkpoint, verify equality and inert replacement checks, then enqueue full E2E.

### 2026-08-08 — reviewed import-boundary work head pushed and verified

- Action: Pushed the complete updated branch from the prior PR head and reconciled all delivery refs.
- Evidence: pushed `e4d3bc97f..96f88e8a1`; local `HEAD`, `origin/Feature/platform_polyrepo_import-boundary`, and PR #428 `headRefOid` all equal `96f88e8a1c2bfdda279dbcd70f6a24744bab2258` after fetch.
- Outcome: the reviewed and locally verified implementation is published; replacement CI is running. This checkpoint is the sole local tail.
- Follow-up: transport this checkpoint, verify equality again, then wait for the full replacement PR gate before queue admission.

### 2026-08-08 — import-boundary code/security review finalized

- Action: Completed the full review after committing the cross-platform runner fix and reconciled the workflow-security lens because `.github/workflows/test.yml` is in scope.
- Evidence: `reviews/Feature-platform_polyrepo_import-boundary.md`; reviewed and security-reviewed through `da3b75a77d771e94bac76df65b1ed6eb135c3772`; range `9a18371a..da3b75a7` (6 commits). `NAT1` is fixed; no other findings remain.
- Outcome: the current code head is review-clean and ready for the compound push protocol.
- Follow-up: push and verify the reviewed branch head, transport the push checkpoint, then reconcile replacement PR checks before queueing.

### 2026-08-08 — import-boundary branch updated; review finding fixed and re-proved

- Action: Merged current `origin/main`, ran the mandatory full solution build, then reviewed the complete frontend boundary and CI diff. Reproduced a Windows-only runner launch failure and changed the runner to execute dependency-cruiser's JavaScript entrypoint through Node.
- Evidence: merge commit `8a80bd3ad` brings base `9a18371a0`; `dotnet build api/Concertable.slnx` succeeded with 0 errors. Review finding `NAT1` in `reviews/Feature-platform_polyrepo_import-boundary.md`; direct `.cmd` spawn returned `EINVAL`. Clean `node:20-bookworm` verification passed `npm run test:boundaries` and all 11 `npm run lint:boundaries` scans with zero violations.
- Outcome: the updated branch is current, builds, and the boundary gate is cross-platform. The review finding is fixed locally; replacement PR checks have not run yet.
- Follow-up: commit the fix and checkpoint, finalize the review watermark, push through the compound protocol, then require the full replacement PR gate before merge-queue admission.

### 2026-08-08 — import-boundary PR #428 opened

- Action: Re-ran the live preflight, confirmed local/remote equality and zero base drift, and opened the plain GitHub PR without setting an E2E label.
- Evidence: PR [#428](https://github.com/Concertable/concertable/pull/428), base `main`, head `Feature/platform_polyrepo_import-boundary` at `e4d3bc97fe932cf6d7cf81db744a908f35efe754`, `OPEN`, not draft, labels `[]`; no open platform-sync PR; initial `instant-merge` and `enable` checks successful, `changes` in progress.
- Outcome: CI now owns the authoritative boundary and six-surface carve proof; PR creation did not mutate the verified source head.
- Follow-up: run the merge workflow for #428; it owns check reconciliation, E2E-tier selection, queue admission, merge confirmation, and closeout transfer.

### 2026-08-08 — import-boundary work head pushed and verified

- Action: Refreshed the base before delivery, merged the 17 intervening docs-only commits with zero path overlap, and pushed the exact current work head to the new remote feature branch.
- Evidence: implementation commit `69e686841`; currency merge/work head `087c5969144c56a83d627f2bb3aaf655d2d5f9a4`; `HEAD..origin/main` = 0; starting remote branch absent; fetched `origin/Feature/platform_polyrepo_import-boundary` = `087c59691`; no open PR.
- Outcome: the verified implementation is published on a current branch; this ledger checkpoint is the sole local tail.
- Follow-up: transport this checkpoint with full local/remote equality, then open the plain GitHub PR and follow its boundary and six-carve gates.

### 2026-08-08 — Phase 3 import-boundary implementation locally complete

- Action: Established dependency-cruiser as the frontend architecture gate, covered all six surfaces and five published tiers with their own tsconfigs, added a two-violation negative proof, and wired a required `fe-boundaries` CI job beside the existing six-surface carve matrix.
- Evidence: Node 20 clean-container boundary proof and 11-workspace scan green; workflow YAML parsed; all five tier builds, four web builds, and two mobile typechecks green; lockfile resolves `dependency-cruiser 17.4.3` with a Node-20-compatible engine.
- Outcome: the durable FE import-boundary implementation is complete and locally verified; the PR remains the authoritative six-carve proof.
- Follow-up: commit, push, open the PR, follow `fe-boundaries` and all six carve jobs to green, then review and merge through full E2E.

### 2026-08-08 — import-boundary worktree activated and base reconciled

- Action: Resumed the Phase 3 import-boundary handoff in its dedicated worktree, refreshed remote state, and reconciled the stale closeout identity before implementation.
- Evidence: branch `Feature/platform_polyrepo_import-boundary`; clean starting tree; no branch-only commits; fast-forward `fb7255b20..372be1041`; `origin/main...HEAD` = `0 0`; no open `chore/platform-sync-*` PR.
- Outcome: the branch directly matches the requested plan work, is current with the base, and has no platform-sync blocker.
- Follow-up: establish the repository-wide FE import-boundary enforcement layer and prove it against all six surfaces and the existing carve/build gates.

### 2026-08-07 — merged #416 feature worktree and branches removed

- Action: Verified the source checkout was clean and its post-PR range changed only the transferred ledger, removed the exact feature worktree, and deleted its local branch.
- Evidence: source HEAD `288ad3335`; `74b9743d8..288ad3335` named only `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`. Remote deletion reported the ref already absent; `git fetch origin --prune` removed the stale tracking ref.
- Outcome: #416 is fully closed out with recovery anchored solely in this docs worktree; its CI/docs-only merge has no FE publication or backend platform-sync consequence.
- Follow-up: end this completed sub-project at its handoff; next invocation starts the Phase 3 FE import-boundary sub-project on a fresh feature worktree.

### 2026-08-07 — #416 recovery transferred to the closeout worktree

- Action: Created this clean docs closeout branch from current `origin/main` at merge `83a3f49a1` and cherry-picked the three post-PR observation commits in order.
- Evidence: source range `74b9743d8..288ad3335` changes only `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; source commit `288ad3335` and transferred head `12565ddec` resolve that ledger to the identical blob `53f7f59201765c6965491fe108331e18fd71f4b2` before this identity update.
- Outcome: the plan recovery state now lives independently of the merged feature worktree.
- Follow-up: remove the clean merged feature worktree and branch, checkpoint the cleanup here, then hand off the ESLint import-boundary sub-project.

### 2026-08-07 — #416 merged through the full-E2E merge queue

- Action: Followed the exact merge-group run through its terminal result without retrying or weakening the gate.
- Evidence: run [31204805838](https://github.com/Concertable/concertable/actions/runs/31204805838), branch `gh-readonly-queue/main/pr-416-b46d10ec873cada34e62083d8c9cedbda080160b`, completed `success`; PR #416 then reported `MERGED` with merge commit `83a3f49a194c5faff60b7912d7c9b8452679f6f0`.
- Outcome: the definitive customer and B2B mobile carve gates are on `main`; no publication or platform-sync gate follows this CI/docs-only merge.
- Follow-up: transfer the ledger-only observation tail to a docs closeout worktree, remove the merged feature worktree/branch, and hand off the separate ESLint import-boundary sub-project.

### 2026-08-07 — #416 admitted to the merge queue at position 1

- Action: Applied the sanctioned one-time auto-merge disable/re-enable using the repository-selected queue method, then queried GraphQL directly.
- Evidence: PR remains at reviewed remote head `74b9743d8`; `mergeQueueEntry.state=QUEUED`, `position=1`; `autoMergeRequest=null` after queue consumption.
- Outcome: #416 is admitted to the full-E2E merge queue with no source-head mutation.
- Follow-up: monitor the exact merge-group formation to merge or terminal failure without retrying a failed run.

### 2026-08-07 — #416 terminal green but never admitted to the merge queue

- Action: Waited for every PR-head check to become terminal, then reconciled PR state, queue entry, labels, and merge-group history for the exact reviewed remote head.
- Evidence: PR #416 `OPEN/CLEAN`, head `74b9743d8`, all PR checks green; branch 0 behind `origin/main`; no `skip-e2e`/`skip-e2e-ui` labels or trailers; auto-merge enabled at `2026-08-07T17:34:20Z`; GraphQL `mergeQueueEntry=null`; no recent merge-group branch matching `/pr-416-`.
- Outcome: sustained green-but-unadmitted GitHub re-evaluation glitch confirmed. Full queue E2E remains required; no CI failure exists.
- Follow-up: apply the sanctioned one-time disable/re-enable and verify actual queue admission before waiting.

### 2026-08-07 — clean review head pushed and verified

- Action: Pushed the committed review artifact and its plan review record to PR #416.
- Evidence: local HEAD, `origin/Feature/platform_polyrepo_mobile-carve`, and PR #416 `headRefOid` all equal `6433eae7784ab0d8595b041c35252a47d72dfd88` after fetch.
- Outcome: the remote PR contains the clean review artifact; this ledger checkpoint is the sole local tail.
- Follow-up: transport this checkpoint, verify equality, then execute the merge workflow.

### 2026-08-07 — PR #416 full code review completed with no findings

- Action: Reviewed the complete branch diff through correctness, service/module boundaries, seeding, C# convention, and changed-behaviour coverage lenses.
- Evidence: `reviews/Feature-platform_polyrepo_mobile-carve.md`; range `59bdd7a8a..e64245e51` (18 commits); stamped watermark `e64245e5192ccbccb30a5fd54d687ca05170c321`; no findings. The CI matrix keys map to existing harness entries, `run_fe` self-triggers, `ci-complete` requires `carve-fe`, and both new jobs are green.
- Outcome: PR #416 is review-clean and ready for the merge workflow after the review checkpoint is pushed.
- Follow-up: commit and push the review checkpoint with verified PR-head equality, then merge #416 through the queue.

### 2026-08-07 — checkpoint transport verified; replacement mobile carves green

- Action: Transported the resolved-debt push checkpoint, verified all branch/PR refs, and monitored the two replacement mobile matrix jobs on the resulting PR head.
- Evidence: local HEAD, remote branch, and PR #416 `headRefOid` all `b1107da10369b28137b179e17040a80678557e93`; run 31203455635 completed `carve-fe (mobile/customer)` and `carve-fe (mobile/b2b)` successfully.
- Outcome: the final remote PR content is green for both definitive mobile Expo exports and ready for code review.
- Follow-up: finalize the branch review, checkpoint it, and hand the reviewed PR to the merge workflow.

### 2026-08-07 — resolved-debt work head pushed and verified

- Action: Pushed the compound branch head containing the PR observation checkpoints plus the resolved debt deletion.
- Evidence: local HEAD, `origin/Feature/platform_polyrepo_mobile-carve`, and PR #416 `headRefOid` all equal `7223c1c6a7fa2df2aeb7510e04f147446e2322fd` after fetch.
- Outcome: PR #416 now contains the mobile carve gate and the mechanically justified removal of its resolved tech-debt entry; this ledger checkpoint is the sole local tail.
- Follow-up: transport this checkpoint, verify equality again, then follow replacement mobile checks and code-review the final head.

### 2026-08-07 — resolved mobile bundling tech debt removed

- Action: Deleted the sole entry in `app/mobile/TECH_DEBT.md` after its exact resolution gate passed; because no other entries remain, the whole area debt file is removed.
- Evidence: PR #416 run 31202906691 has both mobile carve jobs `completed/success`, satisfying the entry's `Resolves when` condition verbatim.
- Outcome: no resolved mobile-bundling debt remains in the working tree; Phase 3's only outstanding implementation item after #416 is the separate ESLint import-boundary sub-project.
- Follow-up: commit and push this closeout through the compound plan push protocol, confirm replacement carves, then code-review #416.

### 2026-08-07 — both definitive mobile carve jobs passed on PR #416

- Action: Monitored the two exact mobile matrix jobs on Actions run 31202906691 at recorded PR head `f0fbd4e6a`.
- Evidence: `carve-fe (mobile/customer)` job 92946961325 = `completed/success`; `carve-fe (mobile/b2b)` job 92946961363 = `completed/success`. The containing run remained in progress only because other jobs were still executing, so job logs were not yet available for extraction.
- Outcome: both mobile surfaces have restored their dependencies from the feed, type-checked, and completed `expo export`; the mobile bundling tech debt's mechanical resolution gate is met.
- Follow-up: delete the resolved debt entry, push the compound docs/checkpoint update, confirm replacement checks, and code-review the final PR head.

### 2026-08-07 — mobile carve gate PR #416 opened

- Action: Transported the verified push checkpoint, confirmed local/remote equality, and opened the plain GitHub PR from `Feature/platform_polyrepo_mobile-carve` to `main`.
- Evidence: local HEAD and `origin/Feature/platform_polyrepo_mobile-carve` both `f0fbd4e6a8ed1629075113c4a21f0694a028a0dc`; PR [#416](https://github.com/Concertable/concertable/pull/416) is `OPEN`, base `main`, head `f0fbd4e6a`, title `ci(mobile): gate carved apps with Expo exports`.
- Outcome: CI now owns the definitive feed-restored Expo export proof for both mobile surfaces. No E2E label was set at creation.
- Follow-up: follow both new mobile matrix jobs to green; diagnose and fix any real bundle failure without weakening the gate.

### 2026-08-07 — mobile carve work head pushed and verified

- Action: Pushed the actual implementation work head before transporting any later plan checkpoint.
- Evidence: pushed `098d4c3a5` to new remote branch `Feature/platform_polyrepo_mobile-carve`; after fetch, `origin/Feature/platform_polyrepo_mobile-carve` equals full work SHA `098d4c3a57fc2ad67185b646befe2892b0096988`.
- Outcome: the committed mobile matrix change is published on the remote branch; this ledger checkpoint is the sole remaining local tail.
- Follow-up: transport this checkpoint, verify local/remote equality, then create the PR.

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
