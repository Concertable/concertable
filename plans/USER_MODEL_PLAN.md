# User-Model Plan — multi-user tenants, roles, permissions, the auth sweep

> **Sequencing: the prerequisite tenant-scoping work is complete** — the tenant-scoping plan
> shipped (full E2E green) and was deleted per the plans rule, so this plan's gate is met and
> **Phase 1 below is the next stage.** That work supplied what this plan builds on:
> `ArtistEntity.TenantId` and the two-party OR-filters behind the ownership sweep (§Phase 3) and the
> messaging re-model (§Phase 8), plus the compliance value object on `TenantEntity`. This plan owns
> everything the tenant-scoping plan deferred to "the later user-model plan": membership, roles, the
> authorization sweep, multi-tenant switching, and the `MessageEntity` group-inbox decision.

## 0. Scope and non-goals

In scope: multiple users per tenant; users in multiple tenants; a tenant-scoped role set with
permissions as the enforcement unit; invitations and member management; the active-tenant
mechanism; retiring the flat `Role` enum, the manager profile tables, and the transitional
`role`/`owner` token claims; re-fronting the B2B payout endpoints; the group-inbox re-model.

Non-goals: Customer-side authorization stays its simple is-registered check (`CustomerUserHandler`
row-existence — already identity-only-compliant). Platform admin stays orthogonal to tenancy:
`AdminProfileEntity` + the `Admin` policy/attribute survive unchanged (they gate one endpoint,
`PATCH /api/Venue/{id}/approve`); expanding the admin story is out of scope. No per-tenant custom
roles, no admin UI for role editing.

Doc corrections to fold in while here: `plans/AUTH_IDENTITY_REFACTOR.md` still says Phase 3 is
pending, but it is structurally complete in code (Auth owns `CredentialEntity`/`AuthDbContext`;
B2B.User is already an event-driven projection; the remote claims provider exists) — refresh its
status when starting Phase 1. `api/Concertable.Payment/CLAUDE.md` still names
`ManagerRegisteredHandler`, deleted by tenant-scoping Phase 3.

## 1. The model

### 1.1 Paradigm: static roles, permissions as the checked unit

Roles are predefined bundles of permissions; **call-sites check permissions, never role names**
(`[HasPermission(Permissions.ApplicationsDecide)]`). Adding or reshaping a role touches the
catalog, never an endpoint. There are no resource-level grants: every governable resource is
tenant-owned, so "may user U act on resource R" decomposes into (a) U's membership in the active
tenant carries the required permission, and (b) `R.TenantId == activeTenant` — and (b) is exactly
what the tenant-scoping filters/interceptor already enforce. Authorization is derived per-request
from B2B's own data; tokens stay identity-only (`sub` + audience), per the architecture north star.

Permissions are **`string` constants**, not an enum (`Permissions.ApplicationsDecide =
"applications.decide"` in `Tenant.Contracts`) — the idiomatic modern ASP.NET Core permission shape,
where the permission string *is* the policy name. The identifier never crosses a serialization
boundary (identity-only tokens carry no permissions; membership rows store the *role*, not
permissions), so the string form costs nothing the enum would have saved. The one thing it gives up
— a compile-checked attribute argument — is recovered by a catalog-coverage test (§Phase 2).

Role→permission binding is a **code-defined static map** — `PermissionCatalog` holding a
`FrozenDictionary<TenantRole, FrozenSet<string>>` plus per-permission persona constraints. No
`RolePermission` table, no per-tenant custom roles: the matrix is unit-testable, versioned with
code, costs no admin UI, and doesn't fight the full-re-scaffold migration convention. Membership
rows store only the role name; expansion happens in code. If custom roles are ever truly demanded,
a per-tenant override table can be added later without touching a single call-site, because
call-sites only know permissions.

### 1.2 Roles and persona

```csharp
public enum TenantRole { Owner = 1, Manager = 2, Finance = 3, Staff = 4, Door = 5, Sound = 6 }
public enum TenantType { Venue = 1, Artist = 2 }
```

- **Owner** — full control including destructive/financial config. Every tenant has ≥1 Owner,
  enforced as an invariant (§6).
- **Manager** — runs the business day-to-day: profile, opportunities, applications, concerts,
  messaging, inviting staff.
- **Finance** — money only: payout/billing config, settlement view + trigger.
- **Staff** — general operational member: schedule, messaging, day-of-show ops.
- **Door** — day-of-show entry: schedule + check-in/guest list (reserved surface, see §1.3).
- **Sound** — day-of-show tech: schedule + ops/tech notes (reserved surface).

Exactly **one role per membership** (unique `(TenantId, UserId)`). A user may hold memberships in
many tenants, including one venue-tenant and one artist-tenant simultaneously. No extra roles:
"Booker"/"Promoter" collapse into Manager, "Accountant" is Finance.

Persona lives on the tenant, not the role: `TenantType` is set at provisioning from the
registration client-id (`venue-web`/`venue-mobile` → Venue, `artist-web`/`artist-mobile` →
Artist). Permissions may carry a persona constraint checked against the active tenant's type —
that constraint is what replaces today's `[VenueManager]`/`[ArtistManager]` split.

### 1.3 Permission catalog and matrix

(V) = venue-tenant only, (A) = artist-tenant only. *Reserved* = defined in the catalog now so the
six-role matrix is meaningful, first enforced when day-of-show features ship; until then Door and
Sound reduce to `OperationsView` (+ `ConcertsOpsEdit` for Sound once it has a surface).

| Permission | Gates (real call-sites today) | O | M | F | St | D | So |
|---|---|---|---|---|---|---|---|
| `OperationsView` | `VenueDashboardController`, `ArtistDashboardController`, `GET /api/Venue/user`, `GET /api/Artist/user`, schedule reads | X | X | X | X | X | X |
| `ProfileEdit` | `VenueController` POST/PUT, `ArtistController` POST/PUT | X | X | | | | |
| `OpportunitiesManage` (V) | `OpportunityController` POST, POST /bulk, PUT | X | X | | | | |
| `ApplicationsDecide` (V) | `ApplicationController` venue side: list, eligibility, checkout, accept; booking decisions | X | X | | | | |
| `ApplicationsSubmit` (A) | `ApplicationController` artist side: apply, pending, denied, eligibility, checkout | X | X | | | | |
| `ConcertsManage` (V) | `ConcertController` PUT, PUT /post | X | X | | | | |
| `PayoutsManage` | Stripe onboarding-link / account-status / payment-method / setup-intent (via the B2B proxy, §5), billing config | X | | X | | | |
| `SettlementView` | settlement/transaction reads | X | X | X | | | |
| `SettlementTrigger` | settlement kick — currently worker/dev-driven; gate the moment it becomes user-facing | X | | X | | | |
| `TenantSettingsEdit` | legal name + compliance value object (tenant-scoping Phase 5 surface) | X | | | | | |
| `TenantDelete` | delete tenant | X | | | | | |
| `MembersInvite` | create/revoke invitations | X | X | | | | |
| `MembersRemove` | remove a member | X | | | | | |
| `MembersManageRoles` | change a member's role | X | | | | | |
| `MessagesRead` | tenant inbox visibility (§7) | X | X | X | X | | |
| `MessagesSend` | posting messages | X | X | | X | | |
| `ConcertsOpsEdit` *(reserved)* | set times, stage/tech notes | X | X | | X | | X |
| `ConcertsCheckIn` (V) *(reserved)* | door scanning, guest list | X | X | | X | X | |

Bookings: no standalone booking accept/decline endpoints exist today (booking creation rides
application-accept), so booking decisions fold into `ApplicationsDecide`/`ApplicationsSubmit`.
Split out a `BookingsDecide` permission if/when cancel/decline endpoints appear.

## 2. Data model

Everything lives in the **Tenant module** — it owns tenant composition, and the User module is an
Auth projection (per AUTH_IDENTITY_REFACTOR); putting authority data there would re-entangle it.
Cross-module access goes through the `ITenantModule` facade + `Tenant.Contracts` types only; FKs
stay primitive Guids per `MODULAR_MONOLITH_RULES.md`.

```csharp
// Concertable.B2B.Tenant.Contracts: TenantType, TenantRole, Permissions (string constants),
//                                   InvitationStatus, PermissionCatalog, IMembershipContext,
//                                   membership/invitation DTOs

// Concertable.B2B.Tenant.Domain
public sealed class TenantMembershipEntity : IGuidEntity
{   // unique index (TenantId, UserId); index UserId
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }            // Auth sub
    public TenantRole Role { get; private set; }
    public Guid? InvitedByUserId { get; private set; }  // null = founding Owner
    public DateTime CreatedAt { get; private set; }
    public static TenantMembershipEntity Create(Guid tenantId, Guid userId, TenantRole role, Guid? invitedBy, DateTime at);
    public void ChangeRole(TenantRole role);            // service enforces the last-Owner invariant
}

public sealed class TenantInvitationEntity : IGuidEntity
{   // the Id IS the accept token (unguessable Guid in the emailed link); unique (TenantId, Email) while Pending
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; }           // normalized lower-case; indexed for registration matching
    public TenantRole Role { get; private set; }
    public InvitationStatus Status { get; private set; } // Pending | Accepted | Revoked | Expired
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }     // 7 days
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
}
```

`TenantEntity` gains `TenantType Type`, set at provisioning. `CreatedByUserId` survives as audit
only — authority comes solely from membership rows.

**Deleted at the end state:** `UserEntity.Role`, `VenueManagerProfileEntity`,
`ArtistManagerProfileEntity` (+ their configurations, mappers, policy handlers, attributes), the
Kernel `Role` enum, the transitional `role` and `owner` token claims, `ICurrentUser.Owner`, and
`CurrentUserExtensions.GetOwnerId()`.

## 3. Active-tenant resolution: `X-Tenant-Id` header, fails closed

Tokens are identity-only and live 900s, so the acting tenant is request state, not token state:

- The client sends `X-Tenant-Id: <guid>` on B2B requests. The SPAs persist the selected tenant; a
  tenant switcher lists memberships from `GET /api/auth/me`.
- `TenantContext.ResolveAsync` (the existing `TenantResolutionMiddleware` slot,
  `api/Concertable.B2B/Concertable.B2B.Web/Middleware/TenantResolutionMiddleware.cs`): read the
  header → load the membership row for `(currentUser.Id, headerTenantId)` → memoize
  `(TenantId, TenantType, Role)` for the request. Header absent and the user has **exactly one**
  membership → default to it (this is what keeps every current client and test green through the
  migration). No valid membership → `TenantId` stays null and the request **fails closed**: query
  filters see nothing, permission checks 403.
- Kernel's `ITenantContext` is unchanged. The Tenant module's implementation additionally
  implements a B2B-only contract:

```csharp
// Concertable.B2B.Tenant.Contracts
public interface IMembershipContext
{
    TenantRole? Role { get; }              // null = no active membership
    bool HasPermission(string permission); // pass a Permissions.* constant; catalog bundle + persona constraint vs active TenantType
}
```

## 4. Enforcement in B2B

```csharp
// Concertable.B2B.Tenant.Api — same cross-module Api-reference precedent as today's [VenueManager] in User.Api
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) => Policy = $"perm:{permission}"; // pass a Permissions.* constant
}
```

This is the recognized modern shape — string permissions + a custom policy provider + `[HasPermission]`:

- **`PermissionPolicyProvider : IAuthorizationPolicyProvider`** (Tenant.Infrastructure) builds each
  `perm:<name>` policy **on demand** — `.RequireAuthenticatedUser()` (so anonymous → 401, not 403) +
  a `PermissionRequirement(name)` — and **delegates `GetDefaultPolicyAsync` /
  `GetFallbackPolicyAsync` and any non-`perm:` name to an inner `DefaultAuthorizationPolicyProvider`**,
  so the surviving `Admin` policy and every bare `[Authorize]` keep working (the one place this pattern
  breaks if the delegation is forgotten). Registered **singleton** — there is no startup policy loop.
- `PermissionAuthorizationHandler` (registered **scoped** — it depends on the scoped membership
  context, matching today's profile handlers) does **no DB work of its own**: it calls
  `ITenantResolver.ResolveAsync()` (memoized — safe whether or not `TenantResolutionMiddleware` ran
  first) and reads `IMembershipContext.HasPermission(requirement.Permission)`. One indexed row read per
  request, no cross-request caching — so role changes and removals take effect on the next request,
  which is the entire reason authority lives in the DB and not in a 900s token.
- Service-level ownership checks change shape: `entity.UserId != currentUser.GetId()` guards become
  tenant-scoped reads (`TenantScopedRepository.CurrentTenant` — the dormant seam in
  `Concertable.B2B.DataAccess.Infrastructure/TenantScopedRepository.cs`, wired for exactly this) or
  explicit `entity.TenantId != tenantContext.TenantId` comparisons for two-party resources.
  "Manager not found" profile lookups become `tenantContext.HasTenant` guards. `ICurrentUser.Id`
  remains only for attribution (CreatedBy, SentBy, read state).

## 5. Cross-service: Payment goes service-token-only; consumers front the endpoints

The four end-user `StripeAccountController` endpoints (onboarding-link, account-status,
payment-method, setup-intent) stop serving B2B managers:

- **B2B** adds a thin payouts controller (Tenant module Api) gated
  `[RequirePermission(Permission.PayoutsManage)]`, calling Payment over the existing ServiceToken
  gRPC channel with explicit `ownerId = ITenantContext.TenantId`. Payment's gRPC surface gains the
  missing onboarding-link/account-status/payment-method/setup-intent RPCs (reads under a
  `payment:read` scope claim, setup-intent under the existing `payment:write`). Authorization
  happens where the membership data lives.
- **Customer** keeps using the HTTP endpoints unchanged via the `GetOwnerId() → sub` fallback (a
  ticket payer genuinely is a user) until the claim retirement phase, when Customer's path is
  re-pointed the same way or kept on `sub` directly.
- Payment never learns what an owner *is* — the opaque-OwnerId mandate holds; no Payment→B2B call,
  no membership claims in tokens. The SPAs change the payout feature's base URL from the Payment
  host to their own service.

The `owner` claim, `ICurrentUser.Owner`, and `GetOwnerId()` are deleted once this lands (Phase 5):
one claim cannot represent N memberships, and it goes ambiguous the moment the first invitation is
accepted.

## 6. Registration, invitations, member management

`TenantProvisioningHandler` (same single inbox-deduped handler, one transaction — never a second
consumer of the same event, to avoid ordering races) becomes:

1. `CredentialRegisteredEvent` arrives; manager client-ids map to `TenantType`.
2. **Check pending, unexpired invitations matching the normalized email FIRST.** If any exist:
   create a `TenantMembershipEntity` per invitation (the invitation's role), mark them Accepted,
   and do **not** auto-provision a personal tenant — invited staff don't get junk tenants.
3. Otherwise: create `TenantEntity(Type)` + founding Owner membership — idempotent over
   seed-pre-inserted tenants via the handler's existing-tenant branch (today a no-op once the tenant
   is present; this phase adds "ensure the Owner membership exists" there). The branch must **not**
   re-publish `TenantCreatedEvent` — the seed insert already published it, so re-publishing would
   double-provision Payment.

`CredentialRegisteredHandler` (B2B.User) keeps writing the `UserEntity` projection — eventually
sans `Role` and profile rows (Phase 7).

Member-management endpoints (Tenant module):

| Endpoint | Guard | Notes |
|---|---|---|
| `GET /api/tenants/mine` | `[Authorize]` | the caller's memberships (feeds the tenant switcher) |
| `GET .../members` | `OperationsView` | list members + roles |
| `POST .../invitations {email, role}` | `MembersInvite` | email via `Concertable.Shared.Email` with accept link carrying the invitation Id |
| `DELETE .../invitations/{id}` | `MembersInvite` | revoke |
| `POST .../invitations/{id}/accept` | `[Authorize]` | caller email must match; 409 if already a member; status → Accepted |
| `PUT .../members/{userId}/role` | `MembersManageRoles` | last-Owner invariant |
| `DELETE .../members/{userId}` | `MembersRemove` | last-Owner invariant; self-leave allowed except sole Owner |
| `DELETE /api/tenants/current` | `TenantDelete` | Owner only by matrix |

Invariant enforced in the service layer: **at least one Owner always remains** — role changes,
removals, and self-leave all check it.

## 7. Messaging: the group inbox (decision deferred here from the tenant-scoping work)

`MessageEntity` becomes tenant-pair-scoped: `FromTenantId`, `ToTenantId`, `SentByUserId`
(attribution), content/action/sent-date as today. Visibility = active tenant ∈
{`FromTenantId`, `ToTenantId`} — a Bucket-B two-party entity under tenant-scoping Phase 4's
OR-filter mechanism — **and** the member holds `MessagesRead`. Any member of either tenant with
that permission sees the thread.

The boolean `Read` column is replaced by a thread-level read pointer (thread identity = the tenant
pair): `ThreadReadStateEntity { TenantId, UserId, CounterpartTenantId, LastReadAt }` — per-user
unread badges without per-message-per-user row explosion.

Notification **routing stays separate from visibility**: tenant scoping makes a thread visible to
the org; who gets pinged is the Notification side's job (members holding `MessagesRead`, filtered
by their notification preferences), never the query filter's.

`MessagesSend` gates posting. Message *actions* additionally require the action's own permission —
accepting an application via a message action still requires `ApplicationsDecide`.

## 8. Phases — each independently shippable, each ends green

Verification gate for every phase: `dotnet build api/Concertable.slnx` and the affected module unit +
integration test projects via `dotnet test`. The E2E suites (API `Concertable.B2B.E2ETests` + the UI
regress against `api/Shared/Tests/Concertable.E2ETests/E2E_BASELINE.md`) are run **only when the phase
is massive or behaviorally risky** — per [`plans/CLAUDE.md`](./CLAUDE.md), not by reflex. Phases that
flip user-facing behavior on a covered flow (notably 2, 5, 6, 7, 8) clear that bar; foundational
zero-behavior-change phases (1) do not. Phases marked *re-scaffold* end with `./initial-migrations.ps1`
from `api/` (never additive migrations).

### Phase 1 — Membership table + `TenantType` + Owner provisioning ✅ *(re-scaffold; zero behavior change)*

> **Shipped** on `Feature/tenant-membership`. Build green; `Tenant.UnitTests` (28) + `Tenant.IntegrationTests`
> (10, incl. registration → tenant + Owner membership + persona, and idempotency over the seeded row) pass.
> E2E intentionally **not** run — zero-behavior-change foundational phase (see `plans/CLAUDE.md`). The one
> deviation from the design below: `TenantContext` resolves `tenantId` from the membership row but does **not**
> yet stash the membership on the scoped context — deferred to Phase 4's `IMembershipContext` work where it has
> a consumer (avoided dead state in Phase 1).

Membership becomes the source of truth for "whose tenant is this" while one-user-per-tenant still
holds.

- `Tenant.Domain`: `TenantMembershipEntity` + EF configuration (unique `(TenantId, UserId)`, index
  `UserId`); `TenantEntity.Type` added; `TenantEntity.Create(...)` takes the persona.
- `Tenant.Infrastructure/Events/TenantProvisioningHandler.cs`: derive `TenantType` from
  `e.ClientId`; create the founding Owner membership on the create branch; ensure it exists
  (idempotently) on the existing-tenant branch (without re-publishing `TenantCreatedEvent`).
- `Tenant.Infrastructure/Services/TenantContext.cs`: resolve via the membership row (single row
  today) instead of `GetByCreatedByUserIdAsync`; stash the loaded membership on the scoped context.
- `TenantService` / `ITenantModule.GetTenantIdByUserIdAsync`: membership-backed — the `owner` claim
  in `UserClaimsController` keeps working unchanged.
- Seeding: add a `Memberships` list to `SeedState` (founding Owner membership per manager) inserted
  by `TenantDevSeeder`/`TenantTestSeeder` alongside tenants — same documented direct-insert
  exception as tenants (deterministic ids; the handler re-announces idempotently over them). Only
  founding-Owner rows are ever seeded; invitation-created memberships are handler/API-written and
  never seeded (extend `api/docs/SEEDING_CONVENTIONS.md`'s never-seed list).
- Exit grep: `GetByCreatedByUserIdAsync` — after this phase only the provisioning handler's
  idempotency check may use it.

Tests: `Concertable.B2B.Tenant.UnitTests` (resolve-via-membership; persona derivation);
`Tenant.IntegrationTests` asserts registration creates tenant + Owner membership + persona.
Everything else untouched.

### Phase 2 — Permission policies replace `[VenueManager]`/`[ArtistManager]` ✅ *(no re-scaffold)*

> **Shipped** on `Feature/permission-policies` (off `Feature/tenant-membership`). Build green; the affected
> suites pass — `Tenant.UnitTests` (49, incl. catalog-coverage + membership permission/persona cases) and the
> integration suites that drive the real authorization pipeline: `Tenant` (10), `Venue` (25), `Artist` (17),
> `Concert` (63), `User` (3). E2E skipped by policy: the blast radius is the B2B authorization pipeline, which
> the integration suites exercise end-to-end through the real ASP.NET middleware (every swapped family,
> wrong-persona → 403, Admin delegation, bare `[Authorize]`, anonymous → 401); token issuance, claims, Auth,
> and the SPA are untouched by this phase.
>
> **Deviation from the design below:** persona is enforced at the *call-site* — `[HasPermission(perm,
> TenantType.X)]` — not as a catalog per-permission constraint. The shared `ProfileEdit`/`OperationsView`
> permissions gate both venue *and* artist controllers, so one catalog constraint can't express the split;
> pinning persona at the call-site reproduces today's `[VenueManager]`/`[ArtistManager]` gating exactly
> (provably identical — the Venue/Artist suites already assert wrong-persona → 403). `PermissionCatalog` is
> therefore a pure role→permission map. This also lands the Phase-1-deferred stash: `TenantContext` now
> memoizes role + persona, since `IMembershipContext` is its first consumer. Venue.Api keeps its `User.Api`
> reference (for the surviving `[Admin]` on `VenueController.Approve`); Artist/Concert Api swap it for `Tenant.Api`.

- New in the Tenant module: `Permissions` (string constants), `TenantRole`, `PermissionCatalog`
  (+ persona constraints), `IMembershipContext`, `HasPermissionAttribute` (Tenant.Api),
  `PermissionRequirement` + `PermissionAuthorizationHandler` + `PermissionPolicyProvider`
  (Tenant.Infrastructure). Registration:
  `AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()` +
  `AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()` — no startup policy loop.
- Attribute swap at every call-site: `VenueController`, `VenueDashboardController`,
  `ArtistController`, `ArtistDashboardController`, `OpportunityController`,
  `ApplicationController`, `ConcertController` — per the matrix in §1.3. Module Api projects swap
  their `User.Api` reference for `Tenant.Api`.
- Delete `VenueManagerAttribute`/`ArtistManagerAttribute`
  (`Concertable.B2B.User.Api/Authorization/`) and
  `VenueManagerProfileHandler`/`ArtistManagerProfileHandler` + their policy registrations
  (`User.Infrastructure/Extensions/ServiceCollectionExtensions.cs`). **Keep** the `Admin` trio.
  Keep writing manager profile rows for now (retired in Phase 7) so `/api/auth/me` and mappers are
  untouched.
- Behavior is provably identical while one-user-per-tenant holds: profile-row-exists ⟺
  Owner-membership-whose-persona-bundle-contains-the-permission.

Tests: handler unit tests (wrong persona, no membership, Finance vs Manager); one integration test
per swapped controller family asserting 403 for the wrong persona (artist manager POSTs a venue); a
catalog-coverage unit test asserting every `Permissions.*` constant appears in the matrix and every
call-site permission string is a real constant (recovers the compile-time guarantee the enum gave).
Memberships are already seeded from Phase 1, so fixtures pass unchanged.

### Phase 3 — Service-layer ownership: UserId → TenantId ✅ *(no re-scaffold)*

> **Shipped** on `Feature/permission-policies`. Build green; the affected suites pass —
> `Concert.UnitTests` (52), `Venue.IntegrationTests` (25), `Artist.IntegrationTests` (17),
> `Concert.IntegrationTests` (63). E2E skipped by policy: behavior is identical and Phase 3 isn't in
> the massive/risky set (§8); the integration suites drive the real ASP.NET authorization + tenant-filter
> pipeline end-to-end (cross-tenant ownership, wrong-persona, accept-by-wrong-manager). No model change ⇒
> no re-scaffold.
>
> **Key deviation — cross-tenant ownership is 404, not the "403" written below.** TS Phase 4's tenant
> query filter already enforces ownership on the read-filtered `Venue`/`Artist`: `GetByIdAsync` is a
> filtered LINQ query, so a cross-tenant id returns null ⇒ `NotFoundException` ⇒ **404** (better posture
> than 403 — doesn't reveal a sibling tenant's resource, and is the behavior already shipped in Phase 2).
> The `entity.UserId != currentUser` guards in `UpdateAsync` were therefore dead for cross-tenant and were
> **removed** (the filter is the enforcement); the existing `Update_ShouldReturn404_WhenNotOwner` tests
> (Venue: `VenueManager2`; Artist: `ArtistManagerNoArtist`) **are** the two-tenant cross-ownership coverage.
> Opportunities are **not** read-filtered, so their ownership became explicit
> `opportunity.TenantId == tenantContext.TenantId` comparisons (`OwnsOpportunity*`,
> `ApplicationValidator.CanAcceptAsync` — the latter swept too, though the plan's line refs didn't
> enumerate it).

What shipped:

- **Venue/Artist services**: "my venue/artist" reads (`GetDetailsForCurrentUser*`, `GetIdForCurrentUser*`,
  `Owns*`) moved off `UserId` to `TenantScopedRepository.CurrentTenant` (new repo methods
  `GetIdForCurrentTenantAsync`/`GetDetailsForCurrentTenantAsync`; the dead `GetByUserIdAsync` removed).
  `CreateAsync`'s `IUserModule.GetManagerByIdAsync` "Manager not found" lookup became a
  `tenantContext.HasTenant` guard, with `UserId`/`Email` sourced from `ICurrentUser` (the B2B `ApiFixture`
  now sends the email header, matching production + Customer's fixture). The cross-module facades follow:
  `IVenueModule.GetVenueIdForCurrentTenantAsync`, `IArtistModule.GetIdForCurrentTenantAsync`.
- **Concert**: `OpportunityService` resolves "my venue" via the tenant facade and checks opportunity
  ownership by `TenantId`; `ApplicationService` resolves "my artist" via `GetIdForCurrentTenantAsync`;
  `ApplicationValidator` swapped `ICurrentUser` for `ITenantContext`. Dead `GetWithVenueByIdAsync` removed
  and the now-needless `Venue` include dropped from `GetByApplicationIdAsync`.
- **Dependency severed**: Artist/Venue (Application + Infrastructure) no longer reference the User module
  at all — `IUserModule` usage, the `User.Contracts` global usings, and the `User.Contracts`/`User.Application`
  project references are gone. `GetOwnerByIdAsync` (notification owner) and `SetupCheckoutStep`'s
  `GetManagerByIdAsync` (payout owner) are left for Phase 5/8 — they're attribution/payout, not ownership.

### Phase 4 — Active-tenant resolution + multi-membership ✅ *(no re-scaffold; minimal frontend)*

> **Shipped** on `Feature/active-tenant-resolution` (off `Feature/permission-policies`). Build green; affected
> suites pass — `Tenant.UnitTests` (53, incl. header valid/unowned/malformed + single-default + multi-membership
> fail-closed) and the integration suites that drive the real ASP.NET pipeline: `Tenant` (15, incl. the new
> header-switched isolation set), `User` (3). Re-ran `Venue` (25), `Artist` (17), `Concert` (63) — the middleware
> reorder is behaviour-preserving for the single-membership seed graph. All four web builds green. E2E skipped by
> policy: Phase 4 isn't in the massive/risky set (§8) and the frontend change is zero-UI (the interceptor sends a
> header only when a tenant is selected, and nothing selects one until the Phase 6 switcher).
>
> **Deviations from the design above:**
> - **Resolution point:** chose the "one obvious point" option — `TenantResolutionMiddleware` now runs *between*
>   `UseAuthentication` and `UseAuthorization`, so it (not the permission handler) is the trigger for every
>   request; the handler's own memoized `ResolveAsync` then no-ops. The middleware was also converted to a
>   factory-activated `IMiddleware` (registered scoped).
> - **Owner-claim default:** ordering/predicates over the join-projected `UserMembership` don't translate in EF,
>   so the founding-first default is a dedicated `GetDefaultTenantIdAsync` (orders on the membership's own
>   columns, selects just the id — no join). The general `GetMembershipsAsync` is therefore *unordered* (its two
>   callers — the single-default count check and the `/me` list — don't need order).
> - **`/me` shape:** memberships are additive as `UserBase.Memberships` (populated only by `/me`, empty for
>   cross-module reads), which adds a `User.Contracts → Tenant.Contracts` reference. Retired with the polymorphic
>   DTOs in Phase 7.
> - **Multi-membership test data** is arranged per-test (a fixture write helper), never added to the global seed —
>   the seed graph must stay one-membership-per-operator so the no-header single-default keeps every other suite
>   green. The isolation test drives `/api/organizations` (a `[Authorize]`-only endpoint), so it exercises the
>   *middleware* resolution path specifically.
> - **`owner` claim** stays single-valued via `GetDefaultTenantIdAsync` (founding first) — fully resolved in Phase 5.

### Phase 5 — Payment proxy + kill the `owner` claim *(no re-scaffold; frontend base-URL swap)*

Must land **before invitations create real multi-tenant users** — the per-token `owner` claim goes
ambiguous/stale the moment a user has two tenants or switches mid-session.

- B2B payouts controller per §5; Payment gRPC surface gains the missing RPCs; B2B SPAs swap the
  payout feature from the Payment host to their own service's client.
- Payment's `StripeAccountController` stays for Customer flows (`GetOwnerId() → sub` fallback —
  verify `CustomerProfileClaimsProvider` doesn't mint `owner` before relying on it).
- `UserClaimsController` stops emitting `owner`; `ICurrentUser.Owner`/`GetOwnerId()` survive only
  until Phase 7 cleanup for the Customer fallback path.
- E2E: `StripeE2EAccountResolver` already keys on `TenantSeedIds` (TS Phase 3) — unchanged; run the
  payout UI scenarios in the baseline after the base-URL swap.

### Phase 6 — Invitations + member management + UI *(re-scaffold)*

- `Tenant.Domain/TenantInvitationEntity` + configuration; the §6 endpoints with last-Owner
  invariants; the `TenantProvisioningHandler` invitation-first branch (the one place the
  one-tenant-per-registration rule bends — inbox dedup already gives idempotency).
- Email via the existing `Concertable.Shared.Email` `IEmailSender`.
- Frontend: members/invite pages + tenant switcher in `app/web/b2b` (shared across venue/artist
  apps per the app-injected-slot composition rules), permission-driven gating of member-management
  UI.
- Seeding: invitations and invitation-derived memberships are **never seeded**.

Tests: Tenant integration suite (invite → accept → membership; Manager vs Owner permission
boundaries; last-Owner guards); API E2E covering invited registration via `TestTokenMinter` with a
fresh email; new UI E2E scenario(s) appended to `E2E_BASELINE.md` (additive — counts/summary per
the baseline's parser rules).

### Phase 7 — Retire `Role`, manager profile tables, the `role` claim *(re-scaffold)*

The cleanup sweep over every remaining `Role` site:

- Kernel: delete `Identity/Role.cs`; remove `IUser.Role` (dissolve `IUser` if the user DTOs were
  its only consumers); delete `ICurrentUser.Owner` + `GetOwnerId()` (re-point Customer's payment
  path to `sub` directly).
- B2B User module: drop `UserEntity.Role`; `CredentialRegisteredHandler` loses `RolesByClient`
  (keeps a manager-client-id filter) and stops writing manager profile rows — delete
  `VenueManagerProfileEntity`/`ArtistManagerProfileEntity` + configurations (**keep**
  `AdminProfileEntity`); collapse `UserDtos.cs` polymorphic
  `UserBase`/`AdminDto`/`VenueManagerDto`/`ArtistManagerDto` into one membership-shaped `Me` DTO;
  delete/merge the role mappers.
- Claims: `UserClaimsController` stops emitting `role` — if nothing remains, delete the endpoint,
  `B2BProfileClaimsProvider`, and the `user:claims` scope/client in Auth, completing the
  identity-only north star. Remove `RoleClaimType` from
  `B2B.Web/Extensions/ServiceCollectionExtensions.cs`.
- Customer: drop `CustomerDto.Role` (Customer's only `Role` use).
- Tests: `ApiFixture` client-creation overloads lose role parameters/headers
  (`TestAuthHandler.RoleHeader` deleted); `SeedState.UserFactory` signatures shrink; UI E2E step
  files (`LoginCaptureHooks`, `VenueManagerSteps`, `ArtistSteps`) updated.
- Frontend: `app/web/shared/src/features/auth/guards.ts` — `requireRole`/`requireBusinessRole`
  switch from `user.role` to persona/membership from the new `/me` shape; customer's
  `requireRole("Customer")` becomes a has-customer-profile check. All web workspace builds gate it.

### Phase 8 — Messaging group inbox *(re-scaffold)*

The §7 re-model: `MessageEntity` tenant-pair columns + `SentByUserId`; the Bucket-B two-party
filter (TS Phase 4 mechanics); `ThreadReadStateEntity`; `MessagesRead`/`MessagesSend` gates on the
`MessageController` endpoints (which carry no guards today); notification routing reads membership +
preferences. Update the Conversations integration suite and the messaging UI E2E scenarios.

## 9. Sequencing interplay

- **TS Phase 4 hard-blocks Phase 3** (no `ArtistEntity.TenantId` until then) and Phase 8 (two-party
  filter mechanics); running the RBAC ownership sweep concurrently with TS-4's
  `AcceptExecutor`/settlement rewrite would collide in `ApplicationService`/`OpportunityService`.
- **TS Phase 5 lands first** to avoid interleaved `TenantEntity` re-scaffolds; Phase 6's tenant
  controller is the natural home for its settings UI (`TenantSettingsEdit`).
- **AUTH_IDENTITY_REFACTOR**: structurally complete — refresh its status doc at Phase 1; Phases 5
  and 7 here complete its identity-only north star by deleting the `owner` then `role` claims.

## 10. Risks

- **Stale token claims.** The claims provider caches per `sub`; today a role change wouldn't
  propagate for up to token lifetime. From Phase 2 every authorization decision reads DB membership
  per request, eliminating the class; the residue is the `owner` claim (dies Phase 5) and SPA-side
  `user.role` (dies Phase 7, refetched per session anyway).
- **E2E baseline churn.** Phases 2, 5, 6, 7 touch login/registration/payout surfaces — run the
  baseline regress as each phase's exit gate; only 6 (new scenarios) and 7 (steps referencing
  `Role`) should need baseline edits, both mechanical.
- **Seeding order.** Memberships must exist before any authenticated test request: seed them with
  tenants (after Auth credentials); the provisioning handler must stay idempotent over pre-seeded
  membership. Invitation-derived memberships are never seeded.
- **Invited-registration race.** Keep invitation-matching and tenant-provisioning in the single
  inbox-deduped `TenantProvisioningHandler` transaction — never a second consumer of
  `CredentialRegisteredEvent` for this.
- **Owner-claim removal breaking Customer checkout.** The `GetOwnerId() → sub` fallback is the
  customer path — verify before deleting anything; B2B payout flows must be fully proxied (Phase 5)
  before the claim disappears or payout settings silently miss (`GetByOwnerIdAsync` returns null).
- **Re-scaffold churn.** Four phases re-scaffold (1, 6, 7, 8). Run `./initial-migrations.ps1` only
  at those boundaries and re-run the integration suites immediately — migration drift is the most
  common "ends red" cause in this repo.

## 11. Decision log — considered and rejected

- **Pure RBAC (role-name checks at endpoints)** — scatters role knowledge across every controller;
  adding Finance/Door means touching every endpoint. Permissions invert that dependency.
- **`Permission` enum + N policies registered in a startup loop** — works, and is leaner for a closed
  set, but switched to the idiomatic modern shape recognized across .NET codebases: `string` constants
  + a custom `IAuthorizationPolicyProvider` building `perm:<name>` policies on demand (delegating
  default/fallback to `DefaultAuthorizationPolicyProvider`). The permission string *is* the policy name
  and the identifier never serializes (identity-only tokens; DB stores the role, not permissions), so
  the string form costs nothing the enum saved. Tradeoff: the attribute argument is no longer
  compile-checked — covered by the catalog-coverage test (§Phase 2). Footgun owned: the provider must
  delegate `GetDefaultPolicyAsync`/`GetFallbackPolicyAsync`, or the `Admin` policy and bare
  `[Authorize]` break.
- **ABAC / policy-expression engine** — six roles × ~18 permissions is a lookup table, not a rules
  engine; nothing in the domain is attribute-rich enough to justify one.
- **Resource-scoped grants (per-venue/per-concert ACLs)** — every resource is tenant-owned; tenant
  scoping + permission answers every requirement; a second authority store has no use case.
- **DB role→permission tables / per-tenant custom roles** — admin-UI, seeding, and re-scaffold cost
  for a feature nobody asked for; the static map is testable and call-sites never change if this is
  revisited.
- **Multiple roles per membership** — union semantics complicate the matrix and members UI; real
  combos collapse into Manager/Owner anyway.
- **Membership in the User module** — User is an Auth projection; authority data there re-entangles
  what AUTH_IDENTITY_REFACTOR just separated.
- **Persona derived from owning Venue/Artist rows** — the tenant predates its first profile row;
  persona must drive UI and permission semantics from first login. Hence `TenantType` at
  provisioning.
- **Tenant/role/owner claims in access tokens** — 900s staleness on role change/removal; violates
  the identity-only mandate; one claim can't express N memberships. The `owner` claim dies.
- **Route-segment tenant (`/tenants/{id}/...`)** — full URL churn across controllers and three
  frontends for zero security gain over a membership-validated header.
- **Payment verifying membership by calling B2B** — adapter→data-service runtime coupling; dies at
  repo split; inverts the dependency rule. Hence consumer-fronted endpoints with opaque `ownerId`.
- **Auth-minted short-lived tenant-scoped tokens** — puts authority back in tokens and couples Auth
  to B2B authority data; every service would need to validate a second token shape.
- **Per-message-per-user read rows** — row explosion; a thread-level read pointer suffices for
  unread badges.
- **Keeping manager profile tables alongside memberships** — two competing authority sources; the
  `VenueId`/`ArtistId` linkage is superseded by `Venue/Artist.TenantId`.
- **Cutting Door/Sound until day-of-show features exist** — considered (they gate nothing live
  today); kept as catalog entries with reserved permissions because the cost is one enum value +
  one dictionary entry each, and the members UI needs safe low-privilege defaults for invitees.
