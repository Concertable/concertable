# Self-billing agreement + 12-month renewal consent — progress

- Plan: `plans/launch/SELF_BILLING_AGREEMENT_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable` (main checkout)
- Branch: `Feature/SelfBillingAgreement`
- PR: not opened
- Dependency/package gates: none yet. Phase 3 touches `api/**`, so on merge a
  `chore/platform-sync-*` PR will fire and must be owned to green before close-out.
- Last reconciled: 2026-08-04 — Phase 2 (supplier-facing grant/renew surface) built + committed
  (`6ee5cd3d2` backend; SPA to follow). Backend verified green; SPA build gate in progress.

## Current state

**Phase 2 backend COMPLETE and verified green** (`6ee5cd3d2`); SPA in progress. **Phase 1 done** earlier
(`9d968b51d`, `1a50145b4`, `83609e015`).

Phase 2 backend (`6ee5cd3d2`): `SelfBillingAgreementController` (`GET` status, `POST` grant/renew, `GET pdf`)
— `[Authorize]`, no `[RequiredTenantType]` so both tenant types reach it, single-owner scoped; 400 without an
e-signature via `GrantSelfBillingAgreementRequestValidator` (mirrors Apply/Accept). GET returns a
`SelfBillingAgreementResponse` with status (None/Active/Expired) + the HATEOAS affordance
(`Grant` when never held, `Renew` once lapsed or within the 30-day window, `Pdf` only while in force; grant/renew
are the same POST, the split carries the label). Backing it, `GetLatestAsync` (latest regardless of expiry) was
added to repo + service, replacing the now-unused service `GetCurrentAsync`. The clause text was extracted to
`SelfBillingClause` (single source). The dev/E2E seeder (`ConcertDevSeeder` → `SeededSelfBillingAgreementGranter`)
grants every seeded operator tenant a current agreement through the same domain factory + frozen identity, so
Phase 3's fail-closed gate won't defer seeded settlements. **Integration tests run `ITestSeeder`, not the dev
seeder, so every tenant starts in None state** — letting the endpoint tests drive each affordance state exactly.
Verified: `api/Concertable.slnx` builds; Concert unit **79/79** + integration **141/141** green (8 new endpoint
tests + the Phase 1 gate test).

SPA (uncommitted, build gate in progress): new `@b2b/features/selfBilling` — `selfBillingAgreementApi`,
`useSelfBillingAgreementQuery` / `useGrantSelfBillingAgreementMutation` / `useDownloadSelfBillingAgreementMutation`,
`SelfBillingAgreementPage` (reuses `ESignaturePanel`, minimally generalized with `documentNoun`/`children` props,
+ `SignatureCanvas`), `SelfBillingAgreementBanner` dashboard nag. Per-app routes `/settings/self-billing-agreement`
(venue + artist) + settings-nav link; banner added to both dashboards.

Nothing enforces settlement yet (dormant, by design) — enforcement is Phase 3.

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

Phase 2 backend is committed + verified; finish the Phase 2 SPA build gate (all four web builds green —
the `npm install` had to be re-run for a transient npm-cache EPERM), commit the SPA, then tick the plan §6
Phase 2 boxes.

Then **Phase 3 — Fail-closed enforcement + roadmap tick** (plan §6, final phase) — start only on Tommy's word:
in `FinishExecutor`, after the tax-compliance gate and using the already-resolved `supplierTenantId`, call
`ISelfBillingAgreementGate.HasCurrentAsync`; if not in force → new `SettlementOutcome.DeferredPendingSelfBillingAgreement`,
mint no invoice, surface the reason (mirror `DeferredPendingTaxCompliance`); the hourly sweep self-heals after a
grant, and the per-supplier sequence number is only consumed on a committed invoice. Add the supplier-facing
deferred surface on the settlement/payout screen. Tick `LAUNCH_ROADMAP.md` 🟡 self-billed-invoice line → ✅ +
the §7 launch-ready checklist line, **in that commit**. Phase 3 gate: `api/Concertable.slnx` builds; Concert unit
+ integration green (no-agreement → deferred, no invoice; post-grant sweep issues it; gap-free numbering
preserved). **Final phase → merge-queue full E2E tier (not skip-eligible).** On merge, own the
`chore/platform-sync-*` PR to green, then delete plan + ledger together in the close-out change.

Env note: the unrelated in-flight Deal `Dunet` NU1010 break + `Checkout.cs` move remain stashed
(`git stash list` → `stash@{0}`); pop them back when appropriate.

## Completed work

- **Phase 1** — agreement domain + persistence + gate (dormant): `9d968b51d`, `1a50145b4`, `83609e015`.
- **Phase 2 backend** — supplier grant/renew endpoints + HATEOAS affordance + dev/E2E seeder grant: `6ee5cd3d2`.
- **Phase 2 SPA** — grant/renew page, download mutation, dashboard nag, per-app routes: pending commit (build gate).

## Verification

- **Phase 1:** `api/Concertable.slnx` builds; Concert unit 9/9 + integration 1/1 green.
- **Phase 2 backend (`6ee5cd3d2`):** `api/Concertable.slnx` builds; Concert unit **79/79** + integration
  **141/141** green (full suite). 8 new endpoint tests cover: GET None+grant affordance; grant records the
  supplier e-signature → Active; grant 400 without consent (no row); both artist & venue grant; renew before
  expiry (nearing → renew affordance, appends a 2nd acceptance); renew after expiry (Expired→Active flip);
  own PDF download (`%PDF`); 404 for a tenant with no agreement.
- **Phase 2 SPA:** four web builds (`customer`/`venue`/`artist`/`business`) — gate in progress.

Per-phase gates in plan §6; model-changing phases re-scaffold via `./initial-migrations.ps1`; Phase 3 (final)
uses the merge-queue full E2E tier (not skip-eligible).

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
- **Affordance needs latest-regardless, not just in-force** (Phase 2): grant (never held) vs renew
  (lapsed/nearing) can't be told apart from the in-force query alone, so `GetLatestAsync` (most recent
  regardless of expiry) was added; the Api mapper derives None/Active/Expired + the grant/renew/pdf links
  from it against now (30-day renewal window). Service `GetCurrentAsync` was unused after this and removed;
  the repo keeps its `GetCurrentAsync` (used by the PDF path + the gate).
- **Seeder builds the entity directly, not via the request-scoped service** (Phase 2): `GrantAsync` reads
  ambient `ITenantContext`/`ICurrentUser`, absent in a background seeder — so `SeededSelfBillingAgreementGranter`
  mirrors `SeededApplicationSigner`, calling the domain factory with identity/tax from `ITenantModule` and a
  synthetic supplier e-signature. Clause text extracted to `SelfBillingClause` so both paths share one source.
- **Integration tests run `ITestSeeder`, not the dev seeder** — so the seeded grant does *not* run there and
  every tenant starts in None, which is exactly what lets the endpoint tests set up each affordance state.
- **`ESignaturePanel` reused, not duplicated** (Phase 2 SPA): minimally generalized with optional
  `documentNoun` (default "contract") + `children` (custom binding-terms body) props, both backward-compatible,
  so the self-billing page shows the self-billing clause honestly instead of contract copy.

## Event log

### 2026-08-04 — Phase 2 built (backend committed, SPA pending)

- Action: Synced `origin/main` (4 behind → merged clean), then built Phase 2. Backend committed `6ee5cd3d2`;
  SPA written (uncommitted, awaiting the four-web-build gate — `npm install` re-run for a transient npm-cache
  EPERM lock).
- Evidence: `6ee5cd3d2` (13 files); Concert unit 79/79 + integration 141/141 green; `api/Concertable.slnx` builds.
- Outcome: supplier grant/renew/read/download surface live end-to-end (dormant — no settlement gate yet);
  dev/E2E seeder grants all tenants for Phase 3.
- Follow-up: green the four web builds, commit the SPA, tick plan §6 Phase 2 boxes; Phase 3 on Tommy's word.

### 2026-08-04 — Phase 1 code written + committed (WIP)

### 2026-08-04 — Phase 1 code written + committed (WIP)

- All Phase 1 code written (17 files) and committed as WIP `9d968b51d` on `Feature/SelfBillingAgreement`,
  on top of plan commit `8457690df`. **Unbuilt/unverified**; migration not re-scaffolded; tests not written.
- Follow-up: build + migration + tests per `## Next Steps`.

### 2026-08-04 — Plan authored

- Action: Fetched `origin/main` (0 behind), created branch `Feature/SelfBillingAgreement` off
  `origin/main`. Mapped the shipped invoice engine and Contract/e-sign plumbing, then wrote
  the plan (now `plans/launch/SELF_BILLING_AGREEMENT_PLAN.md`) and this ledger from the progress template.
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
