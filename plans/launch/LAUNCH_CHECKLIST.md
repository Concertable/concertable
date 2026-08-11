# Concertable B2B Launch Checklist

> **Disclaimer:** Research-grounded working doc, not legal advice. Items marked **[LEGAL]** require validation by a UK solicitor before you rely on them. Items marked **[ACCT]** want an accountant's eye.

**Scope:** Everything needed to legally operate Concertable as a UK-based B2B SaaS platform connecting venues and artists for paid gigs, with a disclosed-agent posture for money movement via Stripe Connect Express.

**Out of scope (Phase 2):** Customer-facing ticket sales / marketplace. Mobile app distribution. International expansion.

---

## Launch triage — critical path, ownership & what to start now

> Added 2026-08-11 from a full reconciliation of this checklist + [`LAUNCH_ROADMAP.md`](LAUNCH_ROADMAP.md)
> against real git/PR/worktree state. This is the "what actually gates the launch" view: who owns each
> strand, the elapsed-time long poles, and the order to start them. Detailed **code** status lives in the
> roadmap; this section is the cross-cutting sequencing so the work can be picked up later.

### The 4 long poles (everything else fits around these)

| Long pole | Owner | Lead time | Unblocks |
|---|---|---|---|
| **Company registration** (name + domain → Companies House + PSC + bank + Corp Tax) | You | ~1 week | Stripe, business bank, insurance, HMRC — all need the legal entity to exist first |
| **Solicitor engagement** (T&Cs cluster) | You + solicitor | **2–4 weeks** | Platform/venue/artist terms, privacy + cookie policy, refund/cancellation matrix, OSA sign-off, T&Cs page routes — one engagement clears the whole legal cluster |
| **Stripe production approval** | You (needs entity) | 1–2 weeks | Hard launch gate — no real money without it |
| **Production deployment + config/secrets** (🔴) | Dev (code) | **Weeks** | The app has no prod existence today; gates webhooks, prod Stripe config, status page, DB backups, running anywhere but localhost. Plan: [`../platform/CONFIG_AND_DEPLOYMENT_PLAN.md`](../platform/CONFIG_AND_DEPLOYMENT_PLAN.md) |

The first three are **yours and calendar-bound** — no code substitutes for them, and if they're not
started they *are* the slip. The fourth is the one big **code** long pole and is independent of the
external clocks, so it parallelises.

### Start these now (you) — ordered by lead time / unblocking power

1. **Pay the ICO fee** (~10 min, £40–60) — Phase 2.
2. **Confirm company name + register the domain** — Phase 0. Blocks everything downstream.
3. **Register the Ltd** (Companies House + PSC + business bank + Corp Tax) — Phase 1.
4. **Engage a marketplace/fintech solicitor** — Phase 3. The single biggest unblocker; start the 2–4-week clock early.
5. **Submit the Stripe production application** once the entity exists — Phase 7.
6. In parallel, the lower-urgency external clocks: **insurance broker** (Phase 4), **accountant**
   (Phase 5), **HMRC platform-operator registration** (Phase 6), **email-on-domain + `support@`**
   (Phase 9), **beta-cohort outreach** (Phase 10 — start early, warm intros).

### Ownership legend for the phases below

- **You — external clock:** Phases 0, 1, 3 (via solicitor), 4, 5, 6 (registration), 7 (Stripe activation), 9 (email/support), 10.
- **You own, a dev can draft the doc now (no external gate):** lawful-basis matrix, retention schedule,
  DSAR process, breach-notification process (Phase 2); test→live migration plan (Phase 7);
  incident-response process (Phase 9); first-bookings playbook (Phase 10). **OSA pack already drafted →**
  [`OSA_COMPLIANCE.md`](OSA_COMPLIANCE.md).
- **Code — status (detail in [`LAUNCH_ROADMAP.md`](LAUNCH_ROADMAP.md)):**
  - **Done:** DAC7 onboarding + payout gate · music-licence attestation · cancellation/escrow refund ·
    per-contract VAT + self-billed invoices · booking agreement + e-sign · DoorSplit/Versus door-take ·
    commission Phase 1 · Stripe Express + test-mode fee.
  - **In-flight / blocked:** browser-storage consent (PR #482 — engineering done + green, stuck at merge
    on the review-gate hook) · commission cut-over + payer pricing disclosure (PR #296 — held on the
    Kernel error-convention) · manager-dashboard money slices (cross-service Payment work).
  - **Ready + unowned, small:** DAC7 export script (low urgency — first run 31 Jan 2028) · DAC7 seller
    notification email · OSA report button · marketing site + pricing page.
  - **Big code long pole:** production deployment + config/secrets (🔴, above).

---

## Phase 0 — Decisions that block other work

- [x] **Revenue model** picked: one Payment-owned percentage of the final deal gross calculated by B2B. Payment charges gross + commission and pays the counterparty gross; all four deal types use the same rate. The shipped £10 fee is temporary and must be removed before launch.
- [x] **Multi-tenant model** decided & shipped — backend domain type is `Tenant` (Guid PK, request-scoped filtering, tenant-scoping E2E green). "Organization" is retained only as the user-facing UI/API label. Remaining multi-user membership / roles / auth-sweep work is tracked in `USER_MODEL_PLAN.md`.
- [ ] **Company name** confirmed available at https://find-and-update.company-information.service.gov.uk and as a `.com` / `.co.uk` domain.
- [ ] **Domain** registered.

---

## Phase 1 — Company setup

**Owner: you. Total cost: ~£100-150. Total elapsed: ~1 week.**

- [ ] Register limited company at Companies House (£12 online, ~24hr). SIC code candidates: `62012` (business and domestic software development) or `90020` (support activities to performing arts — what GigPig uses).
- [ ] PSC register filed at incorporation (declare yourself as Person with Significant Control if >25% ownership).
- [ ] Registered office address set up (home address OR registered-office service such as Hoxton Mix / 1st Formations, ~£40/yr).
- [ ] Business bank account opened: Tide / Starling / Monzo Business (free–£5/mo, 1-3 days).
- [ ] Corporation Tax registered with HMRC (auto-triggered post-incorporation via the Companies House → HMRC handover; complete the online form within 3 months of starting to trade).
- [ ] Companies House WebFiling account created for annual filings.
- [ ] Annual confirmation statement reminder set (£34/yr filing fee, due on incorporation anniversary).

---

## Phase 2 — Data protection (UK GDPR)

**Owner: you (+ solicitor for policies). Total cost: ~£500. Total elapsed: ~2 weeks.**

- [ ] ICO data protection fee paid (£40-60/yr depending on size, ~10 min online at https://ico.org.uk/for-organisations/data-protection-fee/).
- [ ] **[LEGAL]** Privacy policy drafted (solicitor draft OR template + solicitor review).
- [ ] **[LEGAL]** Cookie/storage policy drafted from the verified production inventory (often combined with privacy policy).
- [x] **[CODE]** Browser storage audited across anonymous, authenticated, and Stripe-checkout journeys; unnecessary storage removed and each retained first- or third-party technology classified by purpose, owner, duration, and consent requirement. _(#482: audit + drift-guarded `storageManifest.ts` + `BROWSER_STORAGE.md`. Authenticated/Stripe-checkout journeys documented-not-observed by agreed scope — classified by purpose from library behaviour.)_
- [ ] **[CODE] [LEGAL]** Consent UI retained or introduced only for actual non-exempt optional technology, and it must gate that technology's loading; remove the current generic consent machinery if the audit finds no such technology. _**[CODE] shipped** (#482): banner retained (analytics/marketing roadmapped) and its toggles now gate loading via `consentGate.ts`; Stripe + Maps made load-on-use. **[LEGAL] pending**: solicitor call on whether Maps needs a `functional` consent category, and final policy copy._
- [ ] Lawful basis matrix documented per data category (internal doc).
- [ ] Data retention schedule documented (internal doc).
- [ ] DSAR (Data Subject Access Request) process documented (how requests come in, who handles, SLA).
- [ ] Breach notification process documented (72hr to ICO, content of notification, who decides).
- [ ] Stripe DPA signed (template in Stripe dashboard).
- [ ] DPA template prepared for venues/artists when they request one.

---

## Phase 3 — Terms & conditions

**Owner: solicitor. Total cost: £2-5k one-time. Total elapsed: 2-4 weeks.**

All [LEGAL]. Find a solicitor with marketplace/fintech experience.

- [ ] **Platform Terms of Service** — Concertable Ltd ↔ user. Acceptable use, account termination, IP, limits of liability.
- [ ] **Venue Seller Terms** — disclosed-agent posture. Venue is merchant of record. Venue declares VAT. Venue holds music licence. Venue indemnifies platform.
- [ ] **Artist Seller Terms** — disclosed-agent posture where applicable. Artist tax responsibilities.
- [ ] **Cancellation & Refund Policy** — codified "who eats the loss" matrix:
  - Venue cancels >X days before: full refund to artist? compensation?
  - Venue cancels <X days before: penalty?
  - Artist cancels: penalty? blacklist?
  - Force majeure: refunds without penalty
  - Platform fault: refund + compensation
- [ ] **DPA template** for venues/artists to sign with you.
- [ ] **Acceptable Use Policy**.

---

## Phase 4 — Insurance

**Owner: you (via broker — try Hiscox, Superscript, Markel). Total cost: ~£1-3k/yr. Total elapsed: 1 week.**

Not all legally required but Stripe + most enterprise customers will ask for proof.

- [ ] Professional Indemnity Insurance (~£1m cover, ~£500-1500/yr).
- [ ] Cyber Liability Insurance (~£500-1500/yr).
- [ ] Public Liability (cheap, often bundled).
- [ ] D&O insurance — defer until multiple directors or external investment.

---

## Phase 5 — Tax & accounting

**Owner: accountant. Total cost: ~£100-200/mo. Total elapsed: ongoing.**

- [ ] **[ACCT]** Engage accountant OR set up FreeAgent/Xero + bookkeeper.
- [ ] **[ACCT]** Making Tax Digital (MTD) setup for VAT (when applicable).
- [ ] **[ACCT]** Corporation Tax payment schedule confirmed (due 9 months 1 day after accounting period end).
- [ ] **[ACCT]** [If employing] PAYE registration with HMRC.
- [ ] **[ACCT]** VAT registration when turnover approaches £90k/yr (2024-25 threshold); voluntary registration earlier if you want to reclaim input VAT.
- [ ] Annual accounts filing reminder set (Companies House, due 9 months after accounting period end for small co; free).

---

## Phase 6 — HMRC platform reporting (DAC7)

**Owner: you + dev. Total cost: ~1-2 dev days for the export script. Total elapsed: 1 week.**

- [ ] Register as a Reportable Platform Operator with HMRC (online form at https://gov.uk/guidance/reporting-rules-for-digital-platforms).
- [ ] **[CODE]** DAC7 onboarding fields added to venue + artist onboarding:
  - For UK sole traders: NINO + UTR
  - For UK Ltd companies: Company Registration Number + UTR
  - Legal/business name (exact, as registered)
  - Registered/principal address
  - Bank sort code + account number
  - Tax residence country (default UK)
- [ ] **[CODE]** Validation: account cannot receive payout until DAC7 fields complete.
- [ ] **[CODE]** DAC7 annual export script — generates XML in HMRC schema, scoped to calendar year, due 31 January for prior year. Defer until needed (if launching 2026, first export due 31 Jan 2028).
- [ ] **[CODE]** Seller notification email: each seller receives a copy of data reported about them annually before submission.

**Penalty schedule:** up to £5,000 initial + £600/day late + £100 per inaccurate seller record.

---

## Phase 7 — Stripe Connect production

**Owner: you + dev. Total cost: £0. Total elapsed: 1-2 weeks (Stripe approval).**

Codebase audit confirmed: connected accounts created with `Type = "express"` in `StripeAccountClient.cs`. Money flow uses two patterns: `TransferData.Destination` (direct, automatic transfer to connected account) for non-escrow contracts, and `OnBehalfOf` (charge lands briefly on platform balance, transferred on settle) for FlatFee / VenueHire escrow holds.

- [ ] Stripe Connect **Express** mode in use (NOT Custom). Express keeps Stripe as the regulated payment institution — your platform is still in a marketplace-facilitator posture, not a payment institution itself.
- [ ] `OnBehalfOf` escrow holds released within **short windows** (target: ≤7 days post-event). Funds sitting on platform balance for weeks invites FCA scrutiny.
- [x] Platform-fee money movement implemented in test mode: Payment charges gross plus a retained amount, transfers/releases gross, and records the retained amount in the ledger. The currently configured £10 amount is temporary and is not launch pricing.
- [x] Payment percentage expansion implemented and verified: persist each immutable percentage revision once, create payer commitment bindings, expose binding-aware money RPCs, and retain actual transaction/refund/tax/ledger facts.
- [ ] Publish and deploy the binding-owned deferred calculation surface, cut B2B over to the percentage binding package, then remove the temporary £10 seam after all consumers migrate.
- [ ] B2B payer surfaces disclose the deal formula or exact gross, percentage commission and total before commitment for FlatFee, VenueHire, DoorSplit and Guarantee Plus (`Versus`).
- [ ] Production's percentage configuration ID/version, rate and GBP currency are explicitly configured; immutable-revision bootstrap and fail-closed validation are confirmed in the live environment.
- [ ] Production Stripe account activated (Stripe reviews business; takes a few days).
- [ ] Webhooks live + endpoint health-checked.
- [ ] Test mode → live mode migration plan documented.

**Critical:** funds landing on Concertable's Stripe balance via `OnBehalfOf` is normal Express marketplace mechanics — but the duration matters. Brief escrow (days) is fine; weeks-long balance accumulation looks like a Payment Institution operation. Verify with your solicitor that the disclosed-agent T&Cs are drafted for Express semantics, not Standard.

---

## Phase 8 — Online Safety Act compliance

**Owner: you. Total cost: time only. Total elapsed: 1 day.**

Concertable has user-to-user messaging (artist↔venue). OSA 2023 applies. Draft compliance pack (risk
assessment, reporting route, takedown SLA, complaints/appeals, children's-access assessment):
[`OSA_COMPLIANCE.md`](OSA_COMPLIANCE.md) — **[LEGAL]** solicitor to validate.

- [ ] Illegal-content risk assessment documented (B2B-only 1:1 messaging = low risk, but document it).
- [ ] Children's-access assessment documented (draft: not likely accessed by children).
- [ ] Illegal-content reporting route in app (report button + published email).
- [ ] Illegal-content takedown SLA documented (internal).
- [ ] Complaints / appeals process documented.

Reference: https://ofcom.org.uk/online-safety

---

## Phase 9 — Operations

**Owner: you. Mixed costs.**

- [ ] Email on domain (Google Workspace £5.50/mo per user, or Fastmail).
- [ ] Support inbox monitored (`support@`).
- [ ] Status page (StatusPage.io free tier, or BetterStack, or hand-rolled).
- [ ] Database backups verified for production.
- [ ] Incident response process documented (who you call when prod is down, SLA to customers).

---

## Phase 10 — Pre-launch business

**Owner: you.**

- [ ] Marketing site live (`app/web/business` SPA).
- [ ] Beta cohort hand-recruited: ~10 venues + ~50 artists. Hand-curated, not open signups.
- [ ] Support channel for beta users (shared Slack / Discord / WhatsApp).
- [ ] First-bookings playbook — expect to white-glove the first dozen bookings.
- [ ] Pricing page live with revenue model.

---

## Deferred — Phase 2 (customer-facing marketplace)

Don't tackle until B2B has traction.

- Pricing transparency UI in customer checkout (CMA enforcement).
- Venue legal details on ticket emails (not Concertable's).
- CMA secondary-ticketing compliance review.
- Consumer cancellation rights flow.
- Refund processing UI.
- ADR (Alternative Dispute Resolution) provider engagement.
- Accessibility (Equality Act 2010 / WCAG 2.1 AA) audit.

---

## Workflow-specific code changes (tracked elsewhere)

Code-level workflow/legal items are owned by `api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md` — the single source; don't duplicate them here. Current state at a glance:

- ✅ 3% PRS deduction — correctly absent: PRS is the venue's liability via TheMusicLicence (not the platform's), and a flat skim would double-charge an already-licensed venue. (A proper per-tenant pass-through for *non*-self-licensed venues is a separate, still-open item — `LEGAL_REQUIREMENTS.md` item 5, marked ABSENT.)
- [x] ✅ `holdsMusicLicence` self-attestation — **shipped** on `Tenant.Compliance` (record-only bool; `Feature/launch_music-licence-attestation`).
- [x] ✅ `Cancelled`-stage escrow refund — **shipped**: concert-cancel path (PR #76) + application cancellation (`Feature/ApplicationCancel`) both unwind escrow across all four contract types.

---

## References

- ICO registration: https://ico.org.uk
- Companies House: https://find-and-update.company-information.service.gov.uk
- HMRC DAC7 / Reporting rules for digital platforms: https://gov.uk/guidance/reporting-rules-for-digital-platforms
- Stripe Connect docs: https://stripe.com/docs/connect
- Online Safety Act / Ofcom: https://ofcom.org.uk/online-safety
- ICO data protection fee: https://ico.org.uk/for-organisations/data-protection-fee/
