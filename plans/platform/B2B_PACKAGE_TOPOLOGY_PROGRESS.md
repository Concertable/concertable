# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase2`
- Branch: `Refactor/B2bPackageTopologyPhase2`
- PR: draft [#653](https://github.com/Concertable/concertable/pull/653)
- Dependency/package gates: Phase 1 is satisfied by feed-verified `@concertable/web-b2b@0.1.0-alpha.0.4284`; Phase 3 is delivery-gated on the first feed-published versions of both first-class Phase 2 packages.
- Last reconciled: 2026-08-18 against `origin/main` `de4f377e817127bfd200bf9f92a807ac2423c757` and the terminal Phase 1 publication.

## Current state

Phase 1 is terminal. PR #643 merged as `50f89dbfe` after full-E2E merge-group run
[32052220186](https://github.com/Concertable/concertable/actions/runs/32052220186) passed. Consolidated
frontend publication run [32055197413](https://github.com/Concertable/concertable/actions/runs/32055197413)
published and feed-verified `@concertable/web-b2b@0.1.0-alpha.0.4284`. Its merged worktree was removed
through `scripts/worktrees.ps1 close -PlanManaged`.

Phase 2 has a coherent local candidate. `app/web/b2b/shared` is now the first-class
`@concertable/web-b2b` package and both manager surfaces consume that identity. The retained
`@concertable/b2b` identity now belongs to `app/b2b/shared`, with additive artist, venue, and
platform-configured tenant-core exports. Publication, workspace, lockfile, boundary, and Metro
integration are updated, with no consumer cutover begun ahead of Phase 3. The reviewed work head
`60fcd7395f57c9f73eb3cc5e5ee198aecfa8fd5d` created the correctly capitalized remote branch and was
fetched back equal. The push checkpoint then landed as `dca48cd6aae06aa55f8f7b98d8444f03e640f02e`,
and draft PR #653 opened from that exact local/remote head.

## Next Steps

Let exact-head PR CI run [32148856488](https://github.com/Concertable/concertable/actions/runs/32148856488)
through the feed-restored carve and complete frontend matrices; diagnose and fix any real failure at
its focused scope. Once exact-head CI is green, retain the draft and wait for explicit `/merge`
authorization. Do not start Phase 3 delivery until both canonical packages from Phase 2 are published
and feed-verified.

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
- Feed-restored surface carves and the complete frontend matrices remain assigned to exact-head PR CI.

## Reviews

- Phase 1 review is terminal with no open findings in `reviews/Refactor-B2bPackageTopology.md`.
- Full native, frontend-architecture, test-coverage, and workflow-security review of
  `de4f377e8..fc59c26aa` recorded four findings in
  `reviews/Refactor-B2bPackageTopologyPhase2.md`; all were addressed in `6e87fcf36`. Incremental review
  of `fc59c26aa..6e87fcf36` found no new issues, and the review/security watermarks are current through
  `6e87fcf36`.

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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopologyPhase2
Read @plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md and @plans/platform/B2B_PACKAGE_TOPOLOGY_PROGRESS.md and do what its `## Next Steps` says.
```
