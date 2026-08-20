# Code review — Refactor/deal-dispatch-foundation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `bb8aa0840c817fd59273aa4790ee795d82e9d501`  _(2026-08-20)_
**Security-reviewed up to commit:** `2e34ce37840b61432fc1befdcee460c586faf795`  _(2026-08-20)_

> Range reviewed: `133b018d..2e34ce37` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Infrastructure/Services/Strategies/DealStrategyFactory.cs:7`
  Replace the primary-constructor capture with an explicit `private readonly IKeyedServiceProvider services` field and constructor assignment; the `csharp-style` standard requires explicit readonly fields for dependencies read by methods.
- [x] **CV2 — MEDIUM — keyed strategies** — `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Infrastructure/Extensions/ServiceCollectionExtensions.cs:47`
  Restore validated vertical strategy registration with `RequireAll<IDealMapper>()` and `RequireAll<IDealUpdater>()`; plain `AddKeyedSingleton` calls defer an omitted `DealType`/family pair to runtime, while the `keyed-strategies` standard requires incomplete coverage to fail during composition.

## Incremental review — 2026-08-20

No issues found. Reviewed `2e34ce37..bb8aa084` for correctness, microservice isolation, module boundaries,
seeding, C# conventions, security-sensitive changes, and test coverage of changed paths.
