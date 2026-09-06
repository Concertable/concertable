# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Producer`
- Branch: `Refactor/B2bPackageTopologyPhase3-Producer`
- PR: not opened; this is the Phase 3 package-producer stage based on PR #633
- Dependency/package gates: satisfied by feed-verified `@concertable/b2b@0.1.0-alpha.0.4314` and `@concertable/web-b2b@0.1.0-alpha.0.4314` from terminal Phase 2 publication run [32155494572](https://github.com/Concertable/concertable/actions/runs/32155494572).
- Last reconciled: 2026-09-06 against exact PR #633 head
  `5e2dcf6048c6d71533f1946ed23643d36bdcf71e`, merged Phase 2 PR
  [#653](https://github.com/Concertable/concertable/pull/653), and its successful publication run.

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

Phase 3 is split at the real publication boundary. This producer stage expands the retained
`@concertable/b2b` package with the active-profile implementation and the optional/all-membership tenant
contract required by mobile. The consumer/contraction stage remains local until this producer is reviewed,
merged, published, and feed-verified at an exact version. The original combined local branch is retained only
as a recovery reference; it is not a publication candidate. The historical B2B repository handoff is
superseded and is not an execution target.

## Next Steps

Validate and review this producer-only stage, then publish it as a draft PR based on #633. Do not publish the
package before that PR merges. After merge, publish and feed-verify an exact `@concertable/b2b` version; only
then rebase and deliver the stacked consumer/contraction stage and require its standalone feed carves. Do not
substitute the moving `alpha` tag for the exact package dependency gate.

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
- **Mobile B2B tenant edge:** replaced the unsafe identity cast and venue-presence routing with typed
  identity data, all-artist-and-venue membership resolution, SecureStore persistence, chooser/switcher
  composition, validated `X-Tenant-Id` wiring for API and payment clients, and logout/401 clearing.
- **Organization-profile contraction:** consumers and route guards now use only
  `/organization/artist` and `/organization/venue`; no compatibility route was added.

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
- After the publication-safe split and restack onto #633 head `5e2dcf604`, the producer-only stage passed
  the universal package build/tests (9 files/26 tests), cross-platform B2B build/tests (7 files/25 tests),
  boundary tests (8/8), and dependency/entrypoint lint across all 13 workspaces.
- Feed carves of the combined artist and venue consumers restored the current `alpha`
  (`0.1.0-alpha.0.5913`) and correctly failed because that moving tag lacks Phase 2's active-profile
  exports. The exact feed-verified Phase 2 artifact `0.1.0-alpha.0.4314` remains resolvable, but
  `carve-fe.mjs` has no exact-version option and always rewrites dependencies to `alpha`. Mobile also
  requires this candidate's new all-membership tenant-core API, so its terminal carve must follow
  publication of the Phase 3 package expansion.

## Reviews

- Phase 1 review is terminal with no open findings in `reviews/Refactor-B2bPackageTopology.md`.
- Full native, frontend-architecture, test-coverage, and workflow-security review of
  `de4f377e8..fc59c26aa` recorded four findings in
  `reviews/Refactor-B2bPackageTopologyPhase2.md`; all were addressed in `6e87fcf36`. Incremental review
  of `fc59c26aa..6e87fcf36` found no new issues, and the review/security watermarks are current through
  `6e87fcf36`.
- Phase 3 producer review has not started. The combined local proof is validation evidence, not the candidate
  that will be published.

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
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Producer
Validate and review the producer-only Phase 3 candidate, read @plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md and @plans/platform/B2B_PACKAGE_TOPOLOGY_PROGRESS.md, and do what its `## Next Steps` says without publishing the package or consumer cutover ahead of its delivery gates.
```
