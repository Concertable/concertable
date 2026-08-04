# Percentage platform commission and pricing transparency progress

- Plan: `plans/b2b/PLATFORM_COMMISSION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\CommissionBindingDeferredPricing`
- Branch: `Feature/CommissionBindingDeferredPricing`
- PR: [#296 — Own deferred commission pricing in Payment](https://github.com/Concertable/concertable/pull/296)
- Dependency/package gates: Phase 1 is recorded complete. Phase 1b is implemented on PR #296 but cannot enter the publish/platform-sync/deployment gate until the PR merges; this work stops at the PR. As of 2026-08-04 the branch is **current with `origin/main`** and every requested local gate is green; the remaining pre-push step is `/incremental-review` of the post-merge commit.
- Last reconciled: 2026-08-04 after merging current `origin/main` `f05f8832d` into the branch (merge commit `b6fb56c6c`), resolving the single `plans/AGENTS.md` doc conflict, and re-running the full Payment verification gate green.

## Current state

Phase 1b's Payment-owned deferred commission binding and owned-result expansion are implemented on
PR #296 and are now reconciled with current `origin/main`. Local `HEAD` is the unpushed merge commit
`b6fb56c6c` ("Merge remote-tracking branch 'origin/main' …"), which brought `origin/main` `f05f8832d`
(26 commits, including the Concert-owner-response / typed-result DTO reshuffle and the scripts-into-`scripts/`
move) onto the branch. The **only** merge conflict was `plans/AGENTS.md` (docs), resolved by taking
main's newer parallel-worktree / multi-ledger handoff text. Every Payment source file auto-merged; the
solution and standalone Payment carve build clean, so no Payment code overlap needed hand resolution.

Working tree is **clean**. The branch is **0 commits behind** `origin/main` and ahead of its remote
tracking ref `origin/Feature/CommissionBindingDeferredPricing` (whose tip, and PR #296's head, is still
`f487ad1da` — nothing since is pushed). PR #296 is OPEN, non-draft; `mergeStateStatus` was `UNKNOWN`
(GitHub still computing) and it carries no checks because it has never entered the queue.

The five commits between the previously recorded `4946bba27` and the merge base were all doc-only
`docs(plan)` ledger/plan-conformance commits; no code changed between `4946bba27` and this merge.
The recovery stash `76438f7cf003438a313be9049be708c1f72c6990` remains intact.

## Next Steps

Run `/incremental-review` against the existing PR #296 review watermark
(`reviews/Feature-CommissionBindingDeferredPricing.md`) on merge commit `b6fb56c6c`. Address any new
findings, then update this ledger. Do not push or merge PR #296 unless Tommy separately requests it.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\CommissionBindingDeferredPricing
Read @plans/b2b/PLATFORM_COMMISSION.md and @plans/b2b/PLATFORM_COMMISSION_PROGRESS.md and do what its `## Next Steps` says.
```

## Completed work

- Phase 1 is checked complete in the plan, including percentage configuration, immutable history,
  bindings, additive RPCs, transaction/refund facts, migrations, and its earlier verification gate.
- Phase 1b implementation commits include `f93aa0c6b` (Payment-owned binding), `e1f4de726`
  (percentage value model and normalized ownership), `e73b30bb4` (calculation contract alignment),
  and `f693c955d` (owned typed results and review fixes).
- The earlier verified push checkpoint is `f487ad1da`.
- This commit merges current `origin/main` `37c94cd0`, retains main's Payment boundary/entity changes,
  resolves the branch-only accessibility gap, and disambiguates the independently generated Client
  and Infrastructure proto types in the combined Payment unit project.

## Verification

All results below are from the merge commit `b6fb56c6c` (`origin/main` `f05f8832d` merged with the
PR #296 work):

- Docker preflight: `scripts/docker-health.ps1` reported healthy (fresh-container host→container data
  round-trip stable) — note the health/e2e scripts moved to `scripts/` on main.
- `dotnet build api/Concertable.slnx --configuration Release`: 0 errors, 9 warnings (pre-existing
  nullable/analyzer warnings only).
- Complete Payment unit project in Release: 192 passed, 0 failed.
- Payment SQL integration project: 7 passed, 0 failed.
- Standalone Payment carve (`api/Concertable.Payment/Concertable.Payment.slnx`, Release,
  `MinVerSkip=true`): 0 errors.
- `dotnet ef migrations has-pending-model-changes` for `PaymentDbContext`: no model changes since the
  last migration (canonical parseable design-time connection string from `api/initial-migrations.ps1`).

## Reviews

- Review artifact: `reviews/Feature-CommissionBindingDeferredPricing.md`.
- OWN1, CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are fixed and remain closed after current-main
  reconciliation and the complete fresh verification gate.
- The merge added main's Payment.Domain internalisation and payout-status boundary plus a focused
  test-only extern alias required by the branch's Payment.Client reference. These post-review changes
  require `/incremental-review` before any push.

## Decisions, discoveries, blockers, and deviations

- Worktree identity remains valid: the owned-result changes operate on commission code that exists
  only on PR #296, so they remain branch-local rather than moving to a separate typed-result branch.
- The pre-merge working tree was clean. A synthetic empty stash was not created; the verified
  recovery stash remains the durable pre-merge recovery artifact.
- Main's internal-default change must cover branch-only Domain additions too. Keeping those types
  public would silently reintroduce the tech debt main just resolved.
- Both Payment.Client and Payment.Infrastructure generate the same protobuf CLR names by design.
  The unit project references both only for combined coverage, so an explicit Infrastructure assembly
  alias preserves exact enum assertions without coupling or weakening either package boundary.
- The first EF invocation built successfully but was correctly rejected by the design-time connection
  guard. Re-running with the exact non-secret parseable value from `initial-migrations.ps1` passed.
- No local E2E was run: PR #296's merge queue remains the E2E gate.

## Event log

### 2026-08-04 — merged current main and re-ran the full Payment gate green

- Action: In the worktree, `/resume-plan` confirmed both ledgers point at PR #296 with an identical
  merge→verify→`/incremental-review` next action. Fetched origin, merged `origin/main` `f05f8832d` into
  the branch, resolved the single conflict (`plans/AGENTS.md`, docs — took main's parallel-worktree
  handoff text), completed merge commit `b6fb56c6c`, and ran the full Payment verification gate.
- Evidence: 26 incoming commits auto-merged including main's Concert-owner-response/typed-result DTO
  reshuffle and the `scripts/` relocation; `git grep` found no conflict markers; branch now 0 behind
  `origin/main`. Release solution build 0 errors/9 warnings; Payment unit 192/192; Payment integration
  7/7; standalone Payment carve 0 errors; `PaymentDbContext` reports no pending model changes; Docker
  healthy via `scripts/docker-health.ps1`.
- Outcome: The reviewed-and-verified Phase 1b state is now current with `origin/main` on merge commit
  `b6fb56c6c` with every local gate green. The five commits since the old `4946bba27` HEAD were doc-only.
- Follow-up: Run `/incremental-review` on `b6fb56c6c` against `reviews/Feature-CommissionBindingDeferredPricing.md`;
  do not push or merge PR #296 without explicit approval.

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

### 2026-08-03 — Payment integration gate green and review findings fixed

- Action: Confirmed `docker ps` responded, then ran the complete Payment SQL integration project once.
- Evidence: `dotnet test api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj --logger console;verbosity=normal` passed 7/7, including both concurrent partial-refund tests, on `bd494a25f` plus the completing working tree.
- Outcome: OWN1 is freshly reconciled; CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are fixed. All requested Phase 1b local verification gates are green without further code changes.
- Follow-up: Commit the verified state, execute the plan-managed two-leg push with exact remote/PR head equality, and confirm PR #296 is ready without merging it.

### 2026-08-03 — verified work-head push to PR #296

- Action: Committed the verified Payment result/error-boundary fix as `f693c955d`, then pushed range `357a2ca7d..f693c955d` to `origin/Feature/CommissionBindingDeferredPricing`.
- Evidence: After fetching the branch, local `HEAD`, `origin/Feature/CommissionBindingDeferredPricing`, and PR #296 `headRefOid` all equalled `f693c955d60fd21b5e6586ec9a6f52f53dddbfd1`; the PR remained open and non-draft.
- Outcome: Push leg 1 is verified. The work commit contains the fixed CV1, BUG1, CV2, TEST1, TEST2, and BUG2 dispositions plus the green local gate evidence.
- Follow-up: Create and transport one plan-managed checkpoint commit, verify final local/remote/PR equality, then confirm PR #296 ready without merging.

### 2026-08-04 — current-main reconciliation and complete local gate

- Action: Fetched origin, verified PR #296 and the recovery stash, merged `origin/main` `37c94cd0`,
  resolved the Payment overlap, fixed the generated-proto test collision, and ran every requested gate.
- Evidence: PR #296 remained open/non-draft at remote head `f487ad1da`; stash `76438f7c` retained its
  index and untracked parents; Payment integration 7/7; focused mapper 4/4; Payment unit 192/192;
  Release solution and standalone Payment carve at 0 errors; EF reported no pending model changes.
- Outcome: Main's newer Payment boundaries/entities and the branch's commission/owned-result behavior
  coexist in a fully verified local merge commit. All recorded review findings remain closed.
- Follow-up: Run `/incremental-review` for this commit before any push; do not merge PR #296.

### 2026-08-04 — ledger reconciliation to companion-ledger style

- Action: Brought this ledger into the current companion-ledger template (renamed `## Exact next action`
  to `## Next Steps`) and reconciled its current-state sections against fresh durable evidence. No code,
  verification, review, or push was performed. Confirmed this branch owns `plans/b2b/PLATFORM_COMMISSION.md`
  (all `origin/main..HEAD` commits are commission/`payment`; the plan is tracked on `origin/main` and
  heavily edited here) and that `plans/TYPED_RESULT_MIGRATION*` is inherited — `TYPED_RESULT_MIGRATION.md`
  already exists on `origin/main` and its `_PROGRESS.md` arrived via merge commit `4946bba27`; left untouched.
- Evidence: `git status` clean, ahead 28 of `origin/Feature/CommissionBindingDeferredPricing` (tip
  `f487ad1da`); local `HEAD` `4946bba27`; `git rev-list --count HEAD..origin/main` = 20, `origin/main`
  `a22b379bf`; `gh pr view 296` OPEN/non-draft/`DIRTY`/headRefOid `f487ad1da`; `gh pr checks 296` = "no
  checks reported"; review artifact `reviews/Feature-CommissionBindingDeferredPricing.md` present.
- Outcome: Ledger now reflects that the reviewed-and-verified state is the unpushed `4946bba27` merge,
  which is 20 commits behind current `origin/main` with PR #296 `DIRTY`. `## Next Steps` updated to require
  a base update + re-verify + `/incremental-review` before any push.
- Follow-up: Do the base update and re-verify in this worktree per `## Next Steps`; do not push or merge PR #296 without explicit approval.
