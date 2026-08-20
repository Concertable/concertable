# Code review — Refactor/deal-dispatch-foundation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e4fdc642d8856c776a7d47f51e1379c6eb1dcf5e`  _(2026-08-20)_
**Security-reviewed up to commit:** `beab16bd980c28c76021016ddd3101fa38b1ce91`  _(2026-08-20)_

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

## Incremental review — 2026-08-20 (post-main sync)

No issues found. Reviewed `bb8aa084..beab16bd` (28 commits) for correctness, microservice isolation,
module boundaries, seeding, C# conventions, security-sensitive changes, and test coverage of changed paths.

## Incremental review — 2026-08-20 (pre-merge checkpoint tail)

No issues found. Reviewed `beab16bd..e4fdc642` (3 commits) for correctness, microservice isolation,
module boundaries, seeding, C# conventions, and test coverage. The range changes only the plan ledger
and review artifact; it contains no runtime or security-sensitive changes.
