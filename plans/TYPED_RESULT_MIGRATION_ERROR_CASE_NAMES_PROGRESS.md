# Typed Result natural error-name convention progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\NaturalErrorCaseNames`
- Branch: `Docs/NaturalErrorCaseNames`
- PR: not opened
- Dependency/package gates: docs-only; no package or platform-sync consequence
- Last reconciled: 2026-08-04 from `origin/main` `9dfb5e63d`

## Current state

The corrected error representation convention from merged PR #340 remains in force. This follow-up
specifies that operation-error static values and union cases use natural domain names directly, with
no `Case` suffix or wrapper factory that merely aliases the same case. Each name must agree with its
`ErrorDefinition`; for example, `PayerNotFound` is valid only with NotFound semantics.

## Next Steps

Verify the markdown-only diff, commit it, push the branch, and open the docs PR. Do not merge it.

## Completed work

- Confirmed PR #340 merged as `d41b5c47ff188d6b36b51669fe4b6802206ff4ec`.
- Refined the canonical code convention and migration plan around natural, semantically accurate
  operation-error names.
- Left the Payment implementation worktree untouched.

## Verification

- `git diff --check` passed on 2026-08-04.
- The branch is `0` commits behind `origin/main`.
- Every changed path is markdown; no build or test run is required.

## Reviews

No code review is required for this docs-only refinement. GitHub review is pending.

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

## Resume prompt

```text
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\NaturalErrorCaseNames
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_ERROR_CASE_NAMES_PROGRESS.md and do what its `## Next Steps` says.
```
