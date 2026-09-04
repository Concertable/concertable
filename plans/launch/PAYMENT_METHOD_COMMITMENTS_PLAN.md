# Payment method commitments plan

## Outcome

Make Payment the sole owner of provider payment-method identifiers and verification evidence. Consumer services identify a durable Payment-owned commitment by opaque operation type and consumer correlation; they never store or forward a provider payment-method identifier.

## Delivery

1. Extend Payment's durable session operation so a successful SetupIntent records its resolved payment method privately.
2. Add a Payment-owned commitment reference and operations that create or replay setup/verification sessions by that stable reference, validate a committed method, and perform charges or escrow deposits from it.
3. Harden the producer against the Stripe-correctness gaps recorded in `plans/launch/PAYMENT_BOUNDARY_DECISION.md` §5: present only `always`-consented saved methods, record the customer's variable-amount merchant-initiated agreement on the operation, migrate the legacy `identity:action` Stripe idempotency keys onto the attempt-keyed shape, and confirm `PaymentMethodChargeError` separates the authentication-required recovery from new-method declines.
4. Publish the changed Payment Contracts and Client packages.
5. Migrate B2B Application, Booking, and Concert to the published reference-based surface; remove their payment-method and verification mirrors; re-scaffold affected initial migrations.

## Invariants

- A provider payment-method identifier exists only in Payment persistence and provider adapter calls.
- Payment remains consumer-domain agnostic: operation type and consumer correlation are opaque strings.
- Repeating a commitment request for the same reference replays the canonical Payment session operation.
- Only a terminal successful setup or verification operation may supply a payment method to a later financial operation.
- Common lifecycle work stays in common flow. Present-day deal divergence uses one keyed strategy contract; no union represents identical call shapes.

## Package cut-over

The producer change is one Payment package merge. B2B consumes only after the new Payment package is published and its pin is advanced. The producer and consumer remain separate PRs.

The raw-identifier legacy surface, the `ApplicationApply`/`ApplicationAccept` vocabulary, and the contract renames recorded in `plans/launch/PAYMENT_BOUNDARY_DECISION.md` §6 are follow-on breaking-contract plans, not phases of this plan.
