# Repository and DbContext permission hierarchy producer progress

- Plan: `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-context-permission-hierarchy`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Refactor-data-access-repository-permission-hierarchy-expand`
- Branch: `Refactor/DataAccessRepositoryPermissionHierarchyExpand`
- PR: [#590](https://github.com/Concertable/concertable/pull/590) (merged)
- Dependency/package gates: satisfied. PR #590 merged, published `0.1.0-alpha.0.1007`, and
  platform-sync PR #592 merged green.
- Last reconciled: 2026-08-17 from the active contraction worktree at `95305c7a9`.

## Current state

The additive package slice is implemented in the producer worktree. It changes only the two packable
DataAccess projects and their focused unit tests: `IWriteDbContext`, `IDbContext`, `ReadDbContext`,
`WriteRepository<TEntity>`, `Repository<TEntity, TKey>`, and the additive protected
`ReadRepository.Context` property are present while every legacy public shape remains available.

The six source files are byte-equivalent to the already reviewed producer portion of #561. No B2B,
Customer, Payment, or other service consumer source is part of this slice. Focused tests, exact local
package preparation, diff checks, and the plan graph are green. The reviewed work head
`d2c3b346320a96ebc404c731a124263d0c66af8c` is pushed and verified equal to the remote branch; draft
PR #590 is open. Exact-head CI run `31898015544` packed the additive platform successfully, then found
that the new shared `ReadDbContext` simple name collided with Customer's existing compatibility base
in three inheritance clauses. Those clauses now explicitly alias the existing Customer base, preserving
their old-package behavior until the consumer migration removes it.

## Next Steps

Completed workstream. The remaining legacy contraction is owned by
`REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PROGRESS.md`.

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
- Exact-head CI run `31898015544` failed only on ambiguous Customer `ReadDbContext` inheritance after
  successfully packing the additive platform; it was not retried.
- Customer Web built with 0 errors against exact local platform `0.1.0-local.1786813897648` after the
  three explicit Customer-base aliases were added.
- Replacement exact-head CI run `31898750230` passed on `3116eb40d`: 50 jobs green, including local
  platform pack, full solution build, all five service carves, and selected unit/integration matrices.
- Green platform-sync PR #588 merged as `0c9ec894a`, advancing the platform baseline to
  `0.1.0-alpha.0.1005`. It merged cleanly into this branch at `aefcdee73`; incremental review found no
  overlap. The package-sensitive Customer build exceeded the five-minute local command cap without a
  compiler diagnostic, so replacement exact-head CI remains the authoritative post-merge gate.

## Reviews

- Formal review of `1f4ea1f72..ff7cdc954` found no issues across correctness, microservice isolation,
  module boundaries, seeding, C# conventions, or changed-path test coverage. Review artifact:
  `reviews/Refactor-DataAccessRepositoryPermissionHierarchyExpand.md`.
- Incremental review of `ff7cdc954..07bf600f5` found no issues; the review watermark now resolves to
  exact commit `07bf600f5d86bb980215c46e97f4ad4262e25ab7`.
- Incremental review of checkpoint-only range `07bf600f5..3116eb40d` found no issues; the local review
  watermark matches the exact remote PR head.
- Incremental review of `3116eb40d..aefcdee73` found no issues; it covered only the incoming green
  platform sync and merge commit.
- Plan-managed work-head push succeeded for `895020d39..3575330ce`; local, remote-tracking, and PR
  heads were verified equal at `3575330ce19149d5d97f30d44d2d0c1ce283c4e4`.
- Current-main work-head push succeeded for `3116eb40d..aefcdee73`; local, remote-tracking, and PR heads
  were verified equal at `aefcdee7340f60ae0e3b9c9c30f70b7d1e33b872`.

## Decisions, discoveries, blockers, and deviations

- Published layer: `Concertable.DataAccess.Application` and `Concertable.DataAccess.Infrastructure`;
  both republish in the same platform release.
- Direct consumers that migrate source: B2B, Customer, and Payment. No published consumer package
  re-exposes the new types into another service layer.
- The consumer PR remains delivery-ready rather than merge-ready until this producer's real package is
  published and synchronized.
- A new shared simple type name can be source-breaking when a consumer already imports a same-named
  compatibility type. The expansion keeps those consumers on their existing base through explicit
  aliases; their real migration remains in #561.

## Downstream handoffs

- `plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PROGRESS.md` resumes when the published
  platform baseline contains the additive DataAccess API and the generated sync is merged.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-data-access-repository-permission-hierarchy-expand
Read @plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PLAN.md and @plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PRODUCER_PROGRESS.md and do what its `## Next Steps` says.
```
