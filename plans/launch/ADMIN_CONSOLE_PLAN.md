# Admin Console + Production Admin Provisioning Plan

**Next steps live in @plans/launch/ADMIN_CONSOLE_PROGRESS.md → `## Next Steps`**

## Context

Two already-shipped backend surfaces have no way to be driven in production:

- OSA moderation — `ModerationController` (`api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Api/Controllers/ModerationController.cs`): `[Admin]`-gated report queue, hide/restore message, resolve report.
- Venue approval — `VenueController.Approve` (`api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Api/Controllers/VenueController.cs`): `[Admin]`-gated `PATCH {id}/approve`. `VenueEntity.Approved` is otherwise decorative (enforcement is `launch/tenant-verification`, out of scope here).

Both are gated on the B2B `"Admin"` authorization policy, which succeeds only when the signed-in
user's `sub` claim has a matching row in `AdminProfiles`
(`api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Authorization/AdminProfileHandler.cs`).
That row is written by `CredentialRegisteredHandler`
(`api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Events/CredentialRegisteredHandler.cs`)
**unconditionally** whenever an Auth credential registers with `ClientIds.Admin` — and the only thing
that ever does that today is `AuthDevSeeder` (dev-only, hardcoded password). There is no admin SPA
(`app/web/` is `b2b/{artist,business,venue}`, `customer`, `shared` — `business` is the marketing
gateway page, not a manager app) and no production path to create or rotate an admin credential.

This plan builds both: a real provisioning mechanism, and the console SPA that drives it plus the two
existing backends. Downstream: unblocks `launch/tenant-verification` (evidence-review UI) and the
post-launch admin-audit-log item. Out of scope: tenant-verification's evidence upload/state machine,
the audit log itself.

## Design decisions

### 1. Provisioning is invitation-gated and fail-closed — but the grant fires post-login, not at registration

The unconditional grant in `CredentialRegisteredHandler` is the actual security hole this plan closes.
Today it's inert because no Duende client named `admin` exists, so nobody can reach
`/connect/authorize?client_id=admin` to register one. The moment this plan adds that client for the
admin SPA's login, `AuthService.RegisterAsync` (`api/Concertable.Auth/src/Concertable.Auth/Services/AuthService.cs`)
— which accepts **any** `clientId` resolved from the OIDC authorize context with no allow-list — becomes
a self-serve "become an admin" endpoint. Auth is identity-only by design (`api/Concertable.Auth/AGENTS.md`)
and doesn't know B2B's admin concept, so the fix belongs where authority already lives: B2B, not Auth's
registration.

New `AdminInvitationEntity` in the User module (`Concertable.B2B.User.Domain`), same shape as
`TenantInvitationEntity` minus the tenant fields: `Id` (Guid), `Email` (normalized), `Status`
(Pending/Accepted/Revoked/Expired), `CreatedByUserId`, `CreatedAt`, `ExpiresAt` (7-day TTL, matching
`InvitationService`'s `InvitationTtl`). Created via a new `[Admin]`-gated `POST /api/AdminInvitation`
(an existing admin invites the next one by email).

**The grant does not run inside `CredentialRegisteredHandler`.** An initial design put it there
(`ClientIds.Admin` branch, unconditionally-adjacent to `UserEntity` creation), reasoning it "reuses the
existing reactive path." That design shipped in #624 and was corrected before Phase 2 wired the client:
`CredentialRegisteredEvent` fires at raw registration *submission*, before the account's email is ever
verified (`CredentialEntity.IsEmailVerified` defaults `false`; `SendEmailVerificationAsync` runs *after*
the event already published). Granting off it means anyone who knows the bootstrap email or an invited
admin's email can register with it first — unverified — and get admin, or at minimum permanently consume
the one-time bootstrap slot/invitation via Auth's global email-uniqueness check.

The grant instead runs from `AdminService.EnsureCurrentUserAdminGrantedIfEligibleAsync`, called from
`UserController.Me()` (`GET /api/auth/me`) — the first authenticated request every SPA makes right after
login. This is guaranteed post-verification with **zero cross-service contract changes**: Auth's own
`CredentialEntity.CanAuthenticate` already requires `IsEmailVerified`, so an unverified account cannot
successfully log in at all, and therefore never reaches `Me()`. `CredentialRegisteredHandler` keeps doing
exactly one thing for the admin client: create the plain, inert `UserEntity` row (same as every other
manager client) — no different from Venue/Artist registration, and unaffected by whether the email is
ever verified.

The eligibility rule itself is unchanged, just relocated and now backed by `IAdminRepository` instead of
a raw `DbContext`:

1. a pending, unexpired `AdminInvitationEntity` exists for the caller's email (accept it, grant the
   profile), or
2. `AdminProfiles` is empty **and** the caller's email matches a configured bootstrap email (see below) —
   the one-time first-admin path.

Otherwise the call is a no-op — the account stays a plain, inert `UserEntity` with no `AdminProfileEntity`.
Self-registering through the admin client with no invitation and no bootstrap match is provably a no-op
for authority; the admin SPA additionally never links to the generic sign-up page (UX only, not the
security boundary).

There is no explicit "accept" HTTP call (unlike `InvitationController.Accept`) — acceptance is implicit
in the email match at first-login time. This is simpler than the tenant-invite flow and needs no new
endpoint. A legitimate invitee needs no extra step beyond what they'd do anyway: they must verify their
email to log in at all, and logging in is exactly what triggers the grant.

**Residual, explicitly out-of-scope risk:** an attacker can still *register* (never verifying) with the
bootstrap or an invited email, permanently squatting it via Auth's global email-uniqueness check and
blocking the real owner from ever registering that exact address. Neither this design nor a
`CredentialRegisteredEvent` contract change touches this — it's a pre-existing, platform-wide property of
every registration (venue/artist/customer signup has the identical shape), not something this plan
introduces. Treated as accepted, matching the security review's own non-blocking conclusion; not solved
here.

**Existing-user caveat:** `AuthService.RegisterAsync` checks email uniqueness globally, not per client
(`context.Credentials.AnyAsync(c => c.Email == email)`), matching existing Venue/Artist behavior. An
admin invite therefore needs a **not-already-registered** email — same constraint every manager signup
already has. Not solved here; a person who wants to be both e.g. a venue manager and an admin needs two
emails. Fine for the MVP; not worth a redesign for a handful of platform operators.

### 2. Bootstrap needs no secret store — it's a configured, non-secret email address

The very first admin has nobody to invite them. Rather than inventing a shared-secret bootstrap token
(which would need a real secret store — the still-open `launch/production deployment + config/secrets`
gate, planned in `plans/platform/CONFIG_AND_DEPLOYMENT_PLAN.md` but not yet built), bootstrap reuses the
same email-ownership proof every registration already relies on: Auth's existing email-verification
flow. This is actually enforced (not just aspirational) because the bootstrap match runs from
`AdminService.EnsureCurrentUserAdminGrantedIfEligibleAsync`, reachable only via an authenticated request
— and `CredentialEntity.CanAuthenticate` already requires `IsEmailVerified`, so no unverified account can
ever reach it. See design decision 1 for the full mechanism and why an earlier draft that granted inside
`CredentialRegisteredHandler` didn't actually have this property.

`Admin:BootstrapEmail`, read via `IConfiguration` in `AdminService`'s composition
(a small bound options type is fine) — plain non-secret config, no different in posture from every
other `IConfiguration`-sourced value in this codebase today (e.g. `ServiceAuth:*ClientSecret`). In
non-Production environments, default it in code to `SeedUsers.AdminEmail`
(`api/Concertable.Shared/src/Seed/Concertable.Seed.Identity/SeedUsers.cs`, `"admin@test.com"`) when the
config key is absent, so dev/E2E exercise the exact same fail-closed path `AuthDevSeeder` already relies
on with zero seeder changes. In Production, an absent key simply disables bootstrap (no crash — this
is optional config, not a required secret) until it's set as an operational step before the real
launch cutover. **No new secret-store dependency is introduced** — nothing to log in `TECH_DEBT.md`
beyond what the config/deployment plan already owns platform-wide.

### 3. Revocation and listing

- `GET /api/Admin` — list current admins (join `AdminProfiles.Sub` → email via the same
  `IUserModule.GetEmailsByIdsAsync` batch-join `InvitationService` already uses) + pending invitations.
- `DELETE /api/Admin/{sub}` — revoke an admin's rights (delete the `AdminProfileEntity`; the underlying
  Auth credential/`UserEntity` is untouched — this is a rights revocation, not an account deletion).
  Guarded by a **last-admin invariant** mirroring `MembershipService.IsLastOwnerAsync`
  (`api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/MembershipService.cs`):
  refuse to remove the last admin so the platform can never lock itself out.
- `DELETE /api/AdminInvitation/{id}` — revoke a pending invite.

No separate self-check endpoint: `IsAdmin` is a flat, unparameterized fact about the caller's identity —
the same shape as the `Memberships` list `UserController.Me()` (`GET /api/auth/me`) already attaches to
`UserDto` for every B2B app. It's folded into that existing response rather than given its own
`AdminController` endpoint (an earlier draft added `GET /api/Admin/me` mirroring
`VenueController.IsOwner`, but that's the wrong precedent — `IsOwner` is a *parameterized, per-resource*
check (`ownership of venue {id}`), not a flat identity fact; `Memberships` is the correct comparison).
`AdminController` is therefore `[Admin]`-gated at the class level, with no `[Authorize]`-only exception —
every action on it requires Admin.

### 4. Admin is a new top-level `app/web/admin` app — not a folder under `b2b/`

`app/web/b2b/shared` is explicitly scoped to "both manager apps" (venue + artist) and is full of tenant
machinery — `TenantChooser`, `TenantSwitcher`, memberships, opportunities, contracts — none of which an
admin has or needs. `@b2b/*` is aliased only in the venue/artist tsconfig/vite configs
(`app/web/b2b/shared/AGENTS.md`), so parking Admin there would either widen that tier's audience (the
anti-pattern the tier doc explicitly warns against) or fight the alias boundary. Admin's shape —
plain OIDC login, no tenant selection, calls a handful of `[Admin]`-gated B2B endpoints — is structurally
closer to `app/web/customer` (a standalone top-level app consuming only `@concertable/shared` +
`@concertable/web`) than to venue/artist. New sibling directory: `app/web/admin/`.

`AdminBootstrapExtensions.cs`/`AppHostExtensions.AddAdminSpa` follows the existing `AddCustomerSpa`
shape (`api/Concertable.Frontend.Hosting/AppHostExtensions.cs`) — no `tierSegments`, so it resolves to
`app/web/admin/`, not `app/web/b2b/admin/`.

Identity: the shared `User` type (`app/shared/src/features/auth/types.ts`) is already the flat
intersection with no persona subtypes (see `app/shared/AGENTS.md` — the old `Admin` union member was
already removed). Admin authority is fully server-enforced per request via `[Admin]`; the SPA needs no
composed identity layer at all (unlike B2B's `B2bIdentity`) — it reads the base `User` and calls
`GET /api/auth/me` (the same identity call every B2B app makes) for the route guard, reading
`UserDto.IsAdmin` off the response.

Auth wiring: a new Duende Web client, `ClientIds.Admin` ("admin", already defined in `ClientIds.cs`),
registered the same way as Venue/Artist — `Config.WebClients` gains an `Admin` case, `SpaClientSettings`
gains an `Admin: WebClientSettings` block, `appsettings.json`/`appsettings.Production.json` etc. gain the
redirect URIs. Scope is `openid profile concertable.b2b.api` (same as Venue/Artist — Admin calls B2B, not
a separate API resource).

Data layer: one `adminApi.ts` per the standard `xApi` pattern, one axios instance configured with
`.withAuth(...)` only — no `.withTenant(...)` (Admin has no tenant context; the builder already makes
that call optional).

## Phases

Phase 1 merged before the repository clarified its delivery rule. No package-publish, platform-sync,
or deployment boundary separates the remaining work, so Phases 2-4 build and test as checkpoints on
one complete continuation PR. Reviewability alone does not justify another partial merge.

### Phase 1 — Admin provisioning backend

- `AdminInvitationEntity` (Domain) + EF configuration + `IAdminRepository`/`AdminRepository`
  (`Concertable.B2B.User.Infrastructure` — its own repository per `api/agents/CODE_PATTERNS.md`'s "one
  repository per entity", not folded into `UserRepository`; no new module, `UserDbContext` is shared).
- `CredentialRegisteredHandler`: unchanged for the admin client beyond creating the plain `UserEntity` —
  the invitation-or-bootstrap gate lives in `AdminService.EnsureCurrentUserAdminGrantedIfEligibleAsync`,
  called from `UserController.Me()` post-login (design decision 1; corrected before Phase 2 wired the
  OIDC client, moving the gate out of the registration-event handler it originally shipped in).
- New `AdminController` (`Concertable.B2B.User.Api/Controllers/`), `[Admin]`-gated at the class level:
  `POST /api/AdminInvitation`, `DELETE /api/AdminInvitation/{id}`, `GET /api/Admin`,
  `DELETE /api/Admin/{sub}`. Service layer (`IAdminService`/`AdminService`) mirrors `InvitationService`'s
  shape; last-admin invariant mirrors `MembershipService.IsLastOwnerAsync`; `IsCurrentUserAdminAsync`
  backs `UserController.Me()`'s `UserDto.IsAdmin` field, not a separate endpoint (design decision 3).
- Invite email reuses the existing outbox `IEmailSender` pattern
  (`TenantInvitationCreatedDomainEventHandler` is the template).
- `./initial-migrations.ps1` re-scaffold for the new entity.
- **Verification gate:** unit tests for the last-admin invariant and the invitation entity's
  accept/expire/revoke transitions; integration tests proving (a) invitation-matched registration grants
  `AdminProfileEntity`, (b) bootstrap-email registration grants it when `AdminProfiles` is empty, and
  (c) — the load-bearing one — registration with **no** invitation and a **non-bootstrap** email creates
  a `UserEntity` but **no** `AdminProfileEntity`. Existing `ModerationApiTests`/venue-approval integration
  tests keep passing unchanged (they seed `AdminProfileEntity` via `ITestSeeder`, not through the
  handler, so this phase doesn't touch their fixture).

### Phase 2 — Admin console SPA shell + provisioning UI

- `app/web/admin/` scaffold: `package.json`, `vite.config.ts`, `tsconfig*.json`, `index.html`, matching
  the `customer` app's shape (no `@b2b/*` alias). OIDC client id is `ClientIds.Admin` (`"admin"`)
  verbatim, so the Duende client id and the SPA's `VITE_OIDC_CLIENT_ID` agree, exactly like Venue/Artist
  do today (`venue-web`/`artist-web` match `ClientIds.VenueWeb`/`ClientIds.ArtistWeb`).
- Routes: `login.tsx` (mirrors `app/web/b2b/venue/src/routes/login.tsx`), `auth.callback.tsx`,
  `__root.tsx`, `_admin/route.tsx` (guard: calls `GET /api/auth/me`, reads `UserDto.IsAdmin`, redirects
  non-admins), a landing page listing admins + pending invitations with invite/revoke actions wired to
  Phase 1's endpoints.
- Auth service changes: `Config.WebClients` + `SpaClientSettings.Admin` +
  `appsettings.json`/`appsettings.Production.json`/`appsettings.E2E.json` redirect URIs.
- AppHost wiring: `AppHostExtensions.AddAdminSpa` (mirrors `AddCustomerSpa`); called from
  `Concertable.B2B.AppHost/Program.cs` (alongside `AddVenueSpa`/`AddArtistSpa`/`AddBusinessSpa`) and the
  umbrella `Concertable.AppHost/Program.cs`.
- **Verification gate:** all four (now five) web builds green (`app/web/AGENTS.md`'s gate, extended to
  the new app); focused component/hook tests for the invite/revoke flow if the app's test setup supports
  it at this scope.

This phase alone closes the roadmap gap's headline: a real, driveable production admin-provisioning
path. Phases 3-4 wire the two already-shipped backends into the same shell.

### Phase 3 — Moderation UI

- Reports queue page (`GET /api/Moderation/reports`, paginated), hide/restore message actions, resolve
  report action — wired to the existing `ModerationController` verbatim (no backend changes).
- **Verification gate:** focused component/hook tests; manual smoke against the existing
  `ModerationApiTests` fixture data shape.

### Phase 4 — Venue approval UI

- Backend: `IAdminVenueRepository`/`AdminVenueRepository`
  (`api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Infrastructure/Repositories/AdminVenueRepository.cs`)
  currently only exposes `GetByIdAsync` — add a paginated "pending approval" query
  (`Approved == false`), a service method, and a new `[Admin]`-gated `GET` endpoint (e.g.
  `GET /api/Venue/pending-approval`) on `VenueController`. This is genuinely new backend surface, not
  just UI wiring — the roadmap's "unlocks the shipped backends" undersells this one endpoint's worth of
  gap.
- Frontend: pending-venues list page + approve action wired to the existing
  `PATCH /api/Venue/{id}/approve`.
- **Verification gate:** integration test for the new pending-approval query/endpoint; focused
  component/hook tests for the approve action.

## Non-goals

- Tenant-verification's evidence upload, state machine, or enforcement gate (`launch/tenant-verification`
  — downstream, separate plan).
- The admin action audit log (post-launch roadmap row).
- A real secret store for `Admin:BootstrapEmail` or anything else — that's
  `plans/platform/CONFIG_AND_DEPLOYMENT_PLAN.md`'s job; this plan's config reads are ordinary
  `IConfiguration`, consistent with (not worse than) everything else in the codebase today.
- Unifying one email across multiple B2B client roles (venue/artist/admin) — out of scope, see design
  decision 1's caveat.
