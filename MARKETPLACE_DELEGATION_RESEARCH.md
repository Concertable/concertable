# Research: Can Concertable delegate ticketing to an external marketplace API?

> **Date:** 2026-06-22 · **Method:** `/deep-research` (web fan-out + 3-vote adversarial verification) + codebase scout + targeted backfill.
> **Status:** Throwaway working doc. The decisions it drove live in [`plans/b2b/LAUNCH_PLAN.md`](plans/b2b/LAUNCH_PLAN.md) §9 / §1 / decision-log. Delete this once the citable facts have been used (solicitor / investor conversations). Git history is the archive.
>
> **Why it exists separately from the plan:** the plan holds the *decision* (one line). This holds the *evidence* — primary-source facts (payout timings, merchant-of-record status, SEC numbers) that are reusable outside the codebase but don't belong in a plan.

---

## TL;DR — the decision

**You cannot delegate ticketing to an external marketplace API and keep your headline USP.** Not primarily because of the data problem (settlement needs sales numbers) — that's solvable — but because **every external ticketing platform is the *merchant of record*: they hold the buyer's money and pay the *venue* after the show.** Your settlement engine works by being in the flow of funds (Stripe Connect). If Eventbrite/DICE/Skiddle hold the money, "automated settlement" degrades to "automated invoicing" — which is exactly what GigPig now does.

**So the call (now recorded in LAUNCH_PLAN §9):**
- **v1 revenue feed for DoorSplit/Versus = manual door-takings entry.** Standalone, no external dependency, ships all four contract types.
- **Durable feed = owned checkout (the deferred marketplace).** The only model where you hold ticket funds directly.
- **External-ticketer import = ruled out.** Buys only a *verified* sales number over manual entry, for high integration cost and DICE-only availability.

The B2B-first / marketplace-deferred strategy was already correct — this research **confirmed** it and **closed an open decision**, it didn't reverse anything.

---

## The reframe: delegation loses the *money*, not just the data

Your DoorSplit/Versus settlement does two things: (1) read ticket revenue, (2) move the resulting split. Read the "who holds the money" column below top-to-bottom — in every delegated model the money lands in *someone else's* account and is paid to the *organiser* after the show. Concertable never touches it.

| Platform | Create events via public API? | Read back ticket-sales data? | Who holds the buyer's money? | Organiser paid when? |
|---|---|---|---|---|
| **DICE** (Partner API) | ❌ Read-only GraphQL, no mutations | ✅ Per-event sales, orders, attendees, returns, transfers | **DICE** (merchant of record) | Within **5 business days *after* the event** |
| **Skiddle** | ❌ Selling is via Promotion Centre dashboard, not the API | ❌ Sales live in the dashboard, not the API | **Skiddle** | After the event; remittance Tue & Thu, +2–3 days |
| **Eventbrite** | ⚠️ Endpoint exists but org-scoped + public-app approval (2020 deprecation broke user-scoped apps) | ⚠️ Orders API gated by the same approval regime | **Eventbrite** (merchant of record under EPP) | **After the event** by default; pre-event payout gated to "qualified creators" |
| **Ticketmaster / Universe** | ❌ Discovery API read-only; publishing is enterprise/partner-gated | ❌ Not for grassroots third parties | Ticketmaster | Enterprise terms — irrelevant to pub/club gigs |
| **Songkick / Bandsintown** | These are **discovery**, not ticketing — no checkout, no money | n/a — listings only | n/a | n/a |

**The one that looks viable (DICE) still isn't:** it exposes the *sales numbers* via API, but it already paid the venue, so you can't *execute* the split — you'd be emailing the venue an instruction to pay the artist. That's an invoice. That's GigPig.

---

## Where the moat actually is (sharper than "contracts")

- **Flat-fee / venue-hire auto-payout is NOT a moat.** GigPig now markets automated payments and a **"Payment House" that takes one venue payment and splits/distributes it to artists** (per their 2026 site copy). If your MVP only did flat-fee, you'd be rebuilding GigPig.
- **Door-split / versus settlement IS the moat — *because* it requires owning the ticket-money flow.** GigPig structurally can't do it: no owned ticketing → no ticket revenue to split a door deal from.

So the differentiator is **revenue-share settlement computed from ticket data you control** — inseparable from owning checkout. The one thing that makes you not-GigPig is the one thing delegation destroys.

---

## The distinction you were conflating: checkout rail ≠ consumer marketplace

"The marketplace is too much work" bundles two very different things:

1. **Ticketing checkout (the money rail)** — event page + buy button + Stripe Connect split + QR ticket. *You already have this* in the `Customer` Ticket module. It's a hosted per-event checkout, not "building Eventbrite." It's the rail the USP runs on. **Keep it.**
2. **Consumer discovery marketplace (the destination)** — browse, search, recommendations, preferences, notifications, the B2C brand, the mobile app. *This* is the expensive, network-effect-dependent, density-dependent part. **Defer it** (you already do — MARKETPLACE_PLAN, Q1 2027+).

Get demand to the checkout without a destination: venues/artists drive their own audience to the link; optionally syndicate listings *outward* to Bandsintown/Songkick for top-of-funnel. That uses external marketplaces for *reach* (safe) while keeping the *money* on your rail.

---

## UK regulatory note (one reason owned-checkout is also the *safe* path)

- **Stripe Connect (separate charges & transfers)** keeps **Stripe** as the regulated payment institution and **you as the platform** — the lane you want.
- Taking ticket money *into your own account* and paying it out yourself risks being a regulated payment activity needing **FCA authorisation** (PSRs/EMRs).

This is *why* owning checkout via Stripe Connect is right — it's "in the flow of funds" without becoming a payment institution. Ties to LAUNCH_PLAN R8 (confirm disclosed-agent posture with the solicitor). *Not legal advice.*

---

## Appendix A — Verified findings (survived 3-vote adversarial verification)

Each was checked by 3 independent skeptic agents; only claims that weren't refuted are listed.

**DICE**
- Partner GraphQL API is **read-only** — only query operations, **no mutations** → cannot create events/ticket types via API. *(3-0)* — `https://partners-endpoint.dice.fm/graphql/docs/index.html`
- API **does** expose per-event ticket sales, orders, attendee identities (name/email/DOB/phone), returns, transfers. *(3-0)* — same
- API does **not** expose payout/settlement data. *(3-0)* — same
- DICE collects/holds all proceeds; pays the vendor **within 5 business days *after* the event**. *(3-0)* — `https://support.dice.fm/article/758-mio-ticketing-terms-and-conditions-uk`

**Skiddle**
- Web API does **not** sell tickets — selling is via the separate Promotion Centre dashboard. *(3-0)* — `https://www.skiddle.com/api/`
- API landing/terms = **read-only data consumption**; no event-creation, no sales/orders/attendee/payout access. *(3-0)* — `https://www.skiddle.com/api/` , `https://www.skiddle.com/api/join.php`
- Holds buyers' funds, **releases after the event**; remittance Tue & Thu (11am & 4pm), +2–3 days to land. *(3-0)* — `https://promotioncentre.co.uk/blog/everything-you-need-to-know-about-remittance/`

**Eventbrite**
- **2020 deprecation** of user-scoped endpoints (incompatible with the Organizations & Permissions model). *(3-0)* — `https://groups.google.com/g/eventbrite-api/c/8urwbU6efFc`
- Under EPP, Eventbrite is **merchant of record** (collects face value + fees, remits to creator, books revenue gross); FPP is net. *(2-0)* — `https://www.sec.gov/Archives/edgar/data/0001475115/000147511524000197/eb-20240930.htm`
- Holds **$327.6M** of creators' ticket money on its own balance sheet (Sep 30 2024), legally unrestricted, partly invested in T-bills. *(2-0)* — same
- Pays creators **after** the event by default; pre-event "advance payouts" gated to "qualified creators." *(3-0)* — same

**Backfill (targeted fetch, not 3-vote verified):**
- **GigPig** now markets automated payments + a "Payment House" that splits one venue payment across artists; standard 14-day-after-sign-off terms. — `https://www.gigpig.uk/venues/frequently-asked-questions` + site copy
- **Eventbrite** event-creation endpoint still exists but is org-scoped + public-app-approval-gated.
- **Ticketmaster** Discovery API is read-only; Partner API is gated/enterprise.

## Appendix B — Confidence & gaps

- **High confidence** (primary sources, 3-vote verified): all platforms being merchant-of-record and paying out post-event; DICE read-only-but-sales-readable; Skiddle selling only via dashboard; Eventbrite SEC numbers.
- **Medium-high:** GigPig's "Payment House" — from GigPig's own marketing copy, not independent; treat exact mechanics as directional.
- **Thin:** the wider competitor landscape (Encore, Function Central, Muso). The verification votes for those died on a transient API overload (529), so breadth is shallower than the run's token count implies. The core thesis doesn't rest on them.
- **What the run got wrong/wasteful:** the auto-synthesis step failed on the same 529 overload (report hand-written instead); `/deep-research` was a heavy instrument for a question that was ~70% about the codebase — a scout + a few targeted fetches did most of the work.
