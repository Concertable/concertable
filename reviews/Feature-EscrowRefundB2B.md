# Code review — Feature/EscrowRefundB2B

**Reviewed up to commit:** `829a795746f8189751fac2191f6a2da3d612121a`  _(2026-07-02)_

> This first pass reviews **PR1 (Payment)** — already merged as #75 (`9352b8c4`), the merge-base of
> this branch. Range reviewed: `260b2fb9..fd3d8d23` (7 commits, the merged Payment-side diff).
> Reviewed at the user's request to catch smells that could still be corrected on this branch.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **BUG1 — MEDIUM — Correctness / API contract** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:204`
  `RefundByBookingIdAsync` has **asymmetric semantics** vs its sibling `ReleaseByBookingIdAsync`
  (same file, line 146). Release treats any non-`Held` escrow as a **benign no-op** — `Result.Ok(null)`
  — so a booking-lifecycle caller can invoke it blindly without knowing escrow state (that is the whole
  point of the `ByBookingId` convenience method). Refund only no-ops on `Refunded`; for any other
  non-refundable status it delegates to `RefundAsync`, which **hard-fails** — `RefundAsync` (line 176)
  returns `Result.Fail("… cannot refund")` for `Pending` / `Failed`. That failure becomes a gRPC
  `FailedPrecondition` (`EscrowGrpcService.RefundByBookingId`) which the B2B `EscrowClient` surfaces as
  `Result.Fail`. Consequence for **this branch's cancel flow**: cancelling a booking whose escrow never
  advanced past `Pending` (hold initiated, webhook not yet confirmed) or is `Failed` will fail the whole
  refund/cancel instead of no-op'ing. Decide the intended contract: if "cancel is safe to call
  regardless of escrow state" (the Release precedent), `RefundByBookingIdAsync` should treat
  `Pending`/`Failed` as `Result.Ok(null)` too, not propagate a hard failure. Correctable on this branch
  since it's the B2B consumer side. (Confidence it's a real asymmetry: high; whether it bites depends on
  how the B2B cancel handler treats a `FailedPrecondition` from refund.)

## Notes (not findings — already tracked)

- **DTO triple-duplication + Stripe name-collision aliases.** The rename leaves `Transfer`/`Refund`
  colliding with `Stripe.Transfer`/`Stripe.Refund`, requiring scattered `using` aliases (StripeApiClient,
  StripeTransferClient, PaymentManager, FakeStripePaymentIntentClient), and the service DTO / proto /
  client DTO still exist as three hand-duplicated shapes. This is **explicitly captured** as deferred
  expand/contract work in `plans/PAYMENT_DTO_CONSOLIDATION.md` (can't be one PR — breaking published
  `Payment.Contracts`/`Payment.Client` package change). Not a new finding; do not re-flag.

## Clean

- **Microservice isolation (Lens B):** clean. All changes are within Payment + its `Payment.Client`
  adapter package; B2B test fixtures implement `IEscrowClient` (adapter-service Client contract) — a
  permitted cross-boundary reference. No data-service→data-service coupling introduced.
- **Seeding (Lens D):** untouched.
- **C# conventions (Lens E):** clean — new log messages are source-generated `[LoggerMessage]`
  (`NoEscrowToRefundForBooking`, `EscrowAlreadyRefunded`); braces correct; no primary-ctor captures;
  4 new unit tests cover no-escrow / already-refunded / held / released(destination-charge) paths.

## Incremental review — 2026-07-02 (PR2, the B2B branch diff)

> Range reviewed: `9352b8c..829a7957` (7 commits) — the actual `Feature/EscrowRefundB2B` work
> (B2B `Cancelled` state, cancel workflow/step/endpoint, HATEOAS action, FE cancel button). The
> first pass above covered only the merged PR1 Payment diff; this section covers the B2B side.

### Findings

- [x] **FE1 — LOW — Correctness (user-facing copy)** — `app/web/b2b/venue/src/features/concerts/components/CancelBookingButton.tsx:38` (dialog) & `:23` (toast)
  _Fixed: copy softened to "any payment held is refunded in full" (dialog + toast), true for all four contract types. All four web builds green._
  The confirm dialog says *"The artist's payment is refunded in full and the concert is removed"* and
  the success toast says *"Booking cancelled and payment refunded"* — unconditionally, for every
  contract type. But the cancel window is `Booked`-only, and at `Booked` only **FlatFee / VenueHire**
  hold escrow; **DoorSplit / Versus** hold none (verify is a SetupIntent, payout is at Finish), so
  `RefundEscrowStep` → `RefundByBookingIdAsync` is a documented no-op there (Payment returns
  `Ok(null)`, `EscrowService.cs:211`). For a DoorSplit/Versus booking the copy tells the venue money
  was refunded when nothing was held. The button is offered for all four types (gated only on
  `State == Booked`), so the mismatch is reachable. Minor (no data/behaviour bug — cancel still
  succeeds), but the messaging is inaccurate for half the contract types.

### Notes (not findings)

- **BUG1 (PR1) does not bite the cancel flow — mitigated by the `Booked`-only gate.** The MEDIUM
  finding above warns `RefundByBookingIdAsync` hard-fails for a `Pending`/`Failed` escrow. Cancel is
  reachable only from `Booked` (`ConcertWorkflowBuilder.cs:65` `Add(Booked, Cancel, Cancelled)`), and
  `Booked` is entered only via `EscrowPaymentSucceeded` (⟹ escrow `Held`) or `VerifyPaymentSucceeded`
  (⟹ no escrow row) — never with escrow in `Pending`/`Failed`. So `RefundEscrowStep` always sees
  `Held` (refunds) or absent (`Ok(null)` no-op); the hard-fail branch is unreachable here. Keep BUG1
  open for the Payment contract itself, but it is not a defect in this branch's cancel path.
- **New `ConcertCancelledEvent` has no consumer yet** — intentional and documented in
  `plans/b2b/ESCROW_REFUND.md` (Customer/Search/Notification consumers are explicit follow-on work,
  not required by this gate). Consequence to track in the follow-on: a cancelled concert is not yet
  removed from the Customer marketplace / Search read models. Not flagged — deferred by design.

### Clean (B2B diff)

- **Correctness (Lens A):** `CancelExecutor` mirrors `FinishExecutor` exactly (load concert →
  transition Application under `Trigger.Cancel` → run the workflow step → raise `concert.Cancel()`;
  external escrow call before the single `SaveChangesAsync`, idempotent refund). `ConcertCancelledDomainEventHandler`
  is a `IPreCommitDomainEventHandler` publishing via `IBus`, identical to the `ConcertPosted`/`ConcertChanged`
  handlers. FE query-key invalidation (`["concert", id]`) matches `useConcertQuery`.
- **Microservice isolation (Lens B):** clean — all B2B-internal; the new integration event lives in
  `Concert.Contracts`; escrow refund goes through the `Payment.Client` adapter (permitted). No
  data-service→data-service coupling.
- **Module boundaries (Lens C):** clean — controller calls the `IConcertWorkflowModule` facade; the
  facade delegates to the dispatcher/executor; no EF inlined in the facade.
- **Seeding (Lens D):** untouched.
- **C# conventions (Lens E):** clean — `FailedToCancelConcert` is source-generated `[LoggerMessage]`;
  explicit ctors + `private readonly` (no primary-ctor captures); single-statement `if` without
  braces; workflow classes converted to `public { get; }` auto-props assigned `this.X = param` per the
  dependency-holder convention; `LifecycleState.Cancelled` + `Trigger.Cancel` appended (no migration —
  no persisted-schema delta, confirmed in the plan).

