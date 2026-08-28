# Code review — Chore/TechDebt-run-20260828-9898

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `ea537c1a0757d2b70265f88e6f52b487d53aabcf`  _(2026-08-28)_

> Range reviewed: `caa13a0a05aa3d101b884f93eca05aaa5d7ad37a..ea537c1a0757d2b70265f88e6f52b487d53aabcf` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. `AsbTopology` gains `WithService(serviceName)` plus scoped, parameterless
`Subscribe<TEvent>()`/`Queue<TCommand>()` overloads, both funneling through shared
`SubscribeCore`/`QueueCore` helpers alongside the retained (unchanged) string-taking overloads — no
duplication, no behavior change to existing call sites. `RequireServiceName()` throws before any
topic/queue side effect runs, so the guard is exception-safe. The five service topologies
(`AddAuthTopology`, `AddB2BTopology`, `AddCustomerTopology`, `AddPaymentTopology`, `AddSearchTopology`)
are deliberately left on the old overloads, since they consume `Concertable.AppHost.Shared` as a
version-pinned published package and migrating them now would build against an unpublished API —
tracked as the remaining step in `api/Concertable.AppHost.Shared/TECH_DEBT.md`. Three new unit tests
cover naming, mid-chain isolation between two `WithService` scopes, and the guard-throws case; all 10
tests in `Concertable.AppHost.Shared.UnitTests` pass, and `Concertable.AppHost.Shared` builds with zero
errors. No security-sensitive path is touched. Independent native review (code-reviewer) confirms no
correctness, duplication, or efficiency issues.
