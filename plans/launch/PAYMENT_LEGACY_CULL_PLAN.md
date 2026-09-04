# Payment legacy cull and vocabulary plan

## Outcome

Payment's published surface carries no raw provider identifier and no consumer-domain vocabulary.
The bespoke session paths are retired onto the durable operation subsystem, and the contract names
say what they are. One breaking producer release delivers all of it.

## Delivery

1. Remove the raw-identifier surface: the legacy `PayAsync(..., string paymentMethodId, ...)`
   overloads, the `PaymentMethodId`-bearing `DepositEscrowCommand`, the held-intent fetch that
   served `FindHeldIntentAsync`, the optional `payment_method_id` on
   `PaymentSessionOperationRequest`, and the payment-method-id enrichment of integration-event
   metadata in `SetupIntentWebhookHandler`. Verification gate: Payment suites green; compatibility
   baselines re-recorded for the breaking release.
2. Retire the bespoke `CreateHoldSession` / `CreateVerifySession` / `CreateSetupSession` paths onto
   the durable operation subsystem, and remove the legacy single-attempt idempotency shim (the
   `api/Concertable.Payment/TECH_DEBT.md` entry that resolves here). `VerifyTransactionEntity` and
   the transaction log remain ledger-only. Delete `TransactionTypes.ApplicationApply` and
   `ApplicationAccept`, and replace the B2B nouns in `PaymentMetadataKeys` (`applicationId` →
   `contextId` per the existing column precedent; `venueManagerId` → `payerOwnerId`;
   `opportunityId` deleted — consumer context rides the consumer correlation).
3. Vocabulary pass (`plans/launch/PAYMENT_BOUNDARY_DECISION.md` §6): `ApplicationId` → `ContextId`
   property; `ConsumerCorrelation` → `ClientReference`; `PaymentSessionSpecification` →
   `PaymentSessionDefinition`; `CreateOrReplay` → `Create` (a gRPC method rename — wire-breaking,
   which is why it batches here). Rename gate:
   `grep -rniE "applicationApply|applicationAccept|ConsumerCorrelation|CreateOrReplay|PaymentSessionSpecification|FindHeldIntent"`
   returns zero repo-wide, with an explicit allowlist for deliberate survivors.
4. Publish the breaking packages; both consumers bump pins and revalidate. Consumption contract:
   rename adoption only — both consumers are already on the reference surface when this plan is
   allowed to start, so no behavioural consumer change ships here.

## Invariants

- No provider identifier and no consumer-domain noun on any published Payment contract.
- One breaking release carries the removal and the renames together; the coordination cost is paid
  once.
- This plan must not begin delivery while any consumer still calls a raw-identifier API.

## Package cut-over

Breaking producer change: its own release, publish-then-bump, gated on both consumer migrations
(PR #633's B2B migration and the Customer payment-reference migration) being terminal.
