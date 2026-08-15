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

Phase 1 complete on `Feature/launch_venue-legal-on-emails` (branched from `origin/main` at `520761dd4`).
`BookingConfirmationEmailGenerator` + interface/records added in `Concert.Infrastructure.Services`, registered
in the module's `ServiceCollectionExtensions`, with 6 focused unit tests green. Chosen both recommended
defaults (synchronous notifier; mirror the invoice's legal fields — no new company-number field). Generator is
registered but not yet consumed — Phase 2 wires it. No code writes touch Tenant internals; it reads the same
`TenantDto`/`TaxComplianceDto` Contracts the invoice uses.

## Next Steps

Implement **Phase 2 — Send the confirmation to both parties at booking-confirmed**:

1. Add `BookingConfirmationNotifier` (`IBookingConfirmationNotifier`) in `Concert.Infrastructure.Services`,
   mirroring `ConcertNotifier`. It resolves `(venueTenantId, artistTenantId)` from the booking's application
   (`IApplicationRepository.GetTenantPairByIdAsync`), reads `ITenantModule.GetByIdAsync` +
   `GetTaxComplianceAsync` for both, resolves each tenant's recipient emails
   (`GetMemberUserIdsAsync` → `IUserModule.GetEmailsByIdsAsync`), calls `IBookingConfirmationEmailGenerator`,
   and sends the one generated email to every recipient via `IEmailTransport`. Both recipients get the same
   both-party legal block. Build the party `DisplayName`s from `artist.Name` / `venue.Name` and the date from
   `concert.Period`.
2. Register it in `ServiceCollectionExtensions`; inject into `ConcertDraftService` and invoke in `CreateAsync`
   after the existing both-party `notifier.ConcertDraftCreatedAsync(...)` calls.
3. Log the new synchronous email against the `api/Concertable.B2B/TECH_DEBT.md` outbox item.
4. Integration test: the booking-confirmed transition sends the venue's and the artist's members an email
   whose body carries both parties' legal details (legal name + registered address + VAT), via the existing
   `EmailSender.Sent` harness; a second test asserts graceful degradation when a party's `TaxCompliance` is
   absent. No model change → no migration.
5. Commit; push. Then select the merge-queue E2E tier per the merge skill's Step 4 (do not run E2E locally
   ahead of the queue).

## Completed work

- Plan + ledger authored; roadmap §7 checklist line carries the `launch/venue-legal-on-emails` key (unticked
  — feature not shipped). `plan_graph.py` passes.
- **Phase 1** — `IBookingConfirmationEmailGenerator` + `BookingConfirmationParty` / `BookingConfirmationEmail`
  records (`Concert.Infrastructure/Services/IBookingConfirmationEmailGenerator.cs`) and
  `BookingConfirmationEmailGenerator` (`…/BookingConfirmationEmailGenerator.cs`): pure producer, subject +
  HTML body, two-party legal block, HTML-encoded values, graceful degradation. Registered in
  `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`. Hosted in Infrastructure (not
  Application) because its contract is the `Tenant.Contracts` DTOs — the dependency Infrastructure already
  owns via `InvoiceIssuer`; Application deliberately doesn't reference `Tenant.Contracts`.

## Verification

- Phase 1: `Concert.Infrastructure` builds clean; `BookingConfirmationEmailGeneratorTests` — 6/6 pass
  (both-party render; `TaxCompliance` present → address + VAT; absent → legal name only; VAT null + address
  present → address shown, VAT omitted; placeholder legal name renders; HTML in legal details is encoded).

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
