# Code review — Feature/TenantInvitations

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `5c79aed0d516afab5623d04fa90ac3cdd10695fe`  _(2026-07-19)_

> Range reviewed: `0c729011..5c79aed0` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

Reviewed Phase 6.1 (invitation entity/schema/migration) + 6.2 (member endpoints, `MembershipService`,
last-Owner invariant, `IUserModule` batch email) through all five lenses. **Microservice isolation,
module boundaries, seeding, and C# conventions are all clean** — the Tenant→User call goes through the
`IUserModule` facade (intra-B2B, `User.Contracts` only), the service persists via
`repository.SaveChangesAsync` (not `IUnitOfWork`), the migration is a re-scaffold (not additive), no
primary ctors, no inline log templates, and the never-seed doc bullet is added. One correctness gap:

- [x] **BUG1a — LOW (correctness, latent) — FIXED — `MembershipService.DeleteCurrentTenantAsync`** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/MembershipService.cs:59`
  `DeleteCurrentTenantAsync` now removes the tenant's invitation rows (new
  `ITenantRepository.ListInvitationsByTenantAsync` + `RemoveInvitation`, in the same tracked
  `SaveChangesAsync` transaction as the membership/tenant deletes) so no invitation outlives its tenant.
  Build green. **Verification caveat:** no integration test exercises the new cleanup line, because 6.2
  has no way to create an invitation (no invite endpoint; invitations are never seeded) — the test lands
  with 6.3's invite/accept endpoints. The existing `DeleteOrganization` integration tests still cover the
  path (the new loop is a no-op over an empty set).
- [ ] **BUG1b — LOW — REMAINS for Phase 6.3 — accept-path tenant-existence guard**
  Guard `POST /api/invitations/{id}/accept` on the invited tenant still existing before creating the
  membership (belt-and-suspenders for an accept racing a tenant delete). Cannot be written here — the
  accept endpoint is 6.3. With BUG1a done, delete now clears pending invites, so this is a secondary
  defense against the concurrent-accept race, not the primary fix.
  Deleting a tenant explicitly removes its memberships and the tenant row, but **not its
  `tenant.Invitations` rows**. There is no FK/cascade between `TenantInvitationEntity` and `TenantEntity`
  (bare-`Guid` `TenantId`, matching the membership pattern — `TenantInvitationEntityConfiguration.cs`
  configures no relationship), so nothing cleans them up. **Not hit in the current range** (6.2 has no
  way to create an invitation — the invite endpoint is 6.3), so it's not a defect in what ships today.
  But once 6.3 adds `POST api/organizations/invitations`, deleting an org will orphan its `Pending`
  invitations; if 6.3's `POST /api/invitations/{id}/accept` then creates the membership without
  re-checking the tenant still exists, an orphan invite would produce a membership for a deleted tenant.
  **Fix in 6.3:** remove the tenant's invitations in `DeleteCurrentTenantAsync` (and/or add a
  cascade-delete FK), and guard the accept path on tenant existence.

### Considered and dropped (below the confidence bar)

- `ListMembersAsync` indexes `emails[m.UserId]` (`MembershipService.cs:29`), which throws
  `KeyNotFoundException` if a member's user is absent from the User projection. This is the **intended
  fail-loud** behaviour (`GetEmailsByIdsAsync` is documented to omit unmatched ids rather than default
  them, per the root "don't default away a failure" rule), and a membership always corresponds to a
  registered user — the projection is populated for every member (confirmed by
  `GetMembers_AsOwner_ReturnsAllMembersWithEmails` asserting real emails). Not a practical bug.
- Theoretical TOCTOU on the last-Owner check (two concurrent Owner demotions each reading `CountOwners == 2`).
  Per-request scoped `DbContext`, genuinely rare, and outside the "hit in practice" bar.
