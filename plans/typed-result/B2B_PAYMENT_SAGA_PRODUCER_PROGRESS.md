# B2B Payment saga producer progress

- Plan: `plans/typed-result/B2B_PAYMENT_SAGA_PRODUCER_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/b2b-payment-saga-producer`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-typed-result_payment-financial-saga`
- Branch: `Refactor/typed-result_payment-financial-saga`
- PR: #544, `https://github.com/Concertable/concertable/pull/544`
- Base reconciliation: `64fc7f8e2` includes current main `2e6e0cc78`
- Review watermark: `64fc7f8e2`; runtime security review through `9f59b6da8`; no open findings
- Package gate: Messaging prerequisite is published and synced; Payment publication authorized

## Current state

All producer phases are implemented through `6458ec0d0`. Payment.Contracts owns additive capture,
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

The producer is reconciled with current main and platform `0.1.0-alpha.0.968`. It now consumes the
released `Reunion` and `Reunion.Errors` `0.1.0-alpha.3` artifacts containing producer commit
`113be42`; no Reunion extension was copied or recreated locally.

## Next Steps

Reconcile the open PR's `DIRTY` state with current `origin/main`, rerun the smallest affected build and
focused tests if the merge changes the Payment closure, then push the reconciled head. Use its exact-head
PR CI as the authoritative full build, carve, unit, and integration gate. After CI and review are green,
re-enqueue with `full-e2e`. Merge only after the queue E2E passes, then follow Payment package publication
and cumulative platform sync before resuming B2B.

## Completed work

- Topology scan: one Payment producer package layer and one B2B consumer hop; no Customer source change.
- Producer worktree created from current `origin/main`.
- Phases 1-3: exact Reunion closure, additive Contracts surface, Payment operation journal,
  booking-idempotent Stripe calls, pending-refund recovery, and regenerated initial migration are in
  this commit.
- Phase 4: exact package closure, standalone runtime carve, package-ownership inventory, and repository
  compatibility gates are complete.
- Messaging PR #536 merged as `5c4dc3ddf`; its package publication and cumulative platform-sync PR
  #541 are terminal and green.
- Current main was reconciled in `15de28fb8`; Payment now uses released Reunion alpha.3.
- Tommy explicitly authorized pushing, opening, and merging the Payment producer on 2026-08-13.
- Published work head `a26895d80` is verified equal across local, remote, and PR #544; `full-e2e` is
  applied because the branch adds published cross-service command/event contracts.
- Current main was reconciled in `64fc7f8e2`; its remote-validation workflow/docs changes introduced
  no Payment runtime change, and the guidance conflict preserves both policies.
- Merge-group run `31705953582` failed the flat-fee acceptance API E2E because booking-derived Stripe
  idempotency keys collided with keys retained from an earlier test run. Its diagnostics also exposed
  that the three financial-operation command queues were registered for handling but absent from the
  Payment AppHost topology.
- Stripe retry identity is now carried explicitly as a financial operation ID or commission binding ID;
  metadata is observability-only, legacy calls without either identity receive no custom key, and the
  shared private formatter owns the Stripe key shape. Payment topology now provisions capture, deposit,
  and refund command queues. The regression fix and its focused coverage are checkpointed in this commit.
- Verified work push: starting remote/PR head `e53a2661126322977f2446a4afe54494702e08f1`, pushed range
  `e53a26611..da0fd3b6e`, and work/local/remote/PR head
  `da0fd3b6e18cf92d8acb729252609c67c21da1f3`. PR #544 remained open and reported `DIRTY`, so current-main
  reconciliation is the next action before authoritative exact-head CI and queue admission.

## Verification

- Focused saga/idempotency unit tests: 10/10.
- Full Payment unit suite on the reconciled head: 253/253.
- Payment E2E helper unit suite: 5/5.
- Full Payment integration suite: 9/9, including SQL persistence through the regenerated migration.
- Docker fresh-container HTTP data round-trip: green.
- Payment Web Release build against exact Reunion `113be42`: 0 errors, 0 warnings.
- Payment solution Release build against exact Reunion `113be42`: 0 errors.
- Reconciled Payment solution Release build: 0 errors, 0 warnings.
- Full API Release build: 0 errors; four existing B2B/Customer nullable warnings.
- Source-only Payment carve from `5aaf13d`: Web, Workers, and Client each build with 0 errors and 0
  warnings against package dependencies. The aggregate solution additionally contains repository-level
  AppHost, E2E helper, and integration-fixture project references and is not itself a standalone carve.
- Exact CI-style standalone Payment carve from `9f59b6da8`: all nine deployable projects restored from
  published dependencies and built in Release with 0 errors and 0 warnings.
- Package ownership inventory: the new commands and events exist only in Payment.Contracts, Payment
  runtime, and Payment tests; Payment.Contracts and Payment.Client have no B2B or Customer references.
- Scoped whitespace format and verification: green.
- Regression fix focused Payment service/adapter unit slice: 58/58.
- Financial-operation topology unit test: 1/1.
- Regression fix Payment AppHost build: 0 errors, 0 warnings.
- Regression fix scoped Payment format and `git diff --check`: green. The solution-wide format command
  still reports pre-existing Shared Kernel whitespace and B2B namespace findings outside this change.
- Initial migrations re-scaffold: every unchanged context retained its ID; Payment regenerated with
  `FinancialOperations` and the unique nullable refund `OperationId` index.
- Plan graph: 0 errors and 0 warnings; `git diff --check` green.
- Current-session integration attempt: 5/9 passed; four fixtures failed during Testcontainers Docker
  named-pipe startup before test execution. This is the sole remaining local delivery gate.
- Full correctness/security review through `9f59b6da8`: clean; no open findings.

## Decisions and deviations

- Strict runtime isolation: B2B sends only Payment-owned Contracts messages; Payment handlers,
  idempotency, and persistence stay inside Payment.
- Scope is SEC1 accept/withdraw/cancel capture, deposit, and refund. Finish/settlement is outside this
  finding.
- Normal Payment configuration consumes the released Reunion alpha.3 family containing `113be42`.

## Downstream handoffs

- Waiting ledger: `plans/typed-result/B2B_PROGRESS.md` on branch
  `Refactor/B2BTypedResultMigration` in worktree
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`.
  Gate: a green Payment integration rerun enables the Payment producer merge; Payment publication plus
  generated platform sync then enables final B2B merge-ready revalidation.
