# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: none — Phase 3's worktree closed after merge; Phase 4 opens a fresh one
- Branch: next proposed `Feature/launch_tenant-verification` (Phase 3's branch merged and deleted)
- PR: [#792](https://github.com/Concertable/concertable/pull/792) — **MERGED** (`564649a26`), `full-e2e`
  label (new observable HTTP/settlement behavior — a positive trigger). Its causally-linked publish
  (`0.1.0-alpha.0.1195`) opened sync PR #794, which merged green automatically at `af8890dc0`.
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-26, Phase 3 fully terminal (merged, reviewed clean, sync merged green)

## Current state

Phases 1–3 are all merged to `main` and fully terminal. No implementation started yet on Phase 4 (admin
review + cross-module contact + notification).

## Next Steps

1. `/open-worktree Feature/launch_tenant-verification` (branch off fetched `origin/main`) and start
   Phase 4 of `TENANT_VERIFICATION_PLAN.md` (admin review + cross-module contact + notification):
   - Admin-listing query: `GetPendingAsync(pageParams)` over `TenantVerificationEntity` where
     `Status == Pending`, ordered by `SubmittedAt`.
   - Add `GetContactByTenantIdAsync(Guid tenantId, CancellationToken ct = default)` to `IVenueModule`
     and `IArtistModule` (returning `Option<TenantContact(Name, Email)>` — **decide in this phase**
     whether `TenantContact` lives in `Concertable.Kernel` or is declared once and duplicated per module
     contract — plan §1.6, an open implementation detail, not a design fork). Admin service composes the
     pending list with the correct module's contact by `TenantType`.
   - `TenantVerificationAdminController` (`[Admin]`, Tenant.Api): `GET pending`,
     `POST {tenantId}/approve`, `POST {tenantId}/reject`.
   - `IVerificationNotifier` (Tenant.Infrastructure), using `IEmailTransport`, called directly from the
     admin approve/reject service methods — mirrors `ContentReportNotifier`'s direct-call shape,
     resolving the target email via the new `GetContactByTenantIdAsync`.
   - Unit tests for the admin service and notifier; integration tests for the admin endpoints mirroring
     the venue-approval integration coverage from admin-console Phase 4.
   - Build + focused tests; commit; review (this phase adds `[Admin]` controllers under
     `Concertable.B2B.Tenant.Api` — `Controller[A-Za-z]*\.cs$` is a security-sensitive path here, so
     expect the stale-security-marker check to fire); push to a new PR.
2. Update this ledger **in the normal checkout** — never inside the delivery worktree.

## Completed work

- **Phase 1 — Domain** (PR #772, **merged** `5222bce51`, reviewed clean): `TenantVerificationEntity`
  (`Pending`/`Approved`/`Rejected`, transitions validated through `Concertable.Kernel.StateMachine<TState,
  TTrigger>`) and `VerificationDocumentEntity` (append-only evidence). 19 unit tests. Skip-e2e tier.
- **Phase 2 — Tenant-facing submission API** (PR #784, **merged** `1867f0a72`, reviewed clean):
  `IVerificationService`/`VerificationService`, `VerificationController`
  (`api/organization/verification`), evidence upload via `IBlobStorageService`. 174 unit tests, full
  integration coverage. `full-e2e` tier. Tech debt logged separately (`Genres` set-shaping, unrelated to
  verification) in Concert/Artist `TECH_DEBT.md`.
- **Phase 3 — Cross-module gate + enforcement** (PR #792, **merged** `564649a26`, reviewed clean):
  `ITenantModule.IsVerifiedAsync(tenantId)` (fail-closed, mirrors `IsTaxComplianceCompleteAsync`) via
  `TenantModule` → `VerificationService.IsVerifiedAsync` → `VerificationRepository.IsApprovedByTenantIdAsync`
  (a new `Any` query, no `.Include(Documents)`). Enforced at
  `OpportunityService.CreateAsync`/`CreateMultipleAsync` (new
  `OpportunityMutationError.VenueNotVerified`, `opportunity.venue_not_verified`, Forbidden) and
  `FinishExecutor.FinishAsync` (new `SettlementOutcome.DeferredPendingVerification`, logged via
  `Log.SettlementDeferredPendingVerification`, positioned immediately after the existing tax-compliance
  pair check and before the self-billing-agreement gate — no sweep changes needed). Seed fixtures
  extended: `SeedState.Verifications` gives every tax-compliant seeded tenant an `Approved` row (via new
  `VerificationFactory.Approved`), and a dedicated `SeedState.UnverifiedTenant`/`UnverifiedVenueManager`
  fixture (tax-complete, venue-owning, no verification row — outside `SeedUsers.Managers`, so it touches
  no shared cross-service seed package) isolates the new gate from the pre-existing tax-compliance one.
  Its causally-linked publish (`0.1.0-alpha.0.1195`) opened sync PR #794, which merged green — Phase 3's
  own delivery obligation ended there.

## Verification

- `dotnet test Concertable.B2B.Tenant.UnitTests` and `Concertable.B2B.Concert.UnitTests` (2026-08-26,
  commit `4a7145bd4`): 174 + 234 passed, 0 failed.
- PR #792's own CI (build, every carve, every unit/integration/architecture-tests project, `full-e2e`):
  all green before enqueueing; merge-queue `merge_group` run also green.

## Reviews

`reviews/Feature-launch_tenant-verification.md` — spent, deleted with this phase's close-out (per the
review-lifecycle standard). Final state before deletion: reviewed up to `d2821f682` / security-reviewed
up to `d7f398ffc` (the security-sensitive range — `.Contracts` — didn't move again after that commit).
No findings across the full review or its incremental follow-up. The incremental round caught a real
bug the static review couldn't: `VerificationApiTests` (Phase 2's own suite) needs its tenant to start
with no verification row, but Phase 3's new default-`Approved` seeding gave `VenueManager1` one — CI
failed 4 of 7 tests observably. Fixed by pointing that file at `UnverifiedVenueManager` instead.

## Decisions, discoveries, blockers, and deviations

- Verification stays modeled on `Tenant` (`TenantVerificationEntity`), not duplicated onto
  `Venue`/`Artist` — plan §1.1, load-bearing for every phase, do not re-litigate.
- Only two enforcement points, exactly as scoped: opportunity publication and settlement. Artist
  Application/Apply is deliberately not gated — plan §1.4.
- **Phase 6 (removing `VenueEntity.Approved` and its admin surface) is now unblocked** — Phase 3's gate
  is merged and green, so the old signal may be dropped once its replacement (Phase 4's admin review +
  Phase 5's UI) also ships. Still start Phase 6 only after Phase 5, per the plan's phase order.
- `IsVerifiedAsync` deliberately is **not** exposed on `ITenantContext` (the ambient tenant context in
  `Concertable.Kernel.Identity`, shared across all five services) — that type is deliberately anemic
  (`TenantId` + `IsHost` only), and the settlement gate must check two *other* tenants' verification
  (supplier/customer resolved off the concert, not the ambient request tenant) which an ambient-context
  field could never express. `ITenantModule.IsVerifiedAsync` mirrors `IsTaxComplianceCompleteAsync`'s
  shape exactly for the same reason.
- The fail-closed gate meant every seeded tenant needed an `Approved` verification row or the whole
  existing Concert/Opportunity/Tenant integration-test suite would defer — not called out explicitly in
  the plan's Phase 3 checklist text but a hard precondition. Fixed via `SeedState.Verifications` (mirrors
  the existing `bareTenantUserIds`/tax-compliance seeding pattern). **Load-bearing for Phase 4/5/6**: any
  new seeded tenant/venue/artist fixture added in later phases needs an explicit decision about whether
  it's verified, the same way tax-compliance already requires one.
- A tenant needing to be tax-complete but *specifically* unverified (isolating gates from each other, and
  for HTTP tests, one that also owns a venue and can authenticate) has no combination among the
  pre-existing seeded fixtures. `SeedState.UnverifiedTenant` + `UnverifiedVenueManager` (+ one extra
  seeded venue, id `9001`) fill that gap as a purely B2B-local addition — not part of the shared
  `Concertable.Seed.Identity.SeedUsers` roster, so no cross-service seed package is touched. **Reuse this
  fixture** for any future "needs one unverified party" test rather than inventing another.
- A cross-repo standards gap from Phase 2's review (`tomjseery/dotagents` PR #12, framework types off
  service signatures etc.) is still open, unmerged — unrelated to this plan, not re-checked here.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
