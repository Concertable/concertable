# Code review — Feature/payments_provider-contract-baseline

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `45faf5d7dc960528cc0767099d29e1b15cafe109`  _(2026-08-17)_
**Security-reviewed up to commit:** `45faf5d7dc960528cc0767099d29e1b15cafe109`  _(2026-08-17)_

> Range reviewed: `7c1253f6..45faf5d7` (6 first-parent branch commits; incoming `origin/main` changes excluded except merge resolution).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/agents/CODE_CONVENTIONS.md:310`
  Corrected the new extension-placement rule so related receiver extensions stay together without
  incorrectly forbidding an operation mapper from grouping several related wire receiver types.

## Incremental review — 2026-08-17

No issues found. Native and security review covered the requested event-name and extension-syntax
correction at `008e0a95..45faf5d7`. The stable `.v1` wire identity is unchanged, the superseded CLR
name was never part of the published compatibility baseline, and every extension container introduced
or edited by this feature uses C# 14 `extension(Receiver)` blocks. Also checked correctness,
microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.
