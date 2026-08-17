# Admin console progress

- Plan: `plans/launch/ADMIN_CONSOLE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/admin-console`
- Worktree: current checkout (`C:\Users\tommy\source\repos\Concertable`)
- Branch: `Feature/launch_admin-console`
- PR: [#624](https://github.com/Concertable/concertable/pull/624) (reviewed, CI green, merge blocked — see below)
- Dependency/package gates: none. No published-package boundary crosses this plan (Auth + B2B edits land
  in the same repo, no NuGet republish/platform-sync gate).
- Last reconciled: 2026-08-16, `/review` + `/security-review` complete and clean (one test-coverage
  finding fixed inline); merge attempt surfaced an unrelated hook bug — see Blocked below.

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

Since then: ran `/review` on #624 (native + security layers) — no findings above the confidence bar
except one test-coverage gap (`AdminService.GetOverviewAsync`/`IsCurrentUserAdminAsync`/
`RevokeInvitationAsync` had no unit test; fixed inline, 6 tests added, 26/26 green) and one sub-8-confidence
security note (recorded above in Next Steps, Phase-2 pre-flight). `reviews/Feature-launch_admin-console.md`
is clean (zero open findings) and its markers are current at HEAD.

Blocked: `gh pr merge 624 --auto` is refused by `.claude/hooks/merge-review-gate.py`'s security-marker
check, which has a real bug — it requires `Security-reviewed up to commit:` to exactly equal HEAD, but
the commit that stamps the marker always creates a new HEAD, so the marker can never literally equal its
own commit's hash (the primary `Reviewed up to commit:` check has a `review_only` tolerance for exactly
this; the security check never got the same tolerance, so it's unsatisfiable by construction for any
security-sensitive branch, not just this one).
Blocked by: needs Tommy to choose how the hook fix lands — approve the one-line `review_only` tolerance
directly, edit it himself, or fold it into #624 instead of a separate `Fix/` branch. Asked via
`AskUserQuestion`; awaiting his answer (he asked to clarify the question first).
Unblock action: Tommy answers/clarifies, then whichever path he picks gets executed and #624 retried.
Resume when: the hook fix lands (however he chooses) and `gh pr merge 624 --auto` succeeds, or Tommy
merges #624 by another route.

## Next Steps

1. Once #624 is reviewed and merged, start Phase 2 (admin console SPA shell) per
   `plans/launch/ADMIN_CONSOLE_PLAN.md` "Phase 2" from a fresh worktree based on current `origin/main`:
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
2. Phases 3 (moderation UI) and 4 (venue approval UI, plus the new `GET /api/Venue/pending-approval`
   endpoint) follow once Phase 2 is green — see the plan for scope.

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

None yet — PR #624 is ready for review, CI green, awaiting Tommy's review.

## Decisions, discoveries, blockers, and deviations

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
