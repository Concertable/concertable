# API rate limiting

Throttle the four unthrottled abuse surfaces — **login, apply, messaging, file upload** — with .NET's
built-in `AddRateLimiter`, registered once at the shared web-host pipeline seam and applied per-endpoint
in the services that own those endpoints. Rejections return **429** with a sensible **`Retry-After`**.
In-process limiter only; identity-aware partitioning (per authenticated user where identity exists,
per-IP for anonymous surfaces).

Next steps live in **@plans/launch/RATE_LIMITING_PROGRESS.md → `## Next Steps`**. This plan holds the
design and outstanding phases; the ledger holds operational truth. Read [`plans/AGENTS.md`](../AGENTS.md)
and [`plans/agents/PLAN.md`](../agents/PLAN.md) before working from it.

## Why this, why now

Zero `AddRateLimiter` exists anywhere in `api/` (verified against `origin/main` 2026-08-17; the only
matches are the `.artifacts/` package cache's transitive `System.Threading.RateLimiting` and worktree
copies of the roadmap). Login, apply, messaging and file upload accept unlimited request volume — the
brute-force / scraping / cost-amplification gap a Stripe production review or a pen-test raises. Launch
gate `launch/rate-limiting` (roadmap §7).

## The seam — one shared web extension, opted into per host

**Decision: a single rate-limiting registration in `Concertable.ServiceDefaults`, opted into by each
web host, with policy *application* (which endpoints get which policy) owned per-service.** This is the
"shared pipeline seam" the roadmap names. The reasoning, from repository evidence:

- **`Concertable.ServiceDefaults` is the one seam every web host already consumes.** Every host calls
  `AddServiceDefaults()` / `MapDefaultEndpoints()` from it, and it is referenced as a **feed package** by
  both Auth and B2B. The alternative shared web library, `Concertable.Shared.Api`, is **not** referenced
  by Auth (Auth is Razor Pages + IdentityServer, no controllers) — so it cannot be the universal seam
  without adding a new dependency to Auth. ServiceDefaults can.
- **Zero new package references.** ServiceDefaults already carries `FrameworkReference
  Microsoft.AspNetCore.App`, so `Microsoft.AspNetCore.RateLimiting` (`AddRateLimiter`/`UseRateLimiter`)
  and the `System.Threading.RateLimiting` partition primitives are already on hand. Rate limiting is
  cross-cutting host-pipeline infrastructure of exactly the same kind as the health-checks / telemetry /
  resilience already centralized there.
- **But it must be a *separate, web-only* opt-in pair, not folded into `AddServiceDefaults`.**
  `AddServiceDefaults` is also called by non-web hosts (`*.Workers`, `*.Seed.Simulator`) that have no
  HTTP pipeline. Folding an HTTP concern into the universal host defaults would wrongly touch them. So
  ServiceDefaults exposes a dedicated `AddDefaultRateLimiting` / `UseDefaultRateLimiting` pair (matching
  its existing `AddDefaultHealthChecks` / `MapDefaultEndpoints` vocabulary) that only the *web* hosts
  opt into.
- **Definitions centralized, application per-endpoint.** Duplicating the limiter config, the 429 /
  `Retry-After` handler and the partition logic across five hosts would drift — the roadmap's whole
  point is "cheap at the shared pipeline seam." So the *policy definitions* live once in ServiceDefaults;
  only *which endpoints wear which named policy* is per-service, because those endpoints live in
  different services (login in Auth; apply / messaging / upload in B2B).
- **Why app-layer, not a CDN/gateway rule.** A gateway only sees client IP, so it cannot express
  "per authenticated user" or "per tenant" limits — the identity-aware partitioning the roadmap asks
  for *requires* the app layer. A gateway rule is complementary future defence-in-depth, never a
  substitute (and is the roadmap R11 scope-cut fallback, not this plan).

### What ServiceDefaults defines

- **`RateLimitingOptions`** — bound with **sane hard-coded defaults** now. A real config store is the
  separate `launch/config-and-deployment` gate; this plan does **not** block on it — the options type is
  simply the seam that gate will later bind. (`AddDefaultRateLimiting` reads
  `IConfiguration`/`IOptions` but falls back to the coded defaults when unset.)
- **A global fallback limiter** — partitions on the authenticated `sub` claim when present, else on
  client IP. Agnostic: every service has `sub`-bearing tokens and a remote IP. A generous ceiling so it
  only catches gross abuse on endpoints no named policy covers.
- **Four named policies**, exposed as public string constants (`RateLimitPolicies.Login` / `.Apply` /
  `.Messaging` / `.Upload`) so consumers reference constants across the package boundary, not magic
  strings.
- **`OnRejected`** — sets status **429** and writes **`Retry-After: <seconds>`** from the lease's
  `MetadataName.RetryAfter` (fixed-window limiters expose it). Emits a ProblemDetails body where the host
  supports it.

### Partitioning and default limits (tune during implementation)

| Policy | Applied to | Partition | Default (illustrative) |
|---|---|---|---|
| `Login` | Auth `/connect/token` + `Pages/Account` sign-in/register/reset POSTs | per **IP** (anonymous) | ~10 / min / IP |
| `Apply` | B2B `POST /api/Application/{opportunityId}` | per authenticated **user** (`sub`) | ~10 / min / user |
| `Messaging` | B2B conversation message-send surface (see Phase 2) | per authenticated **user** (`sub`) | ~30 / min / user |
| `Upload` | B2B `POST /api/Blob/upload` (anonymous today) | per **IP** | ~10 / min / IP |
| *(global fallback)* | any endpoint with no named policy | `sub` if present, else IP | generous ceiling, e.g. ~200 / min |

**Tenant vs user partitioning.** B2B *authority* is the active tenant (`X-Tenant-Id` → `ITenantContext`),
but tenant is a B2B-specific concept absent from the shared seam; partitioning on `sub` satisfies the
roadmap's "per-authenticated user where identity exists" without coupling ServiceDefaults to B2B's
`ITenantContext`. A tenant-level refinement (a B2B-owned policy keyed on the resolved tenant) is possible
later but is **out of scope** here — noted, not built.

### Middleware placement

`UseDefaultRateLimiting()` (= `app.UseRateLimiter()`) must sit **after** authentication (so `User`/`sub`
is populated for authenticated partitioning) and after routing (so endpoint-metadata named policies
resolve), and **before** the endpoint terminals. In B2B: after `UseAuthentication()` /
`UseMiddleware<TenantResolutionMiddleware>()`, before `UseAuthorization()` / `MapControllers()`. In Auth:
after `UseIdentityServer()` and routing.

## Constraints / out of scope

- **In-process limiter only.** `AddRateLimiter` is per-instance in-memory; under horizontal scale each
  replica limits independently. A distributed/Redis-backed store for multi-instance correctness is
  **out of scope** and logged in [`api/TECH_DEBT.md`](../../api/TECH_DEBT.md) with reasoning (Phase 1).
- **Not in scope:** the config/secrets deployment store (`launch/config-and-deployment`), the admin
  console, tenant verification, and any tenant-level partition refinement — separate roadmap gates.

## Phases

### Phase 1 — Rate-limiting seam in `Concertable.ServiceDefaults` (producer)

Implement `RateLimitingOptions`, `RateLimitPolicies` constants, `AddDefaultRateLimiting(this
IHostApplicationBuilder)` (global fallback + four named policies + `OnRejected` 429/`Retry-After`), and
`UseDefaultRateLimiting(this WebApplication)`. No new package references (framework ref already present).

- **Focused test proving a limiter trips** — a self-contained in-memory `WebApplication` (new
  `Concertable.ServiceDefaults` test project — none exists today) that maps a stub endpoint under the
  `Apply` policy and asserts the N+1th request returns **429 with a `Retry-After` header**. This proves
  the control at the seam, independent of any consumer.
- Log the distributed-store deferral in `api/TECH_DEBT.md`.
- **Verification gate:** build `Concertable.ServiceDefaults` + the new test project; focused test green.
- **Delivery:** own PR. Merge → `publish-packages` republishes → `platform-sync` opens the pin-bump PR;
  additive (new public methods) so **non-breaking → the sync PR auto-merges green**. This phase is the
  hard delivery gate for Phase 2 (consumers cannot compile against the new methods until the package is
  on the feed and pins are bumped — ServiceDefaults is a feed package, not in the `UseLocalCore` swap
  set).

### Phase 2 — Opt in and apply policies in Auth & B2B (consumers)

Delivery-gated on Phase 1's published version + platform sync. Auth and B2B are independent services and
each only consumes the already-published ServiceDefaults, so they are independently deliverable (group
into the fewest PRs).

- **Auth:** `AddDefaultRateLimiting()` + `UseDefaultRateLimiting()` at the correct pipeline point; apply
  `Login` to the token endpoint and the `Pages/Account` sign-in/register/password POSTs. `/connect/token`
  is owned by IdentityServer — attach the policy via a path-scoped limiter branch since it is not a
  controller action we map; confirm the cleanest attachment during implementation.
- **B2B:** `AddDefaultRateLimiting()` + `UseDefaultRateLimiting()` (placed after auth /
  `TenantResolutionMiddleware`, before authorization / `MapControllers`); apply `Apply` to
  `ApplicationController.Apply`, `Upload` to `BlobController.Upload`, and `Messaging` to the message-send
  surface. **Locate the message-send surface first:** `MessageController` today is read-only + `mark-read`
  (no REST send endpoint), so the user-facing send path is SignalR (`NotificationHub`) or event-generated
  — confirm at implementation; if send is SignalR-only, apply a per-user limiter check in the hub send
  method rather than endpoint metadata (endpoint rate-limiting middleware does not cover hub methods).
- **Per-IP policies need real client IPs.** `Login` and `Upload` partition on
  `Connection.RemoteIpAddress`. Behind the ingress/reverse proxy that is the proxy's IP unless
  `ForwardedHeaders` (`X-Forwarded-For`) is honoured — without it every anonymous request collapses into
  one partition and the per-IP limit becomes a single global bucket. Confirm each web host runs
  `UseForwardedHeaders` (with a trusted-proxy/network config) **before** `UseDefaultRateLimiting`, or add
  it, as part of applying the per-IP policies.
- **Integration test** in B2B's fixture proving a real throttled endpoint (`/api/Application/{id}` or
  `/api/Blob/upload`) returns 429 + `Retry-After` after the limit.
- **Verification gate:** smallest affected build (Auth host / B2B host + module); focused unit +
  integration tests green; draft-PR CI owns the full build/carve/unit/integration matrix.

## Definition of done

- `AddDefaultRateLimiting`/`UseDefaultRateLimiting` shipped in ServiceDefaults and published; login,
  apply, messaging and upload throttled in Auth + B2B; rejections return 429 + `Retry-After`; automated
  tests prove a limiter trips (seam + a real endpoint).
- Roadmap line 44 (`launch/rate-limiting`, 🟠 → ✅) and the §7 launch-ready line ("Rate limiting active
  on auth, apply, messaging and upload endpoints") ticked in the same commit as the shipping work; the
  roadmap is **not** deleted.
- Distributed-store deferral recorded in `api/TECH_DEBT.md`.

## Validation posture

Remote-first: draft PR at the first coherent checkpoint; PR CI owns build/carve/unit/integration. **No
local E2E** — this touches no positive E2E trigger (see [`docs/REMOTE_VALIDATION.md`](../../docs/REMOTE_VALIDATION.md)
and the merge skill's Step 4).
