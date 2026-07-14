# DoorSplit/Versus door-revenue settlement

> **Blocker:** the 🔴 in [LAUNCH_PLAN.md](./LAUNCH_PLAN.md) — "DoorSplit/Versus door-take entry at
> settlement." The revenue-share contract types are Concertable's moat vs GigPig/GigXchange; without
> this they settle for **£0** in production. This plan makes them sellable at launch.
>
> **Branch:** `Feature/DoorRevenueSettlement` (create before any code — plan doc itself is exempt).

---

## The problem — precise, from the code

DoorSplit/Versus settle a **percentage of the night's revenue** (`CalculateArtistShare(totalRevenue)`).
Today that `totalRevenue` comes from `ConcertRepository.GetTotalRevenueByConcertIdAsync`:

```csharp
.Select(c => c.TicketsSold * c.Price)   // ConcertRepository.cs
```

`TicketsSold` is **marketplace data** — it's only ever incremented by `TicketSaleProcessor`, which
reacts to a `PaymentSucceededEvent(type=Ticket)`, i.e. a fan buying a ticket **through Concertable's
own checkout**. That checkout is the deferred customer marketplace ([MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md), Q1 2027+).

So in the pure-B2B launch:

1. **Nothing produces that event** — the venue sells tickets on their own site / box office / cash,
   which Concertable never sees. `TicketsSold` stays `0`.
2. **Settlement runs on `0 × Price = £0`.** The artist's share of zero is zero. Every DoorSplit/Versus
   booking pays the artist nothing.
3. **It fires automatically the moment the gig ends.** `ConcertCompletionRunner` sweeps ended `Booked`
   concerts and runs `PayoutFinishStep` unconditionally — there's no point at which a human supplies
   the real figure.

Integration tests (`ConcertDoorSplitApiTests`, `ConcertVersusApiTests`) only pass because the seeder
fakes `TicketsSold`. That masks the £0 in prod.

## The model — self-declaration is the design, not a placeholder

For any ticket **Concertable doesn't process**, the revenue can only ever be **declared by the venue**,
never verified — you can't verify a transaction you were never part of and have no feed for. This is
not a launch limitation with a better successor coming; it's the permanent shape for externally-sold
tickets. The industry already settles door deals exactly this way (promoter's count + cashbox); this
digitises it and adds a signed agreement, an audit trail, and automatic payout on top.

The trust posture we ship on, stated honestly:

- **The venue declares the door revenue** for the night at settlement (the external ticket sales +
  cash on the door — the gross the split is calculated against).
- **It is not verified, and we don't claim it is.** The checks are contractual (signed booking
  agreement binds the split), social (the artist was in the room and sees a full/empty house), and
  repeated-game (a venue that lowballs artists loses its acts). The residual risk — a venue shaving a
  slice the artist can't precisely dispute — is real and inherent; no self-declared system removes it.
- **Verified coverage only ever *shrinks* the declared slice, never removes it.** The only way to get a
  verified number is to be *in* the transaction — sell it ourselves (marketplace) or have the venue's
  ticketer feed sales back (integration). Both cover only the sales that flow through those channels;
  cash on the door and any unconnected channel stay declared forever. See "Future — optional coverage
  extensions" — parked, **not** a fix for the trust gap.

## The fix — one revenue field, venue-declared, gating settlement

Introduce `DoorRevenue` on the concert as the settlement revenue source for the revenue-share types,
declared by the venue manager after the gig, gating the auto-settlement so it can't fire on £0.

`DoorRevenue` is deliberately the **same field name the marketplace plan already assumes**
(`Concert.DoorRevenue`, MARKETPLACE_PLAN.md lines 32/75) — so the design is "one revenue field, two
possible writers": venue-declared now, and (when/if the marketplace or a ticketer integration lands) a
verified writer that pre-fills the same field. v1 is not throwaway.

---

## Phase 1 — Domain + settlement rewire (backend, no UI)

The load-bearing change. Behaviour flips: revenue-share settles off `DoorRevenue`, and only once it's
present.

- **`ConcertEntity.DoorRevenue`** — nullable `decimal?` (null = not yet declared). Domain method
  `DeclareDoorRevenue(decimal amount)` with validation (`>= 0`; guard state — only a `Booked`, ended,
  revenue-share concert). Keep `TicketsSold` and `TicketSaleProcessor` exactly as-is — they stay the
  dormant marketplace writer.
- **Settlement reads `DoorRevenue`.** Repoint `PayoutFinishStep` / `GetTotalRevenueByConcertIdAsync` at
  `DoorRevenue` instead of `TicketsSold * Price`. Absent `DoorRevenue` reaching the payout step is a
  bug (the gate below should make it unreachable) — throw, don't `?? 0` it into a silent £0 (root
  `CLAUDE.md`: don't default away a failure).
- **Gate the auto-settlement sweep.** `ConcertCompletionRunner`/`GetEndedConfirmedIdsAsync` must only
  pick up a revenue-share concert once `DoorRevenue` is set. Fixed-amount types (FlatFee, VenueHire)
  are unaffected — they finish on end as today (escrow already captured; no input needed). Express the
  predicate as "settlement inputs ready," not a bare `ContractType` switch in agnostic code — see
  [CODE_PATTERNS.md](../../api/docs/CODE_PATTERNS.md) on keyed strategies; a `RequiresDoorRevenue`
  capability on the workflow is the on-pattern way to avoid re-branching on the type.
- **Migration** — `./initial-migrations.ps1` from `api/` (re-scaffold, never additive).
- **Tests** — `ConcertDoorSplitApiTests` / `ConcertVersusApiTests` must stop asserting against
  `TicketsSold * Price` and instead declare `DoorRevenue`, then assert the settlement charge equals
  `CalculateArtistShare(DoorRevenue)`. Add: a revenue-share concert with **no** `DoorRevenue` is **not**
  swept/settled (proves the gate). Fix the seeders accordingly.

**Gate:** `dotnet build api/Concertable.slnx` green · Concert module unit + integration via
`integration-debug`. Zero UI, but it flips a covered settlement flow → **run API E2E** (`e2e-api-debug`)
before calling the phase done.

## Phase 2 — Venue declares door revenue (endpoint)

- **Endpoint** — venue-manager action to declare the night's revenue on an ended, `Booked`,
  revenue-share concert (e.g. `POST /api/Concert/{id}/door-revenue` with a `DoorRevenueRequest`).
  Authorized to the concert's **venue tenant** only (via `ITenantContext`, per
  [api/CLAUDE.md](../../api/CLAUDE.md) — not the shared identity type). Idempotent / re-declarable only
  while still `Booked` (once settlement fires it's frozen).
- **HATEOAS** — expose a `declare-door-revenue` link on the concert/booking read model, gated to the
  venue when the concert has ended, is `Booked`, revenue-share, and `DoorRevenue` is still null.
  Suppress on fixed-fee types and once settled.
- **Validation** — `DoorRevenueValidators` (`>= 0`, present). Don't `TryParse`-swallow a bad value into
  a benign one.
- **Reminder nag (in-phase or fast-follow)** — a venue with an ended revenue-share gig and no declared
  revenue is a stuck settlement. Surface it (notification / dashboard task) so payout isn't silently
  parked forever. Minimum: it's visible; escalation policy can be v1.1.
- **Tests** — integration: authorized venue declares → concert becomes sweep-eligible → settlement
  charges `CalculateArtistShare(DoorRevenue)` and pays the artist tenant; non-venue caller rejected;
  declaration blocked once `Complete`.

**Gate:** build green · Concert module integration via `integration-debug`.

## Phase 3 — Venue SPA screen

**Research first — do NOT design the interface up front.** Before writing any UI, study how the venue
manager SPA already surfaces concerts/bookings and their actions, and mirror those patterns rather than
inventing new ones. In particular read: the concert detail route/page
(`app/web/b2b/venue/src/routes/_venue/my/concerts/concert.$id.tsx`, `b2b/shared/.../MyConcertPage.tsx`),
the existing HATEOAS-gated action components (`CancelBookingButton.tsx`, the agreement download flow
`useDownloadAgreement.ts`, `ESignaturePanel.tsx`) for the established action/hook/link pattern, and the
settlement surfacing already present on the dashboard (`useVenueSettlements.ts`,
`VenueSettlementsWidget.tsx`) — the door-take action likely belongs alongside or feeds into that. Also
confirm the boundary tier: shared vs `b2b/shared` vs venue-only (per [app/web/CLAUDE.md](../../app/web/CLAUDE.md)),
and remember all four app builds are the gate. Only after that, write up the concrete screen/flow in
this phase (or a short sub-plan) and build it. The bullets below are the *requirements* the design must
satisfy, not the design itself:

- **Surface the task** — ended revenue-share gigs awaiting declaration appear as an action ("Enter door
  takings to settle"), driven off the HATEOAS link (never a client-side contract-type check).
- **Entry** — a gross-revenue input (£) with a plain-language note that this is the figure the artist's
  share is calculated from and that it's a declared, contractually-binding number. Submit → endpoint →
  refresh; the action clears and the booking moves toward `Complete`.
- Fixed-fee bookings never show this — they settle automatically.

**Gate:** build green · this is the final phase and flips a user-facing money flow on a covered path →
**run UI E2E** via `e2e-ui-debug` (a DoorSplit or Versus settle-via-declared-revenue scenario).

## Phase 4 — Docs close-out (rides Phase 3's PR; no standalone PR)

- **[LAUNCH_PLAN.md](./LAUNCH_PLAN.md)** — flip the 🔴 DoorSplit/Versus line to ✅ with a one-line note
  (venue-declared `DoorRevenue` gates settlement; `TicketsSold` remains the dormant marketplace writer).
- **[MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md)** — reconcile: `DoorRevenue` now exists as the
  venue-declared writer; marketplace switch-on adds a *second, verified* writer to the same field. It no
  longer "replaces manual entry" — it supplements it for the fraction sold through us.
- Capture the honest self-declaration posture where the legal/trust record lives
  ([LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/src/Modules/Contract/LEGAL_REQUIREMENTS.md) or the
  Contract ARCHITECTURE) — declared, not verified; the checks; the residual risk.
- `git rm` this plan in the commit that lands the last phase (plans are working docs, not an archive).

---

## Future — optional coverage extensions (parked, NOT part of this work)

These *shrink the declared slice* for venues who opt in; none removes self-declaration, and none is a
launch dependency. Logged here so the decision is recorded, not rediscovered:

- **Inbound ticketer reconciliation** — venue connects their existing ticketer (Skiddle/DICE/Eventbrite);
  Concertable pulls sold counts via API and pre-fills / verifies the declared `DoorRevenue`. The direct
  lever on the trust gap — but a big per-ticketer integration + reconciliation build for *partial*
  coverage (venues still hold cash-on-door and unconnected channels back). Was explicitly ruled out for
  v1 scope in LAUNCH_PLAN §9 — as a scope call, not forever.
- **Outbound inventory distribution** — Concertable holds allocation and pushes inventory out to
  external apps to sell, becoming the source-of-truth inventory system. Much larger — a ticketing
  backbone, adjacent to being a primary ticketing platform. Long-horizon only.
- **Own marketplace** — the already-planned customer checkout ([MARKETPLACE_PLAN.md](../customer/MARKETPLACE_PLAN.md)):
  tickets sold through us are verified via `TicketsSold`, feeding the same `DoorRevenue` field.

All three are *coverage*, not *correctness*: the declared figure remains the settlement model for
everything not sold through an observable channel.

## Verification gate (per [plans/CLAUDE.md](../CLAUDE.md))

- Every phase: `dotnet build api/Concertable.slnx` green + affected module unit/integration via
  `integration-debug`.
- Phase 1 changes the model → ends with `./initial-migrations.ps1` from `api/`.
- Phase 1 flips a covered settlement flow → API E2E (`e2e-api-debug`).
- Phase 3 is the final phase + user-facing money flow → UI E2E (`e2e-ui-debug`).
