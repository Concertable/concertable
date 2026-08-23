# Architecture-tests rename — progress

- Plan: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_PLAN.md`
- Roadmap: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_ROADMAP.md`
- Roadmap item: `architecture-tests-rename/tier-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Refactor/architecture-tests-rename`
- Phase 1 PR: [#746](https://github.com/Concertable/concertable/pull/746) — **MERGED** (`685f66ec9`)
- Phase 1 platform-sync: [#749](https://github.com/Concertable/concertable/pull/749) — **MERGED**
  (`6a5db574c`, version `0.1.0-alpha.0.1149`)
- Phase 2 branch: `refactor/architecture-tests-rename_phase2-package-rename`
- Phase 2 PR: [#754](https://github.com/Concertable/concertable/pull/754) — **MERGED** (`1d25c3b58`)
- Phase 2 platform-sync: [#758](https://github.com/Concertable/concertable/pull/758) — **OPEN**, auto-merge
  armed by automation, all checks green when last observed; stuck in the merge queue behind unrelated
  CI/runner congestion (not a code problem)

## Current state — Phase 1 terminal, Phase 2 code merged, sync PR in flight

Phase 1 landed. PR #746 merged into `main` (`685f66ec9`); the resulting `publish-packages` republish and
`chore/platform-sync-0.1.0-alpha.0.1149` (#749) both went green and merged (`6a5db574c`) with no consumer
migration needed (package-only pin bump). `main` is current, local checkout resynced to `6a5db574c`. The
Composition tier is gone: six composition-test projects are now `*.ArchitectureTests`, with B2B's dynamic
host-graph tests folded into its existing `Concertable.B2B.ArchitectureTests` (as `B2BHostGraphTests`)
alongside the static ArchUnit rules. Tier gate, CI leg, `test.ps1` suite, skill route, and all `.slnx` files
updated. The shared published lib `Concertable.Composition.Testing` is intentionally untouched (Phase 2), so
the change was non-breaking.

Currency required three rounds of merging `origin/main` in during review (`9e44faeb9`, `aaedcdc4d`,
`6062892f9`) because this repo's `main` was moving fast from concurrent work — each merge was clean, no
conflicts. Local full-solution build repeatedly hit a persistent `MSB3021 Access denied` copying pre-existing
DLLs to `bin/` (and separately on `scripts/test.ps1`) — a local sandbox/environment file-lock unrelated to
this branch's diff, confirmed harmless via targeted builds and exact-head PR CI (which built clean every
time). After the PR merged, `git checkout main && git pull --ff-only` also hit the same lock mid-fast-forward
and left `main`'s ref stale with a partially-updated working tree; recovered by advancing the ref directly
(`git update-ref refs/heads/main refs/remotes/origin/main`, which never writes files) then `git reset`
(index-only) plus a handful of targeted `git checkout HEAD -- <file>` calls for the small set of files that
had drifted — `scripts/test.ps1` itself remains permanently un-restorable in this working tree and is
flagged separately for Tommy to investigate (not a branch or plan concern).

**Phase 2** landed as one PR rather than the plan's original producer/sync split — see the plan doc's
"Deviation" note for why (this repo's `UseLocalPlatformPackages` CI mechanism makes the split actively
counterproductive here). PR #754 merged clean through the queue after three more `origin/main` merges (same
fast-moving-main pattern) and a security-layer review (touches `Concertable.Auth`/`Concertable.Payment`
paths per this repo's `security_paths` inventory — no findings, confirmed no actual auth/payment code
changed). The same `checkout main` file-lock issue recurred after this merge too, plus a second untracked-
stale-file cleanup (leftover pre-closeout copies of another team's `admin-console` plan/review files from
the same interrupted-checkout pattern — confirmed via `git log --diff-filter=D` that they were already
`git rm`'d on `main`, so deleting the stale on-disk copies was safe). The resulting platform-sync PR #758
(`0.1.0-alpha.0.1161`) is a routine, no-consumer-migration-needed version bump; automation already armed its
auto-merge and every check that completed was green, but it's been stuck in the merge queue for an extended
period behind what looks like genuine CI-runner congestion (merge_group runs staying `in_progress` well past
normal completion time, unrelated PRs' queue entries repeatedly rebatching). This is an environment issue,
not a code or process one — no action needed from this session; automation will land it once the queue
clears.

## Completed milestones

- Categorization verdict + full reference inventory gathered (subagents).
- Plan/roadmap/ledger created (`2284dcb21`).
- Phase 1 tier collapse implemented, built green, pushed (`868a9868a`).

## Latest verification

- `dotnet build api/Concertable.slnx -c Debug`: **0 errors** (12 pre-existing warnings, none from this change).
- `dotnet test` Concertable.AppHost.ArchitectureTests: **3/3 passed**.
- `dotnet test` Concertable.B2B.ArchitectureTests (static ArchUnit + folded host-graph): **17/17 passed**.
- Grep gate: no `CompositionTests` token remains in tracked source but the two unrelated unit classes
  (`TypedErrorCompositionTests`, `ConcertWorkflowCompositionTests`) and the deliberate Phase-2 survivors
  (`Concertable.Composition.Testing` lib name + its `using`/pins).

## Reviews

Phase 1: `reviews/Refactor-architecture-tests-rename.md` (spent — Phase 1 fully merged, findings fixed;
delete once Phase 2 also closes, per review-lifecycle). Reviewed up to `45d746d7d`, re-stamped three times
as main's pace forced repeated merges. Native layer: no findings. Repo lenses: two findings, fixed —
`docs/INDEX.md` named the pre-rename CI matrix; the `.ArchitectureTests` skill route was missing
`module-structure`. Security layer: no findings.

Phase 2: `reviews/refactor-architecture-tests-rename_phase2-package-rename.md` (spent — PR #754 merged, no
open findings; delete once the sync PR closes). Reviewed up to `ae68e5299`, re-stamped three times across
main merges (each pass confirmed the true branch-owned diff was byte-identical to the first, so no
re-review was needed beyond the first pass). Native layer: no findings. Security layer (triggered by the
`Concertable.Auth`/`Concertable.Payment` path touches): no findings, confirmed no actual auth/payment
production code changed.

## Next Steps

Confirm platform-sync PR #758 reached `MERGED` (it was green and auto-merge-armed, just stuck behind CI
queue congestion when last checked — `gh pr view 758 --json state,mergeCommit`). Once it's merged: this
plan's whole rename is terminal — run the grep gate
(`grep -rniE "composition\.testing|compositiontests"`, expect only the two allowlisted unit-test classes),
`git rm` both spent review files (`reviews/Refactor-architecture-tests-rename.md` and
`reviews/refactor-architecture-tests-rename_phase2-package-rename.md`), delete this plan and ledger
together, and tick `architecture-tests-rename/tier-collapse` done in
`plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_ROADMAP.md`, landed as a `Docs/*` closeout PR
through `/merge-docs` per the `plans` skill's lifecycle step 5.
