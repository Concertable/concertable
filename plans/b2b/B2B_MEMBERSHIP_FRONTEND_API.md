# B2B Membership Frontend API

## Goal

Refactor the B2B tenant and membership frontend boundary into a cohesive plain-data API. TanStack Query continues to hold the `/auth/me` JSON response, Zustand continues to hold `activeTenantId`, membership selection stays pure, and React consumers use persona-oriented hooks instead of interpreting roles or query-cache data themselves.

This is one frontend PR on `Refactor/B2BMembershipFrontendApi`. The implementation starts only after `Feature/RetireRoleClaim` has merged, because that branch introduces the membership-derived `requireBusinessPersona` route guard that this sweep must retain and reorganize. Fetch `origin`, bring this branch up to date with `origin/main`, and confirm the role-claim commit is present before Phase 1.

## Findings

- `app/web/b2b/shared/src/features/tenant/model.ts` currently mixes pure membership selection, React hooks, TanStack Query observation, singleton cache reads, Zustand writes, router invalidation, tenant reconciliation, and permission derivation.
- `useMemberships()` currently means every B2B membership, while its only consumers immediately filter by persona. No current React consumer needs every membership across both personas, so the new public API does not need `useAllMemberships`.
- `MembersPage` is the only permission consumer. It calls `useHasPermission` three times, which repeats active-membership/query/store composition and exposes one boolean hook per permission check.
- `useSyncUser(identityApi.getMe)` in each B2B root is intended to own the `['auth', 'me']` fetch. The tenant reader attaches another `identityApi.getMe` query function to the same key, and route guards can populate that key first through the universal `userApi.getMe`. The functions hit the same endpoint, but the ownership and result typing are ambiguous.
- The role-claim branch replaces `requireBusinessRole` with `requireBusinessPersona`, but places that guard and its manual cache interpretation back into `model.ts`. The guard is the correct use case; its dependencies need explicit boundaries.
- `getTenantChoicePending` and `reconcileActiveTenant` are the imperative route API. Their current behavior is deliberate: filter to the requested persona, clear a persisted selection that is not among that persona's memberships, require a choice only when that persona has several memberships, and preserve the single-membership fallback.
- Tenant selection currently writes Zustand, invalidates the router, and invalidates all queries. Invitation acceptance writes the accepted tenant before a full navigation. Logout clears the persisted tenant. These behaviors stay unchanged.
- `Membership`, `B2bIdentity.memberships`, and hook results are mutable at their TypeScript boundaries even though consumers treat the response as immutable JSON.

## Target shape

### Plain data and pure selection

- Keep `Membership` and `B2bIdentity` as interfaces. Make membership fields readonly and expose `memberships` as `ReadonlyArray<Membership>`.
- Replace the pure portion of `model.ts` with a focused membership module containing persona filtering, active-membership resolution, and tenant-choice detection. Every function accepts `ReadonlyArray<Membership>` and has no React, TanStack Query, Zustand, router, API-client, or singleton imports.
- Keep the existing semantics: `resolveActiveMembership` first matches `activeTenantId` within the requested persona and falls back only when that persona has exactly one membership.
- Keep the role-to-permission catalog as module-level readonly sets. Expose a pure role-to-set function internally, use the native `ReadonlySet.has` consumer API, and return one shared empty readonly set when there is no active membership. No set is stored in TanStack Query data.

### Reactive consumer API

The tenant feature barrel exposes this component API:

```ts
const memberships = useMemberships(persona);
const activeMembership = useActiveMembership(persona);
const permissions = usePermissions(persona);

permissions.has("MembersInvite");
```

- `useMemberships(persona)` subscribes to the existing `meQueryKey` as a disabled reader and filters the cached identity through the pure membership function. It does not provide a query function and never initiates `/auth/me`.
- Do not add `useAllMemberships` unless an implementation-time sweep discovers a real all-personas React consumer.
- `useActiveMembership(persona)` composes `useMemberships(persona)` with the Zustand `activeTenantId`.
- `usePermissions(persona)` derives the stable role set from `useActiveMembership(persona)` and returns the shared empty set when absent.
- `useTenantChoicePending(persona)` and `useSelectTenant()` remain use-case hooks. `TenantChooser` and `TenantSwitcher` consume `useMemberships(persona)` directly.
- `MembersPage` calls `usePermissions(persona)` once and uses `.has(...)` for invite, role-management, and removal affordances.

### Identity and imperative boundaries

- Add a B2B root synchronization hook that delegates to the shared `useSyncUser` with `identityApi.getMe`. The venue and artist roots use that named hook instead of exporting the raw identity API through the feature barrel.
- Make the shared business-auth guard accept the B2B identity fetcher/query definition used by the root. `requireBusinessPersona` ensures the B2B identity through that same definition before reading memberships, so `['auth', 'me']` has one B2B query function and one JSON shape.
- The disabled membership observer uses no query function. It only subscribes to the root/guard-owned query.
- Put singleton query-cache access in one identity-cache module. Expose use-case functions to the guard, choice check, and reconciliation code; route files never call `queryClient.getQueryData` or interpret `B2bIdentity` themselves.
- Keep `requireBusinessPersona(persona)` in a guard module, tenant-choice cache inspection in the selection boundary, and side-effectful stale-selection reconciliation in the active-tenant boundary. Rename the imperative choice predicate to `isTenantChoicePending(persona)` alongside `useTenantChoicePending(persona)`.
- Keep the Zustand store internal to the tenant feature. Expose narrowly named active-tenant reads/writes needed by B2B client setup, invitation acceptance, logout cleanup, selection, and reconciliation instead of exporting the store from the public barrel.
- Delete `model.ts`, replace `tenantPermissions.ts` with the focused permission module, remove the obsolete `useSamePersonaMemberships`, `useHasPermission`, `hasPermission`, `getTenantChoicePending`, raw `identityApi`, and raw store barrel exports, and remove existing phase/design-narration comments touched by the refactor.

### Intended module boundaries

| Module | Responsibility |
| --- | --- |
| `types.ts` / `constants.ts` | Readonly JSON-facing identity, membership, persona, role, and header types/constants |
| `memberships.ts` | Pure persona filtering, active membership resolution, and choice detection |
| `permissions.ts` | Permission type, stable role sets, shared empty set, and pure role lookup |
| `hooks/useMemberships.ts` | Disabled `meQueryKey` observer plus `useMemberships`, `useActiveMembership`, and `usePermissions` |
| `hooks/useTenantSelection.ts` | Reactive choice state and router/query invalidating selection callback |
| `identity.ts` | B2B identity query function and root `useSyncB2bIdentity` ownership hook |
| `identityCache.ts` | The only singleton TanStack Query cache reader for B2B identity/memberships |
| `activeTenant.ts` | Narrow Zustand reads/writes and side-effectful stale-selection reconciliation |
| `guards.ts` | `requireBusinessPersona`, composed from shared business authentication and cached B2B memberships |

The final barrel exports the readonly types and role list; the three primary membership/permission hooks; tenant-choice/select hooks; `isTenantChoicePending`, `reconcileActiveTenant`, and `requireBusinessPersona`; the chooser/switcher components; the root identity-sync hook; and the narrow active-tenant actions required across B2B features. It does not export a raw API object, query client, cache shape interpreter, Zustand store, or role-plus-permission predicate.

## Phase 1 — Pure membership/permission model and React API

1. Add the pure membership selectors and focused permission catalog with readonly inputs and stable set instances.
2. Add the reader-only membership observer and the `useMemberships`, `useActiveMembership`, `usePermissions`, `useTenantChoicePending`, and `useSelectTenant` hooks.
3. Migrate `TenantChooser`, `TenantSwitcher`, and `MembersPage` to the new API.
4. Keep the existing imperative route exports temporarily so both manager apps remain shippable at the end of the phase.
5. Verify all four web builds, `dotnet build api/Concertable.slnx`, and the affected B2B Tenant unit/integration tests through the `integration-debug` workflow. Commit the phase with this plan checked off.

## Phase 2 — Query ownership, guards, reconciliation, and public-boundary cleanup

1. Add the named B2B identity-sync hook and make the shared business-auth guard accept the B2B identity query function/definition.
2. Move cache reading, `requireBusinessPersona`, tenant-choice inspection, and stale active-tenant reconciliation into their focused modules; migrate both manager route trees.
3. Hide the Zustand store behind active-tenant actions/readers and migrate B2B client setup, invitation acceptance, tenant selection, reconciliation, and logout cleanup.
4. Remove `model.ts` and every obsolete barrel export and import. Run a repository-wide grep for `useSamePersonaMemberships`, `useHasPermission`, direct component `hasPermission`, `getTenantChoicePending`, raw B2B `identityApi` imports, public `useActiveTenantStore` imports, and manual `meQueryKey` cache interpretation; only the intentional shared key owner and focused identity-cache module may remain.
5. Verify all four web builds, `dotnet build api/Concertable.slnx`, and the affected B2B Tenant unit/integration tests through the `integration-debug` workflow. Do not run UI E2E locally before the PR: this broad guard/cache refactor should go through the merge queue without a `skip-e2e` label so the existing invitation and tenant-switching scenarios validate it.
6. Delete this plan in the completing commit, update `plans/b2b/LAUNCH_PLAN.md` only if the implementation changes the recorded Phase 7 state, then hand the PR to `/code-review` before merge.

## Definition of done

- Public component code uses `useMemberships(persona)`, `useActiveMembership(persona)`, and `usePermissions(persona)` with native set membership checks.
- No Membership/Memberships class, Array subclass/prototype augmentation, hydration layer, permission-derived membership model, or permission-per-hook API exists.
- `/auth/me` remains plain JSON in TanStack Query, has one B2B query function, and is fetched only by the root sync/route guard ownership path.
- `activeTenantId` remains persisted in Zustand; selection, single-membership fallback, chooser behavior, stale-selection clearing, invitation activation, request headers, and logout clearing retain their current behavior.
- Pure membership and permission functions have no UI/framework/singleton imports, and imperative cache readers are confined to the named tenant feature boundary.
- Venue, artist, customer, and business builds pass; the API solution and affected Tenant tests pass; merge-queue UI E2E passes.
