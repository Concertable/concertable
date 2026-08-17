# Admin console progress

- Plan: `plans/launch/ADMIN_CONSOLE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/admin-console`
- Worktree: none; Phase 1's merged worktree is no longer registered and the continuation worktree has not been created
- Branch: none for the remaining work; Phase 1 branch `Feature/launch_admin-console` merged
- PR: [#624](https://github.com/Concertable/concertable/pull/624) — **MERGED** (`7fd40bf59860c27f1c1d1e48537901b022de0f43`, 2026-08-17T14:18:26Z); Phases 2-4 have no PR yet
- Dependency/package gates: none. No published-package boundary crosses this plan (Auth + B2B edits land
  in the same repo, no NuGet republish/platform-sync gate).
- Last reconciled: 2026-08-17 after the repository clarified that reviewability alone does not split a plan

## Current state

Phase 1 (backend provisioning) implemented as PR #624. First CI run surfaced one genuine failure (the
rest of the matrix's failures were cascading cancellations from that same run, not separate bugs):
`Registration_BootstrapEmail_GrantsNoAdminProfile_WhenAnAdminAlreadyExists` fired a
`CredentialRegisteredEvent` for a new userId using the exact email of the already-seeded admin
(`SeedUsers.AdminEmail`) — a collision real registration can never produce (Auth enforces global email
uniqueness) — which hit the `Users.Email` unique index. Fixed by freeing the email via
`ClearAdminsAsync()` and provisioning a distinct admin first, so the test isolates the
AdminProfiles-non-empty gate instead of an artificial collision. Branch was also 73 commits behind
`origin/main`; merged clean (no conflicts), full B2B solution rebuilds green post-merge. Second CI run:
all 54 required checks passed (5 E2E jobs correctly skipping per this plan's phase scope). PR marked
ready for review.

Ran `/review` on #624 (native + security layers), then a further round driven by Tommy's own
line-by-line read of the diff: a repository-boundary fix (extracted `IAdminRepository` from
`IUserRepository`), a self-check redesign (`AdminController.Me()` deleted, `IsAdmin` folded into
`UserDto`/`GET /api/auth/me` instead), an `InsertAsync` cleanup (logged as recurring debt across five
other services in `api/Concertable.B2B/TECH_DEBT.md`), a C# convention fix (`AdminMappers.cs` → C# 14
`extension()` blocks), and an error-design fix (`RevokeAdminInvitationError` now wraps
`AdminInvitationRevocationError` as a composite case instead of duplicating it). Merge-queue CI then
caught one genuine build break: the platform package `0.1.0-alpha.0.1049` (published mid-review, from
the in-flight repository-context-permission-hierarchy refactor) dropped `Repository<T,TContext,TKey>`'s
protected `context` field — every other repository in the codebase had already been migrated by the
platform-sync bot's consumer-fix step, but `AdminRepository` wasn't on `main` yet so it was missed.
Fixed by hand. Separately found and fixed `chore/platform-sync-0.1.0-alpha.0.1049` (#634) stuck with
`autoMergeRequest: null` (a transient GitHub API 503 in `platform-sync.yml`'s one-shot auto-merge call,
no retry) — merged #634 by hand to unblock, then shipped the actual retry fix as
[#640](https://github.com/Concertable/concertable/pull/640) (auto-merge armed, landing independently).
All unit tests green (26/26). `reviews/Feature-launch_admin-console.md` is clean (zero open findings).

**#624 merged.** One sub-8-confidence security note from the review was NOT closed before merging —
it's inert until Phase 2 registers the `admin` OIDC client, and is carried below as a Phase 2 pre-flight
item, not a Phase 1 blocker.

## Next Steps

Create one fresh continuation worktree from current `origin/main` and implement Phases 2-4 together on
one complete draft PR. Begin with the Phase 2 security pre-flight below, then build the admin SPA,
moderation UI, and venue-approval UI without a phase merge stop:

1. Build the admin console SPA shell and provisioning UI:
   - `app/web/admin/` scaffold (mirrors the `customer` app's shape, no `@b2b/*` alias).
   - Routes: `login.tsx`, `auth.callback.tsx`, `__root.tsx`, `_admin/route.tsx` (guard via
     `GET /api/auth/me`, reading `UserDto.IsAdmin`), landing page listing admins + pending invitations
     wired to Phase 1's `AdminController` endpoints.
   - Auth service: `ClientIds.Admin` ("admin") Duende Web client — `Config.WebClients` +
     `SpaClientSettings.Admin` + redirect URIs in `appsettings*.json`. This is what makes Phase 1's
     fail-closed gate load-bearing (today `admin` has no OIDC client, so the path is unreachable).
   - AppHost wiring: `AppHostExtensions.AddAdminSpa` (mirrors `AddCustomerSpa`), called from
     `Concertable.B2B.AppHost/Program.cs` and the umbrella `Concertable.AppHost/Program.cs`.
   - Verification gate: all five web builds green; focused component/hook tests for invite/revoke.
   - **Security pre-flight (found in #624's `/review`/`/security-review` pass):** design decision 2
     claims bootstrap "reuses the same email-ownership proof every registration already relies on:
     Auth's existing email-verification flow" — verified against the shipped `AuthService.RegisterAsync`/
     `CredentialEntity` code that this is **not actually true**: `CredentialRegisteredEvent` fires at
     registration submit time, before `IsEmailVerified` is ever set, and carries no verified-status
     field. `GrantAdminIfEligibleAsync` (`CredentialRegisteredHandler.cs`) grants off the raw event
     email with no verification check. This is provably inert in Phase 1 (no `admin` OIDC client
     exists yet, so `RegisterAsync` can never be invoked with `client_id=admin`), which is why the
     `/security-review` pass scored it 5/10 (below the blocking bar) — but the moment Phase 2 registers
     the client, the gap goes live: anyone who knows the bootstrap email or an invited admin's email
     could self-register with it before the real owner does, consuming the one-time bootstrap slot or
     the invitation, and (via Auth's global email-uniqueness check) permanently blocking the legitimate
     admin from ever registering that email. **Close this before or as part of Phase 2** — either gate
     `GrantAdminIfEligibleAsync` on a verified-email signal (Auth would need to carry/expose one at
     registration time, which it currently doesn't for any client), or require the admin SPA's login
     flow to force an explicit verify-then-retry step before the grant becomes reachable. Full finding:
     `reviews/Feature-launch_admin-console.md` (SEC layer note, below the 8-confidence bar for a
     blocking finding but real).
2. In the same PR, complete Phase 3 moderation UI and Phase 4 venue approval UI, including the new
   `GET /api/Venue/pending-approval` endpoint. Phase checkpoints must remain green, but none is an
   independent delivery candidate.

## Completed work

- **Phase 1 — Admin provisioning backend** (PR #624): `AdminInvitationEntity` (User.Domain, mirrors
  `TenantInvitationEntity` minus tenant fields) with Accept/Expire/Revoke transitions and a 7-day TTL;
  EF configuration + `IAdminRepository`/`AdminRepository` (own repository, bound to
  `AdminInvitationEntity` — not folded into `UserRepository`; no new module, `UserDbContext` shared);
  `CredentialRegisteredHandler` rewritten with the invitation-or-bootstrap gate on `ClientIds.Admin`
  (`Admin:BootstrapEmail` config, defaults to `SeedUsers.AdminEmail` outside Production);
  `IAdminService`/`AdminService` (mirrors `InvitationService`/`MembershipService`, last-admin invariant
  mirrors `IsLastOwnerAsync`); `AdminController` (`[Admin]`-gated at the class level:
  `POST`/`DELETE /api/AdminInvitation`, `GET`/`DELETE /api/Admin`); `UserController.Me()`
  (`GET /api/auth/me`) now also returns `UserDto.IsAdmin`, backed by
  `IAdminService.IsCurrentUserAdminAsync` — no separate `AdminController` self-check endpoint (see the
  plan's design decision 3 for why `VenueController.IsOwner` was the wrong precedent to mirror here);
  invite email via the existing outbox/`IBus` pattern; `./initial-migrations.ps1` re-scaffold for the new
  `AdminInvitations` table.

## Verification

- `dotnet build` on `Concertable.B2B.Web` (full host) and every touched project: green.
- `Concertable.B2B.User.UnitTests`: 20/20 passing — `AdminInvitationEntityTests` (Create/Accept/Revoke/
  Expire/IsActive transitions) + `AdminServiceTests` (last-admin invariant, invite conflict cases).
- `Concertable.B2B.User.IntegrationTests.AdminProvisioningTests` (new `UserApiFixture` in the shared
  fixtures project): compiles clean; covers invitation-matched grant, case-insensitive email match,
  expired-invitation non-grant, bootstrap-email grant only when `AdminProfiles` is empty, bootstrap
  email non-grant once an admin exists, no-invitation/non-bootstrap registration (UserEntity created,
  no AdminProfileEntity), non-admin-client no-op, and inbox-dedup idempotency on redelivery. Could not
  execute locally (no Docker in this environment) — deferred to draft-PR CI per the remote-validation
  policy; existing `ModerationApiTests`/venue-approval integration tests are untouched (they seed
  `AdminProfileEntity` via `ITestSeeder`, not through the handler).

## Reviews

Tommy reviewed the diff line-by-line and raised four design questions, all resolved: `IUserRepository`
mixing entities (extracted `IAdminRepository`), error-mapping placement (kept as an extension in its own
`AdminErrorMappers.cs`, since `ToDto` is genuinely reused but the error mapper isn't), the `Me()`
self-check endpoint (folded into `UserDto.IsAdmin`/`GET /api/auth/me`), and an `AddAsync`+`SaveChangesAsync`
pair that should have been `InsertAsync`. `reviews/Feature-launch_admin-console.md` (deleted post-merge
per its own lifecycle policy — all findings resolved and the PR merged).

## Decisions, discoveries, blockers, and deviations

- Remaining Phases 2-4 land together. There is no package, publication, deployment, or runtime gate
  between them, so reviewability is not a reason for another partial PR.
- Confirmed via `AdminProfileHandler` that the `"Admin"` policy is checked entirely on B2B's side
  (`AdminProfiles.Sub == sub` claim), not via any Auth-side role/claim — so provisioning is a B2B-only
  concern; Auth's `RegisterAsync` needs no changes.
- Confirmed `AuthService.RegisterAsync` resolves `clientId` from the OIDC authorize context
  (`interaction.GetAuthorizationContextAsync(ReturnUrl)?.Client?.ClientId`), not a caller-supplied value
  — today's registration is unreachable for `ClientIds.Admin` only because no Duende client named
  `"admin"` exists yet. Adding one (Phase 2) is what makes the Phase 1 fail-closed gate load-bearing;
  ship Phase 1 before or together with Phase 2, never Phase 2 alone.
- Confirmed integration tests for `ModerationController`/venue approval seed `AdminProfileEntity`
  directly via `ITestSeeder` (`fixture.SeedState.Admin`), not through `CredentialRegisteredHandler` — so
  Phase 1's handler change doesn't touch their fixtures.
- Confirmed B2B's production startup currently runs **no** `IDbInitializer` at all
  (`Concertable.B2B.Web/Program.cs`: the initializer only runs `if (!app.Environment.IsProduction())`)
  — so the bootstrap mechanism deliberately avoids depending on any "runs at B2B startup in production"
  hook (none is proven to exist yet) and instead triggers lazily inside the existing
  `CredentialRegisteredHandler` reactive path, which already runs in every environment.
- The invite email deliberately carries **no** accept link — unlike `TenantInvitationCreatedDomainEventHandler`,
  admin acceptance is implicit at registration-time email match (design decision 1), and the admin
  console's real URL doesn't exist until Phase 2. The email just tells the invitee which email to
  register with; revisit once Phase 2 gives it a real base URL, if a link becomes worth adding.
- `AdminProfileEntity` stayed in `Concertable.B2B.User.Infrastructure` (not moved to Domain) —
  `IUserRepository`'s new admin members return only primitive `Guid`/`bool` values, never the entity, so
  no Application-layer type ever needs to see it. `AdminInvitationEntity` did move to Domain per the
  plan's explicit instruction, since `AdminService`/`IUserRepository` legitimately pass it across the
  Application boundary (mirrors `TenantInvitationEntity`).
- Needed three new `[assembly: InternalsVisibleTo(...)]` grants beyond what the plan called out:
  `Concertable.B2B.User.Infrastructure` → `Concertable.B2B.User.UnitTests` (to unit-test `AdminService`
  and dispatch `CredentialRegisteredHandler` directly in integration tests) and → `Concertable.B2B.IntegrationTests.Fixtures`
  + `Concertable.B2B.User.IntegrationTests`; `Concertable.B2B.User.Application` → `DynamicProxyGenAssembly2`
  (Moq needs this to mock the internal `IUserRepository`, same as every other module's `.Application`
  assembly already grants).
- Added `UserApiFixture` to the shared `Concertable.B2B.IntegrationTests.Fixtures` project (mirrors
  `TenantApiFixture`) — exposes `AdminInvitations`, `IsAdminAsync`, `AddAdminInvitationAsync`, and
  `ClearAdminsAsync` (removes both the seeded `AdminProfileEntity` *and* its `UserEntity` row, since the
  seeded admin's email — `SeedUsers.AdminEmail` — collides with the default bootstrap email and `Users.Email`
  has a unique index). `UserApiTests`/`IntegrationCollection` now use it in place of the base `ApiFixture`.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable
Read @plans/launch/ADMIN_CONSOLE_PLAN.md and @plans/launch/ADMIN_CONSOLE_PROGRESS.md and do what its `## Next Steps` says.
```
