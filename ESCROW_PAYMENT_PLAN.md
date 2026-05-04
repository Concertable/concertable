# Escrow-Based Payment

**Branch:** `Feature/EscrowPayment` (off `master`)

## The problem

Today every charge uses Stripe `PaymentIntent` with `transfer_data.destination = <recipient connected account>` (destination charges). Money moves from payer's card → recipient's connected account in a single atomic Stripe call. There is no platform-held window between charge and recipient credit.

That shape forecloses everything an escrow system needs:

| Capability | Today | Why blocked |
|---|---|---|
| Hold venue's funds while artist performs | No | Charge already credited the artist before the concert happens |
| Configurable dispute / cancellation window | No | Once a destination charge clears, refunding it pulls money back from the artist's connected balance — may be empty |
| Clean refund path on dispute / cancellation | No | `RefundTicketAsync` is `NotImplementedException`; no equivalent for settlement charges |
| Partial release | No | The transfer happened atomically with the charge |
| Auditable held-funds balance per booking | No | No row exists between charge and credit |

Off-session settlement charges (DoorSplit / Versus at concert finish) compound the risk: by the time the worker fires the charge, the venue's card may decline (`Feature/ApplyCardVerification` mitigates the worst of this at PM-storage time, but doesn't change settlement-time risk).

## Decision

**Switch to Stripe's separate-charges-and-transfers pattern.** Money lands in the platform's Stripe balance at charge time; a separate `Transfer` to the artist's connected account fires at release time (concert finish + dispute window).

A new `EscrowEntity` in `Payment.Domain` is the system of record for held funds — tracking `Held → Released | Refunded | Disputed`. Concert reads escrow state via `IPaymentModule`; **`BookingEntity` gains no escrow field**, no FK, no nav. Cross-module link is `EscrowEntity.BookingId` only (Payment → Concert, by primitive). This preserves the modular-monolith boundary: Payment owns its data; Concert asks via the facade.

**The TPH (`StandardApplication` / `PrepaidApplication` / `StandardBooking` / `DeferredBooking`) does NOT change.** It encodes "which aggregate's lifetime owns the PaymentMethod handle" — orthogonal to where the resulting charge sits. The PM lifecycle stays exactly as today; only what Stripe does with the charge changes (no `transfer_data.destination`, transfer fired later).

### Trade-offs vs alternatives

| Option | Held-funds window | Concert dates >7 days | Verdict |
|---|---|---|---|
| `capture_method=manual` (auth-and-capture) | Until capture | **Stripe caps auth at ~7 days** | Unworkable — concerts are usually further out |
| Keep destination charges + ad-hoc refunds | None | N/A | Fails the requirement |
| **Separate charges + transfers (chosen)** | Unlimited | Yes | Right primitive for marketplace escrow |
| Stripe Issuing / Treasury managed accounts | Unlimited | Yes | Over-engineered + regulatory overhead for current needs |

### Stripe-account implications

Holding funds in the platform balance has knock-on effects worth confirming with Stripe support / your accountant before code lands:

- **Platform Stripe payout schedule** — currently likely auto-payout; needs to be set to manual or extended so held escrow funds aren't paid out to your bank prematurely.
- **Fee structure** — separate charges + transfers carries different Stripe fees than destination charges (no automatic application fee mechanism — fees come off the charge, transfers are gross to the artist).
- **Dispute liability** — with destination charges, disputes can claw back from the artist's connected balance. With separate transfers, the platform's balance absorbs disputes until reconciled.
- **VAT / tax-point** — the moment of "supply" may shift; check with your accountant.

## What stays untouched

- `ApplicationEntity` TPH (`StandardApplication` / `PrepaidApplication`) — no field changes, no new subtype.
- `BookingEntity` TPH (`StandardBooking` / `DeferredBooking`) — no field changes, no new subtype.
- PM lifecycle on `PrepaidApplication.PaymentMethodId` and `DeferredBooking.PaymentMethodId` — still set at the same points, still consumed by the same workflow services.
- `Feature/ApplyCardVerification` (`VerifyAndVoidAsync` at PM-storage points) — still relevant; verify-then-hold is the natural composition.
- `ICustomerPaymentModule` for ticket purchases — out of scope by default (see Open product decisions).
- **DoorSplit / Versus settlement** — stays on `IManagerPaymentModule.PayAsync` (off-session destination charge at finish). Not escrow, not a regression, not an inconsistency. See next section.

## Why DoorSplit / Versus cannot escrow

It looks like both escrow and deferred-settlement "hold money until finish" — they don't. Three things are easy to conflate:

- **Booking commitment** — artist + venue have agreed; calendar slot locked. Both share this. It's `BookingEntity` lifecycle, not payment.
- **Funds custody** — who is sitting on the actual cash during the window. **Escrow = platform Stripe balance. Deferred = still on payer's bank account.**
- **Charge timing** — when the card actually moves. Escrow = at accept. Deferred = at finish.

|  | Escrow (FlatFee, VenueHire) | Deferred (DoorSplit, Versus) |
|---|---|---|
| Money leaves payer's card at accept? | Yes | No |
| Funds custody during window | Platform | Payer |
| Amount known at hold time | Yes (`contract.Fee` / `HireFee`) | **No — `revenue × pct` (+ guarantee)** |
| Failure risk at finish | None (already captured) | Card decline (mitigated by `VerifyAndVoidAsync`, not eliminated) |
| Refund mechanism | `RefundAsync` + transfer reversal | Just don't fire the PaymentIntent |
| What we persist | `EscrowEntity` + `ChargeId` | `DeferredBooking.PaymentMethodId` (today) |

The killer is the amount. Stripe will not authorize "an unspecified number of pounds." DoorSplit / Versus settlement amounts are computed from `concertRepository.GetTotalRevenueByConcertIdAsync` *at finish*. There is nothing to hold at accept.

A future variant — **partial-escrow Versus** (hold the `guarantee` via escrow at accept, charge the variable door-share at finish) — is a real product option but explicitly out of scope for this branch. Default: Versus stays fully deferred.

## Stage 0 — Settlement Transaction Outbox (precursor)

Restore the two-phase write pattern for `SettlementTransactions` before adding escrow. Today `SettlementTransactionHandler.HandleAsync` writes the row only on `payment_intent.succeeded`, reconstructing every field from Stripe metadata — a workaround for an earlier deadlock under stripe-cli/Docker webhook delivery. Escrow is two-phase by nature (Held → Released), so doing the outbox first means escrow gets the right plumbing for free instead of rewriting the same code twice.

### 0.1 Restore Pending → Complete write

`UpfrontConcertService.InitiateAsync` and `DeferredConcertService.FinishedAsync` create the `SettlementTransactionEntity` row at status `Pending` **before** calling `IManagerPaymentModule.PayAsync`. The webhook handler updates that row to `Complete` (matched by `PaymentIntentId`).

```csharp
public async Task<IAcceptOutcome> InitiateAsync(...)
{
    var booking = await bookingService.CreateStandardAsync(applicationId);

    var transaction = SettlementTransactionEntity.Create(
        fromUserId: payerId,
        toUserId: payeeId,
        paymentIntentId: string.Empty,
        amount: (long)(amount * 100),
        status: TransactionStatus.Pending,
        bookingId: booking.Id);
    await transactionRepository.AddAsync(transaction);
    await transactionRepository.SaveChangesAsync();

    var settlementMetadata = new Dictionary<string, string>
    {
        ["type"] = "settlement",
        ["transactionId"] = transaction.Id.ToString()
    };

    var payment = await managerPaymentModule.PayAsync(payerId, payeeId, amount, settlementMetadata, paymentMethodId, session);
    if (payment.IsFailed) throw new BadRequestException(payment.Errors);

    return new ImmediateAcceptOutcome(payment.Value);
}
```

`SettlementTransactionEntity` needs a method to attach the `PaymentIntentId` post-creation, since the row is written before Stripe responds:

```csharp
public void AttachPaymentIntent(string paymentIntentId)
{
    if (!string.IsNullOrEmpty(PaymentIntentId))
        throw new DomainException("PaymentIntent already attached.");
    PaymentIntentId = paymentIntentId;
}
```

Two options for the attach:
- **(a)** `IManagerPaymentModule.PayAsync` returns the `PaymentIntentId` in `PaymentResponse`; caller attaches + saves.
- **(b)** Webhook handler matches on `metadata["transactionId"]` and updates both `PaymentIntentId` and `Status` together.

Recommend (a) — caller already has the response in hand, fewer round-trips, webhook stays simple.

### 0.2 Drop load-bearing user IDs from Stripe metadata

`fromUserId` / `toUserId` / `amount` / `bookingId` flow through Stripe metadata today as the source of truth in `SettlementTransactionHandler`. Once the local `Pending` row is the source of truth, strip them — keep only `type` + `transactionId` (settlement) or `type` + `concertId` + `quantity` (ticket purchase).

`SettlementTransactionHandler` becomes:

```csharp
public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
{
    var transactionId = int.Parse(@event.Metadata["transactionId"]);
    var transaction = await transactionRepository.GetByIdAsync(transactionId)
        ?? throw new NotFoundException($"Settlement transaction {transactionId} not found");

    if (transaction.Status == TransactionStatus.Complete)
    {
        logger.LogInformation("Settlement {TransactionId} already complete; skipping", transactionId);
        return;
    }

    transaction.Complete();
    await transactionRepository.SaveChangesAsync(ct);
}
```

### 0.3 Add `PaymentFailedHandler`

Stripe fires `payment_intent.payment_failed` on declines. `WebhookProcessor` currently filters to `succeeded` only — extend to publish a sibling `PaymentFailedEvent` and a Concert-side handler that marks the related `BookingEntity` as `PaymentFailed` (the existing domain transition `BookingEntity.FailPayment()` already exists).

### 0.4 Workers reconciliation job for orphaned Pending rows

A `SettlementReconciliationFunction` in `Concertable.Workers` runs hourly, finds `SettlementTransactions` where `Status = Pending AND CreatedAt < UtcNow - 1h`, queries Stripe for the matching PaymentIntent (by `metadata.transactionId`), and updates the local row. Catches webhook delivery failures.

### 0.5 Fix the original deadlock properly

The original two-phase deadlock came from `READ_COMMITTED` isolation + the webhook UPDATE racing the INSERT. Fix at the SQL Server level rather than collapsing the writes:
- Ensure the `Concertable` database has `READ_COMMITTED_SNAPSHOT ON`.
- The webhook handler's lookup is by primary key (single-row point-lock UPDATE), not a range scan.

```sql
ALTER DATABASE Concertable SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
```

### 0.6 Tests

- Integration: existing settlement tests continue to pass (assert `Pending` row exists between accept and webhook, then `Complete` after webhook fires).
- Integration: new `Settlement_ShouldStayPending_WhenWebhookNotFired` — verifies the row exists at `Pending` if the test never invokes the fake webhook.
- Integration: new `Settlement_ShouldFailBooking_WhenPaymentFails` — `payment_intent.payment_failed` → booking status `PaymentFailed`, transaction stays `Pending`.
- Workers unit: `SettlementReconciliationFunction` calls Stripe for orphaned rows, updates status.

## Stage 1 — Stripe-layer escrow primitives

### 1.1 New methods on `IStripePaymentClient`

`IStripePaymentClient` is the thin Stripe SDK wrapper today (one method, `CreatePaymentIntentAsync`). Extend to cover the escrow primitives:

```csharp
public interface IStripePaymentClient
{
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
    Task<Transfer> CreateTransferAsync(TransferCreateOptions options);
    Task<Refund> CreateRefundAsync(RefundCreateOptions options);
    Task<Reversal> CreateTransferReversalAsync(string transferId, TransferReversalCreateOptions options);
}
```

### 1.2 Drop `transfer_data.destination` from the hold path

`StripePaymentIntentClient.ChargeAsync` builds today's destination-charge intent. It splits into two methods on the abstract base:

```csharp
public async Task<Result<PaymentResponse>> HoldAsync(StripeChargeOptions opts)
{
    if (string.IsNullOrEmpty(opts.DestinationStripeId))
        return Result.Fail("Recipient does not have a Stripe account");
    if (await stripeAccountClient.GetAccountStatusAsync(opts.DestinationStripeId) != PayoutAccountStatus.Verified)
        return Result.Fail("Recipient is not eligible for payouts");

    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(opts.Amount * 100),
        Currency = "GBP",
        PaymentMethod = opts.PaymentMethodId,
        Customer = opts.StripeCustomerId,
        Confirm = true,
        PaymentMethodTypes = ["card"],
        ReceiptEmail = opts.ReceiptEmail,
        Metadata = opts.Metadata,
        OnBehalfOf = opts.DestinationStripeId
    };

    Configure(options);

    try
    {
        var intent = await stripeClient.CreatePaymentIntentAsync(options);
        return intent.ToPaymentResult();
    }
    catch (StripeException ex) { return Result.Fail($"Stripe Error: {ex.Message}"); }
}
```

Note `OnBehalfOf` (not `TransferData.Destination`) — keeps the transaction associated with the artist's connected account for fee/regulatory purposes (UK FX, tax-point) but funds settle to the platform balance pending the transfer.

`ChargeAsync` (today's destination-charge path) is **kept** for ticket purchases until / unless the customer ticket flow also moves to escrow.

### 1.3 `ReleaseAsync` and `RefundAsync`

```csharp
public async Task<Result<TransferResponse>> ReleaseAsync(StripeReleaseOptions opts)
{
    try
    {
        var transfer = await stripeClient.CreateTransferAsync(new TransferCreateOptions
        {
            Amount = (long)(opts.Amount * 100),
            Currency = "GBP",
            Destination = opts.DestinationStripeId,
            SourceTransaction = opts.ChargeId,
            Metadata = opts.Metadata
        });
        return new TransferResponse(transfer.Id);
    }
    catch (StripeException ex) { return Result.Fail($"Stripe Error: {ex.Message}"); }
}

public async Task<Result<RefundResponse>> RefundAsync(StripeRefundOptions opts)
{
    try
    {
        if (!string.IsNullOrEmpty(opts.TransferId))
        {
            await stripeClient.CreateTransferReversalAsync(opts.TransferId, new TransferReversalCreateOptions
            {
                Amount = (long)(opts.Amount * 100)
            });
        }

        var refund = await stripeClient.CreateRefundAsync(new RefundCreateOptions
        {
            PaymentIntent = opts.PaymentIntentId,
            Amount = (long)(opts.Amount * 100),
            Reason = opts.Reason,
            Metadata = opts.Metadata
        });
        return new RefundResponse(refund.Id);
    }
    catch (StripeException ex) { return Result.Fail($"Stripe Error: {ex.Message}"); }
}
```

`SourceTransaction = opts.ChargeId` ties the transfer to the original charge for Stripe's reporting / dispute tracking.

### 1.4 Extend `IPaymentManager`

```csharp
internal interface IPaymentManager
{
    Task<Result<PaymentResponse>> ChargeAsync(ChargeRequest request, CancellationToken ct = default);
    Task<Result<HoldResponse>> HoldAsync(HoldRequest request, CancellationToken ct = default);
    Task<Result<TransferResponse>> ReleaseAsync(ReleaseRequest request, CancellationToken ct = default);
    Task<Result<RefundResponse>> RefundAsync(RefundRequest request, CancellationToken ct = default);
}
```

`ChargeAsync` stays for ticket purchases (destination charges, immediate transfer). The escrow trio is additive.

## Stage 2 — `EscrowEntity` + repository

### 2.1 Domain entity in `Payment.Domain`

```csharp
public class EscrowEntity : IIdEntity, IAuditable
{
    private EscrowEntity() { }

    private EscrowEntity(int bookingId, Guid fromUserId, Guid toUserId, long amount, string chargeId, DateTime releaseAt)
    {
        BookingId = bookingId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        ChargeId = chargeId;
        ReleaseAt = releaseAt;
        Status = EscrowStatus.Held;
    }

    public int Id { get; private set; }
    public int BookingId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public long Amount { get; private set; }
    public EscrowStatus Status { get; private set; }
    public string ChargeId { get; private set; } = null!;
    public string? TransferId { get; private set; }
    public DateTime ReleaseAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public static EscrowEntity Create(int bookingId, Guid fromUserId, Guid toUserId, long amount, string chargeId, DateTime releaseAt)
        => new(bookingId, fromUserId, toUserId, amount, chargeId, releaseAt);

    public void Release(string transferId, DateTime now)
    {
        if (Status != EscrowStatus.Held)
            throw new DomainException("Only held escrow can be released.");
        TransferId = transferId;
        ReleasedAt = now;
        Status = EscrowStatus.Released;
    }

    public void Refund(DateTime now)
    {
        if (Status is not (EscrowStatus.Held or EscrowStatus.Disputed))
            throw new DomainException("Only held or disputed escrow can be refunded.");
        RefundedAt = now;
        Status = EscrowStatus.Refunded;
    }

    public void MarkDisputed()
    {
        if (Status != EscrowStatus.Held)
            throw new DomainException("Only held escrow can be disputed.");
        Status = EscrowStatus.Disputed;
    }
}

public enum EscrowStatus { Held, Released, Refunded, Disputed }
```

### 2.2 Repository + EF config

`IEscrowRepository` (Payment.Application, internal): `GetByIdAsync`, `GetByBookingIdAsync`, `GetReleaseDueAsync(DateTime asOf)`, `AddAsync`, `SaveChangesAsync`.

`EscrowEntityConfiguration` in `Payment.Infrastructure/Data/Configurations/`:
- `payment` schema (matches `PaymentDbContext` convention).
- Index on `BookingId` (single-escrow-per-booking — unique).
- Index on `(Status, ReleaseAt)` for the auto-release scanner.
- `ChargeId` indexed for webhook lookup.

`PaymentDbContext` gains `DbSet<EscrowEntity> Escrows`. `ApplicationDbContext` adds `ExcludeFromMigrations` mirror so AppDb scaffolding ignores it. `IReadDbContext` gains `IQueryable<EscrowEntity> Escrows` (read-only projection used by Concert-side facade impl).

### 2.3 Migration

Per `CLAUDE.md`: run `./initial-migrations.ps1` from `api/` to nuke and re-scaffold every module's `InitialCreate`. Don't add additive migrations.

## Stage 3 — Cross-module facade

### 3.1 `IPaymentModule` extensions

`IPaymentModule` (Payment.Contracts) gains escrow verbs. These are the cross-module surface other modules see — Stripe-shape stays inside `Payment.Infrastructure`.

```csharp
public interface IPaymentModule
{
    Task<Result<EscrowResponse>> HoldAsync(
        Guid payerId,
        Guid payeeId,
        decimal amount,
        IDictionary<string, string> metadata,
        string paymentMethodId,
        PaymentSession session,
        int bookingId,
        DateTime releaseAt,
        CancellationToken ct = default);

    Task<Result<TransferResponse>> ReleaseAsync(
        int escrowId,
        CancellationToken ct = default);

    Task<Result<RefundResponse>> RefundAsync(
        int escrowId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken ct = default);

    Task<EscrowDto?> GetEscrowByBookingIdAsync(
        int bookingId,
        CancellationToken ct = default);
}

public record EscrowResponse(int EscrowId, string ChargeId, EscrowStatus Status);
public record TransferResponse(string TransferId);
public record RefundResponse(string RefundId);
public record EscrowDto(int Id, int BookingId, decimal Amount, EscrowStatus Status, DateTime ReleaseAt, DateTime? ReleasedAt);
```

`HoldAsync` is the orchestrator: ensures payer's Stripe customer exists (mirrors `ManagerPaymentModule.PayAsync`), calls `IPaymentManager.HoldAsync`, on success creates the `EscrowEntity` (status `Held`), returns the `EscrowResponse`.

`GetEscrowByBookingIdAsync` is the **read facade Concert calls** (per the modular-monolith decision: Booking has no escrow field; Concert asks Payment).

### 3.2 `IManagerPaymentModule` deprecation path

`IManagerPaymentModule.PayAsync` (immediate destination charge) stays for now — used by FlatFee accept (which moves to escrow in Stage 4) and for any non-escrow flows we want to keep. Flag for review at Stage 4 close-out: if every call site has migrated to `IPaymentModule.HoldAsync`, delete `IManagerPaymentModule.PayAsync` and the destination-charge path in `StripePaymentIntentClient`.

## Stage 4 — Workflow strategy diffs

The strategies move `PayAsync(immediate destination charge)` → `HoldAsync` at accept and add `ReleaseAsync` at finish. Per-strategy table:

| Strategy | Today at Accept | Tomorrow at Accept | Today at Finish | Tomorrow at Finish |
|---|---|---|---|---|
| **FlatFee** | `PayAsync(VM→Artist, OnSession)` immediate | `HoldAsync(VM→Artist, OnSession, bookingId)` | `bookingService.CompleteByConcertIdAsync` only | + `IPaymentModule.ReleaseAsync(escrowId)` |
| **DoorSplit** | `VerifyAndVoidAsync` only; `DeferredBooking` keeps PM | unchanged | Compute share, `PayAsync(VM→Artist, OffSession)` immediate | Compute share, `HoldAsync(...)` then `ReleaseAsync` *(no held window — they collapse into one call)* |
| **Versus** | Same as DoorSplit | unchanged | Same as DoorSplit + guarantee | Same as DoorSplit + guarantee |
| **VenueHire** | `PayAsync(Artist→VM, OffSession)` immediate via `UpfrontConcertService.InitiateAsync` | `HoldAsync(Artist→VM, OffSession, bookingId)` | `bookingService.CompleteByConcertIdAsync` only | + `IPaymentModule.ReleaseAsync(escrowId)` |

### 4.1 `UpfrontConcertService.InitiateAsync` rewrite

Used by FlatFee and VenueHire today. The signature gains `int bookingId` already in scope (line 35) and a `releaseAt` (concert end + dispute window):

```csharp
public async Task<IAcceptOutcome> InitiateAsync(
    int applicationId, Guid payerId, Guid payeeId, decimal amount,
    string paymentMethodId, PaymentSession session, DateTime releaseAt)
{
    var result = await applicationValidator.CanAcceptAsync(applicationId);
    if (result.IsFailed) throw new BadRequestException(result.Errors);

    var booking = await bookingService.CreateStandardAsync(applicationId);

    var metadata = new Dictionary<string, string> { ["type"] = "settlement" };

    var hold = await paymentModule.HoldAsync(
        payerId, payeeId, amount, metadata, paymentMethodId, session, booking.Id, releaseAt);
    if (hold.IsFailed) throw new BadRequestException(hold.Errors);

    return new ImmediateAcceptOutcome(new PaymentResponse(hold.Value.ChargeId));
}
```

`releaseAt` comes from the strategy: typically `concert.EndAt + DisputeWindow` (see Open product decisions).

### 4.2 `UpfrontConcertService.FinishedAsync` gains release

```csharp
public async Task FinishedAsync(int concertId)
{
    await bookingService.CompleteByConcertIdAsync(concertId);

    var booking = await bookingRepository.GetByConcertIdAsync(concertId)
        ?? throw new NotFoundException("Booking not found");
    var escrow = await paymentModule.GetEscrowByBookingIdAsync(booking.Id)
        ?? throw new NotFoundException($"Escrow not found for booking {booking.Id}");

    if (escrow.Status == EscrowStatus.Held && DateTime.UtcNow >= escrow.ReleaseAt)
    {
        var release = await paymentModule.ReleaseAsync(escrow.Id);
        if (release.IsFailed) throw new BadRequestException(release.Errors);
    }
}
```

If `ReleaseAt` is in the future (dispute window not elapsed), defer to the auto-release scanner — don't release at concert finish.

### 4.3 `DeferredConcertService.FinishedAsync` collapses Hold + Release

For DoorSplit / Versus there's no held window — money is computed at finish and moved at finish. Two choices:

- **(a)** Keep using `IManagerPaymentModule.PayAsync` (destination charge) for these — they have no "held" semantics.
- **(b)** Use `HoldAsync` + `ReleaseAsync` back-to-back for consistency, accepting the extra Stripe call.

Recommend (a) — these contracts genuinely don't escrow (the artist hasn't pre-funded; the venue charges based on door takings *after* the concert), so forcing them through the escrow path is shape-matching for its own sake. Keep `IManagerPaymentModule.PayAsync` alive specifically for this case.

### 4.4 Auto-release scanner (Workers)

`EscrowAutoReleaseFunction` in `Concertable.Workers` runs hourly:

```csharp
public async Task ExecuteAsync(CancellationToken ct)
{
    var due = await escrowReadRepository.GetReleaseDueAsync(timeProvider.GetUtcNow().DateTime, ct);
    foreach (var escrowId in due)
    {
        var result = await paymentModule.ReleaseAsync(escrowId, ct);
        if (result.IsFailed)
            logger.LogWarning("Auto-release failed for escrow {EscrowId}: {Errors}", escrowId, result.Errors);
    }
}
```

This is the **safety net** for FlatFee / VenueHire releases that the synchronous `FinishedAsync` path didn't catch (e.g. concert finished but dispute window hadn't elapsed yet).

## Stage 5 — Webhook events

### 5.1 New events in `Payment.Contracts/Events/`

```csharp
public record EscrowReleasedEvent(int EscrowId, string TransferId) : IIntegrationEvent;
public record EscrowRefundedEvent(int EscrowId, string RefundId, long Amount) : IIntegrationEvent;
public record EscrowDisputedEvent(int EscrowId, string DisputeId) : IIntegrationEvent;
```

### 5.2 `WebhookProcessor` extensions

Today filters to `PaymentIntent` + `succeeded` only. Extend to handle:
- `transfer.created` / `transfer.paid` → publish `EscrowReleasedEvent` (matched by `transfer.source_transaction` → escrow's `ChargeId`).
- `charge.refunded` → publish `EscrowRefundedEvent` (matched by `refund.payment_intent` → escrow's `ChargeId`).
- `charge.dispute.created` → publish `EscrowDisputedEvent`.

### 5.3 Payment-side handlers

`EscrowReleasedHandler`, `EscrowRefundedHandler`, `EscrowDisputedHandler` in `Payment.Infrastructure/Events/` — each calls the matching `EscrowEntity` method (`Release` / `Refund` / `MarkDisputed`) and `SaveChangesAsync`.

### 5.4 Concert-side handlers (optional — only if Concert needs to react)

If concert state needs to follow escrow state (e.g. mark booking `Cancelled` when escrow refunds), add handlers in `Concert.Infrastructure/Events/` subscribing to the same events. Otherwise no Concert-side changes needed; Concert reads escrow on demand via `GetEscrowByBookingIdAsync`.

## Stage 6 — Read surface for the FE

### 6.1 Manager dashboard "held funds"

New endpoint `GET /api/payments/escrow/held` on `PaymentController` (`Payment.Api`):
- Returns escrows where `Status = Held` and the current user is `FromUserId` or `ToUserId`.
- Backed by `IReadDbContext.Escrows` projection.
- DTO mirrors `EscrowDto` + booking summary (concert name, date, amount).

### 6.2 Booking detail page

When the FE loads a booking, surface escrow state alongside the booking shape. Either:
- **(a)** Booking response includes a nested `escrow` object (Concert calls `IPaymentModule.GetEscrowByBookingIdAsync` server-side and includes in response).
- **(b)** FE makes a separate call to `/api/payments/escrow/by-booking/{id}`.

Recommend (a) — single round-trip, FE doesn't need to know about Payment's URL surface.

## Stage 7 — Tests

### Unit (Payment.UnitTests)
- `EscrowEntity.Release_ShouldFail_WhenNotHeld`
- `EscrowEntity.Refund_ShouldFail_WhenAlreadyReleased`
- `EscrowEntity.MarkDisputed_ShouldFail_WhenNotHeld`
- `PaymentModule.HoldAsync_ShouldCreateEscrow_WhenStripeSucceeds`
- `PaymentModule.HoldAsync_ShouldNotCreateEscrow_WhenStripeFails` — atomicity
- `PaymentModule.ReleaseAsync_ShouldThrow_WhenEscrowNotHeld`

### Integration (Payment.IntegrationTests)
- `EscrowFlow_ShouldHoldThenRelease_OnUpfrontFlatFee`
- `EscrowFlow_ShouldHoldThenRelease_OnVenueHire`
- `EscrowFlow_ShouldRefund_WhenBookingCancelled`
- `EscrowFlow_ShouldNotRelease_BeforeReleaseAt` — auto-release scanner respects window
- `Webhook_TransferPaid_ShouldMarkEscrowReleased`
- `Webhook_ChargeRefunded_ShouldMarkEscrowRefunded`

### Integration (Concert.IntegrationTests)
- Existing FlatFee / VenueHire accept tests assert escrow row created at `Held` (was: no row).
- Existing finish tests assert escrow → `Released` after webhook (was: no row tracking).

### Workers unit
- `EscrowAutoReleaseFunction_ShouldRelease_WhenReleaseAtElapsed`
- `EscrowAutoReleaseFunction_ShouldSkip_WhenReleaseAtFuture`
- `SettlementReconciliationFunction_ShouldComplete_WhenStripeReturnsSucceeded` (Stage 0)

### Manual
- Stripe CLI `stripe listen --forward-to localhost:5001/api/webhooks/stripe`.
- VenueHire happy path: artist applies → venue accepts → escrow row at `Held`, charge in platform balance, no transfer yet → trigger concert finish → transfer fires → escrow at `Released`, artist's connected balance credited.
- VenueHire refund path: cancel booking before release → escrow at `Refunded`, artist's connected balance untouched, customer refunded from platform balance.

## Stage 8 — Frontend touch-up

- Manager dashboard: new "Held funds" card showing total + count, links to detail page.
- Booking detail: show escrow state badge (`Held` / `Released` / `Refunded` / `Disputed`), release date when `Held`.
- Cancel-booking flow: warn that funds will be refunded (with 5-10 business day timing copy).
- Copy fix on apply / accept success: "Card was charged and funds are held until the concert is performed" instead of today's "card was charged and artist was paid".

## Open product decisions

These are not architectural — they're product calls that need answers before code lands. Bringing them up here so they're not litigated mid-implementation.

1. **Dispute window length** — release on concert finish? +24h? +7 days? Different per contract?
2. **Customer ticket purchases** — escrow them, or keep destination charges (today) so VMs get paid immediately?
3. **DoorSplit / Versus settlement** — keep as immediate destination charge (Stage 4 recommendation), or force through escrow for shape symmetry?
4. **Cancellation policy** — full refund? Partial (platform takes a fee)? Time-bound (free cancel >7 days out)?
5. **Dispute resolution UX** — admin tool to manually release / refund disputed escrows, or automatic on Stripe dispute webhook?
6. **VenueHire artist refund** — if venue cancels, artist should be refunded the hire fee — is the artist also entitled to a "wasted preparation" payout?

## Out of scope (this branch)

- Stripe Treasury / Issuing migration (over-engineered; revisit if marketplace volume warrants).
- Multi-currency escrow (GBP only for now, matches today).
- Escrow on customer ticket purchases (decision item 2 above; default keep-as-is).
- Per-ticket refund flow (`RefundTicketAsync` is still `NotImplementedException` — separate ticket-flow PR).
- Admin UI for manual escrow ops (decision item 5; can ship behind feature flag in a follow-up).
- 3DS step-up on hold creation (off-session SCA-exempt for stored cards; if 3DS triggered, hold fails — same UX as today's settlement charge failure).

## Acceptance

1. Stage 0 in: `Settle_ShouldStayPending_WhenWebhookNotFired` passes; existing 46 Concert integration tests still green.
2. New escrow tables exist in `payment` schema after `./initial-migrations.ps1` re-scaffold.
3. VenueHire happy path: apply → accept → DB row in `Escrows` at `Held` with non-null `ChargeId`, null `TransferId`. Stripe dashboard shows charge in platform balance, no transfer.
4. Concert finish (or auto-release scanner): same row updates to `Released`, `TransferId` populated. Stripe dashboard shows transfer to artist's connected account.
5. Refund path: `IPaymentModule.RefundAsync` → row at `Refunded`, Stripe shows refund + transfer reversal.
6. FE manager dashboard shows held-funds card with correct total.
7. Build green; all suites pass.
