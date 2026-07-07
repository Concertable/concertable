# Code review — Fix/PlatformSyncHardening

**Reviewed up to commit:** `93cec02ed8ccc374407d94ceabd318b6904a18e1`  _(2026-07-07)_

> Range reviewed: `ccbf03b0..93cec02e` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

Diff scope: `.github/workflows/platform-sync.yml` (the 1e hardening) + `plans/PLATFORM_PACKAGE_SYNC.md`
(doc-only). No C#/service code, so the microservice-isolation, module-boundary, seeding, and C#-convention
lenses (B–E) don't apply — this is a Lens A (correctness) review of the workflow shell/YAML.

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.

Traced every path in the hardened workflow and all are sound:

- **Cascade guard** (`platform-sync.yml:58-65`) — greps the triggering commit for `chore/platform-sync-`
  or `chore(platform): sync`. Correct against this repo's observed merge-commit format (master log shows
  `Merge pull request #N from Concertable/<branch>`), so a sync-PR merge is reliably caught by the
  branch-name pattern; the subject pattern covers a squash. `workflow_dispatch` correctly bypasses.
- **Feed version resolve** (`platform-sync.yml:76-84`) — `curl -f` fails closed on 404; empty/garbage
  `.versions[]` is rejected by the `case` sanity check; `sort -V | tail -1` is a pure numeric compare on
  the shared `0.1.0-alpha.0.<height>` suffix, so it can't mis-rank. Feed-sourced, never recomputed →
  no phantom versions (the `.553`/`.554` NU1102 class of bug is gone).
- **Heredoc** (`platform-sync.yml:112-121`) — after YAML `run: |` strips the common 10-space indent,
  `EOF` lands at column 0, a valid terminator.
- **`set -e`/`pipefail`** — GA's default bash runs `-eo pipefail`; `! gh pr view` inside `if` correctly
  does not trip `-e`, so the create/update path works.
- **Auto-merge removal** — complete; no residual `gh pr merge --auto`.

### Note (not a finding)

The cascade guard's correctness rests on the merge-commit message containing either the sync branch name
or the `chore(platform): sync` subject. That holds for the repo's current merge-queue format. If the
merge-commit template is ever changed to drop both, the guard would silently stop firing and the
publish→sync cascade could reopen. Below the confidence bar for a finding (currently correct, and the
inline comment documents the mechanism) — logged here only so a future merge-format change knows to
re-check it.
