# Code review — Refactor/B2bPackageTopologyPhase3-Consumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `62f3a3493e830435cf48eac6845541019451494b`  `(2026-09-09)`
**Security-reviewed up to commit:** `62f3a3493e830435cf48eac6845541019451494b`  `(2026-09-09)`
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

## Review pass — 2026-09-09 — incremental

**Candidate base:** `33c1d6797c3dcfadf5b63c568a4885a0244641a5`
**Candidate head:** `d9958fd079f7161ecadee40714a3c93e727b7d79`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Consumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:560d66d0a4fc5d6371b9888a42eb27a8a91e3e9191626298e7bceff4a7e8e3c4` `(6 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

- [x] **2 — HIGH — an escrow observed in `Processing` could never be authorized.** `Processing` was the
  only non-terminal state without an `Authorize` edge, so a manual-capture PaymentIntent sampled in
  `processing` before `requires_capture` was refused with `IllegalTransition from Processing to
  Authorized` and its capture never ran. Pre-existing on `main`; surfaced here because this queue run was
  the first to sample that ordering. `PROVIDER_CONTRACT.md` already sanctions both legs of the sequence,
  so the edge was missing rather than withheld. Added, declared in `SessionEdges`, and covered by
  `Evaluate_AuthorizedAfterProcessing_IsApplied`.
- [x] **3 — HIGH — invitation acceptance deadlocked on its own query.** `useAcceptInvitation` ran its POST
  inside a `useQuery` and selected the tenant through `useTenant`, whose `selectTenant` awaits an
  unfiltered `queryClient.invalidateQueries()`. From inside that `queryFn` the await waited on a refetch
  of the same query, so the page held its `accept-pending` spinner indefinitely and `members-roster`
  never rendered — three scenarios failed on it. Introduced by this candidate: the previous code left both
  invalidations unawaited. Fixed at the call site, which refreshes memberships and calls
  `tenantSession.select` directly; it finishes with `window.location.assign`, so the SPA cache
  invalidation it deadlocked on was redundant there. `selectTenant` keeps its awaited invalidation
  because the in-SPA tenant switcher depends on it for a deterministic refresh, and weakening that would
  have traded this failure for a stale read in
  `Switching organization scopes member management to the chosen tenant`.

### Security

`Processing -> Authorized` widens no authorization: `ResolveAuthorization` still requires an
`Authorization` session kind, and `PaymentSessionStateMachine.Evaluate` still rejects `Authorized` for a
`Payment` context, which the retained `Evaluate_AuthorizedForAutomaticPayment_IsRejected` covers. The
accept flow performs the same membership refetch and the same `tenantSession.select`, which still filters
a tenant absent from the caller's memberships, so no tenant becomes selectable that was not before.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `b693f15a7bb113ef41d682b680b2f3be81ebe390`
**Candidate head:** `62f3a3493e830435cf48eac6845541019451494b`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Consumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:be3ae868011df06267521d7c6b2dfc4945524eb49e9e7270bb9e1cc71118bf22` `(4 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Consumers.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

- [x] **4 — HIGH — a published signature was narrowed inside the consumer stage.** Finding 3's fix removed
  `tenantType` from `AcceptInvitationPage` and `useAcceptInvitation`, both `@concertable/web-b2b` exports,
  and updated the two SPA route call sites. The carved SPAs restore that package from the feed, where the
  parameter is still required, so `carve-fe (web/b2b/venue)` and `carve-fe (web/b2b/artist)` failed with
  `TS2741: Property 'tenantType' is missing`. Only a republish could have changed that shape, and this is
  the consumer stage. This is the same publish-before-consume boundary that `carve-fe (mobile/b2b)`
  enforced earlier on `queryClient`; the in-monorepo typecheck cannot see it because the workspace
  resolves the local source. The exported signature and both call sites are restored, and finding 3's fix
  is confined to the hook body.
