# Code review — Feature/launch_admin-console

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `44440b7cb` _(2026-08-21)_
**Security-reviewed up to commit:** `44440b7cb` _(2026-08-21)_

> Range reviewed: `42f76099..44440b7cb`.
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

No further findings — checked correctness, microservice/module boundaries, C# conventions
(`csharp-style`, `csharp-naming`), and test tier placement (`unit-testing`, `integration-testing`)
against the branch's full diff. Security review: the post-login grant design
(`EnsureCurrentUserAdminGrantedIfEligibleAsync` called from `UserController.Me()`, never from
`CredentialRegisteredHandler` at registration time) correctly requires email verification before an
admin grant can occur — the property this branch exists to establish — and was preserved intact
through every origin/main merge in this branch's history, including the #651 module-extraction merge
that could have silently reverted it.
