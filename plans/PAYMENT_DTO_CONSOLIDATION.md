# Payment DTO Consolidation — one shared shape, kill the `Response` suffix

**Goal:** replace Payment's duplicated service/client DTOs with a **single shared record per shape**
in `Payment.Contracts`, and drop the `Response` suffix from the C# DTOs (it's wrong: `Result<T>` is
already the service wrapper, and `Response` is reserved for the HTTP API layer). Proto message names
stay `*Response` — that's the native gRPC RPC vocabulary and it's wire-only.

## Why this exists (the smell)

Today the same escrow shapes exist **three times**:

| Layer | Type (today) | Notes |
|---|---|---|
| Service DTO | `Payment.Application/DTOs/ServiceDtos.cs` — `EscrowDeposit`/`Transfer`/`Refund`/`PaymentOutcome` | already de-`Response`d (Phase 1 of Feature/EscrowRefund); **internal** to Payment |
| Proto message | `Payment.Client/Protos/payment.proto` — `EscrowResponse`/`TransferResponse`/`PaymentResponse`/`RefundResponse` | generated wire type — **stays `*Response`** |
| Client DTO | `Payment.Client` — `EscrowResponse`/`TransferResponse`/`PaymentResponse`/`RefundResponse` | **public**, consumed by B2B + Customer |

The service DTO and client DTO are hand-duplicated and coincide in shape. They should be **one** type.

## Target design

One record per shape in **`Payment.Contracts`** (the only project both `Payment.Application` and
`Payment.Client` reference):

- `EscrowDeposit(int EscrowId, string ChargeId, EscrowStatus Status, string? ClientSecret = null)`
- `Transfer(string TransferId)`
- `Refund(string RefundId)`
- `PaymentOutcome { bool RequiresAction; string? ClientSecret; string? TransactionId; }`

Then:
- `IEscrowService`/`IPaymentManager` (service) return the Contracts types.
- `IEscrowClient`/`IManagerPaymentClient`/`ICustomerPaymentClient` (client) return the Contracts types.
- Both the **server-side** mappers (`Payment.Infrastructure/Grpc/*Mappers`) and the **client-side**
  mappers (`Payment.Client/Adapters/*Mappers`) map proto ⇄ the shared Contracts DTO.
- Delete the `Payment.Application/DTOs` copies and the `Payment.Client/*Response.cs` copies.

## Why it can NOT be one PR (the constraint that bit us)

`Payment.Contracts` and `Payment.Client` are **published NuGet packages** (org GitHub Packages feed,
lockstep MinVer via `$(ConcertablePlatformVersion)`). B2B and Customer consume them by version — they
compile against the **published package**, not the source in the same solution. Proven: a consumer's
`project.assets.json` resolves `Concertable.Payment.Client/0.1.0-alpha.0.536` with `"type": "package"`.

A **return-type change has no back-compat shim** (you can't have a method return both the old and new
type). So this is a breaking package change and must go **expand/contract** across merges — publish the
new shape first, migrate consumers against the published package, then delete the old.

## Steps (expand/contract)

- [ ] **Step 1 — introduce shared DTOs in `Payment.Contracts`; migrate Payment's own service + client.**
  - Add the four records to `Payment.Contracts`.
  - Point `Payment.Application`, `Payment.Infrastructure`, and `Payment.Client` at them; update both
    mapper sets. Delete `Payment.Application/DTOs/ServiceDtos.cs` copies and `Payment.Client/*Response.cs`.
  - **This changes the published contract** (client methods now return `Contracts.Transfer` etc.).
  - **Gotcha:** `Transfer`/`Refund` collide with the Stripe SDK (`Stripe.Transfer`/`Stripe.Refund`) in
    any file with `using Stripe;` (e.g. `MockEscrowClient`). Add `using Transfer = Concertable.Payment.Contracts.Transfer;`
    (and `Refund`) aliases in those files.
  - **Gate:** `Concertable.Payment.slnx` builds green (Payment is self-contained here — all source).
    The root `Concertable.slnx` / CI will be red on B2B+Customer until Step 2, because they still pin the
    old package — expected. Merge order: this publishes new `Payment.Contracts` + `Payment.Client`.

- [ ] **Step 2 — migrate B2B + Customer consumers to the shared types.**
  - Bump each service's `ConcertablePlatformVersion` pin to the version published by Step 1.
  - Replace `*Response` usages: `TicketPayment : PaymentOutcome`; the escrow/manager/customer mocks;
    any other named usages (most consumer code uses `var` and is unaffected).
  - **Gate:** `Concertable.B2B.slnx` + `Concertable.Customer.slnx` build; integration tests
    (`integration-debug`). Root `Concertable.slnx` green again.

- [ ] **Step 3 — remove the old names.**
  - Delete any leftover `*Response` shims and the `MA0053` suppression that references
    `Payment.Client.PaymentResponse` (retarget/remove).
  - **Gate:** full `Concertable.slnx` green; `git rm` this plan file in the commit that closes Step 3.

## Notes / decisions carried forward

- **Proto stays `*Response`.** Only the C# DTOs change.
- **Naming = Stripe-aligned** (`Transfer`/`Refund`/`EscrowDeposit`/`PaymentOutcome`) — decided on
  Feature/EscrowRefund. Accept the Stripe-SDK name collision (handled by `using` aliases where needed).
- The service-layer rename (Phase 1 of Feature/EscrowRefund) already did the `Application/DTOs` half of
  Step 1; Step 1 here relocates those into `Payment.Contracts` and deletes the `Application` copies.
