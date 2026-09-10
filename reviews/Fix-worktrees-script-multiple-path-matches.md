# Code review — Fix/worktrees-script-multiple-path-matches

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `66458562235c0a89b23664da748bc8ee74cadea4`  `(2026-09-10)`
**Judgment:** `approved`

## Review pass — 2026-09-10 — code

**Candidate base:** `aa64924852582e62c2601ed853903562fc0886e8`
**Candidate head:** `66458562235c0a89b23664da748bc8ee74cadea4`
**Candidate branch:** `Fix/worktrees-script-multiple-path-matches`
**Candidate scope:** `all`
**Candidate path-set:** `scripts/worktrees.ps1` `(1 path)`
**Work-order path:** `reviews/Fix-worktrees-script-multiple-path-matches.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

The branch was 2304 commits behind base when picked up — opened 19 August, its CI green against a
19 August main. `origin/main` was merged in before review; the merge was clean and the net diff against
current base is still the single line below. Lenses run as the strong parent; subordinate dispatch is
disabled by session policy.

```
-        $executable = (Get-Command $Program -CommandType Application -ErrorAction Stop).Source
+        $executable = (Get-Command $Program -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
```

### Findings

None.

### Verified, no finding

- **Correctness.** `Get-Command` returns one `ApplicationInfo` per PATH match. On a duplicated PATH the
  array's `.Source` member-enumerates to a `string[]`, and `& $executable` then fails instead of
  invoking anything, which is why `close` could not run. `Select-Object -First 1` takes the match PATH
  itself would resolve, because `Get-Command` returns them in PATH order.
- **Completeness.** The script has exactly one `Get-Command` and one `.Source`, both on this line, so
  the fix leaves no second instance of the defect.
- **Interaction with the surrounding code.** `-ErrorAction Stop` still throws before the pipeline when
  nothing resolves, so the not-found path is unchanged. `Set-StrictMode -Version Latest` is in force and
  is satisfied — the property access is on a single object after the filter. `Select-Object -First 1`
  stopping the upstream cmdlet early is harmless for a completed enumeration.
- **Behaviour.** `scripts/worktrees.ps1 audit` runs to completion on the merged head and returns the full
  worktree table, which the defect prevented.
- **Comment policy.** The fix carries no comment, which is correct here. PR #656 is the same one-line
  change plus a three-line comment narrating the choice; it is a duplicate and should be closed rather
  than landed.
