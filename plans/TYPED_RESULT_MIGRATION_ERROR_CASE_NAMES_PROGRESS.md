# Typed Result natural error-name convention progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\NaturalErrorCaseNames`
- Branch: `Docs/NaturalErrorCaseNames`
- PR: [#343 — docs(api): prefer natural typed error names](https://github.com/Concertable/concertable/pull/343)
- Dependency/package gates: docs-only; no package or platform-sync consequence
- Last reconciled: 2026-08-04 from `origin/main` `9dfb5e63d`

## Current state

PR #343 is open at remote head `5b3bf3dee9e818328f66ab3541c751d384abd9bd`. Its checks are terminal
and green/skipped for the docs-only diff, but GitHub reports `DIRTY`: the branch is nine commits
behind `origin/main`, and merged PR #344 changed the same convention and plan area. The naming rule
must be reconciled with current main before the PR can enter the merge queue.

## Next Steps

Merge `origin/main` into `Docs/NaturalErrorCaseNames`, reconcile the PR #344 overlap without losing
the natural, semantically accurate error-name rule, verify the markdown diff, push the updated
branch, then re-run the merge workflow for PR #343.

## Completed work

- Confirmed PR #340 merged as `d41b5c47ff188d6b36b51669fe4b6802206ff4ec`.
- Refined the canonical code convention and migration plan around natural, semantically accurate
  operation-error names.
- Committed the docs change as `7d2c88b38299c87d1bccc703c586dec54a7529e0`, pushed it, and opened
  PR #343.
- Left the Payment implementation worktree untouched.

## Verification

- `git diff --check` passed on 2026-08-04.
- The branch is `0` commits behind `origin/main`.
- After the work push, local HEAD and `origin/Docs/NaturalErrorCaseNames` both resolved to
  `7d2c88b38299c87d1bccc703c586dec54a7529e0`.
- Every changed path is markdown; no build or test run is required.

## Reviews

GitHub review is pending on PR #343.

## Decisions, discoveries, blockers, and deviations

- Use natural domain vocabulary such as `ApplicationNotFound`, `ApplicationError`, `PayeeNotFound`,
  and `RecipientUnavailable` directly.
- Do not append `Case` to operation-error names.
- Do not add a wrapper factory that only renames or constructs the same case.
- A name and its `ErrorDefinition` must agree: `PayerNotFound` uses NotFound semantics; a broader or
  different definition requires an honestly broader or different name.
- The sealed-record/static-value and necessary-Dunet/per-case-definition representation convention
  from PR #340 is unchanged.

## Event log

### 2026-08-04 — natural error-name refinement requested

- Action: reconciled the preferred natural names with the existing per-case definition convention.
- Evidence: the current convention examples already pair `PayerNotFound` with
  `ErrorDefinition.NotFound`; the migration plan now makes that semantic agreement mandatory.
- Outcome: the docs distinguish stable Result/Option factories from operation-error case identities.
- Follow-up: verify, commit, push, and open the docs-only PR.

### 2026-08-04 — docs PR opened

- Action: pushed the verified work commit and opened PR #343.
- Evidence: local and remote work heads matched at
  `7d2c88b38299c87d1bccc703c586dec54a7529e0`; GitHub returned
  `https://github.com/Concertable/concertable/pull/343`.
- Outcome: the requested docs PR is open and the Payment implementation remains untouched.
- Follow-up: review PR #343; merge only on Tommy's explicit instruction.

### 2026-08-04 — merge preflight found stale conflicting base

- Action: fetched current `origin/main` and inspected PR #343 before queue admission.
- Evidence: `origin/main...HEAD` was `9 2`; GitHub reported `mergeStateStatus=DIRTY`; the remote PR
  head remained `5b3bf3dee9e818328f66ab3541c751d384abd9bd`; all PR checks were terminal with `ci-complete`,
  `changes`, and `instant-merge` passing and runtime jobs skipped for the docs-only diff.
- Outcome: queue admission is blocked until current main is merged and the PR #344 overlap is
  reconciled.
- Follow-up: update, verify, and push the source branch before enqueueing.

## Resume prompt

```text
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\NaturalErrorCaseNames
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_ERROR_CASE_NAMES_PROGRESS.md and do what its `## Next Steps` says.
```
