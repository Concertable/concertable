# B2B package topology cutover progress

- Plan: `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/b2b-package-topology`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2bPackageTopology`
- Branch: `Refactor/B2bPackageTopology`
- PR: not opened
- Dependency/package gates: Phase 2 is delivery-gated on a feed-published and verified Phase 1 `@concertable/web-b2b` version; Phase 3 is delivery-gated on the first feed-published versions of both first-class Phase 2 packages.
- Last reconciled: 2026-08-17 against clean `origin/main` `9205e82df4359df8ddf8dfdace07b4aa09b6d186` and the settled two-package topology.

## Current state

Phase 1 is implemented and verified locally in the dedicated producer worktree. The reusable packer
stages the published files under an alias without mutating the source manifest; PR CI now runs its
focused unit test, and the publish workflow packs, verifies, publishes, and feed-verifies both names.
The existing source package remains `@concertable/b2b`; no consumer manifests, imports, lockfile
entries, or runtime files have changed.

## Next Steps

Run `/review` on the committed Phase 1 producer bridge before opening a draft PR. Do not begin Phase 2
delivery until the merged workflow has published and feed-verified the exact
`@concertable/web-b2b` version.

## Completed work

- **Phase 1 local candidate:** non-mutating alias packer and install-level unit test; parameterized B2B
  package verification; dual tarball/feed publication; PR-CI tool-test wiring; package-topology plan
  and roadmap registration. The complete local candidate is verified and ready to commit.

## Verification

- `node --test app/scripts/pack-fe-package-alias.test.mjs`: passed 1/1 from the root execution context.
- `npm run build:web-packages`: passed in dependency order; `@concertable/shared` tests 6/6,
  `@concertable/b2b` tests 17/17, and shared, web, customer, and B2B package builds all completed green.
- `git diff --check`: passed.
- `app/package.json`: JSON parse passed.
- New-file trailing-whitespace scan: passed.
- `app/web/b2b/shared/package.json` and `app/package-lock.json`: zero diff; consumers and runtime source
  are unchanged.

## Reviews

Not started. `/review` is the next delivery gate after the local Phase 1 commit.

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
