# Self-billing agreement + 12-month renewal consent — progress

- Plan: `plans/launch/SELF_BILLING_AGREEMENT_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable` (main checkout)
- Branch: `Feature/Launch_SelfBillingAgreement`
- PR: not opened
- Dependency/package gates: none yet. Phase 3 touches `api/**`, so on merge a
  `chore/platform-sync-*` PR will fire and must be owned to green before close-out.
- Last reconciled: 2026-08-04 — plan authored from a fresh code map of the shipped
  `Feature/VatAndSelfBilledInvoicing` (invoice engine) and `Feature/BookingAgreement` (Contract/e-sign)
  code; build not started.

## Current state

**Phase 1 COMPLETE and verified green.** Commits `9d968b51d` (code, 17 files), `1a50145b4` (Concert
migration re-scaffold + 9 unit tests), `83609e015` (gate integration test) on `Feature/SelfBillingAgreement`.
`dotnet build api/Concertable.slnx` green; Concert unit 9/9 + integration 1/1 green. Plan §6 Phase 1 boxes
ticked. Nothing enforces or surfaces the agreement yet (dormant, by design). The renewal-append and
single-owner owner/stranger-404 checks are HTTP-level and land in Phase 2 with the endpoints.

Files in the checkpoint: `SelfBillingAgreementEntity` (immutable, single-owner `ITenantScoped`, frozen
`InvoiceParty` + `SupplierESignature`, `ExpiresAtUtc = AcceptedAtUtc + 12 months`, `ClauseText`,
`PlatformTermsVersion`, `PdfBlobName` under `self-billing-agreements/`); `DisplayNames.SelfBillingAgreement`;
`Schema.Tables.SelfBillingAgreements`; `SelfBillingAgreementConfiguration` (reuses
`InvoicePartyConfiguration`/`ESignatureConfiguration`) registered in `ConcertConfigurationProvider`; DbSet +
`ApplySingleOwner<SelfBillingAgreementEntity>` on `ConcertDbContext`; DbSet on `PublicConcertDbContext`;
single-owner `SelfBillingAgreementRepository : TenantScopedRepository<>` (`GetCurrentAsync`); system
filter-free `SelfBillingAgreementGate` on `PublicConcertDbContext`; `SelfBillingAgreementService`
(grant/renew/read-current/PDF; requires tax compliance to snapshot identity); `SelfBillingAgreementDocument`;
DI registrations. Design decisions below unchanged.

Original authoring context (still accurate): The shipped seams the build will reuse are mapped and cited in the plan:
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

Phase 1 is complete and verified. **Do not start Phase 2 until Tommy names it and says go.**

When told to start **Phase 2 — Supplier-facing grant/renew surface** (plan §6): add the endpoints
(`GET`/`POST`/`GET .../pdf` on a controller reachable by **both** tenant types — not
`[RequiredTenantType(TenantType.Venue)]`; POST rejects 400 without the e-signature), the
`selfBillingAgreement` HATEOAS affordance (grant vs renew, state-gated), the shared-SPA grant/renew page +
`useDownloadSelfBillingAgreementMutation` + dashboard nag, and the dev/E2E seeder grant. Then the Phase 2
gate (plan §6): solution + both manager SPAs build; grant-400-without-consent, grant records the
e-signature, renew before/after expiry, read-current, **both** artist & venue can grant, download-own /
stranger-404, HATEOAS grant-vs-renew flips with state.

Env note: the unrelated in-flight Deal `Dunet` NU1010 break + `Checkout.cs` move are stashed
(`git stash list`) to keep the build green; pop them back when appropriate.
4. Run Concert unit + integration via the `integration-debug` skill; drive to green.
5. Commit the finished Phase 1, check off the plan's Phase 1 box.

Then stop — Phase 2 only when Tommy names it.

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

### 2026-08-04 — Phase 1 code written + committed (WIP)

- All Phase 1 code written (17 files) and committed as WIP `9d968b51d` on `Feature/SelfBillingAgreement`,
  on top of plan commit `8457690df`. **Unbuilt/unverified**; migration not re-scaffolded; tests not written.
- Follow-up: build + migration + tests per `## Next Steps`.

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
Read @plans/launch/SELF_BILLING_AGREEMENT_PLAN.md and @plans/launch/SELF_BILLING_AGREEMENT_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
