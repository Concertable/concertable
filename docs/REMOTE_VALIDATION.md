# Remote validation workflow

Concertable supports several feature agents working concurrently on one development machine. The
workstation is the inner loop; GitHub Actions is the full validation plane. Do not turn every
worktree into an independent CI runner.

## Ownership of each gate

| Gate | Owner |
|---|---|
| Formatting, generators, grep/invariant checks | Local worktree |
| Smallest changed project/surface build | Local worktree |
| Focused unit tests for changed behavior | Local worktree |
| Full solution build and standalone service carves | Draft-PR CI |
| Complete unit and integration matrices | Draft-PR CI |
| API and UI E2E selected by the merge policy | Merge queue |

Local integration or E2E runs are diagnostic tools, not routine completion gates. Run the narrowest
failing test, class, module, or scenario locally after CI or the merge queue reports a failure. A
developer may explicitly request a broader local run, but plans and skills must not add one “to be
safe.”

## Delivery loop

1. Implement in the branch's own worktree.
2. Run the cheapest checks that directly cover the changed code: required generators/invariants, the
   smallest buildable project or frontend surface, and focused unit tests.
3. Commit each coherent checkpoint. At the first checkpoint, push and open a draft PR if none exists.
4. Push later coherent checkpoints to that draft PR without waiting for another instruction. A new
   push supersedes the prior remote validation run.
5. Treat CI on the exact remote head as the authoritative build, carve, unit, and integration result.
6. On failure, inspect the remote log first, reproduce only the failing scope locally, fix it, run its
   focused regression, then push once. Do not run every local suite before pushing the fix.
7. Mark the PR ready only after review is clean and exact-head PR CI is green. Enqueue or merge only
   when Tommy explicitly asks.

Draft-PR creation and checkpoint pushes are part of an authorized implementation workflow. They do
not authorize merge, deployment, destructive operations, or unrelated external changes.

## Resource discipline

- Never run `dotnet build api/Concertable.slnx`, every unit project, or the full integration matrix as
  routine local verification.
- Do not launch local integration or E2E suites concurrently from different worktrees. While one
  diagnostic run owns Docker/Testcontainers, other agents continue implementation or wait for remote
  checks.
- Batch related fixes before pushing so CI validates useful checkpoints. GitHub cancels an obsolete
  pull-request run when a newer commit reaches the same PR.
- Historical plan/ledger evidence remains historical. Reconcile only outstanding instructions to this
  policy.

## Exceptions

Some work cannot obtain meaningful remote feedback until an artifact is published or a platform-sync
PR exists. Follow the package cut-over and platform-sync workflows for those delivery gates. A
targeted local build against an exact producer artifact is still valid; it does not restore the old
full-local-gate default.
