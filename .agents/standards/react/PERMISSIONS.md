# Permissions — one set per role, read through `useTenant`

Permissions come from `useTenant(tenantType).permissions`: one stable `ReadonlySet<TenantPermission>` per
`TenantRole` in `features/tenant/permissions.ts`, consumed with native `.has(permission)`.

## Model the full set, never a partial catalog

The 13 `TenantPermission` literals mirror the backend
`Concertable.B2B.Tenant.Contracts.SharedPermissions` constant **names** one for one. Model the **full** set:
a partial catalog silently desyncs the day the backend matrix changes, and nothing fails until a user is
wrongly allowed or denied.

## The frontend gate is cosmetic

The server enforces. A permission check here decides what to render, never what is permitted — so a missing
check is a UI bug, and a *relied-upon* check is a security bug.
