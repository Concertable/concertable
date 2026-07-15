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

## Execution shape — ONE PR, several commits (2026-07-15)

Phases 1's work is merged. The remaining phases ship on **one branch → one PR**, but as **one commit per
phase** (per [plans/CLAUDE.md](../CLAUDE.md) Lifecycle) — Phase 2 (backend), Phase 3 (SPA), Phase 4 (docs
close-out, folded into the final commit). Branch: **`Feature/DoorRevenueSettlement`** (recreated; the
Phase-1 branch merged and was deleted).

### Decisions pinned before starting (were vague / omitted in the original draft)

- **No new migration in Phase 2.** `DoorRevenue` (Phase 1) and `TicketsSold` (marketplace) columns
  already exist; `IsRevenueShare` is *derived* (`Booking is DeferredBooking`), never stored. Phase 2 adds
  no schema change → no `initial-migrations.ps1`.
- **Money is `decimal` pounds end-to-end** — matches `ConcertEntity.Price`/`DoorRevenue` (decimal) and the
  existing `deal.fee` `NumberInput` (`step="0.01"`, pounds). The request body, read fields, and SPA input
  all carry pounds. Do **not** route door-revenue through the cents `formatCurrency(amountCents)` helper;
  render pounds directly. Convert only if a shared money component demands cents.
- **Explicit venue-tenant guard is added in Phase 2.** Phase 1's `DeclareDoorRevenueAsync` guards
  type/timing/state but does *not* assert the caller owns the booking — it leans on the permission gate +
  row filter (a non-party venue currently falls through to a 400, not a clean 403). Phase 2 injects
  `ITenantContext` into `ConcertService` and adds `if (concert.Booking.VenueTenantId != tenantContext.TenantId)
  throw new ForbiddenException(...)` — fail-closed, matching the `ApplyExecutor`/`SetupCheckoutStep` pattern.
- **Reminder nag scope = the dashboard KPI count + the on-concert action, in this PR. Email/in-app
  escalation is v1.1.** The venue dashboard is KPI-counts-based (`VenueDashboardCounts` = ApplicationsTo
  Review / OpenOpportunities / UpcomingConcerts). The minimum "it's visible" is a new `AwaitingDoorRevenue`
  count (Phase 2 backend) surfaced on the dashboard (Phase 3) plus the HATEOAS action on the concert page.
  No new notification plumbing.

## Phase 2 — Venue declares door revenue (endpoint + read-model surfacing)

**Read-model prerequisite (do first — the HATEOAS gate and the Phase 3 breakdown both need it).** The
internal read DTO `ConcertDetails` (`Concert.Application/DTOs/ConcertDtos.cs`) currently carries `State`,
`EndDate`, `Price`, `TotalTickets` — but **not** `DoorRevenue`, `TicketsSold`, or any revenue-share signal,
so nothing downstream can gate on them today. Add them:
- `ConcertDetails`: add `decimal? DoorRevenue`, `int TicketsSold`, `bool IsRevenueShare`.
- Projection `QueryableConcertMappers.ToDetails` (`Concert.Infrastructure/Mappers`): project
  `DoorRevenue = c.DoorRevenue`, `TicketsSold = c.TicketsSold`, `IsRevenueShare = c.Booking is DeferredBooking`
  (EF TPH translates the `is` to a discriminator check; `Booking` is already joined by this projection).
- Public wire shape stays clean: `TicketsSold`/`DoorRevenue` are **venue-private** — expose them on
  `ConcertDetailsResponse` as nullable, **populated only in `ToCurrentUserDetailsResponse`** (the owner
  read), left null by the anonymous `ToDetailsResponse`, exactly the "null on public, set on owner" contract
  `Actions` already uses. Never surface `DoorRevenue` on the marketplace read.

**Endpoint** — `[HasPermission(VenuePermissions.ConcertsManage)] [HttpPost("{id}/door-revenue")]` on
`ConcertController`, binding `[FromBody] DoorRevenueRequest` → `concertService.DeclareDoorRevenueAsync(id,
request.DoorRevenue)` → `NoContent()`. Mirrors the existing `Post`/`Cancel` endpoints. Identity is the
route + `ITenantContext`, never the body (api/CLAUDE.md DTOs-vs-Requests rule). Re-declarable while `Booked`;
the Phase-1 service already throws `ConflictException` once settled.
- `DoorRevenueRequest { public decimal DoorRevenue { get; init; } }` in
  `Concert.Application/Requests/ConcertRequests.cs`.
- `DoorRevenueRequestValidator : AbstractValidator<DoorRevenueRequest>` (`GreaterThanOrEqualTo(0)`)
  alongside `Concert.Application/Validators/ConcertValidators.cs`. No `TryParse`-swallow of a bad value.

**Explicit venue-tenant guard** — inject `ITenantContext` into `ConcertService`; add the
`VenueTenantId == tenantContext.TenantId` fail-closed check in `DeclareDoorRevenueAsync` (see the pinned
decision above).

**HATEOAS** — add `ActionLink? DeclareDoorRevenue` to the `ConcertActions` record
(`Concert.Api/Responses/ConcertResponses.cs`); gate it in `ToCurrentUserDetailsResponse`:
`State == Booked && IsRevenueShare && DoorRevenue is null && EndDate < utcNow` →
`ActionLink("/api/Concert/{id}/door-revenue", HttpMethods.Post)`, else null. The mapper is currently
pure/static and the `Cancel` gate is time-free — thread `utcNow` in as a parameter, sourced from an
injected `TimeProvider` in the `GetDetailsForCurrentUser`/`GetDetailsByApplicationId` controller actions
(keeps it testable; no `DateTime.UtcNow` inside the mapper). Suppress on fixed-fee and once declared/settled.

**Reminder nag** — add `AwaitingDoorRevenue` to `VenueDashboardCounts` (`Concert.Contracts`), compute it in
`ConcertDashboardRepository.GetVenueCountsAsync` (`Period.End < now && State == Booked && Booking is
DeferredBooking && DoorRevenue == null`, scoped to the venue), and thread it through `VenueDashboardKpis` +
its mapper. (Frontend surfacing is Phase 3; escalation is v1.1.)

**Tests** — integration (`Concert.IntegrationTests/Concert/`): authorized `VenueManager1` declares **over
HTTP** (`POST /api/Concert/{id}/door-revenue`) → concert becomes sweep-eligible → settlement charges
`CalculateArtistShare(TicketsSold*Price + DoorRevenue)` and pays the artist tenant; non-venue (artist)
caller → **403** (mirror `Cancel_ShouldReturn403_WhenCallerIsArtist`); declaration after `Complete` → **409**;
assert the `declareDoorRevenue` link appears on the owner read only when the gate holds. **Migrate the arrange
helpers off their stopgaps to the real endpoint**: `ConcertWorkflowExtensions.DeclareDoorRevenueAsync`
(currently resolves `IConcertService` in-process) and the E2E `ConcertDb` raw-SQL `UPDATE` (its own comment
says "until the declare endpoint lands (Phase 2)").

**Gate:** build green · Concert unit + integration via `integration-debug`. No model change → no migration.
API E2E is left to the merge-queue gate (the settle-after-declare path is covered by integration + the
Phase-3 UI E2E).

## Phase 3 — Venue SPA screen

**Mirror the existing patterns — don't invent.** The recon below (2026-07-15) already located them; read
each before writing the analogue. The gating principle is unchanged: render off the **HATEOAS link's
presence**, never a client-side contract-type check.

*Three-tier boundary (per [app/web/CLAUDE.md](../../app/web/CLAUDE.md), confirmed):* data/types/mutation
logic → `app/shared` (`@concertable/shared`); DOM/design-system/web glue → `app/web/shared`; venue-only
manager code → `app/web/b2b/venue/src`. Declaring door takings is a **venue-only** decision (the artist app
renders no manager actions), exactly like `CancelBookingButton` — so the component is venue-only; only the
type + api method + data hook are shared.

**Shared tier (`app/shared/src/features/concerts/`):**
- Add `declareDoorRevenue?: ActionLink | null` to `ConcertActions` in `types.ts`, and surface the owner-only
  `ticketsSold` / `doorRevenue` on the concert read type (matching the Phase-2 response fields).
- `concertApi.declareDoorRevenue(id, { doorRevenue })` → `api.post('/concert/${id}/door-revenue', ...)` — a
  convention path, mirroring `concertApi.cancelConcert` (the UI gates on the link, calls the convention URL).
- `useDeclareDoorRevenue(id)` mutation mirroring `useCancelConcert`: on success
  `invalidateQueries(["concert", id])` (the `useMyConcertQuery` key) plus the venue settlements/dashboard keys.
- Zod request schema (`schemas/`) — `doorRevenue >= 0`, mirroring `updateConcertRequestSchema`.

**Venue tier (`app/web/b2b/venue/src/features/concerts/`):**
- `DeclareDoorRevenueButton`/panel alongside `CancelBookingButton.tsx` (export from the feature barrel);
  inject it via the `renderActions` slot in `routes/_venue/my/concerts/concert.$id.tsx`, gated on
  `concert.actions?.declareDoorRevenue` — same shape as the existing `concert.actions?.cancel` clause.
- Input pattern: `NumberInput min={0} step="0.01"` with a `(£)` label (the `DealFields.tsx` money-field
  pattern); validate via the Zod schema `safeParse` surfacing `error.issues[0].message` (the `ESignaturePanel`
  pattern). Submit → `useDeclareDoorRevenue` → refresh; the action clears as the booking moves toward `Complete`.
- **The breakdown is mandatory and is the whole point of getting the UX right:** the input is the **external
  door take only** (venue's own ticketing + other ticketers + cash on the door), with a plain-language note
  that it **excludes** tickets sold through Concertable. Show the sum the split actually applies to:
  `Concertable sales (ticketsSold × price, known) + your declared external take = total`. Do **not** present
  the input as "the figure the share is calculated from" — that invites the venue to enter the whole gross
  and double-count Concertable's own sales.
- Fixed-fee bookings never show this (no link) — they settle automatically.

**Dashboard nag (surface the Phase-2 count):** show the `AwaitingDoorRevenue` KPI on the venue dashboard
(alongside the existing KPI counts / `VenueSettlementsWidget`) so a stuck settlement is visible. Minimal —
a count/badge that points the venue at the gigs awaiting declaration.

**Gate:** **all four app builds** green (`npm -w @concertable/web-{customer,venue,artist,business} run build`
— the customer/artist builds prove no venue-only concept leaked into a shared tier) · this is the final
code phase and flips a user-facing money flow on a covered path → **run UI E2E** via `e2e-ui-debug` (a
DoorSplit or Versus settle-via-declared-revenue scenario).

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
