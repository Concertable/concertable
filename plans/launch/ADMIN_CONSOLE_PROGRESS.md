# Admin console progress

- Plan: `plans/launch/ADMIN_CONSOLE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/admin-console`
- Roadmap status: **not yet ticked** — Phase 4 (venue approval UI) remains before
  `plans/launch/LAUNCH_ROADMAP.md` line 42 can flip to `[x]`.
- Worktree: closed (Phase 2/3 worktree `.worktrees/Feature-launch_admin-console` removed post-merge). A
  fresh worktree off `origin/main` is needed for Phase 4.
- Branch: Phase 2/3 ran on `Feature/launch_admin-console` (Phase 2 — new branch of the same name, Phase
  1's was deleted on merge); also closed/deleted post-merge. Phase 4 needs a new branch of the same name
  (recreated from `origin/main`, per this file's own folder convention).
- PR: **Phase 2 — MERGED** as [#648](https://github.com/Concertable/concertable/pull/648). **Phase 3
  (moderation UI) — MERGED** as [#722](https://github.com/Concertable/concertable/pull/722)
  (2026-08-22T04:14:31Z), via a required split-PR prerequisite,
  [#733](https://github.com/Concertable/concertable/pull/733) (`Feature/navbar-shell-shell`, MERGED
  2026-08-22T03:30:06Z — extracted the shared `Navbar.tsx`/`spinner.tsx` diff so `carve-fe (web/admin)`
  could build against a published `@concertable/web` alpha rather than local source), plus its
  platform-sync follow-through [#734](https://github.com/Concertable/concertable/pull/734) (non-breaking,
  auto-merged) and a review-lifecycle cleanup
  [#735](https://github.com/Concertable/concertable/pull/735) (retired the spent
  `Feature-launch_admin-console.md`/`Feature-navbar-shell-shell.md` review files). Phase 1:
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
- Reconciled 2026-08-20/21: Tommy said "let's merge." Logged two small, related pieces of tech debt
  Tommy flagged along the way (not fixed, per his call): `api/Concertable.B2B/src/Modules/User/TECH_DEBT.md`
  (`UserEntity.FromRegistration` is the only domain factory in the B2B layer not named `Create` — no
  disambiguation need justifies the exception) and
  `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/TECH_DEBT.md` (`AdminProvisioningTests.LogInAsync`
  reuses that same production factory, plus a redundant `email` param, to fake an identity for an
  already-persisted user instead of reading the real row back). Before arming auto-merge, confirmed the
  currency check (root `AGENTS.md` "Before enabling auto-merge") — branch was 21 commits behind
  `origin/main`; merged clean (no conflicts), rebuilt `Concertable.B2B.Web`/`Concertable.Auth`/both
  AppHosts to 0 errors, pushed (`107fafcf5`). Draft-PR CI on that head then genuinely failed:
  `carve-fe (web/b2b/artist)` and `carve-fe (web/b2b/venue)`, ~150 TypeScript errors combined — real,
  not flaky. Root cause: the `origin/main` sync brought in #595's camelCase-JSON-enums refactor
  (`DashboardApplicationStatus`, `ActivityType`, `SettlementDirection`, `PaymentMethod`, `TenantType`,
  `Genre`, `StripeConnectState`, `PayoutAccountStatus`), and neither `app/web/b2b/artist` nor
  `app/web/b2b/venue` was in that refactor's scope, so their dashboard fixtures/widgets/route guards
  still used the old PascalCase literals. Recased every affected literal (dozens per app, via the
  compiler's own `Did you mean` hints; a first automated pass had a column-offset bug — TS points
  diagnostics at the enclosing property/argument, not the literal itself — caught before committing by
  verifying the actual diff, not just script exit status) plus two `Record<K,V>` object-key sets the
  compiler only reports one violation of at a time. Fixed as `4d2563f29`, pushed. **Verified:** both
  apps' full builds clean, all five web app builds green (`web-customer`/`web-business`/`web-admin`/
  `web-artist`/`web-venue`, exit 0 each), `npm run lint:boundaries` clean across all 13 workspaces.
  Draft-PR CI re-validating on `4d2563f29` — confirm genuinely green (see Next Steps) before arming
  auto-merge.
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

## Phase 3 close-out (2026-08-22)

**#648 merged** (Phase 2), then Phase 3 (moderation UI) was built on a fresh
`Feature/launch_admin-console` branch/worktree off `origin/main` and delivered real UX/architecture
fixes beyond the plan's bare scope, driven by Tommy's own line-by-line review:

- Replaced the hand-rolled `_admin/route.tsx` header with the shared `Navbar` (branded — logo, real nav
  chrome — instead of a bare unbranded bar), extending `Navbar` with optional `profileSlot`/`showSearch`/
  `showMailbox` props (default `true`/`true`, so every existing caller is unaffected) so a tenant-less
  surface like Admin can supply its own profile menu and skip search/mailbox rather than being forced to
  render stubs for concepts that don't apply to it.
- Destructive/primary actions (`Revoke`, `Hide`) recolored off `variant="ghost"` (invisible-by-design,
  wrong for actions with real consequences) to `variant="destructive"`/appropriate defaults across
  `AdminsRoster`, `PendingInvitations`, `ReportsQueue`.
- Extracted a shared `Spinner` primitive (`app/web/shared/src/components/ui/spinner.tsx`), replacing a
  hand-rolled spinner div.
- `moderationApi.ts`/`adminApi.ts` adopted a `const BASE`/`INVITATION_BASE` route-literal pattern instead
  of repeating the route string per call; logged as tech debt for the still-unconverted call sites in
  `app/shared`/`app/web/b2b/shared` (their own `TECH_DEBT.md` files) and written up as a new standard in
  the external `react-agents` skill repo (`HTTP.md`, `http-layer`).
- **Adopted `react-hook-form` + `zodResolver` as the canonical form pattern**, replacing the hand-rolled
  `useState`+`zod.safeParse` "write-boundary" shape for `InviteForm` and `ResolveReportDialog`. Caught and
  fixed a real bug live via Playwright: `mode: "onBlur"` + `reValidateMode: "onChange"` left a stale
  validation error after the user fixed an invalid value; `mode: "onChange"` alone clears correctly.
  Migration for the remaining hand-rolled forms (`app/shared`, `app/web/b2b/shared`, customer) logged as
  tech debt in each tree's own `TECH_DEBT.md`, citing this migration as the model; the `FORMS.md`/
  `STRUCTURE.md` standards in `react-agents` were rewritten around this shape.
- Self-caught in review: the AppHost's per-worktree SQL-volume-isolation fix
  (`api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs`) hashed
  `Directory.GetCurrentDirectory()`, which varies by invocation style; fixed to hash
  `AppContext.BaseDirectory` (fixed per checkout regardless of how the AppHost is launched).

**Delivery chain, in order:** `carve-fe (web/admin)` on #722 failed because it builds against the
*published* `@concertable/web` package, not local source — the Navbar/Spinner changes hadn't been
published yet (`publish-fe-packages.yml` only fires on push to `main`). Split those two files onto a
fresh branch/worktree, opened [#733](https://github.com/Concertable/concertable/pull/733), merged it
first (merge-queue CI, ~30 min — the queue reruns full CI against a live merge with `main`, not just the
PR's own branch checks). Its merge triggered `publish-fe-packages.yml`; once that republished the alpha
dist-tag, re-ran (not retried-blind — the root cause was fixed) the previously-failing `carve-fe
(web/admin)` job on #722's existing CI run, which then passed. #722 had already auto-armed itself into
the merge queue by the time its local `origin/main` sync was ready to push (GitHub queues on green
checks, independent of a manual `--auto` re-arm) — let the queue's own merge-with-`main` build stand in
for the local rebuild-before-arming step, since it validates the identical thing. #722 merged
2026-08-22T04:14:31Z, triggering `publish-packages` → platform-sync PR
[#734](https://github.com/Concertable/concertable/pull/734) (non-breaking `Directory.Packages.props`
bump only — auto-merged green). Retired the spent `reviews/Feature-launch_admin-console.md` and
`reviews/Feature-navbar-shell-shell.md` via [#735](https://github.com/Concertable/concertable/pull/735)
per `review-lifecycle`.

**Noticed, out of scope:** a scheduled `Verify service mirror parity` run on `main` failed across all six
service mirrors (`api/Concertable.Payment`→`Concertable/payment`, etc.) shortly before #722 merged — not
caused by this branch (no mirror-parity-relevant paths in its diff) and not a required check on any of
this chain's PRs, but a systemic mirror-sync issue worth a separate look.

**Two upstream doc/standard PRs opened during Phase 3, still awaiting Tommy's review/merge (not
auto-merged — no blanket authorization covers them):** `tomjseery/react-agents` PR #3
(`http-layer`/`HTTP.md` `const BASE` rule) and PR #4 (`write-boundary`/`FORMS.md` rewrite around
`react-hook-form`). `Concertable/agent-standards` PR #23 (`pr-screenshots` skill) also still open.

## Next Steps

**Phase 4 — Venue approval UI** is the only remaining phase (plan §"Phase 4"): new
`IAdminVenueRepository` pending-approval query + service method + `[Admin]`-gated
`GET /api/Venue/pending-approval` endpoint (genuinely new backend surface, not just UI wiring), a
pending-venues list page, and the approve action wired to the existing `PATCH /api/Venue/{id}/approve`.
Start by creating a fresh worktree/branch (`Feature/launch_admin-console`) off current `origin/main` —
the prior one was closed post-#722-merge. Only once Phase 4 lands and its verification gate passes does
`plans/launch/LAUNCH_ROADMAP.md`'s `launch/admin-console` item (line 42) get ticked `[x]`.

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
Create a fresh worktree/branch (Feature/launch_admin-console) off current origin/main.
Read @plans/launch/ADMIN_CONSOLE_PLAN.md and @plans/launch/ADMIN_CONSOLE_PROGRESS.md and do what its `## Next Steps` says (Phase 4 — venue approval UI).
```
