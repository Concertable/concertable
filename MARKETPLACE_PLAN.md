# Marketplace Add-on Plan

> **Status:** Deferred from initial B2B launch (Nov 2026). Earliest realistic switch-on: **Q1 2027**, later if B2B traction needs all the focus.
>
> **Goal:** Turn on customer-facing ticket sales for events booked through Concertable.
>
> **Design principle:** Must be **additive** to B2B — no major refactor of B2B code paths required to switch on.
>
> **Updated:** 2026-05-18
>
> **Companion docs:** [LAUNCH_PLAN.md](LAUNCH_PLAN.md), [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md), [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md).

---

## 1. Why this is a separate plan

The user explicitly chose to focus the v1 launch on the B2B side (venue↔artist booking + automated settlement). The customer-facing marketplace (ticket sales to the general public) is **planned but not part of v1**. It switches on later as an additive capability.

The differentiation thesis remains: unlike GigPig, Concertable wants to be a **fully established system** for ticket sales too — not a "we're not responsible post-settlement" disclaimer model. So when marketplace switches on, it's a real consumer product with proper consumer-protection mechanics, not a hand-off.

## 2. What's already in place

A surprising amount of the marketplace infrastructure exists today. Switch-on is mostly UI + compliance wrapping, not new architecture.

| Component | Status today |
|---|---|
| Customer-facing SPA (`app/web/customer/`) | Exists; currently functional in dev. Can be feature-flagged off at launch. |
| Customer module (`api/Modules/Customer/`) | Exists — owns CustomerEntity + Preferences |
| `ConcertEntity.Price`, `TotalTickets`, `AvailableTickets` | Exist on Concert entity already |
| `TicketEntity` | Exists; ticket-buy flow already wired |
| Stripe Connect Express customer charges | Same setup as B2B side — Stripe direct charges to the venue's connected account work for both B2B and customer flows |
| DoorSplit + Versus contract types | Already calculate artist share from `Concert.DoorRevenue` — settlement workflow handles ticket revenue regardless of whether it came from customer ticket sales or manual entry |

In short, **the wiring exists**. What's missing is the consumer-protection layer that wraps it.

## 3. What's needed to switch on

### Code work (~3-4 weeks of focused effort)

| Item | Why |
|---|---|
| **Pricing transparency UI in customer checkout** | CMA enforcement — all fees (platform fee, payment processing, VAT) must be shown *before* the customer reaches the pay button, not added at the last step. Cannot launch without this. |
| **Refund processing UI for customers** | Customer can request refund; venue or platform approves; refund routes back via Stripe. Bigger than it sounds because of state machine implications. |
| **Customer-facing email templates** | Booking confirmation, ticket delivery, refund confirmation — all must show the **venue's legal entity** as the seller (not Concertable's), per disclosed-agent posture. |
| **Customer refund flow on Cancelled event** | Already partially modelled on B2B side; needs customer-side wiring (notify customers, batch-refund, status updates). |
| **Accessibility audit + WCAG 2.1 AA fixes** | Equality Act 2010. Customer-facing pages need to pass; B2B internal pages are technically also covered but less risky. |
| **Customer-side terms acceptance flow** | Distinct from venue/artist seller terms — these are buyer terms, must accept at first purchase. |
| **Online Safety Act risk assessment extended** | Customer-facing event listings + reviews = more in-scope content than B2B messaging. Risk assessment update + content-moderation route. |
| **PCI compliance pass-through documentation** | Stripe handles PCI but you should document the pass-through for due diligence requests. |

### Legal work (~4-8 weeks elapsed)

| Item | Why |
|---|---|
| **Customer T&Cs (solicitor)** | Separate from venue/artist seller terms. Consumer-protection language, refund rights, dispute resolution. |
| **CMA secondary-ticketing compliance review** | Even if you prohibit resale (recommended for v1), the prohibition itself must be enforceable + documented. CMA can audit. |
| **Consumer cancellation rights policy** | Event ticket sales are exempt from the 14-day cooling-off (CCR 2013 reg 28(1)(h)) but must clearly disclose this. |
| **ADR (Alternative Dispute Resolution) provider engagement** | Optional but expected for consumer marketplaces — gives customers a path beyond your support inbox. |
| **Accessibility statement** | Required if you have a public-facing service. Brief doc explaining where you stand on WCAG 2.1 AA. |

### Operational

- **Customer support inbox + SLA** — separate from B2B support if volumes warrant; same inbox if not
- **Refund processing workflow** — who approves, how fast (typical: within 5 working days)
- **Dispute escalation process** — when customer support can't resolve, who decides
- **Fraud monitoring** — chargeback patterns, repeat-cancellation customers

## 4. What's NOT changing (proof that switch-on is additive)

This is the key constraint the user asked for. Verifying it explicitly:

| B2B component | Touched by marketplace switch-on? |
|---|---|
| `OrganizationEntity` + `ComplianceContext` | **No.** Already venue-keyed; ticket sales just feed money into the same Stripe account. |
| Settlement workflows (FlatFee, DoorSplit, VenueHire, Versus) | **No.** They already handle ticket revenue. Customer ticket purchases just populate `Concert.DoorRevenue` instead of manual entry. |
| Stripe Connect Express setup | **No.** Same connected accounts, same charge routing. |
| Venue/Artist onboarding | **No.** DAC7 compliance fields already collect what's needed; the venue's legal entity is already what customers see on tickets. |
| `BookingEntity` snapshot pattern | **No.** Already snapshots venue + artist compliance at Accept. |
| Auth model (Organization + Membership) | **No.** Customers use a different auth path (CustomerEntity, separate role); doesn't intersect with venue/artist. |
| Module boundaries | **No.** Customer module exists; ticket logic already lives in Concert + Customer modules. |

### Things that *might* need a small B2B touch (flag for review at switch-on time)

| Concern | Likely change |
|---|---|
| Pricing transparency may need platform fee surfaced on `BookingEntity` differently than today | If today the platform fee is implicit / not stored, may need to add `PlatformFeeSnapshot.PlatformFee` to the snapshot. Small change. |
| Cancellation flow may need consumer-protection branches | E.g. "if venue cancels, customers get auto-refund regardless of venue agreement" — adds rules to the existing `Cancelled` state machine but doesn't change architecture. |
| Settlement timing | B2B might settle immediately; customer-paid bookings may need to hold for a chargeback window. Configurable on the booking, not architectural. |
| Refund routing | Customer refunds reverse the original ticket charge (in the venue's Stripe account). Currently no code path for this; needs an `ICustomerRefundService`. New code, doesn't touch existing code. |

**Bottom line:** the architectural foundation laid in the Organization refactor handles marketplace correctly. The switch-on work is overwhelmingly **adding new code paths**, not modifying existing ones.

## 5. Effort estimate when the time comes

| Workstream | Effort |
|---|---|
| Code (the items in §3) | ~3-4 weeks of focused dev |
| Legal/solicitor | ~4-8 weeks elapsed |
| Pre-launch business (beta customers, support, marketing) | ~2 weeks |
| **Total calendar time** | **~2-3 months** |

If B2B launch in Nov 2026 goes well, marketplace switch-on could realistically target Q1-Q2 2027. If B2B onboarding takes all your time, push it.

## 6. Risks specific to marketplace

| Risk | Mitigation |
|---|---|
| CMA enforcement action over pricing transparency or hidden fees | Get the pricing-transparency UI right from day one; show **all fees pre-checkout**. Audit the implementation against CMA's published guidance. |
| Chargebacks hit Concertable balance via Stripe Express OnBehalfOf flows | Reserve a small float on platform balance for chargeback coverage; pursue customer for cause on legitimate disputes. |
| Consumer dispute escalation overwhelms support | ADR provider engagement before launch; clear refund policy that reduces ambiguity. |
| Accessibility complaint to EHRC (Equality and Human Rights Commission) | WCAG 2.1 AA pass before launch; published accessibility statement. |
| Online Safety Act enforcement (event listings or comments containing illegal content) | Risk assessment documented; takedown route live; SLA for response. |

## 7. Decision points to revisit when planning switch-on

- **Resale allowed?** Recommended: **no for v1**. Sidesteps a large compliance surface (Digital Economy Act 2017 secondary-ticketing rules). Revisit if customers/venues complain.
- **Refund policy granularity** — full refund only? Partial refunds? Credits? Decide before customer T&Cs draft.
- **Platform booking fee on customer ticket** — separate from venue's price, shown transparently. Decide pricing.
- **Display of seller legal entity** — full company name, or just trading name? Must be unambiguous on receipts; venue chooses on confirmations.

## 8. Reference

- [LAUNCH_PLAN.md](LAUNCH_PLAN.md) §8 — high-level marketplace mention
- [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md) "Deferred — Phase 2" section — the items it explicitly defers
- DICE Standard Ticket Terms — useful reference for consumer-facing disclosed-agent language
- CMA guidance on ticketing — required reading before drafting customer T&Cs
- ICO guidance on cookies + PECR — applies to customer SPA cookie banner

## Decision log

- **2026-05-18** — Marketplace deferred from v1 launch. Rationale: user prioritizes B2B SaaS differentiation; marketplace consumer-protection surface is large enough to push v1 launch significantly later if pulled in.
- **2026-05-18** — Switch-on is **additive**, not invasive. Codified the principle: marketplace work must not require major refactor of B2B code paths. Verified against current architecture (§4).
- **2026-05-18** — Resale: prohibited for v1 if/when marketplace launches. Revisit only on real customer demand. Cuts compliance scope sharply.
