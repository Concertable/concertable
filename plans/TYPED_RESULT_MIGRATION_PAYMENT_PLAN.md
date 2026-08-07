# Payment owned-result expansion plan

- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Progress ledger: `plans/TYPED_RESULT_MIGRATION_PAYMENT_PROGRESS.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\PaymentOwnedResultExpansion`
- Branch: `Feature/PaymentOwnedResultExpansion`
- Status: delivery in progress

## Objective

Replace Payment's published FluentResults client surface with Concertable-owned typed
`Result`/`Option` contracts while keeping Payment independently buildable and preserving stable error
codes across its gRPC boundary. Deliver the breaking Payment package before migrating B2B and
Customer through the generated platform-sync PR.

## Scope

- Payment-owned client interfaces and adapters.
- Operation-specific Payment error unions and structured gRPC error transport.
- Reviewed-gross persistence and validation for commission-bound money movement.
- Escrow release/refund idempotency through `Result<Option<T>, TError>`.
- Payment application, domain, infrastructure, protobuf, migration, and test changes required by the
  cutover.
- Package publication, generated platform-sync consumer migration, and the B2B handoff.

## Non-goals

- Source-level compatibility shims for the removed FluentResults interfaces.
- Local source references from B2B or Customer to bypass package publication.
- Migrating unrelated service-owned Result contracts in this branch.
- Running local E2E before the merge queue requests it.

## Contract decisions

- Payment exposes only the owned-result interfaces; the legacy published clients are removed in one
  intentional breaking package cutover.
- Every operation error is a named Dunet union with one exhaustive root `Definition` switch.
- Published codes remain stable through `[ErrorCode]` where natural case naming would change them.
- Client reverse mapping is closed per operation and rejects unknown or changed wire contracts with
  `PaymentContractMismatchException`; `FromCode` parser chains are not used.
- Payment persists one exact payer-reviewed `Money` gross per commission binding and rejects
  unconfirmed or different bound money movement before Stripe.
- Successful escrow release/refund no-ops return `Option.None`; executed operations return
  `Option.Some`.

## Phases

- [x] Establish the owned error and structured gRPC foundation.
- [x] Replace application, infrastructure, and client operations with owned Result/Option contracts.
- [x] Remove FluentResults clients, compatibility adapters, and obsolete registrations.
- [x] Persist and enforce the reviewed gross at bound money-movement boundaries.
- [x] Reconcile donor behavior, review findings, migrations, and standalone package isolation.
- [x] Align every Payment error and reverse mapper with the current repository convention.
- [x] Complete Payment SQL integration on a healthy Docker engine.
- [ ] Incrementally review and push the current verified branch to PR #392.
- [ ] Run full merge-queue E2E and follow PR #392 to a terminal state.
- [ ] Own package publication and the generated breaking platform-sync PR through green.
- [ ] Wake the blocked B2B ledger, close donor PR #296, and close out this plan and ledger together.

## Verification gates

Before pushing the delivery head:

1. Build `api/Concertable.Payment/Concertable.Payment.slnx` in Release with zero errors and warnings.
2. Pass all Payment unit tests.
3. Pass all Payment SQL integration tests after the Docker preflight succeeds.
4. Build `api/Concertable.slnx` in Release with zero errors.
5. Confirm Payment contains no FluentResults, `ToLegacy`, `FromCode`, obsolete client interfaces,
   static error catalogs, or per-case `Definition` overrides.
6. Run the incremental code review required for commits added after the canonical review watermark.

The merge queue must run full E2E because this is a breaking published-package cutover. After merge,
publication and the generated platform-sync PR are live gates rather than follow-up cleanup.

## Closeout

Keep this plan and its progress ledger until PR #392, package publication, the generated platform-sync
consumer migration, the B2B handoff, and donor PR closure are terminal. Then transfer the final state
to the required docs closeout worktree and delete both files together.
