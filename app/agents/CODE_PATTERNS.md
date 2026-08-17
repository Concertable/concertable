# Frontend Code Patterns — Concertable's own precedents

The generic structural standard lives in the `tiered-shared-code`, `react-structure`, `client-state`,
`server-state`, `http-layer` and `write-boundary` skills. Sibling of
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md) (naming/style); this file is about **structure**, and carries
only what this repo adds.

The five sharing tiers and the build gate are in [`../AGENTS.md`](../AGENTS.md); each tier's own boundary
rules are in its own `AGENTS.md`. Read those first — this file assumes them.

## Identity: universal `User` in shared, `B2bIdentity` composed in the b2b `tenant` feature

The base intersection lives in `@concertable/shared` — `id`, `email`, `isEmailVerified`, universal profile
fields. No `venueId`/`artistId`, no `memberships`, no product subtypes.

`@b2b/*`'s `features/tenant` owns the B2B view: `B2bIdentity extends User` adding
`readonly memberships: ReadonlyArray<Membership>`, populated by a **B2B-owned, typed `/auth/me` query**
(`features/tenant/api/identityApi.ts`) returning what the B2B backend actually sends. B2B code reads
memberships from *that* module, never off the shared `User`, and never by casting a field the shared type
does not declare. `app/mobile/b2b` declares its own equivalent in `navigation/identity.ts`.

`@concertable/customer/*` composes its own buyer identity the same way if it ever needs more than the base.

This mirrors the backend split exactly: `ICurrentUser` in Kernel carries only `Id`/`Email`/`IsAuthenticated`,
and the tenant concept lives in a separate `ICurrentTenant` that only B2B depends on
([`api/AGENTS.md`](../../api/AGENTS.md)).

## The `tenant` feature owns both the active-tenant state and the imperative session

Because B2B identity is stateful domain data — which tenant is active, what the memberships are, whether a
choice is pending — that module is also the feature owning the reactive state; the two patterns land on one
module. `activeTenantId` lives in one private Zustand store (`features/tenant/store`); React consumers read
`useTenant`; route `beforeLoad` (`guards.ts`), request headers and logout share **one** internal
`tenantSession.ts`. Derivations are pure over explicit inputs — `memberships.ts`, `permissions.ts`.

## Permissions come from `useTenant(tenantType).permissions`

One stable `ReadonlySet<TenantPermission>` per `TenantRole` in `features/tenant/permissions.ts`, consumed
with native `.has(permission)`. The 13 `TenantPermission` literals mirror the backend
`Concertable.B2B.Tenant.Contracts.SharedPermissions` constant **names** one-for-one — model the **full**
set, because a partial catalog silently desyncs the day the backend matrix changes. The frontend gate is
cosmetic; the server enforces.

## Mount-only Effects go through `useMountEffect`

[`app/shared/src/hooks/useMountEffect.ts`](../shared/src/hooks/useMountEffect.ts) — for the narrow case the
`react-structure` skill sanctions (syncing with something outside React), not as a way to fetch.
