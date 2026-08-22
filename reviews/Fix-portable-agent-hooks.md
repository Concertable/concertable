# Code review — Fix/portable-agent-hooks

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `4be19b792cff06c64c9f6cd05c9110f0b6820fa0`  _(2026-08-21)_

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
- [ ] **NAT2 — HIGH — native** — `.codex/hooks.json:11`
  Windows launch commands use a repo-root-relative wrapper path even though Codex sessions can start in any
  repository subdirectory. Resolve the wrapper from the Git top level without reintroducing quote-sensitive
  inline Python, and execute all three commands from a nested cwd.
- [ ] **NAT3 — MEDIUM — native** — `.agents/hooks/tests/test_repo_hook_wiring.py:68`
  The consumer suite executes Codex commands only on Windows, leaving the changed POSIX commands unexecuted
  on Linux/macOS. Execute every POSIX command through Bash on POSIX alongside the nested-cwd Windows cases.
