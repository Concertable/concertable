# Concertable B2B Launch Checklist

> **Disclaimer:** Research-grounded working doc, not legal advice. Items marked **[LEGAL]** require validation by a UK solicitor before you rely on them. Items marked **[ACCT]** want an accountant's eye.

**Scope:** Everything needed to legally operate Concertable as a UK-based B2B SaaS platform connecting venues and artists for paid gigs, with a disclosed-agent posture for money movement via Stripe Connect Standard.

**Out of scope (Phase 2):** Customer-facing ticket sales / marketplace. Mobile app distribution. International expansion.

---

## Phase 0 — Decisions that block other work

- [ ] **Revenue model** picked: per-gig fee / subscription / % commission / hybrid. Drives Stripe billing setup, pricing UI, and venue onboarding copy.
- [ ] **Multi-tenant org model** decision. Recommended *yes* — see future `TENANT_REFACTOR_PLAN.md`. Block company setup on this only if it changes the company name or domain.
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
- [ ] **[LEGAL]** Cookie policy drafted (often combined with privacy policy).
- [ ] Cookie consent banner deployed on all three SPAs (customer/venue/artist) — CookieYes, Osano, or hand-rolled.
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
- [ ] Application fees configured if taking % cut.
- [ ] Production Stripe account activated (Stripe reviews business; takes a few days).
- [ ] Webhooks live + endpoint health-checked.
- [ ] Test mode → live mode migration plan documented.

**Critical:** funds landing on Concertable's Stripe balance via `OnBehalfOf` is normal Express marketplace mechanics — but the duration matters. Brief escrow (days) is fine; weeks-long balance accumulation looks like a Payment Institution operation. Verify with your solicitor that the disclosed-agent T&Cs are drafted for Express semantics, not Standard.

---

## Phase 8 — Online Safety Act compliance

**Owner: you. Total cost: time only. Total elapsed: 1 day.**

Concertable has user-to-user messaging (artist↔venue). OSA 2023 applies.

- [ ] Risk assessment documented (B2B-only messaging = low risk, but document it).
- [ ] Illegal-content reporting route in app (button or email).
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

## Workflow-specific code changes (separate from this checklist)

These live in `api/Modules/Contract/LEGAL_REQUIREMENTS.md` and should be reconciled with research findings:

- [ ] Strip the 3% PRS deduction from settlement logic (PRS is the venue's responsibility via TheMusicLicence, not the platform's).
- [ ] Add venue field: `holdsMusicLicence: bool` (self-attestation at onboarding).
- [ ] Confirm `Cancelled` stage refund mechanic is wired end-to-end.

---

## References

- ICO registration: https://ico.org.uk
- Companies House: https://find-and-update.company-information.service.gov.uk
- HMRC DAC7 / Reporting rules for digital platforms: https://gov.uk/guidance/reporting-rules-for-digital-platforms
- Stripe Connect docs: https://stripe.com/docs/connect
- Online Safety Act / Ofcom: https://ofcom.org.uk/online-safety
- ICO data protection fee: https://ico.org.uk/for-organisations/data-protection-fee/
