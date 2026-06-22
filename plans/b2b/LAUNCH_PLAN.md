# Concertable Launch Plan

> **Goal:** Production launch of the B2B platform (venue↔artist booking + automated settlement) by **November 2026**.
>
> **Updated:** 2026-06-22
>
> **Companion docs:** [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md), [USER_MODEL_PLAN.md](USER_MODEL_PLAN.md), [MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md), [../../api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md).

---

## Status — what's shipped vs. what's left (code-verified 2026-06-22)

**Shipped — verified in code, don't rebuild:** Tenant model + membership + 6-role RBAC · payout re-keyed to `TenantId` · Stripe Connect Express with both money flows (escrow `OnBehalfOf` for FlatFee/VenueHire; `TransferData.Destination` for DoorSplit/Versus) · artist↔venue messaging · settlement for all four contract types · the 3% PRS skim is correctly absent.

**Decide first — gates everything (see §9):**
- [ ] Revenue model (per-gig / subscription / % commission / hybrid) — whatever it is, the fee rides on the settlement transaction (§9 principle).
- [x] DoorSplit/Versus revenue source — **resolved: manual door-takings entry + charge-the-venue** for v1 (external-ticketer import ruled out — §9). All four contract types ship in the pure-B2B MVP, no marketplace dependency. See §9 / R9.

**Build — MVP blockers, in priority order:**
- [ ] 🔴 **Cancellation + escrow refund** — add a `Cancelled` stage and wire `EscrowEntity.Refund()` (the method exists; B2B never calls it). Today escrow money can come in but can't be refunded in-app.
- [ ] 🔴 **E-signed booking agreement** — click-wrap at Accept, terms snapshotted, PDF via `IPdfService` (LEGAL_REQUIREMENTS item 2).
- [ ] 🔴 **DoorSplit/Versus door-take entry at settlement** — venue enters the door take; Concertable charges the venue for the artist's share + fee and pays the artist (reuses FlatFee escrow). Without it the two revenue-share types — the actual moat vs GigPig/GigXchange — can't settle. See §1 / R9.
- [ ] 🟠 **DAC7 onboarding completion** — NINO / UTR / Company-Reg on `Tenant.Compliance`; gate payout until complete (no de-minimis for services — reportable from £1).
- [ ] 🟠 **Self-billed VAT invoice engine** — HMRC legends, per-supplier agreement + annual renewal, VAT-status branching (items 1, 3, 4).
- [ ] 🟡 **`holdsMusicLicence` attestation** on `Tenant.Compliance` (~0.5 day; the venue's responsibility, we just record it).
- [ ] 🟡 **Finish Swim-lane B** — membership/invitation endpoints + auth sweep + messaging group-inbox (USER_MODEL_PLAN Phases 6-8).
- [ ] 🟡 **Per-contract-type VAT calculation** (items 1, 3).

**Verify before trusting — competitor table-stakes, not confirmed in code:** reviews/reputation end-to-end · calendar sync (Google/Apple/Outlook) · financial/settlement CSV export · pricing-transparency UI.

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
- Per-booking signed agreement (click-wrap e-signature, terms snapshotted at Accept) — see [LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md) item 2
- Per-contract-type VAT calculation + VAT-compliant self-billed invoices per settlement (items 1, 3, 4)
- Per-tenant configuration surface (PRS, VAT, platform fee, payment terms, cancellation defaults)

**DoorSplit/Versus revenue source — resolved (2026-06-22):** these two settle against door/ticket revenue, which standalone B2B (no marketplace) has no automatic feed for. The confirmed v1 feed is **manual door-take entry + charge-the-venue**: the venue enters the door take at settlement, Concertable charges the venue for the artist's share (+ our fee) and pays the artist through Stripe Connect — identical to FlatFee escrow. So **all four contract types ship in the pure-B2B MVP** with no marketplace dependency. The own checkout (deferred) only *upgrades* DoorSplit/Versus later (verified number instead of self-reported, no venue credit risk). See §9.

**Out of scope for v1 (planned, not abandoned):**
- Customer-facing ticket marketplace — see [MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md). Designed to be additive; switch-on planned Q1 2027 or later once B2B has traction.
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
**Detail:** [USER_MODEL_PLAN.md](USER_MODEL_PLAN.md)

The tenancy refactor — the load-bearing structural change that everything else attaches to, sequenced as the phases in the timeline below. The tenant-scoping foundation (Tenant module with a Guid PK, request-scoped tenant filtering, the compliance value object) has shipped; the outstanding work — multi-user membership, roles, and the authorization sweep — is tracked in [USER_MODEL_PLAN.md](USER_MODEL_PLAN.md).

### Swim-lane C — Compliance UI/UX + workflow polish
**Owner:** you (or contractor dev)
**Detail:** §5 of this plan

The smaller code items that don't fit in either of the other swim-lanes: cookie banner, pricing transparency, refund/cancellation codification, DAC7 export script, legal-page routes, OSA report-content flow, etc. Some items block on legal text (T&Cs) being drafted first; others can run earlier.

## 3. 6-month timeline

Calendar-realistic, not optimistic. Slips are flagged as risks (§6).

| Month | Swim-lane A (Legal/Business) | Swim-lane B (Architecture) | Swim-lane C (Compliance UI/UX) |
|---|---|---|---|
| **Month 1 (Jun 2026)** | Company registered (Companies House, ~£12, 24hr) · ICO fee paid (~£40-60/yr) · Solicitor engaged + briefed for T&Cs · **Revenue model decided** · **DoorSplit/Versus revenue-source decision** (§9) | **Phase 0** — `Tenant` module scaffolding · **Phase 1** — `ComplianceContext` value object + tenant config surface | **Music licence attestation field** spec (= PRS self-licensed flag; wired in Phase 1) · _(PRS correction in `LEGAL_REQUIREMENTS.md` ✅ done 2026-06-01)_ |
| **Month 2 (Jul 2026)** | Business bank account opened · Accountant engaged · Solicitor drafts circulating | **Phase 2** — Venue/Artist wired to Tenant | **Cookie banner** scaffolding on all 3 SPAs (text from solicitor still pending) |
| **Month 3 (Aug 2026)** | Insurance arranged (Professional Indemnity + Cyber) · Stripe production application submitted | **Phase 3** — `PayoutAccountEntity` re-key to TenantId | **Pricing transparency UI** in checkout (now that revenue model is known) |
| **Month 4 (Sep 2026)** | Solicitor T&Cs finalised · DPA signed with Stripe · ICO documentation (privacy policy, lawful basis, retention) | **Phase 4** — `ComplianceContext` snapshot on Booking · **Phase 5** — Organization setup UI | **Privacy + T&Cs page routes** wired up (solicitor text now in hand) · **Venue legal details on emails** template change · **Booking agreement + click-wrap e-sign** at Accept (PDF via `IPdfService`) |
| **Month 5 (Oct 2026)** | HMRC platform-operator registration · Stripe production approved · Marketing site live | **Phase 6** — Multi-user membership + auth sweep | **Refund / cancellation codification** in `Cancelled` workflow · **Per-contract VAT calculation** + **self-billed invoice generation** (reuses agreement PDF plumbing) · **OSA report-content flow** (button + email + policy doc) · **DAC7 export script** (defer the actual run until Jan 2028) |
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

Phase 0 — Tenant scaffolding (Month 1)
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
| PRS correction in `LEGAL_REQUIREMENTS.md` (✅ done 2026-06-01 — was "remove 3% line"; now per-tenant pass-through, venue's liability) | – | – | done |
| Music licence attestation field (on `Tenant.Compliance`) = PRS self-licensed flag | 0.5 days | Phase 1 | Month 1 |
| Tenant configuration surface (PRS / VAT / platform fee / payment terms / cancellation defaults) | 1-2 days | Phase 1 | Month 1-2 |
| Booking agreement + click-wrap e-signature at Accept (snapshot terms, PDF via `IPdfService`) — `LEGAL_REQUIREMENTS.md` item 2 | 3-5 days | Phase 4 (Booking snapshot), `IPdfService` | Month 4 |
| Per-contract-type VAT calculation (branches on supply direction + supplier VAT status) — items 1, 3 | 2-3 days | Tenant config (VAT fields) | Month 5 |
| Self-billed VAT invoice generation per settlement (sequential numbering, HMRC fields, PDF) — item 4 | 2-3 days | VAT calculation, agreement PDF plumbing | Month 5 |
| Cookie consent banner on 3 SPAs (scaffolding) | 1-2 days | – (scaffolding can land before solicitor text) | Month 2 |
| Cookie banner text + privacy policy text from solicitor → wired into banner | 0.5 days | Solicitor draft (Month 4) | Month 4 |
| Pricing transparency UI in checkout (all fees pre-checkout) | 1-2 days | Revenue model decision | Month 3 |
| Privacy + T&Cs page routes (footer of every page) | 1 day | Solicitor draft | Month 4 |
| Venue legal details on emails (booking confirmation, invoices) | 1 day | Phase 5 (setup UI captures legal name) | Month 4 |
| Refund / cancellation matrix codification in `Cancelled` workflow | 3-5 days | Cancellation policy text from solicitor | Month 5 |
| Online Safety Act report-content flow (button + email destination + policy doc) | 1 day | – | Month 5 |
| DAC7 annual export script (writes XML in HMRC schema, doesn't run until Jan 2028) | 2-3 days | Phase 6 complete | Month 5 |

**Total Swim-lane C effort:** ~20-31 working days (up from ~12-19 after adding the booking-agreement, VAT-calculation, self-billed-invoice, and tenant-config items). Roughly 4-6 calendar weeks of focused work, spread across the 6 months because of dependency timing. The VAT chain (calculation → invoice) is the densest cluster and lands in Month 5 — watch it doesn't collide with the Phase 6 auth sweep (R6).

## 6. Risk register

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Tenancy refactor takes longer than 24 days (EF nested owned-types surprises, migration-script issues) | Medium | High | Phase 0 scaffolding has explicit go/no-go assessment at the end. If it took >3 days, recalibrate timeline before continuing. |
| R2 | Solicitor T&Cs drafting takes longer than 4 weeks | Medium | High | Brief solicitor in Month 1, not Month 3. Keep a parallel "draft v1" using a quality T&Cs template as backup. |
| R3 | Stripe production approval delayed (Stripe asks for more info / rejects) | Medium | High | Submit application Month 3, not Month 5. Have ICO fee + insurance + company info ready as supporting docs. |
| R4 | Revenue model decision keeps slipping → blocks pricing UI + solicitor work | Medium | Medium | **Hard deadline: end of Month 1.** Force a decision even if imperfect; revisit at v1.1. |
| R5 | Beta cohort hard to recruit (no organic demand pre-launch) | Medium | Medium | Start recruitment Month 4 not Month 6. Hand-pick first 10 venues + 50 artists via warm intros, not open signups. |
| R6 | Phase 6 auth sweep introduces regressions across 25+ controllers | Medium | Medium | Test coverage assessment in Month 4. If integration test coverage is <60%, write tests first or split Phase 6 into smaller PRs. |
| R7 | DAC7 schema changes between now and first export (Jan 2028) | Low | Low | Defer DAC7 export *implementation* if HMRC publishes schema updates; keep onboarding field collection on-spec. |
| R8 | Solicitor flags an issue we haven't planned for (e.g. requires PSR registration, not just disclosed-agent) | Low | High | First solicitor consultation in Month 1 should explicitly confirm disclosed-agent posture is viable on Stripe Connect Express. If they push back, this plan needs major rework. |
| R9 | DoorSplit/Versus manual-entry settlement screen slips → two of four contract types unsellable at launch | Low | Medium | **Resolved 2026-06-22 (§9):** manual door-take entry + charge-the-venue feeds DoorSplit/Versus at v1 so all four ship; external-ticketer import ruled out; owned checkout (marketplace) is the deferred durable feed. Residual is only *building* the door-take entry screen — the money mechanic reuses FlatFee escrow. FlatFee + VenueHire remain the standalone floor if that screen slips. |
| R10 | VAT calculation + invoice work (Month 5) collides with Phase 6 auth sweep | Medium | Medium | Both land Month 5. If Phase 6 is running hot, pull the VAT chain forward to Month 4 (it depends only on the tenant VAT fields from Phase 1, not on Phase 6). |

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
- [ ] Booking agreement generated + click-wrap consent recorded at every Accept
- [ ] VAT calculated per contract type + self-billed invoice generated per settlement
- [ ] Tenant config surface live (PRS / VAT / fee / payment terms read from it, not constants)
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

See [MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md) for the detail. Headline:
- Most of the marketplace infrastructure already exists (Customer SPA, Customer module, TicketEntity, ConcertEntity price/capacity fields).
- Switch-on is primarily UI work (pricing transparency, refund UI, consumer-facing emails) + consumer-protection legal (separate customer T&Cs from solicitor + CMA secondary-ticketing review).
- The B2B tenancy refactor doesn't change; settlement workflows don't change; Stripe Connect doesn't change.
- Estimated effort when the time comes: ~2-3 calendar months.

**Earliest realistic marketplace switch-on:** Q1 2027 (3 months after B2B launch). Push later if B2B traction needs all the focus.

## 9. Decision points still open

These need answers but aren't urgent yet. (The **DoorSplit/Versus revenue source** is no longer open — *resolved 2026-06-22*: manual door-take entry + charge-the-venue ships all four contract types; external-ticketer import ruled out; own checkout deferred. Reasoning in §1 + the decision log; build item in the Status block; risk in R9.)

- **Revenue model** — per-gig fee / subscription / % commission / hybrid. Decide by end of Month 1. **Constraint (settled 2026-06-22):** the fee must ride on the *settlement transaction* routed through our Stripe Connect (charge the venue for the artist's share **+ our fee**, then pay the artist) — never fully invoice-only/GigPig-style, which leaves no transaction to take a cut from. Points toward a % cut on settlement. (Corollary — we never force venues onto our checkout; the marketplace is additive — is in §1 and the decision log.)
- **Subscription tiers** (if going subscription) — what's free, what's paid, what's the price point? Decide by Month 3 (when pricing page work starts).
- **Beta cohort sourcing** — warm intros via existing music industry contacts? Cold outreach? Industry events? Decide by Month 4.
- **Support tooling** — shared inbox (Front, Helpscout) or just Gmail? Discord/Slack/WhatsApp for beta? Decide by Month 5.

## 10. Reference

- [LAUNCH_CHECKLIST.md](LAUNCH_CHECKLIST.md) — full legal/business setup checklist
- [USER_MODEL_PLAN.md](USER_MODEL_PLAN.md) — Swim-lane B detail: the outstanding multi-user tenant / roles / auth-sweep work
- [MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md) — Phase 2 marketplace switch-on plan
- [../../api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/Modules/Contract/LEGAL_REQUIREMENTS.md) — B2B legal backlog (rewritten 2026-06-01: contract-type-centric, items 0-9, PRS corrected)
- [../../api/Concertable.Customer/LEGAL_REQUIREMENTS.md](../../api/Concertable.Customer/LEGAL_REQUIREMENTS.md) — marketplace/fan legal leads (future, separate system)
- [../../api/Concertable.B2B/Modules/Contract/ARCHITECTURE.md](../../api/Concertable.B2B/Modules/Contract/ARCHITECTURE.md) — contract + workflow architecture
- [MODULAR_MONOLITH_RULES.md](../../api/docs/MODULAR_MONOLITH_RULES.md) — module boundary rules

## Decision log

- **2026-05-18** — B2B-first launch, marketplace deferred. Rationale: user wants to focus on the SaaS side that differentiates from GigPig; marketplace switch-on can be additive without major B2B refactor.
- **2026-05-18** — Target launch Nov 2026 (6 months from plan creation). Reviewable monthly.
- **2026-05-18** — Three swim-lane structure adopted. Independent parallel workstreams with clearly defined dependencies; allows the legal slow-path and the code work to proceed concurrently.
- **2026-05-18** — Multi-user membership table in scope from day one (not deferred). Avoids a second auth refactor later.
- **2026-06-01** — `LEGAL_REQUIREMENTS.md` rewritten contract-type-centric (items 0-9); PRS corrected (not 3%, not platform's liability — per-tenant pass-through at ~4.2%); fan/marketplace items split into a separate Customer doc.
- **2026-06-01** — Booking agreement + click-wrap e-signature added to v1 scope (legal item 2). It's the backbone for the audit trail, self-billed invoice, and cancellation-terms consent; GigPig/GigXchange market this as "contract signing" and it matters more for us given multi-direction money movement.
- **2026-06-01** — Per-contract-type VAT calculation + self-billed invoicing added to v1 scope (was implied, never sequenced). VAT branches on supply direction (VenueHire reverses it) and supplier registration status.
- **2026-06-01** — Tenant configuration surface adopted: PRS/VAT/fee/payment-terms/cancellation defaults read from per-tenant config, not constants.
- **2026-06-01** — Flagged DoorSplit/Versus revenue-source gap (R9): two of four contract types can't settle without a revenue feed; FlatFee + VenueHire are the standalone-safe v1 floor.
- **2026-06-20** — Organization refactor superseded by the tenancy route. `ORGANIZATION_REFACTOR_PLAN.md`, `TENANCY_DESIGN.md`, and `TENANT_SCOPING_PLAN.md` shipped and were deleted per the plans rule (git history is the archive). The B2B module is now `Tenant` (Guid PK) with request-scoped filtering and a compliance value object; tenant-scoping is complete (full E2E green). **"Organization" is retained ONLY as the user-facing UI/API label** (Settings → Organization, `api/organizations`) — the backend domain type is `Tenant`. Remaining Swim-lane B work (multi-user membership, roles, the auth sweep) moved to `USER_MODEL_PLAN.md`. This plan's Swim-lane B / §7 references were updated to match; the killed `OrganizationMembership` / `OrganizationId` terminology was scrubbed.
- **2026-06-22** — **Ticket distribution researched (two deep-research passes); model decided.** The merchant-of-record incumbents can't support API-syndication (DICE read-only, Skiddle read-only/non-commercial/competitor-barred, Eventbrite create app-gated; all hold the money). **Ticket Tailor** is the one external platform clearing all three bars (public create API, `ORDER`/`ISSUED_TICKET` webhooks, funds to the connected Stripe with no hold) — but funds land in the *organiser's* Stripe, not ours, so it can't give Concertable fund control unless Concertable is the connected account (which is just the own checkout). **Decision:** §9 option (b) external-ticketer import ruled out; DoorSplit/Versus settle via (a) manual entry + charge-the-venue; (c) own checkout is the deferred durable feed; Ticket Tailor is a post-launch data-ingestion option only. Sources: Ticket Tailor / TicketSource / DICE Partners / Skiddle / Weeztix API docs, Eventbrite SEC 10-Q, GigPig FAQs.
- **2026-06-22** — **MVP scope + monetization principle settled.** (1) The v1 MVP is **pure B2B competing head-on with GigPig/GigXchange** on the contract types they structurally can't settle — DoorSplit/Versus. A flat-fee-only MVP wouldn't differentiate, so those two must be sellable at v1; manual door-take entry + charge-the-venue makes all four ship with no marketplace and no ticketing ownership. (2) **Monetization principle:** Concertable's fee rides on the settlement transaction routed through our Stripe Connect, never invoice-only (else there's no transaction to take a cut from). (3) Pure-B2B also keeps consumer-protection / CMA / consumer-fund-holding liability out of the MVP; the marketplace stays deferred as the additive *second* distribution option. Reinforces (does not reverse) the 2026-05-18 B2B-first / marketplace-deferred decision. Updated §1, §9, R9.
