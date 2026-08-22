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

Currency resolved: merged `origin/main` in (`9e44faeb9`), no conflicts, pushed. Local full-solution build hit
a persistent `MSB3021 Access denied` copying three unrelated pre-existing Customer-module DLLs to `bin/`
(also seen earlier on `scripts/test.ps1`) — a local sandbox/environment file-lock unrelated to this branch's
diff (confirmed via targeted builds of the changed `.ArchitectureTests` projects and via exact-head PR CI,
which built clean). PR #746 is current with `origin/main`, all 76 PR-level checks are green (73 pass, 3
`skipping` merge_group-gated E2E suites, as expected), and the `skip-e2e` label is applied (no positive
end-to-end trigger — pure test-project rename + CI/doc config). A security review is required and in
progress because this branch's own diff touches `.github/workflows/test.yml` (the merge gate's
security-sensitive path); not yet stamped.

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

`reviews/Refactor-architecture-tests-rename.md`, reviewed up to `9e44faeb9` (re-stamped after the main
merge; the only branch-owned content past the original watermark was the two fixes below plus docs-only
ledger commits — no new findings). Native layer (Layer 1): no findings. Repo lenses: two findings, both
fixed and committed (`349920c18`) — `docs/INDEX.md` still named the pre-rename `composition-tests` CI
matrix; the `.ArchitectureTests` skill route was missing `module-structure` for the static ArchUnit half
B2B's own AGENTS.md says that skill governs. Security layer: required (branch touches
`.github/workflows/test.yml`) and in progress — a security-review subagent is scanning
`549af7cc0..439be9780`; not yet stamped.

## Next Steps

A `/security-review` scan of the branch is running in the background (required before `gh pr merge` can pass
the merge gate, since this branch's diff touches `.github/workflows/test.yml`). Once it returns: fold any
real HIGH/MEDIUM findings into the review file and fix them, or record "no findings" and stamp
`**Security-reviewed up to commit:** \`439be978019acd101c743f540f391463db29e18d\`` in
`reviews/Refactor-architecture-tests-rename.md`, commit, push, then run `gh pr merge 746 --merge --auto` and
poll to `MERGED` per the `merge` skill's step 4b. After merge: return to a clean `main`, remove this
worktree's need (branch developed in the main checkout — no worktree to close), and follow the
publish/platform-sync consequence (this PR touches `api/**` test projects and `.slnx` files, so a sync PR
will likely open) to green per the `merge` skill's step 6. Phase 2 (renaming the published
`Concertable.Composition.Testing` package to `Concertable.Testing.Architecture` via publish-then-bump) is a
separate chain that starts only after this PR lands.
