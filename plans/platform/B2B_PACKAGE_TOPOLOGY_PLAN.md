# B2B package topology cutover

Next steps live in
[`B2B_PACKAGE_TOPOLOGY_PROGRESS.md`](B2B_PACKAGE_TOPOLOGY_PROGRESS.md) -> `## Next Steps`.

## Objective

Separate the two responsibilities currently hidden behind `@concertable/b2b` without a publication
gap:

- `@concertable/web-b2b` owns the existing manager-web shared tier from `app/web/b2b/shared`.
- `@concertable/b2b` remains a first-class package and becomes the cross-platform B2B core, with
  additive artist, venue, and tenant-core exports shared by web and mobile.

This is a package-ownership cutover. It preserves the established web active-profile and tenant
semantics, makes them the cross-platform contract, and repairs mobile's missing active-tenant
selection and request-header wiring while moving ownership to the tier every legitimate consumer can
run.

## Delivery and implementation DAGs

| Phase | Implementation | Delivery gate |
|---|---|---|
| 1. Expand the web identity | Publish the existing built `app/web/b2b/shared` package unchanged under both its current `@concertable/b2b` name and the additive `@concertable/web-b2b` alias. Current manifests, imports, consumers, and runtime source stay unchanged. | Merge the additive producer PR and prove the same new version installs and verifies under both names from GitHub Packages. |
| 2. Establish both owner packages | Rename the existing web source package to `@concertable/web-b2b`, mechanically repoint manager-web-only imports and build/package tooling, and establish the retained cross-platform `@concertable/b2b` package with additive artist, venue, and tenant-core exports. | Start only after Phase 1's exact `@concertable/web-b2b` version is visible on the feed. Both packages must build, pack, feed-restore, and carve without relying on the former identity collision. |
| 3. Cut consumers over and contract duplicates | Migrate web and mobile active-profile and tenant consumers to the cross-platform `@concertable/b2b` APIs, remove the superseded universal-shared profile code and duplicated web tenant core, remove the temporary alias bridge, and finish the downstream organization-profile route-contraction integration. | Start only after Phase 2 has published and feed-verified both first-class packages. Every owned consumer must pass the focused web/mobile/package/carve gates and the old duplicate implementations must grep to zero before closeout. |

Phase 2 may be prepared against the exact Phase 1 tarball but is delivery-gated until that artifact is
published. Phase 3 may be prepared against exact Phase 2 artifacts but is merge-gated until both
first-class package identities are available from the feed. Every intermediate merge stays green.

## Phase 1 - expand the web package identity

- Add one reusable script that stages the existing package files with only the manifest `name` changed,
  packs with lifecycle scripts disabled, and never writes into the source package.
- Unit-test that the alias installs with the original version, files, and manifest metadata while the
  source manifest remains unchanged.
- Extend frontend publication to pack, locally verify, idempotently publish, and feed-verify
  `@concertable/web-b2b` after the normal `@concertable/b2b` package has built.
- Keep `app/web/b2b/shared/package.json`, all workspace dependencies, imports, lockfile entries, and
  runtime source unchanged.

Gate: alias-packer unit test, ordered web-package build/tests, workflow syntax, and `git diff --check`
pass. Exact-head PR CI owns the complete frontend package, boundary, and carve matrices.

## Phase 2 - establish the web and cross-platform packages

- Make `app/web/b2b/shared` the first-class `@concertable/web-b2b` package and migrate every
  manager-web-only dependency/import/build reference that must follow it.
- Establish the retained `@concertable/b2b` package at the cross-platform tier with additive artist,
  venue, and tenant-core exports. The tenant core owns membership resolution, a private store, and a
  platform-configured session/storage boundary; it must not import React DOM, browser-only routing,
  manager-web UI, or platform storage.
- Update the workspace lockfile, package build order, publication verification, carve inputs, and
  boundary declarations so both identities are explicit and independently restorable.
- Keep consumer behaviour unchanged; this phase establishes ownership and additive APIs before the
  broad consumer cutover.

Gate: tier builds/tests, all four web builds, both mobile typechecks/exports, boundary tests, local pack
verification, feed-restored carves, and exact-head PR CI pass for both packages.

## Phase 3 - migrate consumers and contract duplicated ownership

- Move web and mobile artist/venue active-profile consumers to the cross-platform `@concertable/b2b`
  exports and keep the same active-tenant lookup, cache, mutation, and onboarding behaviour on both
  platforms. Platform-specific rendering does not justify divergent client semantics.
- Move tenant-core consumers to `@concertable/b2b`, then remove the duplicated manager-web tenant core.
- Configure mobile B2B with the same active-membership semantics as web: select across all artist and
  venue memberships, attach the selected tenant through `X-Tenant-Id`, clear it on logout, and expose
  chooser/switcher UI at the mobile application edge.
- Remove the superseded artist/venue active-profile implementation from universal shared once every
  web and mobile consumer resolves the cross-platform owner.
- Remove the temporary Phase 1 alias bridge after `@concertable/web-b2b` is the normal published web
  package and the retained `@concertable/b2b` is the normal published cross-platform package.
- Reconcile and finish the downstream organization-profile route-contraction integration against the
  published package topology; do not recreate compatibility APIs in the route branch.

Gate: zero unintended imports of the removed universal/web duplicates, ordered package build/tests,
all four web builds, both mobile typechecks/exports, boundary tests, feed-restored carves, the focused
organization-profile integration tests, and exact-head PR CI all pass.

## Non-goals

- Removing or deprecating the `@concertable/b2b` package identity.
- Changing active-profile HTTP or onboarding semantics. Mobile tenant selection is intentionally
  repaired to match the already-supported web product semantics.
- Moving manager-web components into the cross-platform package.
- Changing backend B2B service/package identities.
