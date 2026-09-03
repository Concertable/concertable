# Payment method commitments progress

- Plan: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-operation-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payment-method-commitments`
- Branch: `Feature/payment-method-commitments`
- PR: not opened
- Dependency/package gates: B2B migration waits for the Payment Contracts and Client packages from this producer change
- Last reconciled: 2026-09-04 after resolving the incremental producer review and rerunning local validation

## Current state

Payment now owns provider payment-method identifiers and resolves stable `PaymentOperationReference` values for setup, validation, charges, escrow deposits, and authorization capture. All full and incremental producer review findings are resolved and all local validation gates are green; the existing raw-identifier APIs remain temporarily for package-compatible consumer migration.

## Next Steps

Run a fresh incremental review over the latest remediation commit, then open and deliver the Payment package PR before migrating B2B.

## Completed work

- Implemented durable provider-session payment-method persistence and reference-based Payment operations in this commit.
- Re-scaffolded the Payment initial migration in this commit; unchanged contexts retained their existing migration identities.
- Resolved all four full-review findings: provider reconciliation races, atomic attempt transitions, mapper extension syntax, and composite error-contract coverage.
- Resolved all three incremental-review findings: complete transition validation before mutation, retry transient reference resolution without terminal rejection, and move reference orchestration coverage to SQL-backed integration tests.
- Added Payment's canonical host-backed integration fixture and migrated the reference-payment scenarios onto the production service composition.
- Scoped the Payment topology service once through the topology builder.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --no-restore`: passed with 0 warnings and 0 errors.
- Payment unit tests: 573 passed.
- Payment integration tests: 57 passed.
- Payment architecture tests: 9 passed.
- Plan graph: 0 errors and 0 warnings.
- Superseded commitment/resolver/provider-result names: no matches under `api/Concertable.Payment`.

## Reviews

The canonical producer review is recorded in `reviews/Feature-payment-method-commitments.md`. Its four full-review findings and three incremental-review findings are resolved; a fresh incremental review from the recorded watermark remains pending.

## Decisions, discoveries, blockers, and deviations

- `PaymentOperationReference` is consumer-domain agnostic; B2B will own any closed operation enum and map it at its Payment adapter boundary.
- `ProviderSession` is the reusable internal provider-resource carrier for create, retrieve, cancel, and webhook flows.
- `PaymentMethodChargeError` is additive because changing the published `ManagerPaymentOperationError` union broke generated Dunet binary compatibility.
- `CaptureEscrowByReferenceCommand` and `DepositEscrowByReferenceCommand` keep the durable-reference distinction while satisfying Aspire's 64-character resource-name limit.
- Null-forgiving Result error extraction is recorded in `api/Concertable.Payment/TECH_DEBT.md` pending an exhaustive non-null Reunion failure accessor.
