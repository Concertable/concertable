# Code review — Refactor/architecture-tests-rename

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `bda4ae413e7f9eeefe459350007e37d5796e4a8e`  _(2026-08-22)_

> Range reviewed: `549af7cc0..bda4ae413` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — MEDIUM — convention (docs-and-debt)** — `docs/INDEX.md:156`
  The machine-check row for host-composition coverage still names `` the `composition-tests` CI matrix `` —
  that CI job was renamed to `architecture-tests` in this branch (`.github/workflows/test.yml`). Update the
  enforcer cell to name the `architecture-tests` CI matrix, per the `docs-and-debt` standard's rule that a
  doc must stay accurate against the real code/commands it cites.

- [x] **CV2 — MEDIUM — convention (skill-routes / module-structure)** — `.agents/skill-routes.json:92-97`
  This same commit's own `Concertable.B2B.ArchitectureTests/AGENTS.md` states "The static rules being
  asserted are the `module-structure` skill" for the ArchUnitNET files now living in this folder, but the
  route this diff repointed to `\.ArchitectureTests[^/]*/.*\.cs$` lists only `composition-testing` in its
  `skills` array — it no longer carries `dotnet-standards:module-structure` / `dotnet:module-structure` for
  the static half, so the write-time hook won't load that skill before an author edits B2B's static ArchUnit
  rules. Add `dotnet-standards:module-structure` and `dotnet:module-structure` to that route's skills list
  (matching the pairing already used at line 32-37 for `\.Application/`).

## Native review (Layer 1)

No findings — the subagent reported the mechanical rename/collapse as clean: no dropped references, no CI
inconsistencies, correct tier-gate and script updates.
