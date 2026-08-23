# Code review — Fix/portable-agent-hooks

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `a21199b65`  _(2026-08-23)_

> Range reviewed: `2323c77..4be19b7` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — HIGH — native** — `.agents/hooks/merge_review_gate.py:197`
  Codex omits an `exec_command` call's separate `workdir` from PreToolUse input, so a bare merge command can
  be judged against the turn checkout instead of the checkout where it runs. Make Codex fail closed unless
  the merge command explicitly establishes its target checkout, and test both rejection and explicit-target
  evaluation.

  **Resolved:** Re-vendored the merged producer contract from `Concertable/agent-standards@5c0d433`: Codex
  now requires the canonical absolute `pushd` checkout envelope, while Claude evaluates the same proven
  target and fails closed on attempted-but-unprovable `pushd` forms. The producer's 59-test merge-gate suite,
  the 13-test consumer provenance suite, and `vendor-hooks.ps1 -Check` all pass.
- [x] **NAT2 — HIGH — native** — `.codex/hooks.json:11`
  Windows launch commands use a repo-root-relative wrapper path even though Codex sessions can start in any
  repository subdirectory. Resolve the wrapper from the Git top level without reintroducing quote-sensitive
  inline Python, and execute all three commands from a nested cwd.

  **Resolved:** Every repo-local Codex event now resolves the shared `run-repo-hook.cmd` launcher through
  `git rev-parse --show-toplevel`, including `SessionStart`; the inline Python launcher is gone. The focused
  Windows wiring regression executes all four exact manifest commands through `cmd.exe` from
  `.agents/hooks/tests` and proves each intended hook script launches successfully.
- [x] **NAT3 — MEDIUM — native** — `.agents/hooks/tests/test_repo_hook_wiring.py:68`
  The consumer suite executes Codex commands only on Windows, leaving the changed POSIX commands unexecuted
  on Linux/macOS. Execute every POSIX command through Bash on POSIX alongside the nested-cwd Windows cases.

  **Resolved:** Codex manifest structure is now validated for both platform command fields on every OS,
  while native execution runs all four exact Windows commands through `cmd.exe` or all four exact POSIX
  commands through Bash from a nested checkout directory. Claude wiring validation now also accounts for
  its `SessionStart` hook, and its complete native command set continues to execute.

- [x] **NAT4 — HIGH — native incremental** — `.claude/settings.json:44`
  Claude's repo-local `SessionStart` hook still invokes `python` directly instead of the portable Bash
  launcher. Linux/macOS systems with only `python3` can therefore report a failing duplicate registration.
  Route `session_floor.py` through the same `CLAUDE_PROJECT_DIR`/`cygpath` and `run-repo-hook.sh` command as
  every other Claude hook, declare Bash explicitly, and require that launcher for every Claude command in
  the wiring regression.

  **Resolved:** Claude `SessionStart` now uses the same quoted `CLAUDE_PROJECT_DIR`/`cygpath` Bash launcher
  as all other repo-local Claude hooks and explicitly selects Bash. The wiring regression requires that
  exact launcher and shell on all four registrations, executes every native command, and the complete
  24-test hook suite plus the PowerShell 7 vendor provenance check pass.

## Incremental review closure

All finding commits through `a5380e553` were reviewed in fresh native contexts. The final increment
`540d83005..a5380e553` was clean with no actionable findings. The complete consumer hook suite passed 24
tests with one expected non-native Codex POSIX skip, and all 11 vendored files matched merged producer
commit `5c0d433`.

Current `origin/main` was merged at `a21199b65`. A fresh integration review was clean: the merge changed no
reviewed hook, manifest, launcher, or review-file blob; the post-merge 24-test suite and 11-file provenance
gate remained green.
