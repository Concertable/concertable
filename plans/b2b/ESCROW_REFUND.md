# Escrow Refund + Booking Cancellation

Implements the 🔴 top MVP blocker from [LAUNCH_PLAN.md](LAUNCH_PLAN.md): *"Cancellation + escrow
refund — add a `Cancelled` stage and wire `EscrowEntity.Refund()` (the method exists; B2B never calls
it). Today escrow money can come in but can't be refunded in-app."*

**Delivery:** originally scoped as one PR, but the carve forces a **two-PR split** (see below). Each
phase is a **clear-safe checkpoint** — it ends build-green with its gate passed. This file is the
handoff; keep it ticked in lockstep with the commits, and `git rm` it in the final phase's commit.

### Delivery split — forced by the Payment.Client package boundary

B2B compiles against the **published** `Concertable.Payment.Client` package (pinned via
`ConcertablePlatformVersion` in `api/Concertable.B2B/Directory.Packages.props`), not the source next to
it. `RefundByBookingIdAsync` (Phase 2) exists only in source until `publish-packages.yml` ships it — and
that runs **only on push to `master`**. So B2B code calling it (Phase 3+) cannot build on a feature
branch that also carries the Payment change. Per [`../CLAUDE.md`](../CLAUDE.md) "Boundary-blocked
refactors", the work is split:

- **PR1 = Payment** (`Feature/EscrowRefund`, Phases 1+2): ✅ **merged** (`9352b8c4`). CI published
  `Concertable.Payment.Client 0.1.0-alpha.0.547` with `RefundByBookingIdAsync`.
- **PR2 = B2B** (Phases 3–6): in progress on `Feature/EscrowRefundB2B` (fresh off updated master).
  Pin bumped to `.547` (`cb7841ac`), Phase 3 code cherry-picked from the old parked `8863b4f0`
  (`4f73352c`), build + B2B Concert integration green. Continuing with Phases 4–6.

## Scope

**This plan cancels a *concert*, not an *application* — they are different behaviours, do not conflate
them.**

- **Cancel a concert (THIS plan).** A booking that reached `Booked`: a draft concert exists and escrow
  is `Held`. Cancelling it kills the concert (`Booked → Cancelled`) and **refunds the money**. It is
  concert-keyed end to end (`ConcertEntity.Cancel()`, `IConcertWorkflowModule.CancelAsync(concertId)`,
  `RefundEscrowStep`). The HTTP surface and HATEOAS action are therefore **concert-shaped**
  (`POST /api/Concert/{id}/cancel`, a `cancel` action on the concert response) — *not* on
  `ApplicationActions`. `LifecycleState` living on `ApplicationEntity` is just where the booking state
  machine sits; it does not make this an "application" action.
- **Cancel an application (SEPARATE, future plan — equally important).** An artist's *bid* on an
  opportunity, **before** it is booked: pre-money, no escrow, no refund. Today this is **withdraw**
  (artist) / **reject** (venue); a holistic "cancel an application" behaviour (incl. cancel from
  `Accepted`/`PaymentFailed` pre-capture) is its own concern and gets its own plan. Tracked in
  [LAUNCH_PLAN.md](LAUNCH_PLAN.md) so it is not lost. **Do not fold it into this plan.**

**This plan = the technical mechanism** for the concert-cancel path: a `Cancelled` lifecycle state +
the cross-service refund path (B2B workflow → Payment escrow refund) + the cancel action on the B2B
SPAs + integration/E2E coverage.

**NOT in this plan** (separate [LAUNCH_PLAN.md](LAUNCH_PLAN.md) Swim-lane C item — needs solicitor
input): the *cancellation policy matrix* — cancellation fees, cutoff windows, who-eats-the-Stripe-fee,
partial vs full refund per contract type. This feature exposes the *hooks* (a policy result feeds the
refund amount); it hard-codes **full refund, no fee** as the v1 default and leaves the matrix as a
parameter to fill in later.

## Cross-service map (why this is bigger than one module)

`EscrowEntity` lives in **Payment** (adapter service); B2B's Concert module drives it over gRPC via
`Concertable.Payment.Client`. So the refund path must be built end-to-end across the boundary:

```
FE (B2B venue/artist SPA)  →  B2B Concert API (cancel endpoint)
   →  B2B Concert workflow (Cancelled transition + RefundEscrowStep)
      →  Payment.Client IEscrowClient.RefundByBookingId  (gRPC)
         →  Payment Escrow gRPC service  →  EscrowService.RefundByBookingId
            →  Stripe refund/reverse-transfer  +  EscrowEntity.Refund()
```

## Per-contract-type mechanics (the real complexity)

| Contract type | Money flow (from LAUNCH_PLAN §Status) | Refund mechanic |
|---|---|---|
| **FlatFee / VenueHire** | escrow, `OnBehalfOf` (funds held on platform) | Stripe **refund of the charge** (`ChargeId`). Straightforward while `Held`. |
| **DoorSplit / Versus** | `TransferData.Destination` (funds routed to the connected acct at charge) | Refund **with `reverse_transfer`** (claws the transferred share back). Needs its own handling. |
| **Artist checkout (pre-capture Hold)** | PaymentIntent held, not yet captured | **Cancel the PaymentIntent**, not a refund — no money moved yet. |

**v1 escrow-refund scope decision (confirm during Phase 1):** cover the **escrow-holding path
(FlatFee/VenueHire) fully first**; implement destination-charge reversal (DoorSplit/Versus) in the
same phase if the Stripe reverse-transfer path is clean, else split it out. The pre-capture Hold
cancel is a distinct, simpler path (cancel intent) — handle it where a Booked-but-not-captured state
is cancellable.

## Open questions to resolve as we go

- **Which states are cancellable, by whom?** Current `LifecycleState`: `Applied · Rejected ·
  Withdrawn · Accepted · PaymentFailed · Booked · AwaitingSettlement · SettlementFailed · Complete`.
  Cancel-with-refund makes sense from **`Booked`** (escrow `Held`, concert drafted) up to **before
  `Complete`** (settlement paid out). `Accepted`/`PaymentFailed` pre-money = a plain state change, no
  refund. **`Complete`** (already settled/paid) = out of scope for in-app refund (manual/dispute).
  Decide venue-vs-artist permission per state (Phase 3).
- **New state shape:** one `Cancelled` terminal state, or `Cancelled` + a distinct `Refunded`? Lean
  **single `Cancelled`** (the `EscrowStatus.Refunded` on the Payment side records the money fact;
  B2B's lifecycle only needs "this booking is dead").
- **Refund amount:** v1 = full, no fee (policy matrix is the separate Swim-lane C item). Keep the
  amount a parameter so the matrix can populate it later.

## Phases

- [x] **Phase 1 — Payment: escrow refund capability (service-local).**
  - `EscrowService.RefundByBookingIdAsync(bookingId, …)` — resolve escrow by booking, issue the Stripe
    refund (charge refund for OnBehalfOf; `reverse_transfer` for destination charges), call
    `EscrowEntity.Refund(refundId, now)`, persist. Idempotent (re-entry on an already-`Refunded`
    escrow is a no-op success).
  - proto: add `rpc RefundByBookingId(RefundByBookingIdRequest) returns (RefundByBookingIdResponse)`
    to `service Escrow` in `payment.proto`; implement the gRPC handler.
  - **Gate:** Payment builds; `Concertable.Payment.UnitTests` cover `RefundByBookingIdAsync`
    (domain `Refund()` is already tested); carve-payment paths resolve.

- [x] **Phase 2 — Payment.Client: refund adapter.**
  - `IEscrowClient.RefundByBookingIdAsync` + `EscrowClient` impl (mirror `ReleaseByBookingIdAsync`).
    Additive: returns the existing published `RefundResponse` — the `Response`→shared-`Refund` rename is
    deferred to [PAYMENT_DTO_CONSOLIDATION.md](../PAYMENT_DTO_CONSOLIDATION.md) (breaking package change).
    Forward-compat: the `IEscrowClient` mocks (B2B fixtures) implement the new method now so B2B/Customer
    stay green when the new Payment.Client package publishes.
  - **Gate:** solution builds. ✓

- [x] **Phase 3 — B2B: `Cancelled` state + cancel workflow/step.** *(PR1 merged as `9352b8c4`;
  Payment.Client republished at `0.1.0-alpha.0.547`. PR2 branch `Feature/EscrowRefundB2B` = pin bump
  `cb7841ac` + cherry-picked `8863b4f0`. Gate passed: solution builds green; B2B Concert integration
  suite green. Migration re-scaffold was a confirmed **no-op** — appending `LifecycleState.Cancelled`
  + a domain-event `Cancel()` method carries no persisted-schema delta; `ConcertDbContextModelSnapshot`
  unchanged.)*
  - ✅ Added `Cancelled` to `LifecycleState` + `Cancel` to `Trigger`; transition `Booked → Cancelled`
    for **all four** contract types (`WithCancel<TStep>()` in `ConcertWorkflowBuilder`). Scope decision:
    cancel-with-refund window is **`Booked` only** — escrow types (FlatFee/VenueHire) hold money there so
    refund; payout types (DoorSplit/Versus) hold **no** money at `Booked` (verify is a SetupIntent, payout
    is at Finish), so `RefundByBookingIdAsync` is a correct no-op (no escrow row). Deferred: pre-capture
    Hold-cancel from `Accepted`, and cancel from `AwaitingSettlement` (payout in-flight).
  - ✅ `RefundEscrowStep : ICancelStep` → `IEscrowClient.RefundByBookingIdAsync`; wired into all 4 workflows.
  - ✅ `CancelExecutor` + `CancellationDispatcher` + `IConcertWorkflowModule.CancelAsync`;
    `ConcertCancelledDomainEvent` → `ConcertCancelledDomainEventHandler` → `ConcertCancelledEvent`
    (B2B-only for now; Customer/Search/Notification consumers are follow-on, not required by the gate).
  - ✅ Bumped `ConcertablePlatformVersion` `.535 → .547`, cherry-picked `8863b4f0`, build green,
    B2B Concert integration green. (Re-scaffold attempted; produced no schema diff, reverted the churn.)

- [x] **Phase 4 — B2B API: concert-cancel endpoint + auth + HATEOAS action.** *(Done. `POST
  /api/Concert/{id}/cancel` on `ConcertController`, `[HasPermission(VenuePermissions.ApplicationsDecide)]`;
  `ConcertActions.Cancel` on `ConcertDetailsResponse`, gated on `State == Booked` (surfaced via
  `ConcertDetails.State` + the `ToDetails` projection). `MockEscrowClient` gained a `Refunds` tracker.
  4 `ConcertCancelApiTests` (FlatFee/VenueHire refund + Cancelled; DoorSplit no-escrow no-op; artist→403)
  green; full B2B Concert integration 67/67. Note: the cancel action is state-gated + endpoint-secured
  (403 for non-venue); viewer-scoping of the HATEOAS hint is deferred, not required for correctness.)*
  - `POST /api/Concert/{concertId}/cancel` on **`ConcertController`** (already `[TenantPersona(Venue)]`),
    calling the concert-keyed `IConcertWorkflowModule.CancelAsync(concertId)` → `NoContent`. Auth:
    `[HasPermission(VenuePermissions.ApplicationsDecide)]` — cancelling is reversing the venue's booking
    decision, same authority as `accept` (the venue is the escrow payer on FlatFee/VenueHire). Artist-side
    cancel is deferred (see Phase 3 scope note).
  - HATEOAS: a new **`ConcertActions`** block on the concert response with a `cancel` `ActionLink`,
    emitted **only when the booking is in the cancellable `Booked` window** (concert exists + escrow held).
    *Not* `ApplicationActions` — this is a concert action (see Scope). First concert-action record; mirror
    the `ApplicationResponseMapper` conditional-action pattern.
  - **Gate:** build; integration tests (endpoint → `Booked → Cancelled` transition + escrow refund via the
    `MockEscrowClient`, for the escrow-holding types; no-op refund asserted for DoorSplit/Versus).

- [x] **Phase 5 — FE: cancel action on the B2B venue SPA.** *(Done. Venue-only — cancelling is a venue
  decision (Phase 4 auth); artist is deferred, consistent with Phase 3.)*
  - `ConcertActions { cancel? }` + optional `actions?` on the universal `Concert` type (customer-safe);
    `cancelConcert` API + `useCancelConcert` mutation (invalidates `["concert", id]`) beside the existing
    `updateConcert`/`useMyConcert`. `CancelBookingButton` (destructive Button + confirmation `Dialog` +
    `sonner` toast + `isPending` states) lives in the **venue** app; `MyConcertPage` (b2b/shared) gained a
    `renderActions(concert)` slot so the venue injects the button and the artist injects nothing. Rendered
    only when `concert.actions?.cancel` is present (Booked window).
  - **Gate:** all four web builds green (boundary gate) — venue/artist/customer/business ✓.

- [~] **Phase 6 — Full verify + close out.** *(In progress. Integration verified; no migration needed;
  cancel-specific E2E scenarios still to author; plan NOT yet deleted.)*
  - ✅ **Integration**: `ConcertCancelApiTests` cover the cancel→refund chain per contract type against
    the Payment mock — FlatFee/VenueHire (refund fired + `Cancelled`), DoorSplit (no escrow at Booked →
    correct no-op), artist→403. Full B2B Concert integration 67/67.
  - ✅ **Migration**: no re-scaffold needed — nothing since Phase 3 changed the persisted model (Phase 4
    added `ConcertDetails.State`, a read-projection field only; Phase 5 is FE).
  - ✅ **E2E cancel scenarios — authored.** All four pieces built: (a) the `Booked`-with-held-escrow
    booking is **driven** through the live accept→hold→charge flow (no seed — escrow rows aren't
    seedable), mirroring `ConcertDraftTests`; (b) `StripeFixture.GetRefundAsync` (Stripe-refund
    assertion) + `PaymentDb.GetEscrowStatusAsync`/`GetEscrowRefundIdAsync` (escrow-status accessors);
    (c) API E2E `ConcertCancelledTests` (FlatFee + VenueHire: cancel → `Cancelled` + escrow `Refunded`
    + real Stripe refund `succeeded` + cancel action gone), mirroring `ConcertFinishedTests`; (d) a
    cancel scenario in `FlatFeeWorkflow.feature` + `MyConcertPage` page object + step defs (FE
    `CancelBookingButton` gained `data-testid` hooks). Solution + venue web build green. **Not yet run
    locally — gated to the merge queue's full E2E suites (the decision on PR2).**
  - ⬜ **Close-out (do when cancel E2E is green in the merge queue):** `git rm` this plan file; tick the
    🔴 concert-cancellation blocker in [LAUNCH_PLAN.md](LAUNCH_PLAN.md).

## Reference

- `api/Concertable.Payment/src/Concertable.Payment.Domain/EscrowEntity.cs` — `Refund()`, states.
- `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs` — escrow ops.
- `api/Concertable.Payment/src/Concertable.Payment.Client/Protos/payment.proto` — `service Escrow`.
- `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/EscrowClient.cs` — client adapters.
- `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/Lifecycle/LifecycleState.cs`
- `.../Concert.Infrastructure/Services/Workflow/{Workflows,Steps}/` — per-type workflows + steps
  (`ReleaseEscrowFinishStep` is the template for `RefundEscrowStep`).
