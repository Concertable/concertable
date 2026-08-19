# API rate limiting

Throttle Concertable's genuine abuse surfaces — credential entry, file upload, payment/cost-amplification,
spammable user-content writes, and anonymous scraping reads — with .NET's built-in `AddRateLimiter`.
Rejections return **429** with a **`Retry-After`** header. **Opt-in per endpoint, no global fallback**;
in-process limiter only; identity-aware partitioning (per authenticated `sub` where identity exists, per
client IP for anonymous surfaces).

Next steps live in **@plans/launch/RATE_LIMITING_PROGRESS.md → `## Next Steps`**. This plan holds the
design and outstanding phases; the ledger holds operational truth. Read [`plans/AGENTS.md`](../AGENTS.md)
and [`plans/agents/PLAN.md`](../agents/PLAN.md) before working from it.

## Why opt-in, not a global fallback (the decision, with evidence)

An earlier iteration installed a **global fallback limiter** (a blanket 200/min cap over every endpoint)
plus four centrally-defined named policies. That was reversed after enumerating the actual surface. A
subagent sweep classified **every** HTTP endpoint across all five services:

| Service | HTTP endpoints | Need a limit | % |
|---|---|---|---|
| B2B | 95 | 16 | 17% |
| Customer | 25 | 9 | 36% |
| Auth (app handlers) | 13 | 5 | 38% |
| Search | 7 | 5 | 71% |
| Payment | 6 | 1 | 17% |
| **Total** | **146** | **36** | **~25%** |

Only ~25% are real abuse/cost surfaces (fewer still counting Auth's framework OIDC endpoints and
Payment's 29 internal gRPC methods, all "no"). The other ~75% are authenticated, permission-gated,
tenant-scoped domain CRUD, internal service-to-service, webhooks, admin, or health — bounded by the
exactly **one** endpoint (upload) for its whole platform, with no global cap.

A global fallback therefore buys little (there are no meaningful "forgotten endpoints" on a bounded,
known surface) and costs real risk: a blanket per-user cap does nothing for the ~75% workflow-bounded
endpoints, cannot partition the anonymous majority (no `sub` to key on → collapses to IP), and can
false-positive on legitimate high-volume traffic (a SPA firing 20-30 calls per page load, shared-NAT
clients). **Decision: opt-in named policies only, each attached to a deliberately-identified surface.**

Two-thirds of the 36 surfaces are **anonymous** (marketplace/profile/review reads, search/autocomplete,
the Auth credential POSTs, anon blob upload) → **per-IP**. The rest are authenticated writes → **per-user**.

## The seam — shared mechanism, per-service policies

**`Concertable.ServiceDefaults` owns only the repeated mechanism; each service defines and owns the named
policies for the surfaces it exposes.** This matches "shared is the intersection, never the union"
(`api/AGENTS.md`) — ServiceDefaults never learns any one service's specific abuse surfaces.

- **`Concertable.ServiceDefaults` is the one web seam every host consumes** (referenced as a feed package
  by every service; `Concertable.Shared.Api` is not — Auth is Razor Pages, no controllers). It already
  carries `FrameworkReference Microsoft.AspNetCore.App`, so `AddRateLimiter`/`UseRateLimiter` and the
  `System.Threading.RateLimiting` primitives need **zero new package references**.
- **Web-only opt-in pair, not folded into `AddServiceDefaults`** (which non-web Workers/Seed hosts also
  call). Web hosts call `AddDefaultRateLimiting()` + `UseDefaultRateLimiting()`.
- **`AddDefaultRateLimiting`** registers `AddRateLimiter` with the 429 rejection status and the shared
  `OnRejected` (429 + `Retry-After` from the lease's `MetadataName.RetryAfter` + a ProblemDetails body).
  **No global limiter, no named policies.**
- **`AddRateLimitPolicy(policyName, RateLimitWindow defaults, bool perUser)`** declares one named
  fixed-window policy. Each service calls it once per surface class it owns. Window binds from
  `RateLimiting:<policyName>` config **lazily** (named `IOptionsMonitor<RateLimitWindow>`, resolved per
  request) over the passed defaults — so a host or integration test that layers configuration after
  builder creation still wins. `perUser` partitions on `sub` (fallback IP); `false` always partitions IP.
- **`RateLimitWindow`** (`PermitLimit` / `WindowSeconds` / `QueueLimit`) is the config primitive the
  `launch/config-and-deployment` gate later tunes per section without a code change.

### Middleware placement

`UseDefaultRateLimiting()` (= `app.UseRateLimiter()`) sits **after** authentication (so `sub` is populated)
and routing (so endpoint-metadata policies resolve), **before** the endpoint terminals. Per-IP policies
need real client IPs, so each host runs `UseForwardedHeaders` (trusted-proxy binding owned by
`launch/config-and-deployment`) before the limiter.

## The surfaces (opt-in targets, from the sweep)

| Service | Policy | Endpoints | Partition |
|---|---|---|---|
| Auth | credential (per-IP) | `Pages/Account` Login/Register/ForgotPassword/ResetPassword POSTs | IP |
| Auth | change-password (per-user) | `POST /Account/ChangePassword` (currently **unthrottled — gap**) | user |
| B2B | public-read (per-IP) | anon `artist/{id}`, `venue/{id}`, the 4 review GET/summary endpoints | IP |
| B2B | upload (per-IP) | anon `POST api/blob/upload` | IP |
| B2B | apply / messaging / checkout (per-user) | application submit, message `report`, artist checkout create | user |
| B2B | profile-image (per-user) | artist/venue profile create+update (multipart image) | user |
| Customer | public-read (per-IP) | anon `artist/{id}`, `concert/{id}`, `venue/{id}`, the 3 review-list GETs | IP |
| Customer | purchase / review (per-user) | `POST ticket/purchase`+`checkout`, review POST | user |
| Search | search (per-IP) | the 5 `[AllowAnonymous]` query/autocomplete/header endpoints | IP |
| Payment | setup-intent (per-user) | `POST api/StripeAccount/setup-intent` (creates a Stripe object) | user |

**Never limit:** Payment `POST /api/Webhook` (Stripe, signature-verified, bursty/retried — a 429 drops
real payment events), the gRPC surface, framework OIDC/token endpoints, health probes.

## Constraints / out of scope

- **In-process limiter only.** `AddRateLimiter` is per-instance in-memory; under horizontal scale each
  replica limits independently. A distributed/Redis-backed store is out of scope, logged in
  [`api/TECH_DEBT.md`](../../api/TECH_DEBT.md).
- **Not in scope:** the config/secrets deployment store (`launch/config-and-deployment`, which later
  tunes the `RateLimiting:*` sections and the production trusted-proxy binding), tenant-level partition
  refinement, and a CDN/gateway rule (roadmap R11).
- **Adjacent auth gaps found by the sweep, logged not fixed here** (different concern from rate limiting):
  B2B `DELETE api/blob/{fileName}` + `GET download` are anonymous; Payment `GET /api/Transaction` has no
  `[Authorize]`. Logged in `api/TECH_DEBT.md`.

## Delivery — forced publish-first split

`Concertable.ServiceDefaults` is a published feed package and is **not** in the `UseLocalCore` swap set,
so consumers only ever compile against its published pin. The new `AddRateLimitPolicy` API therefore
cannot be consumed — locally or in CI — until ServiceDefaults is republished. A single PR carrying both
the seam refactor and the new consumer wiring cannot pass CI. Hence two PRs:

### Phase 1 — Rate-limiting seam refactor in `Concertable.ServiceDefaults` (producer) — this PR

Rebuild the seam to the opt-in shape: `AddDefaultRateLimiting` (plumbing only), `AddRateLimitPolicy`,
`RateLimitWindow`, `UseDefaultRateLimiting`; lazy named-options config binding (fixes the earlier
eager-bind wart that forced env-var-only test overrides). Remove the old global limiter, `RateLimitPolicies`
constants, and `RateLimitingOptions`. Unit tests prove the partition primitive trips (429 + `Retry-After`)
and resolves per-user vs per-IP keys.

- **Verification gate:** build `Concertable.ServiceDefaults` + its test project; unit tests green.
- **Delivery:** own PR (#655, repurposed). Merge → `publish-packages` republishes → `platform-sync` bumps
  every pin. Non-breaking on `main` (no consumer there uses the rate-limit API yet), so the sync
  auto-merges green. This is the hard delivery gate for Phase 2.

### Phase 2 — Opt in and apply policies across all five services (consumers) — new branch

Delivery-gated on Phase 1's published version + platform sync. In each web host: `AddDefaultRateLimiting()`
+ `UseDefaultRateLimiting()` (correctly placed), `UseForwardedHeaders` before the limiter, `AddRateLimitPolicy`
for each surface class the service owns (per the table above), and `[EnableRateLimiting(<policy>)]` on the
endpoints. Policy-name constants live per service. Lift the integration-fixture "disable rate limiting"
step into one shared `Concertable.Testing.Integration` helper (no per-fixture copy-paste; uses normal
config now the eager-bind is fixed). One integration test per partition kind proves 429 + `Retry-After`.
Tick roadmap line 44 + §7 in the shipping commit.

## Definition of done

- Seam shipped and published (Phase 1); the ~36 identified surfaces throttled across all five services
  with correct per-IP/per-user partitioning (Phase 2); rejections return 429 + `Retry-After`; automated
  tests prove a limiter trips.
- Roadmap line 44 (`launch/rate-limiting`, 🟠 → ✅) and the §7 launch-ready line ticked in the same commit
  as the shipping (Phase 2) work; roadmap not deleted.
- Distributed-store deferral and the adjacent auth gaps recorded in `api/TECH_DEBT.md`.

## Validation posture

Remote-first: draft PR at the first coherent checkpoint; PR CI owns build/carve/unit/integration. **No
local E2E** — touches no positive E2E trigger (see [`docs/REMOTE_VALIDATION.md`](../../docs/REMOTE_VALIDATION.md)
and the merge skill's Step 4).
