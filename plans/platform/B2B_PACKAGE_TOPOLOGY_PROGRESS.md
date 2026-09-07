# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Consumers`
- Branch: `Refactor/B2bPackageTopologyPhase3-Consumers`
- PR: draft consumer PR [#950](https://github.com/Concertable/concertable/pull/950), restacked on package-only
  producer PR [#951](https://github.com/Concertable/concertable/pull/951) at exact producer candidate
  `420345df04f7cd31e39015c92d6309cb5e0ff491`
- Dependency/package gates: Phase 2 is satisfied by feed-verified
  `@concertable/b2b@0.1.0-alpha.0.4314` and `@concertable/web-b2b@0.1.0-alpha.0.4314`
  from terminal publication run [32155494572](https://github.com/Concertable/concertable/actions/runs/32155494572).
  PR #949 published and feed-verified `0.1.0-alpha.0.6301`, but exact consumer validation proved its
  public hooks still lacked the tenant-scoped signatures. The Phase 3 exact producer-publication gate is
  therefore explicitly **unsatisfied** until PR #951 publishes and feed-verifies a newer exact version.
- Last reconciled: 2026-09-08 against producer candidate `420345df0`, locally restacked consumer head
  `0fcdd340255a65c6a013df2ac730c31004776ecf`, green focused validation, and the coordinated hold keeping
  PRs #951/#950 out of the merge queue until the ordered M1 stack PRs #942-#945 is terminal.

## Current state

Phase 1 is terminal. PR #643 merged as `50f89dbfe` after full-E2E merge-group run
[32052220186](https://github.com/Concertable/concertable/actions/runs/32052220186) passed. Consolidated
frontend publication run [32055197413](https://github.com/Concertable/concertable/actions/runs/32055197413)
published and feed-verified `@concertable/web-b2b@0.1.0-alpha.0.4284`. Its merged worktree was removed
through `scripts/worktrees.ps1 close -PlanManaged`.

Phase 2 is terminal. PR #653 merged as `4963a70a27d1e5dccb6b8b250ed676903052cbc3`
after exact-head CI run [32148992284](https://github.com/Concertable/concertable/actions/runs/32148992284)
passed the frontend package, boundary, carve, build, unit, and integration gates. Publication run
[32155494572](https://github.com/Concertable/concertable/actions/runs/32155494572) then published and
feed-verified both first-class package identities at `0.1.0-alpha.0.4314`.

Phase 3 is split at the real publication boundary. The producer stage expands the retained
`@concertable/b2b` package with the active-profile implementation and the optional/all-membership tenant
contract required by mobile. This stacked consumer/contraction stage migrates web and mobile consumers,
removes the duplicate universal and manager-web implementations, repairs the mobile tenant edge, and closes
the organization-profile route integration. It may be reviewed and published as a draft, but cannot complete
its feed carves or merge until the producer is merged, published, and feed-verified at an exact version. The
original combined local branch is retained only as a recovery reference; it is not a publication candidate.

The five substantive consumer commits are now replayed onto exact follow-up producer head `420345df0`.
Conflicts retained the landed M3 build-config API and carve coverage, and retained PR #951's final reviewed
tenant-session implementation. The resulting stage diff contains no `app/b2b/shared` paths, preserving the
corrected producer/publication boundary.

## Next Steps

Keep PRs #951 and #950 prepared but out of the merge queue until the ordered M1 stack PRs #942-#945 is
terminal. Then refresh #951 onto exact `main`, deliver it first, and bind its frontend publication to the
landing SHA. Do not merge #950 before that publication emits and feed-verifies an exact newer
`@concertable/b2b` version. At that gate, set
`B2B_PHASE3_PRODUCER_VERSION` to that exact version and run `npm run validate:b2b-phase3-consumers` from
`app/`; this pins the venue, artist, and mobile B2B carves to the named producer publication. Restack/retarget
#950 onto the resulting exact `main`, run exact-head CI, then deliver. Do not substitute the moving `alpha`
tag for the exact package dependency gate.

## Completed work

- **Phase 1 terminal (PR #643, merge `50f89dbfe`):** non-mutating alias packer, install-level unit test,
  dual publication/feed verification, clean correctness/architecture/security review, full E2E, and
  published `@concertable/web-b2b@0.1.0-alpha.0.4284`.
- **Phase 2 coherent candidate (`fc59c26aa`):** first-class manager-web package rename and consumer
  import migration; new cross-platform B2B package with artist/venue Query and Mutation APIs plus a
  configurable, persisted tenant session; explicit workspace/build/publication/boundary integration;
  obsolete alias packer removal; and complete lockfile regeneration.
- **Phase 2 review fixes (`6e87fcf36`):** removed editor facades that mirrored Query data into Zustand
  and bypassed the zod write boundary, and added focused artist/venue multipart contract tests.
- **Reviewed work-head push:** created `origin/Refactor/B2bPackageTopologyPhase2` from no prior remote
  ref and verified its fetched tip equals `60fcd7395f57c9f73eb3cc5e5ee198aecfa8fd5d`.
- **Draft PR #653:** opened from verified local/remote head
  `dca48cd6aae06aa55f8f7b98d8444f03e640f02e`; initial exact-head run 32148856488 has a green change
  detector with the six feed-restored carves, frontend boundaries, and local platform pack pending.
- **Mobile workspace resolution:** both Metro configurations watch every junctioned shared workspace
  they resolve locally, while carved/feed installs continue to use physical package directories.
- **Phase 2 terminal (PR #653, merge `4963a70a2`):** exact-head CI passed and publication run
  32155494572 feed-verified both first-class B2B packages at `0.1.0-alpha.0.4314`.
- **Phase 3 combined local proof:** moved active artist/venue profile ownership into
  `@concertable/b2b`, preserved the established `my` and public-detail cache keys, migrated web/mobile
  profile and tenant consumers, removed universal active-profile and manager-web tenant-core duplicates,
  and retained only manager-web UI/platform adapters. The proof was then split before publication so the
  producer can be published and exact-version verified ahead of its consumers.
- **Phase 3 producer stage:** contains only the cross-platform package implementation, dependency manifest
  and lockfile closure, focused tests, and package verification assertion required for publication.
- **Phase 3 consumer/contraction stage:** is stacked on the producer and contains only the web/mobile consumer
  migration, duplicate deletion, platform adapters, mobile tenant/session wiring, dependency closure, and
  organization-profile route consumers.
- **Mobile B2B tenant edge:** replaced the unsafe identity cast and venue-presence routing with typed
  identity data, all-artist-and-venue membership resolution, SecureStore persistence, chooser/switcher
  composition, validated `X-Tenant-Id` wiring for API and payment clients, and logout/401 clearing.
- **Organization-profile contraction:** consumers and route guards now use only
  `/organization/artist` and `/organization/venue`; no compatibility route was added.
- **Consumer review repairs:** active artist/venue profile caches and drafts are tenant-scoped, the mobile
  navigation/editor subtree remounts on tenant changes, tenant selections are serialized with latest-wins
  persistence and pending controls, failed tenant-session hydration exposes a retry path, and terminal B2B
  consumer carves accept only a named exact package version.

## Verification

- Phase 1 exact-head CI attempt 2 passed after attempt 1 failed closed on a GitHub GraphQL 503 before
  any build/test job ran. Full-E2E merge-group run 32052220186 and publication run 32055197413 passed.
- `@concertable/b2b`: 5 focused test files and 15 tests passed; build typecheck and alias rewriting
  passed.
- Existing package gates passed: universal shared 6/6, manager-web B2B 17/17, web shared 25/25, and
  shared, web, customer, mobile, B2B, and web-B2B package builds.
- Boundary tooling passed 2/2 tests and dependency-cruiser reported zero violations across all 12
  workspaces.
- Customer, venue, artist, and business production web builds passed. Both mobile TypeScript checks
  and both Android exports passed.
- CI-equivalent, version-pinned local tarballs passed clean-consumer verification:
  `@concertable/b2b` under Node and Metro/Android, and `@concertable/web-b2b` under Node.
- Workspace lockfile regeneration, plan graph validation, package JSON parsing, identity/platform
  grep gates, and `git diff --check` passed.
- Phase 2 feed-restored surface carves and complete frontend matrices passed in exact-head run
  32148992284; publication and feed verification passed in run 32155494572.
- Phase 3 cross-platform core passed 7 files/25 tests and package build; universal shared passed 5
  files/12 tests; manager-web B2B passed 10 files/17 tests and package build.
- Ordered web-package build passed. Customer, admin, business, artist, and venue production builds
  passed; the final post-cache-key artist and venue reruns also passed.
- Mobile shared package build passed. Both mobile TypeScript checks and both Android exports passed;
  the exports required an unsandboxed rerun because the sandbox denied execution of Expo's Hermes
  binary.
- Frontend boundary tests passed 8/8 and dependency/entrypoint lint passed across all 13 workspaces.
  Duplicate/unintended-import grep gates and `git diff --check` passed.
- Both B2B packages passed prepack build/tests and produced local tarballs. Clean-consumer verification
  remains publication-gated because unversioned source tarballs retain workspace `*` dependencies;
  the publication workflow pins all intra-Concertable dependencies before packing.
- At rebased producer head `e1dc7a380`, the ordered web-package build, boundary tests (8/8), and
  dependency/entrypoint lint across all 13 workspaces passed. Publication-equivalent exact local packages at
  `0.1.0-alpha.0.6300` passed clean Node and Metro/Android consumer verification: `@concertable/b2b`
  SHA-256 `dd07afac9032015419bf983d5443d604d6c6be24e8748d7964df0ef4133cb429` and
  `@concertable/web-b2b` SHA-256 `080b4f9f473afc6370c64de99c260a81f49e1760068977903f00ce450343eaad`.
- At rebased consumer head `33f797195`, all six ordered package builds/tests, all five production web builds,
  both mobile TypeScript checks and Android/Hermes exports, boundary tests (10/10), and dependency/entrypoint
  lint across all 13 workspaces passed. The exact-version carve-tooling cases passed; terminal standalone
  feed carves remain gated on the producer's exact published version.
- After restacking onto follow-up producer `420345df0`, all six package builds/tests, all five production
  web builds, both mobile TypeScript checks and Android/Hermes exports, boundary tests (12/12), and
  dependency/entrypoint lint across all 13 workspaces passed. The Hermes exports and mobile carve fixtures
  required unsandboxed reruns solely to execute the workspace Hermes binary and write temporary Git metadata.
  Terminal exact-feed validation remains correctly gated on PR #951's unpublished version.
- Feed carves of the combined artist and venue consumers restored the current `alpha`
  (`0.1.0-alpha.0.5913`) and correctly failed because that moving tag lacks Phase 2's active-profile
  exports. The exact feed-verified Phase 2 artifact `0.1.0-alpha.0.4314` remains resolvable. The repaired
  carve accepts a named exact version for terminal validation and rejects explicit tags or ranges; mobile
  still requires the unpublished all-membership tenant-core API, so terminal carves remain gated on the
  Phase 3 producer publication.
- Consumer repair verification passed the cross-platform B2B suite (9 files/34 tests), including focused
  tenant cache/draft scope, concurrent latest-wins persistence, pending state, and hydration retry cases.
  The cross-platform package build, mobile B2B TypeScript and Android export gates, artist and venue
  production builds,
  manager-web suite (10 files/17 tests), frontend boundary tests (10/10), and dependency/entrypoint lint
  across all 13 workspaces passed. Exact-version carve tooling tests passed 7/7; the actual feed-restored
  terminal carves remain blocked on the explicitly unsatisfied Phase 3 producer-publication gate.

## Reviews

- Phase 1 review is terminal with no open findings in `reviews/Refactor-B2bPackageTopology.md`.
- Full native, frontend-architecture, test-coverage, and workflow-security review of
  `de4f377e8..fc59c26aa` recorded four findings in
  `reviews/Refactor-B2bPackageTopologyPhase2.md`; all were addressed in `6e87fcf36`. Incremental review
  of `fc59c26aa..6e87fcf36` found no new issues, and the review/security watermarks are current through
  `6e87fcf36`.
- Phase 3 producer and consumer-stage reviews are independent. The consumer review is complete through
  rebased head `33f797195`: all six findings are resolved and the final functional/security pass found no
  new issue. The canonical work order is
  `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md`; the combined local proof remains validation
  evidence, not a candidate published as one indivisible change.

## Decisions, discoveries, blockers, and deviations

- `@concertable/b2b` is retained as the cross-platform B2B owner; it is not an old identity to grep out
  or retire. `@concertable/web-b2b` becomes the manager-web-only tier.
- One source directory produces both names only for the Phase 1 publication bridge; no duplicate
  workspace package or runtime source tree is permitted.
- Web and mobile active-profile consumers must retain the same active-tenant behaviour after moving to
  the cross-platform package. The cutover changes ownership, not product semantics.
- Mobile currently chooses a surface from the presence of any venue membership and never attaches
  `X-Tenant-Id`. Phase 3 must replace that behavior with the web-equivalent active-membership chooser,
  persisted platform adapter, validated tenant session, and tenant-aware client wiring.
- The organization-profile route-contraction work is a Phase 3 downstream integration consumer and
  must not invent compatibility APIs while package publication gates are still open.
- The feed `alpha` tag is not a reproducible Phase 2 dependency gate: it advanced from the verified
  `0.1.0-alpha.0.4314` to `0.1.0-alpha.0.5913`, which does not contain Phase 2's additive active-profile
  exports. Do not alter production pins or topology to conceal this drift; preserve package publication
  before consumer/contraction delivery and validate exact versions.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Consumers
Validate and review the consumer/contraction Phase 3 candidate, read @plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md and @plans/platform/B2B_PACKAGE_TOPOLOGY_PROGRESS.md, and do what its `## Next Steps` says without merging or delivering consumers ahead of the exact producer package gate.
```
