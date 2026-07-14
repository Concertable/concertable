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

- **The venue declares the door revenue** for the night at settlement — the **external** take only
  (tickets sold on the venue's own site / other ticketers + cash on the door), **excluding** anything
  sold through Concertable's own checkout (that's `TicketsSold`, which we already know). The split is
  calculated against the **sum**: `TicketsSold * Price + DoorRevenue`, not `DoorRevenue` alone.
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

## Phase 1 — Domain + settlement rewire (backend, no UI) — ✅ DONE

Revenue-share now settles off a venue-declared `DoorRevenue`, and only once it's present.

**As built** (the original bullets proposed an explicit `AwaitingDoorRevenue` lifecycle state + a
`RequiresDoorRevenue` marker capability; both were dropped as smells — a pure marker with no behaviour,
and a new state where the type system already answers the question):

- `ConcertEntity.DoorRevenue` (nullable `decimal`) + `DeclareDoorRevenue(amount)` domain method (`>= 0`,
  throws otherwise). It's a **concert-level fact** (same shelf as `TicketsSold`), not a booking one.
  It is the **external/box-office/cash** take only — it does **not** replace `TicketsSold` (Concertable's
  own ticket sales), it is **added** to it.
- **No new lifecycle state, no marker.** "Is this a revenue-share settlement?" = `Booking is
  DeferredBooking` (a real behaviour-bearing type — DoorSplit/Versus use it, FlatFee/VenueHire use
  `StandardBooking`). "Awaiting declaration" = a `DeferredBooking` whose `DoorRevenue` is still `null`;
  the gig stays `Booked` until declared.
- `PayoutFinishStep` settles a % of the **total gross** = `TicketsSold * Price` (Concertable's own
  ticket sales, known) **+** `DoorRevenue` (venue-declared external take). Throws if the door figure is
  still `null` (total comes back `null` — the gate makes that unreachable). NB: settling off `DoorRevenue`
  alone was a bug — it would pay the artist £0 on Concertable-sold tickets.
- Sweep gate `ConcertRepository.GetEndedConfirmedIdsAsync`: `… && !(Booking is DeferredBooking &&
  DoorRevenue == null)` — fixed types finish on end; a deferred gig is skipped until declared.
- Declare op `IConcertService.DeclareDoorRevenueAsync` — a plain guarded concert mutation (loads the
  concert, guards `is DeferredBooking` + ended + `Booked`, sets the field, saves), **not** a workflow
  executor: it fires no lifecycle transition and has one behaviour for all revenue-share types. See the
  Concert-module [`CLAUDE.md`](../../api/Concertable.B2B/src/Modules/Concert/CLAUDE.md) for the
  executor-vs-service-method rule. Re-declarable while `Booked`; frozen once settled.
- Migration: **only the B2B Concert module** re-scaffolded (`DoorRevenue` column). `initial-migrations.ps1`
  re-scaffolds every module, but master re-scaffolds per changed module — so only Concert's migration ships.
- Tests: `ConcertDoorSplit/VersusApiTests` declare then assert `CalculateArtistShare(DoorRevenue)`; added
  a gate test (`Finish_ShouldNotSettle_WhenDoorRevenueNotDeclared`). E2E arrange declares via a raw-SQL
  `ConcertDb` helper (no declare endpoint until Phase 2).
- **Also folded in (per owner):** a shared-fixture fix giving the integration test host a loopback client
  IP (`TestClientIpStartupFilter`). Pre-existing break from commit `86ab35bc` "require an origin IP on
  every e-signature" — the harness never set one, so every `Apply` test 500'd on `master`. Unrelated to
  door revenue but unblocks the Concert integration suite.

**Gate:** build green · Concert unit 57/57 · Concert integration 106/106. API E2E not run locally (a
separate `Outbox`-at-startup harness issue on this machine); the merge-queue E2E gate is the real check.

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
- **Entry** — an input (£) for the **external door take only** (venue's own ticketing + other ticketers
  + cash on the door), with a plain-language note that it **excludes** tickets sold through Concertable
  and that it's a declared, contractually-binding number. The artist's share is calculated on the **sum**
  `TicketsSold * Price + DoorRevenue`, so the screen must show that breakdown (Concertable sales, known +
  declared external take = total the split applies to) — **not** present the input as "the figure the
  share is calculated from", which would make the venue enter the whole gross and double-count
  Concertable's own sales. Submit → endpoint → refresh; the action clears and the booking moves toward
  `Complete`.
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
