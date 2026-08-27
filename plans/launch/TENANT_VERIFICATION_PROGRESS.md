# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `.worktrees/Feature-launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: not opened — Phase 5 (`3c77f8115`) + Phase 6 land together in a fresh PR off current `main`
  (Phase 4's #799 merged `c99c7795c`)
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-27, Phase 5 committed + branch rebased on `main`; Phase 6 in progress

## Current state

Phases 1–4 merged to `main`. **Phase 5 code-complete and committed (`3c77f8115`, rebased to `43b42bfb7`)**
— admin SPA `features/verification`, tenant-facing banner + evidence-upload form in `app/web/b2b/shared`,
new `/settings/verification` routes in venue + artist SPAs. All five web builds green, `lint:boundaries`
green, focused FE tests pass. **One Phase 5 item outstanding:** manual in-app smoke (submit / approve /
reject / publication-block / banner) — needs the local OIDC + B2B stack; run it before the plan closes at
the end of Phase 6.

Phase 6 (retire the decorative `VenueEntity.Approved` surface) now in progress in this worktree, stacked
on the Phase 5 commit.

## Next Steps

Execute the plan's **Phase 6** checklist in this worktree:

1. Remove `VenueEntity.Approved` / `Approve()` (keep `VenueChangedDomainEvent` for other profile changes),
   `VenueController` `[Admin] PATCH {venueId}/approve` + `[Admin] GET pending-approval`,
   `IVenuePrivilegedRepository.GetPendingApprovalAsync`, `IVenueService.ApproveAsync` /
   `GetPendingApprovalAsync`, `ApproveVenueError`, and the `Approved` mapping in the Venue response/queryable
   mappers. Drop `Approved` from `SeedState` venue seeding.
2. Remove `app/web/admin/src/features/venues/` + its route and nav entry.
3. Re-scaffold the Venue module migration via `./initial-migrations.ps1` from `api/` (never additive).
4. Run the removal grep gate for `VenueEntity.Approved`, `venueService.ApproveAsync`, `pending-approval`,
   `ApproveVenueError` — zero remaining or a justified allowlist entry.
5. Build affected projects + focused tests; run the five web builds; commit.
6. Tick `launch/tenant-verification` in `plans/launch/LAUNCH_ROADMAP.md` and its §7 Architecture-checklist
   line; delete this plan + ledger in the final verified commit.
7. Run the deferred Phase 5 manual smoke, then hand off for review. Do not start unrelated launch-gate work
   in this worktree.

## Completed work

- **Phase 1 — Domain** (PR #772, merged `5222bce51`, clean): `TenantVerificationEntity` +
  `VerificationDocumentEntity` (append-only evidence), transitions via `Concertable.Kernel.StateMachine`.
- **Phase 2 — Tenant-facing submission API** (PR #784, merged `1867f0a72`, clean):
  `IVerificationService` / `VerificationController` (`api/organization/verification`), evidence upload via
  `IBlobStorageService`, content-type + magic-byte + size validation.
- **Phase 3 — Cross-module gate + enforcement** (PR #792, merged `564649a26`, clean):
  `ITenantModule.IsVerifiedAsync` (fail-closed, mirrors `IsTaxComplianceCompleteAsync`); enforced at
  `OpportunityService.CreateAsync` / `CreateMultipleAsync` (`OpportunityMutationError.VenueNotVerified`,
  `opportunity.venue_not_verified`) and `FinishExecutor.FinishAsync`
  (`SettlementOutcome.DeferredPendingVerification`, after the tax-compliance pair check). Seed:
  `SeedState.Verifications` gives every tax-compliant seeded tenant an `Approved` row;
  `SeedState.UnverifiedTenant` / `UnverifiedVenueManager` (+ seeded venue `9001`) isolate the new gate.
  Sync PR #794 merged.
- **Phase 4 — Admin review + cross-module contact + notification** (PR #799, merged `c99c7795c`):
  consolidated into the existing `VerificationController` / `VerificationService` (not a separate admin
  controller) — `[Admin] GET api/verification/pending`, `POST {tenantId}/approve`, `POST {tenantId}/reject`.
  `IVenueModule` / `IArtistModule` gain `GetContactByTenantIdAsync`; `TenantContact` is a
  `readonly record struct` per module's own Contracts. `IVerificationNotifier` emails on decision (direct
  call, `ContentReportNotifier` shape). Two review findings fixed on branch (see Reviews).
- **Phase 5 — Admin SPA + tenant-facing UI** (`3c77f8115`, not yet PR'd): admin `features/verification`
  (paged pending queue, approve, reason-required reject dialog) + `/_admin/verification` route/nav;
  `app/web/b2b/shared` `features/verification` — `VerificationBanner` (mirrors DAC7 `TaxDetailsBanner`,
  added to both manager dashboards), `VerificationPage` + `VerificationForm` (three fixed doc-type uploads,
  zod schema mirroring the backend allowlist), multipart POST with PascalCase enum tokens. New
  `/settings/verification` route + nav in venue + artist SPAs; new `./features/verification` package export.
  Five web builds + `lint:boundaries` green; focused FE tests pass.

## Verification

- Phase 5: all five `app/web` builds green + `lint:boundaries` green; focused FE tests
  (`usePendingVerifications`, `submitVerificationRequestSchema`) pass — 2026-08-27, commit `3c77f8115`.
- Backend suites last green at Phase 4 (`c99c7795c`): Tenant unit 174 / arch 18 / Tenant integration 82 /
  Venue integration 35 / Artist integration 22.
- Merge-queue E2E tier is the merge gate (`merge` skill Step 4) — not run locally.

## Reviews

No review recorded yet for the Phase 5 + Phase 6 slice. Phase 4's review file was deleted on merge (all
findings resolved). Phase 5 + 6 need a fresh `reviews/Feature-launch_tenant-verification.md` before merge.

## Decisions, discoveries, blockers, and deviations

- Verification stays modeled on `Tenant` (`TenantVerificationEntity`), not duplicated onto `Venue` /
  `Artist` — plan §1.1, load-bearing for every phase, do not re-litigate.
- Only two enforcement points: opportunity publication and settlement. Artist Apply is deliberately not
  gated — plan §1.4.
- **Phase 5 manual in-app smoke is outstanding** — deferred (needs local OIDC + B2B stack). Not a code
  blocker for Phase 6; must be run before the plan closes out.
- **Seeding is load-bearing for Phase 6.** Every seeded tenant has an `Approved` verification row
  (`SeedState.Verifications`); Phase 6 re-scaffolds the Venue migration and must also strip `Approved` from
  the Venue seed factory without disturbing verification seeding. Any new seeded venue/artist fixture needs
  an explicit verified/unverified decision, same as tax-compliance.
- **Reuse `SeedState.UnverifiedTenant` / `UnverifiedVenueManager`** for any "needs one unverified party"
  test rather than inventing another fixture.
- **`Concertable.B2B.E2ETests/AppFixture.cs`'s standalone seed host hand-duplicates a subset of
  `AddB2BWebHost`'s registrations** — check its registrations before assuming a new (or removed)
  `IVenueModule` / `IArtistModule` / `ITenantModule` dependency behaves in E2E. MED tech-debt entry in
  `api/Concertable.B2B/TECH_DEBT.md`.
- Per `unit-testing`, admin-service orchestration over several collaborators defaults to the integration
  tier — prefer integration coverage for any Phase 6 test work over mocked unit tests.
- A cross-repo standards gap from Phase 2's review (`tomjseery/dotagents` PR #12) is still open, unmerged —
  unrelated to this plan.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
