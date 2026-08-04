# Self-billing agreement + 12-month renewal consent — progress

- Plan: `plans/b2b/SELF_BILLING_AGREEMENT.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Feature/SelfBillingAgreement`
- PR: not opened
- Dependency/package gates: none yet. Phase 3 touches `api/**`, so on merge a
  `chore/platform-sync-*` PR will fire and must be owned to green before close-out.
- Last reconciled: 2026-08-04 — plan authored from a fresh code map of the shipped
  `Feature/VatAndSelfBilledInvoicing` (invoice engine) and `Feature/BookingAgreement` (Contract/e-sign)
  code; build not started.

## Current state

Plan and this ledger authored; **no code written**. Branch `Feature/SelfBillingAgreement` created off
`origin/main` (0 commits ahead). The shipped seams the build will reuse are mapped and cited in the plan:
`InvoiceEntity`/`ContractEntity` (immutable-snapshot shape), `ESignature` VO + `ESignatureRequest`
(advanced-tier e-sign), `IPdfBlobCache` (lazy render), `IPdfRenderer` + `IDocument` (QuestPDF),
`SettlementPayeeResolver` (supplier direction), `FinishExecutor` (the fail-closed tax gate the
self-billing gate mirrors), `ITenantModule` (legal identity + VAT), the `ActionLink` HATEOAS pattern,
and the Concert `Schema`/`DisplayNames` constants.

Key design decision: the agreement is owned by the **Concert module** (max reuse of the in-place
e-sign/PDF/HATEOAS machinery; local enforcement), as a **single-supplier-tenant** entity with two
composed stances — single-owner self-service + a system tenant-filter-free gate read. Rejected
alternative (Tenant module) and its cost are recorded in the plan §3.

## Next Steps

Do not start until Tommy says to build. Then, on `Feature/SelfBillingAgreement`:

**Phase 1 — Agreement domain + persistence + gate (dormant).** Read `plans/b2b/SELF_BILLING_AGREEMENT.md`
§4–§6 and `plans/agents/PLAN.md` first. Add `SelfBillingAgreementEntity` (immutable, single-owner
supplier `TenantId`, frozen `InvoiceParty` identity, `ESignature Supplier`, `AcceptedAtUtc`,
`ExpiresAtUtc = +12 months`, `PlatformTermsVersion`, `ClauseText`, `PdfBlobName` under
`self-billing-agreements/`), `DisplayNames.SelfBillingAgreement`, `Schema.Tables.SelfBillingAgreements`,
EF config (reuse `InvoicePartyConfiguration`/`ESignatureConfiguration`), the single-owner
`SelfBillingAgreementRepository` and system-read `ISelfBillingAgreementGate`, `ISelfBillingAgreementService`
(grant/renew/read-current/PDF) with `SelfBillingAgreementDocument : IDocument`. Re-scaffold with
`./initial-migrations.ps1` from `api/`. Hit the Phase 1 verification gate, then commit.

## Completed work

None yet.

## Verification

None yet. Per-phase gates defined in the plan §6; each phase = `dotnet build api/Concertable.slnx` +
Concert unit/integration via `integration-debug`; model-changing phases re-scaffold via
`./initial-migrations.ps1`; Phase 3 (final) uses the merge-queue full E2E tier (not skip-eligible).

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **Ownership: Concert module** (plan §3). The Tenant-module alternative was rejected because it forces
  extracting `ESignature` + `IPdfBlobCache` to a shared seam — reinventing machinery the brief says to
  reuse.
- **Single-supplier-tenant tenancy, two stances** — new for Concert (all existing Concert entities are
  two-party `IVenueArtistTenantScoped`): single-owner self-service + a system/filter-free gate read by
  explicit supplier id (needed because `FinishExecutor` and the tenant-less hourly sweep check a supplier
  who is not the request tenant). Both are standard composed-tenancy building blocks.
- **Booking agreement shipped as `Contract`**, not "BookingAgreement": `ContractEntity` /
  `ContractDocument` / `contracts/` prefix / `Contract` HATEOAS link. The self-billing document mirrors
  `ContractDocument`.
- **PDFs are lazy render-on-download only** (no background pre-gen) for both invoice and contract; the
  agreement follows the same lazy pattern.
- **Fail-closed but self-healing:** the invoice legend already asserts a self-billing agreement, so
  enforcement makes it truthful; deferral (not a hard error) plus the hourly sweep means no settlement is
  permanently stranded once consent is granted, and the gap-free sequence number is only consumed on a
  committed invoice.
- **Phase order puts the grant path before enforcement** so consent (and the seeder's grant) can exist
  before the gate requires it.
- **Supplier can be artist or venue** (per `SettlementPayeeResolver`); the supplier-facing endpoints must
  not be `[RequiredTenantType(TenantType.Venue)]`.

## Event log

### 2026-08-04 — Plan authored

- Action: Fetched `origin/main` (0 behind), created branch `Feature/SelfBillingAgreement` off
  `origin/main`. Mapped the shipped invoice engine and Contract/e-sign plumbing, then wrote
  `plans/b2b/SELF_BILLING_AGREEMENT.md` and this ledger from the progress template.
- Evidence: plan + ledger files on `Feature/SelfBillingAgreement` (uncommitted at time of writing);
  code map from `Feature/VatAndSelfBilledInvoicing` / `Feature/BookingAgreement` source.
- Outcome: design + independently-shippable phases (each with a verification gate) recorded. Build not
  started, per the task.
- Follow-up: await Tommy's go-ahead to start Phase 1. When the feature ships (Phase 3), tick the
  `LAUNCH_ROADMAP.md` 🟡 self-billed-invoice line to ✅ in the same commit.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable
Read @plans/b2b/SELF_BILLING_AGREEMENT.md and @plans/b2b/SELF_BILLING_AGREEMENT_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
