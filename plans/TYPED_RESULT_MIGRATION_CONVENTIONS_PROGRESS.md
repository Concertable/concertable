# Typed Result convention update progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\TypedErrorRepresentation`
- Branch: `Docs/TypedErrorRepresentation`
- Correction PR: [#340 — docs(api): simplify typed error representation](https://github.com/Concertable/concertable/pull/340)
- PR: [#335 â€” docs(api): codify typed error union conventions](https://github.com/Concertable/concertable/pull/335)
- Dependency/package gates: docs-only; no package or platform-sync consequence
- Last reconciled: 2026-08-04 from `origin/main` `c45b33740`

## Current state

PR #335 merged the first convention revision. This correction removes its positional `Match`
pattern: payload-free errors are sealed definition records, and Dunet is used only when alternatives
carry data or require case discrimination. Necessary unions declare `Definition` abstract on the root
and override it on every case. No runtime code or package contract is changed.

## Next Steps

Merge docs-only correction PR #340 through the `merge-docs` workflow.

## Completed work

- Reconstructed the plan baseline from merged Phase 1 PR #290, platform-sync PR #291, current open
  PRs #296 and #282, and the Phase 2 ledger on `Feature/CommissionBindingDeferredPricing`.
- Updated `api/agents/CODE_CONVENTIONS.md` with the concrete Dunet declaration pattern and general
  owned Result/Option rules missing from the shorter convention.
- Updated `plans/TYPED_RESULT_MIGRATION.md` to match the convention and identify PR #296 as the only
  owner of Payment Phase 2.
- Captured the docs and reconstructed baseline in commit `eb87a6225`.
- Pushed `eb87a6225` to `origin/Docs/TypedResultConventions` and opened draft PR #335.

## Verification

- `git diff --check` â€” passed against the uncommitted docs working tree on 2026-08-04.
- Repo-wide markdown scan found no other plan prescribing an operation-error `Definition` switch or
  conflicting Dunet definition convention.
- Before push, `git rev-list --count HEAD..origin/main` returned `0`; after push, local and remote work
  heads both resolved to `eb87a6225c49cc0d4002411d3da07b9f8f15abc2`.
- No build or test run is required for markdown-only changes.

## Reviews

No code review has run yet.

## Decisions, discoveries, blockers, and deviations

- Use sealed definition records for payload-free alternatives and Dunet only where alternatives
  carry data or require case discrimination.
- Necessary Dunet unions declare `Definition` abstract on the root and override it on every case.
- Generated full `Match` remains available only for other genuinely exhaustive owner-local logic.
- Public codes remain explicit under today's Kernel API. A future type-derived code helper and
  `ErrorCode` override must be a shared Kernel change with exact contract tests, not a service-local
  convention.
- Domain-behavior messages remain explicit; only the existing DisplayName-backed standard not-found
  template is derived.
- The shared convention change belongs to this docs PR. Payment implementation remains branch-local
  to `Feature/PaymentOwnedResultExpansion`; PR #296 is frozen donor state for that phase.
- The first `Docs/TypedErrorConvention` worktree was removed by a concurrent cleanup before commit;
  the diff was reconstructed in the flat `Docs-TypedResultConventions` worktree without touching any
  Payment worktree.

## Event log

### 2026-08-04 â€” reconstructed baseline and convention decision

- Action: reconciled the legacy plan with current repository and PR evidence, then documented the
  owned-error and general Result/Option conventions.
- Evidence: PR #290 merged, PR #291 merged, PRs #296 and #282 open, Phase 2 branch ledger, current docs
  diff, and successful `git diff --check`.
- Outcome: the canonical convention and migration plan now prescribe the same current Dunet API and
  future native-union seam; PR #296 is recorded as the sole Phase 2 owner.
- Follow-up: push the work commit and open the requested docs PR.

### 2026-08-04 â€” docs PR opened

- Action: pushed the verified docs commit and opened draft PR #335.
- Evidence: local and `origin/Docs/TypedResultConventions` both resolved to
  `eb87a6225c49cc0d4002411d3da07b9f8f15abc2`; GitHub returned PR
  `https://github.com/Concertable/concertable/pull/335`.
- Outcome: the requested docs PR is open and contains only the convention, canonical plan, and this
  progress ledger.
- Follow-up: review PR #335; merge only on Tommy's explicit instruction, then resume Phase 2 on #296.

### 2026-08-04 — corrected error representation convention

- Action: replaced the positional `Definition.Match` convention with payload-free sealed definition
  records and abstract-root/per-case definitions for necessary Dunet unions.
- Evidence: the branch is based on `origin/main` `c45b33740`, which contains PR #335 merge
  `5af2bcb64`; `git diff --check` passed and every changed path is markdown.
- Outcome: the convention now matches the verified Payment implementation shape and the
  DisplayName-backed message-less not-found rule.
- Follow-up: commit, push, open, and merge the docs-only correction PR.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs-TypedResultConventions
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
