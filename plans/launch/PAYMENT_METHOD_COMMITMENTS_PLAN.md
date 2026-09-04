# Payment operation ownership plan

## Outcome

Make Payment the sole owner of provider payment-method identifiers, payment operations, verification evidence, escrow, settlement, and ledger correlation. Consumer services identify their contexts through opaque `PaymentOperationReference` values and never store or forward a provider payment-method identifier. Payment's published surface and persistence contain payment vocabulary only.

## Delivery

1. Extend Payment's durable session operation so a successful SetupIntent records its resolved payment method privately.
2. Add a Payment-owned commitment reference and operations that create or replay setup/verification sessions by that stable reference, validate a committed method, and perform charges or escrow deposits from it.
3. Harden the producer against the Stripe-correctness gaps recorded in `plans/launch/PAYMENT_BOUNDARY_DECISION.md` section 5: present only `always`-consented saved methods, record the customer's variable-amount merchant-initiated agreement on the operation, migrate the legacy Stripe idempotency keys onto the attempt-keyed shape, and separate authentication-required recovery from new-method declines.
4. Make `PaymentOperationReference(OperationType, ClientReference)` the single consumer-correlation primitive. Remove `BookingId` from Payment's bus, gRPC, application, domain, ledger, escrow, settlement, and schema surfaces; key escrows uniquely by `(OperationType, ClientReference)`; carry the reference on every financial-operation outcome; bump the settlement fingerprint version; and re-scaffold Payment's initial migration.
5. Remove the legacy raw-identifier and consumer-role surfaces. Delete the raw payment-method and intent-id commands/RPCs, the bespoke setup/verify/hold session paths, `CustomerPayment`, and the unused transaction types. Replace `ManagerPayment` with Payment-owned settlement operations and payment reporting, route Customer's on-session flow through the durable session subsystem, keep verification rows as ledger history only, and use agnostic context/owner metadata throughout. Consumption contract: `Concertable.Payment.Client` publishes session, settlement, reporting, and escrow clients whose requests carry opaque references and typed outcomes; consumers never receive or send a provider identifier.
6. Complete the breaking vocabulary pass: `ConsumerCorrelation` to `ClientReference`, `PaymentSessionSpecification` to `PaymentSessionDefinition`, `CreateOrReplay` to `Create`, consumer-named transaction/reporting types to payment-owned names, and the remaining published `Manager*`, `Customer*`, `Booking*`, `Concert*`, and `Application*` identities to their payment concepts. Remove Payment.Hosting's B2B package references and dead subscription, correct `ARCHITECTURE.md`, and enforce the invariant with standing published-surface and host-reference architecture tests. Re-record the deliberately breaking compatibility baselines and require the superseded-identity grep gate to be empty apart from explicit historical baselines.
7. Publish the breaking Payment Contracts and Client packages in one release.
8. Migrate B2B Application, Booking, and Concert on PR #633 and Customer ticket purchase on its own consumer branch directly to the final published surface; remove their provider-identifier mirrors and re-scaffold affected initial migrations.

## Invariants

- A provider payment-method identifier exists only in Payment persistence and provider adapter calls.
- Payment remains consumer-domain agnostic: operation type and client reference are opaque strings.
- Repeating a commitment request for the same reference replays the canonical Payment session operation.
- Only a terminal successful setup or verification operation may supply a payment method to a later financial operation.
- Common lifecycle work stays in common flow. Present-day deal divergence uses one keyed strategy contract; no union represents identical call shapes.

## Package cut-over

The producer change is one deliberately breaking Payment package merge on PR #933. B2B and Customer consume only after the new Payment packages are published and their pins advance. Producer and consumer changes remain separate merges because consumers compile against published packages, but each consumer migrates directly from the old raw-identifier surface to the final reference surface once.
