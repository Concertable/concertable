# Payment method commitments progress

- Plan: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-operation-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payment-method-commitments`
- Branch: `Feature/payment-method-commitments`
- PR: not opened
- Dependency/package gates: B2B migration waits for the Payment Contracts and Client packages from this producer change
- Last reconciled: 2026-09-03 after the producer implementation and local validation gates completed

## Current state

Payment now owns provider payment-method identifiers and resolves stable `PaymentOperationReference` values for setup, validation, charges, escrow deposits, and authorization capture. The producer change is implemented and locally green; the existing raw-identifier APIs remain temporarily for package-compatible consumer migration.

## Next Steps

Review the complete producer diff, resolve all findings, then open and deliver the Payment package PR before migrating B2B.

## Completed work

- Implemented durable provider-session payment-method persistence and reference-based Payment operations in this commit.
- Re-scaffolded the Payment initial migration in this commit; unchanged contexts retained their existing migration identities.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --no-restore`: passed with 0 warnings and 0 errors.
- Payment unit tests: 570 passed.
- Payment integration tests: 53 passed.
- Payment architecture tests: 9 passed.
- Superseded commitment/resolver/provider-result names: no matches under `api/Concertable.Payment`.

## Reviews

Full producer review pending; no review artifact yet.

## Decisions, discoveries, blockers, and deviations

- `PaymentOperationReference` is consumer-domain agnostic; B2B will own any closed operation enum and map it at its Payment adapter boundary.
- `ProviderSession` is the reusable internal provider-resource carrier for create, retrieve, cancel, and webhook flows.
- `PaymentMethodChargeError` is additive because changing the published `ManagerPaymentOperationError` union broke generated Dunet binary compatibility.
- `CaptureEscrowByReferenceCommand` and `DepositEscrowByReferenceCommand` keep the durable-reference distinction while satisfying Aspire's 64-character resource-name limit.
- Null-forgiving Result error extraction is recorded in `api/Concertable.Payment/TECH_DEBT.md` pending an exhaustive non-null Reunion failure accessor.
