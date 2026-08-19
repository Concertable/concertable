# Concertable Launch Roadmap

> **Roadmap** for the launch epic — the living progress tracker, not a plan (no `_PROGRESS.md`, never deleted, lives until launch). Each buildable item spins off its own feature plan; see [`../agents/ROADMAP.md`](../agents/ROADMAP.md).
>
> **Goal:** Production launch of the B2B platform (venue↔artist booking + automated settlement) by **November 2026**.
>
> **Companion docs:** [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md), [USER_MODEL_PLAN.md](../b2b/USER_MODEL_PLAN.md), [MARKETPLACE_PLAN.md](../marketplace/MARKETPLACE_PLAN.md), [../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md).

---

## Status — what's shipped vs. what's left

**Shipped — verified in code, don't rebuild:** Tenant model + membership + 6-role RBAC · payout re-keyed to `TenantId` · Stripe Connect Express with both money flows (escrow `OnBehalfOf` for FlatFee/VenueHire; `TransferData.Destination` for DoorSplit/Versus) · temporary Payment-owned £10 platform fee on all four settlement types · append-only balanced Payment ledger · artist↔venue messaging · settlement for all four contract types · the 3% PRS skim is correctly absent.

**Decisions locked (see §9 / decision log):**
- [x] Revenue model — **resolved for launch 2026-07-30: one Payment-owned percentage of the final B2B-calculated deal gross.** The payer pays gross plus commission and the payee receives gross. B2B owns four deal-gross strategies; Payment owns one deal-agnostic commission calculation. The shipped £10 fee is temporary and must be removed before launch. See [PLATFORM_COMMISSION_PLAN.md](PLATFORM_COMMISSION_PLAN.md) and the decision log.
- [x] DoorSplit/Versus revenue source — **resolved: manual door-takings entry + charge-the-venue** for v1 (external-ticketer import ruled out — §9). All four contract types ship in the pure-B2B MVP, no marketplace dependency. See §9 / R9.

**Code sweep 2026-08-16 — eight previously untracked gaps.** A verification pass over the admin,
tenant-verification, Stripe-webhook, GDPR, rate-limiting and audit surfaces found work the roadmap had
never listed: four launch gates (webhook coverage, tenant verification, admin console, GDPR subject
rights), rate limiting, the open settlement-dispute decision in §9, and two post-launch rows (email
preferences, admin audit log). They are folded into the lists below and into §5/§7/§9 with their
code evidence inline, rather than kept as a separate appendix. The three "verify before trusting"
table-stakes items were resolved in the same pass.

**Build — MVP blockers, in priority order:**
- [x] ✅ **Concert cancellation + escrow refund** — cancel a *booked concert* (escrow `Held`): `Booked → Cancelled` + refund. Wires `EscrowEntity.Refund()` (the method existed; B2B never called it). Shipped in PR #76 (concert-cancel path across all four contract types, venue SPA cancel action, API + UI E2E). **This is the concert-cancel path only** — application-cancel below is still open.
- [x] ✅ **Application cancellation** — shipped (`Feature/ApplicationCancel`): artist **withdraw** + venue **reject** from `Applied`; venue **cancel** / artist withdraw from `Accepted`/`PaymentFailed` → terminal `Cancelled` with the escrow unwind via the existing `RefundByBookingIdAsync` (no new Payment capability), late-capture compensation for the 3DS-window race, opportunity re-opens on cancel (application- and concert-cancel alike), HATEOAS-gated actions in both manager SPAs (venue Deny/Cancel; artist My Applications page + Withdraw). Optional FlatFee hold-release RPC deliberately skipped — orphaned accept-checkout holds self-expire in ~7 days (logged in [api/TECH_DEBT.md](../../api/TECH_DEBT.md)).
- [x] ✅ **E-signed booking agreement** — shipped (`Feature/BookingAgreement`): click-wrap consent at Apply + Accept, agreed terms snapshotted at Accept (immutable `BookingAgreementEntity`, terms-fingerprint guard against mid-flight edits), PDF via `IPdfRenderer` (`BookingAgreementDocument`, generated background-at-Accept with a lazy render-on-download fallback, stored under the `agreements/` blob prefix), both-party-authorized `GET /api/Application/{id}/agreement` + `/agreement/pdf` endpoints, HATEOAS `agreement` link, download links in both manager SPAs. **Advanced-tier self-hosted e-signature** (typed full name required + optional drawn signature, rendered into the PDF Signatures block; no third party / no per-signature cost) — upgraded from the original Tier 1 click-wrap. LEGAL_REQUIREMENTS item 2.
- [x] ✅ **DoorSplit/Versus door-take entry at settlement** — shipped (`Feature/DoorRevenueSettlement`): the venue declares the **external** door take on an ended, still-`Booked` revenue-share concert (`POST /api/Concert/{id}/door-revenue`, venue-tenant guarded, HATEOAS-gated "Enter door takings" action in the venue SPA). Settlement charges the artist's % of **`TicketsSold × Price + DoorRevenue`** — Concertable's own ticket sales **plus** the declared external take, never either alone. The "awaiting declaration" rule is single-sourced via a composable `PredicateSpecification` combinator (published to Kernel, platform `.576`), shared by the completion sweep and a backend `AwaitingDoorRevenue` KPI. **All four contract types now settle.** See §1 / R9.
- [x] ✅ **DAC7 onboarding completion** — shipped (`Feature/Dac7Onboarding`): fail-closed DAC7 payout gate + jurisdiction seam (UK-only, keyed strategy) + tax-details nag on both dashboards; VAT collapsed to a single number. NINO / UTR / Company-Reg on `Tenant.Compliance`; no payout until the payee tenant is jurisdiction-complete (no de-minimis for services — reportable from £1).
- [x] ✅ **Self-billed VAT invoice engine** — complete (`Feature/VatAndSelfBilledInvoicing` + `Feature/SelfBillingAgreement`): invoice generation (per-settlement immutable invoice, gap-free per-supplier numbering, VAT-status branching (inclusive-gross decompose), HMRC self-billing legends + both parties' VAT numbers, PDF via `IPdfRenderer` lazy render-on-download `invoices/` prefix, two-party-scoped `GET /api/Concert/{id}/invoice[/pdf]` + HATEOAS link — items 1, 3, 4) **plus** the per-supplier self-billing *agreement* + 12-month renewal consent: immutable e-signed `SelfBillingAgreementEntity` (single-owner, frozen identity + supplier e-signature, `ExpiresAtUtc = AcceptedAtUtc + 12 months`, lazy PDF under `self-billing-agreements/`), append-only grant/renew surface in both manager SPAs (HATEOAS grant/renew/pdf + dashboard nag), and a **fail-closed settlement gate** — `FinishExecutor` mints no self-billed invoice unless the supplier holds a current agreement, deferring + self-healing on the hourly sweep exactly like the tax-compliance gate. The invoice's "raised under a self-billing agreement" legend is now always truthful.
- [x] ✅ **`holdsMusicLicence` attestation** `launch/music-licence-attestation` on `Tenant.Compliance` — shipped (`Feature/launch_music-licence-attestation`): one `bool` on the shipped `TaxCompliance` VO, threaded through the org read/update DTO + mapper and the b2b/shared Org setup form (new "Music licence" checkbox). Record-only — the venue's responsibility; no verification, no payout/booking gate.
- [ ] 🟠 **Manager front-page dashboards.** `launch/manager-front-page` Venue and artist manager workbenches are implemented through the current fixture-backed and KPI slices; remaining UX freeze, real-data wiring, delivery, and closeout are owned by [MANAGER_FRONT_PAGE_PLAN.md](MANAGER_FRONT_PAGE_PLAN.md).
- [x] ✅ **Swim-lane B complete** — membership/invitation endpoints + auth sweep + messaging group-inbox (USER_MODEL_PLAN Phases 6-8, all shipped; plan deleted). **Phase 6** (`Feature/TenantInvitationsFrontend`): invitation endpoints + last-Owner invariants + provisioning invitation-first branch + member-management UI + tenant switcher + UI E2E. **Phase 8** (`Feature/MessagingGroupInbox` + `Feature/MessagingGroupInboxPhase2`): tenant-owned conversations, per-member read pointer, member SignalR + email fan-out, org-identity/member-attribution DTO + group-inbox SPA, new Conversations unit/integration + UI E2E. **Phase 7** (`Feature/RetireRoleClaim`): retired the flat `Role` enum, manager-profile tables, and the `role` token claim — B2B tokens are identity-only; `/me` collapsed to one membership-shaped DTO; guards/persona derive from tenant memberships.
- [x] ✅ **Per-contract-type VAT calculation** — shipped (`Feature/VatAndSelfBilledInvoicing`): inclusive-gross decomposition branching on supply direction + supplier VAT-registration status, in the Tenant tax area, consumed by Concert via `ITenantModule` (items 1, 3).
- [ ] 🟠 **Percentage commission + B2B pricing transparency** `launch/platform-commission` — Payment Phase 1 is merged, published and synced: immutable percentage revisions, Payment-issued bindings, binding-aware money RPCs, and durable transaction/refund/tax/ledger facts. Phase 1b now removes caller-supplied commission and total from post-binding actions so B2B retains only the binding ID and frozen gross; its package publication, Payment deployment and platform sync are the hard gate before the four gross strategies and payer disclosure in Phase 2. The temporary £10 seam is removed only in Phase 3. See [PLATFORM_COMMISSION_PLAN.md](PLATFORM_COMMISSION_PLAN.md).
- [x] ✅ **Browser-storage audit + consent correction** `launch/browser-storage-consent` — shipped (`Feature/launch_browser-storage-consent`, #482): evidence-led audit (static sweep + anonymous runtime capture) of the four SPAs' device storage, every item classified necessary/functional/optional in a drift-guarded `app/web/shared/src/lib/storageManifest.ts` and the engineering inventory `app/web/shared/BROWSER_STORAGE.md`. Removed the dead `sidebar_state` cookie; made the two boot-time third parties load on use only (lazy Stripe `getStripe()`; Google Maps via a scoped `MapsProvider` on find/detail routes, no longer at app boot); added `consentGate.ts` so the retained analytics/marketing banner's toggles actually gate loading (the integration point for roadmapped GA4/pixels). Banner retained by decision — analytics/marketing is roadmapped and UK PECR mandates the banner once such tech loads. Legal-gated tail only: solicitor policy-copy wire into the `/cookies` page (separate item, line 198) and whether Maps needs a `functional` consent category.
- [ ] 🔴 **Stripe webhook coverage — disputes, account status, money-movement failures** `launch/stripe-webhook-coverage` — surfaced by the 2026-08-16 sweep. Payment handles exactly four events (`payment_intent.succeeded|payment_failed`, `setup_intent.succeeded|setup_failed`); [../payments/PROVIDER_CONTRACT_BASELINE_PLAN.md](../payments/PROVIDER_CONTRACT_BASELINE_PLAN.md) confirms "only succeeded/failed subsets are handled today" and puts full webhook handling **outside its own scope**, so nothing owns this. Unhandled: **`charge.dispute.created`** — chargebacks are invisible, and `EscrowStatus.Disputed` is an enum value nothing ever sets; **`account.updated`** — a connected account losing its payouts capability is never detected, so settlement keeps routing money at a restricted account; **`payout.failed`/`transfer.failed`** — silent money failures. Hard gate: this is real money on the differentiating settlement path. Sequence after the Payment provider-contract baseline so normalized states and transition legality land first.
- [ ] 🔴 **Tenant verification — venue and artist legitimacy** `launch/tenant-verification` — surfaced by the 2026-08-16 sweep. `VenueEntity.Approved` and `[Admin] PATCH /api/Venue/{id}/approve` exist, but the flag is **decorative**: no query filter, guard or workflow reads it, so an unapproved venue publishes opportunities, accepts artists and takes money exactly like an approved one. No evidence is ever collected (a venue uploads only `Banner`/`Avatar`); there is no reject/pending/suspended state, no reason, no re-submission, no notification, and no record of who approved. Artists have no verification concept at all. Needs an evidence-upload surface (licence / proof of address / company docs) on its own blob prefix, a real verification state machine on the tenant, admin review with reasons, and the gate **actually enforced** at opportunity publication and at settlement.
- [ ] 🔴 **Admin console + production admin provisioning** `launch/admin-console` — surfaced by the 2026-08-16 sweep. There is **no admin SPA** (`app/web/` is `b2b`, `customer`, `shared`) and **no production path to create an admin**: the `Admin` policy is granted only when a credential registers with `ClientId == "admin"` (`CredentialRegisteredHandler`), which only `AuthDevSeeder` ever does. The already-shipped admin backends — OSA moderation (hide/restore/resolve) and venue approval — are therefore unreachable in production. Blocks `launch/tenant-verification` and is what actually closes the OSA enforcement loop.
- [ ] 🔴 **GDPR subject rights — erasure + data export** `launch/gdpr-subject-rights` — surfaced by the 2026-08-16 sweep. No account deletion, data export or anonymisation anywhere in `api/` or `app/`; the roadmap tracks the ICO *fee* but no DSAR capability. Not a `DELETE` endpoint: settled invoices, self-billing agreements and ledger entries are HMRC-retained for six years, so this needs a designed retain-vs-erase split (anonymise the identity, keep the financial record), an export format, and a documented response SLA.
- [x] ✅ **API rate limiting** `launch/rate-limiting` — shipped (`Feature/launch_rate-limiting`) as an opt-in seam in `Concertable.ServiceDefaults` (`AddDefaultRateLimiting`/`AddRateLimitPolicy`/`UseDefaultRateLimiting`, 429 + `Retry-After`, per-`sub` or per-IP fixed-window partitioning, lazy per-policy config binding; producer #655 + platform-sync #663) plus named policies applied across all five web hosts on the ~36 real abuse surfaces the sweep identified (credential/change-password, public reads, blob upload, apply/messaging/checkout, profile-image, purchase/review, search, setup-intent). Opt-in, no global fallback — evidence and rationale in [RATE_LIMITING_PLAN.md](RATE_LIMITING_PLAN.md). In-process only (distributed store deferred) and three adjacent anonymous-endpoint auth gaps logged in [api/TECH_DEBT.md](../../api/TECH_DEBT.md).
- [ ] 🔴 **Production deployment + config/secrets** — the app has **no** deployment path, config store, or secret store (all local Aspire + emulators; secrets committed to source, incl. a plaintext Azure SQL password). Surfaced 2026-07-17. Hard launch gate. Plan: [../CONFIG_AND_DEPLOYMENT_PLAN.md](../platform/CONFIG_AND_DEPLOYMENT_PLAN.md).

**Architecture refactors — ready, not launch gates:**

- [x] ✅ **Deal-type strategy registration** — shipped in PR #451: module-local factories and vertically declared registration replace the repeated `DealType → strategy` dictionaries while preserving named business facades and the Deal/Concert boundary. `launch/deal-strategy-registration`
- [ ] 🟡 **Deal representation and common-interface dispatch** `launch/deal-closed-sum-model` — immediate architecture owner before lifecycle PR #633 resumes. First land the B2B-local generator/analyzer and Deal-owned mapper/updater net10 foundation from current `main`; then PR #633 consumes it for Application terms and heterogeneous operation factories. One reusable generator template emits invariant common-interface factories and dedicated union factories while each runtime factory remains module-owned. Heterogeneous operations use Dunet implementation unions on net10 and native implementation unions on C# 15; consumers match operation kind and multiple Deals may share one implementation. The later .NET 11 cut-over closes the published Deal hierarchy without changing consumer factory APIs. Plan: [DEAL_CLOSED_SUM_MODEL_PLAN.md](DEAL_CLOSED_SUM_MODEL_PLAN.md).
- [ ] 🔴 **Application → Booking → Concert module ownership** `launch/deal-lifecycle-ownership` — design approved 2026-08-16; draft PR #633 is suspended until the Deal generator/mapper/updater foundation is terminal on `main`, then resumes its preserved compile-recovery frontier and consumes that machinery directly. Split the current
  Concert umbrella into honest Opportunity, Application, Booking/Contract, and Concert ownership;
  each lifecycle aggregate owns independent state, transitions, and contextual operations. Its current
  keyed selectors are provisional delivery seams owned for replacement by the Deal dispatch plan:
  honest same-interface mapper/updater/terms families use generated invariant factories, while
  heterogeneous lifecycle executors/steps use unions and matches; identical behavior is direct and
  static variation is data. The fixed
  stage order never varies by `DealType`; no umbrella process entity, shared
  workflow module, cross-module state machine, Deal-owned orchestration, or Rust decision engine is
  allowed. The follow-on .NET 11 slice owns native unions for closed internal values, never DI service
  dispatch. See
  [DEAL_LIFECYCLE_OWNERSHIP_PLAN.md](DEAL_LIFECYCLE_OWNERSHIP_PLAN.md).

**Competitor table-stakes — verified ABSENT 2026-08-16 (was "verify before trusting"):**

- **B2B reviews/reputation** `launch/b2b-reviews` — `IVenueReviewService`/`IArtistReviewService` in B2B are read-only projections over *fan* reviews (rows keyed by `Email`). There is no venue↔artist post-gig review submission anywhere.
- **Calendar sync** (Google/Apple/Outlook) `launch/calendar-sync` — nothing, not even an ICS feed.
- **Financial/settlement CSV export** `launch/settlement-export` — zero occurrences in `api/`.

None is a launch gate. All three are post-launch competitive parity unless a beta venue demands one.

The legal/business track is [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md); the hard launch gates are in §7.

---

## 1. Vision and scope

**In scope for the v1 launch:**
- B2B SaaS marketplace for venue↔artist bookings
- Four contract types (FlatFee, DoorSplit, VenueHire, Versus)
- Automated settlement via Stripe Connect Express
- Disclosed-agent legal posture (Concertable acts as venue/artist's agent for money handling)
- Multi-staff Tenant model (Owner + Manager roles)
- DAC7-compliant seller onboarding
- Cancellation/refund handling on the B2B path (venue or artist cancels — escrow refunds correctly)
- Per-booking signed agreement (click-wrap e-signature, terms snapshotted at Accept) — see [LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md) item 2
- Per-contract-type VAT calculation + VAT-compliant self-billed invoices per settlement (items 1, 3, 4)
- Per-tenant configuration surface (PRS, VAT, payment terms, cancellation defaults). The platform fee is Payment-owned platform configuration, not a tenant override.

**DoorSplit/Versus revenue source — resolved (2026-06-22):** these two settle against door/ticket revenue, which standalone B2B (no marketplace) has no automatic feed for. The confirmed v1 feed is **manual door-take entry + charge-the-venue**: the venue enters the door take at settlement, Concertable charges the venue for the artist's share (+ our fee) and pays the artist through Stripe Connect — identical to FlatFee escrow. So **all four contract types ship in the pure-B2B MVP** with no marketplace dependency. The own checkout (deferred) only *upgrades* DoorSplit/Versus later (verified number instead of self-reported, no venue credit risk). See §9.

**Out of scope for v1 (planned, not abandoned):**
- Customer-facing ticket marketplace — see [MARKETPLACE_PLAN.md](../marketplace/MARKETPLACE_PLAN.md). Designed to be additive; switch-on planned Q1 2027 or later once B2B has traction.
- Mobile app distribution to App Store / Play Store
- Native push notifications
- Multi-currency / international expansion
- More granular membership roles beyond Owner/Manager
- Org-switcher UI (one user managing multiple orgs)

**The differentiation thesis:** GigPig and GigXchange are flat-fee booking tools — GigPig even markets automated payments and a "Payment House" that splits one venue payment across artists (2026 site copy) — so *flat-fee* booking + auto-payout is table stakes, not a moat. Concertable's edge is **settling the revenue-share contract types (DoorSplit/Versus), which neither competitor does**: the door take is entered at settlement and the artist's share moves through our own Stripe Connect (charge the venue → pay the artist). Crucially this needs **no ticketing ownership** — manual door-take entry + charge-the-venue is enough — so the moat is the typed revenue-share *settlement*, not a ticketing platform. Unlike DICE (closed, ticketing-first), Concertable also owns the venue↔artist booking + contract workflow. **That is the whole MVP thesis: out-compete GigPig/GigXchange on the contract types they structurally can't settle** — which is why DoorSplit/Versus must be sellable at v1 (now resolved via manual entry, §9), not deferred.

## 2. Three parallel swim-lanes

Three workstreams run in parallel across the six months. Each has different owners and dependencies.

### Swim-lane A — Legal & Business
**Owner:** you (with solicitor + accountant)
**Detail:** [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md)

Company registration, ICO, T&Cs, insurance, accounting, HMRC platform-operator registration, Stripe production activation. Mostly admin work scattered across the six months; some elapsed-time dependencies (solicitor drafting takes 2-4 weeks).

### Swim-lane B — Architecture
**Owner:** you (or contractor dev)
**Detail:** [USER_MODEL_PLAN.md](../b2b/USER_MODEL_PLAN.md)

The tenancy refactor — the load-bearing structural change that everything else attaches to, sequenced as the phases in the timeline below. The tenant-scoping foundation (Tenant module with a Guid PK, request-scoped tenant filtering, the compliance value object) has shipped; the outstanding work — multi-user membership, roles, and the authorization sweep — is tracked in [USER_MODEL_PLAN.md](../b2b/USER_MODEL_PLAN.md).

### Swim-lane C — Compliance UI/UX + workflow polish
**Owner:** you (or contractor dev)
**Detail:** §5 of this plan

The smaller code items that don't fit in either of the other swim-lanes: browser-storage audit and consent correction, pricing transparency, refund/cancellation codification, DAC7 export script, legal-page routes, OSA report-content flow, etc. Some items block on legal text (T&Cs) being drafted first; others can run earlier.

## 3. 6-month timeline

Calendar-realistic, not optimistic. Slips are flagged as risks (§6).

| Month | Swim-lane A (Legal/Business) | Swim-lane B (Architecture) | Swim-lane C (Compliance UI/UX) |
|---|---|---|---|
| **Month 1 (Jun 2026)** | Company registered (Companies House, ~£12, 24hr) · ICO fee paid (~£40-60/yr) · Solicitor engaged + briefed for T&Cs · **Revenue model decided** · **DoorSplit/Versus revenue-source decision** (§9) | **Phase 0** — `Tenant` module scaffolding · **Phase 1** — `ComplianceContext` value object + tenant config surface | **Music licence attestation field** spec (= PRS self-licensed flag; wired in Phase 1) · _(PRS correction in `LEGAL_REQUIREMENTS.md` ✅ done 2026-06-01)_ |
| **Month 2 (Jul 2026)** | Business bank account opened · Accountant engaged · Solicitor drafts circulating | **Phase 2** — Venue/Artist wired to Tenant | Browser-storage inventory + policy classification; consent only where actual optional technology requires it |
| **Month 3 (Aug 2026)** | Insurance arranged (Professional Indemnity + Cyber) · Stripe production application submitted | **Phase 3** — `PayoutAccountEntity` re-key to TenantId | **Pricing transparency** at each payer commitment point (Payment quote package first) |
| **Month 4 (Sep 2026)** | Solicitor T&Cs finalised · DPA signed with Stripe · ICO documentation (privacy policy, lawful basis, retention) | **Phase 4** — `ComplianceContext` snapshot on Booking · **Phase 5** — Organization setup UI | **Privacy + T&Cs page routes** wired up (solicitor text now in hand) · **Venue legal details on emails** template change · **Booking agreement + click-wrap e-sign** at Accept (PDF via `IPdfRenderer`) |
| **Month 5 (Oct 2026)** | HMRC platform-operator registration · Stripe production approved · Marketing site live | **Phase 6** — Multi-user membership + auth sweep · **Admin console + admin provisioning** · **Tenant verification** (needs the console) | **Refund / cancellation codification** in `Cancelled` workflow · **Per-contract VAT calculation** + **self-billed invoice generation** (reuses agreement PDF plumbing) · **OSA report-content flow** (button + email + policy doc) · **DAC7 export script** (defer the actual run until Jan 2028) |
| **Month 6 (Nov 2026)** | Beta cohort recruited (~10 venues + 50 artists) · Support process live · Pricing page live | Bugfixes from beta feedback · final integration tests | **Stripe webhook coverage** (disputes / account status / payout failures) · **GDPR erasure + export** · **Rate limiting** · Final polish · accessibility quick-pass · **LAUNCH** |

## 4. Critical path

Dependencies that constrain the order:

```
Percentage commission decision (Month 1)
    └─→ Payment binding package → platform sync → B2B gross calculators + pricing transparency (Month 3)
    └─→ Solicitor T&Cs drafting (Month 1-4)
            └─→ Privacy + T&Cs page routes (Month 4)
            └─→ Refund / cancellation codification (Month 5)

Browser-storage inventory (anonymous → authenticated → Stripe checkout)
    └─→ remove unjustified storage + classify necessary Auth/Stripe storage
            └─→ cookie/storage policy (solicitor-reviewed)
            └─→ consent mechanism only for actual non-exempt optional technology

Phase 0 — Tenant scaffolding (Month 1)
    └─→ Phase 1 — Compliance value object (Month 1-2)
            └─→ Phase 2 — Venue/Artist FK (Month 2)
                    └─→ Phase 3 — Stripe re-key (Month 3)
                            └─→ Phase 4 — Booking snapshot (Month 4)
                                    └─→ Phase 5 — Setup UI (Month 4)
                                            └─→ Phase 6 — Membership refactor (Month 5)
                                                    └─→ Beta + launch (Month 6)

Admin console + production admin provisioning (Month 5)
    └─→ Tenant verification (evidence upload + admin review + enforced gate)

Payment provider-contract baseline
    └─→ Stripe webhook coverage (disputes, account.updated, payout/transfer failures)

Stripe production approval (~2-4 weeks elapsed)
    └─→ Must be approved before Month 6 launch
```

**Hard gates that block launch:**
- Solicitor-drafted T&Cs in production (Month 4)
- ICO fee paid (Month 1)
- Stripe production approved (by Month 5)
- DAC7 fields collected for every paid seller (Month 4 onwards, soft gate)
- Insurance active (Month 3)

## 5. Swim-lane C — Compliance UI/UX work in detail

| Item | Effort | Depends on | Month |
|---|---|---|---|
| PRS correction in `LEGAL_REQUIREMENTS.md` (✅ done 2026-06-01 — was "remove 3% line"; now per-tenant pass-through, venue's liability) | – | – | done |
| Music licence attestation field (on `Tenant.Compliance`) = PRS self-licensed flag | 0.5 days | Phase 1 | Month 1 |
| Tenant configuration surface (PRS / VAT / payment terms / cancellation defaults) | 1-2 days | Phase 1 | Month 1-2 |
| Booking agreement + click-wrap e-signature at Accept (snapshot terms, PDF via `IPdfRenderer`) — `LEGAL_REQUIREMENTS.md` item 2 | 3-5 days | Phase 4 (Booking snapshot), `IPdfRenderer` | Month 4 |
| ✅ Per-contract-type VAT calculation (branches on supply direction + supplier VAT status) — items 1, 3 | 2-3 days | Tenant config (VAT fields) | done |
| ✅ Self-billed VAT invoice generation per settlement (sequential numbering, HMRC fields, PDF) — item 4 · self-billing *agreement* + renewal still outstanding | 2-3 days | VAT calculation, agreement PDF plumbing | done |
| Browser-storage audit + consent correction across all four web SPAs; remove unjustified storage/scaffolding and document or gate what remains | TBD by feature plan | Browser evidence + solicitor classification | Pre-launch |
| Percentage commission + pricing transparency at payer commitment (exact checkout + deferred settlement review) | 3 phases | Payment binding package + platform sync | Month 3 |
| Privacy + T&Cs page routes (footer of every page) | 1 day | Solicitor draft | Month 4 |
| Venue legal details on emails (booking confirmation, invoices) | 1 day | Phase 5 (setup UI captures legal name) | Month 4 |
| Refund / cancellation matrix codification in `Cancelled` workflow | 3-5 days | Cancellation policy text from solicitor | Month 5 |
| ✅ Online Safety Act report-content flow — in-app report route, persisted report record, acknowledgement + safety-inbox emails, and admin moderation (hide/restore/resolve). The published `report@`/`safety@` fallback ships with the footer legal pages (solicitor-gated), so the reporting route is not fully closed | 1 day | – | done |
| Tenant suspension as an admin enforcement action (suspension state enforced at membership resolution; held escrow + pending payouts resolved explicitly per booking) — split out of the OSA report-content work 2026-08-14: suspending a paying customer needs the illegal-content **enforcement clause in the T&Cs**, which is solicitor-owned and does not exist yet | 2-3 days | T&Cs enforcement clause **[LEGAL]** | Post-solicitor |
| Admin console SPA + production admin provisioning (unlocks the shipped OSA moderation + venue approval backends) | 5-8 days | – | Pre-launch |
| Venue/artist verification: evidence upload + admin review workflow, gate enforced at opportunity publication + settlement | 5-8 days | Admin console | Pre-launch |
| Stripe webhook coverage: `charge.dispute.created`, `account.updated`, payout/transfer failures | 3-5 days | Payment provider-contract baseline | Pre-launch |
| GDPR erasure + data export (retain-vs-erase split against HMRC six-year retention) | 3-5 days | Retention policy from solicitor **[LEGAL]** | Pre-launch |
| API rate limiting across auth, apply, messaging, upload | 1 day | – | Pre-launch |
| Venue↔artist settlement dispute path (contested door take / no-show) — see §9 | TBD by decision | Dispute + mediation clause from solicitor **[LEGAL]** | Post-solicitor |
| Email notification preferences + unsubscribe (the PECR line between transactional and marketing mail) | 1-2 days | – | Post-launch |
| Admin action audit log (which admin approved / hid / resolved what, and why) | 1-2 days | Admin console | Post-launch |
| DAC7 annual export script (writes XML in HMRC schema, doesn't run until Jan 2028) | 2-3 days | Phase 6 complete | Month 5 |

**Total Swim-lane C effort:** ~40-60 working days (up from ~20-31 after the 2026-08-16 sweep added the admin-console, tenant-verification, webhook-coverage, GDPR, rate-limiting, dispute-path, email-preferences and audit-log rows). Roughly 8-12 calendar weeks of focused work, spread across the 6 months because of dependency timing — the sweep roughly doubled this lane, so the Month 5-6 window is now the binding constraint, not the VAT chain. The VAT chain (calculation → invoice) remains the densest legacy cluster in Month 5 — watch it doesn't collide with the Phase 6 auth sweep (R6).

## 6. Risk register

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Tenancy refactor takes longer than 24 days (EF nested owned-types surprises, migration-script issues) | Medium | High | Phase 0 scaffolding has explicit go/no-go assessment at the end. If it took >3 days, recalibrate timeline before continuing. |
| R2 | Solicitor T&Cs drafting takes longer than 4 weeks | Medium | High | Brief solicitor in Month 1, not Month 3. Keep a parallel "draft v1" using a quality T&Cs template as backup. |
| R3 | Stripe production approval delayed (Stripe asks for more info / rejects) | Medium | High | Submit application Month 3, not Month 5. Have ICO fee + insurance + company info ready as supporting docs. |
| R4 | Pricing shown by B2B drifts from Payment's live fee before delayed settlement | Medium | High | Require a Payment-issued binding to an immutable Payment configuration revision on the eventual charge; never duplicate live fee config in B2B. |
| R5 | Beta cohort hard to recruit (no organic demand pre-launch) | Medium | Medium | Start recruitment Month 4 not Month 6. Hand-pick first 10 venues + 50 artists via warm intros, not open signups. |
| R6 | Phase 6 auth sweep introduces regressions across 25+ controllers | Medium | Medium | Test coverage assessment in Month 4. If integration test coverage is <60%, write tests first or split Phase 6 into smaller PRs. |
| R7 | DAC7 schema changes between now and first export (Jan 2028) | Low | Low | Defer DAC7 export *implementation* if HMRC publishes schema updates; keep onboarding field collection on-spec. |
| R8 | Solicitor flags an issue we haven't planned for (e.g. requires PSR registration, not just disclosed-agent) | Low | High | First solicitor consultation in Month 1 should explicitly confirm disclosed-agent posture is viable on Stripe Connect Express. If they push back, this plan needs major rework. |
| R9 | DoorSplit/Versus manual-entry settlement screen slips → two of four contract types unsellable at launch | Low | Medium | **Resolved 2026-06-22 (§9):** manual door-take entry + charge-the-venue feeds DoorSplit/Versus at v1 so all four ship; external-ticketer import ruled out; owned checkout (marketplace) is the deferred durable feed. Residual is only *building* the door-take entry screen — the money mechanic reuses FlatFee escrow. FlatFee + VenueHire remain the standalone floor if that screen slips. |
| R10 | VAT calculation + invoice work (Month 5) collides with Phase 6 auth sweep | Medium | Medium | Both land Month 5. If Phase 6 is running hot, pull the VAT chain forward to Month 4 (it depends only on the tenant VAT fields from Phase 1, not on Phase 6). |
| R11 | The 2026-08-16 sweep roughly doubled Swim-lane C (~20-31 → ~40-60 days) against a fixed November 2026 date, and four of the additions are launch gates landing in Months 5-6 — the same window as the Phase 6 auth sweep and production deployment | **High** | **High** | The lane no longer fits its window on current sequencing. Either move the launch date, or cut scope explicitly: the honest candidates are shipping tenant verification as manual/offline admin review (evidence by email, flag flipped by hand) rather than a built upload workflow, and deferring rate limiting to a CDN/gateway rule. Do **not** cut the webhook-coverage or GDPR gates — the first is money correctness on the differentiating path, the second is a regulator obligation. Reassess at the end of Month 4. |

## 7. Definition of "launch-ready"

Concrete checklist for Month 6. Don't launch without all of these green.

### Legal/business
- [ ] Limited company registered, PSC filed
- [ ] ICO fee paid for the current period
- [ ] Solicitor-drafted T&Cs live on the platform: Platform terms, Venue seller terms, Artist seller terms, Privacy policy, Cookie policy
- [ ] Refund + cancellation policy documented and codified in the `Cancelled` workflow
- [ ] DPA signed with Stripe; DPA template ready for venue/artist signing
- [ ] Insurance active (Professional Indemnity + Cyber)
- [ ] Accountant engaged; first quarterly review scheduled
- [ ] HMRC platform-operator registration filed (DAC7)
- [ ] Stripe production account approved + webhooks live

### Architecture
- [ ] Tenancy refactor merged and integration-tested (tenant-scoping done; membership + auth sweep per USER_MODEL_PLAN.md still outstanding)
- [ ] All Stripe Connect Express payouts flowing through TenantId
- [ ] ComplianceContext snapshot populated on every Booking created post-launch
- [ ] Auth checks routed through tenant membership (not legacy TPH FK)
- [x] Booking agreement generated + click-wrap consent recorded at every Accept
- [x] VAT calculated per contract type + self-billed invoice generated per settlement, gated on a current e-signed self-billing agreement (12-month renewal)
- [ ] Tenant config surface live (PRS / VAT / payment terms read from it, not constants)
- [ ] Stripe webhook coverage handles disputes, connected-account status changes, and payout/transfer failures
- [ ] Venue/artist verification enforced before an opportunity can be published or a settlement can run
- [ ] Pre-launch dataset cleared / fresh seeded

### Compliance UI/UX
- [x] Browser storage inventory complete; unnecessary storage removed; necessary Auth/Stripe storage documented; any retained consent UI gates real optional technology (otherwise removed) — shipped in #482 (`storageManifest.ts` + `BROWSER_STORAGE.md`; sidebar cookie removed; Stripe/Maps load-on-use; banner retained and wired to gate via `consentGate.ts`)
- [ ] Privacy + T&Cs pages accessible from every footer
- [ ] Pricing transparency on all four payer journeys (gross, platform fee and total shown before commitment)
- [ ] Venue legal details on booking confirmation emails + invoices `launch/venue-legal-on-emails`
- [ ] Online Safety Act report-content button + email destination live `launch/osa-report-content` — **live:** in-app report button on inbound messages, structured safety-inbox email, persisted report record, reporter acknowledgement, admin hide/restore/resolve. **Outstanding:** the always-available published `report@`/`safety@` address on the footer legal pages, which depends on the solicitor-gated Privacy/T&Cs page routes above
- [x] Music licence attestation captured in Org setup form
- [ ] GDPR erasure + data export routes live, with the HMRC-retention split documented `launch/gdpr-subject-rights`
- [ ] Admin console reachable in production, with a real admin provisioning path (see "Build — MVP blockers")

### Operational
- [ ] support@ inbox monitored; SLA documented (target: first response within 1 working day)
- [ ] Status page live
- [ ] Database backups verified
- [x] Rate limiting active on auth, apply, messaging and upload endpoints
- [ ] Incident response process documented
- [ ] First 10 beta venues + 50 beta artists onboarded
- [ ] Marketing site live with pricing page

### Not required at launch
- Native mobile apps
- Multi-currency support
- Customer marketplace switch-on
- DAC7 export script *run* (first run isn't due until Jan 2028)
- Org-switcher / multi-org UX
- More granular membership roles
- B2B venue↔artist reviews / reputation
- Calendar sync (Google/Apple/Outlook)
- Financial/settlement CSV export
- Email notification preferences / unsubscribe (while all outbound mail stays transactional)
- Admin action audit log

## 8. Marketplace add-on (post-launch)

The marketplace is **deliberately additive** — designed so it can be switched on later without major refactor of the B2B code paths.

See [MARKETPLACE_PLAN.md](../marketplace/MARKETPLACE_PLAN.md) for the detail. Headline:
- Most of the marketplace infrastructure already exists (Customer SPA, Customer module, TicketEntity, ConcertEntity price/capacity fields).
- Switch-on is primarily UI work (pricing transparency, refund UI, consumer-facing emails) + consumer-protection legal (separate customer T&Cs from solicitor + CMA secondary-ticketing review).
- The B2B tenancy refactor doesn't change; settlement workflows don't change; Stripe Connect doesn't change.
- Estimated effort when the time comes: ~2-3 calendar months.

**Earliest realistic marketplace switch-on:** Q1 2027 (3 months after B2B launch). Push later if B2B traction needs all the focus.

## 8b. Repo topology — stay monorepo until the polyrepo trigger fires (post-launch)

We stay on the **monorepo** through launch and well beyond. What we have is already a *monorepo of
independently-deployable services*, not a lazy monolith: package-clean service closures off the org
feed, `EnforceServiceBoundary` + `carve-*` CI gates, standalone AppHosts, and six read-only mirror
repos that already clone-and-build (see [../POLYREPO.md](../platform/POLYREPO.md) and
[../../api/ARCHITECTURE.md](../../api/ARCHITECTURE.md)). The split to a **true polyrepo** (mirrors
become the writable dev repos) is a **one-way door** — bias to cutting *late*, not on time.

The trigger is the **AND** of two conditions; the first alone is necessary but **not** sufficient:

1. **Cost gone (codebase milestone).** Post-launch, cross-service contracts frozen, `Shared`/`Kernel`
   core stopped churning, and the **cross-boundary commit rate down to ~single digits** (measured
   **~47%** pre-launch — half of `api/` commits touch >1 service; ~16% touch `Shared`). Above ~20% the
   publish→platform-sync→migrate tax bleeds you, and atomic cross-service changes stop being possible.
2. **A distinct owner (team milestone).** A second engineer owns a service end-to-end and is genuinely
   blocked by the shared merge stream or needs a hard source boundary. Polyrepo's payoff is *purely
   organizational* — with one person (+ agents) the monorepo is strictly better (atomic cross-service
   changes, trivial cross-service E2E, zero cross-repo tax), no matter how stable or large the code gets.

Stable-steady-state feature delivery makes a split *cheap and safe*; it does not make it *worth it*.
Worth-it waits on the second owner. Parallel-agent isolation is **not** a reason to split — worktrees
already give per-service isolation inside the monorepo, and a cross-cutting refactor is *harder* across
repos (N coordinated PRs + a sync), not easier.

When both fire, execute **service-by-service, cheapest-first** — `Auth`/`Payment` (stable adapters,
low co-change) before the churny `B2B`/`Customer`/`Shared` core — never big-bang. The mirrors already
build standalone, so each flip is mechanical (make the mirror writable, redirect dev, give it its own
copy of the workflows).

## 9. Decision points still open

The DoorSplit/Versus revenue source and revenue model are now locked in the decision log below. The
settlement-dispute question below is a **product + legal** decision on the critical path; the two after
it are operational choices that are not urgent yet.

- **Venue↔artist settlement disputes** — an artist says they played, the venue says they no-showed; on
  DoorSplit/Versus the venue self-reports the door take and the artist has no way to contest the number.
  Escrow only releases or refunds — there is **no contested path**, and this sits directly on the
  differentiating contract types. Needs both a product decision (do we mediate, or is it strictly
  between the parties with Concertable as disclosed agent?) and a solicitor-drafted dispute/mediation
  clause; raise it alongside the illegal-content enforcement clause in §5. Decide by Month 5.
- **Beta cohort sourcing** — warm intros via existing music industry contacts? Cold outreach? Industry events? Decide by Month 4.
- **Support tooling** — shared inbox (Front, Helpscout) or just Gmail? Discord/Slack/WhatsApp for beta? Decide by Month 5.

## 10. Reference

- [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md) — full legal/business setup checklist
- [USER_MODEL_PLAN.md](../b2b/USER_MODEL_PLAN.md) — Swim-lane B detail: the outstanding multi-user tenant / roles / auth-sweep work
- [MARKETPLACE_PLAN.md](../marketplace/MARKETPLACE_PLAN.md) — Phase 2 marketplace switch-on plan
- [../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md) — B2B legal backlog (rewritten 2026-06-01: contract-type-centric, items 0-9, PRS corrected)
- [../../api/Concertable.Customer/LEGAL_REQUIREMENTS.md](../../api/Concertable.Customer/LEGAL_REQUIREMENTS.md) — marketplace/fan legal leads (future, separate system)
- [../../api/Concertable.B2B/src/Modules/Deal/ARCHITECTURE.md](../../api/Concertable.B2B/src/Modules/Deal/ARCHITECTURE.md) — deal + workflow architecture
- [CONVENTIONS.md](../../api/agents/MODULE_STRUCTURE.md) — module boundary rules

## Decisions locked

The settled calls that constrain the work above. Full rationale + dated history are in git
(`git log -p plans/launch/LAUNCH_ROADMAP.md`) — not duplicated here.

- **B2B-first.** The customer ticket marketplace is deferred and additive (§8), not a v1 dependency.
- **All four contract types ship in v1.** DoorSplit/Versus settle via **manual door-take entry +
  charge-the-venue** — external-ticketer import ruled out; own checkout is the deferred durable feed. §9
- **Revenue model: one percentage of final deal gross.** B2B owns four deal-specific gross
  calculations; Payment owns the universal rate, binds it when the payer commits, charges commission
  on top of gross and records the retained amount in the ledger. No fixed/minimum/cap model remains
  after the launch cut-over. [PLATFORM_COMMISSION_PLAN.md](PLATFORM_COMMISSION_PLAN.md)
- **Monetization principle:** the fee always rides the settlement transaction routed through our
  Stripe Connect — never invoice-only (else there's no transaction to take a cut from). §9
- **Backend domain type is `Tenant`** (Guid PK, request-scoped filtering, compliance value object);
  **"Organization" is the user-facing UI/API label only.** Multi-user membership/roles/auth-sweep
  are the outstanding Swim-lane B work — see [USER_MODEL_PLAN.md](../b2b/USER_MODEL_PLAN.md).
