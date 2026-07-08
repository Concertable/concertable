# E-signed booking agreement (click-wrap at Accept)

> Implements the 🔴 **E-signed booking agreement** MVP blocker in [LAUNCH_PLAN.md](./LAUNCH_PLAN.md)
> — exactly [LEGAL_REQUIREMENTS.md item 2](../../api/Concertable.B2B/src/Modules/Contract/LEGAL_REQUIREMENTS.md)
> (Tier 1 click-wrap only; Tier 2 drawn/DocuSign e-signature explicitly out of scope).
>
> **Branch:** `Feature/BookingAgreement` · single service (B2B) + web SPAs · **no published-package
> change → single PR, no two-PR split** (see "Package boundaries" below).

## What exists today (investigated 2026-07-08)

- **Accept flow.** Only the venue accepts (`ApplicationController.Accept`/`AcceptCheckout`, gated
  `VenuePermissions.ApplicationsDecide`); the artist's verbs are Apply/Withdraw. Every accept funnels
  through `AcceptExecutor.ExecuteAsync(applicationId, paymentMethodId?)` inside a
  `LifecycleTransitioner.TransitionAsync(Trigger.Accept, effect)` transaction: contract resolved via
  `IContractResolver`→`IContractModule`, per-type workflow step creates the `BookingEntity`
  (`CaptureEscrowAcceptStep` FlatFee / `DepositEscrowAcceptStep` VenueHire / `PaidAcceptStep`
  DoorSplit+Versus), then `app.Accept(booking)`. The "simple vs checkout" split is a **frontend**
  routing decision keyed on the HATEOAS `actions.checkout` link (`ApplicationResponseMapper`); both
  paths end at the same `POST /api/Application/{id}/accept` (`AcceptRequest(string PaymentMethodId)`).
  The concert draft is created later (payment webhook → `Book` transition), not at Accept.
- **Terms are mutable and never frozen.** `Application` holds only `OpportunityId` + a copied
  `ContractType`; the numbers live on the opportunity's `ContractEntity` (TPT: `FlatFeeContracts` etc.),
  which `OpportunitySyncer.UpdateAsync` edits **in place with no pending-applications guard**. Nothing
  snapshots the agreed numbers at Accept — the exact gap item 2 closes.
- **Snapshot convention** = denormalized scalar columns copied at the freeze moment (Customer
  `TicketEntity` purchase snapshot). No JSON-column precedent. Per-`ContractType` behaviour uses the
  keyed-strategy pattern (`PayeeResolver`, `ContractMapper` — `api/docs/CODE_PATTERNS.md`).
- **`IPdfService` exists and is already wired into B2B** (`Concertable.Shared.Pdf`, QuestPDF Community,
  `byte[] Render(IDocument)`; `AddSharedPdf()` already called in B2B.Web + B2B.Workers). Precedent
  `IDocument`: Customer's `TicketReceiptDocument`. Nothing to build in shared code.
- **Blob storage exists and is already wired into B2B** (`IBlobStorageService`, Azure/Azurite via
  Aspire, `FakeBlobStorageService` for tests, single container `"images"`, `overwrite: true`).
  Entities store blob-name strings (venue/artist `BannerUrl`), never `byte[]` columns.
- **Greenfield:** no terms-version, cancellation-terms, or consent concept exists anywhere (items 6/7).

## Decisions

1. **`BookingAgreementEntity` lives in the Concert module** (owns Accept + the parties; reads terms via
   `IContractModule` per item 2). Snapshot = denormalized scalar columns (TicketEntity convention):
   parties (`VenueId`/`ArtistId`, names, both `TenantId`s), event `DateRange` (from the opportunity),
   `ContractType`, `PaymentMethod`, the terms numbers as a nullable union (`Fee?`, `HireFee?`,
   `Guarantee?`, `ArtistDoorPercent?`), a rendered human-readable `TermsText`, `PlatformTermsVersion`,
   both consent blocks (user id + UTC timestamp + optional IP/user-agent), `PdfBlobName?`,
   `CreatedAtUtc`. Unique on `BookingId`. **Not** `ITenantScoped` — the agreement is dual-party;
   access is explicit party checks, not the single-tenant filter.
2. **Snapshot is written inside the `AcceptExecutor` transition effect**, after the accept step creates
   the booking — same transaction as the state change, so no accept without an agreement and vice
   versa. Builder reads the already-resolved `IContractAccessor.Contract` + application/opportunity +
   artist/venue read models (all Concert-module-local).
3. **`TermsText` via a keyed strategy** — `FrozenDictionary<ContractType, IAgreementTermsRenderer>`
   facade per `CODE_PATTERNS.md` (mimics `PayeeResolver`). Reused by the PDF document.
4. **Both parties consent; two click-wraps.**
   - **Artist at Apply** — the application is the artist's offer; an "I agree to the contract terms"
     gate on Apply (both standard and prepaid/checkout apply paths). Recorded on `ApplicationEntity`
     as a nullable owned `Consent` value object (null = predates click-wrap), copied into the
     agreement at Accept. Matters most for VenueHire, where the **artist** is the payer.
   - **Venue at Accept** — the item-2 gate: `AcceptRequest` grows `AgreedToTerms`; server 400s without
     it, on both accept paths (the checkout path also ends at `POST /accept`).
   - **Consent = `Consent` VO** (UserId, AtUtc, Ip?, UserAgent?) mapped as an EF owned type
     (`DateRange` precedent); `VenueConsent` is **required** on the agreement. Who/when from
     `ICurrentUser`/`TimeProvider`; IP/UA from a module-local ambient `IClientContext` accessor
     (`TenantContext` precedent) injected only into `ApplyExecutor`/`BookingAgreementBuilder` — the
     consent flag never travels below the controller, and no service/dispatcher signatures change.
5. **Terms-fingerprint guard for the mutability race.** Opportunity edits mutate the contract in place
   with no guard, so the artist could consent to terms the venue then changes before accepting. At
   Apply we store a deterministic fingerprint of what the artist consented to (contract type + numbers
   + `PaymentMethod` + opportunity `DateRange`); at Accept the builder recomputes it from the live
   contract and **400s on mismatch** ("terms changed since the artist applied — the artist must
   re-apply"). Cheap, and makes the artist's consent legally meaningful.
6. **`PlatformTermsVersion` from config** (`Legal:PlatformTermsVersion` in B2B appsettings). No
   versioned-terms model exists yet (item 7) and cancellation terms don't exist (item 6) — for MVP the
   cancellation terms are part of the versioned platform terms document, and the agreement snapshots
   the version string. When items 6/7 land they feed real values; nothing speculative built here.
7. **PDF stored in the existing blob container** under an `agreements/{bookingId}-{guid}.pdf` name
   (blob names with `/` act as virtual folders) — avoids any change to the published
   `Concertable.Shared.Blob` package. Immutability is app-level write-once: generated once, blob name
   recorded on the agreement, never regenerated with the same name. Generation is **background at
   Accept** (existing `ITaskRunner` pattern, keeps the accept request fast and blob outages non-fatal)
   with a **lazy fallback on download** (if the blob is missing, render from the immutable snapshot,
   persist, serve) — which also makes the download endpoint integration-testable against
   `FakeBlobStorageService`.
8. **Surface:** `GET /api/Application/{id}/agreement` (metadata) + `GET /api/Application/{id}/agreement/pdf`
   (file, `application/pdf`, authenticated + party-checked — the existing `BlobController` passthrough
   is NOT suitable for a legal document). HATEOAS: `agreement` link on `ApplicationActions`, gated on
   the agreement existing (surface `AgreementId?`/`HasAgreement` on `ApplicationDto`). FE: click-wrap
   checkboxes (artist apply page; venue `AcceptApplicationPage` — checkbox beside `AcceptContractSummary`,
   Accept/Continue disabled until ticked, consent carried through the checkout route into the final
   accept mutation) + a "Booking agreement" download link on the artist My Applications detail and the
   venue application detail, gated on the HATEOAS link.

## Package boundaries — none crossed

Everything lands in B2B (Concert module + B2B.Web) and `app/web`. `Concertable.Shared.Pdf` and
`Concertable.Shared.Blob` are consumed as already-referenced published packages with **no interface
change** (decision 7 exists precisely to avoid touching `IBlobStorageService`). Single PR.
Log as tech debt: agreements share the `"images"` container and rely on app-level write-once
(`UploadAsync` is `overwrite: true`); a dedicated container / no-overwrite upload is an additive
shared-package change for later.

## Phases

### ✅ Phase 1 — Snapshot backbone (zero user-facing behavior change) — SHIPPED

All items landed; gate passed (build green · unit 56/56 · Concert integration 92/92 incl. the four
freeze tests · migrations re-scaffolded). Notes for later phases:

- The agreement gets the **two-party venue-artist query filter** (same stance as Application/Booking —
  private deal document); Phase 3's endpoints still add explicit party checks on top.
- Seeder check outcome: seeders write accepted applications/bookings as direct `SeedState` rows, so
  seeded bookings simply predate the feature — same as production rows at deploy. Nothing to change.
- `./initial-migrations.ps1` was broken for the Messaging Outbox/Inbox contexts (B2B consumes
  Messaging as a published package, so EF loaded the packaged assembly's compiled `InitialCreate`);
  fixed in this phase — Messaging now scaffolds standalone via its design-time factories.

### ✅ Phase 2 — Click-wrap consent, both parties — SHIPPED

All items landed; gate passed (solution build green · B2B Concert integration 97/97 incl. consent
400s / consent recorded / fingerprint-mismatch 400 · Customer Concert 2/2 · four web builds green ·
migrations re-scaffolded). Notes for later phases / this phase's decisions:

- **Checkbox placement resolved to _independent gate per submit point_**, not the "carried through the
  checkout route" wording in decision 8. The consent flag never leaves the client (`applicationApi`
  hardcodes `agreedToTerms: true` server-side), and the venue checkout route is deep-linked directly
  by UI E2E, so carried state is unusable. Each submit surface owns its own local checkbox: artist
  `ApplyAction` (simple) + `ArtistApplyCheckoutPage`; venue `AcceptApplicationPage` (simple path only —
  the checkout path shows "Continue" ungated, its gate lives on `VenueAcceptCheckoutPage`).
- **`OpportunityRepository.GetPeriodByIdAsync`** (new, for the fingerprint) needs `AsNoTracking()` —
  projecting the owned `DateRange` in a tracking query throws; fixed here (caught by integration).
- **UI E2E:** one idempotent `LocatorExtensions.EnsureCheckedAsync` (aria-checked guard — a Radix
  checkbox click just toggles, so re-ticking from overlapping entry points must be safe); ticks wired
  at every checkbox entry point (`VenueDetailsPage.AgreeAndApplyAsync`, `ApplyCheckoutPage.PayWith*`,
  `AcceptApplicationPage.AgreeAndConfirmAsync`, `ApplicationCheckoutPage.Submit*`, and the three venue
  deep-link `Given`s that pay via `payment` directly). Not run this phase — deferred to Phase 3's gate.

### Phase 3 — PDF, storage, download, FE surface

- `BookingAgreementDocument : IDocument` (QuestPDF, `TicketReceiptDocument` precedent): parties, event
  dates, contract type + `TermsText`, payment method, both consent blocks (who/when/IP), platform-terms
  version, agreement reference, generated-at.
- Generation service: render via `IPdfService`, upload via `IBlobStorageService` under the
  `agreements/` prefix, record `PdfBlobName` — background at Accept via `ITaskRunner`, lazy fallback in
  the download path (decision 7).
- `GET .../agreement` + `GET .../agreement/pdf` endpoints with explicit both-party authorization
  (follow the existing application read auth — both parties already read applications); HATEOAS
  `agreement` link; `AgreementId` on `ApplicationDto`.
- FE: download links in both manager SPAs, gated on the HATEOAS link. All four web builds.
- **Gate (final phase):** build green · Concert integration tests via `integration-debug` (download
  returns PDF bytes for both parties, 403 for a stranger, lazy-generate path) · four web builds ·
  **UI E2E regress via `e2e-ui-regress`** — the feature changes user-facing behavior in covered money
  flows (apply + accept), which meets the massive/risky bar. Docker pre-flight per root CLAUDE.md.
- Same commit as this phase completing: tick the LAUNCH_PLAN 🔴 item, log the blob tech-debt line in
  `api/Concertable.B2B/TECH_DEBT.md`, and `git rm` this plan.

## Explicitly out of scope (don't build)

- Tier 2 e-signature (drawn/typed/DocuSign) — item 2 says add only if a customer demands it.
- Real versioned platform terms + registration-time consent (item 7), cancellation-terms matrix
  (item 6), VAT/invoice work (items 1/3/4) — the agreement only snapshots the version string seam.
- Blocking opportunity edits while applications are pending — the fingerprint guard covers the legal
  risk; an edit-lock is a product decision for later.
- Shared-package blob changes (dedicated container, write-once upload) — tech-debt note instead.
