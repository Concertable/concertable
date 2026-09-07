# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M3-Frontend-Build-Config`
- Branch: `Refactor/RepoSplit-M3-Frontend-Build-Config`
- PR: [#948](https://github.com/Concertable/concertable/pull/948), draft; restacked directly onto exact landed
  `origin/main` `516f4cc25936289744babef3f98b1a297035fbb6`. Local, upstream, and PR head were proven equal
  at `b6596ca8573d0dd4b3f398248190f1d3a64b48ac`; this commit carries the focused split-inventory
  repair found by that head's CI.
- Dependency/package gates: PR #633 and the exact landed-main restack/review are complete. M3 does not depend
  on M1 or M2. Its merge must causally trigger the real `@concertable/build-config` publication and feed
  verification before this delivery is terminal.
- Last reconciled: 2026-09-07 against landed `origin/main`
  `516f4cc25936289744babef3f98b1a297035fbb6`, patch-equivalent restacked candidate
  `93d102222f9cc25b5f4b68af97e6f08df59f16b0`, and the focused M3 validation and review evidence below.

## Current state

Checkpoint 6A is terminal and checkpoint 6B preparation is active. Existing private `auth`, `b2b`,
`customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities. The remaining
selected repositories are `platform-dotnet`, `platform-frontend`, and separate `system`; none is created by
this packet. General shared frontend code covers web and mobile while those remain package tiers, not
repository boundaries.

M1 is published as four draft stacked PRs #942-#945. M2 is published as independent sibling draft PR #947.
M3's seven commits were restacked without conflict from #633 snapshot `ad4ad986f` onto the exact landed-main
merge `516f4cc25`; `git range-diff` reports every old/new pair as patch-equivalent (`=`) and preserves the
original order. The reviewed pre-checkpoint candidate is `93d102222`, and draft PR #948 was force-with-lease
updated to checkpoint `b6596ca85`. Its first exact-head CI exposed a stale generated split inventory: the
generator still named the obsolete `platform-web` target and did not assign the new build-config workspace.
The focused repair assigned that owned topology and regenerated the inventory. Exact-head CI run
`34162425623` is green. This commit closes the remaining publication-rail omission by adding build-config to
the main-branch frontend publisher and its clean feed-consumer verification.
M3 extracts the product-neutral `@concertable/build-config` package, makes product workspaces own their
package lists, and uses the shared Metro resolver for both mobile applications without encoding product
ownership into the platform tier.

## Next Steps

- Validate and review this publication-rail repair, push one stable candidate, require exact-head PR CI, then
  make PR #948 ready and deliver it independently through the merge queue.
- Bind the post-merge frontend publication run to #948's landing commit and require
  `@concertable/build-config` to pass the workflow's clean feed-consumer verification before calling M3
  terminal.
- Keep M1, M2, and M3 separate. Do not create repositories, import history, publish packages, or perform a
  service cutover from this preparation branch.

## Completed work

- Checkpoint 6A closed through `.github` PRs #1 and #2; all eleven reusable workflows passed from the public
  fixture before shared policy was applied and read back.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established the retained target identities; the later
  `f4709fe4b` record preserves the selected `platform-dotnet`, `platform-frontend`, and `system` topology.
- M1 is represented by draft PRs #942-#945 and creates no repository.
- M2 remains owned by its sibling worktree; its current delivery gate is recorded there.
- M3 implementation commits are now `9a4e894f8` and `901b27c72` on landed main. The generic Metro resolver
  preserves #633's Stripe and React Native package visibility without product-specific platform code.
- The original seven-commit sequence was preserved as `9a4e894f8`, `901b27c72`, `b53944dfd`, `cdecab835`,
  `0cdf50364`, `2dbd0a7ac`, and `93d102222`; the exact old/new range-diff is patch-equivalent throughout.
- Checkpoint `b6596ca85` carried the refreshed landed-main review and preparation metadata and was published
  to draft PR #948 with local/upstream/PR head equality proven.
- This commit assigns `app/build-config` to the corrected `platform-frontend` extraction target, replaces the
  obsolete `platform-web` generator/map label, and refreshes the generated inventory.

## Verification

- Landed-main integrity: seven-commit `git range-diff` is patch-equivalent throughout and
  `git diff --check 516f4cc25..93d102222` passed.
- Frontend boundaries: 10/10 tests passed; dependency lint reported zero violations across all 13 workspaces.
- Package matrix: all six packages built; 109 package tests passed across the five packages with test scripts.
- Product builds: all five web builds, both mobile TypeScript checks, and both Android/Hermes exports passed.
  The sandbox denied execution of `hermesc.exe`; the unchanged commands passed outside that restriction.
- Isolation: both fresh feed-restored mobile carves passed typecheck and Android/Hermes export with shared
  assets resolved from `node_modules/@concertable/mobile` and source package directories absent.
- Independent packed consumer: CommonJS dependency-cruiser/Metro, ESM Vite/Vitest, TypeScript config and
  package subpath resolution passed; the tarball contained only the eight intended files.
- Split inventory: `python eng/repository-split/inventory.py --check` passes; a focused assertion proves
  `app/build-config` targets `platform-frontend` and no frontend workspace remains unassigned.

## Reviews

The fresh frozen-head full review over `516f4cc25936289744babef3f98b1a297035fbb6..93d102222f9cc25b5f4b68af97e6f08df59f16b0`
approved the complete 30-path landed-main candidate with no functional or security findings. This commit adds
the CI-driven inventory/map repair; the durable work order
`reviews/Refactor-RepoSplit-M3-Frontend-Build-Config.md` owns its required incremental watermark before the
next push.

## Decisions, discoveries, blockers, and deviations

- `platform-frontend` owns general shared web/mobile packages and tooling. Web and mobile remain package
  tiers within that repository; they are not separate repositories.
- Product packages own their workspace membership. Shared build helpers accept explicit caller-owned inputs
  and do not import B2B or Customer manifests.
- The extraction generator and map use the selected `platform-frontend` identity and assign
  `app/build-config` to that target; the superseded `platform-web` label must not return.
- The shared Metro helper discovers project and package `node_modules` roots generically, preserving native
  package visibility introduced by #633 without a Stripe-specific platform rule.
- No repository creation, history import, visibility change, package publication, or cutover was performed.
