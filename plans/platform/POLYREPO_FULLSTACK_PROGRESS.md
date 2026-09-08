# Full-stack polyrepo — frontend build separation progress

- Plan: `plans/platform/POLYREPO_FULLSTACK_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-fullstack`
- Also delivers: `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` **checkpoint 8B** — the `concertable`-side
  consumer work for the frontend platform publisher cutover.
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-FrontendRegistryPackageConsumers`
- Branch: `Refactor/FrontendRegistryPackageConsumers`
- PR: none yet. Phase 3 import-boundary PR [#428](https://github.com/Concertable/concertable/pull/428) merged as `162b8412a`; mobile-carve PR [#416](https://github.com/Concertable/concertable/pull/416) merged as `83a3f49a1`; publish-first mobile-retarget PR [#413](https://github.com/Concertable/concertable/pull/413) merged as `62646f4cd`; carved-web CSS [#405] (`d9c62e2c5`); Phase 3b [#389] (`1cbeb2175`); Phase 3a [#378] (`fba490e25`); Phase 2 [#360] (`a3f9535`); Phase 1 [#301]+[#319].
- Dependency/package gates: **all seven tiers are published and current with `main`.** The `alpha` dist-tag
  is `0.1.0-alpha.0.6314` for `@concertable/{shared,web,mobile,customer,b2b,web-b2b,build-config}`; every
  tier's last source commit is at height ≤ 6300, so the published set matches `main` exactly.
  **8B's third deliverable is blocked on 8A:** `platform-frontend` does not exist, so the monorepo must keep
  publishing the four platform IDs. No `api/**` → no backend platform-sync.
- Last reconciled: 2026-09-08 — measured 8B's three deliverables against `origin/main` `ef8d505fd` before
  writing anything, and reconciled this ledger from its stale Phase-3 state.

## Current state

Phases 0–3 are **terminal on `main`**. The frontend carve gate is the seven-surface `carve-fe` matrix in
`.github/workflows/test.yml` (`web/{customer,admin}`, `web/b2b/{venue,artist,business}`,
`mobile/{customer,b2b}`), each archiving its surface alone, restoring every `@concertable` tier purely from
`npm.pkg.github.com` and building standalone; `fe-boundaries` separately enforces that no workspace reaches
another except through its published package. Both are `run_fe`-gated and required by `ci-complete`. That is
already checkpoint 8's hard stop — *product builds succeed with the platform source directories absent* — so
it is not rebuilt here.

**Checkpoint 8B, measured:** of its three deliverables, the hard stop and the feed publication are done, the
per-repo-ready lockfiles were absent entirely, and removing monorepo publication is correctly 8A-gated.
The registry switch exists only as a carve-time transform: every workspace still declares
`@concertable/*: "*"`, and `carve-fe.mjs` rewrites `"*"` → `"alpha"` inside the temp tree. So the
*capability* is proven each CI run, but a `git archive` of a surface is not itself installable.

This branch lands the safe half: a committed standalone `package-lock.json` per surface, restored with
`npm ci` in the carve, and retirement of the `@concertable/build-config` local-pack special case.

**Terminal Phase-3 facts still worth carrying.** Only `@concertable/{web,b2b}` carry web class strings, so
`app/web/shared/src/index.css` scans tier dists through two `@source` globs (`../dist/**/*.js`,
`../../b2b/dist/**/*.js`) alongside the sibling-`src` globs — each set is inert in the layout it does not
belong to and Tailwind ignores an `@source` matching nothing, so both coexist (#405, `d9c62e2c5`).
`@concertable/mobile` is the only className-bearing mobile tier and owns its own brand assets at
`app/mobile/shared/assets/`; the surface `app.json` icon/splash assets stay at `app/mobile/assets/` (#413,
`62646f4cd`). `dependency-cruiser` runs through its JavaScript entrypoint under `process.execPath` because
spawning the npm `.cmd` shim fails with `EINVAL` on Windows (#428, `162b8412a`).

## Next Steps

**1. Land this branch's 8B safe half.** Commit the per-surface lockfiles, the `npm ci` carve restore and the
retired `build-config` special case; review against `reviews/Refactor-FrontendRegistryPackageConsumers.md`;
open the PR and take `carve-fe` (all seven surfaces) plus `fe-boundaries` to green. `carve-fe` is the
decisive gate — it is the only thing that proves `npm ci` accepts a Windows-generated lockfile on the Linux
runner. `run_fe` is true for this diff (it touches `app/**` and `app/scripts/carve-fe.mjs`).

**2. Then 8B's remaining two deliverables, both gated on 8A.** They cannot land while `platform-frontend`
does not exist, because either one breaks every carve restore the moment it merges:

- *Declare the registry switch.* Replace `@concertable/{shared,web,mobile,build-config}: "*"` in each
  surface manifest with the pinned feed version, so the manifest itself is registry-resolvable instead of
  relying on the carve rewrite. This stops surfaces consuming local tier source in-monorepo, so it needs a
  local-source escape hatch (the npm counterpart of the backend's `UseLocalCore`) and version-bump
  propagation — Phase 4's `chore/fe-platform-sync-*` bump PR — landing with it, not after.
- *Remove monorepo publication of the four platform IDs.* Drop `@concertable/build-config`,
  `@concertable/shared`, `@concertable/web` and `@concertable/mobile` from
  `.github/workflows/publish-fe-packages.yml` (names array, feed-verify step, and the `paths:` triggers) and
  from `app/scripts/version-fe-packages.mjs`'s lockstep set, leaving only the three product tiers
  `@concertable/{customer,b2b,web-b2b}`. `platform-frontend` must be publishing those four IDs first, or
  the lockstep version diverges and every surface lock goes unresolvable.

Blocked: checkpoint 8A has not run — `Concertable/platform-frontend` does not exist and publishes nothing.
Blocked by: `REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` checkpoint 8A (repository creation is Tommy's,
and is explicitly outside this ledger's authorization).
Unblock action: Tommy creates `Concertable/platform-frontend`; 8A lands the general web/mobile shared
history plus Changesets publication there and publishes initial versions of the four platform IDs.
Resume when: `npm view @concertable/shared --registry=https://npm.pkg.github.com` resolves a version whose
publisher is `platform-frontend`, for all four platform IDs.

## Completed work

- **Checkpoint 8B, safe half (this branch):** each of the seven surfaces carries a committed standalone
  `app/<surface>/package-lock.json` pinning `0.1.0-alpha.0.6314` for every `@concertable` tier it declares,
  resolved from `npm.pkg.github.com`; `carve-fe.mjs` restores with `npm ci` instead of a fresh `npm install`,
  gains `--write-lock` (fronted by `npm run lock:carve`) to regenerate them, and no longer packs
  `@concertable/build-config` locally for mobile — it comes off the feed like every other tier. A new offline
  test asserts every surface has a lock, declares its tiers as `alpha`, resolves them from the feed, and sits
  on one lockstep version.

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

- **Checkpoint 8B measurement (2026-09-08), taken before any edit.** Against `origin/main` `ef8d505fd`
  (commit height 6432): all 14 workspaces declare `@concertable/*: "*"`, so the registry switch exists only
  as `carve-fe.mjs`'s carve-time rewrite. Zero per-surface lockfiles existed and the carve ran
  `npm install`. `publish-fe-packages.yml` publishes all seven IDs. A direct registry probe of
  `https://npm.pkg.github.com/@concertable%2f<tier>` returned HTTP 200 for all seven with
  `dist-tags.alpha = 0.1.0-alpha.0.6314` (`shared` 48 versions, `web`/`mobile`/`customer`/`b2b` 42,
  `web-b2b` 25, `build-config` 1); every tier's last source commit is at height ≤ 6300, so the published set
  matches HEAD. Phase 3's import-boundary PR #428 is merged (`162b8412a`, 2026-08-09), and `test.yml`
  carries the seven-surface `carve-fe` matrix plus `fe-boundaries`, both `run_fe`-gated and required by
  `ci-complete`.

- **Nested-lockfile inertness (2026-09-08).** A throwaway two-package npm workspace fixture with a
  deliberately corrupted nested `a/package-lock.json`: root `npm install --package-lock-only` built the root
  lock without reading or rewriting the nested file, and root `npm ci` installed the root-locked
  `is-odd@3.0.1` rather than the nested lock's bogus pin — exit 0, no warning either time.

- **Standalone lockfile generation (2026-09-08).** `--write-lock` produced all seven locks against the live
  feed (642/641/642/618/600 packages for `web/{customer,admin}` and `web/b2b/{venue,artist,business}`;
  878/877 for `mobile/{customer,b2b}`). Every `@concertable` entry in all seven resolves from
  `npm.pkg.github.com` at `0.1.0-alpha.0.6314`, including `@concertable/build-config` in both mobile locks.

- **Carve harness tests (2026-09-08).** `node --test scripts/carve-fe.test.mjs` passed 8/8, covering the five
  web `--prepare-only` carves, both mobile carves now asserting every `@concertable` specifier — including
  `build-config` — is rewritten to `alpha` with no `app/build-config` in the carved tree, and the new
  lockstep-pin invariant.

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

- **Checkpoint 8B safe-half full review:** `reviews/Refactor-FrontendRegistryPackageConsumers.md`, range
  `3052fb9ac..73e36b581` (12 paths). One LOW finding, `FRP1`: the candidate makes a committed per-surface
  lockfile a required input to the `carve-fe` gate without adding `npm run lock:carve` to `app/README.md`'s
  command block, so a developer who adds a dependency to a surface gets a red carve job with no pointer to
  the fix. Fixed on this branch. The pass records eight verified-clean checks, of which two were load
  bearing: `app/package-lock.json` cannot desync because its `packages[""]` does not record `scripts`, and
  the Windows-generated locks carry every platform variant of each native dependency, so the Linux runner's
  `npm ci` resolves its own binaries. No changed path qualifies for a security marker.

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

- **`@concertable/build-config` is on the feed — the carve's local-pack special case is retired, do not
  re-add it.** `carve-fe.mjs` used to `npm pack` `app/build-config` into the mobile carve as a `file:`
  devDependency and then delete the source, on the stated premise that "the new producer is not on the feed
  until its separately gated publisher cutover". That premise ended at `f2f5d01b0` (on `main` 2026-09-07;
  `publish-fe-packages.yml` already carries the ID): the feed serves
  `@concertable/build-config@0.1.0-alpha.0.6314` under `alpha`. Mobile carves now take it from the feed
  through the same `"*"` → `"alpha"` rewrite as every other tier, and `app/build-config` is no longer
  archived into the carved tree at all.

- **The standalone lockfiles keep their real name at `app/<surface>/package-lock.json`, because a nested
  lockfile inside an npm workspace is inert.** Verified with a throwaway workspace fixture: root `npm ci`
  installed the root-locked version and ignored a deliberately bogus nested lock — exit 0, no warning — and
  root `npm install --package-lock-only` neither read nor rewrote it. So one file is authoritative once the
  surface stands alone and invisible while it is a workspace member, and no carve-time rename is needed.
  `app/package-lock.json` stays the only lockfile the monorepo resolves.

- **The carve restores with `npm ci`, not `npm install`.** The committed lock is what a standalone surface
  repo restores, so the gate has to fail on a lock that no longer satisfies its surface rather than silently
  re-resolving around it. Consequence to accept deliberately: a PR that changes a tier's source is carved
  against the last *published* tier, exactly as it was under the floating tag — publication happens on merge
  to `main`, so the carve has never seen an unpublished tier.

- **`--write-lock` deletes the archived lock before regenerating.** npm keeps an already-locked version for a
  dist-tag spec instead of re-resolving it, so an in-place refresh would never pick up a newer lockstep
  publish and `npm run lock:carve` would be a silent no-op.

- **Which IDs checkpoint 8A takes.** Per the migration plan's extraction map, `platform-frontend` owns the
  four platform IDs `@concertable/{shared,web,mobile,build-config}`; `@concertable/{customer,b2b,web-b2b}`
  are product tiers that stay in `concertable`. Those four are exactly what 8B's third deliverable removes
  from `publish-fe-packages.yml` and from `version-fe-packages.mjs`'s lockstep set — and the reason it
  cannot land before 8A publishes.

- **Overlap with in-flight PR [#950](https://github.com/Concertable/concertable/pull/950).** That PR also
  edits `app/scripts/carve-fe.mjs` and `app/scripts/carve-fe.test.mjs`, adding a `--package-version=<exact>`
  flag that pins the rewrite to one publication for a terminal consumer proof. Whichever branch merges second
  resolves a textual conflict in the header usage line, the flag-parsing block and the mobile test, plus one
  functional interaction: an exact `--package-version` cannot be restored with `npm ci`, because the
  committed lock is generated against `alpha`. Reconciliation, decided here so it is not left open —
  `--package-version` implies the `npm install` path and only the default tag rewrite uses `npm ci`.

- **A carved surface does not inherit the root `app/package.json` `overrides` block**, so its resolution can
  differ from the monorepo's and the committed locks record the un-overridden graph. Pre-existing — the carve
  has always installed without them — and deliberately left alone here; a standalone surface repo will need
  its own overrides decision when it gets one.

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
