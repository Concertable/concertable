# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Consumers`
- Branch: `Refactor/B2bPackageTopologyPhase3-Consumers`
- PR: producer PR [#949](https://github.com/Concertable/concertable/pull/949) merged as
  `15ce7946f0e8ffd1376d599d15639426c9076527`; its package-only tenant-scope follow-up, producer PR
  [#951](https://github.com/Concertable/concertable/pull/951), merged as
  `2df4105989ce462644cc9bc764138aeb0ecc958d`. The active stage is consumer PR
  [#950](https://github.com/Concertable/concertable/pull/950), based on `main`.
- Dependency/package gates: the Phase 2 baseline is satisfied by feed-verified
  `@concertable/b2b@0.1.0-alpha.0.4314` and `@concertable/web-b2b@0.1.0-alpha.0.4314` from terminal
  publication run [32155494572](https://github.com/Concertable/concertable/actions/runs/32155494572).
  Run [34165141235](https://github.com/Concertable/concertable/actions/runs/34165141235) feed-verified
  `0.1.0-alpha.0.6301`, but the exact consumer carve proved the tenant-scoped public hook and store
  declarations were still in the consumer stage. #951 supplies them; its frontend publication run
  [34354787081](https://github.com/Concertable/concertable/actions/runs/34354787081) emits the exact
  version that satisfies the Phase 3 consumer gate.
- Last reconciled: 2026-09-09 against landed `origin/main` `2df4105989ce462644cc9bc764138aeb0ecc958d`,
  merge-group run 34351208460 (32/32 B2B UI and 7/7 Customer UI scenarios green), and the merged
  payment-resolver fix that unblocked it.


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

That follow-up is PR #951. It was 118 commits behind `origin/main`, so current `main` was merged in as
`6474cec75` — a conflict-free merge whose exact head passed CI run
[34274926997](https://github.com/Concertable/concertable/actions/runs/34274926997). Its full review then
recorded and resolved three MEDIUM findings. The consumer stage, PR #950, is still a draft based on this
branch and must be revalidated against the exact published version this producer emits, never against
source-built packages or the moving `alpha` tag.

## Next Steps

On PR #950's worktree, now that #951 is merged and its frontend publication is terminal: record the exact
published `@concertable/b2b` version, run `npm run lock:carve` in `app/` so every standalone surface
lockfile resolves `alpha` to that exact version, and commit the regenerated lockfiles. Then set
`B2B_PHASE3_PRODUCER_VERSION` to that exact version and run `npm run validate:b2b-phase3-consumers` from
`app/`, which pins the venue, artist, and mobile B2B carves to the named producer publication. Rebuild, run
a fresh full review into `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md` (no work order is on
disk), un-draft, and deliver. Only that newer exact version may satisfy the consumer stage's feed carves;
do not substitute a stale lockfile pin or the moving `alpha` tag. The M1 queue hold on PRs #942-#945 is
satisfied.

## Superseded Next Steps

Paused: authorized delivery owner - make green draft PR #949 ready and merge it, then allow the frontend
publication workflow to publish and feed-verify one exact `@concertable/b2b` version. Only after that gate
may the consumer/contraction stage run its exact-version standalone feed carves and proceed toward
delivery. Do not substitute the moving `alpha` tag for the exact package dependency gate.


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
- Exact-head PR CI run 34161321266 passed, including `fe-boundaries` job 101863614281, all seven frontend
  carve jobs 101863614328/329/339/370/375/409/438, and aggregate `ci-complete` job 101864711068.
- PR #949 merged as `15ce7946f0e8ffd1376d599d15639426c9076527` after full-E2E merge-group run
  34164789818 passed. Frontend publication run 34165141235 then published and feed-verified
  `@concertable/b2b@0.1.0-alpha.0.6301`.
- Exact-version consumer validation against `0.1.0-alpha.0.6301` failed deterministically in the venue carve:
  the published `useVenueQuery()` and `useMyVenue(options?)` declarations did not yet contain the tenant
  arguments used by the prepared consumer. The extracted follow-up package passes 9 files/34 tests, its
  build, boundary tests 8/8, and dependency/entrypoint lint across all 13 workspaces.
- At PR #951 head `6474cec75` (current `main` merged in, conflict-free), exact-head CI run 34274926997
  passed every job including all seven `carve-fe` jobs, `fe-boundaries`, `local-platform-pack`,
  `container-images` and `ci-complete`.
- After the review fixes, the ordered web-package build passed end to end: universal shared 9 files/26
  tests, mobile shared 3 files/3 tests, cross-platform B2B 9 files/35 tests, manager-web B2B 13 files/28
  tests, and all six package builds. Frontend boundary tests passed 10/10 and dependency/entrypoint lint
  reported zero violations across all 13 workspaces. `git diff --check` passed. The emitted
  `@concertable/b2b` declarations now carry `useArtistQuery(tenantId: string | undefined)`,
  `useVenueQuery(tenantId: string | undefined)` and `useMyVenue(tenantId, options?)` — the exact shape the
  `0.1.0-alpha.0.6301` carve proved missing.
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
- The PR #949 producer review completed through rebased head `e1dc7a380` with four findings resolved across
  its commits — B2B ownership of write contracts, active-profile publication verification, corrected phase
  status, and real artist/venue `features/*/types` exports. That work order was spent when #949 merged and
  is not on disk.
- The PR #951 follow-up carries its own work order at
  `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`. Its full pass over
  `ef8d505fd..6474cec75` recorded three MEDIUM findings — tenant-selection serialization that left
  `configure`/`resolve` outside the queue, `isLoading` reporting `false` while a disabled query waits for a
  tenant, and optional `tenantId` parameters that let a stale consumer of the published package compile.
  All three are resolved. No frozen path matches the merge gate's `security_paths` inventory, so the pass
  carries no security marker.
- The PR #951 work order additionally carries an incremental pass over the merged payment-resolver fix,
  with the `Security-reviewed up to commit:` marker the merge gate requires for a Payment path. It was
  spent when #951 merged.
- Phase 3 producer and consumer-stage reviews are independent. The consumer stage's historical findings
  were resolved against a superseded candidate and **no consumer work order is on disk**, so the consumer
  stage requires a fresh full review at `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md` over its
  current `main`-merged candidate. The combined local proof remains validation evidence, not a candidate.


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
- **The exact-version consumer gate now exists in the repository.** PR #964
  (`Refactor/FrontendRegistryPackageConsumers`, merged as `6cd7fd615`) gave every carved surface its own
  standalone `package-lock.json` — `app/web/{customer,admin}`, `app/web/b2b/{venue,artist,business}` and
  `app/mobile/{customer,b2b}` — regenerated by `npm run lock:carve` in `app/`. `carve-fe.mjs` still
  rewrites `@concertable/*` specifiers to `alpha`, but the lockfile now resolves that tag to one frozen
  version — `0.1.0-alpha.0.6314` as committed by #964, refreshed to `0.1.0-alpha.0.6462` by PR #965
  (`Chore/FrontendCarveLockRefresh`, merged as `eefd70efc`). This supersedes the earlier finding that `carve-fe.mjs` had no
  exact-version option: **PR #950 must run `npm run lock:carve` after this producer publishes and commit
  the regenerated lockfiles**, or its carves will silently install the stale package instead of the
  tenant-scoped one. `app/web/b2b/{artist,venue}` and `app/mobile/b2b` additionally gain a
  `@concertable/b2b` dependency on that branch, which only a regenerated lockfile can resolve.
- **The merge-queue blocker is diagnosed and fixed: it was a Payment defect, not a flake and not #959.**
  Two merge-group attempts ([34286556068](https://github.com/Concertable/concertable/actions/runs/34286556068)
  and [34291413140](https://github.com/Concertable/concertable/actions/runs/34291413140)) were ejected by the
  same single UI scenario of 32, `Venue manager completes 3DS challenge on flat fee`, timing out after 30s on
  `Then a draft concert is created`. Neither the frontend nor this branch's diff was involved.
  **Diagnosed from the CI artifacts, not from a local reproduction.** The failure screenshot shows the venue
  SPA sitting on `VenueAcceptCheckoutFlow` with "Acceptance confirmed", polling
  `/concert/application/{id}` every second through `useConcertByApplicationQuery` and navigating via
  `<Navigate>` when it appears. The earlier "SignalR-only redirect" reading was wrong: the redirect is
  poll-driven and `useVenueNotifications` only accelerates it, so the frontend was never the problem and the
  `Load`-to-`Commit` step change was unrelated. `payment-web` logged
  `PaymentProviderUnavailableException` three times for `concertable.payment.capture-escrow.v1` and then the
  command was gone; no `payment_intent.succeeded` for that intent appears anywhere in either run, so the
  escrow was authorized and never captured.
  **Cause:** `PaymentOperationResolver` treated a rejected transition evaluation as
  `PaymentOperationError.ProviderUnavailable`. `FinancialOperationHandler.HandleResolutionFailureAsync`
  throws only for that case, `AzureServiceBusReceiver.AbandonWithBackoffAsync` abandons with capped
  exponential backoff, and Azure Service Bus dead-letters at `MaxDeliveryCount` - three deliveries under the
  emulator. Fixed in `8f2567623` by resolving from the canonical attempt, so a concurrent observation that
  already reached `Authorized` proceeds and anything else is rejected with its own typed error and reaches
  the consumer as `CaptureEscrowRejectedEvent`.
  **Ruled out along the way, each with evidence:** the frontend; the hourly `ConcertFinishedFunction`
  (`0 0 * * * *`, settled 22 concerts at 00:00:00 in run 2, but run 1 failed at 22:58 before its 23:00
  firing); a retained `last_payment_error` (the Playwright trace shows it null on the final
  `requires_capture` intent); the sequential 3DS webhook ordering; the eager/webhook reconcile race; and a
  Stripe retrieve failure (`"Stripe rejected"` appears zero times in both runs). PR #959 was the recorded
  top suspect and was not implicated.
  **Not fixed, recorded as a new HIGH in `api/Concertable.Payment/TECH_DEBT.md`:** nothing re-drives a
  dead-lettered financial operation, `PaymentSessionAttemptEntity.NextReconcileAt` is written and indexed but
  never read, and `PaymentSessionReconciliationSource.Sweep` has no implementation. A genuine provider outage
  lasting longer than the delivery budget still strands an authorized escrow, which is a production blocker
  in its own right and a larger change than belongs on this PR.
- **An earlier hypothesis, now disproved, still produced a correct change.**
  Merge-group run [34286556068](https://github.com/Concertable/concertable/actions/runs/34286556068) failed
  one UI scenario of 32 — `Venue manager completes 3DS challenge on flat fee` timed out after 30s in
  `Then a draft concert is created`, `waiting for navigation to "**/my/concerts/concert/**" until "Load"`.
  `VenueManagerSteps.DraftConcertCreated` was the last step still calling `WaitForURLAsync` with
  Playwright's default `WaitUntil = Load`; the concert page holds connections open (Stripe iframes, Google
  Maps), so the load event need not fire inside the timeout even though the route already changed. The repo
  already owns the fix — `WaitForSpaUrlAsync` (`WaitUntilState.Commit`), used throughout the Customer UI
  suite — and the step now uses it. `app/b2b/shared` is imported by no surface, so this branch's own change
  cannot reach that scenario.
- The feed `alpha` tag is not a reproducible Phase 2 dependency gate: it advanced from the verified
  `0.1.0-alpha.0.4314` to `0.1.0-alpha.0.5913`, which does not contain Phase 2's additive active-profile
  exports. Do not alter production pins or topology to conceal this drift; preserve package publication
  before consumer/contraction delivery and validate exact versions.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase3-Consumers
Validate and review the consumer/contraction Phase 3 candidate, read @plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md and @plans/platform/B2B_PACKAGE_TOPOLOGY_PROGRESS.md, and do what its `## Next Steps` says without merging or delivering consumers ahead of the exact producer package gate.
```
