# Venue/artist legal details on booking-confirmation emails progress

- Plan: `plans/launch/VENUE_LEGAL_ON_EMAILS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/venue-legal-on-emails`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_venue-legal-on-emails`
- Branch: `Feature/launch_venue-legal-on-emails`
- PR: `not opened`
- Dependency/package gates: `none — all implementation dependencies shipped (Phase 5 org setup UI, invoice engine, Concertable.Shared.Email); the change is B2B-internal and additive, so no package/platform-sync gate blocks implementation`
- Last reconciled: `2026-08-15 — plan authored from a full code audit of the invoice half, the Tenant legal source of truth, the shared email infra, and the Concert booking lifecycle against origin/main (520761dd4)`

## Current state

Plan and this ledger written; no code yet. Worktree is on `Feature/launch_venue-legal-on-emails`, branched
from `origin/main` at `520761dd4`, clean. The design is grounded in existing seams (audit below); two open
decisions are surfaced in the plan for review, both with a recommendation the phases assume.

## Next Steps

Unless Tommy picks an alternative for an open decision at plan review, proceed on the plan's recommended
defaults (synchronous `BookingConfirmationNotifier`; mirror the invoice's legal fields — no new company-number
field) and implement **Phase 1 — Legal-details content generator + unit tests**:

1. Add `BookingConfirmationEmailGenerator` (`IBookingConfirmationEmailGenerator`) in the Concert module. It
   takes each party's `TenantDto` + `TaxComplianceDto?` and the concert summary and returns subject + HTML
   body with a two-party legal-details block (legal name always; registered address + VAT only when the
   party's `TaxComplianceDto` is non-null — omit, never blank). Author the body as HTML (`SmtpEmailTransport`
   sends every body as HTML). No new legal-detail model — consume the existing Contracts DTOs directly.
2. Unit-test cases (a)–(e) from the plan's Phase 1 gate: both parties rendered; `TaxCompliance` present →
   address + VAT; absent → legal name only; VAT null but address present → address shown, VAT omitted;
   placeholder pre-org-setup legal name still renders.
3. Build the smallest affected Concert project + run the focused unit tests to green, then commit and push a
   draft PR (first coherent checkpoint) so remote CI validates the exact head.

Then continue with Phase 2 (wire `BookingConfirmationNotifier` into `ConcertDraftService.CreateAsync`,
both parties, + integration test). Full phase detail lives in the plan.

## Completed work

- Plan + ledger authored; roadmap §7 checklist line carries the `launch/venue-legal-on-emails` key (unticked
  — feature not shipped). `plan_graph.py` passes.

## Verification

None yet — no code written.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

Audit findings that ground the design (verified in code; the relevant paths are unchanged between the audited
tree and `origin/main`):

- **Invoice half shipped, nothing to rebuild.** `InvoiceIssuer.BuildPartyAsync`
  (`Concert.Infrastructure/Services/InvoiceIssuer.cs:70-84`) snapshots legal identity onto `InvoiceParty`
  (`Concert.Domain/ValueObjects/InvoiceParty.cs`: `LegalName`, `VatNumber?`, flattened registered address; no
  company number) at `FinishExecutor.FinishAsync:87`, reading `ITenantModule.GetByIdAsync` +
  `GetTaxComplianceAsync`.
- **No booking-confirmation email exists.** B2B sends six emails only: 5 application-lifecycle notices via
  `ApplicationNotifier`→`Messenger.cs:32` + the org invite (`InvitationService.cs:134`).
  `ApplicationNotifier.AcceptedAsync` is artist-only, plaintext, no legal details. The both-party confirmed
  point (`ConcertDraftService.CreateAsync`→`ConcertNotifier.ConcertDraftCreatedAsync`) sends **in-app SignalR
  only, no email**. So the confirmation email is net-new — the reason this is larger than the roadmap's "1 day".
- **Tenant source of truth.** `TenantEntity.LegalName` (single name) + nullable `TaxCompliance` VO
  (`VatNumber?`, `SellerIdentifier` = one overloaded company/UTR/NINO field, structured `RegisteredAddress`,
  `BankReference`, `HoldsMusicLicence`), exposed cross-module as `TenantDto` + `TaxComplianceDto` via
  `ITenantModule`. **No distinct company-registration number** anywhere (entity, DTO, invoice, or Org form) —
  drives Open decision 2.
- **Seams the design reuses, all existing:** legal details = `ITenantModule.GetByIdAsync` +
  `GetTaxComplianceAsync` (invoice's reads); tenant pair + recipients = `GetTenantPairByIdAsync` +
  `GetMemberUserIdsAsync` + `GetEmailsByIdsAsync` (`ApplicationNotifier`'s reads); send = `IEmailTransport`
  (`Messenger`'s send); hook = `ConcertDraftService.CreateAsync` (both-party, all four contract types).
- **Open decision 1 — delivery:** recommend synchronous notifier (house style, assertable in the existing
  harness); outbox/event is the tech-debt-aligned alternative (`api/Concertable.B2B/TECH_DEBT.md`), deferred.
- **Open decision 2 — company number:** recommend mirroring the invoice (legal name + registered address +
  VAT, no new field); a real company-number field is additive Tenant work, deferred.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_venue-legal-on-emails
Read @plans/launch/VENUE_LEGAL_ON_EMAILS_PLAN.md and @plans/launch/VENUE_LEGAL_ON_EMAILS_PROGRESS.md and do what its `## Next Steps` says.
```
