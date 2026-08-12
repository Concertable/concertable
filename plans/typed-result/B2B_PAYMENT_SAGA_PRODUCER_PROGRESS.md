# B2B Payment saga producer progress

- Plan: `plans/typed-result/B2B_PAYMENT_SAGA_PRODUCER_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/b2b-payment-saga-producer`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-typed-result_payment-financial-saga`
- Branch: `Refactor/typed-result_payment-financial-saga`
- PR: not opened
- Base: `origin/main` `e10fd17fa20ee91b04d0a738275314312c73cd6b9`
- Package gate: producer implementation authorized; publication not requested

## Current state

All producer phases are implemented and verified at `5aaf13d769e6ddeea84e7b6d820215dfee223157`. Payment.Contracts owns additive capture,
deposit, refund, success, rejection, and deferred messages. Payment.Web handles commands without any
B2B runtime reference. A Payment-owned journal persists intent before Stripe, commits terminal outcome
plus outbox message together, replays terminal outcomes, and re-enters pending operations after a
process failure. Capture/deposit are booking-idempotent; refund reservations carry operation IDs and
resume the same Stripe-idempotent request after a crash.

Reunion commit `113be42f532d5d7e8daf1c362262ff7a7854b7bc` is verified through exact net10 artifacts:
`Reunion` SHA-256 `36F5C1C66BD9D63DFD180AEF69D266FDF05FB5EEDBE7573DCEB326063129A9A2`,
`Reunion.Errors` `993E8F966BEDEF06C94D8D8FDC28C89A7856BCFCB6DD21980CE64F329FD82544`, and
the released `Reunion.AspNetCore` artifact
`5BCE01783D79B99F60FB1F848560B04563169C9346A84CF02815E483A5E8767C`. All are version
`0.1.0-local.113be42` under `artifacts/reunion-113be42`. Temporary local source and version inputs are
restored and are not part of the candidate.

The exact producer packages are `Concertable.Payment.Contracts.0.1.0-alpha.0.947.nupkg` with
SHA-256 `34D5AA954FE40FEF54759783C57240914FBA5F0286EB0C17103A39544775CFCB` and
`Concertable.Payment.Client.0.1.0-alpha.0.947.nupkg` with SHA-256
`3FB139300483AC9564F15D92578539E3712AF91D8C873FC9D803BAD5C172BB2C`. Both record source commit
`5aaf13d769e6ddeea84e7b6d820215dfee223157` and are under `artifacts/payment-saga-5aaf13d`.

## Next Steps

Copy the exact Payment and Reunion artifact closure to the B2B worktree and implement the consumer
phase recorded in `plans/typed-result/B2B_PROGRESS.md`. Keep this producer branch unpushed and
unpublished until the consumer is locally verified and a later delivery instruction authorizes the
publish-first merge sequence.

## Completed work

- Topology scan: one Payment producer package layer and one B2B consumer hop; no Customer source change.
- Producer worktree created from current `origin/main`.
- Phases 1-3: exact Reunion closure, additive Contracts surface, Payment operation journal,
  booking-idempotent Stripe calls, pending-refund recovery, and regenerated initial migration are in
  this commit.
- Phase 4: exact package closure, standalone runtime carve, package-ownership inventory, and repository
  compatibility gates are complete.

## Verification

- Focused saga/idempotency unit tests: 10/10.
- Full Payment unit suite: 249/249.
- Full Payment integration suite: 9/9, including SQL persistence through the regenerated migration.
- Docker fresh-container HTTP data round-trip: green.
- Payment Web Release build against exact Reunion `113be42`: 0 errors, 0 warnings.
- Payment solution Release build against exact Reunion `113be42`: 0 errors.
- Full API Release build: 0 errors; two existing generated nullable warnings in the B2B UI E2E project.
- Source-only Payment carve from `5aaf13d`: Web, Workers, and Client each build with 0 errors and 0
  warnings against package dependencies. The aggregate solution additionally contains repository-level
  AppHost, E2E helper, and integration-fixture project references and is not itself a standalone carve.
- Package ownership inventory: the new commands and events exist only in Payment.Contracts, Payment
  runtime, and Payment tests; Payment.Contracts and Payment.Client have no B2B or Customer references.
- Scoped whitespace format and verification: green.
- Initial migrations re-scaffold: every unchanged context retained its ID; Payment regenerated with
  `FinancialOperations` and the unique nullable refund `OperationId` index.
- Plan graph: 0 errors and 0 warnings; `git diff --check` green.

## Decisions and deviations

- Strict runtime isolation: B2B sends only Payment-owned Contracts messages; Payment handlers,
  idempotency, and persistence stay inside Payment.
- Scope is SEC1 accept/withdraw/cancel capture, deposit, and refund. Finish/settlement is outside this
  finding.
- Normal Payment configuration still pins Reunion core alpha.1. The producer candidate is locally
  verified against exact `113be42` artifacts and remains delivery-gated on their published successor;
  no local feed or disposable package pin is committed.

## Downstream handoffs

- Waiting ledger: `plans/typed-result/B2B_PROGRESS.md` on branch
  `Refactor/B2BTypedResultMigration` in worktree
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`.
  Gate: exact locally packed Payment artifacts enable preparation; publication plus generated platform
  sync enables final merge-ready revalidation.
