# Code review — Feature/launch_admin-console

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `0d7821d6d4985d905600caf99c873513179982e0`  _(2026-08-17)_
**Security-reviewed up to commit:** `0d7821d6d4985d905600caf99c873513179982e0`  _(2026-08-17)_

> Range reviewed: `d5669a836..0d7821d6d` (14 commits). Commits after `5492efa58` are docs-only except
> `6aabf147b` (the IAdminRepository extraction below) and `0d7821d6d` (merge of `origin/main`, resolving
> one conflict in `api/agents/CODE_CONVENTIONS.md` by keeping both sides' additions — no semantic change).
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

- [x] **MB1 — MED — module boundaries** — `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Application/Interfaces/IUserRepository.cs`
  `IUserRepository` mixed three unrelated entity types (`UserEntity`, `AdminProfileEntity`,
  `AdminInvitationEntity`) — the minority repository shape in this codebase; Concert and Conversations
  both give every entity its own repository, including satellite entities in the same module/DbContext
  (`ApplicationRepository`/`BookingRepository`/`ConcertRepository`/.../`ContentReportRepository`/
  `MessageRepository`). Fixed: extracted `IAdminRepository`/`AdminRepository` (bound to
  `AdminInvitationEntity`) for the admin-authority and admin-invitation queries; `IUserRepository` is now
  `UserEntity`-only. `AdminService` depends on `IAdminRepository`. Also collapsed a redundant
  `GetInvitationByIdAsync`/`AddInvitation` pair (now covered by the inherited base `GetByIdAsync`/
  `InsertAsync` once correctly bound) and an `AddAsync`+`SaveChangesAsync` pair into one `InsertAsync`
  call — the same slip found and logged as recurring across five other services
  (`api/Concertable.B2B/TECH_DEBT.md`). Added "One repository per entity" to `api/agents/CODE_PATTERNS.md`
  and logged `ITenantRepository`'s pre-existing three-entity mix as tech debt in a new Tenant module
  `TECH_DEBT.md` (out of scope for this branch — pre-existing, cross-cutting).

<!-- NAT layer (Layer 1, code-reviewer subagent): no findings cleared the 80% confidence bar. Diff closely mirrors InvitationService/TenantInvitationEntity/MembershipService precedent. -->

<!-- SEC layer: 5 candidates identified, all filtered. Two concurrency findings (bootstrap-empty-check TOCTOU, last-admin-invariant TOCTOU) scored 2/10 — structurally blocked by unique constraints / mirror the accepted MembershipService.IsLastOwnerAsync pattern / self-heal. One finding (admin grant trusts an unverified registration email — CredentialRegisteredHandler.cs:78-99, no email-ownership proof despite the plan's design-decision-2 claiming it reuses Auth's verification flow) scored 5/10: real gap, but inert today since no `admin` OIDC client exists yet (Phase 2 adds it). Below the 8-confidence bar for a SEC# finding here; noted as a Phase-2 pre-flight item, not a Phase-1 blocker — see plan ledger. -->

