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

- **PR1 = Payment** (`Feature/EscrowRefund`, Phases 1+2): already committed, builds green, merge first.
  On merge to master, CI publishes a new `Concertable.Payment.Client` version containing
  `RefundByBookingIdAsync`.
- **PR2 = B2B** (Phases 3–6): the Phase 3 code is written and **parked on branch
  `Feature/EscrowRefundB2B`** (commit `8863b4f0`, does not build until PR1 publishes). To resume:
  1. After PR1 merges, note the published `Concertable.Payment.Client` version (CI bumps it).
  2. Branch off updated `master`; bump `ConcertablePlatformVersion` in
     `api/Concertable.B2B/Directory.Packages.props` to that version.
  3. Rebase/cherry-pick `Feature/EscrowRefundB2B` onto it (`git cherry-pick 8863b4f0`).
  4. `dotnet build api/Concertable.slnx` → `integration-debug` → `./initial-migrations.ps1`.

## Scope

**This plan = the technical mechanism**: a `Cancelled` lifecycle state + the cross-service refund
path (B2B workflow → Payment escrow refund) + the cancel action on the B2B SPAs + integration/E2E
coverage.

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

- [~] **Phase 3 — B2B: `Cancelled` state + cancel workflow/step.** *(code written, parked on
  `Feature/EscrowRefundB2B` @ `8863b4f0` — blocked on PR1 publishing Payment.Client; see Delivery split.)*
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
  - ⬜ **Still to do on resume (after PR1 publishes):** bump the pin + rebase (Delivery split), then
    `./initial-migrations.ps1` from `api/`, then **Gate:** build + B2B Concert `integration-debug`.

- [ ] **Phase 4 — B2B API: cancel endpoint + auth + HATEOAS action.**
  - Cancel endpoint on the Concert/Application controller; `[Authorize]` the right role(s) per state;
    add `cancel` to the per-role `ApplicationActions` vocabulary (venue/artist) the FE already models.
  - **Gate:** build; integration tests (endpoint → transition + refund via the Payment mock/real).

- [ ] **Phase 5 — FE: cancel action on B2B venue + artist SPAs.**
  - Cancel button + confirmation modal (consequences: refund issued, booking dead), wired to the
    endpoint via the HATEOAS `cancel` action; loading/empty/error states.
  - **Gate:** `npm -w @concertable/web-venue run build` + `npm -w @concertable/web-artist run build`.

- [ ] **Phase 6 — Full verify + close out.**
  - Integration: B2B cancel → Payment refund chain end-to-end (Stripe test mode: refund lands, escrow
    `Refunded`, lifecycle `Cancelled`) for each covered contract type.
  - **E2E (this is a payments path → clears the "run E2E" bar):** full cancel→refund flow on the
    Aspire stack via `e2e-api-debug` (+ `e2e-ui-debug` for the SPA action).
  - Final `./initial-migrations.ps1` if the model shifted since Phase 3.
  - **`git rm` this plan file in the commit that closes Phase 6.**
  - Update [LAUNCH_PLAN.md](LAUNCH_PLAN.md): tick the 🔴 "Cancellation + escrow refund" blocker.

## Reference

- `api/Concertable.Payment/src/Concertable.Payment.Domain/EscrowEntity.cs` — `Refund()`, states.
- `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs` — escrow ops.
- `api/Concertable.Payment/src/Concertable.Payment.Client/Protos/payment.proto` — `service Escrow`.
- `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/EscrowClient.cs` — client adapters.
- `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/Lifecycle/LifecycleState.cs`
- `.../Concert.Infrastructure/Services/Workflow/{Workflows,Steps}/` — per-type workflows + steps
  (`ReleaseEscrowFinishStep` is the template for `RefundEscrowStep`).
