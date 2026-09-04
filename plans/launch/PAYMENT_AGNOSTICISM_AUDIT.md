# Payment agnosticism audit

Independent audit, 2026-09-04, commissioned after the owner challenged the claim that
`Concertable.Payment` is consumer-domain agnostic. Findings verified against the worktree at
`6d9fc4a63`. Depth classes: **(a)** internal naming, **(b)** published contract, **(c)** persisted
schema, **(d)** structural — Payment stores, requires or is keyed by a consumer concept.

This is a standing reference: it spins off no phases of its own and owes no ledger.

## Verdict

The claim is false, and not only cosmetically. Payment runs two parallel models: the agnostic
`PaymentSessionOperations` subsystem (`OperationType` + `ConsumerCorrelation` + `PayerOwnerKey`), and
an older B2B-shaped one (`BookingId`, `ManagerPayment`, `ConcertId`, `ApplicationId`) that owns
escrow, the ledger, settlement and every money-movement RPC. The new primitive was added *beside* the
old model rather than replacing it.

**Payment is an agnostic adapter for B2B and a partial one for Customer.** The consumer it is
nominally shared with cannot use its escrow, ledger or financial-operation subsystems at all.

`api/Concertable.Payment/ARCHITECTURE.md:14` asserts the invariant is already satisfied ("Payment
owns zero consumer-domain knowledge"). That sentence is false and load-bearing — a cleanup measured
against it will under-scope. Correct it in the same change.

## (d) Structural

- **D1 — escrow is keyed by a B2B booking.** `IX_Escrows_BookingId` is **unique**
  (`Data/Configurations/EscrowEntityConfiguration.cs:17`), plus `GetByBookingIdAsync` and the
  `ReleaseByBookingId` / `RefundByBookingId` RPCs. Payment enforces "one escrow per B2B booking" as a
  database constraint. Agnostic form: `UNIQUE (OperationType, ClientReference)`.
- **D2 — the financial-operation aggregate refuses to exist without one.**
  `Domain/Entities/FinancialOperationEntity.cs:15` throws when `bookingId <= 0`. Customer cannot use
  the subsystem.
- **D3 — the double-entry ledger is closed to Customer.** `Application/DTOs/LedgerPosting.cs:10`
  takes a non-nullable `int BookingId`; `LedgerTransactions.BookingId` is `nullable: false` with an
  index. Ticket payments never reach the ledger and structurally cannot. Strongest disproof of
  agnosticism.
- **D4 — webhook handlers hard-index consumer keys out of provider metadata.**
  `Infrastructure/Events/TicketTransactionHandler.cs:22` (`ConcertId`) and
  `VerifyTransactionHandler.cs:22-23` (`ApplicationId`, `VenueManagerId`) use the indexer, so Payment
  *requires* consumers to stamp their domain ids into Stripe metadata or the handler faults. Renaming
  the key does not fix this.
- **D5 — a published Payment package compiles against B2B.** `Concertable.Payment.Hosting` is
  `IsPackable=true` and carries `PackageReference` to `Concertable.B2B.Concert.Contracts` and
  `Concertable.B2B.Tenant.Contracts`; `PaymentTopology.cs:2,22` does
  `using Concertable.B2B.Concert.Contracts.Events;` and `.Subscribe<ConcertChangedEvent>()`. That
  subscription is the only occurrence of `ConcertChangedEvent` in the entire Payment tree — there is
  no handler. Dead subscription, live compile edge.
- **D6 — the replay fingerprint hashes the booking id.**
  `Domain/SettlementOperationFingerprint.cs:28,47,67`. Removing it needs a fingerprint version bump.
- **D7 — `Manager` vs `Customer` is a consumer-role split, not a payments split.** Read off
  `ManagerPaymentService.PayCoreAsync`, the real distinction is fee-split + settlement row + ledger
  posting versus a plain charge — which `PaymentSessionFundsRouting` already models.
  `CustomerPaymentService.cs:27,46` takes an `int concertId` it never uses.

## (b) Published contract

- `booking_id` on ~14 proto fields, 3 RPC method names (`ReleaseByBookingId`, `RefundByBookingId`,
  `RefundBoundCommissionByBookingId`) and 2 response messages.
- `int BookingId` on 11 published bus contracts in `Contracts/FinancialOperations.cs` — **including
  `CaptureEscrowByReferenceCommand` and `DepositEscrowByReferenceCommand`, added on this branch**,
  where it sits in the same record as the agnostic `PaymentOperationReference`.
- `ManagerPayment`: the gRPC service, 3 request messages, `IManagerPaymentOperationsClient`,
  `IManagerPaymentReportingClient`, `ManagerSettlement`, both error unions, and the wire-visible code
  `payment.manager_operation_conflict`. The noun is wrong even inside B2B —
  `IManagerPaymentReportingClient` is consumed by the artist dashboard as well as the venue one.
- `CustomerPayment` service, `concert_id` on two messages, `GetTicketRevenue*` RPCs,
  `TransactionTypes.Ticket`.
- `PaymentMetadataKeys`: `ConcertId`, `BookingId`, `ApplicationId`, `VenueManagerId`, `OpportunityId`
  (the last is dead — declared and referenced nowhere).
- `TransactionTypes.ApplicationApply` / `ApplicationAccept` are also DI keys
  (`ServiceCollectionExtensions.cs:161-166`), so they route behaviour, not just label it.

## (c) Persisted schema

`Escrows.BookingId` (int, NOT NULL, **unique** index), `FinancialOperations.BookingId`,
`LedgerTransactions.BookingId`, plus `SettlementTransactionEntity.BookingId`,
`TicketTransactionEntity.ConcertId`, `VerifyTransactionEntity.ApplicationId`.

Precedent worth copying: the `Transactions` table already solved this — the column is `ContextId` and
the TPH subtypes map their domain-named properties onto it
(`TransactionEntityConfiguration.cs:23,31,51`). There the leak is class (a) only.

## Legitimately Payment's own — do not rename

`Escrow`, `Commission`, `Settlement`, `PayoutAccount`, `Ledger` (and its account/entry/leg
vocabulary), `Payer`/`Payee`, `Money`/`Currency`, `Charge`, `Refund`, `Transfer`, `PaymentIntent` /
`SetupIntent` / `PaymentMethod` / `Mandate`, `PaymentSession` and its operation/attempt/fingerprint
vocabulary, `FundsRouting`, `OnSession`/`OffSession`, `PlatformFee`.

Two traps:

- **`Customer` is ambiguous.** `StripeCustomerId`, `customer_session_secret`, `customer_token`,
  `ProvisionCustomerAsync`, `CustomerRegisteredHandler` are **Stripe's** Customer object — keep every
  one. Only `service CustomerPayment` / `CustomerPayRequest` / `ICustomerPaymentOperationsClient` /
  `CustomerPaymentService` are the Concertable-consumer noun.
- **`Verify` is legitimate** — card verification is a payments operation, already modelled as
  `PaymentSessionKind.PaymentMethodVerification`. Only its `ApplicationId` field leaks.

## `BookingId` does two jobs

- **Correlation / replay identity** — on the bus contracts, the pay requests and the fingerprint.
  This is exactly `PaymentOperationReference(OperationType, ConsumerCorrelation)` one generation
  earlier, with the consumer's type baked into the field name instead of carried as data. Straight
  substitution.
- **Payment's own alternate key for an escrow** — the unique index and the `*ByBookingId` RPCs. Here
  `ClientReference` alone is insufficient: uniqueness must become `(OperationType, ClientReference)`,
  because two consumers minting small ints will collide the moment Customer uses escrow.

Two costs the equivalence hides: `int` to `string` is a **column type change**, not a rename; and the
value is inside the fingerprint's SHA-256 payload, so it needs a `CurrentVersion` bump and a
migration story.

## What the existing cull plan misses

`plans/launch/PAYMENT_LEGACY_CULL_PLAN.md` states the right invariant and delivers the
provider-identifier half competently. Grepping it and `PAYMENT_BOUNDARY_DECISION.md` for `BookingId`,
`booking_id`, `ManagerPayment`, `ConcertId`, `concert_id` returns **zero hits**. Its Delivery misses:
all of `booking_id`; all of `ManagerPayment`; `ConcertId` / `CustomerPayment` / `Ticket`; the B2B
package dependency; the D4 structural requirement (a key rename leaves it intact); and any schema
step at all. Its rename-gate grep would pass with every one of those still present.

`tests/Concertable.Payment.ArchitectureTests/PaymentContractReferenceTests.cs:14-24` checks only
assembly references to `Concertable.B2B*` / `Concertable.Customer*`, passes today with every leak
above in place, and does not cover `Payment.Hosting` — the one assembly that would fail it. The
invariant needs a standing test over the published contract surface, not a one-off grep.

## Highest-value fix

Make `(OperationType, ClientReference)` the single correlation primitive and delete `BookingId` from
Payment — proto, bus contracts and schema — in one breaking release. It is the only leak that is
simultaneously published, persisted and structural; it is what closes the ledger to Customer; and it
breaks silently the first time Customer touches escrow.

Cheapest independent win, no consumer impact: delete the two `Concertable.B2B.*` package references
and the dead `Subscribe<ConcertChangedEvent>()` from `Payment.Hosting`.
