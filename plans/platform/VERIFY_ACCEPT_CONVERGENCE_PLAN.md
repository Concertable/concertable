# Verify/Accept convergence — fix the booking race behind the flaky "a draft concert is created" E2E

**Status:** ✅ Phase 1 landed on `Bug/DoorSplitE2ETimeout` (PR #237). Phases 2 & 3 remain — separate
concerns, their own branches off `origin/main`.

> **Lifecycle-ownership constraint (2026-08-16):** the approved module split must preserve this durable
> two-signal join while removing the combined Application lifecycle state. Pre-accept verification may
> remain immutable evidence on Application because Booking does not yet exist; Booking owns payment
> state after acceptance. Do not recreate a shared process row or cross-module workflow to keep the race fix.

## The bug (confirmed, with evidence)

Accepting a **verified-payment** deal (`DealType.DoorSplit` and `DealType.Versus` — everything wired
with `WithVerifiedPayment()`) has **two independent async inputs that must both complete before a
concert is booked:**

1. **The venue's `POST /api/Application/{id}/accept`** — transitions the application `Applied → Accepted`
   and, in the same transaction, creates the **booking**, registers the card, issues the contract
   (`AcceptExecutor.ExecuteAsync`).
2. **Stripe's async confirmation that the card is chargeable** — the `payment_intent.amount_capturable_updated`
   webhook → `payment-web` publishes `PaymentSucceededEvent` (metadata `Type=Verify`) over ASB → B2B's
   `VerifyPaymentProcessor` → `VerifyExecutor` transitions `Accepted → Booked` and runs the **Book** step
   (`CreateConcertDraftStep` → `ConcertDraftService` → `ConcertNotifier` SignalR `ConcertDraftCreated`).

**Book is wired to fire only off input (2), and (2) hard-fails if (1) hasn't landed yet.** The webhook
is triggered by the browser confirming the PaymentIntent, which happens *just before* the `/accept`
POST is even sent (`StripePaymentForm` calls `onSuccess` → `handleAccept` → `/accept`). So the webhook
and the accept race, and the webhook frequently wins:

- App still `Applied` when the verify event arrives → `LifecycleStateMachine.Next(Applied, VerifyPaymentSucceeded)`
  throws `ConflictException: Cannot VerifyPaymentSucceeded from Applied` (no such transition in
  `WithVerifiedPayment`).
- Or app just reached `Accepted` but the booking row isn't committed/visible yet → `VerifyExecutor`'s
  effect throws `NotFoundException: Booking not found`.

Either way the ASB receiver (`AzureServiceBusReceiver.AbandonWithBackoffAsync`) retries with
**exponential backoff, `min(2^(deliveryCount-1), 30)`s**. The system *does* self-heal — but the backoff
oversleeps, so the draft is created (and `ConcertDraftCreated` pushed) **tens of seconds late**, past the
E2E's 60s `WaitForURLAsync` and past any reasonable user's patience. The SPA hangs on a single
fire-and-forget SignalR push and never navigates.

**Evidence — natural CI failure** (run `30372466717`, `e2e-ui-tests`, branch `Bug/DoorSplitE2ETimeout`,
scenario *"Venue manager books artist on a versus deal"*, plain valid card, no 3DS): the uploaded
`e2e-diagnostics.log` shows **6× `Cannot VerifyPaymentSucceeded from Applied`** and
`NotFoundException: Booking … not found`, with the failing scenario's draft finally
`Creating concert draft for booking 48` at **15:40:47** — after the 60s wait had already expired. Draft
created *late*, not created-on-time-with-a-lost-push. A frontend poll alone therefore cannot fix it.

Reproduced deterministically by forcing the ordering (a temporary `Task.Delay` in the accept effect so
the webhook always wins): unfixed → 60s timeout with the same `ConflictException` backoff signature.

## Why it only surfaced recently

Two independent re-architectures collided:

- **Workflow rebuilt onto strict lifecycle state machines** (`7d0788b9 "Rebuild concert workflow on
  contract lifecycle state machines"`) — an out-of-order trigger now *throws* (`ConflictException`)
  instead of being a soft no-op.
- **ASB cutover added exponential-backoff-with-dead-lettering** (`b37d6f46`, `92706ba9`) — retrying the
  early event went from an instant re-poll to a 1→2→4→8→16→30s sleep.

Individually invisible; together they make a latent, always-present race both **fatal** (throws) and
**slow to heal** (backoff), so it trips the 60s gate under CI load. The `WithVerifiedPayment` failure
path (`(Applied, VerifyPaymentFailed) → Applied`, a silent no-op) has the same latent race — a verify
*failure* arriving before accept is currently **dropped**.

## The fix: a durable join (rendezvous), not a retry-as-a-wait

This is the durable equivalent of `Task.WhenAll` for two signals that cross process/request/transaction
boundaries and can arrive seconds apart (or straddle a restart). You cannot hold the HTTP request or the
message handler open waiting for the other — that's the "block the consumer / hold the lock" anti-pattern
the current backoff is badly emulating. Instead **persist each signal's arrival and let whichever lands
second drive Book.** Order becomes irrelevant; no throw, no backoff, no timing dependence.

### Shape

- **Persist the payment outcome per application, not per booking.** When the webhook wins the race the
  booking doesn't exist yet, so the fact must live on something that exists early. Record a nullable
  payment-verification outcome (`None` / `Verified` / `Failed`, plus the Stripe `TransactionId` for
  audit/idempotency) on the **application** (`ApplicationEntity`), written idempotently.
- **One idempotent convergence operation — `TryBook(applicationId)`** — that both writers call *after*
  committing their own half. In its own transaction it loads the application (with a concurrency token),
  and **only if** state == `Accepted` **and** payment == `Verified` **and** not already `Booked`, it runs
  the Book step (transition `Accepted → Booked`). Otherwise it no-ops.
- **`VerifyPaymentProcessor` / `VerifyExecutor` (success):** record `Verified` on the application
  (idempotent), then call `TryBook`. Never throw for "app not `Accepted` yet" — early arrival records and
  returns cleanly (message completes, no ASB retry).
- **`AcceptExecutor`:** after committing `Accepted` + booking, call `TryBook`.
- **Failure path (symmetric):** verify-failed records `Failed`; if `Accepted` → `PaymentFailed`
  transition + notify (today's behaviour); if `Applied` → record, and let the accept side observe
  `Failed` and route to `PaymentFailed` instead of booking. Kills the silent-drop bug too.
- **`EscrowPayment` deals (FlatFee/VenueHire):** the escrow capture is *initiated by* the accept step, so
  the succeeded event can't precede accept — no race there today. Leave `EscrowExecutor` as-is; do **not**
  spread the convergence there speculatively. (If a future change makes escrow client-initiated, revisit.)

### The hazard to get right: lost update

`TryBook` must be called **after each writer commits its own half**, and must **read committed state**,
so the writer that lands *second* always sees both halves and books. If both halves commit near-
simultaneously both may call `TryBook` seeing `{Accepted, Verified}`; the `Accepted → Booked` transition
under an optimistic-concurrency token (row version on the application) lets exactly one win and the other
no-op. Do **not** implement `TryBook` as an unguarded check-then-act in the writer's own transaction
using a pre-commit snapshot — that reintroduces the lost update this design exists to remove. (The
existing inbox dedup on `VerifyPaymentProcessor` still guards against duplicate *webhook* delivery; it is
not a substitute for the `TryBook` idempotency guard.)

## Phases

Each phase is independently shippable and ends with targeted local verification plus exact-head PR CI
green (see `plans/AGENTS.md`). Model changes end with `./initial-migrations.ps1` from `api/` (never
additive).

### ✅ Phase 1 — the join (THE fix; unblocks the flake) — DONE
Landed as the durable join. Final design (deltas from the sketch above are deliberate — see the commit
message for the full rationale):
- `ApplicationEntity` carries `PaymentVerification` (`None`/`Verified`/`Failed`) + `PaymentTransactionId`.
- Convergence is `IVerifyExecutor.ConvergeAfterAcceptAsync` + the record-then-converge entry points; the
  early webhook now **records and returns cleanly** (no throw → no ASB backoff) and books only from a
  booking-pending state.
- **No RowVersion.** The double-draft hazard is already closed by the pre-existing 1:1 unique index on
  `Concert.BookingId`; the two writers touch disjoint columns (`State` vs `PaymentVerification`), so EF's
  modified-columns-only UPDATE means no lost update. The accept-side converge swallows the concurrent-race
  `ConflictException` / duplicate-key.
- Tenancy: converge runs in each writer's **own** scope (accept = venue request scope, webhook = host
  message scope) — never a fresh `IScoped` (the fail-closed trap); `Concert`/read-models are unfiltered so
  both scopes can read/write the draft.
- **Gate met:** build green; Concert unit 66/66; Application DoorSplit+Versus (incl. new webhook-first &
  webhook-fail-first join tests) 19/19; Contract 16/16; FlatFee+VenueHire+Cancel+Withdraw 38/38. UI E2E
  (DoorSplit-3DS + Versus "a draft concert is created") runs in the **merge queue**, not locally.

### Phase 2 — frontend resilience (defense-in-depth; separate PR)
Even with Phase 1, the SPA hangs navigation on a **single fire-and-forget SignalR push**, and
`useCheckoutFlow`'s own internal timeout (`app/shared/src/features/concerts/hooks/useCheckoutFlow.ts`,
default 30s) is shorter than the 60s expectation. A genuinely dropped push still strands the user
forever. Add a **reconciling fallback**: alongside the push subscription, poll the existing
`GET /api/Concert/application/{applicationId}` (`ConcertController.GetDetailsByApplicationIdAsync`,
404 until the draft exists) until it returns the concert id, then navigate. This makes the flow robust
to a lost/late push — it is **not** the root fix (it can't rescue a draft that isn't created yet, which
is why Phase 1 comes first).

### Phase 3 — messaging retry policy (system-wide latent fix; separate PR)
`AzureServiceBusReceiver`'s exponential-backoff-to-30s-on-every-throw is the wrong policy for
"event arrived before the state it needs" — a *transient/not-ready* condition distinct from a genuine
handler fault. Phase 1 removes this specific path from that trap, but the next out-of-order event across
the system will hit it again. Give the transport a first-class notion of a transient/not-ready fault
that retries promptly (short, bounded) while genuine faults keep exponential backoff — done deliberately,
not bolted on. (An earlier throwaway attempt threaded a `bool` through duplicated catch blocks + a new
exception type + per-processor catches — do it cleanly this time: single catch, one classification
point, no per-handler ceremony.)

## Out of scope / notes
- Do **not** bump the 60s Playwright timeout, `@quarantine`, or re-run-until-green.
- Do **not** widen `WithVerifiedPayment` to allow `(Applied, VerifyPaymentSucceeded)` — that would skip
  `Accepted` and book without a booking. The join, not a new transition, is the fix.
- Migrations are free here (no live data); re-scaffold, don't hand-write additive.
