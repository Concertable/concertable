# Code review — Feature/launch_admin-console

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `5492efa5877681fb50a909849c24ef3116c0439e`  _(2026-08-16)_
**Security-reviewed up to commit:** `5492efa5877681fb50a909849c24ef3116c0439e`  _(2026-08-16)_

> Range reviewed: `d5669a836..5492efa58` (9 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — test coverage** — `api/Concertable.B2B/src/Modules/User/Tests/Concertable.B2B.User.UnitTests/AdminServiceTests.cs`
  `AdminService.GetOverviewAsync`, `IsCurrentUserAdminAsync`, and `RevokeInvitationAsync` had no covering unit
  test (neither their success nor failure branches), and no HTTP-level test exercises the `AdminController`
  endpoints backing them (`GET /api/Admin`, `GET /api/Admin/me`, `DELETE /api/AdminInvitation/{id}`) — the
  existing `AdminProvisioningTests` integration suite only drives `CredentialRegisteredHandler` directly, never
  these endpoints. Fixed: added `RevokeInvitationAsync_InvitationNotFound_*`, `RevokeInvitationAsync_AlreadyAccepted_*`,
  `RevokeInvitationAsync_Pending_*`, `IsCurrentUserAdminAsync_NoCurrentUser_*`, `IsCurrentUserAdminAsync_CurrentUserIsAdmin_*`,
  and `GetOverviewAsync_JoinsAdminEmailsAndPendingInvitations` to `AdminServiceTests.cs`.

<!-- NAT layer (Layer 1, code-reviewer subagent): no findings cleared the 80% confidence bar. Diff closely mirrors InvitationService/TenantInvitationEntity/MembershipService precedent. -->

<!-- SEC layer: 5 candidates identified, all filtered. Two concurrency findings (bootstrap-empty-check TOCTOU, last-admin-invariant TOCTOU) scored 2/10 — structurally blocked by unique constraints / mirror the accepted MembershipService.IsLastOwnerAsync pattern / self-heal. One finding (admin grant trusts an unverified registration email — CredentialRegisteredHandler.cs:78-99, no email-ownership proof despite the plan's design-decision-2 claiming it reuses Auth's verification flow) scored 5/10: real gap, but inert today since no `admin` OIDC client exists yet (Phase 2 adds it). Below the 8-confidence bar for a SEC# finding here; noted as a Phase-2 pre-flight item, not a Phase-1 blocker — see plan ledger. -->

