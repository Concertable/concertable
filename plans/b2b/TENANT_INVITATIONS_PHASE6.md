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
| `POST` | `api/invitations/{id}/accept` | `[Authorize]` | **top-level** (D3); caller email must match invite; 409 if already a member; expired → 410/400; create membership + `Accept()`. |
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

## 6. Frontend (`app/web/b2b/shared`, injected per-app)

- **Extend the FE `User` type** (`app/shared/src/features/auth/types.ts`) with `memberships: Membership[]`
  + a `Membership` type mirroring `MembershipDto` — they already ride on `/me`, so no new fetch (read via
  `useAuthStore((s) => s.user?.memberships)`).
- **Complete the tenant switcher:** add the dropdown under
  `app/web/b2b/shared/src/features/tenant/components/`, drive it from `user.memberships`, call
  `useActiveTenantStore.getState().setActiveTenant(id)` (store + header interceptor already exist).
- **Members + invitations feature** under `app/web/b2b/shared/src/features/members/`: `membersApi`
  (`listMembers`, `listInvitations`, `invite`, `revokeInvitation`, `changeRole`, `removeMember`) using the
  shared `axiosClient` (explicit endpoint verbs — **no HATEOAS `_links` in the FE**); `useQuery`/`useMutation`
  hooks with `invalidateQueries`; a `MembersPage` (list + role/remove row actions via a `renderActions` slot)
  and an invite form. Mirror the `organizations` feature's shape + per-app route injection.
- **Accept page** at `/settings/members/accept?invitationId=…` → `POST /api/invitations/{id}/accept` (requires
  auth; redirect to login/register first if no session).
- **Route injection:** register the pages under each app's `_venue`/`_artist` settings tree
  (`b2b/venue/src/routes/_venue/settings/members.tsx`, artist mirror) — the `beforeLoad` there already
  enforces the business role. Gate member-management UI on the active membership's role (backend
  `HasPermission` remains the source of truth; FE gate is cosmetic).
- **Boundary gate:** all four `tsc -b`/vite builds green (venue, artist, business, customer). Customer must
  not resolve `@b2b/*`.

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

### 6.3 — Invitations create/revoke/accept + provisioning branch + email *(no re-scaffold)*
`POST`/`DELETE invitations`, `POST /api/invitations/{id}/accept`, `InvitationService` + email
(`IEmailSender`+`IUriService`, add `Urls:Frontend`), the `TenantProvisioningHandler` invitation-first
branch. **Gate:** build + Tenant integration (invite→accept→membership, dup/already-member 409, expiry,
email captured in `MockEmailSender.Sent`) **+ API E2E** — invited-registration via `TestTokenMinter` with a
fresh email (this flips the registration/provisioning flow, which E2E covers → massive/risky bar met).

### 6.4 — Frontend switcher + members/invite/accept pages *(final)*
FE `User.memberships` type, switcher UI, members feature + pages, accept page, per-app route injection.
**Gate:** all four web builds green + **UI E2E** (append a members/invite scenario to `E2E_BASELINE.md`,
additive) via `e2e-ui-debug`. Final phase → delete this plan file in the landing commit; tick the
LAUNCH_PLAN + USER_MODEL_PLAN Phase 6 lines in the same commit.

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
