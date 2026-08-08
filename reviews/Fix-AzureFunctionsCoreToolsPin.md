# Code review — Fix/AzureFunctionsCoreToolsPin

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d6fb5b20c17ce8288bc61896ceb4169a26e3ab5d`  _(2026-08-08)_

> Range reviewed: `9e14721f6..d6fb5b20c` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. CI-workflow-only change (pins `azure-functions-core-tools` to `4.12.1` in three
`test.yml` spots, plus a `.github/TECH_DEBT.md` entry) — the C#/module-boundary/microservice/seeding
lenses don't apply. Correctness lens: verified locally that `4.13.0`/`4.13.1`/`4.13.2` all 404 from
Microsoft's CDN and `4.12.1` installs and runs (`func --version` succeeds) before making the change.
