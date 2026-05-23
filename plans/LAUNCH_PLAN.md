# Concertable Launch Plan

> **Goal:** Production launch of the B2B platform (venue↔artist booking + automated settlement) by **November 2026**.
>
> **Updated:** 2026-05-18
>
> **Companion docs:** [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md), [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md), [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md), [api/Modules/Contract/LEGAL_REQUIREMENTS.md](api/Modules/Contract/LEGAL_REQUIREMENTS.md).

---

## 1. Vision and scope

**In scope for the v1 launch:**
- B2B SaaS marketplace for venue↔artist bookings
- Four contract types (FlatFee, DoorSplit, VenueHire, Versus)
- Automated settlement via Stripe Connect Express
- Disclosed-agent legal posture (Concertable acts as venue/artist's agent for money handling)
- Multi-staff Organization model (Owner + Manager roles)
- DAC7-compliant seller onboarding
- Cancellation/refund handling on the B2B path (venue or artist cancels — escrow refunds correctly)

**Out of scope for v1 (planned, not abandoned):**
- Customer-facing ticket marketplace — see [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md). Designed to be additive; switch-on planned Q1 2027 or later once B2B has traction.
- Mobile app distribution to App Store / Play Store
- Native push notifications
- Multi-currency / international expansion
- More granular membership roles beyond Owner/Manager
- Org-switcher UI (one user managing multiple orgs)

**The differentiation thesis:** unlike GigPig (no escrow, no contracts, no settlement enforcement), Concertable provides typed contracts and automated settlement. Unlike DICE (closed, ticketing-first), Concertable also handles the venue↔artist booking workflow.

## 2. Three parallel swim-lanes

Three workstreams run in parallel across the six months. Each has different owners and dependencies.

### Swim-lane A — Legal & Business
**Owner:** you (with solicitor + accountant)
**Detail:** [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md)

Company registration, ICO, T&Cs, insurance, accounting, HMRC platform-operator registration, Stripe production activation. Mostly admin work scattered across the six months; some elapsed-time dependencies (solicitor drafting takes 2-4 weeks).

### Swim-lane B — Architecture
**Owner:** you (or contractor dev)
**Detail:** [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md)

The Organization refactor — the load-bearing structural change that everything else attaches to. Six sequenced phases, ~15-24 working days end-to-end. Includes the multi-user membership table from day one (Phase 6) so no second auth sweep is ever needed.

### Swim-lane C — Compliance UI/UX + workflow polish
**Owner:** you (or contractor dev)
**Detail:** §5 of this plan

The smaller code items that don't fit in either of the other swim-lanes: cookie banner, pricing transparency, refund/cancellation codification, DAC7 export script, legal-page routes, OSA report-content flow, etc. Some items block on legal text (T&Cs) being drafted first; others can run earlier.

## 3. 6-month timeline

Calendar-realistic, not optimistic. Slips are flagged as risks (§6).

| Month | Swim-lane A (Legal/Business) | Swim-lane B (Architecture) | Swim-lane C (Compliance UI/UX) |
|---|---|---|---|
| **Month 1 (Jun 2026)** | Company registered (Companies House, ~£12, 24hr) · ICO fee paid (~£40-60/yr) · Solicitor engaged + briefed for T&Cs · **Revenue model decided** | **Phase 0** — `Organization` module scaffolding · **Phase 1** — `ComplianceContext` value object | **PRS line removal** from `LEGAL_REQUIREMENTS.md` (small) · **Music licence attestation field** spec (will be wired in Phase 1) |
| **Month 2 (Jul 2026)** | Business bank account opened · Accountant engaged · Solicitor drafts circulating | **Phase 2** — Venue/Artist wired to Organization | **Cookie banner** scaffolding on all 3 SPAs (text from solicitor still pending) |
| **Month 3 (Aug 2026)** | Insurance arranged (Professional Indemnity + Cyber) · Stripe production application submitted | **Phase 3** — `PayoutAccountEntity` re-key to OrganizationId | **Pricing transparency UI** in checkout (now that revenue model is known) |
| **Month 4 (Sep 2026)** | Solicitor T&Cs finalised · DPA signed with Stripe · ICO documentation (privacy policy, lawful basis, retention) | **Phase 4** — `ComplianceContext` snapshot on Booking · **Phase 5** — Organization setup UI | **Privacy + T&Cs page routes** wired up (solicitor text now in hand) · **Venue legal details on emails** template change |
| **Month 5 (Oct 2026)** | HMRC platform-operator registration · Stripe production approved · Marketing site live | **Phase 6** — Multi-user membership + auth sweep | **Refund / cancellation codification** in `Cancelled` workflow · **OSA report-content flow** (button + email + policy doc) · **DAC7 export script** (defer the actual run until Jan 2028) |
| **Month 6 (Nov 2026)** | Beta cohort recruited (~10 venues + 50 artists) · Support process live · Pricing page live | Bugfixes from beta feedback · final integration tests | Final polish · accessibility quick-pass · **LAUNCH** |

## 4. Critical path

Dependencies that constrain the order:

```
Revenue model decision (Month 1)
    └─→ Pricing transparency UI (Month 3)
    └─→ Solicitor T&Cs drafting (Month 1-4)
            └─→ Privacy + T&Cs page routes (Month 4)
            └─→ Cookie banner final text (Month 4)
            └─→ Refund / cancellation codification (Month 5)

Phase 0 — Org scaffolding (Month 1)
    └─→ Phase 1 — Compliance value object (Month 1-2)
            └─→ Phase 2 — Venue/Artist FK (Month 2)
                    └─→ Phase 3 — Stripe re-key (Month 3)
                            └─→ Phase 4 — Booking snapshot (Month 4)
                                    └─→ Phase 5 — Setup UI (Month 4)
                                            └─→ Phase 6 — Membership refactor (Month 5)
                                                    └─→ Beta + launch (Month 6)

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
| PRS 3% line removal from `LEGAL_REQUIREMENTS.md` + code grep | 0.5 days | – | Month 1 |
| Music licence attestation field (on Organization) | 0.5 days | Phase 1 | Month 1 |
| Cookie consent banner on 3 SPAs (scaffolding) | 1-2 days | – (scaffolding can land before solicitor text) | Month 2 |
| Cookie banner text + privacy policy text from solicitor → wired into banner | 0.5 days | Solicitor draft (Month 4) | Month 4 |
| Pricing transparency UI in checkout (all fees pre-checkout) | 1-2 days | Revenue model decision | Month 3 |
| Privacy + T&Cs page routes (footer of every page) | 1 day | Solicitor draft | Month 4 |
| Venue legal details on emails (booking confirmation, invoices) | 1 day | Phase 5 (setup UI captures legal name) | Month 4 |
| Refund / cancellation matrix codification in `Cancelled` workflow | 3-5 days | Cancellation policy text from solicitor | Month 5 |
| Online Safety Act report-content flow (button + email destination + policy doc) | 1 day | – | Month 5 |
| DAC7 annual export script (writes XML in HMRC schema, doesn't run until Jan 2028) | 2-3 days | Phase 6 complete | Month 5 |

**Total Swim-lane C effort:** ~12-19 working days. Roughly 3 calendar weeks of focused work, but spread across the 6 months because of dependency timing.

## 6. Risk register

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Org refactor takes longer than 24 days (EF nested owned-types surprises, migration-script issues) | Medium | High | Phase 0 scaffolding has explicit go/no-go assessment at the end. If it took >3 days, recalibrate timeline before continuing. |
| R2 | Solicitor T&Cs drafting takes longer than 4 weeks | Medium | High | Brief solicitor in Month 1, not Month 3. Keep a parallel "draft v1" using a quality T&Cs template as backup. |
| R3 | Stripe production approval delayed (Stripe asks for more info / rejects) | Medium | High | Submit application Month 3, not Month 5. Have ICO fee + insurance + company info ready as supporting docs. |
| R4 | Revenue model decision keeps slipping → blocks pricing UI + solicitor work | Medium | Medium | **Hard deadline: end of Month 1.** Force a decision even if imperfect; revisit at v1.1. |
| R5 | Beta cohort hard to recruit (no organic demand pre-launch) | Medium | Medium | Start recruitment Month 4 not Month 6. Hand-pick first 10 venues + 50 artists via warm intros, not open signups. |
| R6 | Phase 6 auth sweep introduces regressions across 25+ controllers | Medium | Medium | Test coverage assessment in Month 4. If integration test coverage is <60%, write tests first or split Phase 6 into smaller PRs. |
| R7 | DAC7 schema changes between now and first export (Jan 2028) | Low | Low | Defer DAC7 export *implementation* if HMRC publishes schema updates; keep onboarding field collection on-spec. |
| R8 | Solicitor flags an issue we haven't planned for (e.g. requires PSR registration, not just disclosed-agent) | Low | High | First solicitor consultation in Month 1 should explicitly confirm disclosed-agent posture is viable on Stripe Connect Express. If they push back, this plan needs major rework. |

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
- [ ] Organization refactor Phases 0-6 merged and integration-tested
- [ ] All Stripe Connect Express payouts flowing through OrganizationId
- [ ] ComplianceContext snapshot populated on every Booking created post-launch
- [ ] Auth checks routed through `OrganizationMembership` (not legacy TPH FK)
- [ ] Pre-launch dataset cleared / fresh seeded

### Compliance UI/UX
- [ ] Cookie consent banner live on all 3 SPAs
- [ ] Privacy + T&Cs pages accessible from every footer
- [ ] Pricing transparency UI in checkout (fees shown pre-checkout)
- [ ] Venue legal details on booking confirmation emails + invoices
- [ ] Online Safety Act report-content button + email destination live
- [ ] Music licence attestation captured in Org setup form

### Operational
- [ ] support@ inbox monitored; SLA documented (target: first response within 1 working day)
- [ ] Status page live
- [ ] Database backups verified
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

## 8. Marketplace add-on (post-launch)

The marketplace is **deliberately additive** — designed so it can be switched on later without major refactor of the B2B code paths.

See [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md) for the detail. Headline:
- Most of the marketplace infrastructure already exists (Customer SPA, Customer module, TicketEntity, ConcertEntity price/capacity fields).
- Switch-on is primarily UI work (pricing transparency, refund UI, consumer-facing emails) + consumer-protection legal (separate customer T&Cs from solicitor + CMA secondary-ticketing review).
- The B2B Organization refactor doesn't change; settlement workflows don't change; Stripe Connect doesn't change.
- Estimated effort when the time comes: ~2-3 calendar months.

**Earliest realistic marketplace switch-on:** Q1 2027 (3 months after B2B launch). Push later if B2B traction needs all the focus.

## 9. Decision points still open

These need answers but aren't urgent yet:

- **Revenue model** — per-gig fee / subscription / % commission / hybrid. Decide by end of Month 1.
- **Subscription tiers** (if going subscription) — what's free, what's paid, what's the price point? Decide by Month 3 (when pricing page work starts).
- **Beta cohort sourcing** — warm intros via existing music industry contacts? Cold outreach? Industry events? Decide by Month 4.
- **Support tooling** — shared inbox (Front, Helpscout) or just Gmail? Discord/Slack/WhatsApp for beta? Decide by Month 5.

## 10. Reference

- [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md) — full legal/business setup checklist
- [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md) — Swim-lane B detail
- [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md) — Phase 2 marketplace switch-on plan
- [api/Modules/Contract/LEGAL_REQUIREMENTS.md](api/Modules/Contract/LEGAL_REQUIREMENTS.md) — workflow-level legal requirements (currently aspirational; needs PRS line removed)
- [api/Modules/Contract/CONTRACT_ARCHITECTURE.md](api/Modules/Contract/CONTRACT_ARCHITECTURE.md) — contract + workflow architecture
- [MODULAR_MONOLITH_RULES.md](MODULAR_MONOLITH_RULES.md) — module boundary rules

## Decision log

- **2026-05-18** — B2B-first launch, marketplace deferred. Rationale: user wants to focus on the SaaS side that differentiates from GigPig; marketplace switch-on can be additive without major B2B refactor.
- **2026-05-18** — Target launch Nov 2026 (6 months from plan creation). Reviewable monthly.
- **2026-05-18** — Three swim-lane structure adopted. Independent parallel workstreams with clearly defined dependencies; allows the legal slow-path and the code work to proceed concurrently.
- **2026-05-18** — Multi-user membership table in scope from day one (not deferred). Avoids a second auth refactor later.
