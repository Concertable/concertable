# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: none — not yet opened; planning creates no delivery worktree
- Branch: next proposed `Feature/launch_tenant-verification`
- PR: not opened
- Dependency/package gates: none — single-service (`Concertable.B2B`) + `app/web/admin` +
  `app/web/b2b/shared`, no published-contract boundary crossed
- Last reconciled: 2026-08-23, plan written from repository evidence in the normal checkout (this session)

## Current state

Plan and ledger just written in the normal checkout; no implementation started, no delivery worktree
open.

Verified before writing the plan (this session, 2026-08-23):
- No branch, worktree, ledger, or open PR previously owned this roadmap item.
- `VenueEntity.Approved` (`api/Concertable.B2B/src/Modules/Venue/.../VenueEntity.cs`) is set once by
  `Approve()` and read by nothing except `VenuePrivilegedRepository.GetPendingApprovalAsync`'s own
  filter — no query, guard, or workflow elsewhere reads it.
- `ArtistEntity` carries no verification concept at all.
- The exact fail-closed dual-gate pattern this plan follows already exists twice in
  `FinishExecutor.FinishAsync` (tax compliance via `ITenantModule.IsTaxComplianceCompleteAsync`, and
  self-billing via `ISelfBillingAgreementGate`) — both deferring to `SettlementOutcome` cases retried by
  the hourly `ConcertCompletionRunner` sweep.

## Next Steps

Open the delivery worktree and start Phase 1 of `TENANT_VERIFICATION_PLAN.md`:

1. `/open-worktree Feature/launch_tenant-verification` (branch off `origin/main`).
2. In that worktree, add `TenantVerificationEntity` + `VerificationDocumentEntity` to
   `Concertable.B2B.Tenant.Domain.Entities`, per plan §1.2/§1.3 and Phase 1's checklist.
3. Add EF configurations, compose into `TenantDbContext`, re-scaffold migrations via
   `./initial-migrations.ps1` from `api/`.
4. Write `TenantVerificationEntityTests` covering every legal/illegal transition.
5. Build `api/Concertable.slnx`, run Tenant module unit tests, commit the code in the delivery worktree.
6. Update this ledger and tick Phase 1 in the plan **in the normal checkout** — never inside the
   delivery worktree.

## Completed work

None yet.

## Verification

None run yet.

## Reviews

None yet — no review recorded. Do not merge before Phase 1's own review, per the `plans` standard.

## Decisions, discoveries, blockers, and deviations

- Verification is modeled on `Tenant` (new `TenantVerificationEntity`), not duplicated onto
  `Venue`/`Artist` — see plan §1.1 for the full rationale (mirrors `TenantEntity.TaxCompliance` +
  `ITenantModule.IsTaxComplianceCompleteAsync`, which `FinishExecutor` already consumes as a fail-closed
  gate). This is a load-bearing decision for every later phase — do not re-derive or re-litigate it.
- Only two enforcement points, exactly as scoped: opportunity publication and settlement. Artist
  Application/Apply is deliberately not gated — see plan §1.4.
- Phase 6 (removing `VenueEntity.Approved` and its admin surface) must not start before Phase 3's new
  gate is merged and green — the old signal cannot be dropped before the new one is proven.
- **Process correction (2026-08-23):** this plan was initially written inside a delivery worktree
  (`Feature/launch_tenant-verification`), because `agent-standards` PR #20 ("Keep planning state in the
  normal checkout") had CI-passed but was left unmerged. That PR is now merged
  (`ab1755df0a67d9f537fe6f74df4909b204b71286`, 2026-08-23). The wrongly-placed worktree/branch was
  deleted (it carried only the misplaced planning commit, no real delivery work) and this plan/ledger
  were rewritten here, in the normal checkout, per the corrected standard. Future phases must never
  write to a delivery worktree's copy of these files.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
