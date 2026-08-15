# Repository and DbContext permission hierarchy producer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Refactor-data-access-repository-permission-hierarchy-expand`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchyExpand`
- PR: not opened
- Dependency/package gates: current platform-sync PR #588 was pending but had no failed check when this
  branch was created from `origin/main` at `1f4ea1f72`; refresh current main before delivery.
- Last reconciled: 2026-08-15 against the extracted producer diff and package topology.

## Current state

The additive package slice is implemented in the producer worktree. It changes only the two packable
DataAccess projects and their focused unit tests: `IWriteDbContext`, `IDbContext`, `ReadDbContext`,
`WriteRepository<TEntity>`, `Repository<TEntity, TKey>`, and the additive protected
`ReadRepository.Context` property are present while every legacy public shape remains available.

The six source files are byte-equivalent to the already reviewed producer portion of #561. No B2B,
Customer, Payment, or other service consumer source is part of this slice. Focused tests, exact local
package preparation, diff checks, and the plan graph are green.

## Next Steps

1. Commit the additive producer checkpoint with the plan graph, run a fresh branch review, and resolve
   any findings.
2. Refresh current-main currency, then push, open a draft PR, and follow exact-head CI.
3. Use the already authorized delivery chain to merge
   the producer and follow package publication plus the generated platform-sync PR to green and merged.
4. Update the consumer ledger when the published baseline gate opens, close this producer worktree,
   and resume #561 from current `origin/main`.

## Completed work

- Scanned the published package topology and fixed the delivery sequence at one DataAccess expansion,
  one direct-consumer migration, and one DataAccess contraction.
- Extracted the additive DataAccess package surface from #561 without copying service consumers.

## Verification

- Producer files compare byte-for-byte with the reviewed implementations on #561.
- DataAccess unit tests passed 12/12 in Release.
- `./scripts/local-platform.ps1 prepare` produced 40 packages at exact local version
  `0.1.0-local.1786813897648`, including both DataAccess packages.
- `git diff --check` passed.
- Plan graph passed with 0 errors and 0 warnings.

## Reviews

- The extracted code was reviewed as part of #561, but this branch diff still requires its own formal
  review before delivery.

## Decisions, discoveries, blockers, and deviations

- Published layer: `Concertable.DataAccess.Application` and `Concertable.DataAccess.Infrastructure`;
  both republish in the same platform release.
- Direct consumers that migrate source: B2B, Customer, and Payment. No published consumer package
  re-exposes the new types into another service layer.
- The consumer PR remains delivery-ready rather than merge-ready until this producer's real package is
  published and synchronized.

## Downstream handoffs

- `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PROGRESS.md` resumes when the published
  platform baseline contains the additive DataAccess API and the generated sync is merged.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-data-access-repository-permission-hierarchy-expand
Read @plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md and @plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PRODUCER_PROGRESS.md and do what its `## Next Steps` says.
```
