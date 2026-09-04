# Payment operation ownership progress

- Plan: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-operation-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payment-method-commitments`
- Branch: `Feature/payment-method-commitments`
- PR: [#933](https://github.com/Concertable/concertable/pull/933) (open, owner-blocked until the final breaking surface is merge-ready)
- Dependency/package gates: B2B migration on PR [#633](https://github.com/Concertable/concertable/pull/633) and the Customer payment-reference migration both wait for the breaking Payment Contracts and Client packages from this producer
- Last reconciled: 2026-09-04 after the owner folded the agnosticism audit and legacy cull into PR #933 and the branch merged current `origin/main`

## Current state

Delivery items 1–3 are implemented and previously reviewed. The owner decided that PR #933 must also deliver the complete consumer-agnostic Payment surface, raw-identifier cull, and breaking vocabulary pass in one producer release because neither consumer has adopted the intermediate surface. The former legacy-cull plan is absorbed and deleted. Current `origin/main` is merged at `d66dd4ba5`; the Payment solution builds cleanly after that merge. The previous review approval is historical and a fresh full review is required for the final candidate.

## Next Steps

1. Execute Delivery item 4 from `PAYMENT_METHOD_COMMITMENTS_PLAN.md`: replace every Payment-side `BookingId` correlation with `PaymentOperationReference(OperationType, ClientReference)` across bus contracts, gRPC, financial-operation state, escrow, settlement, ledger, and persistence; enforce the escrow composite alternate key; bump the settlement fingerprint version; run focused tests; then re-scaffold Payment's initial migration.

## Completed work

- Delivered durable payment-session operation state, Payment-owned payment-method commitments, provider-truth reconciliation, reference-based setup/validation/charge/escrow operations, and SQL-backed integration coverage.
- Hardened saved-method consent, merchant-initiated consent evidence, Stripe attempt-key idempotency, and typed authentication-required recovery.
- Resolved the prior full and incremental review findings; the historical approved watermark is `448316d2a260e1507dc1c8e1ca3dba607fb5b9ec`.
- Merged current `origin/main` into the branch at `d66dd4ba5` before beginning the breaking release.
- Owner decision, 2026-09-04: absorb the agnostic surface, legacy cull, and vocabulary cleanup into PR #933 as one breaking Payment release; remove the dormant cull plan and its later producer gate.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --no-restore`: passed after the base merge with 0 warnings and 0 errors.
- Before the scope expansion: Payment unit tests 589 passed, integration tests 59 passed, architecture tests 9 passed, and plan graph passed with 0 errors and 0 warnings.

## Reviews

The canonical producer review is `reviews/Feature-payment-method-commitments.md`. Its existing findings are closed, but its watermark predates the breaking agnosticism work. Run a fresh full review after Delivery items 4–6 are green, then stamp the exact reviewed head in a separate reviews-only commit.

## Downstream handoffs

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` — worktree `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`, PR [#633](https://github.com/Concertable/concertable/pull/633). Gate: the final Payment Contracts and Client packages publish and B2B's platform pin advances; B2B then migrates directly from the raw surface to the final reference surface.
- `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PROGRESS.md` — no worktree yet. Gate: the same package publish; Customer then migrates its on-session purchase flow directly to the final session-operation surface.

## Decisions, discoveries, blockers, and deviations

- The final invariant is stronger than provider-id ownership: Payment contains no B2B or Customer product knowledge. `Booking`, `Concert`, `Ticket`, `Application`, `Opportunity`, and `Manager` are forbidden product vocabulary; Stripe's own `Customer` object and Payment's `Escrow`, `Commission`, `Settlement`, `PayoutAccount`, `Ledger`, `Payer`, and `Payee` vocabulary remain legitimate.
- `PaymentOperationReference(OperationType, ClientReference)` is the only consumer-correlation primitive. Payment stores and compares both opaque strings but never interprets consumer values. Escrow uniqueness is the composite pair, not `ClientReference` alone.
- Removing the integer booking correlation changes persisted column types and the settlement fingerprint payload. Payment has no deployed rows, so the migration story is an initial-migration re-scaffold; `SettlementOperationFingerprint.CurrentVersion` still advances for explicit hash identity.
- The final package surface separates durable session operations, settlement operations, payment reporting, and escrow operations. Customer's old role-specific gRPC service and all raw payment-method/intent-id APIs are removed rather than renamed.
- `PaymentMethodChargeError.AuthenticationRequired` remains distinct from new-method recovery and carries no provider artifact; consumers re-enter by operation reference.
- `KeyedUnionBuilder` and the B2B Deal union wrapper remain consumer-owned typed-escalation machinery. Only their obsolete payment-method-based union usage is removed during the B2B migration.
