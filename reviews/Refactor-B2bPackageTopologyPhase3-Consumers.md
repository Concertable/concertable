# Code review — Refactor/B2bPackageTopologyPhase3-Consumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `1c1ba53d61244c6f8feb6e4a0852ce3aa19c71d0`  `(2026-09-09)`
**Security-reviewed up to commit:** `1c1ba53d61244c6f8feb6e4a0852ce3aa19c71d0`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-09 — full

**Candidate base:** `2df4105989ce462644cc9bc764138aeb0ecc958d`
**Candidate head:** `0a48ab4fcd98177150417e514430e6d1ed0a676c`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Consumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:88db54e58bdcb284a7fcc2dbb9ff2db9fcf5fb00c56bb828f327c74673440d06` `(80 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Findings

- [x] **1 — MEDIUM — the mobile profile screens loaded forever without a tenant.** Producer finding C1
  asked this stage to confirm that nothing renders `useMyArtist`/`useMyVenue` before a tenant resolves,
  since those hooks now report `isLoading: true` while their query is disabled
  (`useMyArtist.ts:139`, `isLoading: tenantId === undefined || query.isLoading`). On web the
  `requireArtist`/`requireVenue` guards cover it. On mobile `AuthenticatedNavigator` covers the
  authenticated paths correctly — loading, load failure, zero memberships and unresolved selection each
  return before the tabs, and `VenueTabs`/`ArtistTabs` mount only inside `ActiveTenantProvider` with a
  resolved membership. **The unauthenticated path did not:** `RootNavigator` renders `ArtistTabs`
  outside `ActiveTenantProvider` when `user === undefined`, `ArtistTabs` carries a real `MyArtistTab`,
  and `useActiveTenantId()` there is `undefined` — so tapping My Artist rendered its skeleton
  permanently, with no error and no sign-in prompt. Previously the screen reached the API and showed an
  error state, so this candidate introduced it. Fixed: both screens now state the requirement instead of
  relying on a loading flag they cannot satisfy. Venue is not reachable that way today and carries the
  same guard rather than depending on the navigator shape holding.

### Checked and clear

- **Producer finding C2 is pre-existing and correctly out of scope.**
  `app/web/b2b/{artist,venue}/src/features/{artist,venue}/guards.ts` fetch outside React Query from
  `beforeLoad`, which the `server-state` standard forbids, and cast with `(e as any)`. The diff only
  repoints the import from `@concertable/shared/...` to `@concertable/b2b/features/{artists,venues}` and
  renames `getMyArtist`/`getMyVenue` to `getArtist`/`getVenue`; both smells pre-date this branch on
  `main`. Recorded rather than folded into an ownership cutover.
- **The guard cannot fire before the tenant resolves.** `getArtist()` takes no tenant argument and
  depends on the ambient header, so ordering matters. `_artist/route.tsx` awaits
  `resolveTenantRoute("artist")` — which itself awaits `tenantSessionReady` then
  `tenantSession.resolve` — and only then calls `requireArtist`, sequentially in the same `beforeLoad`.
- **The contraction is complete and one-directional.** The universal-shared active-profile
  implementation and the manager-web tenant core are deleted, not duplicated; web retains only the
  platform adapter `webTenantSession.ts`, and mobile gains its own `tenantStorage`/context edge.
- **`app/web/TECH_DEBT.md` was updated with the move** rather than left pointing at the deleted
  `useTenantStore`, so the storage-classification entry still names a real writer.

### Security

The diff moves tenant selection and the request header, so it was reviewed for tenant isolation.

- **The trust boundary is unchanged.** `X-Tenant-Id` was a client-supplied header before this change and
  still is; server-side authorization of it is untouched by this diff and remains the actual control.
- **Both clients stay scoped.** `b2bClient.ts` attaches `TENANT_HEADER` to `apiClient` and
  `paymentClient`, so neither surface silently loses scoping in the move.
- **A tampered selection is filtered before it is sent.** `tenantSession.tenantIdForRequest` returns the
  stored id only when `memberships().some((m) => m.tenantId === activeTenantId)`, so a `localStorage`
  value naming a tenant the user does not belong to is never attached.
- **Persisted state is parsed defensively.** `loadActiveTenantId` wraps `JSON.parse` in try/catch and
  type-checks `activeTenantId` before returning it, and keeps the previous zustand `persist` envelope so
  an existing selection survives rather than silently resetting.
- **Sign-out clears both.** `addUserUnloaded` clears the tenant session, and `clearMemberships` drops
  the cached identity query, so neither the selection nor another tenant's memberships outlive a session
  on a shared browser.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `1b50d34333a20c3f84ae32e8de4cbe8256d02c16`
**Candidate head:** `1c1ba53d61244c6f8feb6e4a0852ce3aa19c71d0`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Consumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:fd9df82493e989672f13769067b6abafb62a545b002121f58fab4f18605f0303` `(7 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. Generated lockfiles only, from `npm run lock:carve`.

All seven standalone lockfiles resolve every `@concertable/*` tier to exactly `0.1.0-alpha.0.6503`, so
`carve-fe.test.mjs`'s one-lockstep-version assertion holds and no surface is left on the superseded
`0.1.0-alpha.0.6462` or `0.1.0-alpha.0.6498`. The venue, artist and mobile B2B locks now contain
`@concertable/b2b`, which they declare and previously did not resolve. No source, manifest or committed
specifier changed: the declarations stay `alpha` and the pin lives only in the lockfiles.
