# Payment method commitments progress

- Plan: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-operation-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payment-method-commitments`
- Branch: `Feature/payment-method-commitments`
- PR: [#933](https://github.com/Concertable/concertable/pull/933) (open)
- Dependency/package gates: B2B migration and PR [#633](https://github.com/Concertable/concertable/pull/633) both wait for the Payment Contracts and Client packages from this producer change
- Last reconciled: 2026-09-04 after opening PR #933, recording the Stripe boundary decision, and scheduling the hardening pass

## Current state

Payment now owns provider payment-method identifiers and resolves stable `PaymentOperationReference` values for setup, validation, charges, escrow deposits, and authorization capture. All full and incremental producer review findings are resolved and all local validation gates are green; the existing raw-identifier APIs remain temporarily for package-compatible consumer migration. PR #933 is open. The Stripe boundary research is recorded in `plans/launch/PAYMENT_BOUNDARY_DECISION.md`; its §5 hardening items land on this branch before merge (plan Delivery item 3).

## Next Steps

1. Land the plan's Delivery item 3 hardening on this branch, from `plans/launch/PAYMENT_BOUNDARY_DECISION.md` §5: set `PaymentMethodAllowRedisplayFilters` to `["always"]` (`Services/StripeAccountClient.cs:268`); record variable-amount merchant-initiated consent evidence (terms version, timestamp) on the operation row at setup; migrate the `"{identity}:{action}"` Stripe idempotency keys (`Services/StripeRequestOptions.cs:49`) onto the `PaymentSessionIdempotencyKey(operationId, attemptId, revision)` shape; confirm `PaymentMethodChargeError` separates the `authentication_required` recovery (retry the same intent on-session) from new-method declines. Run the incremental review over the new commits.
2. Push the stable candidate and deliver PR #933 through merge, package publication, and platform sync.
3. Dispatch the downstream handoff: PR #633 (`DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`) advances its Payment pins against the published packages, revalidates, goes ready, and merges.
4. Migrate B2B and the SPA against the published Contracts and Client packages (plan Delivery item 5) in a fresh worktree from the then-current main.

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

The canonical producer review is recorded in `reviews/Feature-payment-method-commitments.md`. All findings are resolved; the approved review watermark covers `6510ca80cc7b27557512eac2f24f859ab1269254` and the security watermark covers `eef36ac547f8a61c025af2f428c45317a64223de` (the later commit is test-only).

## Downstream handoffs

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` — worktree `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`, PR [#633](https://github.com/Concertable/concertable/pull/633). Gate: Payment Contracts and Client packages published from PR #933 and the B2B platform pin advanced.

## Decisions, discoveries, blockers, and deviations

- `PaymentOperationReference` is consumer-domain agnostic; B2B will own any closed operation enum and map it at its Payment adapter boundary.
- `ProviderSession` is the reusable internal provider-resource carrier for create, retrieve, cancel, and webhook flows.
- `PaymentMethodChargeError` is additive because changing the published `ManagerPaymentOperationError` union broke generated Dunet binary compatibility.
- `CaptureEscrowByReferenceCommand` and `DepositEscrowByReferenceCommand` keep the durable-reference distinction while satisfying Aspire's 64-character resource-name limit.
- Null-forgiving Result error extraction is recorded in `api/Concertable.Payment/TECH_DEBT.md` pending an exhaustive non-null Reunion failure accessor.
- `plans/launch/PAYMENT_BOUNDARY_DECISION.md` confirms this design against Stripe's documentation: destination-charge topology needs no payment-method cloning, and the reference model matches Stripe's server-authoritative saved-card flow. Its §5 gaps are the hardening pass (Delivery item 3); its §6 naming and legacy-cull items are follow-on breaking plans, not this plan.
