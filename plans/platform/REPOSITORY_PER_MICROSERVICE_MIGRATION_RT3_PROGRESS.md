# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage3-Hosting-rt3`
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` at `438744ed7d150eb76c72d494c19bc6cb280176a5`
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — open; exact-head CI running
- Dependency/package gates: published platform `0.1.0-alpha.0.1281` is available; no implementation blocker remains
- Last reconciled: **2026-08-31** from the exact branch/PR head, package run `33408113198`, and completed review work order

## Current state

RT3 exclusively owns the standalone AppHost cutover from foreign source references to published Hosting
packages and digest-pinned service containers. The implementation, package-mode builds, and final incremental
review are complete at the exact pushed PR head. No sibling may edit its AppHosts, composition tests, review
work order, branch, PR, or this ledger.

## Next Steps

Own [PR #897](https://github.com/Concertable/concertable/pull/897) exact-head CI to a terminal result, address
any failure on the RT3 branch, and keep the review watermark current after substantive repairs. Do not queue
or merge the PR without Tommy's explicit authorization.

## Completed work

- Hosting seam and digest repairs landed through PRs #870, #881, #888, and #892.
- Platform `0.1.0-alpha.0.1281` published successfully in run `33408113198` and was merged into the RT3 candidate.
- All five standalone AppHosts built in Release package mode against `1281`; inventory and diff gates passed.

## Verification

Focused composition suites, all five package-mode AppHost builds, split inventory, and diff checks passed at
the reviewed head. Exact-head PR CI is in progress.

## Reviews

`reviews/Plan-RepoSplit-Stage3-Hosting-rt3.md` is complete and approved through
`438744ed7d150eb76c72d494c19bc6cb280176a5`, including security review, with no open RT3 findings.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image references remain immutable digests.
- The RT3 owner must merge this newly tracked ledger from `origin/main` and carry later material updates on its substantive branch commits.
