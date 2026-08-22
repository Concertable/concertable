# Architecture-tests rename — progress

- Plan: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_PLAN.md`
- Roadmap: `plans/architecture-tests-rename/ARCHITECTURE_TESTS_RENAME_ROADMAP.md`
- Roadmap item: `architecture-tests-rename/tier-collapse`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Refactor/architecture-tests-rename`
- PR: none yet
- Package gate: Phase 2 renames the published `Concertable.Composition.Testing` package (publish-then-bump).

## Current state

Plan written. Investigation complete: verified both test kinds are architecture fitness functions (same
category); mapped the full blast radius (6 test projects, the shared published lib, tier gate, skill-routes,
CI matrix, `test.ps1`, docs); confirmed the B2B name collision (merge required) and the missing CI
architecture leg. No code changed yet.

## Completed milestones

- Categorization verdict + full reference inventory gathered (subagents).
- Plan/roadmap/ledger created on `Refactor/architecture-tests-rename`.

## Latest verification

None yet — no code change.

## Reviews

None yet.

## Next Steps

Execute Phase 1 on this branch (all steps are reversible working-tree edits):

1. `git mv` the four clean project folders (Auth/Payment/Search/Customer `*.CompositionTests` →
   `*.ArchitectureTests`) + the AppHost one; rename each `.csproj`, update `namespace`, test-class name,
   and `AGENTS.md` title.
2. Fold B2B: move `B2BCompositionTests.cs` into `Concertable.B2B.ArchitectureTests` (namespace →
   `Concertable.B2B.ArchitectureTests`), add the missing project refs, delete the old B2B composition
   project + folder + its slnx entry.
3. Collapse the tier: `api/TestConventions.targets` (drop `.CompositionTests` line + error-message token),
   `.agents/skill-routes.json` (regex → `\.ArchitectureTests`), `scripts/test.ps1` (Architecture suite),
   `.github/workflows/test.yml` (architecture leg + `needs:` graph), `api/Concertable.slnx` paths.
4. Docs: affected `AGENTS.md`, `docs/INDEX.md`, `api/TECH_DEBT.md`, `reviews/*` prose.
5. Gate: `dotnet build api/Concertable.slnx` to 0 errors; `./scripts/test.ps1 architecture` green; the
   Phase-1 grep gate. Commit + push, open a draft PR, then run `/review`.

Phase 2 (published-package rename) starts only after Phase 1's PR lands.
