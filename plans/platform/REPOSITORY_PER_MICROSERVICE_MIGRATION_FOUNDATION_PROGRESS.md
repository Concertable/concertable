# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M3-Frontend-Build-Config`
- Branch: `Refactor/RepoSplit-M3-Frontend-Build-Config`
- PR: [#948](https://github.com/Concertable/concertable/pull/948), draft; independent sibling based on
  monorepo PR #633 snapshot `ad4ad986f4f61f328ec9aae14a5fec1ccde364db`, at exact local, upstream, and PR
  head `35cc942836469d298415edf37c1ecc805a32d1c6`
- Dependency/package gates: M3 delivery waits for PR #633, an exact landed-main restack and review, and the real
  `@concertable/build-config` publication; preparation and review do not depend on M1 or M2.
- Last reconciled: 2026-09-06 against corrective topology commits `82bf5dbbb` and `bb59d9ba3`, current M3
  head `35cc942836469d298415edf37c1ecc805a32d1c6`, PR #633 base snapshot
  `ad4ad986f4f61f328ec9aae14a5fec1ccde364db`, and live PR #633 head
  `5e2dcf6048c6d71533f1946ed23643d36bdcf71e`.

## Current state

Checkpoint 6A is terminal and checkpoint 6B preparation is active. Existing private `auth`, `b2b`,
`customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities. The remaining
selected repositories are `platform-dotnet`, `platform-frontend`, and separate `system`; none is created by
this packet. General shared frontend code covers web and mobile while those remain package tiers, not
repository boundaries.

M1 is published as four draft stacked PRs #942-#945. M2 is published as independent sibling draft PR #947.
M3 is published as independent sibling draft PR #948 at exact local, upstream, and PR head `35cc94283`, with
six commits above #633 snapshot `ad4ad986f`. Live #633 is two commits ahead at `5e2dcf604`; M3 is prepared
but not current, and the exact landed-main restack is still required before delivery. The implementation
review is approved through `11b322e92`, publication metadata through `84bb9f3a4`, and current head
`35cc94283` carries the ledger correction and review record.
M3 extracts the product-neutral `@concertable/build-config` package, makes product workspaces own their
package lists, and uses the shared Metro resolver for both mobile applications without encoding product
ownership into the platform tier.

## Next Steps

- Keep draft PR #948 based directly on #633 while the dependency remains open; do not merge it ahead of #633.
- After #633 lands, restack M3 exactly once onto the exact landed `origin/main`, revalidate the focused delta,
  and refresh exact-head review and CI.
- Before delivery, publish and feed-verify the real `@concertable/build-config` package, replace local
  validation artifacts with the real pin, then rerun both packed mobile carves and the independent consumer.
- Keep M1, M2, and M3 separate. Do not create repositories, import history, publish packages, or perform a
  service cutover from this preparation branch.

## Completed work

- Checkpoint 6A closed through `.github` PRs #1 and #2; all eleven reusable workflows passed from the public
  fixture before shared policy was applied and read back.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established the retained target identities; the later
  `f4709fe4b` record preserves the selected `platform-dotnet`, `platform-frontend`, and `system` topology.
- M1 is represented by draft PRs #942-#945 and creates no repository.
- M2 is active in its sibling worktree and remains delivery-gated by #633 and its Docker-backed migration
  proof.
- M3 commits `9654935ae` and `62772dc20` were restacked as `9a87b3235` and `96aa1b987` onto #633. The generic
  Metro resolver preserves #633's Stripe and React Native package visibility without product-specific
  platform code.
- M3 checkpoint commit `11b322e92` and approved review commit `9216a0883` were published as draft PR #948
  with #633 as its explicit base. Publication metadata is approved through `84bb9f3a4`; current head
  `35cc94283` carries the ledger correction and review record.

## Verification

- Frontend boundaries: 10/10 tests passed; dependency lint reported zero violations across 13 workspaces.
- Package matrix: all six packages built; 109 package tests passed across the five packages with test scripts.
- Product builds: all five web builds, both mobile TypeScript checks, and both Android/Hermes exports passed.
- Isolation: both fresh feed-restored mobile carves passed typecheck and Android/Hermes export with the shared
  assets resolved from `node_modules/@concertable/mobile` and source package directories absent.
- Independent packed consumer: CommonJS dependency-cruiser/Metro, ESM Vite/Vitest, TypeScript, and package
  subpath resolution passed; the tarball contained only the eight intended files.
- `git diff --check`: pass at the `ad4ad986f` snapshot. Live #633 is two commits ahead; no post-landed-main
  validation is claimed until the required restack.

## Reviews

The original M3 full review over `c6240ecea..62772dc20` found no defects. The implementation agent supplied
the artifact-producing carve and Expo evidence omitted by that read-only pass. The fresh frozen-head review
over `ad4ad986f..11b322e92` approved the complete 27-path candidate with no functional or security findings;
publication metadata is approved through `84bb9f3a4`, and current head `35cc94283` adds only the ledger and
review record. Its durable work order is `reviews/Refactor-RepoSplit-M3-Frontend-Build-Config.md`. The
mandatory landed-main restack changes ancestry and therefore requires a refreshed exact-head review.

## Decisions, discoveries, blockers, and deviations

- `platform-frontend` owns general shared web/mobile packages and tooling. Web and mobile remain package
  tiers within that repository; they are not separate repositories.
- Product packages own their workspace membership. Shared build helpers accept explicit caller-owned inputs
  and do not import B2B or Customer manifests.
- The shared Metro helper discovers project and package `node_modules` roots generically, preserving native
  package visibility introduced by #633 without a Stripe-specific platform rule.
- No repository creation, history import, visibility change, package publication, or cutover was performed.
