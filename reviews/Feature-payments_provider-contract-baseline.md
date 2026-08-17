# Code review — Feature/payments_provider-contract-baseline

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `cb2d41c30c32304b951ac63d4b9823fe4bcdfcd2`  _(2026-08-17)_
**Security-reviewed up to commit:** `cb2d41c30c32304b951ac63d4b9823fe4bcdfcd2`  _(2026-08-17)_

> Range reviewed: `45faf5d7..cb2d41c3` (5 first-parent branch commits; incoming `origin/main` changes excluded except merge resolution).
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

## Incremental review — 2026-08-17 (deterministic error-kind mapper)

No issues found. Native and security review verified the protobuf-to-Reunion `ErrorKind` translation is
defined once in a `FrozenDictionary`, exposed through the receiver-owned `ToErrorKind()` extension,
and still rejects unspecified and forward-unknown protobuf values. Also checked correctness,
microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.
