# Typed Result convention update progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs-TypedResultConventions`
- Branch: `Docs/TypedResultConventions`
- PR: [#335 â€” docs(api): codify typed error union conventions](https://github.com/Concertable/concertable/pull/335)
- Dependency/package gates: the docs change is independent; Phase 1 merged in PR #290 and synced in PR #291; Phase 2 is owned by PR #296; Phase 3 PR #282 waits for Phase 2 publication and platform sync
- Last reconciled: 2026-08-04 after opening PR #335 and verifying its pushed work head

## Current state

The docs-only branch updates the API convention and canonical migration plan. Dunet error unions use
one centralized positional full `Match` for `Definition` and other genuinely exhaustive owner-local
mappings. The docs also make factory/case naming, exact public-contract tests, explicit messages,
default/null behavior, and the future native-union seam explicit.

The docs change is committed as `eb87a6225`, pushed with local and remote heads equal, and open as
draft PR #335. No runtime code or package contract is changed.

## Next Steps

Review PR #335 and make it ready or merge it only on Tommy's explicit instruction. After it lands,
continue Payment Phase 2 only on PR #296 and follow that branch's own ledger.

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

- Use Dunet's generated full `Match` now; do not replace the definition with per-case overrides or an
  ordinary C# switch.
- Preserve the `Match`-shaped consumer API during the eventual released native-union cutover so
  preview null/default switch arms do not leak into service code.
- Keep `Case` when it prevents a collision with the natural factory name; remove it only for a
  genuinely distinct case name.
- Public codes remain explicit under today's Kernel API. A future type-derived code helper and
  `ErrorCode` override must be a shared Kernel change with exact contract tests, not a service-local
  convention.
- Domain-behavior messages remain explicit; only the existing DisplayName-backed standard not-found
  template is derived.
- The shared convention change belongs to this docs PR. Payment implementation remains branch-local
  to PR #296 because its error surface includes that branch's unmerged commission work.
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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs-TypedResultConventions
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
