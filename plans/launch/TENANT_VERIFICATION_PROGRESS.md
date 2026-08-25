# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: none — Phase 1's worktree closed after merge; Phase 2 opens a fresh one
- Branch: next proposed `Feature/launch_tenant-verification` (Phase 1's branch merged and deleted)
- PR: [#772](https://github.com/Concertable/concertable/pull/772) — **MERGED** (`5222bce51`). Its
  version-sync [#778](https://github.com/Concertable/concertable/pull/778) (`0.1.0-alpha.0.1181`) was
  superseded (closed, not merged) by an unrelated later publish from PR #776 — "only one platform-sync PR
  is ever live" — before it could land; ownership transferred to that producer per the `merge` skill.
  Phase 1's own delivery obligation ends at its own successful publish, which completed.
- Dependency/package gates: none — single-service (`Concertable.B2B`) + `app/web/admin` +
  `app/web/b2b/shared`, no published-contract boundary crossed
- Last reconciled: 2026-08-25, Phase 1 fully terminal (merged, published, sync resolved by supersession)

## Current state

Phase 1 (domain: `TenantVerificationEntity` + evidence + migration) is merged to `main` (PR #772, reviewed
clean) and fully terminal — its publish succeeded; its sync PR was superseded by an unrelated later publish
rather than needing action here. No implementation started yet on Phase 2 (tenant-facing submission API).

## Next Steps

1. `/open-worktree Feature/launch_tenant-verification` (branch off current `origin/main`) and start
   Phase 2 of `TENANT_VERIFICATION_PLAN.md` (tenant-facing submission API):
   - Add `IVerificationService`/`VerificationService` (Tenant.Application/Infrastructure):
     `GetOwnAsync`, `SubmitAsync(files, documentTypes)` — uploads each file to `verification-evidence/`
     via `IBlobStorageService`, transitions to `Pending` via `TenantVerificationEntity.Submit`/`Resubmit`.
   - `VerificationController` (`api/organization/verification`), `[Authorize]`,
     `[HasPermission(SharedPermissions.TenantSettingsEdit)]` on the mutating endpoint,
     `[EnableRateLimiting(RateLimitPolicies.Upload)]` on the upload endpoint.
   - Content-type allowlist (PDF/JPEG/PNG) and per-file size cap on the upload path.
   - Unit tests for the service; integration tests for the controller (round-trip submit → read status).
   - Build + focused tests; commit; review; push to a new PR.
2. Update this ledger **in the normal checkout** — never inside the delivery worktree.

## Completed work

- **Phase 1 — Domain** (PR #772, **merged** `5222bce51`, reviewed clean): `TenantVerificationEntity`
  (`Pending`/`Approved`/`Rejected`, transitions validated through `Concertable.Kernel.StateMachine<TState,
  TTrigger>` — the first real consumer of that shared abstraction in this codebase) and
  `VerificationDocumentEntity` (append-only evidence, `Licence`/`ProofOfAddress`/`CompanyRegistration`).
  EF configurations composed into `TenantDbContext` (confirmed no new tenancy stance needed —
  `TenantDbContext` is already unscoped, matching `TenantMembershipEntity`/`TenantInvitationEntity`,
  neither of which is `ITenantScoped`). Migration re-scaffolded via `./initial-migrations.ps1`.
  19 unit tests. Skip-e2e tier (domain-only, no HTTP/UI/published-contract surface).

## Verification

- `dotnet test Concertable.B2B.Tenant.UnitTests` (2026-08-24, commit `89c9addfc`): 156 passed, 0 failed.
- `dotnet build Concertable.B2B.Web.csproj` (2026-08-24, commit `2a66f1a03`): 0 warnings, 0 errors — the
  full B2B service, every module.
- Full-solution `./initial-migrations.ps1` run hit an unrelated, pre-existing build error in
  `Concertable.Customer.DataAccess.Infrastructure` (MSB3030, CoreCompile silently producing no output) —
  reproduces on a clean worktree checkout but not on the normal checkout in the same session, and touches
  no file this plan changes. Not investigated further; flagged here in case it recurs for the next phase's
  full-solution build. The Tenant module's own re-scaffold (this phase's actual requirement) completed and
  is verified above.

## Reviews

`reviews/Feature-launch_tenant-verification.md`, reviewed up to `20a5061f1` (marker) / pushed to `89c9addfc`
(review-file-only commit on top — current per the review-only exception). 2 findings, both fixed and
verified: **NAT1** — `TenantVerificationChangedDomainEvent` carried a live entity reference instead of a
snapshot, risking stale data if two transitions raised before one dispatch; now snapshots primitives via a
new `Announce()` helper. **NAT2** — `Reject`/`VerificationDocumentEntity.Create` didn't validate length
against their EF max-length columns; now throw `DomainException` instead of hitting a raw SQL error at
`SaveChanges`. 0 open findings — clear to merge.

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
