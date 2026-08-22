# Code review — Feature/launch_admin-console

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `bec1b4051` _(2026-08-22)_
**Security-reviewed up to commit:** `82f1fb498` _(2026-08-21)_ — range below touches no
`.agents/merge-gate.json` `security_paths` (no `Concertable.Auth`, `Concertable.Payment`,
`.Contracts`, or `Controller*.cs`), so Step 1d did not re-run.

> Range reviewed: `42f76099..82f1fb498`, then `82f1fb498..bec1b4051`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Admin/Concertable.B2B.Admin.Infrastructure/Services/AdminService.cs`
  `EnsureCurrentUserAdminGrantedIfEligibleAsync` called `repository.SaveChangesAsync` unguarded after
  a grant. Two concurrent first authenticated requests for the same eligible email (e.g. two tabs
  after login) both pass the eligibility check, both grant, and the loser's `SaveChangesAsync` throws
  an unhandled duplicate-key `DbUpdateException` — a 500 for what is a legitimate race, not a real
  conflict. Fixed: both grant branches now call `TrySaveGrantAsync`, which catches
  `DbUpdateException` via `IsDuplicateKey()`/`DiscardFailedChanges()` and returns `true` either way
  (the race winner made the caller admin regardless of who "won" the write), `false` only when the
  caller was never eligible. Covered by
  `EnsureCurrentUserAdminGrantedIfEligibleAsync_MatchingPendingInvitation_NonDuplicateSaveFailure_Propagates`
  (a genuine non-duplicate failure still propagates) — the duplicate-key-swallowed branch itself isn't
  unit-tested since constructing a real `SqlException` isn't practical at that tier.

- [x] **NAT2 — LOW — efficiency** — `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Api/Controllers/UserController.cs`
  `Me()` called `adminModule.EnsureCurrentUserAdminGrantedIfEligibleAsync()` then, on every request,
  a second `adminModule.IsCurrentUserAdminAsync()` to read the result — a redundant DB round-trip on
  the hottest authenticated endpoint in the app. Fixed:
  `EnsureCurrentUserAdminGrantedIfEligibleAsync` now returns `Task<bool>` (whether the caller is admin
  after the call) and `Me()` uses that value directly, dropping the second call.

- [x] **CONV1 — MEDIUM — test convention** — `api/Concertable.B2B/src/Modules/Admin/Tests/Concertable.B2B.Admin.IntegrationTests/AdminProvisioningTests.cs`,
  `api/Concertable.B2B/src/Modules/User/Tests/Concertable.B2B.User.IntegrationTests/UserProvisioningTests.cs`
  Both files hand-wrote `fixture.Services.CreateScope()` to dispatch
  `IIntegrationEventHandler<CredentialRegisteredEvent>` — `testing/INTEGRATION.md` requires
  `IScoped<IEnumerable<IIntegrationEventHandler<TEvent>>>.RunAsync(...)` for exactly this (the
  established precedent in `EscrowPaymentProcessorTests.cs`/`ApiFixture.cs`). Found via self-audit
  against the actual standard after this branch's history showed it had been skipped. Fixed in both
  files; the DbContext-scoping `CreateScope()` calls in `UserProvisioningTests.cs` used purely for
  assertion reads are a different, legitimate use and were left as-is.

- [x] **NAT3 — HIGH — correctness (CI-caught)** — `api/Concertable.B2B/tests/Concertable.B2B.CompositionTests/B2BCompositionTests.cs`
  `Functions_MissingAdminModule_FailsWithUnresolvedDependency` asserted the **Workers** host fails
  composition without `IAdminModule` — true under the old registration-time-grant design this branch's
  #651 merge resolution correctly moved away from. `IAdminModule`'s only real consumer is now
  `UserController.Me()` in the **Web** host, so Workers has nothing to lose and the test silently
  stopped exercising anything (caught by CI's `composition-tests` job going red, not by local review).
  Fixed: renamed to `Web_MissingAdminModule_FailsWithUnresolvedDependency`, asserting against the Web
  host build instead. Verified: `Concertable.B2B.CompositionTests` 5/5 green.

No further findings — checked correctness, microservice/module boundaries, C# conventions
(`csharp-style`, `csharp-naming`), and test tier placement (`unit-testing`, `integration-testing`)
against the branch's full diff. Security review: the post-login grant design
(`EnsureCurrentUserAdminGrantedIfEligibleAsync` called from `UserController.Me()`, never from
`CredentialRegisteredHandler` at registration time) correctly requires email verification before an
admin grant can occur — the property this branch exists to establish — and was preserved intact
through every origin/main merge in this branch's history, including the #651 module-extraction merge
that could have silently reverted it.

## Incremental review — 2026-08-22

Range: `82f1fb498..bec1b4051` (Phase 3 — moderation UI, plus follow-on fixes), scoped to the paths this
branch's own commits touched — `app/web/admin/**`, `app/web/shared/src/components/Navbar.tsx`,
`app/shared/TECH_DEBT.md`, `app/web/b2b/shared/TECH_DEBT.md`, `app/web/customer/TECH_DEBT.md`,
`api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs` — rather than the full
`git diff 82f1fb498..HEAD`, which would also replay ~150 unrelated commits merged in from `origin/main`
(dashboard consumer work, the N3/N4 doc restructure, the Kernel state machine, several platform-sync
bumps) that already went through their own review and CI on `main`. Layer 1 (native) via the
`code-reviewer` subagent at medium effort; Layer 2 (lenses) by hand.

- [x] **NAT4 — MEDIUM — reuse/duplication** — `app/web/shared/src/components/Navbar.tsx`
  The desktop link list and the mobile `DropdownMenu` link list duplicated the same href-vs-`to`
  branching logic with different wrapper markup — a future `NavLink` shape change would need updating
  in two places. Fixed: extracted `NavLinkAnchor({ link, className })`, used by both call sites.

- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs`
  The new per-checkout SQL volume isolation hashed `Directory.GetCurrentDirectory()`, which varies with
  invocation style — a developer's `dotnet run` from inside the AppHost project folder gets a different
  cwd than `scripts/e2e.ps1`, which `Set-Location $repoRoot` first. The same worktree would get two
  different volumes depending on how it's launched, weakening the fix's own goal (one volume per
  worktree, reused across restarts). Fixed: hash `AppContext.BaseDirectory` (the build output path)
  instead — fixed per checkout regardless of invocation cwd.

Lens B (service isolation): N/A — no cross-service call added; the AppHost change is orchestration
config, not a runtime dependency. Lens C (module boundaries): `Navbar`'s new `profileSlot`/
`showSearch`/`showMailbox` props are the `tiered-shared-code` slot pattern correctly applied
(`ProfileMenu`'s hardcoded `/settings` links don't fit a tenant-less surface); all five web builds stay
the enforcement. Lens D (seeding): N/A, no seeder touched. Lens E (conventions): `write-boundary`
(`react-hook-form` + `zodResolver`, checked live via Playwright — a `mode: "onBlur"` +
`reValidateMode: "onChange"` combination left a stale error after fixing an invalid value, verified
before and after; fixed to `mode: "onChange"`, see commit `4a39e13c1`), `http-layer`
(`const BASE`/`INVITATION_BASE`), `contract-naming`, `csharp-style`/`csharp-naming` on the AppHost
extension — no further findings. Lens F (test coverage): the two hand-rolled-`useState`-to-`react-hook-form`
migrations and the button-variant changes are UI-only with no new branching logic to assert beyond what
manual + Playwright verification already covered; the `AddSqlServerContainer` volume-naming change has
no existing test harness for this extension method to extend (pre-existing gap, not introduced here).
