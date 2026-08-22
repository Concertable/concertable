# Architecture-tests rename — progress

- Plan: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_PLAN.md`
- Roadmap: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_ROADMAP.md`
- Roadmap item: `architecture-tests-rename/tier-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Refactor/architecture-tests-rename`
- PR: [#746](https://github.com/Concertable/concertable/pull/746)
- Package gate: Phase 2 renames the published `Concertable.Composition.Testing` package (publish-then-bump).

## Current state

Phase 1 implemented, committed (`868a9868a`), reviewed, and pushed; PR #746 open. The Composition tier is
gone: six composition-test projects are now `*.ArchitectureTests`, with B2B's dynamic host-graph tests
folded into its existing `Concertable.B2B.ArchitectureTests` (as `B2BHostGraphTests`) alongside the static
ArchUnit rules. Tier gate, CI leg, `test.ps1` suite, skill route, and all `.slnx` files updated. The shared
published lib `Concertable.Composition.Testing` is intentionally untouched (Phase 2), so the change is
non-breaking.

The branch is 9 commits behind `origin/main` — currency must be resolved (merge main in, rebuild to 0
errors, push) before enqueueing/merging; not a blocker for the open PR itself.

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

`reviews/Refactor-architecture-tests-rename.md`, reviewed up to `bda4ae413`. Native layer (Layer 1): no
findings. Repo lenses: two findings, both fixed and committed (`349920c18`) — `docs/INDEX.md` still named
the pre-rename `composition-tests` CI matrix; the `.ArchitectureTests` skill route was missing
`module-structure` for the static ArchUnit half B2B's own AGENTS.md says that skill governs.

## Next Steps

Bring the branch current with `origin/main` (9 commits behind), rebuild `api/Concertable.slnx` to 0 errors,
push, then run `/merge` on [PR #746](https://github.com/Concertable/concertable/pull/746). Phase 2 (renaming
the published `Concertable.Composition.Testing` package to `Concertable.Testing.Architecture` via
publish-then-bump) is a separate chain that starts only after this PR lands.
