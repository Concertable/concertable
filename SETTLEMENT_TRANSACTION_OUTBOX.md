# Settlement Transaction — Restore Two-Phase / Outbox Write

## Status

**Pending.** Picked up *after* the E2E dev-seeder PR (`E2EDevSeedingRefactor`) merges.
The current implementation in `SettlementTransactionHandler.HandleAsync` is a known
workaround, not the intended design.

## Background — what's there now

`SettlementTransactionHandler.HandleAsync` (in
`api/Modules/Payment/Concertable.Payment.Infrastructure/Events/SettlementTransactionHandler.cs`)
currently writes a `SettlementTransactions` row **only** in the Stripe webhook,
pulling all fields out of `@event.Metadata`:

```csharp
public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
{
    var meta = @event.Metadata;
    var bookingId = int.Parse(meta["bookingId"]);
    var amountPence = long.TryParse(meta.GetValueOrDefault("amount"), out var a) ? a : 0;

    await transactionService.LogAsync(new SettlementTransactionDto
    {
        BookingId  = bookingId,
        FromUserId = Guid.Parse(meta["fromUserId"]),
        ToUserId   = Guid.Parse(meta["toUserId"]),
        PaymentIntentId = @event.TransactionId,
        Amount     = amountPence,
        Status     = TransactionStatus.Complete,
        CreatedAt  = timeProvider.GetUtcNow().DateTime
    });
}
```

This replaced the original handler:

```csharp
public Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    => transactionService.CompleteAsync(@event.TransactionId);
```

## Why it was changed

Under E2E with stripe-cli forwarding webhooks from Docker, the original
two-write flow would deadlock against itself:

1. **Settle-time (synchronous):** `INSERT SettlementTransaction (Status=Pending)`,
   commit.
2. **Stripe call** (sub-second).
3. **Webhook arrives:** `UPDATE SettlementTransaction SET Status=Complete WHERE …`.

Stripe-CLI in Docker delivers the webhook fast enough to interleave with the
INSERT's commit, producing
`Process ID N deadlocked on lock resources … chosen as deadlock victim`.
Whoever hit this collapsed the writes into one (webhook-only) to make the
deadlock go away.

## Why the current shape is wrong

Single-write-on-webhook gives up properties a financial system needs:

- **No row for failed payments.** Stripe declines never raise
  `payment_intent.succeeded` → no row is written. You cannot reconcile
  "attempts vs successes" off your own DB.
- **No row when webhooks are missed.** Deploy, network blip, Stripe outage —
  the money may have moved without any local trace until a customer
  complains.
- **No pre-Stripe audit row.** "Did the API call Stripe at 11:42:03?" requires
  reading app logs, not the DB.
- **No idempotency hook.** A pre-existing `Pending` row keyed on something
  stable (`(BookingId, IdempotencyKey)`) is the natural place to reject
  double-charges. Currently relies entirely on Stripe's idempotency layer.
- **Settlement metadata is now load-bearing.** Upstream services
  (`UpfrontConcertService`, `DeferredConcertService`) pack `fromUserId` /
  `toUserId` / `amount` into Stripe metadata only because the webhook depends
  on them. Domain data flowing through a third party's metadata bag is
  fragile (size limits, schema drift, GDPR).

## Target shape (outbox / two-phase)

1. **Pre-Stripe write (synchronous, own transaction):**
   `transactionService.InitiateAsync(new SettlementTransactionDto { …,
   Status = Pending })` → INSERT, commit *before* calling Stripe. Returns
   the row's id.
2. **Call Stripe.**
3. **On `paymentIntent.succeeded` webhook:**
   `transactionService.CompleteAsync(@event.TransactionId)` (back to a
   one-liner) — `UPDATE` the existing row to `Complete` and stamp the
   real Stripe response.
4. **On `paymentIntent.payment_failed` webhook:** `FailAsync(...)` →
   `UPDATE` to `Failed` with the failure code.
5. **Background reconciliation job (Workers project):** sweep `Pending`
   rows older than threshold (e.g. 15 min), call
   `paymentIntents.retrieve(...)` against Stripe, mark `Complete` /
   `Failed` / `Unknown_RequiresInvestigation`.

## Fixing the deadlock properly

The deadlock is real but solvable without abandoning the two-phase design.
Options, in preference order:

1. **Commit the INSERT in its own scope before calling Stripe.** Verify no
   ambient `TransactionScope` / `IUnitOfWork` is holding the connection
   open across the Stripe HTTP call. Today `IUnitOfWorkBehavior` is in
   play in some flows — check it isn't wrapping `PayAsync`.
2. **Enable `READ_COMMITTED_SNAPSHOT` on the SQL Server database** (or
   `ALLOW_SNAPSHOT_ISOLATION`). Default isolation is the usual cause of
   the INSERT-vs-UPDATE deadlock; row-versioning lets the UPDATE proceed
   without blocking on the INSERT's lock window. Apply via migration.
3. **Webhook UPDATE must target a primary-key or unique-index match.**
   `WHERE PaymentIntentId = @id` against an index. Point-locks don't
   deadlock with point-locks. Verify
   `IX_SettlementTransactions_PaymentIntentId` exists and is used.
4. **Webhook handler should be tolerant of "row not yet visible".** If
   the UPDATE finds zero rows, requeue the event for retry rather than
   failing or skipping. Stripe will retry on its own anyway, but
   defence-in-depth helps if Stripe ever stops retrying.

## Concrete steps (when picked up)

1. `ITransactionService`: split `LogAsync` into
   `InitiateAsync(dto) -> int` (Pending) and `CompleteAsync(intentId)` /
   `FailAsync(intentId, reason)` (UPDATE by `PaymentIntentId`).
   Keep idempotency: `CompleteAsync` is a no-op on already-Complete rows.
2. `UpfrontConcertService.InitiateAsync` and
   `DeferredConcertService.FinishedAsync`: call `InitiateAsync` *before*
   `paymentFlow.PayAsync`. After the Stripe call returns, stash the
   `PaymentIntentId` on the row (could be a separate `LinkAsync` step or
   just include it in `InitiateAsync` as nullable + UPDATE post-Stripe).
3. Drop `fromUserId` / `toUserId` / `amount` from Stripe metadata —
   they're now in the local row, no need to round-trip them. Keep
   `bookingId` in metadata so the webhook can resolve which row to
   complete *only if* `PaymentIntentId` matching alone is insufficient.
4. `SettlementTransactionHandler.HandleAsync` returns to the original
   one-liner: `transactionService.CompleteAsync(@event.TransactionId)`.
5. Add a `PaymentFailedHandler` for `paymentIntent.payment_failed` and
   wire to `FailAsync`.
6. Migration: enable `READ_COMMITTED_SNAPSHOT` on the application DB. Add
   index `IX_SettlementTransactions_PaymentIntentId` if missing.
7. Workers: add `SettlementReconciliationJob` running on a 5-minute
   schedule that sweeps `Pending` rows older than 15 min and reconciles
   against Stripe.
8. E2E tests should still pass without change — `/e2e/finish` still
   returns the synchronous `TransactionId` from `DeferredFinishOutcome`,
   and the post-webhook `Complete` status is observable via existing
   polling.
9. Drop the `Microsoft.Extensions.DependencyInjection` /
   `TimeProvider` deps from `SettlementTransactionHandler` once it's
   back to a one-liner — unused.

## Risks / things to watch

- **Stripe retry storms** — webhook `Complete` must be idempotent on
  re-delivery. Either guard with a status check or use a unique constraint
  + swallow the duplicate.
- **Reconciliation job vs. webhook race** — both can mark a row
  `Complete` simultaneously. Same idempotency guard handles it.
- **GDPR** — pulling `fromUserId` / `toUserId` out of Stripe metadata is
  a quiet improvement for data-minimisation; mention in the PR.
- **DB enabling RCSI** is a one-way door for some DR replication setups.
  Confirm with whoever owns the SQL infra.

## Files in scope (when work begins)

- `api/Modules/Payment/Concertable.Payment.Application/Interfaces/ITransactionService.cs`
- `api/Modules/Payment/Concertable.Payment.Infrastructure/Services/TransactionService.cs`
- `api/Modules/Payment/Concertable.Payment.Infrastructure/Events/SettlementTransactionHandler.cs`
- `api/Modules/Payment/Concertable.Payment.Infrastructure/Events/PaymentFailedHandler.cs` (new)
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Workflow/UpfrontConcertService.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Workflow/DeferredConcertService.cs`
- `api/Concertable.Workers/SettlementReconciliationJob.cs` (new)
- New migration enabling RCSI + `IX_SettlementTransactions_PaymentIntentId`.
