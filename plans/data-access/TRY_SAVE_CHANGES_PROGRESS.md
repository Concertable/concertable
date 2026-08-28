# Shared TrySaveChanges progress

- Plan: `plans/data-access/TRY_SAVE_CHANGES_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/try-save-changes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Chore-TechDebt-dataaccess-trysave`
- Branch: `Chore/TechDebt-dataaccess-trysave`
- PR: not opened
- Dependency/package gates: DataAccess must publish and the generated platform sync must merge before Payment can consume the helper
- Last reconciled: `2026-08-28` against `origin/main` `3b7e3e56d0411a73e55589794006a23b3fcedf9f`

## Current state

The producer implementation is complete locally. A C# 14 extension on `DbContext` owns concurrency
recovery, and the existing unit-of-work contract exposes it without leaking a context to services.

## Next Steps

Land the producer expand PR, then migrate every direct implementation in its generated platform-sync PR.

## Completed work

- Added `TrySaveChangesAsync` to `IUnitOfWork<TContext>` and delegated it through the `DbContext` extension.

## Verification

- DataAccess unit tests: 19 passed, 0 failed.

## Reviews

- Producer review pending.

## Decisions, discoveries, blockers, and deviations

- The helper handles only `DbUpdateConcurrencyException`; duplicate-key handling remains separate debt.
- The contract change requires publish then sync because Payment consumes the packaged assembly.
- The generated platform-sync PR is expected to fail with `CS0535` until Payment's direct unit-of-work
  implementations adopt the published member; that migration belongs in the sync PR.
