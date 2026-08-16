# Code review — Fix/merge-gate-command-matching

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `4faca7e0a95984b6e4e14b2182052dd334e2bc0e`  _(2026-08-16)_

> Range reviewed: `origin/main..HEAD`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

All three defects below were found by using the gate, not by reading it — each one blocked real work
during the session that produced them.

- [x] **BUG1 — MEDIUM — correctness** — `.claude/hooks/merge-review-gate.py` (`is_merge_enable`)
  Enabling tokens were tested with `"--auto" in command`, and `--auto` is a **substring of
  `--disable-auto`**. So the branch the code's own comment describes as "the safe direction — allow" was
  unreachable, and every disable was gated too — including the first half of the documented re-assert
  remedy. **Fixed** by stripping `--disable-auto` before the token scan; the compound re-assert still
  gates correctly because its second half is a genuine enable.

- [x] **BUG2 — MEDIUM — correctness** — `.claude/hooks/merge-review-gate.py` (invocation detection)
  The gate fired on the merge string appearing **anywhere** in a command, so it blocked commands that
  merely *quoted* it: editing this hook file, and creating the PR whose body described the fix. **Fixed**
  with an invocation regex — the command at a command position (start, or after `;`/`&&`/`||`/`|`/
  newline, optionally env-prefixed) — so quoting it in an `echo` or heredoc no longer counts.

- [x] **BUG3 — MEDIUM — correctness (adopted from #495)** — git ran in the hook process's directory
  (the pinned project dir), not where the merge runs. A `cd <worktree> && merge` with no PR number was
  therefore judged against the wrong branch's review. **Fixed** by adopting #495's `merge_target_dir`:
  the last `cd` before the invocation wins, else the tool cwd. Its test cases came across too.

### Checked and clean

- **Still fails closed:** unresolvable PR, bare merge on an unreviewed checkout, missing review, open
  findings — all blocked. The security-marker layer is untouched.
- **The re-assert compound is still gated**, verified explicitly, because that is the one case where a
  naive "contains --disable-auto → allow" would fail open.
- **#495 is superseded, not ignored.** Its `merge_target_dir` and worktree test cases are adopted here;
  the PR itself is 890 commits behind and edits the same file, so rebasing it would cost more than
  folding it in.
