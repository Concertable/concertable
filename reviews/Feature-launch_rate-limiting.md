# Code review — Feature/launch_rate-limiting

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `6050bd927b85a2d9f51d3dd4faf073100e07b4e5`  _(2026-08-17)_

> Range reviewed: `bfbfd863c..6050bd927` (4 commits — Phase 1 seam + test + plan docs).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Layer 1 (native `code-reviewer`): correctness / reuse / simplification / efficiency / error handling — no findings. It verified the partitioning (`sub` with `ClaimTypes.NameIdentifier` fallback), the `OnRejected` 429/`Retry-After`/ProblemDetails path, fixed-window config binding, and the `AddRateLimiter`/`GlobalLimiter`/`AddPolicy`/`UseRateLimiter` wiring against .NET 10 rate-limiting semantics.

Layer 2 (Concertable lenses):
- **Microservice isolation / module boundaries / seeding** — N/A; `Concertable.ServiceDefaults` is shared host infrastructure with no cross-service or cross-module references.
- **C# conventions** — file-scoped namespaces, sealed types, no primary constructors on stateful types, single-statement branches without braces, no inline logging. Clean.
- **Test coverage** — the focused test exercises both the allowed path and the over-limit 429 + `Retry-After` path, and proves config binding (limit supplied via in-memory config).

Security layer: not run — none of the changed paths match the merge gate's `_SECURITY_PATTERNS` (no Auth/Payment/`*.Contracts`/`*Controller*.cs`/workflow/credential paths), so no `Security-reviewed up to commit:` marker is required.

### Noted, not a finding (out-of-diff, tracked in the plan)
- The IP-keyed policies (`Login`, `Upload`) partition on `Connection.RemoteIpAddress`, which collapses to the proxy IP unless the host runs `UseForwardedHeaders` before `UseDefaultRateLimiting`. This is a Phase-2 consumer/host-pipeline concern (the seam correctly reads the connection IP), and `Login` runs on `Concertable.Auth`, which already configures ForwardedHeaders. Pinned as an explicit Phase-2 requirement in `plans/launch/RATE_LIMITING_PLAN.md`.
