# Payment metadata keys — kill the magic strings

Replace the bare string literals used as **payment-event metadata dictionary keys** (and the stray
`type` *values*) with constants, the Stripe webhook **event-type** strings with the Stripe SDK's own
`EventTypes.*`, and give the metadata reads an honest type + a single read helper.

Constant surfaces (both in `Concertable.Payment.Contracts`, alongside the existing `TransactionTypes`):

- **`PaymentMetadataKeys`** — the dictionary *keys*: `Type`, `ConcertId`, `FromUserId`,
  `FromUserEmail`, `ToUserId`, `Amount`, `Currency`, `Quantity`, `PaymentMethodId`, `BookingId`,
  `EscrowId`, `ApplicationId`, `VenueManagerId`, `OpportunityId`.
- **`TransactionTypes`** — extended with the inline `type`-value literals: `EscrowRelease`,
  `EscrowRefund`, `ApplicationApply`, `ApplicationAccept`.
- Stripe event strings (`"payment_intent.succeeded"` etc.) → `Stripe.EventTypes.PaymentIntent*`.

## Why this is multi-PR (the carve)

`PaymentMetadataKeys` lives in the **published** `Concertable.Payment.Contracts` package. B2B and
Customer consume it by `<ConcertablePlatformVersion>` pin, not project reference — so they can't see
the new class until it republishes and a `chore/platform-sync-*` PR bumps their pin. Adding it is
purely additive (non-breaking), so the sync PR goes green **without** forcing the consumer sweep —
hence this plan, so phase 2 isn't forgotten.

## Phase 1 — Payment side (DONE, this PR)

- [x] Add `PaymentMetadataKeys`; extend `TransactionTypes` with the four `type` values.
- [x] Sweep everything referencing `Payment.Contracts` **by project**: `Payment.Infrastructure`
  (`WebhookProcessor`, `PaymentManager`, `EscrowService`, `ManagerPaymentService`,
  `CustomerPaymentService`, `Events/*`, `StripeAccountClient`, `StripeHoldClient`,
  `FakeStripePaymentIntentClient`) and `Payment.Seed` (`E2EStripeAccountClient`).
- [x] `WebhookProcessor` + `FakeStripePaymentIntentClient` use `Stripe.EventTypes.*`.
- [x] **Metadata params narrowed `IDictionary` → `IReadOnlyDictionary`** across everything
  Payment-internal: `IStripeAccountClient` (×4), `IStripeHoldClient`, `ICustomerPaymentService`,
  `IManagerPaymentService`, all impls (real/Fake/E2E), the `Request` records' `Metadata`, the gRPC
  command records, and `DictionaryExtensions.Merge`/`With`. Nothing ever mutated them, so
  `IDictionary` was advertising a capability nothing used. Free: proto `MapField<string,string>`
  implements `IReadOnlyDictionary`, so the gRPC seam needed **no** `.ToDictionary()` copy.
- [x] **Read helpers** on the existing generic `DictionaryExtensions` (beside `Merge`/`With`) —
  deliberately *not* payment-named and *not* in Contracts, because "get a value out of a
  `Dictionary<string,string>`" is a generic dictionary operation, not a payment concept:
  - `GetValue(key)` → the raw string, throws naming the key if absent/empty.
  - `GetValueAs<T>(key)` → parsed via `IParsable<T>` + invariant culture, throws naming the key *and*
    the bad value. Mirrors the existing `GrpcRequestParsers.ParseOrThrow<T>` convention (which can't be
    reused — it throws `RpcException`, right only at the gRPC boundary).
  - `[assembly: InternalsVisibleTo("Concertable.Payment.Seed")]` added so `E2EStripeAccountClient`
    can use them. Note `GetValue` collides with `ConfigurationBinder.GetValue<T>` where
    `Microsoft.Extensions.Configuration` is imported — resolves by receiver type once
    `using Concertable.Payment.Infrastructure;` is in scope.
- [x] Defaulting fixes: `PaymentTransactionHandler` / `PaymentFailureDispatcher` read `type` via
  `GetValue` (was `GetValueOrDefault(…, string.Empty)`, which masked a missing required key as `""`);
  `StripeAccountClient` / `E2EStripeAccountClient` read `Amount`/`Currency` via `GetValueAs<long>` /
  `GetValue` (`Currency` was `?: "GBP"`). A throw here is **not** an unlogged crash —
  `AzureServiceBusReceiver` catches every handler exception, logs it via
  `FailedProcessingEvent(messageType, ex)`, and abandons-with-backoff → retry/DLQ. The anti-pattern is
  `TryParse ? x : <default>`, which would process a malformed payment event silently.
- [x] `TicketPurchaseCompletionTests` (Customer): `IScoped<IIntegrationEventHandler<PaymentSucceededEvent>>`
  replaces the hand-rolled `CreateScope()`/`GetRequiredService`. **Its metadata keys stay literal** until
  Customer's pin includes `PaymentMetadataKeys` (phase 2).
- Gate: `dotnet build api/Concertable.slnx` = 0 errors; `Concertable.Payment.UnitTests` 30/30.

## Phase 2 — B2B + Customer sweep (BLOCKED on the platform-sync pin bump)

**Precondition:** phase 1 merged → `Payment.Contracts` republished → the `chore/platform-sync-*` PR
bumping `<ConcertablePlatformVersion>` in B2B and Customer merged. Only then do these compile.

Swap literals for `PaymentMetadataKeys.*` / `TransactionTypes.*` in:

- **Customer.Ticket.Infrastructure** — `Services/TicketService.cs` (both metadata dicts),
  `Services/Payment/TicketPaymentProcessor.cs`, `Services/Payment/TicketPaymentFailedProcessor.cs`.
- **Customer test** — `TicketPurchaseCompletionTests` metadata dict.
- **B2B.Concert.Infrastructure** — `Services/Workflow/Steps/{Verify,Setup,Hold}CheckoutStep.cs`
  (`Setup`/`Hold` `type` values → `TransactionTypes.ApplicationApply` / `ApplicationAccept`);
  `Services/Payment/{Verify,VerifyFailed,TicketSale,Settlement,SettlementFailed,Escrow,EscrowFailed}PaymentProcessor.cs`.
- **B2B tests/fixtures** — `Concert.UnitTests/Workflow/{Verify,Setup,Hold}CheckoutStepTests.cs` (swap the
  expected value literals too; `Setup`/`Hold` need `using Concertable.Payment.Contracts;`),
  `IntegrationTests.Fixtures/ApiFixture.cs`, `Mocks/MockManagerPaymentClient.cs`, `Mocks/MockEscrowClient.cs`.

B2B and Customer are separate services and can't see Payment.Infrastructure's internal helpers, so each
gets its own small `GetValue`/`GetValueAs` in its own `DictionaryExtensions` (do **not** promote them to
a shared package — they're generic dictionary utils, not a payment contract).

Reads to convert while there:
- `bookingId` / `concertId` / `applicationId` → `meta.GetValueAs<int>(…)`; `fromUserId` →
  `GetValueAs<Guid>(…)` — replacing `int.Parse(meta[...])` / `Guid.Parse(meta[...])`, so a bad value
  names the key instead of throwing a bare `FormatException`.
- **`TicketSaleProcessor` quantity `?: 1`** — genuinely optional on that path (a ticket sale defaults to
  one ticket), so the explicit default is legitimate, not a masked failure. Leave it.

## Follow-up (separate, breaking)

`ICustomerPaymentClient` / `IManagerPaymentClient` in the **published `Payment.Client`** package still
take `IDictionary<string, string>`. Narrowing them to `IReadOnlyDictionary` is a breaking package change
(B2B/Customer call them, and their test fixtures implement them), so it needs its own expand/contract PR
once the platform version is settled.

Done = every metadata-key literal gone (grep returns only the const definitions), build green, affected
Customer/B2B unit + integration tests green. Then `git rm` this plan.
