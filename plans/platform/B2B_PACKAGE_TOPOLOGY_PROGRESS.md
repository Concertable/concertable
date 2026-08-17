# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopology`
- Branch: `Refactor/B2bPackageTopology`
- PR: draft [#643](https://github.com/Concertable/concertable/pull/643)
- Dependency/package gates: Phase 2 is delivery-gated on a feed-published and verified Phase 1 `@concertable/web-b2b` version; Phase 3 is delivery-gated on the first feed-published versions of both first-class Phase 2 packages.
- Last reconciled: 2026-08-17 against `origin/main` `c90ece92a879b97619b9691426a2d65176b0841d` and the settled two-package topology.

## Current state

Phase 1 is implemented, committed, verified, fully reviewed, and pushed to draft PR #643. The reusable packer
stages the published files under an alias without mutating the source manifest; PR CI now runs its
focused unit test, and the publish workflow packs, verifies, publishes, and feed-verifies both names.
The existing source package remains `@concertable/b2b`; no consumer manifests, imports, lockfile
entries, or runtime files have changed. Local, remote-tracking, and PR head are verified equal at
`921412108b63d1eca52346656fc6fee5f1c6f743`. Exact-head PR CI run
[32049656253](https://github.com/Concertable/concertable/actions/runs/32049656253) passed, after which
the branch merged nine new `origin/main` commits cleanly. The alias test and ordered web-package gate
passed again against that current base; the updated merge head is not pushed yet.

## Next Steps

Push the current-main merge head, verify local/remote/PR equality, and let replacement exact-head CI
pass. Then enqueue PR #643 with `full-e2e`, land Phase 1, and verify the publish workflow produced the
exact `@concertable/web-b2b` version before beginning Phase 2 delivery.

## Completed work

- **Phase 1 implementation (`693c68c9a`):** non-mutating alias packer and install-level unit test; parameterized B2B
  package verification; dual tarball/feed publication; PR-CI tool-test wiring; package-topology plan
  and roadmap registration.
- **Review and draft PR (`1613e2e7c`, PR #643):** full correctness/architecture/security review found
  no issues; the reviewed head was pushed and verified equal across local, remote-tracking, and the
  initial draft PR head.

## Verification

- `node --test app/scripts/pack-fe-package-alias.test.mjs`: passed 1/1 from the root execution context.
- `npm run build:web-packages`: passed in dependency order; `@concertable/shared` tests 6/6,
  `@concertable/b2b` tests 17/17, and shared, web, customer, and B2B package builds all completed green.
- After merging current `origin/main` `c90ece92a`, `npm run test:package-tools` passed 1/1 and
  `npm run build:web-packages` passed again with shared 6/6 and B2B 17/17 tests.
- `git diff --check`: passed.
- `app/package.json`: JSON parse passed.
- New-file trailing-whitespace scan: passed.
- `app/web/b2b/shared/package.json` and `app/package-lock.json`: zero diff; consumers and runtime source
  are unchanged.

## Reviews

- Full correctness and architecture review of `9205e82df..693c68c9a` found no issues. Security review
  of the workflow/package-publication changes found no issues. Both watermarks are recorded in
  `reviews/Refactor-B2bPackageTopology.md`.
- Incremental review of the review/ledger-only range `693c68c9a..382070aec` found one stale next-step
  entry; it is corrected in the review checkpoint and no finding remains open.

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
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopology
Read @plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md and @plans/platform/B2B_PACKAGE_TOPOLOGY_PROGRESS.md and do what its `## Next Steps` says.
```
