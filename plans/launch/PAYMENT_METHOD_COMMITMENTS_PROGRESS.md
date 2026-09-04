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
4. The B2B + SPA consumer migration (plan Delivery item 5) rides PR #633 (owner decision, 2026-09-04) — no separate consumer worktree. After the packages publish and #633's Payment pins advance, #633 adopts the reference surface (`SetupPaymentMethod`/`ValidatePaymentMethod`, `*ByReference` commands), deletes `ApplyRequest`/`AcceptRequest.PaymentMethodId`, the pm-id entity columns, and the `FindHeldIntentAsync` round-trip, updates the SPA checkout flow, and re-scaffolds affected initial migrations. Union disposition for this step: collapse the `Apply` union *usage* into a single keyed `IApply` strategy family (its two arms become identical once `PaymentMethodId` is deleted) and remove the `Apply` union record — but **keep** `KeyedUnionBuilder`, its tests, and the `DealUnionBuilder`/`DealUnionFactory`/`IDealUnionFactory` wrapper: they are the retained typed-escalation tier (owner decision, 2026-09-04) for a future capability whose shared-action contract genuinely fractures on legitimate client input (typed request-union + capability-keyed union; see the trichotomy below). Also correct `api/Concertable.B2B/CODE_PATTERNS.md`: the unions table still documents the deleted Accept union, and the keyed-union entry should state its admission test — input stored in terms → keyed strategy; input chosen by the user during the shared action → capability-keyed union with a tagged request union; input negotiated as its own act → separate endpoint.

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

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` — worktree `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`, PR [#633](https://github.com/Concertable/concertable/pull/633). Gate: Payment Contracts and Client packages published from PR #933 and the B2B platform pin advanced. On that gate #633 also performs the consumer migration and union-usage collapse per Next Steps item 4 (owner decision, 2026-09-04); that owner folds the scope into its own plan at its next material checkpoint.

## Decisions, discoveries, blockers, and deviations

- `PaymentOperationReference` is consumer-domain agnostic; B2B will own any closed operation enum and map it at its Payment adapter boundary.
- `ProviderSession` is the reusable internal provider-resource carrier for create, retrieve, cancel, and webhook flows.
- `PaymentMethodChargeError` is additive because changing the published `ManagerPaymentOperationError` union broke generated Dunet binary compatibility.
- `CaptureEscrowByReferenceCommand` and `DepositEscrowByReferenceCommand` keep the durable-reference distinction while satisfying Aspire's 64-character resource-name limit.
- Null-forgiving Result error extraction is recorded in `api/Concertable.Payment/TECH_DEBT.md` pending an exhaustive non-null Reunion failure accessor.
- `plans/launch/PAYMENT_BOUNDARY_DECISION.md` confirms this design against Stripe's documentation: destination-charge topology needs no payment-method cloning, and the reference model matches Stripe's server-authoritative saved-card flow. Its §5 gaps are the hardening pass (Delivery item 3); its §6 naming and legacy-cull items are follow-on breaking plans, not this plan.
- The decision doc's §2 line "no union survives this" applies to the `Apply`/`Accept` union *usages* only, not the machinery. Owner decision (2026-09-04, after a three-way review including an external Codex pass): `KeyedUnionBuilder` and the Deal union wrapper are retained as the typed-escalation tier of the capability architecture — capability-axis-keyed union + tagged request union, entered only when a shared action's contract genuinely fractures on legitimate client-supplied input. The consumer migration deletes the pm-id-based usage, never the builder.
