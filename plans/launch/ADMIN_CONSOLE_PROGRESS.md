# Admin console progress

- Plan: `plans/launch/ADMIN_CONSOLE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/admin-console`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch_admin-console`
- Branch: `Feature/launch_admin-console` (Phase 2 — new branch of the same name, Phase 1's was deleted on merge)
- PR: Phase 2: [#648](https://github.com/Concertable/concertable/pull/648) — **OPEN, CI RE-VALIDATING**
  at head `e9623af6d` after a genuine bug found in the previous draft-PR CI run and fixed locally (see
  reconciliation below); not yet merged — merge waits on Tommy's explicit instruction. Phase 1:
  [#624](https://github.com/Concertable/concertable/pull/624) — **MERGED**
  (`7fd40bf59860c27f1c1d1e48537901b022de0f43`, 2026-08-17T14:18:26Z)
- Dependency/package gates: none. No published-package boundary crosses this plan (Auth + B2B edits land
  in the same repo, no NuGet republish/platform-sync gate).
- Last reconciled: 2026-08-19. #648's branch had drifted 36 commits behind `origin/main` and GitHub
  reported it `DIRTY`/`CONFLICTING`. Merged `origin/main` in: the only real conflict was an additive
  clash in `app/package.json` between this branch's `build:admin` script and main's `@concertable/web-b2b`
  addition (from the merged `Refactor/B2bPackageTopology`, #643) to `build:packages`/`build:web-packages`
  — resolved by keeping both additions (`3b10b5302`). `app/package-lock.json` and
  `app/scripts/check-fe-boundaries.mjs` auto-merged cleanly. Verified locally before push:
  `npm install` clean, `npm run lint:boundaries` clean (all 10 workspaces), `npm run build:admin` green,
  and `dotnet build` green on `Concertable.B2B.Web`, `Concertable.Auth`, `Concertable.B2B.AppHost`, and
  the umbrella `Concertable.AppHost`. Pushed; PR flipped from `DIRTY`/`CONFLICTING` to
  `BLOCKED`/`MERGEABLE` (blocked = draft-PR CI re-running on the new head, not a real block).
  Reconfirmed same day once that run completed: `gh pr checks 648` — every required check `pass`, the
  three E2E jobs correctly `skipping` per this plan's phase scope; `gh pr view 648` — `state OPEN`,
  `mergeStateStatus CLEAN`, `mergeable MERGEABLE`, not draft. **#648 is genuinely ready; nothing further
  to verify — merge is gated only on Tommy's explicit instruction (see Next Steps).**
  `Refactor/b2b_admin-module` now has an **open** PR ([#651](https://github.com/Concertable/concertable/pull/651),
  `state OPEN`, `mergeStateStatus BLOCKED`, not yet merged) — the ledger previously said no PR existed;
  step 1 below remains not-yet-actionable until #651 merges, not blocking.
- Re-reconciled 2026-08-19 (same day, later): polled #648's checks on head `4e1ec207b` to completion —
  45 pending checks resolved to 0 pending / 0 failures, `mergeStateStatus CLEAN`, `mergeable MERGEABLE`,
  not draft. Re-checked #651: still `state OPEN`, `isDraft true`, `mergeStateStatus BLOCKED`, not merged
  — step 1 remains not-yet-actionable. No action taken beyond confirmation; merge still waits on Tommy.
- Reconciled 2026-08-19 (same day, later still): Tommy reported #651 merged. Confirmed via
  `gh pr view 651` (`state MERGED`, `mergedAt 2026-08-19T18:27:56Z`). Merged `origin/main` into this
  branch (137 commits behind) per Next Steps step 1. Ten files conflicted: seven backend (the
  Admin-module extraction moved `IAdminRepository`/`AdminRepository`/`AdminService`/`IAdminService`/
  `IAdminModule`/`AdminModule`/`CredentialRegisteredHandler`/`UserController`/both `AdminServiceTests`/
  `AdminProvisioningTests` out of `Concertable.B2B.User` into a new `Concertable.B2B.Admin` module) and
  three docs (`app/AGENTS.md`, `app/web/AGENTS.md`, `app/web/shared/AGENTS.md`, from the concurrently
  merged `Docs/GuidanceDocsRestructure`, #637).
  **The backend conflict was a real security regression risk, not just a textual clash:** #651 branched
  off `main` after Phase 1 (#624) merged but *before* this branch's post-login security fix (design
  decision 1) was written, so its mechanical module extraction preserved Phase 1's original,
  registration-time grant (`CredentialRegisteredHandler` → `IAdminModule.GrantIfEligibleAsync(sub,
  email)`, wrapped in a new `IUnitOfWorkBehavior` for atomicity) — exactly the gap design decision 1
  closes. Resolved every conflict toward this branch's secure design
  (`AdminService.EnsureCurrentUserAdminGrantedIfEligibleAsync()`, called from `UserController.Me()` via
  the new `IAdminModule` facade) reapplied onto the new module boundary, not a silent revert to the
  registration-time grant. Also: renamed `AddAdmin`→`GrantAdmin` on `IAdminRepository`/`AdminRepository`
  (adopted #651's rename), deleted the now-dead `IUnitOfWorkBehavior`/`UnitOfWork<UserDbContext>`
  registration in `Concertable.B2B.User.Infrastructure` (existed only to support the reverted design),
  updated the stale `AdminService.GrantIfEligibleAsync` doc-comment reference in `AdminInvitationEntity`,
  and split `AdminProvisioningTests` to match #651's module-ownership split: Admin's own
  `AdminProvisioningTests.cs` keeps the grant-eligibility (`Login_*`) tests using a `RegisterAsync`+
  `LogInAsync` helper pair (dispatching `CredentialRegisteredEvent` via the generic
  `IIntegrationEventHandler<T>` interface, not the internal `CredentialRegisteredHandler`, since Admin's
  test project has no `InternalsVisibleTo` grant into `Concertable.B2B.User.Infrastructure`); removed the
  now-redundant/stale `Registration_AdminClient_CreatesUser_EvenWithNoMatchingAdminGrant` from User's
  `UserProvisioningTests.cs` (its premise — the ambient-transaction grant call — no longer exists, and
  it's covered by the existing `[InlineData(ClientIds.Admin)]` case on `Registration_ManagerClient_CreatesUser`).
  **Docs conflict:** adopted #637's slimmed skill-referencing structure in all three files; one of them
  (`app/web/AGENTS.md`) stated a now-stale fact independent of the restructure — "run the four web
  builds" — which is wrong since this branch's Phase 2 added the fifth (`web-admin`); corrected to five
  and flagged that the external `app-tiers` skill (`Concertable/agent-standards` plugin, outside this
  repo) still needs its own follow-up update for the fifth SPA — not fixable from here.
  **Verified:** `dotnet build` on `Concertable.B2B.Web` (full host): 0 errors. Built and ran
  `Concertable.B2B.Admin.UnitTests`: 31/31 passing. Built `Concertable.B2B.Admin.IntegrationTests`,
  `Concertable.B2B.User.IntegrationTests`, `Concertable.B2B.User.UnitTests`: all green (integration
  tests compile-only, no local Docker). `npm run lint:boundaries`: clean across all 13 workspaces.
  `npm run build:admin`: green. Committed as `80208ce22` and pushed; draft-PR CI running on that head.
- Draft-PR CI on `80208ce22`/`bc55fefdd`/`82c0c1701` came back genuinely red — 5/8
  `Concertable.B2B.Admin.IntegrationTests` failing (`gh run view 32296515424`; the wider list `gh pr
  checks` briefly showed of ~10 failing jobs was a transient/stale snapshot — the run's actual job list
  had exactly one real failure plus the cascading `ci-complete` gate). **Root cause, found via a local
  Docker-backed repro (Docker Desktop was down, then recovered) and confirmed by direct experiment (an
  unconditional `throw`/`Assert.Fail` inside the suspect method, since a custom `ILogger` line and a bare
  `Console.WriteLine` both turned out not to be captured by this test runner — a dead end that cost real
  time before switching to xUnit's own assertion-failure reporting, which does reliably surface):** the
  `origin/main` merge (`80208ce22`) silently dropped `CredentialRegisteredHandler`'s
  `await context.SaveChangesAsync(ct);` — git's 3-way merge auto-resolved it as a non-conflicting
  deletion (origin/main's side had replaced the call with an ambient-transaction wrapper this branch's
  conflict resolution correctly removed to keep the post-login grant design, without the removed
  wrapper's own internal save being replaced by anything). Effect: **no B2B manager registration —
  venue, artist, or admin — ever actually persisted a `UserEntity` row**, not just Admin's grant flow;
  confirmed via `Concertable.B2B.User.IntegrationTests` also failing 6/11 before the fix. Restoring the
  `SaveChangesAsync` call then surfaced a second, independent, pre-existing bug from PR #651's own module
  extraction: `AdminApiFixture.ClearAdminsAsync()` only cleared `AdminProfiles`, not the seeded admin's
  underlying `UserEntity` row (the pre-refactor `UserApiFixture.ClearAdminsAsync` cleared both) — masked
  until now because nothing ever hit the database, then a real `Users.Email` duplicate-key violation once
  saves actually ran. Fixed by restoring the `UserDbContext`-based row cleanup, mirroring the original.
  Both fixes committed as `e9623af6d`, pushed. **Verified:** `Concertable.B2B.Admin.IntegrationTests`
  8/8, `Concertable.B2B.User.IntegrationTests` 11/11, `Concertable.B2B.User.UnitTests` 1/1,
  `Concertable.B2B.Admin.UnitTests` 31/31 — all green locally post-fix.
- Parallel, independent work: `Refactor/b2b_admin-module` (separate worktree/session) extracts
  `Concertable.B2B.Admin` out of `Concertable.B2B.User` to match the `Concertable.B2B.Tenant` precedent
  (own `AdminDbContext`, plain `Guid` FKs, `IAdminModule` facade for `UserController.Me()`'s grant-check).
  Purely internal — routes/DTOs unchanged — so it does not block or get blocked by this ledger; fold in a
  note here once it merges. Open as PR #651 as of this reconciliation.

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
carried into Phase 2, and **now resolved** (see below) before the OIDC client was wired, so the gap
never went live.

Phase 1's worktree/branch is closed (no separate worktree existed — Phase 1 ran in the primary
checkout; its branch was deleted on merge). Fresh worktree created for Phase 2:
`.worktrees/Feature-launch_admin-console`, branch `Feature/launch_admin-console`, off `origin/main`
at `bfbfd863c...` (contains #624).

**Security pre-flight closed, before the OIDC client:** the carried-over gap (`GrantAdminIfEligibleAsync`
granting off the raw, unverified registration event) is fixed — asked Tommy for a decision among three
options (Auth contract change / weak client-side mitigation / move the grant to a post-login,
already-verified checkpoint); he picked the third. Implemented as
`AdminService.EnsureCurrentUserAdminGrantedIfEligibleAsync`, called from `UserController.Me()`; see plan
design decision 1 for the full mechanism. Zero cross-service contract changes. `IAdminRepository` gained
`AddAdmin(Guid)`. `AdminServiceTests` gained 6 tests (32/32 total); `AdminProvisioningTests` restructured
around a register-then-log-in two-step flow (compiles clean, deferred to CI per remote-validation policy
— no local Docker).

**Auth OIDC client + AppHost wiring done:** `ClientIds.Admin` ("admin") added to `Config.WebClients` +
`SpaClientSettings.Admin`, redirect URIs in `appsettings.json` (prod) and `appsettings.E2E.json`
(`localhost:5178`); reuses the existing non-Customer scope branch (`openid profile concertable.b2b.api`)
— no `Config.cs` logic change needed. `AppHostExtensions.AddAdminSpa` (mirrors `AddVenueSpa`/
`AddArtistSpa` — B2B-only backend, no separate service), wired into both `Concertable.B2B.AppHost` and
the umbrella `Concertable.AppHost`. Both builds green.

**Phase 2 SPA scaffold done and build-verified:** `app/web/admin/` created as a fifth workspace
(package.json/vite.config.ts/tsconfig*/index.html, port 5178). Deliberately does **not** depend on
`@concertable/b2b` (the venue/artist tenant tier — Admin has no tenant concept) and does **not** use the
shared `AppLayout`/`Navbar`/`ProfileMenu` stack (`ProfileMenu` hard-links `/settings` and
`/settings/payment`, which don't apply to a platform-admin console) — instead a small hand-rolled
`_admin/route.tsx` header using `useAuth()` alone (email from OIDC profile claims, sign-out). Routes:
`login.tsx`/`auth.callback.tsx` (copied from venue), `forbidden.tsx` (new — shown when an authenticated
non-admin B2B user reaches the app), `_admin/route.tsx` (`requireAdmin` guard: `requireAuth` then checks
`Identity.isAdmin` off the same cache entry the guard already populated, no extra fetch),
`_admin/index.tsx` (renders the admins page). `features/identity/` (guard + a feature-private `Identity`
type, not exported — nothing outside the guard needs it) and `features/admins/` (mirrors b2b/shared's
`members` feature shape: one `useAdminOverviewQuery` shared by both the roster and pending-invitations
facades, since the backend returns both in one `GET /api/Admin` call unlike members' two separate
endpoints; `useInviteAdmin`/`useAdminsRoster`/`usePendingInvitations` facades; revoke-admin button
disabled client-side when it's the last admin, mirroring the server's last-admin invariant). All five
web builds green (customer/venue/artist/business/admin) and `npm run lint:boundaries` clean across all
12 workspaces (added `web/admin` to `app/scripts/check-fe-boundaries.mjs`). `app/web/AGENTS.md`'s build
gate and `app/web/shared/AGENTS.md`'s route-contract docs updated from "four" to "five" SPAs;
`BROWSER_STORAGE.md`/`storageManifest.ts` updated for admin's cookie-consent/theme/oidc storage.

Two tech-debt items logged along the way (not fixed, out of scope here):
`api/Concertable.Frontend.Hosting/TECH_DEBT.md` (new — the `AddXSpa` methods' magic port/surface-name
literals, pre-existing across all five, low priority since it's dev-only orchestration likely reworked
for prod deployment) and `app/web/shared/TECH_DEBT.md` (new — `useSyncUser`/`useAuthStore` duplicates
TanStack Query's own cache via a `useEffect` copy, the exact anti-pattern `app/agents/CODE_PATTERNS.md`
already warns against for `useConcertStore`; Admin's own guard/header deliberately avoid the pattern
rather than adding to it).

**Not added:** focused component tests for invite/revoke. Checked precedent first — `b2b/shared`'s
`members` feature (the closest analog) has zero component/hook tests for its equivalent
`InviteForm`/`MembersRoster`/`PendingInvitations` either, only one pure-logic test
(`acceptInvitation.test.ts`, node environment, no jsdom). No single-app `vitest` config precedent exists
in `venue`/`artist` either. Matched precedent rather than inventing new test infrastructure ahead of an
established need; flagging this explicitly rather than silently skipping it.

Branch merged `origin/main` (11 commits: platform-sync version bumps, an `OpportunityMapper`
refactor, the platform-sync auto-merge retry fix, a `CODE_CONVENTIONS.md` update) — clean, no
conflicts. `Concertable.Auth` and both AppHost projects rebuilt green post-merge. Pushed and opened
draft PR [#648](https://github.com/Concertable/concertable/pull/648).

## Next Steps

Paused: Tommy — #648 ([PR #648](https://github.com/Concertable/concertable/pull/648)) is pushed at head
`e9623af6d`, carrying a genuine bug fix for a red draft-PR CI run (found and fixed locally, verified
green — see "Current state"). Resume condition: confirm draft-PR CI is green on `e9623af6d`
(`gh pr checks 648`) before treating it as ready; merge is then gated only on Tommy's explicit
instruction (when given, re-check the `behind` count per the root `AGENTS.md` "Before enabling
auto-merge" currency check first, since more time has passed).

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
  hook (none is proven to exist yet). It originally triggered lazily inside
  `CredentialRegisteredHandler`'s reactive path; **corrected in Phase 2** to trigger from
  `UserController.Me()` instead (every authenticated request path, not just registration) — see the
  security pre-flight entry above and plan design decision 1.
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
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch_admin-console
Read @plans/launch/ADMIN_CONSOLE_PLAN.md and @plans/launch/ADMIN_CONSOLE_PROGRESS.md and do what its `## Next Steps` says.
```
