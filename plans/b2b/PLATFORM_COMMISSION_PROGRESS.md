# Percentage platform commission and pricing transparency progress

- Plan: `plans/b2b/PLATFORM_COMMISSION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\CommissionBindingDeferredPricing`
- Branch: `Feature/CommissionBindingDeferredPricing`
- PR: [#296 — Own deferred commission pricing in Payment](https://github.com/Concertable/concertable/pull/296)
- Dependency/package gates: Phase 1 is recorded complete. Phase 1b is implemented on PR #296 but cannot enter the publish/platform-sync/deployment gate until the PR merges; this work stops at the PR.
- Last reconciled: 2026-08-03 from a fresh origin fetch, the plan, git/worktree state, PRs #296 and #312, platform-sync history, `reviews/Feature-CommissionBindingDeferredPricing.md`, Docker engine state, and the preserved working tree.

## Current state

Reconstructed baseline: Phase 1b's Payment-owned deferred commission binding is committed through
`357a2ca7d`, with additional typed-result and review-fix work restored in the working tree. The branch
merged current `origin/main` at `bd494a25f`, including PR #314 and Payment's atomic refund reservation
work. The pre-merge dirty state remains recoverable from stash `76438f7cf003438a313be9049be708c1f72c6990`.

The restored work replaces expected Payment failures with owned typed results across application,
infrastructure, gRPC, and published clients. Review findings CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are
implemented in the working tree but remain open until the affected verification gate passes. OWN1's
normalized immutable configuration history remains present after the base merge and also awaits fresh
verification against this exact combined state.

The recalled Payment-owned-result refactor is this worktree. It is intentionally branch-local to
`Feature/CommissionBindingDeferredPricing` because it changes commission and deferred-pricing code that
exists only on PR #296, not yet on `main`; creating a separate typed-result branch would split the
in-flight feature. The same work also implements the Payment expansion described by Phase 2 of
`plans/TYPED_RESULT_MIGRATION.md`.

A fresh fetch found local `HEAD` `bd494a25f` 21 commits behind `origin/main` `5a84756de` and 22 commits
ahead. The incoming range has extensive Payment overlap, including commission contracts, domain
entities, gRPC/client adapters, escrow/refund services, migrations, and tests. PR #296 remains open and
non-draft at remote head `357a2ca7d`; GitHub reports `DIRTY`, and its green checks verify only that old
remote head. The current staged, unstaged, and untracked owned-result work must be preserved before a
main merge. Existing stash `76438f7cf003438a313be9049be708c1f72c6990` predates the latest review-fix
edits and is recovery evidence, not a complete substitute for a new snapshot.

## Exact next action

Create a fresh named stash including staged, unstaged, and untracked paths; verify both that snapshot and
the older recovery stash exist. Merge current `origin/main` into the clean worktree, restore the fresh
snapshot, and resolve the substantial Payment overlap while preserving both main's newer boundary/entity
changes and this branch's commission plus owned-result behavior. Then run the Payment unit suite, the
Payment SQL integration project through `integration-debug`, the full Release solution build, standalone
Payment carve, and EF pending-model check against the reconciled tree. If every gate is green, mark the
review findings fixed, update both affected plan ledgers, and commit the verified work. Do not push without
an explicit push instruction and do not merge PR #296.

## Completed work

- Phase 1 is checked complete in the plan, including percentage configuration, immutable history,
  bindings, additive RPCs, transaction/refund facts, migrations, and its earlier verification gate.
- Phase 1b implementation commits include `f93aa0c6b` (Payment-owned binding), `e1f4de726`
  (percentage value model and normalized ownership), and `e73b30bb4` (calculation contract alignment).
- OWN1 was previously resolved and reviewed through `99ef2faac`; the review records immutable SQL
  configuration revisions referenced by bindings, with currency retained on the binding.
- Current `origin/main` (`c7e4d97e9`, PR #314) was merged locally as `bd494a25f` while preserving and
  restoring every pre-existing working-tree path.

## Verification

- Historical evidence recorded by the plan/review for OWN1: 141 Payment unit tests passed, 7 Payment
  integration tests passed, no pending Payment model changes, full solution build at 0 errors, and
  standalone Payment carve at 0 errors. This evidence predates the current typed-result working tree.
- PR #296 head `357a2ca7d` previously had green CI, including Payment unit, Payment integration, solution
  build, and Payment carve; those checks also predate the current uncommitted fixes and base merge.
- Focused BUG1 regressions: 2 passed, 0 failed on the combined `bd494a25f` plus working tree.
- Payment unit tests: 188 passed, 0 failed on the combined working tree.
- `dotnet build api/Concertable.slnx`: succeeded with 0 errors and 3 warnings.
- Standalone Payment deployable carve, copied from the current working tree and built in Release from
  its package closure: succeeded with 0 errors.
- `dotnet ef migrations has-pending-model-changes` for `PaymentDbContext`: no model changes since the
  last migration.
- Payment integration preflight: `docker ps` timed out after 30 seconds. Docker Desktop processes were
  responsive but its engine was unavailable, so Testcontainers was not started and the 7-test result
  remains pending.
- Resume reconciliation: elevated `docker ps` answered successfully with an empty container list, so
  the earlier engine-unavailable blocker is no longer evidenced. No integration test was run, and the
  21-commit current-main reconciliation must happen before final verification.

## Reviews

- Review artifact: `reviews/Feature-CommissionBindingDeferredPricing.md`.
- OWN1 — fixed and historically verified; fresh reconciliation verification pending.
- Incremental range `f2e206133..e73b30bb4` — no findings.
- CI follow-up range `e73b30bb4..99ef2faac` — no findings.
- Incremental range `99ef2faac..357a2ca7d` plus typed-result working tree:
  CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are implemented but remain open pending fresh verification
  of the Payment integration project and the completing commit. Their focused/unit/build evidence is green.

## Decisions, discoveries, blockers, and deviations

- PR #314 supplied the repository-owned `resume-plan` workflow and merged before this resume.
- Worktree identity is valid: the owned-result changes are branch-local work over unmerged PR #296
  commission code, so they belong in this checkout despite also satisfying Typed Result migration Phase 2.
- The branch is 21 commits behind current main and the incoming range overlaps Payment heavily. Final
  verification on `bd494a25f` would be stale; preserve all dirty state, sync, restore, and reverify first.
- `origin/main` replaced optimistic refund concurrency tokens with atomic conditional reserved-gross
  updates. Conflict resolution retained that implementation together with PR #296's percentage VAT
  model and typed transition results; the obsolete concurrency-token path was not restored.
- The generated Payment migration rename had identical competing blobs. The newer main timestamp
  `20260802215519_InitialCreate` is the surviving filename, with its migration attribute reconciled.
- PR #296 is open and was already non-draft before this resume. Its remote head remains `357a2ca7d`
  until the verified work and its push checkpoint are published.

## Event log

### 2026-08-03 — reconstructed baseline

- Action: Reconstructed this ledger from the plan, git history, PR #296, review findings, and recorded verification.
- Evidence: Plan checkboxes; commits `f93aa0c6b`, `e1f4de726`, `e73b30bb4`, `99ef2faac`, and `357a2ca7d`; PR #296 metadata and checks.
- Outcome: Durable current state and exact remaining gate established without fabricating unavailable session history.
- Follow-up: Reverify the combined working tree before closing review findings.

### 2026-08-03 — preserved work and merged current main

- Action: Snapshotted all tracked and untracked changes, fetched origin, merged `origin/main`, resolved Payment model overlap, and reapplied the snapshot.
- Evidence: Retained stash `76438f7cf003438a313be9049be708c1f72c6990`; `origin/main` `c7e4d97e9`; merge commit `bd494a25f`; clean unmerged-path check.
- Outcome: PR #296 is current with the fetched base locally and every original dirty path remains recoverable.
- Follow-up: Complete review fixes and fresh verification before push.

### 2026-08-03 — review reconciliation before verification

- Action: Reconciled OWN1 and every open incremental finding against the restored code and merged base.
- Evidence: Explicit collaborator constructors; typed transition checks; exhaustive definition tests; Payment client transport tests; malformed-trailer fallback; atomic refund reservation integration.
- Outcome: CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are implemented in the working tree; dispositions remain open until tests and builds pass.
- Follow-up: Run the required Payment and build gates.

### 2026-08-03 — local verification partially complete

- Action: Ran the focused BUG1 tests, full Payment unit suite, full solution build, standalone Payment carve, EF model check, and Payment integration Docker preflight.
- Evidence: 2/2 focused tests; 188/188 Payment unit tests; solution and carve at 0 errors; no pending Payment model changes; `docker ps` timed out after 30 seconds.
- Outcome: Code, unit, ownership, and model gates are green. The Payment SQL integration gate could not start because the Docker engine is unresponsive.
- Follow-up: Restart Docker Desktop, run the 7 Payment integration tests once, then complete review dispositions, commit, verified push, and PR-ready state.

### 2026-08-03 - resume reconciliation against current main

- Action: Located the recalled Payment-owned-result work across every repository worktree, fetched origin,
  reconciled the active plan/ledger and review, inspected PRs #296 and #312 plus platform-sync history,
  compared the dirty branch with current main, and rechecked Docker engine availability.
- Evidence: target worktree `Feature/CommissionBindingDeferredPricing`; local `HEAD` `bd494a25f` is 21
  commits behind `origin/main` `5a84756de`; the incoming range changes many of the same Payment files;
  PR #296 is open/non-draft and `DIRTY` at remote head `357a2ca7d`; PR #312 remains open/CLEAN at
  `d82077bd`; no platform-sync PR is open; elevated `docker ps` succeeds; staged, unstaged, and untracked
  owned-result paths remain preserved and untouched.
- Outcome: The old Docker preflight blocker has cleared, but current-main reconciliation is now the first
  gate. No application code, review finding, PR, or remote branch was changed during this resume.
- Follow-up: Snapshot all dirty paths, merge current main, restore and resolve the Payment overlap, then
  repeat the complete affected verification gate before committing; do not push without explicit approval.
