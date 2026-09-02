# Repository-per-microservice migration — Stage 3 RT3 progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage3-Hosting-rt3`
- Branch: `Plan/RepoSplit-Stage3-Hosting-rt3` at `f26d26abfb2830aa1157d2739ad8c45274562d3c`
- PR: [#897](https://github.com/Concertable/concertable/pull/897) — open, unmerged, and carrying the `full-e2e` label
- Dependency/package gates: published platform `0.1.0-alpha.0.1281` is available; Stage 4 is merged on `main`
- Last reconciled: **2026-09-02** from the exact branch/PR head, CI run `33647092121`, and the Stage 4 merge

## Current state

RT3 exclusively owns the standalone AppHost cutover from foreign source references to published Hosting
packages and digest-pinned service containers. The implementation is complete. The branch contains current
`main` at `ac74fdf9a0687a436872a7c1c4da622126e7885b`, including the Stage 4 container-aware E2E repair, and
is zero commits behind `main`. Exact-head PR CI run `33647092121` passed at `f26d26a`.

PR #897 was briefly closed on 2026-09-02 solely to stop an in-progress merge attempt, then immediately
reopened. It was not merged and no source work was lost.

## Next Steps

Own the fresh exact-head CI triggered by this ledger-only checkpoint. If it passes, return PR #897 to the
merge queue with its existing `full-e2e` label under Tommy's standing merge authorization. Own the
merge-group API/UI E2E to a terminal result, repair only genuine RT3 regressions on this branch, and confirm
the merged commit on `main`.

After RT3 lands, this stream is complete. Repository promotion then proceeds through the canonical plan's
single-writer cutovers: refresh each extracted service from the approved final monorepo SHA, freeze that
monorepo path, validate and publish from the service repository, switch package/image consumers, then remove
the frozen monorepo source. Auth follows checkpoints 10A–10E; ongoing Auth changes in the monorepo are allowed
until 10A and must be included in that exact refresh.

## Completed work

- Hosting seam and digest repairs landed through PRs #870, #881, #888, and #892.
- Platform `0.1.0-alpha.0.1281` published successfully in run `33408113198` and was merged into the RT3 candidate.
- All five standalone AppHosts built in Release package mode against `1281`; inventory and diff gates passed.
- Service AppHost implementation files/classes use the canonical local `AppHost.cs` / `AppHost` names.
- Stage 4's container-backed E2E support was merged from `main`; the sole merge conflict in
  `eng/repository-split/inventory.json` passed exact-head CI.

## Verification

Focused composition suites, all five package-mode AppHost builds, split inventory, and diff checks passed.
Exact-head PR CI run `33647092121` passed at `f26d26abfb2830aa1157d2739ad8c45274562d3c`.
The incremental review from the previous watermark found no issue in the requested AppHost renames, E2E
Hosting-package closure additions, or the Stage 4 merge resolution.

## Reviews

The prior native, security, persistence, test-impact, and repository review was approved through
`438744ed7d150eb76c72d494c19bc6cb280176a5`. Incremental review through `f26d26abfb2830aa1157d2739ad8c45274562d3c`
found no open RT3 finding.

## Decisions, discoveries, blockers, and deviations

- RT3 consumes four foreign images: Auth, Payment Web, Payment Workers, and B2B Seed Simulator; image references remain immutable digests.
- A pinned Auth image does not transfer Auth source ownership. The monorepo remains Auth's writer until the explicit checkpoint-10 refresh/freeze/publish cutover.
- There is no ongoing bidirectional source synchronization. Each service promotion performs one final monorepo-to-service refresh and then flips to the service repository as the sole writer.
- The local command runner was unavailable during this checkpoint (`unsupported protocol version 5`), so branch, PR, file, comparison, and CI evidence were verified through GitHub directly.
