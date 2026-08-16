# Admin console progress

- Plan: `plans/launch/ADMIN_CONSOLE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/admin-console`
- Worktree: current checkout (`C:\Users\tommy\source\repos\Concertable`)
- Branch: `Feature/launch_admin-console`
- PR: not opened
- Dependency/package gates: none. No published-package boundary crosses this plan (Auth + B2B edits land
  in the same repo, no NuGet republish/platform-sync gate).
- Last reconciled: 2026-08-16, plan authored fresh this session.

## Current state

Plan and ledger just written; no implementation yet. Branch created off `origin/main`, clean.

## Next Steps

Start Phase 1 (backend provisioning) per `plans/launch/ADMIN_CONSOLE_PLAN.md` "Phase 1":

1. Add `AdminInvitationEntity` to `Concertable.B2B.User.Domain` (mirror `TenantInvitationEntity`'s
   shape minus tenant fields — Id/Email/Status/CreatedByUserId/CreatedAt/ExpiresAt, 7-day TTL).
2. EF configuration + repository access (extend `UserDbContext`/`UserRepository`, no new module).
3. Update `CredentialRegisteredHandler`
   (`api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Events/CredentialRegisteredHandler.cs`)
   per design decision 1: invitation-or-bootstrap gate on the `ClientIds.Admin` branch. Inject
   `TimeProvider` + a bootstrap-email option (`Admin:BootstrapEmail` config key, defaulting to
   `SeedUsers.AdminEmail` outside Production).
4. New `AdminController` in `Concertable.B2B.User.Api/Controllers/` + `IAdminService`/`AdminService` in
   Infrastructure: `POST /api/AdminInvitation`, `DELETE /api/AdminInvitation/{id}`, `GET /api/Admin`,
   `DELETE /api/Admin/{sub}` (last-admin invariant, mirror `MembershipService.IsLastOwnerAsync`),
   `GET /api/Admin/me`.
5. Invite email via the existing outbox pattern (`TenantInvitationCreatedDomainEventHandler` template).
6. `./initial-migrations.ps1` from `api/` to re-scaffold the User module's migration.
7. Tests per the plan's Phase 1 verification gate — the load-bearing one is: registering via
   `ClientIds.Admin` with no matching invitation and a non-bootstrap email must create a `UserEntity`
   but grant **no** `AdminProfileEntity`.
8. Commit when green; push to open the draft PR (first coherent checkpoint).

Then proceed to Phase 2 (SPA shell) in the same or next session — see the plan for its scope. Phases 3-4
follow once Phase 2 is green.

## Completed work

None yet.

## Verification

None yet.

## Reviews

None yet.

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

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable
Read @plans/launch/ADMIN_CONSOLE_PLAN.md and @plans/launch/ADMIN_CONSOLE_PROGRESS.md and do what its `## Next Steps` says.
```
