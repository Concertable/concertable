# B2B authorization module — roadmap

Give `Concertable.B2B` a real `Concertable.B2B.Authorization` module. Today's authorization code is homed by
accident of which module happened to own the underlying data — `PermissionAuthorizationHandler` /
`PermissionRequirement` / `IMembershipContext` live in `Tenant.Infrastructure`/`.Contracts` because Tenant
owns membership rows, and the newer `ManagerClients` (registration-time role classification, from the
auth-identity-model plan) landed in `Concertable.B2B.Infrastructure/Authorization/` for the same reason —
so every module with a protected endpoint depends on Tenant.Contracts for an orthogonal concern. Full
detail and the resolution condition: [`api/Concertable.B2B/TECH_DEBT.md`](../../api/Concertable.B2B/TECH_DEBT.md)
("Authorization code is homed by accident-of-ownership, not by a real module").

## Items

- [ ] `b2b-authorization/authorization-module` — add `Concertable.B2B.Authorization` (`.Contracts`:
  `PermissionRequirement`, a membership/role-fact facade interface Tenant implements; `.Infrastructure`:
  `PermissionAuthorizationHandler`, `ManagerClients`; no `.Domain`/`.Api`, no persisted aggregate of its
  own), invert every B2B module's policy registration to depend on it instead of on Tenant, and move
  `PermissionAuthorizationHandler`/`ManagerClients` into it. Materially bigger than a single feature PR —
  rewires every module's policy registration — needs its own worktree/branch per the worktree identity
  gate.
