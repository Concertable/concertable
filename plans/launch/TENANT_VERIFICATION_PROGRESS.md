# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `.worktrees/Feature-launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: [#792](https://github.com/Concertable/concertable/pull/792) — **DRAFT**, `8be14e1b5`, awaiting
  exact-head CI (build/carve/unit-tests/integration-tests). Not yet marked ready — do that once CI is
  green, per this repo's `open-pr`/`merge` procedure.
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-26, Phase 3 implemented, reviewed clean (native + security, no findings),
  pushed; PR opened as draft, CI not yet observed

## Current state

Phases 1–3 are implemented. Phase 3 (cross-module `IsVerifiedAsync` gate + enforcement at opportunity
publication and settlement) is complete on `Feature/launch_tenant-verification`, committed
(`8be14e1b5`), reviewed clean, and pushed as draft PR #792. Nothing uncommitted in the worktree.

## Next Steps

1. Watch PR #792's CI (build, carve-*, unit-tests, integration-tests) reach green on `8be14e1b5`.
   - If CI is green: mark the PR ready for review, then proceed to `/merge` — tier selection is the
     `merge` skill's Step 4 call (this PR touches a controller/opportunity endpoint and a settlement
     workflow, so expect `full-e2e`, but let the merge skill decide).
   - If CI is red: diagnose in the `Feature-launch_tenant-verification` worktree per the
     `failing-tests`/`e2e-*` skills' tier table, fix, push a new commit to the same PR, re-check.
2. Once PR #792 merges: run `./scripts/worktrees.ps1 close -Worktree
   .worktrees/Feature-launch_tenant-verification -PullRequest 792 -PlanManaged` from the normal
   checkout, then update this ledger (Phase 3 → merged/terminal) and open a fresh worktree for
   **Phase 4** (admin review + cross-module contact + notification) — see `TENANT_VERIFICATION_PLAN.md`
   §Phase 4 for its full scope.
3. Update this ledger **in the normal checkout** — never inside the delivery worktree.

## Completed work

- **Phase 1 — Domain** (PR #772, **merged** `5222bce51`, reviewed clean): `TenantVerificationEntity`
  (`Pending`/`Approved`/`Rejected`, transitions validated through `Concertable.Kernel.StateMachine<TState,
  TTrigger>`) and `VerificationDocumentEntity` (append-only evidence). 19 unit tests. Skip-e2e tier.
- **Phase 2 — Tenant-facing submission API** (PR #784, **merged** `1867f0a72`, reviewed clean):
  `IVerificationService`/`VerificationService`, `VerificationController`
  (`api/organization/verification`), evidence upload via `IBlobStorageService`. 174 unit tests, full
  integration coverage. `full-e2e` tier. Tech debt logged separately (`Genres` set-shaping, unrelated to
  verification) in Concert/Artist `TECH_DEBT.md`.
- **Phase 3 — Cross-module gate + enforcement** (PR #792, **draft, pushed** `8be14e1b5`):
  `ITenantModule.IsVerifiedAsync(tenantId)` (fail-closed, mirrors `IsTaxComplianceCompleteAsync`) via
  `TenantModule` → `VerificationService.IsVerifiedAsync` → `VerificationRepository.IsApprovedAsync`
  (a new `Any` query, no `.Include(Documents)`). Enforced at
  `OpportunityService.CreateAsync`/`CreateMultipleAsync` (new
  `OpportunityMutationError.VenueNotVerified`, `opportunity.venue_not_verified`, Forbidden) and
  `FinishExecutor.FinishAsync` (new `SettlementOutcome.DeferredPendingVerification`, logged via
  `Log.SettlementDeferredPendingVerification`, positioned immediately after the existing tax-compliance
  pair check and before the self-billing-agreement gate — no sweep changes needed). Seed fixtures
  extended: `SeedState.Verifications` gives every tax-compliant seeded tenant an `Approved` row (via new
  `VerificationFactory.Approved`, persisted by the existing `TenantTestSeeder`/`TenantDevSeeder`), and a
  dedicated `SeedState.UnverifiedTenant`/`UnverifiedVenueManager` fixture (tax-complete, venue-owning, but
  no verification row — outside `SeedUsers.Managers`, so it touches no shared cross-service seed package)
  isolates the new gate from the pre-existing tax-compliance one. New tests:
  `TenantVerificationGateApiTests` (settlement defers/settles) and
  `OpportunityApiTests.Create_ShouldReturn403_WhenVenueNotVerified`. 174 + 234 unit tests still green.

## Verification

- `dotnet test Concertable.B2B.Tenant.UnitTests` and `Concertable.B2B.Concert.UnitTests` (2026-08-26,
  commit `8be14e1b5`): 174 + 234 passed, 0 failed.
- `dotnet build` for `Concertable.B2B.Web`, `Concertable.B2B.Tenant.IntegrationTests`,
  `Concertable.B2B.Concert.IntegrationTests` (2026-08-26, commit `8be14e1b5`): 0 errors. Per this repo's
  `remote-validation` policy, the integration/E2E suites themselves run on PR CI, not locally.
- PR #792's own CI: not yet observed — draft, just pushed.

## Reviews

Reviewed at commit `8be14e1b5` — native layer (correctness, reuse, simplification, efficiency, error
handling) and the repo's architecture lenses (service isolation — n/a, no service boundary crossed;
module boundaries; data seeding; language/framework conventions; test coverage): **no findings**.
Security-reviewed at the same commit (diff touches `.Contracts`): **no findings** — tenant-id sourcing
verified server-side at both call sites (`ITenantContext`/`IDealPayeeResolver`, never caller-supplied),
`IsApprovedAsync`'s EF query is parameterized LINQ, the new seed fixture doesn't leak outside dev/test
seeders. Review file deleted (untracked, no findings — spent immediately per the review-lifecycle
standard).

## Decisions, discoveries, blockers, and deviations

- Verification stays modeled on `Tenant` (`TenantVerificationEntity`), not duplicated onto
  `Venue`/`Artist` — plan §1.1, load-bearing for every phase, do not re-litigate.
- Only two enforcement points, exactly as scoped: opportunity publication and settlement. Artist
  Application/Apply is deliberately not gated — plan §1.4.
- Phase 6 (removing `VenueEntity.Approved` and its admin surface) must not start before Phase 3 is
  **merged and green** — the old signal cannot be dropped before the new one is proven. Phase 3 is not
  yet merged (draft PR, CI not yet observed), so Phase 6 stays blocked.
- `IsVerifiedAsync` deliberately is **not** exposed on `ITenantContext` (the ambient tenant context in
  `Concertable.Kernel.Identity`, shared across all five services) — that type is deliberately anemic
  (`TenantId` + `IsHost` only), consumed by every service's tenant-filtering plumbing, and the settlement
  gate must check two *other* tenants' verification (supplier/customer resolved off the concert, not the
  ambient request tenant) which an ambient-context field could never express. `ITenantModule.IsVerifiedAsync`
  mirrors the existing `IsTaxComplianceCompleteAsync` shape exactly for the same reason.
- The fail-closed gate meant every seeded tenant needed an `Approved` verification row or the whole
  existing Concert/Opportunity/Tenant integration-test suite would start deferring — this was not called
  out explicitly in the plan's Phase 3 checklist text but was a hard precondition; fixed via
  `SeedState.Verifications` (mirrors the existing `bareTenantUserIds`/tax-compliance seeding pattern) and
  `VerificationFactory.Approved`.
- The two new gate tests need a tenant that is tax-complete but *specifically* unverified (isolating the
  new gate from the pre-existing tax-compliance gate, and — for the opportunity test — one that also owns
  a venue and can authenticate). None of the existing seeded fixtures combine those properties (the two
  "bare" operators are unverified *and* tax-incomplete). Added `SeedState.UnverifiedTenant` +
  `UnverifiedVenueManager` (+ one extra seeded venue, id `9001`) as a purely B2B-local addition — not
  part of the shared `Concertable.Seed.Identity.SeedUsers` roster, so it touches no cross-service seed
  package (Auth's credential seed, the B2B seeding simulator, Customer's projection seeders are all
  unaffected).
- A cross-repo standards gap from Phase 2's review (`tomjseery/dotagents` PR #12, framework types off
  service signatures etc.) is still open, unmerged — unrelated to this phase, not re-checked here.

## Resume prompt

```
cd .worktrees/Feature-launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
