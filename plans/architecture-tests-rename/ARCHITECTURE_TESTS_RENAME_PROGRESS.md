# Architecture-tests rename — progress

- Plan: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_PLAN.md`
- Roadmap: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_ROADMAP.md`
- Roadmap item: `architecture-tests-rename/tier-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Refactor/architecture-tests-rename`
- PR: [#746](https://github.com/Concertable/concertable/pull/746) — **MERGED** (`685f66ec9`)
- Platform-sync: [#749](https://github.com/Concertable/concertable/pull/749) — **MERGED** (`6a5db574c`,
  version `0.1.0-alpha.0.1149`)
- Package gate: Phase 2 renames the published `Concertable.Composition.Testing` package (publish-then-bump).

## Current state — TERMINAL

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

`reviews/Refactor-architecture-tests-rename.md`, reviewed up to `45d746d7d` (re-stamped three times as
`main`'s fast pace forced repeated merges; each pass confirmed the true branch-owned diff was unchanged, so
no re-review was needed beyond the first). Native layer (Layer 1): no findings. Repo lenses: two findings,
both fixed and committed (`349920c18`) — `docs/INDEX.md` named the pre-rename `composition-tests` CI matrix;
the `.ArchitectureTests` skill route was missing `module-structure` for the static ArchUnit half B2B's own
AGENTS.md says that skill governs. Security layer: stamped at `45d746d7d` — no findings (pure identifier
rename in `.github/workflows/test.yml`, no new untrusted-context interpolation).

## Next Steps

Phase 1 is done; the plan and roadmap item stay open — per the roadmap, Phase 2 (renaming the published
`Concertable.Composition.Testing` package to `Concertable.Testing.Architecture`) is folded into this same
workstream, not a separate item. Start Phase 2 per `ARCHITECTURE_TESTS_RENAME_PLAN.md`'s Phase 2 section: a
producer PR (rename the lib, migrate ProjectReference consumers — AppHost and B2B — in-PR, merge, let
`publish-packages` publish the new id), then a platform-sync PR migrating the four `PackageReference`
services' `PackageReference`/`PackageVersion`/`using` to the new id. Decide there whether to also rename the
DI-validation types (`CompositionValidationOptions`, `ValidateComposition`, `CompositionTestArguments`) —
plan's default is keep. Only once Phase 2's consumer migration lands and the grep gate
(`grep -rniE "composition\.testing|compositiontests"` empty but the two allowlisted unit-test classes) is
clean does this plan close: delete the plan and ledger together (`git rm`) and tick the roadmap item, per
the `plans` skill's lifecycle step 5.
