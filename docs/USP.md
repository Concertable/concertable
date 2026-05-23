# Concertable — Unique Selling Proposition

The product thesis, the competitive landscape, and why the integration nobody else
has built is actually defensible. Written 2026-05-19 from a research session; revise
when assumptions change.

See [`OVERVIEW.md`](./OVERVIEW.md) for what the product *is*; this doc is about
*why anyone would buy it over the alternatives*.

---

## One-line thesis

**Concertable is not selling ticketing, and not selling a settlement engine. It is
selling the absence of workflow seams between booking a gig and selling tickets to
it.** The settlement engine and the ticketing surface are components that make a
seamless workflow possible — not the product themselves.

Every feature decision can be evaluated against: *does this remove a context
switch, or add one?*

---

## Customer segments

The UK live-music venue market is not one segment. Three matter for positioning:

1. **Background-music pubs** — flat fee, no cover charge, music exists to sell beer.
   GigPig owns this. ~95% flat-fee, no demand for door splits because there are no
   tickets to split. **Do not target.**
2. **Music pubs / small ticketed venues / back-rooms** — sometimes flat fee,
   sometimes door split (often informally / on a handshake), £5–£10 cover charge
   for the right act, currently glue together GigPig + spreadsheet + Eventbrite.
   **Primary target.**
3. **Dedicated grassroots music venues (100–500 cap)** — door split, versus,
   guarantees are normal. Existing tooling is even worse for them than for segment 2.
   **Secondary target / upmarket expansion.**

The refined customer description is **"venues that sell tickets to their gigs"** —
which spans the ticketed subset of segment 2 plus all of segment 3, and explicitly
excludes segment 1.

---

## Competitor comparison

| | GigPig | GigXchange | Concertable |
|---|---|---|---|
| **Marketplace** | Yes — venues post, artists apply | Yes — any role can initiate | Yes |
| **Contract / settlement model** | Booking confirmation, flat fee only | Digital agreement + Stripe escrow, flat fee only | Four typed contracts: FlatFee, DoorSplit, VenueHire, Versus (see [`CONTRACT_ARCHITECTURE.md`](./api/Modules/Contract/CONTRACT_ARCHITECTURE.md)) |
| **Door split / revenue share** | No | No | Yes |
| **Artist-pays-venue direction (VenueHire)** | No | No | Yes |
| **Guarantee + split (Versus)** | No | No | Yes |
| **Ticketing** | No | No | Yes (Stripe Connect, full purchase flow) |
| **Fan-facing surface** | No | No | Yes (customer SPA) |
| **Auto-creation of event from booking** | N/A — no event concept | N/A — no event concept | Yes — accepted application creates the concert |
| **Pricing** | £10/booking or £150/mo | 0% commission + 8% transaction fee | TBD |
| **Scale (May 2026)** | 3,000 venues, 18,000 artists, 120k gigs | Open Alpha, first 250 free | Pre-launch |
| **Target customer** | Background-music pubs (segment 1) | Same as GigPig + some agents/promoters | Ticketed small venues (segment 2 + 3) |

**Key correction to common framing:** GigXchange's "contract" is a *legal artifact*
(a generated PDF agreement + escrow deposit hold). It is not a typed settlement
engine. Architecturally they have the equivalent of Concertable's `FlatFeeContract`
and nothing else. Their "contract generator" markets the *existence* of a contract,
not the *sophistication* of money-movement models.

Other adjacents worth knowing:
- **VenuePilot** (US) — closest analogue: booking + ticketing + venue websites
  integrated. Less of a true two-sided marketplace; more "venue's own booking
  dashboard." Worth studying as a "what does this look like at scale" reference.
- **Gigwell** — agency-side booking + settlements, no integrated ticketing.
- **Muzeek** — syncs with third-party ticketing rather than owning it.
- **Encore, Alive Network** — issue contracts but no public evidence of multi-model
  settlement.
- **DICE, Skiddle, Eventbrite, Ticketmaster** — pure ticketing, no booking
  marketplace.

---

## Why nobody has integrated booking-marketplace + ticketing

This is the most important section to understand. The gap is real, not imagined,
and the reasons it exists are structural:

1. **Dual cold-start.** A booking marketplace needs artist supply + venue demand.
   A ticketing platform needs fan supply + event demand. Either is a 3–5 year
   slog. Doing both at once is two cold-starts in parallel.
2. **Different sales motions.** Booking sells venue-by-venue (sales-led, slow,
   relationship-heavy). Ticketing acquires fans (marketing-led, fast,
   SEO/social/aggregation). Different team, different playbook. Companies pick one.
3. **Network-effect mismatch.** Ticketing benefits from being a *destination*
   (DICE, Skiddle build fan-side audiences). Booking benefits from being
   *embedded* in the venue's workflow. Hard to optimise for both.
4. **Specialisation wins early markets.** GigPig won the flat-fee booking niche
   by only doing that. Eventbrite won SMB ticketing by only doing that.
   Multi-product platforms are slower to win any single market.
5. **Acquirers haven't bothered.** Ticketmaster could buy GigPig and integrate,
   but small-venue booking is too small to move TM's numbers. Eventbrite has been
   busy fighting for survival. The economic incentive to merge the two halves
   hasn't existed at the level of the existing players.

**Implication for Concertable:** the moat isn't "we ticketing better" or "we
contract better." The moat is **"we are the only company that has both halves
under one roof, so we are the only company that can automate the seam between
them."** DICE will not build booking; GigPig will not build ticketing; the
incumbents on each side are structurally disinclined to cross over.

---

## The actual USP

The killer integration moments — none of which any competitor can offer:

- Booking confirmed → ticketable concert auto-created, no duplicate data entry,
  no second tool
- Ticket revenue → flows into settlement automatically, no manual reconciliation
- Artist payout → one transaction combining flat fee / door split / guarantee
  top-up, not three separate flows
- Fan attendance data → flows back into venue's view of "is this act a draw"
- Latent door-split demand at segment 2 — door splits are absent today not because
  pubs reject them, but because the infrastructure (ticketing, revenue tracking,
  automated settlement) is missing. Concertable supplies all three; the
  tribute-act-£5-cover deal becomes a frictionless 60/40 split where today it's a
  manual flat fee or a handshake split.

**Pitch in one sentence:** *"The moment you accept the application, the concert
exists and is sellable. No second tool, no re-entering the date, no copy-pasting
the artist name. Add a price, hit save, done."*

---

## Why the "good enough" objection doesn't bite

A reasonable pushback: "DICE will out-ticket you, GigPig will out-book you, you'll
lose on both fronts." Three reasons this is weaker than it sounds:

1. **The downside is bounded by non-exclusivity.** Venues can cross-list on DICE
   while still benefiting from auto-creation on Concertable. The worst case for
   us is "ticketing feature underused" — not "venue switches platforms." Low-risk
   asymmetry.
2. **Segment-2 ticketing bars are low.** A 200-cap music pub doing tribute nights
   needs QR code tickets, mobile checkout, email delivery, refunds. It does not
   need seat maps, presales, queue management, or Apple Wallet integration. The
   threshold for "good enough" is a fraction of what DICE provides.
3. **The auto-creation feature is the actual product.** DICE could build a
   booking tool tomorrow but won't (dilutes their fan-side business). GigPig
   could build ticketing but says they don't (different muscle). The two halves
   stay separate because the incumbents have no reason to cross over.

---

## What the pitch should NOT claim

Three traps to avoid:

- **Don't claim "zero context switch" if the venue cross-lists.** We remove our
  half of the workflow seam, not all of it. Honest framing: *"Concertable handles
  your half automatically. If you also list on DICE, that's still manual — but it
  always was."* One-click syndication to DICE/Skiddle/Eventbrite is a possible
  future feature but is hard (depends on their listing APIs) and not day-one.
- **Don't target segment 1 (background-music pubs).** Competing with GigPig on
  volume + price + admin-time-saved means losing — they have 3,000 venues and
  proof points we don't. Let those venues stay on GigPig.
- **Don't pitch the four contract types as the product.** They're a component.
  The product is the seamless workflow they enable. Lead with the seam removal,
  let the contract sophistication land second as the "and we handle the money
  too" follow-up.

---

## Microservices split — strategic, not just architectural

The plan to extract Customer into a separate event-driven service (see
[memory: project_microservice_direction]) and keep B2B as the primary deploy
target isn't just a tech preference — it matches the natural product split:

- B2B side is a SaaS sale to venue managers. Slow, relationship-led, subscription
  economics, predictable load.
- Customer side is a consumer surface. Fast, marketing-led, per-ticket economics,
  on-sale spike load.

The split also preserves optionality: if it turns out building ticketing well is
the harder half and not worth the effort, the venue-facing product can plug into
third-party ticketing (DICE white-label) and Concertable becomes pure B2B.
Inversely, if ticketing is the surprise winner, the Customer service can
eventually serve venues that don't even use the B2B side.

---

## Risks worth tracking

- **VenuePilot expands to UK.** They have the closest architectural shape and
  funding. Watch for European announcements.
- **GigPig or GigXchange bolts on lightweight ticketing.** Less likely (different
  muscle, different team) but possible. If GigPig added even a basic flat-fee
  ticket page, the auto-creation pitch loses some force for segment-1-ish
  venues.
- **DICE / Skiddle build venue-side workflow tools.** Most likely from DICE
  ("DICE for venues"). Would be a real threat. Mitigated because their incentive
  structure points the other way (fan-side discovery business).
- **Door splits at segment 2 don't materialise even with frictionless tooling.**
  Behavioural change is slow. Plan A is FlatFee being the default experience and
  door split being the upsell, not the lead.
