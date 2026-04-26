# Payment Webhook Refactor — COMPLETE

Payment is a stateless money-movement service. It knows whether a payment is initiated by a
customer or a manager — that is the extent of its business awareness. It has no knowledge of
concerts, tickets, bookings, or any other domain concept.

The webhook flow violated this: Payment's handlers read concert-specific metadata keys, owned a
`WebhookType` enum with values `Concert` and `Settlement`, and called directly into Concert via
`ITicketPaymentModule` and `IConcertWorkflowModule`. Payment was orchestrating Concert. That was
backwards.

---

## Implemented flow

```
Concert calls ICustomerPaymentModule.PayAsync(amount, metadata: { type, concertId, quantity }, ...)
    ↓
Payment merges in identity fields: fromUserId, fromUserEmail, toUserId, amount
Payment stores the full metadata dict on the Stripe PaymentIntent (opaque to Payment)
    ↓
Stripe webhook fires
    ↓
Payment.WebhookProcessor deduplicates + publishes PaymentSucceededEvent { TransactionId, Metadata }
    ↓
Payment.PaymentTransactionHandler logs/completes the transaction record (Payment-internal)
Concert.PaymentSucceededEventHandler reads metadata["type"], routes to ticket issuance or settlement
```

Dependency is strictly one-way: Concert → Payment. Payment never references Concert.Contracts.

---

## What was created

- `Payment.Contracts/Events/PaymentSucceededEvent.cs` — `record PaymentSucceededEvent(string TransactionId, IReadOnlyDictionary<string, string> Metadata) : IIntegrationEvent`
- `Payment.Infrastructure/Events/PaymentTransactionHandler.cs` — handles `PaymentSucceededEvent`; on `type=settlement` calls `transactionService.CompleteAsync`; otherwise logs a new `TicketTransactionDto`. Payment's internal bookkeeping stays Payment-side.
- `Payment.Infrastructure/DictionaryExtensions.cs` — `Merge(this Dictionary<string, string> seed, IDictionary<string, string>? extra)` extension; identity fields seeded first so they cannot be overridden by caller metadata (`TryAdd` semantics).
- `Concert.Infrastructure/Events/PaymentSucceededEventHandler.cs` — handles `PaymentSucceededEvent`; routes on `metadata["type"]`: `"concert"` → `ITicketService.CompleteAsync` + notification; `"settlement"` → `IConcertWorkflowModule.SettleAsync`.

## What was changed

- `ICustomerPaymentModule.PayAsync` — dropped `referenceId`/`count`, added `IDictionary<string, string>? metadata`
- `CustomerPaymentModule.PayAsync` — seeds identity dict (`fromUserId`, `fromUserEmail`, `toUserId`, `amount`) then `.Merge(metadata)`; no hardcoded concert keys
- `IManagerPaymentModule.PayAsync` — same signature change; both `OnSession` and `OffSession` impls updated
- `WebhookProcessor` — removed `IWebhookStrategyFactory` injection; now publishes `PaymentSucceededEvent` after dedup
- `TicketService.PurchaseAsync` — builds `{ type, concertId, quantity }` dict and passes it to `PayAsync`
- `DeferredConcertService.FinishedAsync` + `UpfrontConcertService.InitiateAsync` — build `{ type, bookingId }` dict and pass it to `PayAsync`
- `Payment.Infrastructure.csproj` — removed `Concert.Contracts` project reference
- `Payment.Application/AssemblyInfo.cs` — updated IVT comments to reflect current usage
- `Concert.Infrastructure/GlobalUsings.cs` — removed stale `Payment.Application.Interfaces.Webhook` global using

## What was deleted

- `Payment.Infrastructure/Services/Webhook/TicketWebhookHandler.cs`
- `Payment.Infrastructure/Services/Webhook/SettlementWebhookHandler.cs`
- `Payment.Infrastructure/Factories/WebhookStrategyFactory.cs`
- `Payment.Application/Interfaces/Webhook/IWebhookStrategyFactory.cs`
- `Payment.Application/Interfaces/Webhook/IWebhookStrategy.cs`
- `Payment.Application/Interfaces/Webhook/ITicketWebhookStrategy.cs`
- `Payment.Application/Interfaces/Webhook/ISettlementWebhookStrategy.cs`
- `Payment.Domain/WebhookType.cs`
- `Concert.Contracts/ITicketPaymentModule.cs`
- `Concert.Infrastructure/TicketPaymentModule.cs`
- `Concert.Application/Interfaces/ITicketPaymentDispatcher.cs` (dead interface, never implemented)

---

## Design notes

**`metadata` parameter type is `IDictionary<string, string>?`** on the public interfaces. Callers pass
`new Dictionary<string, string>` directly. The `.Merge()` extension accepts `IDictionary` and
returns a `Dictionary<string, string>`, which satisfies Stripe's `Dictionary<string, string>` requirement
on `TransactionRequest.Metadata` without any cast. `TransactionRequest.Metadata` stays `Dictionary`
because it is a Stripe-boundary object.

**Transaction logging stays Payment-side** via `PaymentTransactionHandler`. The `TicketTransactionEntity`
still carries a `ConcertId` field — that is pre-existing coupling in the Payment domain model, not
introduced here. Removing it is a future concern (replace with a generic reference ID).

**`fromUserEmail` in merged metadata** eliminates the `ICustomerModule.GetCustomerAsync` call that
`TicketPaymentModule` used to need at webhook time. The email is captured at purchase time and
forwarded through Stripe's metadata.
