# Code review — Feature/launch_rate-limiting

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `9d2921e8f823513f77f919c23586eeb0c49eb353`  _(2026-08-19)_

> Range reviewed (latest): `29e7a1ad1..9d2921e8f` — the opt-in seam refactor (#655 producer). See the
> 2026-08-19 incremental section; the 2026-08-17 pass below reviewed the now-superseded seam v1.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Layer 1 (native `code-reviewer`): correctness / reuse / simplification / efficiency / error handling — no findings. It verified the partitioning (`sub` with `ClaimTypes.NameIdentifier` fallback), the `OnRejected` 429/`Retry-After`/ProblemDetails path, fixed-window config binding, and the `AddRateLimiter`/`GlobalLimiter`/`AddPolicy`/`UseRateLimiter` wiring against .NET 10 rate-limiting semantics.

Layer 2 (Concertable lenses):
- **Microservice isolation / module boundaries / seeding** — N/A; `Concertable.ServiceDefaults` is shared host infrastructure with no cross-service or cross-module references.
- **C# conventions** — file-scoped namespaces, sealed types, no primary constructors on stateful types, single-statement branches without braces, no inline logging. Clean.
- **Test coverage** — the focused test exercises both the allowed path and the over-limit 429 + `Retry-After` path, and proves config binding (limit supplied via in-memory config).

Security layer: not run — none of the changed paths match the merge gate's `_SECURITY_PATTERNS` (no Auth/Payment/`*.Contracts`/`*Controller*.cs`/workflow/credential paths), so no `Security-reviewed up to commit:` marker is required.

## Incremental review — 2026-08-17 (correction)

The initial pass **missed a real finding**: the test was misclassified. It boots a host over HTTP
(`TestHost`), which the conventions define as an integration test, but it was authored as an
ambiguously-named `*.Tests` unit-style project (`Assert.*`, unit naming, no `AGENTS.md`). CI also
discovers tests only by `*.UnitTests.csproj`/`*.IntegrationTests.csproj`, so the `*.Tests` project would
never have run. Root cause of the miss: neither authoring nor this review loaded the test-convention docs.

- [x] **CV1 — MED — conventions** — `api/Concertable.ServiceDefaults/tests/…` — test misclassified
  (ambiguous `*.Tests` project booting a host over HTTP). **Fixed:** rewritten as a genuine unit test
  `Concertable.ServiceDefaults.UnitTests` — no host/HTTP; it resolves the limiter `AddDefaultRateLimiting`
  configures (`IOptions<RateLimiterOptions>.GlobalLimiter`) and asserts `AcquireAsync` rejects past the
  limit with `RetryAfter` (`Assert.*`). Sibling `AGENTS.md`/`CLAUDE.md`. A host+HTTP `*.IntegrationTests`
  is operationally impossible for a shared package: the CI harness (`local-platform.ps1`
  `Assert-DataAccessAssembly`) requires every `*.IntegrationTests` to contain
  `Concertable.DataAccess.Infrastructure`, which ServiceDefaults has no reason to reference — that
  assertion is what first failed CI on the integration-named variant. The HTTP 429 mapping is covered by
  Phase 2's B2B endpoint test.

### Noted, not a finding (out-of-diff, tracked in the plan)
- The IP-keyed policies (`Login`, `Upload`) partition on `Connection.RemoteIpAddress`, which collapses to the proxy IP unless the host runs `UseForwardedHeaders` before `UseDefaultRateLimiting`. This is a Phase-2 consumer/host-pipeline concern (the seam correctly reads the connection IP), and `Login` runs on `Concertable.Auth`, which already configures ForwardedHeaders. Pinned as an explicit Phase-2 requirement in `plans/launch/RATE_LIMITING_PLAN.md`.

## Incremental review — 2026-08-19 (opt-in seam refactor, #655 producer)

Reviews `29e7a1ad1..9d2921e8f` at `medium` — the refactor of `Concertable.ServiceDefaults` from a
global-fallback + central-policy model to opt-in named policies (`AddDefaultRateLimiting` plumbing +
`AddRateLimitPolicy` + `RateLimitWindow` + lazy `IOptionsMonitor` binding; removed the global limiter,
`RateLimitPolicies`, `RateLimitingOptions`). This **supersedes** the 2026-08-17 seam-v1 pass above; that
design no longer exists.

**No open findings.** Both layers clean; the one gap found was fixed in-pass.

- Layer 1 (native `code-reviewer`, medium): **no findings**. Verified `Configure<RateLimiterOptions>`
  policy registration composes additively and order-independently with `AddRateLimiter`; the per-request
  `IOptionsMonitor<RateLimitWindow>.Get(name)` is a cached singleton lookup (not a re-bind or per-request
  allocation) and is what lets `BindConfiguration` reloads take effect; `.Configure(defaults)` then
  `.BindConfiguration(...)` orders config-over-defaults correctly with defaults intact when the section is
  absent; the `OnRejected` 429/`Retry-After`/ProblemDetails path matches the official ASP.NET Core pattern
  (status set before invocation, `IProblemDetailsService` null-guarded); `ResolvePartitionKey` fallbacks
  are sound.
- Layer 2 (Concertable lenses):
  - **Microservice isolation / module boundaries / seeding** — N/A; shared host infrastructure, no
    cross-service or cross-module references, no persistence.
  - **C# conventions** — file-scoped namespaces, sealed types, no primary constructors on stateful types,
    single-statement branches without braces, no inline logging. Clean.
  - **Test coverage (Lens F)** — [x] **CV1 — LOW — test-coverage** — `…/RateLimitingTests.cs` — the
    refactor's headline change is *lazy* config binding (the eager-bind fix), but nothing asserted it.
    **Fixed** (commit `9d2921e8f`): added `AddRateLimitPolicy_BindsNamedWindowFromConfigLayeredAfterRegistration_OverDefaults`
    — config added *after* `AddRateLimitPolicy` still wins, absent keys keep the passed defaults. 5 tests pass.

Security layer: not run — no changed path matches the security-sensitive set (no Auth/Payment/`*.Contracts`/
`*Controller*.cs`/workflow/credential paths); ServiceDefaults infra only.
