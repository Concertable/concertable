# Code review — Fix/IntegrationMidnightClock

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `b1f90a793c3747d96945a770e60bb6162cd96661`  _(2026-08-09)_

> Range reviewed: `d57e0c2a6..b1f90a793` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and changed-behaviour coverage. The change is test-only: generated opportunity dates now share the fixture's captured seed clock, and every call site supplies that clock. The targeted Concert integration run passed 144 B2B tests and 11 Customer tests, including all 11 contract cases that failed when CI crossed UTC midnight.
