# Tenant Invitations & Member Management — USER_MODEL_PLAN Phase 6 (concrete)

> **Parent:** [`USER_MODEL_PLAN.md`](./USER_MODEL_PLAN.md) §6 + §8-Phase-6. This is the concrete,
> code-grounded build plan for that phase; the parent stays the master tracker. Launch tracker line
> it advances: [`LAUNCH_PLAN.md`](./LAUNCH_PLAN.md) *"Finish Swim-lane B — membership/invitation
> endpoints + auth sweep + messaging group-inbox"* (Phase 6 = the membership/invitation slice; 7-8
> are separate).
>
> **Branch:** `Feature/TenantInvitations` (worktree, off fresh `master`). **Delete this file** in the
> commit that lands the last sub-phase (6.4).
>
> Grounded in a 4-track investigation of fresh master (2026-07-19). Phases 1-5 of USER_MODEL_PLAN are
> all shipped and verified in this checkout. What follows is **new** unless flagged "exists".

---

## 0. What already exists (don't rebuild) vs what's new

**Already shipped — reuse, don't touch:**
- **All permission constants + the role→permission map.** `SharedPermissions` (`Tenant.Contracts/SharedPermissions.cs`)
  already declares `MembersInvite`, `MembersRemove`, `MembersManageRoles`, `TenantSettingsEdit`,
  `TenantDelete`, `OperationsView`, and the `ByRole` `FrozenDictionary` already grants them correctly:
  **Owner** gets all five member/tenant-admin perms; **Manager** gets `MembersInvite` only; Finance/Staff/Door/Sound
  get none. **No catalog edits needed** — and `PermissionCatalogTests` already guards coverage.
- **The auth pipeline.** `[HasPermission(string)]` + `PermissionPolicyProvider` + `PermissionAuthorizationHandler`
  + `TenantResolutionMiddleware` + `IMembershipContext.HasPermission(perm, requiredPersona?)`. Member
  management is **persona-agnostic**, so gate the controller exactly like `StripeAccountController`
  (class-level `[HasPermission(...)]`, **no** `[TenantPersona]`).
- **Membership read surface.** `MembershipDto(TenantId, LegalName, Type, Role)` + `ITenantModule.GetMembershipsAsync(userId)`
  + `TenantService.GetMembershipsAsync` + `TenantRepository.GetMembershipsAsync`. `GET /api/auth/me`
  (`User.Api/UserController.Me()`) **already returns these memberships** on `UserBase.Memberships`.
- **The `TenantMembershipEntity` + `ChangeRole()`** — the mutation seam is there; its own comment defers
  the last-Owner invariant to the service layer.
- **Frontend active-tenant plumbing (half-built).** `useActiveTenantStore` (zustand+persist, key
  `concertable.active-tenant`), `TENANT_HEADER = "X-Tenant-Id"`, and the `b2bAxios.ts` request
  interceptor that stamps the header **all exist and are wired** — waiting on the switcher UI to call
  `setActiveTenant`. Comments literally say *"Written by the tenant switcher (Phase 6)."*
- **Email + link infra.** `IEmailSender.SendEmailAsync(to, subject, htmlBody)` is already in the B2B DI
  container (raw HTML string, no templating). `IUriService`/`UrlSettings.Frontend` (config section
  `Urls`) is the SPA-link builder. `MockEmailSender.Sent` captures sends for integration assertions.

**New in this phase:** `InvitationStatus` enum, `TenantInvitationEntity` + EF config + migration, the
member/invitation DTOs, the members/invitations controllers + service + repo methods (incl. the
**last-Owner invariant**), the invitation email, the `TenantProvisioningHandler` **invitation-first
branch**, and the frontend switcher + members/invite/accept pages.

---

## 1. Corrections to the parent plan's prose (found in code)

Two places where USER_MODEL_PLAN §6 is **wrong against master** — follow this file, not it:

1. **"the seed insert already published `TenantCreatedEvent`, so re-publishing would double-provision
   Payment"** — **false on master.** The dev/E2E seeders *suppress* the create event (`tenant.ClearDomainEvents()`
   before insert), precisely because publishing at seed time races the Payment ASB subscription. So
   `TenantProvisioningHandler`'s existing-tenant branch **deliberately** calls `tenant.Announce()` to
   re-raise it — that `Announce()` is the *single reliable* `TenantCreatedEvent` publish. **Do not remove
   it.** No-double-provision comes from **inbox dedup on the `CredentialRegisteredEvent` MessageId**, not
   from skipping `Announce()`. "No re-publish" applies **only** to the *new invitation branch* (it adds
   members to the inviter's already-live tenant — never `Create`/`Announce` for it).

2. **Email is not normalized upstream.** `CredentialRegisteredEvent(UserId, Email, ClientId)` carries the
   email verbatim (no lower-case/trim anywhere in Auth). So the handler must normalize `e.Email` itself
   (`.Trim().ToLowerInvariant()`) before matching, and the invite endpoint must store
   `TenantInvitationEntity.Email` normalized the same way. The `(TenantId, Email)` unique index assumes
   normalized storage.

---

## 2. Design decisions (recommended; flag any veto before 6.2)

| # | Decision | Recommendation | Why |
|---|---|---|---|
| D1 | **Route base** — parent sketched `api/tenants/...`; existing controller is `api/organizations`. | Keep **`api/organizations`** for org-scoped endpoints (members, invitation-management, delete-current). | One tenant surface, not two prefixes; matches the shipped `TenantController`. |
| D2 | **`GET /api/tenants/mine`** (parent §6). | **Skip it.** The switcher reads memberships from `/api/auth/me` (already served) via the auth store — no second endpoint, no duplicate query. | Single source; the investigation confirmed FE needs no extra fetch. |
| D3 | **Accept route.** | Top-level **`POST /api/invitations/{id}/accept`**, `[Authorize]`-only — **not** under `api/organizations` (accept needs no active tenant; it's addressed by invitation id + caller email). | Accepting user may have no active/any membership yet; org-scoping would fail-closed 403. |
| D4 | **Members-list emails.** Membership stores only `UserId`; the list must show emails. | Resolve via the **intra-B2B `IUserModule`** facade — add a batch `GetEmailsByIdsAsync(ids)` (additive, same-service, not a published-package change). | Email lives in the User projection; don't denormalize onto membership. |
| D5 | **Service placement.** | New **`MembershipService`/`InvitationService`** (Application interface + Infrastructure impl) beside `TenantService`, rather than bloating `TenantService`. Either is acceptable. | Keeps the last-Owner/invite logic cohesive. |
| D6 | **Accept-link host** (`Urls:Frontend`). It's set **only in the test fixture** today; real `B2B.Web/appsettings.json` has **no `Urls` section**, and there are 3 manager SPAs (venue/artist/business). | Add `Urls:Frontend` to B2B config; point the link at the **manager portal matching the invited tenant's persona** (venue vs artist). For v1 a single business-portal landing that routes post-login is acceptable if per-persona hosts aren't wired. | Link must reach a real SPA; persona picks which. |

> **D6 as-shipped + revised for 6.4.** 6.3 landed a single `Urls:Frontend = https://localhost:5177`.
> Verified against master: **5177 is the business app, which is a static landing page (no router, no
> OIDC, no API) — it cannot host an authenticated accept page.** So D6's "business-portal landing that
> routes post-login" is **not viable as written**; superseded by **D9** below.

### 2.1 — Decisions added / revised for 6.4 (recommended; flag any veto before starting 6.4)

| # | Decision | Recommendation | Why |
|---|---|---|---|
| D7 | **Where the accept page lands the user.** `POST /api/invitation/{id}/accept` returns **204** (no body) — the FE can't read which tenant it joined. | Make the accept endpoint **return the joined `MembershipDto`** (additive: controller returns `Ok(membership)` not `NoContent()`); the FE sets it active and lands on `/settings/members`. Fallback (no backend change): snapshot `/me` memberships, refetch after accept, select the one new `tenantId`. | Setting the active tenant + a sensible landing needs the joined tenant id; the return value is the honest source (vs. a diff). |
| D8 | **Switcher injection vs the customer boundary.** `AppLayout`/`Navbar` live in universal `web/shared` (the customer build compiles them) and must not import `@b2b/*`. | Add an optional slot prop (`headerSlot?: ReactNode`) to `AppLayout`→`Navbar`; the venue/artist `_venue`/`_artist` `route.tsx` (b2b files) pass `<TenantSwitcher/>`. Same app-injected-slot pattern as `OrganizationPage`. | Keeps the switcher (`@b2b`) out of the customer bundle — the four-build boundary gate. |
| D9 | **Accept-link host (revises D6).** The emitted host must be an auth-capable manager portal, not the static business gateway (5177). | Backend emits the link against the **invited tenant's persona portal** (`Urls:VenueFrontend`/`Urls:ArtistFrontend`, chosen from the inviting tenant's `TenantType` in `InvitationService` — ~5 lines, additive); venue + artist both host the accept route. Lighter fallback: repoint the single `Urls:Frontend` to venue-web (5175) and host it there only, routing onward after accept. | 5177 can't authenticate; the invited persona is known at invite time. |

---

## 3. Data model (new) — mirror `TenantMembershipEntity` exactly

**`InvitationStatus`** → `Tenant.Contracts/Enums/InvitationStatus.cs` (explicit 1-based, like the sibling enums):
```csharp
namespace Concertable.B2B.Tenant.Contracts.Enums;
public enum InvitationStatus { Pending = 1, Accepted = 2, Revoked = 3, Expired = 4 }
```

**`TenantInvitationEntity`** → `Tenant.Domain/Entities/TenantInvitationEntity.cs` (sealed, `IGuidEntity`,
private ctor, private setters, static `Create`, mutation methods; `CreatedAt`/`ExpiresAt` passed in, not
`DateTime.UtcNow`; **the Id is the accept token** carried in the emailed link):
```csharp
public sealed class TenantInvitationEntity : IGuidEntity
{
    private TenantInvitationEntity() { }
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = null!;   // normalized lower-case
    public TenantRole Role { get; private set; }
    public InvitationStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }       // CreatedAt + 7d
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    public static TenantInvitationEntity Create(Guid tenantId, string email, TenantRole role,
        Guid createdBy, DateTime at, TimeSpan ttl) => new() {
            Id = Guid.NewGuid(), TenantId = tenantId, Email = email, Role = role,
            Status = InvitationStatus.Pending, CreatedByUserId = createdBy,
            CreatedAt = at, ExpiresAt = at + ttl };

    public void Accept(Guid userId, DateTime at) { /* guard Pending + not expired → throw; set Accepted */ }
    public void Revoke() { /* guard Pending → throw; set Revoked */ }
}
```

**EF config** → `Tenant.Infrastructure/Data/Configurations/TenantInvitationEntityConfiguration.cs`
(`internal sealed`, `ToTable(Schema.Tables.Invitations, Schema.Name)`, explicit `.IsRequired()` per
column, nullable `AcceptedBy*` unconfigured):
```csharp
builder.ToTable(Schema.Tables.Invitations, Schema.Name);
builder.HasKey(i => i.Id);
builder.Property(i => i.TenantId).IsRequired();
builder.Property(i => i.Email).IsRequired();
builder.Property(i => i.Role).IsRequired();
builder.Property(i => i.Status).IsRequired();
builder.Property(i => i.CreatedByUserId).IsRequired();
builder.Property(i => i.CreatedAt).IsRequired();
builder.Property(i => i.ExpiresAt).IsRequired();
builder.HasIndex(i => new { i.TenantId, i.Email }).IsUnique().HasFilter(...Pending...); // one live invite per (tenant,email)
builder.HasIndex(i => i.Email);   // registration-match lookup in the handler
```
> Filtered-unique on `Status = Pending` (so a revoked/expired invite doesn't block a re-invite). If the
> filtered index is awkward under re-scaffold, enforce "one pending per (tenant,email)" in the service
> instead and keep the index non-unique — decide at 6.1.

**Wire-up (all in `Tenant.Infrastructure`):**
- `Schema.cs` → add `public const string Invitations = "Invitations";`.
- `TenantDbContext` → add `public DbSet<TenantInvitationEntity> Invitations => Set<TenantInvitationEntity>();`.
- `TenantConfigurationProvider.Configure` → add `modelBuilder.ApplyConfiguration(new TenantInvitationEntityConfiguration());`.

**Seeding:** invitations are **never seeded** (handler/API-written only). No `SeedState.Invitations`, no
`AddRange` in any seeder. Add an explicit bullet to `api/docs/SEEDING_CONVENTIONS.md`'s never-seed list
for invitation *rows* (the list currently only names invitation-*derived memberships*).

**Migration:** re-scaffold via `api/initial-migrations.ps1` (nuke + regenerate `InitialCreate`) — never a
hand-written additive migration. The Tenant `InitialCreate` will regenerate to include `tenant.Invitations`.

---

## 4. Backend API surface

| Method | Route | Guard | Notes |
|---|---|---|---|
| `GET` | `api/organizations/members` | `OperationsView` | list members: `MemberDto(UserId, Email, Role)` — emails via `IUserModule` batch (D4). |
| `GET` | `api/organizations/invitations` | `MembersInvite` | list **pending** invitations for the members UI. |
| `POST` | `api/organizations/invitations` | `MembersInvite` | body `{ email, role }`; create invite + send email; 409 if already a member or a pending invite exists. |
| `DELETE` | `api/organizations/invitations/{id}` | `MembersInvite` | verify invite ∈ current tenant → `Revoke()`. |
| `POST` | `api/invitation/{id}/accept` | `[Authorize]` | **top-level** (D3); caller email must match invite; 409 if already a member; expired → 404; create membership + `Accept()`. **Landed singular** (`InvitationController`, `[Route("api/[controller]")]`) and returns **204** — the FE calls the singular route (see §6 corrections + D7). |
| `PUT` | `api/organizations/members/{userId}/role` | `MembersManageRoles` | `ChangeRole()`; **last-Owner invariant**. |
| `DELETE` | `api/organizations/members/{userId}` | `MembersRemove` | **last-Owner invariant**; self-leave allowed unless sole Owner. |
| `DELETE` | `api/organizations` | `TenantDelete` | deletes the active tenant (Owner-only by matrix); mirrors existing `GET`/`PUT api/organizations`. |

**Controllers** (`Tenant.Api/Controllers/`, `internal sealed`, ctor-injected): put the org-scoped
endpoints on a new `OrganizationMembersController` (or extend `TenantController`); the accept endpoint on
a small top-level `InvitationsController`. Class-level `[HasPermission(...)]` where all actions share a
guard (mirror `StripeAccountController`); per-action where they differ. Read active tenant with
`tenantContext.GetTenantId()` (throws `ForbiddenException` → 403 fail-closed).

**Application** (`Tenant.Application`): request DTOs `InviteMemberRequest(string Email, TenantRole Role)`,
`ChangeMemberRoleRequest(TenantRole Role)`; `IMembershipService`/`IInvitationService` with
`ListMembersAsync`, `ListPendingInvitationsAsync`, `InviteAsync`, `RevokeInvitationAsync`,
`AcceptInvitationAsync(id, callerUserId, callerEmail)`, `ChangeRoleAsync`, `RemoveMemberAsync`,
`DeleteCurrentTenantAsync`.

**Repository** (`ITenantRepository` + `TenantRepository`, currently read-only for memberships): add
`ListMembershipsByTenantAsync`, `CountOwnersAsync(tenantId)`, membership add/remove,
`AddInvitation`/`GetInvitationByIdAsync`/`ListPendingInvitationsByTenantAsync`/`ListPendingByEmailAsync(email, now)`,
`IsMemberAsync(tenantId, userId)`.

**Last-Owner invariant** (service layer): before demoting/removing, `CountOwnersAsync(tenantId)` and reject
if it would drop to 0 Owners. Applies to role-change, remove, and self-leave.

**Invitation email:** inject `IEmailSender` + `IUriService`. Build the accept link
`uriService.GetUri("/settings/members/accept", new(){ ["invitationId"] = id.ToString() })`, compose an
inline HTML body, `SendEmailAsync(email, subject, body)`. **Add `Urls:Frontend` to B2B config** (D6) — it's
absent from real appsettings today.

---

## 5. Registration flow — the invitation-first branch

In `TenantProvisioningHandler.HandleAsync`, **after** the persona gate + inbox dedup (`AddInboxMessage`),
**before** the existing create/announce block:

```
normalize email = e.Email.Trim().ToLowerInvariant()
pending = context.Invitations.Where(Status==Pending && ExpiresAt>now && Email==normalized)
if pending.Any():
    for each inv:
        if not already a member of inv.TenantId:                       // (TenantId,UserId) unique-index guard
            context.Memberships.Add(TenantMembershipEntity.Create(inv.TenantId, e.UserId, inv.Role,
                                                                   invitedBy: inv.CreatedByUserId, now))
        inv.Accept(e.UserId, now)
    await SaveChangesAsync(); return                                    // NO personal tenant, NO Announce()
// else fall through to current create-or-Announce personal-tenant path (unchanged — keep Announce())
```

Invariants: single handler, single `SaveChangesAsync`, **no second `CredentialRegisteredEvent` consumer**
(dedup + the invited-registration race). Idempotent under redelivery (inbox MessageId + `(TenantId,UserId)`
unique index). Two accept paths coexist: **path 1** = invited user *registers* (this branch, auto-accept);
**path 2** = invited user *already has an account* → `POST /api/invitations/{id}/accept`. Edge: auto-accept
only fires for manager-client registrations (venue/artist); a user landing via customer-web accepts via
path 2 after login — note in the invite email which app to register on.

---

## 6. Frontend — switcher + members/invite/accept (design)

> Verified against merged code (PR #156) on `master`, 2026-07-20. **Corrections to this section's earlier
> sketch and to §4, found in the shipped code — the FE must match these, not the prose:**
> - **Accept route is singular — `POST /api/invitation/{id}/accept`** (`InvitationController`,
>   `[Route("api/[controller]")]`) and returns **204 No Content** (no body). The §4 table's plural
>   `api/invitations/...` is wrong; ASP.NET routing is case-insensitive but the segment is `invitation`.
> - **All enums serialize as PascalCase strings** — `B2B.Web/Program.cs` registers a global
>   `JsonStringEnumConverter`. On the wire `role: "Owner"`, `type: "Venue"`, so the FE models them as
>   string-literal unions, never numbers.
> - **`Urls:Frontend = https://localhost:5177` = the business app**, which is a **static landing page**
>   (`app/web/b2b/business/src/main.tsx` — no router, no OIDC, no API). It **cannot host an authenticated
>   accept page** → see D9.
> - **`/me` already returns `memberships`** (`UserController.Me` → `UserBase.Memberships: MembershipDto[]`),
>   but the FE `User` type doesn't declare the field, so `useSyncUser` silently drops it today.

**App layout (verified).** Four web builds: `@concertable/web-customer` (5174), `@concertable/web-venue`
(5175), `@concertable/web-artist` (5176), `@concertable/web-business` (5177). Sharing tiers:
`app/shared` = `@concertable/shared` (universal, web+mobile — owns the `User` type + the shared axios
clients); `app/web/shared` (web-only shared — `AppLayout`, `Navbar`, `SettingsLayout`, auth guards/store,
`useSyncUser`); `app/web/b2b/shared` = `@b2b/*` (venue+artist+business only — `features/tenant`,
`features/organizations`). **"All four web builds green" IS the boundary gate** (`app/web/CLAUDE.md`): each
app's `tsc -b` compiles the shared trees against its own route tree, so a `@b2b` leak into universal
`web/shared` fails the customer build. **In scope (manager-facing):** venue + artist get the switcher,
members/invite pages, and the accept route; business is the static gateway (no feature UI); customer gets
nothing but must still compile the universal `User.memberships` type.

### 6.a — FE `User.memberships` type (`app/shared/src/features/auth/types.ts`)
- Add the wire enums as string-literal unions: `type TenantType = "Venue" | "Artist"` and
  `type TenantRole = "Owner" | "Manager" | "Finance" | "Staff" | "Door" | "Sound"`.
- Add `interface Membership { tenantId: string; legalName: string; type: TenantType; role: TenantRole }`
  (mirrors `MembershipDto`, camelCase keys).
- Add `memberships: Membership[]` to `BaseUser` (defaults `[]`; customer `/me` never populates it). Confirm
  `userApi.getMe` carries the field through — it rides the raw `/me` payload; verify no explicit mapping
  drops it. Read via `useAuthStore((s) => s.user?.memberships ?? [])`. **No new fetch.**

### 6.b — Active-tenant selection + switcher (the crux — resolves decision 2)
The plumbing already exists (§0): `useActiveTenantStore` (zustand+persist, key `concertable.active-tenant`,
`setActiveTenant(id|null)`), `TENANT_HEADER`, and the `b2bAxios.ts` request interceptor that stamps
`X-Tenant-Id` **only when `activeTenantId` is set** and clears it on `UserUnloaded`/logout. Missing: the UI
that calls `setActiveTenant`, and the fresh-multi-org flow. Backend recap: a **single**-membership user with
no header is defaulted server-side; a **multi**-membership user with no header **fails closed (403)**.

- **Persona filter.** Venue endpoints are persona-pinned, so on venue-web the active tenant must be a
  **Venue** tenant (an Artist active-id would 403 the venue controllers). The switcher/active-membership
  operate over `memberships.filter(m => m.type === <this app's persona>)` — Venue on venue-web, Artist on
  artist-web. Cross-persona "switching" = navigating to the other portal, not selecting here. Each b2b app
  supplies its persona as a constant (venue-web → `"Venue"`).
- **`useActiveMembership()` facade** (`@b2b/features/tenant`): returns the `Membership` whose
  `tenantId === activeTenantId`; else, if exactly one same-persona membership, that one (matches the backend
  single-membership default — header omitted); else `undefined` (multi, none chosen).
- **`TenantSwitcher`** (`@b2b/features/tenant/components/`): a dropdown over the same-persona memberships,
  labelled by `legalName`; selecting calls `setActiveTenant(tenantId)` and invalidates the B2B queries (the
  header changes what they return). Rendered only when there is >1 same-persona membership.
- **Fresh multi-org gate.** If `samePersonaMemberships.length > 1 && !activeTenantId`, force a choice before
  any B2B data call (else fail-closed 403). Resolve in the `_venue`/`_artist` layout: after `/me` loads,
  render the switcher in a blocking "choose your organization" state before the `Outlet`. Single-membership
  users need no selection (backend defaults; header stays absent — today's behaviour unchanged).
- **Switcher injection (boundary — D8).** `AppLayout`/`Navbar` are universal `web/shared` (customer compiles
  them) and must not import `@b2b/*`. Add an optional `headerSlot?: React.ReactNode` to `AppLayout`→`Navbar`;
  the venue/artist `_venue`/`_artist` `route.tsx` pass `<TenantSwitcher/>` into it (those files may import
  `@b2b`). Customer passes nothing.

### 6.c — Permission-gated UI (resolves decision 3)
There is **no HATEOAS `_links` and no permissions claim** in the FE (identity-only tokens). The existing gate
is `user.role` — but that is the **persona** axis (`"VenueManager"`/`"ArtistManager"`), a *different thing*
from the per-membership `TenantRole`. Member-management gating is a **new axis keyed on the active
membership's `TenantRole`**, derived client-side from a small map that mirrors the backend
`SharedPermissions.ByRole`:
- **View roster** — any member (all roles hold `OperationsView`; `GET members` is `OperationsView`-gated).
- **Invite / see pending** — `role ∈ {Owner, Manager}` (`MembersInvite`).
- **Change role / remove member** — `role === Owner` (`MembersManageRoles` / `MembersRemove`).
- **Delete organization** — `role === Owner` (`TenantDelete`) — optional UI for 6.4 (see 6.d).

Ship a tiny `tenantPermissions.ts` (`@b2b/features/tenant`) exposing e.g.
`can(role, "invite" | "manageRoles" | "remove")` whose grants copy `SharedPermissions.ByRole`. **FE gating is
cosmetic (hide/disable); the backend `[HasPermission]` is the source of truth** — a comment on the map names
`SharedPermissions.cs` so drift is visible, and the server re-checks every call regardless.

### 6.d — Members feature + pages + per-app route injection
Mirror the shipped `organizations` feature exactly (`@b2b/features/organizations`: an `api/xApi.ts` object of
`async` arrows on the shared `api` axios instance that `b2bAxios` stamps; raw `useXQuery`/`useXMutation`
hooks with `invalidateQueries`; a shared page injected by a thin per-app route file; an `index.ts` barrel).
Under `app/web/b2b/shared/src/features/members/`:
- `types.ts` — `Member` (`{ userId, email, role }` ← `MemberDto`), `Invitation`
  (`{ id, email, role, createdAt, expiresAt }` ← `InvitationDto`), `InviteMemberRequest` (`{ email, role }`),
  `ChangeMemberRoleRequest` (`{ role }`). camelCase; enums as the 6.a string unions.
- `api/membersApi.ts` (shared `api` from `@concertable/shared/lib/axiosClient`, paths relative to the `/api`
  base): `listMembers` GET `/organizations/members`; `listInvitations` GET `/organizations/invitations`;
  `invite` POST `/organizations/invitations`; `revokeInvitation` DELETE `/organizations/invitations/{id}`;
  `changeRole` PUT `/organizations/members/{userId}/role`; `removeMember` DELETE
  `/organizations/members/{userId}`. (`deleteOrganization` DELETE `/organizations` — optional.)
- hooks: `useMembersQuery`, `useInvitationsQuery` (raw); `useInviteMutation`, `useRevokeInvitationMutation`,
  `useChangeRoleMutation`, `useRemoveMemberMutation` (raw; invalidate the members/invitations keys). The
  invite form is validated by a **zod** schema before becoming the `InviteMemberRequest` (per
  `CODE_CONVENTIONS`); `role` constrained to the assignable `TenantRole`s.
- `MembersPage` — roster table (email + role) with per-row actions (change-role select, remove) gated per
  6.c; a pending-invitations list with revoke and an "expires in N days" hint from `expiresAt`; an invite
  form (email + role) shown only with `MembersInvite`.
- **Per-app route injection** (venue **and** artist):
  - `b2b/venue/src/routes/_venue/settings/members.tsx` renders `<MembersPage/>`; artist mirror
    `_artist/settings/members.tsx`.
  - append `{ label: "Members", to: "/settings/members" }` to `extraLinks` in `_venue/settings/route.tsx`
    **and** `_artist/settings/route.tsx`.
  - the `_venue`/`_artist` layout already enforces `requireBusinessRole` + persona, so these pages are
    authenticated-manager-only. Regenerate `routeTree.gen.ts` before `tsc -b` (`app/web/CLAUDE.md`).

### 6.e — Accept page, path 2 only (resolves decision 4)
Scope: the **already-registered** user who accepts via the emailed link. (Path 1 — invited user *registers* —
is auto-joined server-side by `TenantProvisioningHandler`; no page.) The route path is fixed by the backend
email: **`/settings/members/accept?invitationId=…`**.
- **Host (D9, revises D6).** The business gateway (5177) can't authenticate, so the host must be an
  auth-capable manager portal. **Recommended: backend emits the link against the invited tenant's persona
  portal** (`Urls:VenueFrontend`/`Urls:ArtistFrontend`, from the inviting tenant's `TenantType`) and both
  venue + artist host the accept route. Fallback: repoint the single `Urls:Frontend` to venue-web (5175) and
  host it there only. **Either way the config value must move off 5177.**
- **Route placement — auth only, not persona.** Accept is persona-agnostic (the caller may be a manager of
  the *other* persona, or hold no membership yet), so the route is a **top-level route**
  (`routes/settings/members/accept.tsx`, **outside** the `_venue`/`_artist` pathless layout) with
  `beforeLoad: requireAuth` — not under the persona-gated tree.
- **Login round-trip footgun.** `requireAuth` redirects to `/login?redirect=<location.pathname>` — and
  `pathname` **drops the `?invitationId` query**, so a naive accept route loses the id across login. The route
  must carry `invitationId` through login (include search in the redirect target, or persist it before
  redirecting). Call this out explicitly for the implementer.
- **Flow.** Read `invitationId` from search (zod `validateSearch`) → `POST /api/invitation/{id}/accept`
  (singular) → on 204 set the joined tenant active (D7) and redirect to `/settings/members` on that tenant's
  portal. 403 (email mismatch) / 404 (revoked, expired, or tenant gone) / 409 (already a member) render a
  clear terminal message — declare them as `expectedErrors` on the call so the global toast handler stays out
  of the way.
- **Landing needs the joined tenantId (D7).** Accept returns **204** (no body). **Recommended: return the
  joined `MembershipDto`** so the FE sets it active directly; fallback is a `/me` before/after diff.

### 6.f — Boundary gate
All four `tsc -b`/vite builds green (venue, artist, business, customer). Customer must not resolve `@b2b/*` —
the switcher-injection slot (6.b / D8) and keeping the members + tenant features inside `@b2b` are what hold
that line.

---

## 7. Sub-phases (each independently shippable, each ends green)

Gate for every sub-phase: `dotnet build api/Concertable.slnx` (0 errors) + the affected module unit +
integration suites via the `integration-debug` skill. Re-scaffold phases end with
`./initial-migrations.ps1` from `api/`. E2E only where flagged (per `plans/CLAUDE.md`).

### 6.1 — Invitation entity + schema + migration *(re-scaffold; zero behavior change)* — ✅ DONE (verified)
`InvitationStatus`, `TenantInvitationEntity` + config, `Schema.Tables.Invitations`, DbSet,
`ApplyConfiguration`, Contracts DTOs (`MemberDto`, `InvitationDto`), never-seed doc bullet, re-scaffold.
Repo read/write method signatures may land here (unused = fine). **Gate:** build + `Tenant.UnitTests`/`Tenant.IntegrationTests` + re-scaffold. **No E2E.**
> Landed: enum + entity + EF config + `Schema.Tables.Invitations` + DbSet + `ApplyConfiguration` + `MemberDto`/`InvitationDto` +
> SEEDING_CONVENTIONS never-seed bullet for invitation rows. Filtered-unique index `(TenantId, Email) WHERE [Status] = 1` kept (re-scaffolded
> cleanly — the service-layer fallback was not needed). Repo/service method signatures deferred to 6.2. Tenant `InitialCreate` re-scaffolded to
> include `tenant.Invitations` (only the Tenant migration committed — the full `initial-migrations.ps1` re-timestamps every module identically, so
> non-Tenant regenerations were reverted). Gate green: build 0 errors, Tenant.UnitTests 94/94, Tenant.IntegrationTests 21/21 (migration applied to real SQL).

### 6.2 — Member management endpoints + last-Owner invariant *(no re-scaffold)* — ✅ DONE (verified)
`GET members`, `PUT members/{userId}/role`, `DELETE members/{userId}`, `DELETE api/organizations`;
`MembershipService` + repo methods; `IUserModule` batch email lookup (D4). **Gate:** build + Tenant/Venue/Artist
integration suites (Owner-vs-Manager boundaries, last-Owner guards, self-leave). **No E2E** (new endpoints,
not on a covered E2E flow; integration drives the real auth pipeline).
> Landed: `IUserModule.GetEmailsByIdsAsync` (D4, additive same-service) + `ITenantRepository` membership methods
> (`ListMembershipsByTenantAsync`/`FindMembershipAsync`/`CountOwnersAsync`/`IsMemberAsync`/`AddMembership`/`RemoveMembership`;
> `AddMembership`/`IsMemberAsync` land unused for 6.3). New `IMembershipService` + impl beside `TenantService` (D5) with the
> last-Owner invariant (409 on demote/remove/self-leave of the sole Owner) + `DeleteCurrentTenantAsync` (cascades memberships).
> New `OrganizationMembersController` on `api/organizations` (per-action `[HasPermission]`, no `[TenantPersona]` — mirrors
> `StripeAccountController`); `ChangeMemberRoleRequest` + `IsInEnum` validator; `Tenant.Infrastructure → User.Contracts`
> ProjectReference. Gate green: build 0 errors; **Tenant 34/34, Venue 25/25, Artist 17/17** integration (real auth pipeline —
> Owner-vs-Manager 403 boundaries, last-Owner 409, non-sole-owner self-leave, persona-agnostic venue+artist). Note: seeded
> `*NoVenue/*NoArtist` operators own their own tenant, so a Manager acting in another tenant must send `X-Tenant-Id` or
> resolution fails closed (403) — the permission tests name the tenant explicitly to gate on the real reason.

### 6.3 — Invitations create/revoke/accept + provisioning branch + email *(no re-scaffold)* — ✅ DONE (build + integration green; merge-queue E2E pending)
`POST`/`DELETE invitations`, `POST /api/invitations/{id}/accept`, `InvitationService` + email
(`IEmailSender`+`IUriService`, add `Urls:Frontend`), the `TenantProvisioningHandler` invitation-first
branch. **Gate:** build + Tenant integration (invite→accept→membership, dup/already-member 409, expiry,
email captured in `MockEmailSender.Sent`). **E2E is the merge-queue's job, not a local step** — the
invited-registration flow (`TestTokenMinter`, fresh email, provisioning flip) is covered by the queue's
E2E gate on the PR, so don't burn ~25-30 min running it locally before committing.
> Landed: top-level `InvitationsController` (accept only; `[Authorize]`, email-match gate — the accepting caller may
> belong to no tenant yet). Invite/list/revoke on `OrganizationMembersController` under `api/organizations`
> (`[HasPermission(MembersInvite)]`). `IInvitationService`/`InvitationService` (invite/list/revoke/accept) sending the
> invite email via `IEmailSender`+`IUriService` with `Urls:Frontend` added to B2B `appsettings.json`. `TenantProvisioningHandler`
> invitation-first branch: a registering email with pending, unexpired invitations joins the inviting tenant(s) as an ordinary
> member — no personal tenant, no `Announce()` — inside the inbox-deduped transaction (idempotent over redelivery, case-insensitive
> match). **BUG1b** from the review fixed here — accept path guards on the tenant still existing before creating the membership.
> `InviteMemberRequest` + `InviteMemberRequestValidator`, DI registration. Gate green: build 0 errors; **Tenant integration 52/52**
> (16 `InvitationTests` + 3 invitation-first `TenantProvisioningTests` + existing member/resolution/stripe/tax suites).

### 6.4 — Frontend switcher + members/invite/accept pages *(final)*
Full design + resolved decisions (D7–D9) in **§6**. Ordered build — each step keeps all four web builds
compiling:

1. **Type (6.a)** — `User.memberships` + `Membership`/`TenantType`/`TenantRole` in
   `app/shared/src/features/auth/types.ts`; confirm `getMe` carries `memberships` through.
2. **Active-tenant (6.b)** — `useActiveMembership`, `TenantSwitcher`, the `AppLayout`/`Navbar` `headerSlot`
   (D8), the fresh-multi-org blocking gate in the `_venue`/`_artist` layout, the persona filter.
3. **Permissions (6.c)** — `tenantPermissions.ts` mirroring `SharedPermissions.ByRole`.
4. **Members feature (6.d)** — `features/members` types/api/hooks/`MembersPage`, plus the per-app
   `settings/members.tsx` wrappers and settings-nav links in **both** venue and artist.
5. **Accept (6.e)** — top-level `requireAuth` accept route at `/settings/members/accept`; the D9 host change
   (off 5177) + D7 landing return-value; the login-redirect `invitationId` footgun.
6. **E2E** — add the UI scenario(s) from [`TENANT_INVITATIONS_E2E.md`](./TENANT_INVITATIONS_E2E.md) §"UI E2E
   (6.4 gate)" (invite → accept → manage; switcher stamps `X-Tenant-Id`) as a new `.feature` + Steps +
   PageObjects under `api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui/`, and register each new
   scenario's exact Reqnroll DisplayName in `api/Concertable.Shared/tests/Concertable.E2ETests/E2E_BASELINE.md`
   — add to the `### B2B passing (N)` fenced block, bump that `(N)` **and** the Summary table (additive, per
   the file's parser rules).

**Gate:** all four web builds green
(`npm -w @concertable/web-{customer,venue,artist,business} run build`) **+ UI E2E via `e2e-ui-debug`** — the
final phase earns E2E (it flips user-facing behaviour on the registration/login-adjacent flow;
`plans/CLAUDE.md`). Mandatory `./docker-health.ps1` pre-flight (root `CLAUDE.md`). The PR merge queue is the
E2E gate — don't run the full suite locally ahead of it; only run `e2e-ui-debug` locally if the queue fails on
these scenarios.

**Landing commit (final phase — `plans/CLAUDE.md` Lifecycle 4):** in the commit that lands 6.4, `git rm`
**both** `plans/b2b/TENANT_INVITATIONS_PHASE6.md` and `plans/b2b/TENANT_INVITATIONS_E2E.md` (its exit criteria
are met once the UI scenario lands), and in the **same commit**: tick `plans/b2b/USER_MODEL_PLAN.md` §8 **Phase 6** (that line is Phase-6-specific)
and **annotate** the `plans/b2b/LAUNCH_PLAN.md` "Finish Swim-lane B" line (line ~25) to note the Phase 6
membership/invitation slice has shipped — **don't fully tick it**, Phases 7-8 (auth sweep + messaging
group-inbox) remain under that same item. Before committing, run `git status --short plans/` and confirm both
plan files show `D` (deleted), not survivors.

---

## 8. Risks
- **Invited-registration race** — keep invitation-matching in the single inbox-deduped
  `TenantProvisioningHandler` transaction; never a second consumer (parent R "Invited-registration race").
- **Re-scaffold drift** — only 6.1 re-scaffolds; run integration immediately after `initial-migrations.ps1`.
- **`Urls:Frontend` absent in prod config** — accept links break silently if not added (D6); the value is
  only in the test fixture today.
- **Messaging worktrees in-flight** (`Feature/MessagingOutbox`, `Feature/AzureServiceBusTransport`) — Phase 6
  touches no messaging code; Phase **8** (group inbox) is the collision, deliberately out of scope here.
- **E2E baseline churn** — 6.3/6.4 touch registration/login surfaces; run the regress as the exit gate.
