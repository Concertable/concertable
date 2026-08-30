---
name: concertable-react-client-state
description: Concertable's `tenant` feature owns both the active-tenant state and the imperative session, because B2B identity is stateful domain data so the domain module and the state module are the same one — `activeTenantId` in one private Zustand store, React consumers reading `useTenant`, and route guards plus request headers plus logout sharing one internal `tenantSession.ts` as the single sanctioned imperative object, with derivations pure over explicit inputs. Use when touching active-tenant state, needing tenant state outside React, or adding a getter or setter wrapper around the session.
---

# Client state — the `tenant` feature owns the active tenant and the session

The generic standard is the `client-state` skill: a store is a private implementation detail consumed through
a facade hook, every transition is a named action, derived values are computed from explicit inputs, and there
is one deliberate imperative object for non-React consumers.

Here those two patterns land on **one** module, because B2B identity is stateful domain data — which tenant is
active, what the memberships are, whether a choice is pending — so the feature that owns the domain concept is
also the feature that owns the reactive state.

- `activeTenantId` lives in one private Zustand store, `features/tenant/store`.
- React consumers read `useTenant`.
- Route `beforeLoad` (`guards.ts`), request headers and logout share **one** internal `tenantSession.ts` —
  the single sanctioned imperative object, not a family of getter/setter wrappers.
- Derivations are pure over explicit inputs: `memberships.ts`, `permissions.ts`.

The permission half is [`PERMISSIONS.md`](../../standards/react/PERMISSIONS.md); the identity shape is
[`IDENTITY.md`](../../standards/react/IDENTITY.md).
