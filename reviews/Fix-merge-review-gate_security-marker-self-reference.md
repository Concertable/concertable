# Code review — Fix/merge-review-gate_security-marker-self-reference

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c556cc26f7807f48f6b0c48d96ad31501a190096`  _(2026-08-17)_

> Range reviewed: `a8cb736a5..c556cc26f` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

Single 2-line change to `.claude/hooks/merge-review-gate.py`: applies the existing `review_only()`
tolerance (already used by the primary `Reviewed up to commit:` check three lines above the touched
code) to the `Security-reviewed up to commit:` check as well. Verified:

- `review_only()` was already defined and already imported/in-scope at the edit site — no new
  dependency, no new function.
- The change is syntactically identical in shape to the primary-marker check immediately above it in
  the same function (`if not (head.lower().startswith(sreviewed) or sreviewed.startswith(head.lower()))
  and not review_only(sreviewed, head):`), so it can't silently diverge in behavior from the pattern
  it's mirroring.
- `python3 -c "import ast; ast.parse(...)"` confirms the file still parses.
- Correctness of `review_only()` itself is unchanged (not touched) — it already governs the primary
  marker in production today, so this reuses proven logic rather than introducing new logic.
- No test suite covers this hook (it's a Claude Code `PreToolUse` hook, not part of `dotnet test`);
  behavior was reasoned through by hand against the function's existing docstring, which is unchanged
  and still accurate for the new call site (a commit can never contain its own hash, which is exactly
  why the primary check needed this tolerance, and why the security check needed it identically).

No issues found. This is a minimal, mechanical fix mirroring an already-reviewed, already-production
pattern in the same file.
