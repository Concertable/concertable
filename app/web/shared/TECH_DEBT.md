# app/web/shared technical debt

## MED

### `useSyncUser` copies TanStack Query data into `useAuthStore` — the exact anti-pattern `CODE_PATTERNS.md` already warns against

`features/user/hooks/useSyncUser.ts` runs a `useQuery({ queryKey: meQueryKey, queryFn: getMe })` and
then, in a `useEffect`, copies its `data` into a separate Zustand store (`useAuthStore`, defined one
tier up in `@concertable/shared`). Every consumer that wants the current user then has a choice of two
reactive sources for the same fetch — `useQuery(meQueryKey)` directly, or `useAuthStore((s) => s.user)`
— kept in sync only by that effect.

This is the literal shape `app/agents/CODE_PATTERNS.md` ("A domain's reactive state has explicit
homes") already calls out as the violation: *"Server data copied into a store... snapshots cache data
into global state, which breaks background refetch."* That section's example is
`useConcertStore.draft`; `useAuthStore` is the same anti-pattern in the auth feature, just not yet
flagged. TanStack Query already provides everything the store adds (reactivity, caching, dedup) — the
store is a second cache for identical data, not a distinct capability.

Confirmed consumers of `useAuthStore` today: `ProfileMenu.tsx`, `Navbar.tsx`, `ProfilePage.tsx`,
`features/auth/guards.ts` (writes it from `ensureUser`), and B2B artist's `useApply.ts`. None of them
need anything `useQuery(meQueryKey)` can't give them directly.

Found while scaffolding the admin console SPA (`app/web/admin`): its route guard and header
deliberately do **not** use `useSyncUser`/`useAuthStore` — the guard reads `/auth/me` once via
`queryClient.ensureQueryData`, and the header reads `auth.user?.profile.email` straight from
`react-oidc-context` — so this entry documents pre-existing debt in `@concertable/web`, not something
the admin app added to.

**Resolves when:** delete `useAuthStore`/`useSyncUser`, and have `ProfileMenu`/`Navbar`/`ProfilePage`/
`useApply` read `useQuery({ queryKey: meQueryKey, queryFn: <the app's own getMe> })` directly (each
already knows its own `getMe`, same as `useTenant` in `@b2b/shared` already does for `B2bIdentity`).
The one thing `useSyncUser` currently does that a bare `useQuery` doesn't is gate `enabled` on
`isAuthenticated`/`isLoading` from `useAuth()` — fold that into each call site or a small shared
`useMeQuery()` wrapper that takes `getMe` as a parameter, without a Zustand mirror behind it.
