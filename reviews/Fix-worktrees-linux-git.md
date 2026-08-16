# Code review — Fix/worktrees-linux-git

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `PENDING`  _(2026-08-16)_

> Range reviewed: `7db0c9be9..HEAD`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — correctness (data loss)** — `scripts/worktrees.ps1:201`
  The "run this from a different checkout" guard tested `$current.StartsWith($target + '\')` with a
  hard-coded backslash, so on Linux it never fired for a **subdirectory** of the target — only the exact
  root matched. `close` would then proceed to delete the directory the caller's shell was sitting in.
  **Fixed** with `[IO.Path]::DirectorySeparatorChar`. Verified: running `close` from
  `<worktree>/api` now throws *"Run this command from a different checkout."* instead of proceeding.

- [x] **NAT2 — MEDIUM — error handling** — `scripts/worktrees.ps1:266`
  The `\\?\` long-path prefix was applied unconditionally. On Linux it is read as part of the filename,
  so `Remove-Item -LiteralPath` throws `Cannot find path` under the script's `$ErrorActionPreference =
  'Stop'` rather than reaching the intended `Folder remains: <target>` message — the "fails later with a
  more confusing error" case. **Fixed:** the prefix is applied only when the separator is `\`, using
  `DirectorySeparatorChar` rather than `$IsWindows` (undefined under `Set-StrictMode -Version Latest`
  on Windows PowerShell 5.1).

### Checked and clean

- **The core change is safe on Windows.** `-CommandType Application` filters to external executables, so
  an alias/function/cmdlet named `git` cannot shadow the resolution; Windows resolves `git` through
  `PATHEXT` to the same `git.exe`, and without `-All` `.Source` stays a scalar.
- **No other Windows-only assumption remains:** `Canonical`/`SamePath` trim both separators, `-split
  "\r?\n"` tolerates LF, the junction cleanup works because .NET sets `ReparsePoint` for Unix symlinks,
  the `.worktrees` sibling root is separator-agnostic, and there are no `cmd`/`robocopy`/drive-letter
  assumptions or further `.exe` literals.
- **Verified on Linux:** `./scripts/worktrees.ps1 audit` returns the worktree inventory instead of
  throwing, and the self-deletion guard fires as shown above.
