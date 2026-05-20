# Payment Service — Architecture Plan

> **Step 14** of [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md).
> Companion to [MICROSERVICE_COMMUNICATION.md](MICROSERVICE_COMMUNICATION.md) and
> [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md).

---

## What this service is

Payment is a shared **adapter service** — all Stripe communication is isolated here. B2B
and Customer call it synchronously for interactive payment flows (card sessions, payouts,
escrow). `Payment.Workers` processes async integration events (user provisioning,
transaction recording, failure handling). No other service touches Stripe directly; PCI
scope is limited to this service.

---

## Project layout

All projects live under `Concertable.Payment/` at the repository root, replacing the old
`Modules/Payment/` location. Solution: `Concertable.Payment/Concertable.Payment.slnx`.

| Project | SDK | Role |
|---|---|---|
| `Concertable.Payment.Contracts` | classlib | Facade interfaces, DTOs, `payment.proto` |
| `Concertable.Payment.Domain` | classlib | Domain entities, domain events |
| `Concertable.Payment.Application` | classlib | Application service interfaces, service logic |
| `Concertable.Payment.Infrastructure` | classlib | EF (`PaymentDb`), Stripe clients, event handlers |
| `Concertable.Payment.Api` | classlib | Controllers — `WebhookController`, `StripeAccountController`, `TransactionController` |
| `Concertable.Payment.Client` | classlib | gRPC client adapters — consumed by B2B, Customer, Workers |
| `Concertable.Payment.Web` | `Microsoft.NET.Sdk.Web` | gRPC host + controller host |
| `Concertable.Payment.Workers` | `Microsoft.NET.Sdk.Worker` | ASB subscriber host |
| `Concertable.Payment.UnitTests` | xUnit | Unit tests |

---

## gRPC surface — `payment.proto`

Single `.proto` file, 3 `service` blocks, `csharp_namespace = "Concertable.Payment.Grpc"`.
The `.proto` lives in `Concertable.Payment.Contracts/Protos/payment.proto` — the single
source of truth. `Payment.Web` references it `GrpcServices="Server"`;
`Payment.Client` references it `GrpcServices="Client"`.

### ManagerPayment — mirrors `IManagerPaymentModule`
| RPC | Parameters | Returns |
|---|---|---|
| `Pay` | payerId, payeeId, amount, paymentMethodId, session, bookingId | `PaymentResponse` |
| `CreateSetupSession` | payerId, metadata | `CheckoutSession` |
| `CreateVerifySession` | payerId, metadata | `CheckoutSession` |
| `CreateHoldSession` | payerId, amount, metadata | `CheckoutSession` |
| `FindHeldIntent` | payerId, applicationId | `string` |

### CustomerPayment — mirrors `ICustomerPaymentModule`
| RPC | Parameters | Returns |
|---|---|---|
| `Pay` | payerId, payeeId, amount, metadata, paymentMethodId | `PaymentResponse` |
| `CreatePaymentSession` | payerId, metadata | `CheckoutSession` |

### Escrow — mirrors `IEscrowModule`
| RPC | Parameters | Returns |
|---|---|---|
| `Deposit` | payerId, payeeId, amount, paymentMethodId, session, bookingId | `EscrowResponse` |
| `Capture` | payerId, payeeId, amount, paymentIntentId, bookingId | `EscrowResponse` |
| `ReleaseByBookingId` | bookingId | `TransferResponse` (optional) |

### Type mapping

| C# | Proto |
|---|---|
| `decimal` (money) | `string` (decimal-as-string, lossless) |
| `Guid` | `string` |
| `IDictionary<string,string>` metadata | `map<string,string>` |
| `int` | `int32` |
| `DateTime?` | `google.protobuf.Timestamp` (optional) |
| `PaymentSession` enum | proto `enum` (`OnSession = 0`) |
| `EscrowStatus` enum | proto `enum` (`Pending = 0`) |
| `Result<T>` (IsFailed) | server throws `RpcException(StatusCode.FailedPrecondition, errors)` |
| `Result<T>` (client) | catch `RpcException` → `Result.Fail(message)` |

---

## HTTP surface — `Payment.Web`

Hosted alongside gRPC in the same Kestrel app (`Http1AndHttp2`). Existing controllers
moved from `Payment.Api` as-is. No Minimal APIs; no new endpoints in Step 14.

| Controller | Route | Consumer | Auth |
|---|---|---|---|
| `WebhookController` | `POST api/webhook` | Stripe (external) | None (raw body + Stripe-Signature) |
| `StripeAccountController` | `api/stripeaccount` | B2B SPA | JwtBearer |
| `TransactionController` | `api/transaction` | B2B / Customer SPA | JwtBearer |

`StripeAccountController` — `IUserModule` dependency removed; email resolved via
`IPayoutAccountRepository` → `PayoutAccountEntity.Email`.

---

## Auth

| Surface | Policy | How |
|---|---|---|
| gRPC services | `ServiceToken` — `RequireClaim("scope","payment:write")` | Caller attaches `client_credentials` bearer token via gRPC `CallCredentials` |
| User-facing controllers | JwtBearer, audience `concertable.payment.api` | Standard user JWT from SPA |

Service clients with `payment:write` scope: `concertable-b2b`, `concertable-customer`
(already registered in Duende). `Concertable.Workers` authenticates as `concertable-b2b`
(it is B2B's background runner). Token obtained via `ITokenService.GetTokenAsync("payment:write")`
from `Concertable.Kernel`.

---

## Integration events

### `Payment.Web` publishes (via outbox → ASB)
| Event | Subscribers |
|---|---|
| `PaymentSucceededEvent` | `concertable-b2b`, `concertable-customer`, `concertable-payment` |
| `PaymentFailedEvent` | `concertable-payment` |

### `Payment.Workers` subscribes
| Event | Handler |
|---|---|
| `CustomerRegisteredEvent` | `CustomerRegisteredHandler` — provision Stripe customer |
| `VenueManagerRegisteredEvent` | `ManagerRegisteredHandler` — provision Stripe account |
| `ArtistManagerRegisteredEvent` | `ManagerRegisteredHandler` |
| `PaymentSucceededEvent` | `PaymentTransactionHandler` — record transaction |
| `PaymentFailedEvent` | `PaymentFailureDispatcher` |

### B2B gains new subscription
`PaymentSucceededEvent` → Concert `SettlementPaymentProcessor`, `EscrowPaymentProcessor`.
B2B was publish-only; Step 14 adds `AddInbox` on `DefaultConnection`.

### Customer gains new subscription
`PaymentSucceededEvent` → `TicketPaymentProcessor`.
Customer already has inbox from Step 10.

---

## Client library — `Concertable.Payment.Client`

Consumed by B2B, Customer, and Workers in place of `AddPaymentModule`. Contains:

- `payment.proto` reference (`GrpcServices="Client"`)
- 3 adapters — `GrpcManagerPaymentModule`, `GrpcCustomerPaymentModule`,
  `GrpcEscrowModule` — implement the existing `IManagerPaymentModule`,
  `ICustomerPaymentModule`, `IEscrowModule` interfaces; callers are unchanged
- `AddPaymentClient(IServiceCollection, IConfiguration)` — registers `AddGrpcClient<T>()`
  pointed at logical name `https://payment` (Aspire service discovery), bearer token
  interceptor, and the 3 adapters

---

## Database

Dedicated `PaymentDb` (SQL Server). `PaymentDbContext`. Connection string key `"PaymentDb"`.
Both `Payment.Web` and `Payment.Workers` connect to the same database.

---

## Callers after extraction

| Caller | What they call | How |
|---|---|---|
| B2B Concert workflow steps (10 step classes) | `IManagerPaymentModule`, `IEscrowModule` | gRPC via `AddPaymentClient` |
| Customer `TicketService` | `ICustomerPaymentModule` | gRPC via `AddPaymentClient` |
| `Concertable.Workers` (DoorSplit settlement) | `IManagerPaymentModule.PayAsync` | gRPC via `AddPaymentClient` |
| Stripe | Webhook POST | HTTP → `WebhookController` |
| B2B SPA | Stripe Connect onboarding, transaction history | HTTP → `StripeAccountController`, `TransactionController` |
