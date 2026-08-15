# Venue/artist legal details on booking-confirmation emails progress

- Plan: `plans/launch/VENUE_LEGAL_ON_EMAILS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/venue-legal-on-emails`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_venue-legal-on-emails`
- Branch: `Feature/launch_venue-legal-on-emails`
- PR: `#582 (draft)`
- Dependency/package gates: `none — all implementation dependencies shipped (Phase 5 org setup UI, invoice engine, Concertable.Shared.Email); the change is B2B-internal and additive, so no package/platform-sync gate blocks implementation`
- Last reconciled: `2026-08-15 — plan authored from a full code audit of the invoice half, the Tenant legal source of truth, the shared email infra, and the Concert booking lifecycle against origin/main (520761dd4)`

## Current state

Phases 1 and 2 complete on `Feature/launch_venue-legal-on-emails` (draft PR #582). The both-party
booking-confirmation email is wired end to end: `BookingConfirmationNotifier` resolves both tenants' legal
details (`ITenantModule.GetByIdAsync` + `GetTaxComplianceAsync`) and recipient emails
(`GetMemberUserIdsAsync` → `IUserModule.GetEmailsByIdsAsync`), calls the generator, and sends synchronously
via `IEmailTransport` (Open decision 1) to every member of both tenants. Invoked from
`ConcertDraftService.CreateAsync` after the in-app both-party notification, reading the tenant ids straight
off the loaded `Application` (`VenueTenantId`/`ArtistTenantId`) — no extra query. No model change → no
migration. The synchronous send is logged in `api/Concertable.B2B/TECH_DEBT.md`.

Builds clean; Concert unit suite 139/139 green; integration project compiles. Two integration tests added;
they run in draft-PR CI, which owns the integration matrix (not run locally per `docs/REMOTE_VALIDATION.md`).

## Next Steps

Remote validation, then merge on Tommy's go-ahead:

1. Draft PR #582 is pushed — draft-PR CI owns the full build / carve / unit / integration gate. Confirm green.
2. If a check goes red, diagnose only the failing scope with the matching debug skill (`integration-debug`
   for the two integration tests) and push the fix; do not run E2E locally ahead of the queue.
3. Merge-queue E2E tier: per the merge skill's Step 4 this change is B2B-internal and additive with no
   positive E2E trigger → `skip-e2e` (integration covers the booking-confirmed path).
4. **Merge awaits Tommy's explicit go-ahead.** On merge, follow the `chore/platform-sync-*` PR to green — a
   routine non-breaking version bump (no published-contract change), single PR, no consumer migration. Then
   tick roadmap §7 "Venue legal details on booking confirmation emails + invoices" and mark the §5 row.

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
- **Phase 2** — `IBookingConfirmationNotifier` (`Concert.Application/Interfaces/`, no `Tenant.Contracts` leak
  so it follows the `IConcertNotifier` convention) + `BookingConfirmationNotifier`
  (`Concert.Infrastructure/Services/`). Registered in `ServiceCollectionExtensions`; injected into
  `ConcertDraftService` and invoked in `CreateAsync`. Logged in `api/Concertable.B2B/TECH_DEBT.md` under
  "B2B outbound email is still synchronous inline". Integration tests
  `Concert/BookingConfirmationEmailTests.cs`: (1) full accept→book transition asserts both tenants' members
  receive the email with both legal names + the seeded registered address; (2) graceful degradation via the
  pre-org-setup (tax-incomplete) tenants asserts legal name only, no address/VAT.

## Verification

- Phase 1: `Concert.Infrastructure` builds clean; `BookingConfirmationEmailGeneratorTests` — 6/6 pass
  (both-party render; `TaxCompliance` present → address + VAT; absent → legal name only; VAT null + address
  present → address shown, VAT omitted; placeholder legal name renders; HTML in legal details is encoded).
- Phase 2: `Concert.Infrastructure` + `Concert.IntegrationTests` build clean; full `Concert.UnitTests`
  139/139 pass (no regression from the `ConcertDraftService` ctor change). The two integration tests run in
  draft-PR CI (Docker/SQL-gated; not run locally per `docs/REMOTE_VALIDATION.md`).

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
