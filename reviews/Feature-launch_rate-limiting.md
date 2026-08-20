# Code review — Feature/launch_rate-limiting

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `a67537bad03e90ec3e04783b94eeb61ccba607da`  _(2026-08-19)_
**Security-reviewed up to commit:** `a67537bad03e90ec3e04783b94eeb61ccba607da`  _(2026-08-19)_

> Range reviewed (latest): `6229e87c6..26b9a4354` — re-stamp after merging `origin/main` (the `1073`
> platform-sync pin bump) into #655 to make it current for merge; #655's ServiceDefaults code is
> byte-identical to the clean `9d2921e8f` review (diff `9d2921e8f..HEAD` touches zero ServiceDefaults
> files). See the 2026-08-19 incremental section; the 2026-08-17 pass below reviewed the superseded seam v1.
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

## Incremental review — 2026-08-19 (Phase 2 consumers, `7f782a237..a67537bad`)

Range: the two Phase-2 commits opting all five web hosts into the seam + named policies + trip tests
(`fed21dd91` apply, later fixes). Effort: medium. Two layers run: native (`code-reviewer`) + security
(diff touches Auth/Payment/`*.Contracts`/`*Controller*.cs`, so the security layer ran and its marker is
stamped above).

### Findings

- [x] **NAT1 — MEDIUM — correctness** — `api/Concertable.Auth/src/Concertable.Auth/Program.cs` — Auth
  configured `ForwardedHeadersOptions` only under `if (IsDevelopment())`, so in production
  `UseForwardedHeaders()` ran with `ForwardedHeaders.None` and the per-IP `credential` throttle would
  bucket every client behind the proxy into one 10/min partition (global lockout, not per-attacker).
  **Fixed** (`a67537bad`): configure `XForwardedFor|XForwardedProto` unconditionally like the other four
  hosts; the dev-only block now only adds `XForwardedHost` + clears the known-proxy trust. Trusted-proxy
  `KnownProxies` binding remains deferred to `launch/config-and-deployment` (documented) — until then all
  hosts' per-IP policies collapse to the ingress IP in prod (an availability caveat, not a bypass; the
  security layer confirmed default loopback `KnownProxies` means forged `X-Forwarded-For` is ignored, so
  no spoofing).

- [wontfix] **NAT2 — LOW — reuse** — Customer `[EnableRateLimiting("public-read"|"purchase"|"review")]`
  attributes carry raw literals because `Concertable.Customer.Web.RateLimitPolicies` (the registration-side
  constants) is unreferenceable from the module Api projects, and — unlike B2B, whose constants sit in the
  universally-referenced `Tenant.Contracts` — Customer has **no** service-wide assembly the module
  controllers share. Deliberate trade-off, not an oversight: a typo fails fast at endpoint execution (no
  such policy) and the `public-read` + `review` literals are exercised by the trip tests; only `purchase`
  is unpinned. The durable fix is a new low-level Customer project referenced by the module Api projects +
  host (mirroring `Tenant.Contracts`) — a project-topology decision surfaced to Tommy rather than made
  unilaterally mid-review.

### Layers

- **Layer 1 (native `code-reviewer`, medium):** verified every `[EnableRateLimiting]` string matches a
  registered `AddRateLimitPolicy` name across all five services (no mismatch), the relax/constrain config
  seam overrides `PermitLimit` over code defaults with `QueueLimit`=0 (immediate reject), and the two trip
  tests use disjoint policies/partitions on a shared fixture (deterministic). Findings NAT1/NAT2 above.
- **Layer 1d (security):** verdict — **no new vulnerability introduced.** ForwardedHeaders trust is safe
  in prod (default loopback `KnownProxies` ⇒ `checkKnownIps` true ⇒ forged `X-Forwarded-For` ignored);
  limiter placed after auth in all five hosts (per-user `sub` populated) and after routing (attribute
  metadata resolves); `RateLimitingTestConfig` is test-project-only and cannot leak to prod; the credential
  page-model attributes cover the `OnPost` brute-force paths and Duende ROPC is E2E-only; `OnRejected`
  leaks no identifiers/secrets; no `[Authorize]`/`[HasPermission]` weakened (all additive).
- **Layer 2 (Concertable lenses):** microservice isolation / module boundaries / seeding — N/A (no
  cross-service or cross-module refs added; B2B constants reuse the already-shared `Tenant.Contracts`; no
  persistence/seeder changes). C# conventions — clean after the `is int permit` fix; no inline logging, no
  primary constructors, single-statement branches unbraced. Test coverage (Lens F) — the plan scopes
  "one integration test per partition kind"; both (per-IP `public-read`, per-user `review`) are covered.
