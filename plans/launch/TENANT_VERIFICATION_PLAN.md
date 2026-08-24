# Tenant verification — venue and artist legitimacy

> **Next steps live in @plans/launch/TENANT_VERIFICATION_PROGRESS.md → `## Next Steps`.**

> **Hard launch gate.** `VenueEntity.Approved` is set once at admin discretion and never read by any
> query filter, guard or workflow — an unapproved venue publishes opportunities, accepts artists and
> takes money identically to an approved one. Artists have no verification concept at all. This plan
> replaces the decorative bool with a real, evidence-backed verification state machine owned by the
> `Tenant` module (not Venue, not Artist), enforced fail-closed at opportunity publication and at
> settlement.

## 1. Locked design decisions

### 1.1 Verification lives on `Tenant`, not on `Venue`/`Artist`

`TenantEntity` already carries an optional `TaxCompliance` value object (null until organization setup
completes) exposed cross-module through `ITenantModule.IsTaxComplianceCompleteAsync`, consumed by
`FinishExecutor` as a fail-closed settlement gate. Verification is the same shape of fact — a
tenant-level legitimacy state, not a venue- or artist-specific one — so it follows the same precedent
rather than duplicating a state machine once in Venue and once in Artist:

- one `TenantVerificationEntity` per tenant (Tenant module), not two separate flags;
- exposed cross-module as `ITenantModule.IsVerifiedAsync(Guid tenantId)`, named and shaped exactly like
  `IsTaxComplianceCompleteAsync`;
- automatically covers artists the day it ships — no separate artist-side build.

This **replaces** `VenueEntity.Approved`, `VenueEntity.Approve()`, and the venue-only admin
approve/pending-approval surface (API + admin SPA `features/venues`) rather than running the two gates
in parallel. Per this repo's symmetric-rename convention, the old surface is removed in the same plan
that ships its replacement (Phase 6), not left as a second, competing signal.

**Rejected alternative:** a `VenueVerificationEntity` + `ArtistVerificationEntity` pair mirroring the
existing per-module `Approved` bool. Rejected because it duplicates an identical state machine and
evidence-upload path twice, and because settlement already resolves the two settlement-relevant tenants
(`supplierTenantId`/`customerTenantId`) via `IDealPayeeResolver`, not via Venue/Artist ids — a
tenant-keyed gate composes directly with that resolver; a venue/artist-keyed one would need an extra
lookup at every call site.

### 1.2 State machine

```text
(no row) --submit evidence--> Pending --admin approve--> Approved
                                  |
                                  +--admin reject(reason)--> Rejected --submit evidence--> Pending
```

- No `TenantVerificationEntity` row = never submitted = **not verified** (fails closed, same as null
  `TaxCompliance`).
- `Approved` is the only verified state. `IsVerifiedAsync` returns `Status == Approved`, `false`
  otherwise (including no row, `Pending`, `Rejected`).
- Evidence is a 1-to-many collection (`VerificationDocumentEntity`) on the aggregate: a submission or
  re-submission appends new documents and does not delete prior ones — the admin trail must show what
  was reviewed each time, matching the append-only-evidence posture used elsewhere in B2B (ledger,
  audit-relevant entities).
- Every transition (submit, approve, reject) is a domain method on `TenantVerificationEntity`,
  validated the same way `TenantEntity.UpdateLegalDetails` and `VenueEntity.Approve` are: illegal calls
  throw `DomainException`, never silently no-op.

### 1.3 Evidence documents

Three document types, matching the roadmap wording exactly — `Licence`, `ProofOfAddress`,
`CompanyRegistration` (a closed enum, not free text). Stored via the existing
`Concertable.Shared.Blob.Application.IBlobStorageService` (the same abstraction booking agreements and
self-billing agreements already use), on its own prefix:

```text
verification-evidence/{tenantId}-{documentType}-{guid}.{ext}
```

Uploaded through a plain multipart endpoint (`IFormFile[]`), not through `IImageService` — these are
PDFs and photographs of documents, not profile imagery needing resize/replace semantics. Content-type
allowlist and size cap follow the same validation shape `IImageService` already applies; add a
dedicated check rather than reusing the image-specific service.

### 1.4 Two enforcement points, exactly as scoped — no others

- **Opportunity publication** — `OpportunityService.CreateAsync`/`CreateMultipleAsync` (Concert module):
  gate on the publishing venue's tenant. An unverified venue cannot create an opportunity.
- **Settlement** — `FinishExecutor.FinishAsync` (Concert module): gate on **both** settlement-relevant
  tenants (`supplierTenantId` and `customerTenantId` from `IDealPayeeResolver`), exactly mirroring the
  existing dual tax-compliance check immediately above it. Deferred, not failed — the concert stays
  `Booked` and the hourly `ConcertCompletionRunner` sweep retries once verification completes, self-
  healing exactly like `DeferredPendingTaxCompliance`/`DeferredPendingSelfBillingAgreement`.

**Deliberately not gated:** artist Application/Apply. The roadmap's own checklist line and §5 row name
only "opportunity publication + settlement" as the enforced points. An unverified artist can still apply
to an opportunity; they are blocked at settlement (the same place the tax-compliance and self-billing
gates already stop an unready counterparty), which is sufficient to stop money moving to an unverified
party. Adding a third gate at Apply is scope beyond what was asked and beyond what the roadmap's
definition-of-done checklist requires.

### 1.5 Admin review surface

New `[Admin]`-gated endpoints in `Concertable.B2B.Tenant.Api` (domain-specific admin actions live in
their owning module's Api project under `[Admin]`, exactly like `VenueController`'s existing
approve/pending-approval and `ModerationController` in Conversations.Api — never centralized in
`Concertable.B2B.Admin.Api`, which owns only the platform admin roster/invite surface):

```text
[Admin] GET  /api/tenant/verification/pending                  paged, all tenant types
[Admin] POST /api/tenant/verification/{tenantId}/approve
[Admin] POST /api/tenant/verification/{tenantId}/reject        body: { reason }
```

Tenant-facing endpoints on the existing `OrganizationController` pattern, new
`VerificationController` under `/api/organization/verification`, gated by
`SharedPermissions.TenantSettingsEdit` — the same permission `OrganizationController.Update` already
requires for editing `TaxCompliance`, since verification evidence is the same class of compliance-
sensitive, owner-tier action, not a cosmetic profile edit (`ProfileEdit`):

```text
GET  /api/organization/verification              current status, reason, documents
POST /api/organization/verification/documents     [FromForm] multipart submit/re-submit
```

Rate-limited under the existing `RateLimitPolicies.Upload` policy (already used for other blob-upload
endpoints) — no new policy needed.

### 1.6 Cross-module contact lookup for notification and admin listing

`IVenueModule`/`IArtistModule` currently expose no tenant-keyed lookup (`GetSummaryAsync` takes the
Venue/Artist `int` id, not `TenantId`). The admin pending-list needs the venue/artist name to display,
and the notifier needs the business email. Add one small facade method to each:

```csharp
Task<Option<TenantContact>> GetContactByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
// TenantContact(string Name, string Email)
```

`TenantContact` lives in `Concertable.Kernel` or is declared once and duplicated per module contract
(decide in Phase 4 based on whether a shared shape already exists) — implementation detail, not a
design fork.

### 1.7 Notification

Reuse `Concertable.Shared.Email.Application.IEmailTransport` directly — the same channel
`ContentReportNotifier` (Conversations module) already uses for a comparable "admin acted on this
tenant's submission, tell them" notification. A new `IVerificationNotifier` in
`Concertable.B2B.Tenant.Infrastructure.Services`, called directly from the admin approve/reject service
methods (not via a domain event — `ContentReportNotifier` is called directly from its service too, and
there is no other consumer of an approve/reject event to justify the extra indirection).

## 2. Designs considered

| Design | Result |
|---|---|
| Keep verification on `VenueEntity`/`ArtistEntity`, add a matching bool+reason to `ArtistEntity` | rejected: duplicates the state machine and evidence path twice; settlement already resolves tenants, not venue/artist ids |
| Model verification as a value object on `TenantEntity` (like `TaxCompliance`) instead of its own entity | rejected: evidence documents are a genuine 1-to-many collection with their own lifecycle (upload timestamps, resubmission history) — `TaxCompliance` has no such collection, so the VO shape doesn't fit |
| Gate artist verification at Application/Apply as well as settlement | rejected: not in the roadmap's enforced-points list; settlement is sufficient to stop money moving, and the two named gates are the definition of done |
| Route admin verification actions through the platform `AdminController`/`Admin.Api` | rejected: that module owns the admin roster/invite surface only; every existing domain-specific admin action (`VenueController.Approve`, `ModerationController`) lives in its owning module under `[Admin]` |
| Use `IImageService` for evidence upload | rejected: image-specific resize/replace semantics don't apply to PDFs/scans; use `IBlobStorageService` directly like agreements/invoices do |

## 3. Phases

Single service (`Concertable.B2B`) + one admin SPA (`app/web/admin`) + the b2b-shared org UI. No
published-package boundary is crossed, so no publish/platform-sync hard stop is needed anywhere in this
plan — unlike `PLATFORM_COMMISSION_PLAN.md`, every phase can merge straight to `main`.

### Phase 1 — Domain: `TenantVerificationEntity` + evidence + migration ✅ shipped (`2a66f1a03`, PR #772)

- [x] Add `Concertable.B2B.Tenant.Domain.Entities.TenantVerificationEntity` (`IGuidEntity`,
  `IEventRaiser`): `Id`, `TenantId` (unique FK to `TenantEntity`), `Status`
  (`Pending`/`Approved`/`Rejected`), `RejectionReason` (nullable), `ReviewedByAdminSub` (nullable
  `Guid`), `ReviewedAt` (nullable), `SubmittedAt`. Domain methods `Submit`/`Resubmit`,
  `Approve(adminSub, nowUtc)`, `Reject(adminSub, reason, nowUtc)` — each validates the current state
  before transitioning and throws `DomainException` on an illegal call, following
  `VenueEntity.Approve`/`TenantEntity.UpdateLegalDetails`. Transitions validated through the shared
  `Concertable.Kernel.StateMachine<TState, TTrigger>`.
- [x] Add `VerificationDocumentEntity` (`IIdEntity`): `TenantVerificationId` FK, `DocumentType`
  (`Licence`/`ProofOfAddress`/`CompanyRegistration`), `BlobName`, `UploadedAt`. Owned collection on
  `TenantVerificationEntity`, append-only.
- [x] EF configurations in `Concertable.B2B.Tenant.Infrastructure.Data.Configurations`, composed into
  the existing `TenantDbContext` (no new stance — `TenantEntity` is the tenant root and is already
  unscoped by definition; confirm this holds for the new entities before assuming it).
- [x] Re-scaffold Tenant module migrations via `./initial-migrations.ps1` from `api/` (the `migrations`
  skill) — never an additive migration.
- [x] Unit tests: `TenantVerificationEntityTests` covering every legal and illegal transition, mirroring
  `SelfBillingAgreementEntityTests`/`TenantEntityTests` style.
- [x] Build `api/Concertable.slnx`; run Tenant module unit tests.
- [x] Update this plan (check off) and the ledger in the implementation commit.

### Phase 2 — Tenant-facing submission API

- [ ] `IVerificationService`/`VerificationService` in `Concertable.B2B.Tenant.Application`/
  `Infrastructure`: `GetOwnAsync` (current tenant's status/reason/documents), `SubmitAsync(files,
  documentTypes)` (creates or resubmits, uploads each file to `verification-evidence/` via
  `IBlobStorageService`, transitions to `Pending`).
- [ ] `VerificationController` (`api/organization/verification`), `[Authorize]`,
  `[HasPermission(SharedPermissions.TenantSettingsEdit)]` on the mutating endpoint,
  `[EnableRateLimiting(RateLimitPolicies.Upload)]` on the upload endpoint. `GET` (status) needs no
  special permission beyond `[Authorize]`, matching `OrganizationController.Get`.
- [ ] Content-type allowlist (PDF/JPEG/PNG) and per-file size cap on the upload path.
- [ ] Unit tests for the service; integration tests for the controller (round-trip submit → read status).
- [ ] Build + focused tests; commit.

### Phase 3 — Cross-module gate + enforcement at publication and settlement

- [ ] Extend `ITenantModule` (Tenant.Contracts) with `Task<bool> IsVerifiedAsync(Guid tenantId,
  CancellationToken ct = default)`; implement in `TenantModule`/`TenantService` as
  `verification?.Status == Approved`, `false` when no row exists — fail-closed, same posture as
  `IsTaxComplianceCompleteAsync`.
- [ ] **Opportunity publication gate**: inject `ITenantModule` into `OpportunityService`; in
  `CreateAsync` and `CreateMultipleAsync`, after resolving the active tenant's venue, check
  `IsVerifiedAsync(tenantContext.GetTenantId())` before creating the `OpportunityEntity`. Add
  `OpportunityMutationError.VenueNotVerified` to the `[Union]` (Dunet) with
  `ErrorDefinition.Forbidden<VenueNotVerified>("This venue is not yet verified.")` and error code
  `opportunity.venue_not_verified`, following `VenueNotFound`'s shape exactly.
- [ ] **Settlement gate**: in `FinishExecutor.FinishAsync`, immediately after the existing
  tax-compliance pair check, add the same pattern for verification —
  `tenantModule.IsVerifiedAsync(supplierTenantId)` and `IsVerifiedAsync(customerTenantId)` — returning
  a new `SettlementOutcome.DeferredPendingVerification` case on failure, with a matching
  `logger.SettlementDeferredPendingVerification(...)` `LoggerMessage`. No sweep changes needed:
  `ConcertCompletionRunner` already retries every non-`Settled` outcome hourly.
- [ ] Integration tests: `TenantVerificationGateApiTests` (settlement defers/settles) mirroring
  `SelfBillingAgreementGateApiTests`/`ConcertPayoutComplianceGateApiTests`, and an opportunity-creation
  test proving an unverified tenant's `POST` is rejected.
- [ ] Build + focused tests; commit.

### Phase 4 — Admin review + cross-module contact + notification

- [ ] Admin-listing query: `GetPendingAsync(pageParams)` over `TenantVerificationEntity` where
  `Status == Pending`, ordered by `SubmittedAt`.
- [ ] Add `GetContactByTenantIdAsync(Guid tenantId, CancellationToken ct = default)` to `IVenueModule`
  and `IArtistModule` (returning `Option<TenantContact(Name, Email)>` — decide the shared-type home in
  this phase). Admin service composes the pending list with the correct module's contact by
  `TenantType`.
- [ ] `TenantVerificationAdminController` (`[Admin]`, Tenant.Api):
  `GET pending`, `POST {tenantId}/approve`, `POST {tenantId}/reject`.
- [ ] `IVerificationNotifier` (Tenant.Infrastructure), using `IEmailTransport`, called from the admin
  approve/reject service methods — mirrors `ContentReportNotifier`'s direct-call shape, resolving the
  target email via the new `GetContactByTenantIdAsync`.
- [ ] Unit tests for the admin service and notifier; integration tests for the admin endpoints
  mirroring the venue-approval integration coverage from admin-console Phase 4.
- [ ] Build + focused tests; commit.

### Phase 5 — Admin SPA + tenant-facing UI

- [ ] `app/web/admin/src/features/verification/` mirroring `features/venues` structure: `api/`,
  `hooks/`, `components/` (`PendingVerificationsList.tsx`, `RejectVerificationDialog.tsx` — reason
  required, mirroring `ResolveReportDialog.tsx`), `pages/VerificationPage.tsx`, `types.ts`, `index.ts`.
- [ ] New route `routes/_admin/verification.tsx`; add the nav entry alongside the existing
  Venues/Moderation links in `routes/_admin/route.tsx`.
- [ ] Tenant-facing: a status banner (mirroring the existing DAC7 tax-compliance nag) + evidence-upload
  form in `app/web/b2b/shared` (consumed by both venue and artist manager SPAs), showing
  pending/approved/rejected(+reason) state and a re-submit action.
- [ ] Run all five web builds (per `app/web/AGENTS.md` — the boundary gate) and the affected SPAs'
  typecheck/lint.
- [ ] Manual verification in the running app (submit as venue/artist, approve/reject as admin, confirm
  the opportunity-publication block and dashboard banner) before marking the phase done — per this
  repo's UI-change verification requirement.
- [ ] Build + focused tests; commit.

### Phase 6 — Retire the decorative `VenueEntity.Approved` surface

Start only once Phase 3's gate is proven (merged and green) — the new gate must fully replace the old
one's function before the old one is deleted, never dropped first and rebuilt after.

- [ ] Remove `VenueEntity.Approved`, `VenueEntity.Approve()`, `VenueChangedDomainEvent` usage tied to
  approval specifically (keep the event for other profile changes).
- [ ] Remove `VenueController`'s `[Admin] PATCH {venueId}/approve` and `[Admin] GET pending-approval`,
  `IVenuePrivilegedRepository.GetPendingApprovalAsync`, `IVenueService.ApproveAsync`/
  `GetPendingApprovalAsync`, `ApproveVenueError`.
- [ ] Remove `app/web/admin/src/features/venues/` (superseded by `features/verification`) and its route
  + nav entry.
- [ ] Re-scaffold the Venue module migration to drop the `Approved` column.
- [ ] Run the rename/removal grep gate scoped to the removed symbols
  (`VenueEntity.Approved`, `venueService.ApproveAsync`, `pending-approval`, `ApproveVenueError`) — zero
  remaining references, or an explicit justified allowlist entry.
- [ ] Build + focused tests; run the five web builds; commit.
- [ ] Tick `launch/tenant-verification` in `plans/launch/LAUNCH_ROADMAP.md` (line 41) and the
  Architecture-checklist line in §7. Delete this plan and its ledger in the final verified commit.
- [ ] **Hard stop:** hand off for review; do not begin unrelated launch-gate work in this worktree.

## 4. Out of scope (per the roadmap item and explicit instruction)

- DAC7/tax-compliance gating (`Tenant.Compliance`) — separate, already-shipped gate, untouched here.
- Settlement-dispute path (§9) — separate open decision needing solicitor input.
- Tenant suspension (§5) — separate item, blocked on a T&Cs enforcement clause.
- Gating artist Application/Apply — see §1.4.

## 5. Verification coverage

- Unit: `TenantVerificationEntityTests` (state machine), `VerificationService` tests, admin service
  tests, notifier tests.
- Integration: submit/resubmit round-trip, admin pending-list/approve/reject, the opportunity-publication
  gate, the settlement dual-gate (defer + self-heal), and (Phase 6) the removed-surface regression check.
- Frontend: five web builds green; manual smoke of both the admin review flow and the tenant submission/
  nag flow.
- Merge-queue E2E tier applies per the `merge` skill's Step 4 — not run locally ahead of merge.
