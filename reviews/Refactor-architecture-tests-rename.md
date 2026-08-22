# Code review — Refactor/architecture-tests-rename

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `9e44faeb9006ad67ec784a2c342a32e567383567`  _(2026-08-22)_
**Security-reviewed up to commit:** `cb79c6b590cdf6139cd9f23318bb046573987e63`  _(2026-08-22)_

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

## Security review

Range `549af7cc0..439be9780`, triggered because the branch's own diff touches `.github/workflows/test.yml`
(the merge gate's security-sensitive path). No HIGH/MEDIUM findings. The workflow change is a straight
identifier rename (`composition_projects`→`architecture_projects`, job `composition-tests`→
`architecture-tests`, glob `*.CompositionTests.csproj`→`*.ArchitectureTests.csproj`) — no new interpolation
of untrusted context into a `run:` block, `permissions:` and secrets usage unchanged, `matrix.project`
values still come from a `find`/`jq` scan of filenames already committed to the repo (same trust boundary as
before). No production code, auth logic, or endpoints touched.

## Native review (Layer 1)

No findings — the subagent reported the mechanical rename/collapse as clean: no dropped references, no CI
inconsistencies, correct tier-gate and script updates.

## Incremental review — 2026-08-22

Range `bda4ae413..9e44faeb9`. Commits since the watermark: the CV1/CV2 fixes above, two docs-only ledger
updates, and a merge of `origin/main` to clear a 9-commit currency gap. Diffed against the true branch
boundary (`merge-base(origin/main, HEAD)..HEAD`, per `FULL.md` Step 1's branch-range definition) rather than
the stale marker, the only content this branch itself contributes beyond the original review is exactly the
CV1/CV2 fixes (`docs/INDEX.md`, `.agents/skill-routes.json`) — already reviewed above — plus the ledger
docs. No security-sensitive path is touched relative to `origin/main` (`CredentialRegisteredHandler.cs` /
`UserEntity.cs` etc. arrived from main itself, not from this branch), so no security layer applies. No new
findings.
