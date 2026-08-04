# Typed Result convention update progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs-TypedResultConventions`
- Branch: `Docs/TypedResultConventions`
- PR: not opened
- Dependency/package gates: the docs change is independent; Phase 1 merged in PR #290 and synced in PR #291; Phase 2 is owned by PR #296; Phase 3 PR #282 waits for Phase 2 publication and platform sync
- Last reconciled: 2026-08-04 from `origin/main`, PR metadata, the Phase 2 branch ledger, and this working tree

## Current state

The docs-only branch updates the API convention and canonical migration plan. Dunet error unions use
one centralized positional full `Match` for `Definition` and other genuinely exhaustive owner-local
mappings. The docs also make factory/case naming, exact public-contract tests, explicit messages,
default/null behavior, and the future native-union seam explicit.

The docs change is complete and included in this work commit. No runtime code or package contract is
changed.

## Next Steps

Push `Docs/TypedResultConventions` and open the requested docs-only PR. Record the PR identity and
verified remote head in this ledger. Do not merge it without Tommy's explicit instruction. After the
docs PR lands, continue Payment Phase 2 only on PR #296 and follow that branch's own ledger.

## Completed work

- Reconstructed the plan baseline from merged Phase 1 PR #290, platform-sync PR #291, current open
  PRs #296 and #282, and the Phase 2 ledger on `Feature/CommissionBindingDeferredPricing`.
- Updated `api/agents/CODE_CONVENTIONS.md` with the concrete Dunet declaration pattern and general
  owned Result/Option rules missing from the shorter convention.
- Updated `plans/TYPED_RESULT_MIGRATION.md` to match the convention and identify PR #296 as the only
  owner of Payment Phase 2.
- Captured the docs and reconstructed baseline in this work commit.

## Verification

- `git diff --check` â€” passed against the uncommitted docs working tree on 2026-08-04.
- Repo-wide markdown scan found no other plan prescribing an operation-error `Definition` switch or
  conflicting Dunet definition convention.
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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs-TypedResultConventions
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
