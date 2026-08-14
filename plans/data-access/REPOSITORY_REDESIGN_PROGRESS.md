# Repository redesign progress

- Plan: `plans/data-access/REPOSITORY_REDESIGN_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/repository-redesign`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/data-access_repository-redesign_closeout`
- Branch: `Docs/data-access_repository-redesign_closeout`
- Delivery PR: #530 — https://github.com/Concertable/concertable/pull/530
- Platform sync PR: #558 — https://github.com/Concertable/concertable/pull/558
- Last reconciled: 2026-08-14 — implementation, review, PR CI, merge-queue E2E, package publication, restore verification, and platform sync are terminal.

## Current state

Terminal. The repository redesign is live across the shared DataAccess packages and all service consumers. Shared `ReadRepository` and `WriteRepository` own read and write behavior once; the flat `Repository` facade composes both over one tracked module context. Dedicated read repositories use the shared read base with their no-tracking contexts. The legacy write-facet names and transitional Customer read abstractions are gone.

PR #530 merged as `32ce3ae273a1f3ea5c7ebced36eef5b2e64cbdca` after exact-head PR CI and the `full-e2e` merge-group passed. Publish run `31754611697` shipped and restore-verified platform version `0.1.0-alpha.0.980`. Generated platform-sync PR #558 rebuilt the published consumer closure and merged as `1f48a2597be28ff864f258030ae9a35f6d649469` after merge-group run `31755783027` passed.

## Next Steps

Commit this terminal evidence checkpoint. In the following commit, tick `data-access/repository-redesign` in the roadmap and delete this plan and ledger together. Run docs review, then land the closeout through the docs-only merge path and remove the closeout worktree.

## Completed work

- PR #522 introduced Customer no-tracking read contexts and merged with platform sync green.
- PR #526 introduced the query-only `IReadDbContext` capability and merged with platform sync green.
- PR #530 fixed the source-platform test seam, centralized shared read/write behavior through composition, renamed the write-only facet to `IWriteRepository`/`WriteRepository`, migrated every consumer, and added the enforcing tests and CI carves.
- Local verification packed 40/40 platform projects; the Release solution build completed with 0 errors; 23/23 unit projects passed 1,075 tests; 16/16 integration projects passed 407 tests; every integration output contained exactly one expected-version DataAccess assembly.
- Review covered the complete PR-B diff plus the later CI argument-forwarding fix. No open findings remain.
- PR #530 exact-head CI run `31751090737` passed build, every backend carve, 23 unit projects, 16 integration projects, and `ci-complete` at `fe97f1359c65b5ef4e35a02dfc82a336cee3650b`.
- PR #530 merge-group run `31752363966` completed successfully at `32ce3ae273a1f3ea5c7ebced36eef5b2e64cbdca`; the PR merged at 2026-08-13 23:39:34 UTC.
- Publish run `31754611697` completed successfully; both `publish` and `verify-restore` passed for `0.1.0-alpha.0.980`.
- Platform-sync PR #558 PR-CI run `31754749312` attempt 1 hit an MCR connection reset while pulling SQL Server before any test body ran. Its single fresh-run retry passed all 55 jobs or policy skips.
- Platform-sync PR #558 merge-group run `31755783027` completed successfully; the PR merged at 2026-08-14 00:11:30 UTC.

## Decisions and durable constraints

- Combined repositories compose read and write facets over one tracked module context so read-mutate-save remains one unit of work. Dedicated read repositories inherit the read facet and receive a no-tracking read context.
- The write-only facet remains because keyless sequences and syncers require it; its legacy names and read behavior do not.
- Published-platform consumer tests must use the locally packed source platform consistently. Mixing source-built platform assemblies with feed-compiled consumers can hide or create binary compatibility failures.
- Follow-up debt remains in `api/Concertable.DataAccess/TECH_DEBT.md`: seal `GetByIdAsync`, and add the separately named duplicate-aware insert operation when adopted.
