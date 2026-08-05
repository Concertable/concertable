# Self-billing agreement + 12-month renewal consent — progress

- Plan: `plans/launch/SELF_BILLING_AGREEMENT_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable` (main checkout)
- Branch: `Feature/SelfBillingAgreement`
- PR: **#352** (https://github.com/Concertable/concertable/pull/352) — open, branch current with `origin/main`, code review clean.
- Dependency/package gates: none yet. Phase 3 touches `api/**`, so on merge a
  `chore/platform-sync-*` PR will fire and must be owned to green before close-out.
- Last reconciled: 2026-08-05 — **Phase 3 complete; PR #352 open, code-review clean (0 findings); enqueuing
  full-E2E merge.** Currency merge of `origin/main` done + rebuilt green (0 errors). Remaining: land via merge
  queue, own the `chore/platform-sync-*` PR to green, then delete plan + ledger in the close-out.

## Current state

**Phase 3 backend enforcement COMPLETE, verified green, committed** (`588da60e9`, unpushed): `FinishExecutor`
now calls `ISelfBillingAgreementGate.HasCurrentAsync(supplierTenantId, now)` after the tax gate; no current
agreement → new `SettlementOutcome.DeferredPendingSelfBillingAgreement`, mints no invoice, logs the reason
(`SettlementDeferredPendingSelfBillingAgreement`); the hourly sweep self-heals and no per-supplier sequence
number is consumed across the deferral. The minimal test seed grants no agreements (Phase 2 endpoint tests need
None-start), so `FinishConcertAsync` now ensures the concert's supplier holds a current agreement — keeping every
existing settlement/invoice test green; new `ConcertSelfBillingGateApiTests` drives finish **inline** (no grant)
to cover defer+no-invoice (artist & venue suppliers) and self-heal-after-grant with gap-free first number
(`INV-SEED000001-000001`). No model change → no migration. Verified: full `api/Concertable.slnx` builds; Concert
unit **79/79** + integration **144/144** (+3) + Workers unit **4/4** green.

**Phase 3 surface + roadmap tick DONE** (pending commit): a frontend audit found **no per-settlement status
screen exists** and the sibling tax-compliance gate is itself **banner-only** (no per-settlement "blocked
because…" UI anywhere), so building a bespoke self-billing surface would be an inconsistent one-off. Decision
(confirmed with Tommy): **the shipped Phase 2 `SelfBillingAgreementBanner` is the deferred surface.** Its copy
was sharpened so the out-of-force states (None/Expired) state explicitly that completed gigs can't be invoiced
or paid out until the supplier signs/renews (Active-but-expiring stays a softer warning). `LAUNCH_ROADMAP.md`
line 25 🟡 → ✅ and §7 launch-ready line 188 refreshed; plan §6 Phase 3 boxes ticked. All four web builds green.

**Phase 2 backend COMPLETE and verified green** (`6ee5cd3d2`); SPA committed (`b3b9b05fa`). **Phase 1 done** earlier
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

**Phase 3 COMPLETE + PR #352 open + code-review clean (0 findings).** Tommy gave the go to push (2026-08-05):
branch pushed, currency-merged to `origin/main` + rebuilt green (0 errors), PR #352 opened (plain `gh pr create`,
personal repo — no AB#/assignee). Merge-gate Step 0 review done and clean.

**Immediate next action — enqueue via the `merge` skill.** **Merge-queue full E2E tier — NOT skip-eligible**
(Phase 3 changes settlement behaviour + adds a compliance flow across both SPAs); ensure no stale skip label.
Branch is already current with `origin/main`, so enqueue: `gh pr merge 352 --merge --auto`, then confirm to a
terminal state via the AGENTS.md background until-loop. On merge, **own the `chore/platform-sync-*` PR to green**
(Phase 3 touches `api/**`). Then **delete plan + ledger together** in the close-out change (git history is the archive).

**Note:** PR #352 also carries an unrelated bundled commit — a new slim Concertable `create-gh-pr` skill
(`5070a8026`, GitHub-only, no AB#/ADO) — per Tommy's "dump all this together". Doc/skill markdown; no build/E2E impact.

Env note: the unrelated in-flight Deal `Dunet` NU1010 break + `Checkout.cs` move remain stashed
(`git stash list` → `stash@{0}`); pop them back when appropriate.

## Completed work

- **Phase 1** — agreement domain + persistence + gate (dormant): `9d968b51d`, `1a50145b4`, `83609e015`.
- **Phase 2 backend** — supplier grant/renew endpoints + HATEOAS affordance + dev/E2E seeder grant: `6ee5cd3d2`.
- **Phase 2 SPA** — grant/renew page, download mutation, dashboard nag, per-app routes: `b3b9b05fa`.
- **Phase 3 backend** — fail-closed self-billing gate in `FinishExecutor` + deferred outcome/log + gate tests: `588da60e9`.
- **Phase 3 surface + roadmap** — banner copy sharpened as the deferred surface; `LAUNCH_ROADMAP.md` self-billed line ✅: this commit.

## Verification

- **Phase 1:** `api/Concertable.slnx` builds; Concert unit 9/9 + integration 1/1 green.
- **Phase 2 backend (`6ee5cd3d2`):** `api/Concertable.slnx` builds; Concert unit **79/79** + integration
  **141/141** green (full suite). 8 new endpoint tests cover: GET None+grant affordance; grant records the
  supplier e-signature → Active; grant 400 without consent (no row); both artist & venue grant; renew before
  expiry (nearing → renew affordance, appends a 2nd acceptance); renew after expiry (Expired→Active flip);
  own PDF download (`%PDF`); 404 for a tenant with no agreement.
- **Phase 2 SPA:** four web builds (`customer`/`venue`/`artist`/`business`) green.
- **Phase 3 backend (`588da60e9`):** full `api/Concertable.slnx` builds; Concert unit **79/79** + integration
  **144/144** (+3 `ConcertSelfBillingGateApiTests`: defer+no-invoice for artist & venue suppliers; self-heal
  after grant with gap-free `INV-SEED000001-000001`) + Workers unit **4/4** green. No model change → no migration.
- **Phase 3 surface:** all four web builds green after the banner copy change.

Per-phase gates in plan §6; model-changing phases re-scaffold via `./initial-migrations.ps1`; Phase 3 (final)
uses the merge-queue full E2E tier (not skip-eligible).

## Reviews

- **2026-08-05 — full `/code-review` (merge-gate Step 0), PR #352 — CLEAN, 0 findings.** All six lenses
  passed over the `main..HEAD` self-billing diff (58 code files). Verified sound: filter-free
  `SelfBillingAgreementGate` read (explicit `TenantId == supplier && ExpiresAtUtc > now`, correct for the
  tenant-less sweep, mirrors `ConcertAvailability`); `FinishExecutor` gate ordering (tax → self-billing,
  both defer before any transition; `ConcertCompletionRunner` handles the new enum case); per-supplier
  sequence not consumed on deferral (issuance inside the transition, after both gates; pinned by test);
  entity immutability; HATEOAS affordances; defer/self-heal + endpoint + service coverage; seeding legal;
  no cross-service leaks. One close call dropped below the bar: `SelfBillingAgreementGate` primary
  constructor — its sibling `ConcertAvailability` (same folder, same dep) uses the identical form, so
  flagging only the new one would be an inconsistency-creating nitpick. Review file:
  `reviews/Feature-SelfBillingAgreement.md` (marker at HEAD).

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

### 2026-08-05 — PR #352 opened, code-review clean, enqueuing

- Action: On Tommy's go, ran the merge sequence. Currency pre-step: merged `origin/main` (2 behind — a merged
  platform-sync bump to `0.1.0-alpha.0.795`) into the branch, rebuilt `api/Concertable.slnx` → 0 errors. Pushed;
  opened PR #352 (plain `gh pr create`, no AB#/assignee). Ran the mandatory merge-gate `/code-review` (delegated
  to a background agent following the skill) over the full `main..HEAD` diff. Also bundled in, per Tommy's "dump
  all this together": a new slim Concertable `create-gh-pr` skill (`5070a8026`).
- Evidence: PR #352; review clean (0 findings, all six lenses) → `reviews/Feature-SelfBillingAgreement.md`;
  build green (0 errors) after the currency merge.
- Outcome: PR open, current with main, review-clean — ready to enqueue at full E2E tier.
- Follow-up: `gh pr merge 352 --merge --auto`; confirm terminal state; own the `chore/platform-sync-*` PR to
  green; then delete plan + ledger in the close-out.

### 2026-08-05 — Phase 3 backend enforcement built + committed (`588da60e9`)

- Action: On Tommy's word, started Phase 3. Synced `origin/main` (5 behind → merged clean). Added
  `SettlementOutcome.DeferredPendingSelfBillingAgreement` + `SettlementDeferredPendingSelfBillingAgreement` log;
  wired `ISelfBillingAgreementGate.HasCurrentAsync` into `FinishExecutor` after the tax gate. Key integration
  decision: the minimal test seed grants no agreements (Phase 2 endpoint tests need None-start), so extended
  `FinishConcertAsync` to ensure the supplier holds a current agreement (keeps all existing settlement/invoice
  tests green) and drove the new gate tests' finish **inline** to exercise the deferred path.
- Evidence: `588da60e9` (6 files). Full `api/Concertable.slnx` builds; Concert unit 79/79 + integration 144/144
  (+3 new: `ConcertSelfBillingGateApiTests`) + Workers unit 4/4 green. No model change → no migration.
- Outcome: settlement is now fail-closed on a current self-billing agreement; deferral mints no invoice, burns no
  sequence number, and self-heals on grant. Dormant→enforced.
- Follow-up: supplier-facing deferred surface + roadmap tick (same commit); then PR / full-E2E merge / platform-sync.

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
