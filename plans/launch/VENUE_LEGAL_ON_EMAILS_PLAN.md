# Venue/artist legal details on booking-confirmation emails

Puts the counterparties' legal trading details (legal name, registered address, VAT number where
present) onto a **booking-confirmation email** sent to both the venue and the artist when a booking is
confirmed — consistent with what the shipped self-billed invoice already shows, sourced from the same
Tenant source of truth via the same cross-module seam.

Next steps live in `@plans/launch/VENUE_LEGAL_ON_EMAILS_PROGRESS.md → ## Next Steps`. This plan holds
the design and outstanding phases only; it carries no next-action prose and does not cite the roadmap.

## Why this item is bigger than the roadmap's "1 day"

The roadmap sized this at 1 day on the assumption that a booking-confirmation email already existed and
only needed legal fields added. The audit shows it does **not**:

- **The invoice half is complete.** `InvoiceIssuer.BuildPartyAsync` (`Concert.Infrastructure/Services/
  InvoiceIssuer.cs:70-84`) snapshots each party's legal identity onto the immutable `InvoiceParty` value
  object (`Concert.Domain/ValueObjects/InvoiceParty.cs` — `LegalName`, `VatNumber?`, flattened registered
  address; **no company number**) at settlement, inside `FinishExecutor.FinishAsync` (`…/Workflow/
  Executors/FinishExecutor.cs:87`). It reads legal details from the Tenant module via
  `ITenantModule.GetByIdAsync` (legal name) + `GetTaxComplianceAsync` (VAT + registered address). Nothing
  to rebuild here.
- **No booking-confirmation email exists.** B2B sends exactly six emails, all through
  `Concert.Infrastructure/Services/Messenger.cs:32` (5 application-lifecycle notices via
  `ApplicationNotifier`) and `Tenant.Infrastructure/Services/InvitationService.cs:134` (org invite). The
  closest is `ApplicationNotifier.AcceptedAsync` (`ApplicationNotifier.cs:43-48`) — an **artist-only**,
  plaintext "your application was accepted" notice with **no legal details**. When a booking actually
  reaches the confirmed/`Booked` state and a concert is scheduled (`ConcertDraftService.CreateAsync`), the
  only notification to **both** parties is the in-app SignalR ping via `ConcertNotifier.ConcertDraftCreatedAsync`
  (`ConcertNotifier.cs:12-13`) — **no email is sent to either party**.

So the real work is **creating the both-party booking-confirmation email and putting the legal details on
it**, not editing an existing template.

## The Tenant source of truth (what exists to source from)

Established against the Tenant domain + Contracts (unchanged between the audited tree and `origin/main`):

- `TenantEntity.LegalName` (single legal/trading name; no separate display name). Exposed cross-module as
  `TenantDto(Id, LegalName)` via `ITenantModule.GetByIdAsync`.
- `TenantEntity.TaxCompliance` (nullable until org setup) → `TaxComplianceDto` via
  `ITenantModule.GetTaxComplianceAsync`: `VatNumber?`, `SellerIdentifier` (a single overloaded
  company-number/UTR/NINO field), `RegisteredAddress` (structured 5-field VO), `BankReference`,
  `HoldsMusicLicence`.
- **There is no distinct company-registration-number field** — only the overloaded `SellerIdentifier`,
  which the invoice uses solely to build the invoice number, never surfaces as a party field. The Phase 5
  Org setup form captures legal name, registered address, VAT and `SellerIdentifier`; it does **not**
  capture a separate company number.

The email will read the **same two `ITenantModule` methods the invoice uses** and render directly from
`TenantDto` + `TaxComplianceDto` — no new legal-detail model, no reach into Tenant internals.

## Scope

**In scope**

- A both-party **booking-confirmation email** fired when a booking is confirmed and a concert is
  scheduled — the `ConcertDraftService.CreateAsync` transition, which already fans out the in-app
  both-party notification. This is the honest "confirmed for both parties" point that the venue's Accept
  action drives through to, and it is common to all four contract types (the Book step is shared).
- The email body carries **both** counterparties' legal trading details: legal name (always), registered
  address and VAT number **where the tenant has them** — matching exactly what the invoice's `InvoiceParty`
  surfaces, sourced from `ITenantModule`.
- Graceful degradation: a tenant whose `TaxCompliance` is not yet populated (org setup incomplete — no
  fail-closed compliance gate exists this early in the lifecycle, unlike Finish) shows legal name only; the
  address/VAT lines are omitted, never rendered blank.
- Reuse of `Concertable.Shared.Email` for delivery and the existing tenant-recipient resolution
  (`ITenantModule.GetMemberUserIdsAsync` → `IUserModule.GetEmailsByIdsAsync`, as `ApplicationNotifier` does).

**Out of scope**

- Adding legal details to the invoice (shipped) or to the 5 existing application-lifecycle emails.
- A distinct company-registration-number field on the tenant / Org form — see Open decision 2.
- Migrating B2B email onto the transactional outbox as a whole (owned by `api/Concertable.B2B/
  TECH_DEBT.md` "B2B outbound email is still synchronous inline") — see Open decision 1.
- Privacy/T&Cs/cookie page routes (separate `launch` item), and any new legal copy needing the solicitor.

## Open decisions (resolve at plan review — each changes scope, not the core design)

**1. Delivery: synchronous notifier (recommended) vs transactional outbox.**
The three existing B2B notifiers (`ConcertNotifier`, `ApplicationNotifier`, `Messenger`) send synchronously
via `IEmailTransport`. The correct-long-term target is the transactional outbox (raise a domain event →
pre-commit handler stages a `SendEmailCommand`), which `api/Concertable.B2B/TECH_DEBT.md` already tracks for
**all** B2B email and whose resolution is explicitly "the concert-lifecycle transition raises a domain event
… stages a SendEmailCommand on the same transaction."
  - **Recommendation: send synchronously via a new `BookingConfirmationNotifier`, mirroring `ConcertNotifier`.**
    It ships the compliance value in the house style, is assertable in the existing integration harness (which
    cannot observe an in-process outbox command — see the tech-debt note), and is not itself a hack. Log the
    new email against the existing tech-debt item so it migrates with the rest.
  - **Alternative:** do the outbox/event delivery now (transactional, retried — attractive for a
    compliance-bearing email), accepting a new domain event + a changed integration-test approach (assert the
    staged command). Larger, and pulls a B2B-wide reliability migration into a compliance feature. Recommend
    deferring to the tech-debt item unless we want it now.

**2. Company-registration number: mirror the invoice (recommended) vs capture a new field.**
No distinct company number exists on the tenant, the `TaxComplianceDto`, or the invoice. The outcome asks for
legal details "consistent with what invoices already show," and the invoice shows legal name + registered
address + VAT only.
  - **Recommendation: mirror the invoice — legal name + registered address + VAT.** No new field. Consistent,
    and "company number where the entity has them" resolves to *absent* because no one has it as a distinct
    field today.
  - **Alternative:** add a real company-registration-number field (new property on the `TaxCompliance` VO +
    `TaxComplianceDto` + Org setup form + re-scaffold migrations), then render it on the email (and, for
    consistency, the invoice). Net-new additive Tenant work; only worth it if a genuine company number must
    appear. Recommend deferring.

The phases below assume both recommendations. Choosing an alternative adds a phase (outbox wiring; or the
Tenant-model capture) noted inline.

## Design

**Hook point.** `ConcertDraftService.CreateAsync` (`Concert.Infrastructure/Services/ConcertDraftService.cs:25-67`),
immediately after the existing `notifier.ConcertDraftCreatedAsync(artist…/venue…)` calls (lines 63-64). The
booking has its `Application` loaded, so the confirmation can resolve the tenant pair and both parties'
legal details there. This point is reached by every contract type's Book step, so all four get the email.

**Components** (names follow `api/agents/CODE_CONVENTIONS.md`; not load-bearing):

- `BookingConfirmationEmailGenerator` (`IBookingConfirmationEmailGenerator.Generate(...)`) — a pure content
  generator: given each party's `TenantDto` + `TaxComplianceDto?` and the concert summary (parties, date,
  venue), returns the subject + HTML body with a two-party legal-details block. `SmtpEmailTransport` always
  sends the body as HTML, so author it as HTML. Legal name always; address + VAT lines emitted only when the
  party's `TaxComplianceDto` is non-null. This is the unit-tested heart of the feature.
- `BookingConfirmationNotifier` (`IBookingConfirmationNotifier`), mirroring `ConcertNotifier` — orchestrates:
  resolve `(venueTenantId, artistTenantId)` from the booking's application (the `GetTenantPairByIdAsync`
  seam `ApplicationNotifier` uses); read `ITenantModule.GetByIdAsync` + `GetTaxComplianceAsync` for both
  (the same reads `InvoiceIssuer` uses); resolve recipient emails for both tenants
  (`GetMemberUserIdsAsync` → `GetEmailsByIdsAsync`); call the generator; send to every recipient via
  `IEmailTransport`. Both recipients receive the same both-party legal block (as an invoice/contract does).
- Registered in `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` alongside the existing
  notifiers; injected into `ConcertDraftService`.

**No new legal-detail model** — the generator consumes the existing `TenantDto` / `TaxComplianceDto`
Contracts types directly. **Module boundary respected** — Concert reads Tenant legal details only through
`ITenantModule` (the seam the invoice already uses), never Tenant internals.

**No model change → no migration** (recommended path). `./initial-migrations.ps1` is not required unless
Open decision 2's alternative (new `TaxCompliance` field) is chosen.

## Phases

### Phase 1 — Legal-details content generator + unit tests

Add `BookingConfirmationEmailGenerator`. It renders subject + HTML body from each party's `TenantDto` +
`TaxComplianceDto?` and the concert summary, with the two-party legal-details block and graceful
degradation.

**Verification gate:** builds (smallest affected Concert project); focused unit tests pass —
(a) both parties rendered; (b) `TaxCompliance` present → registered address + VAT shown; (c) `TaxCompliance`
absent → legal name only, no blank address/VAT lines; (d) `VatNumber` null but address present →
address shown, VAT omitted; (e) placeholder legal name (pre-org-setup) still renders. Commit; push to the
draft PR.

### Phase 2 — Send the confirmation to both parties at booking-confirmed

Add `BookingConfirmationNotifier`, register it, inject it into `ConcertDraftService`, and invoke it in
`CreateAsync` after the existing both-party in-app notification. It resolves the tenant pair, both parties'
legal details via `ITenantModule`, and both tenants' recipient emails, then sends the generated email to
each recipient (synchronously via `IEmailTransport`, per Open decision 1). Log the new synchronous email
against the `api/Concertable.B2B/TECH_DEBT.md` outbox item.

**Verification gate:** builds (smallest affected build); an integration test on the booking-confirmed
transition asserts both the venue's and the artist's members receive an email whose body contains both
parties' legal trading details (legal name + registered address + VAT), via the existing `EmailSender.Sent`
harness; a second test asserts graceful degradation when a party's `TaxCompliance` is absent. No model
change → no migration. Commit; push. Final phase: select the merge-queue E2E tier per the merge skill's
Step 4 (do not run E2E locally ahead of the queue); inherit `docs/REMOTE_VALIDATION.md` for the full
build/carve/unit/integration gate.

## Delivery

Single PR (no dependency splits its work). The change is B2B-internal and additive — it touches no published
cross-service contract — so the `chore/platform-sync-*` PR that follows the merge is a routine non-breaking
version bump that auto-merges green; no consumer migration. No package/platform-sync gate blocks
implementation. Merge and deployment await Tommy's explicit instruction.

When the feature ships, tick the roadmap §7 line "Venue legal details on booking confirmation emails +
invoices" and mark the §5 row — never delete the roadmap.
