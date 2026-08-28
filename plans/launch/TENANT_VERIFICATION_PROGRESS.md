# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree:
- Branch: `Feature/launch_tenant-verification`
- PR: [#799](https://github.com/Concertable/concertable/pull/799) — **MERGED**, merge commit
  `c99c7795c`, 2026-08-27T16:48:20Z. Landed `full-e2e` tier (new observable admin/notification HTTP
  behavior).
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-27, Phase 4 merged; starting Phase 5 next

## Current state

Phases 1–4 merged to `main`. Phase 5 (`app/web/admin` verification feature + tenant-facing UI) not yet
started — no worktree open. Phase 4's `Feature/launch_tenant-verification` branch/worktree were deleted
on merge; Phase 5 recreates the same branch name fresh from current `main`.

## Next Steps

1. Start Phase 5 in a fresh worktree — `/open-worktree Feature/launch_tenant-verification` from current
   `main` (Phase 4's branch of the same name was deleted on merge, so this recreates it clean).
2. Scope is the plan's Phase 5 checklist: `app/web/admin/src/features/verification/` (mirroring
   `features/venues`), the `routes/_admin/verification.tsx` route + nav entry, and the tenant-facing
   status banner + evidence-upload form in `app/web/b2b/shared` (mirroring the DAC7 tax-compliance nag).
   Run all five web builds (`app/web/AGENTS.md`'s boundary gate) and manually verify submit/approve/reject
   plus the publication-block and dashboard banner in the running app before marking it done.

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
- **Phase 4 — Admin review + cross-module contact + notification** (PR #799, **merged** `c99c7795c`,
  2026-08-27): consolidated into the existing `VerificationController`/`IVerificationService`/
  `VerificationService` (not a separate admin controller/service) after design review, matching
  `VenueController`/`VenueService`'s one-controller-per-resource shape — `[Admin]`-gated `GET
  api/verification/pending`, `POST {tenantId}/approve`, `POST {tenantId}/reject` alongside the existing
  self-service `GET/POST api/organization/verification*` actions, all sharing one `RouteSegment` const.
  `IVenueModule`/`IArtistModule` gain a symmetric `GetContactByTenantIdAsync(tenantId)`; `TenantContact`
  is a `readonly record struct` declared once per module's own Contracts project (Venue, Artist),
  matching the existing `DisplayNames` precedent — decided against `Concertable.Kernel` because both are
  already-packable, per-service-published projects. `IVerificationNotifier`/`VerificationNotifier`
  emails the tenant on a decision, mirroring `ContentReportNotifier`'s direct-call shape. Two review
  findings fixed on the branch (see Reviews below). 174 unit tests unchanged, 18 architecture tests
  unchanged, 82 Tenant integration tests (+16 new), 35 Venue + 22 Artist integration tests unchanged —
  all green.

## Verification

- `dotnet test` on `Concertable.B2B.Tenant.UnitTests` (174), `Concertable.B2B.ArchitectureTests` (18),
  `Concertable.B2B.Tenant.IntegrationTests` (82), `Concertable.B2B.Venue.IntegrationTests` (35),
  `Concertable.B2B.Artist.IntegrationTests` (22) — 2026-08-26: all passed, 0 failed.
- PR #799 merged via the merge queue at `full-e2e` tier, `c99c7795c`, 2026-08-27T16:48:20Z.

## Reviews

Phase 4's `reviews/Feature-launch_tenant-verification.md` was reviewed clean through several incremental
passes (controller/service consolidation, the `TenantContact` struct conversion, two E2E DI-registration
fixes — see Decisions below) up to the merged commit. Two original findings, both fixed on the branch: a
`Task.WhenAll` DbContext-concurrency bug in the admin pending-queue's contact enrichment (two same-
`TenantType` pending rows on one page crashed the endpoint — two scoped read-DbContexts hit
concurrently), and a missing try/catch around the approve/reject notification call (mirrors
`ContentReportService`'s established shape: a transport failure must not fail a request whose write
already committed). No security findings. Deleted now that every finding is resolved and the PR has
merged, per the review-lifecycle standard.

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
- **Phase 4 deviated from the plan's literal "unit tests for the admin service and notifier"**: per the
  `unit-testing` standard, an orchestration service with several mocked collaborators defaults to the
  integration tier, not mocked unit tests. Covered instead by `TenantVerificationAdminApiTests.cs`
  (integration), which caught the `Task.WhenAll` concurrency bug a mocked unit test would have hidden.
  Applies to Phase 5/6 too: prefer integration coverage for any further admin-service orchestration work.
  Also, `PendingVerification` from the plan's own text is `PendingVerificationProjection` in code —
  `PendingVerification` was reserved for the enriched, API-facing DTO name.
- Contact enrichment in the admin pending-queue is one query per row, not batched per page (fixed to be
  sequential rather than concurrent for correctness; batching is a separate, logged follow-up —
  `api/Concertable.B2B/TECH_DEBT.md`, "Admin verification queue enriches contact per row, not per page").
- **`Concertable.B2B.E2ETests/AppFixture.cs`'s standalone seed host under-provisioned itself once
  `VerificationService` gained `IVenueModule`/`IArtistModule` dependencies**: `ConcertDevSeeder` (a
  registered `IDevSeeder`, eagerly constructed via `IEnumerable<IDevSeeder>`) already depended on
  `ITenantModule`, so extending `VerificationService`'s graph pulled `VenueService`/`ArtistService` into
  a host that never registered `AddSharedImaging`/`AddSharedGeocoding`/`AddSharedEmail` (unlike the real
  app host, which always has). Fixed by adding all three; a MED tech-debt entry in
  `api/Concertable.B2B/TECH_DEBT.md` flags the structural risk (the seed host hand-duplicates a subset of
  `AddB2BWebHost`'s registrations rather than reusing it) for whoever's diff next extends a cross-module
  facade's dependency graph — **check that file's registrations before assuming a new
  `IVenueModule`/`IArtistModule`/`ITenantModule` dependency "just works" in E2E.**
- **Unrelated to this plan but touches a file Phase 5 will likely also touch**: `app/shared/src/features/venues/hooks/useMyVenue.ts` was migrated to `react-hook-form` + `zodResolver`
  by a concurrent, unrelated change while this phase was in flight (commit `f8fe9fe5d`, "validate form
  when editing opportunities") — entering edit mode must call `trigger()` after `reset()`, or
  `formState.isValid` (and anything gating Save on it) stays stale until a field changes, which broke the
  UI E2E "post an opportunity" happy paths for one merge-queue cycle. If Phase 5's evidence-upload
  form or tenant-facing banner uses `useMyVenue`-style edit/save wiring, check this pattern is followed.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
