# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `C:/Users/tommy/source/repos/Concertable.worktrees/Feature/launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: [#772](https://github.com/Concertable/concertable/pull/772) (draft), head `2a66f1a03`
- Dependency/package gates: none — single-service (`Concertable.B2B`) + `app/web/admin` +
  `app/web/b2b/shared`, no published-contract boundary crossed
- Last reconciled: 2026-08-24, Phase 1 implemented, committed, and pushed in this session

## Current state

Phase 1 (domain: `TenantVerificationEntity` + evidence + migration) is complete and pushed as PR #772
(draft). Worktree tree is clean. No implementation started yet on Phase 2 (tenant-facing submission API).

## Next Steps

Continue in the existing worktree/branch/PR above — start Phase 2 of `TENANT_VERIFICATION_PLAN.md`
(tenant-facing submission API):

1. `cd` into the existing worktree (path above) — do not open a new one.
2. Add `IVerificationService`/`VerificationService` (Tenant.Application/Infrastructure):
   `GetOwnAsync`, `SubmitAsync(files, documentTypes)` — uploads each file to `verification-evidence/`
   via `IBlobStorageService`, transitions to `Pending` via `TenantVerificationEntity.Submit`/`Resubmit`.
3. `VerificationController` (`api/organization/verification`), `[Authorize]`,
   `[HasPermission(SharedPermissions.TenantSettingsEdit)]` on the mutating endpoint,
   `[EnableRateLimiting(RateLimitPolicies.Upload)]` on the upload endpoint.
4. Content-type allowlist (PDF/JPEG/PNG) and per-file size cap on the upload path.
5. Unit tests for the service; integration tests for the controller (round-trip submit → read status).
6. Build + focused tests; commit in the delivery worktree; push to PR #772.
7. Update this ledger **in the normal checkout** — never inside the delivery worktree.

## Completed work

- **Phase 1 — Domain** (`2a66f1a03`, PR #772): `TenantVerificationEntity` (`Pending`/`Approved`/`Rejected`,
  transitions validated through `Concertable.Kernel.StateMachine<TState, TTrigger>` — the first real
  consumer of that shared abstraction in this codebase) and `VerificationDocumentEntity` (append-only
  evidence, `Licence`/`ProofOfAddress`/`CompanyRegistration`). EF configurations composed into
  `TenantDbContext` (confirmed no new tenancy stance needed — `TenantDbContext` is already unscoped,
  matching `TenantMembershipEntity`/`TenantInvitationEntity`, neither of which is `ITenantScoped`).
  Migration re-scaffolded via `./initial-migrations.ps1`. 17 new unit tests.

## Verification

- `dotnet test Concertable.B2B.Tenant.UnitTests` (2026-08-24, commit `2a66f1a03`): 153 passed, 0 failed.
- `dotnet build Concertable.B2B.Web.csproj` (2026-08-24, commit `2a66f1a03`): 0 warnings, 0 errors — the
  full B2B service, every module.
- Full-solution `./initial-migrations.ps1` run hit an unrelated, pre-existing build error in
  `Concertable.Customer.DataAccess.Infrastructure` (MSB3030, CoreCompile silently producing no output) —
  reproduces on a clean worktree checkout but not on the normal checkout in the same session, and touches
  no file this plan changes. Not investigated further; flagged here in case it recurs for the next phase's
  full-solution build. The Tenant module's own re-scaffold (this phase's actual requirement) completed and
  is verified above.

## Reviews

None yet — no review recorded. Do not merge before a review, per the `plans` standard.

## Decisions, discoveries, blockers, and deviations

- Verification is modeled on `Tenant` (new `TenantVerificationEntity`), not duplicated onto
  `Venue`/`Artist` — see plan §1.1 for the full rationale (mirrors `TenantEntity.TaxCompliance` +
  `ITenantModule.IsTaxComplianceCompleteAsync`, which `FinishExecutor` already consumes as a fail-closed
  gate). This is a load-bearing decision for every later phase — do not re-derive or re-litigate it.
- Only two enforcement points, exactly as scoped: opportunity publication and settlement. Artist
  Application/Apply is deliberately not gated — see plan §1.4.
- Phase 6 (removing `VenueEntity.Approved` and its admin surface) must not start before Phase 3's new
  gate is merged and green — the old signal cannot be dropped before the new one is proven.
- `TenantVerificationEntity` raises `TenantVerificationChangedDomainEvent` on every transition (Submit/
  Resubmit/Approve/Reject) with no handler yet — legal per the domain-events standard ("zero handlers for
  an event is valid"). No consumer is scoped in this plan; a future phase (or a separate one) may add a
  pre-commit handler if a real need arises (e.g. activity-feed integration). Do not add one speculatively.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
