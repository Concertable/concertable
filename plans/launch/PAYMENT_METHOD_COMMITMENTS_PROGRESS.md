# Payment operation ownership progress

- Plan: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-operation-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payment-method-commitments`
- Branch: `Feature/payment-method-commitments`
- PR: [#933](https://github.com/Concertable/concertable/pull/933) (open, owner-blocked until the final breaking surface is merge-ready)
- Dependency/package gates: B2B migration on PR [#633](https://github.com/Concertable/concertable/pull/633) and the Customer payment-reference migration both wait for the breaking Payment Contracts and Client packages from this producer
- Last reconciled: 2026-09-05 after the fresh full-review findings were resolved and all local Payment gates passed

## Current state

Delivery items 1–6 are implemented. Payment now uses validated opaque operation references throughout its published contracts and persistence, exposes the surviving clean contract as v1, owns generic payment-method and payout-owner registration events, and has no B2B or Customer product dependency. Public outcomes and events carry only Payment-owned identities; provider object identifiers remain private to Payment. The Payment initial migration and candidate v1 compatibility baseline are re-scaffolded. The fresh full review raised PAY-009 through PAY-011; all three are resolved and await the required incremental review of the remediation commit.

## Next Steps

1. Commit the verified PAY-009 through PAY-011 remediation.
2. Run the canonical incremental review against that exact remediation head and stamp its approval in a separate reviews-only commit.
3. Push the reviewed head and complete exact-head remote validation.
4. Stop for the owner's explicit approval before enqueueing PR #933 or publishing the breaking v1 packages.

## Completed work

- Delivered durable payment-session operation state, Payment-owned payment-method commitments, provider-truth reconciliation, reference-based setup/validation/charge/escrow operations, and SQL-backed integration coverage.
- Hardened saved-method consent, merchant-initiated consent evidence, Stripe attempt-key idempotency, and typed authentication-required recovery.
- Resolved the prior full and incremental review findings; the historical approved watermark is `448316d2a260e1507dc1c8e1ca3dba607fb5b9ec`.
- Merged current `origin/main` into the branch at `d66dd4ba5` before beginning the breaking release.
- Owner decision, 2026-09-04: absorb the agnostic surface, legacy cull, and vocabulary cleanup into PR #933 as one breaking Payment release; remove the dormant cull plan and its later producer gate.
- Replaced booking/application correlation with `PaymentOperationReference`, removed the role-specific Customer/Manager surfaces and raw provider-identifier inputs, and split the public clients into session, settlement, reporting, escrow, commission, and payout operations.
- Replaced Auth/B2B ingress knowledge with Payment-owned owner-registration events and reduced Payment's accepted JWT audience to `concertable.payment.api`.
- Re-scaffolded the Payment initial migration and generated the clean candidate contract as compatibility baseline `v1`; the old published package remains historical fixture `0.1.0-alpha.0.1254` only.
- Split internal provider execution results from public Payment outcomes, removed provider identifiers from response and event contracts, and added a published-surface architecture guard.
- Made `PaymentOperationReference` a validated `readonly record struct`, carried it as one value through public requests and repositories, and aligned every persisted reference pair to its canonical 100/200 limits.
- Converted every extension container changed by the breaking candidate to C# 14 extension blocks.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --no-restore --verbosity minimal`: passed with 0 warnings and 0 errors.
- Payment unit tests: 545 passed.
- Payment integration tests: 63 passed against SQL Server.
- Payment architecture tests: 13 passed.
- Compatibility tests: 4 passed against candidate baseline `v1`.
- Superseded Payment source identities (`BookingId`, `ContextId`, `ConsumerCorrelation`, `CustomerPayment`, `ManagerPayment`, Customer/Auth registration events, consumer seed catalogs) scan empty.

## Reviews

The canonical producer review is `reviews/Feature-payment-method-commitments.md`. Its existing findings are closed, but its watermark predates the breaking agnosticism work. Run a fresh full review after Delivery items 4–6 are green, then stamp the exact reviewed head in a separate reviews-only commit.

## Downstream handoffs

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` — worktree `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`, PR [#633](https://github.com/Concertable/concertable/pull/633). Gate: the final Payment Contracts and Client packages publish and B2B's platform pin advances; B2B then migrates directly from the raw surface to the final reference surface.
- `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PROGRESS.md` — no worktree yet. Gate: the same package publish; Customer then migrates its on-session purchase flow directly to the final session-operation surface.

## Decisions, discoveries, blockers, and deviations

- The final invariant is stronger than provider-id ownership: Payment contains no B2B or Customer product knowledge. `Booking`, `Concert`, `Ticket`, `Application`, `Opportunity`, and `Manager` are forbidden product vocabulary; Stripe's own `Customer` object and Payment's `Escrow`, `Commission`, `Settlement`, `PayoutAccount`, `Ledger`, `Payer`, and `Payee` vocabulary remain legitimate.
- `PaymentOperationReference(OperationType, ClientReference)` is the only consumer-correlation primitive. Payment stores and compares both opaque strings but never interprets consumer values. Escrow uniqueness is the composite pair, not `ClientReference` alone.
- Public payment, escrow, transfer, refund, and integration-event outcomes expose only Payment-owned operation identities and status. Payment resolves provider object identifiers privately from the opaque reference when an internal handler needs one.
- Removing the integer booking correlation changes persisted column types and the settlement fingerprint payload. Payment has no deployed rows, so the migration story is an initial-migration re-scaffold; `SettlementOperationFingerprint.CurrentVersion` still advances for explicit hash identity.
- The final package surface separates durable session operations, settlement operations, payment reporting, and escrow operations. Customer's old role-specific gRPC service and all raw payment-method/intent-id APIs are removed rather than renamed.
- `PaymentMethodChargeError.AuthenticationRequired` remains distinct from new-method recovery and carries no provider artifact; consumers re-enter by operation reference.
- `KeyedUnionBuilder` and the B2B Deal union wrapper remain consumer-owned typed-escalation machinery. Only their obsolete payment-method-based union usage is removed during the B2B migration.
