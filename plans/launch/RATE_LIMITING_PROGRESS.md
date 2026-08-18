# API rate limiting progress

- Plan: `plans/launch/RATE_LIMITING_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/rate-limiting`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting`
- Branch: `Feature/launch_rate-limiting`
- PRs: #646 (Phase 1, **MERGED** — ServiceDefaults seam) · #655 (Phase 2, **draft** — Auth + B2B consumers)
- Dependency/package gates: **cleared.** Phase 1 published `Concertable.ServiceDefaults` `0.1.0-alpha.0.1070`;
  `chore/platform-sync-0.1.0-alpha.0.1070` (#654) merged, bumping every pin. No inbound blockers.
- Last reconciled: 2026-08-18, from `origin/main` + repository evidence.

## Current state

**Both phases implemented. Phase 1 merged; Phase 2 built green and pushed to draft PR #655.**

Phase 1 (the shared web-only seam in `Concertable.ServiceDefaults`) merged via #646 and is on the feed at
`0.1.0-alpha.0.1070`. Phase 2 opts Auth and B2B into it and attaches the named policies to the endpoints
each service owns:

- **Auth** — `AddDefaultRateLimiting()` + `UseDefaultRateLimiting()` (after `UseRouting`); `Login` on the
  `Pages/Account` credential POSTs (`Login`/`Register`/`ForgotPassword`/`ResetPassword`).
- **B2B** — the pair placed after auth/`TenantResolutionMiddleware`, before authorization/`MapControllers`;
  `Apply` → `ApplicationController.Apply`, `Upload` → `BlobController.Upload`, `Messaging` →
  `MessageController.Report`. `UseForwardedHeaders` added before the limiter.

Two evidence-based deviations from the presumed surface (see `## Decisions` below): `/connect/token` is
**not** thrown on the tight `Login` cap (it carries service auth in prod), and `Messaging` protects the
`Report` write because there is **no** user-facing message-compose endpoint.

Builds: Auth, B2B.Web, and the Concert integration test project all compile 0/0. The full
build/carve/unit/integration matrix is owned by draft-PR CI (remote-first); the Apply 429 integration
test runs there.

## Next Steps

**Paused: Tommy — review the Phase 2 draft PR #655 and authorize its merge.** Phase 2 is implemented,
built, and pushed; nothing further is safely implementable locally. Draft-PR CI is validating the exact
remote head (build, carve, unit, integration).

- **Resume when:** Tommy authorizes the merge. On merge of an `api/**` change, `publish-packages`
  republishes and `platform-sync` opens a `chore/platform-sync-*` pin bump — follow it to green/merged
  (additive consumer-only change, so non-breaking → auto-merges).
- **Then:** the lifecycle is terminal. Close the worktree with
  `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 655 -PlanManaged`, and delete this plan +
  ledger from a `Docs/*_closeout` worktree (git history is the archive).

## Completed work

- **Phase 1 (merged, #646)** — `RateLimitPolicies`, `RateLimitingOptions`, `RateLimitingExtensions`
  (`AddDefaultRateLimiting`/`UseDefaultRateLimiting`, global fallback + four named fixed-window policies,
  `OnRejected` → 429 + `Retry-After` + ProblemDetails) in `Concertable.ServiceDefaults`, plus its unit test
  project. Distributed-store deferral logged in `api/TECH_DEBT.md`.
- **Phase 2 (draft, #655)** — consumer wiring + policy application in Auth & B2B per the plan. Added
  `Concertable.ServiceDefaults` package reference to `Concertable.B2B.Concert.Api` and
  `Concertable.B2B.Conversations.Api` (needed for the `RateLimitPolicies` constants). Added
  `ApplicationRateLimitApiTests` (Apply → 429 + `Retry-After`, isolated via a unique `sub`). Added the
  production trusted-proxy `ForwardedHeaders` dependency to `api/TECH_DEBT.md` (owned by
  `launch/config-and-deployment`). Roadmap line 44 + §7 line ticked in the shipping commit.

## Verification

- `dotnet build` → 0/0 for `Concertable.Auth`, `Concertable.B2B.Web`, and
  `Concertable.B2B.Concert.IntegrationTests` (transitively builds the module Api projects + fixture).
- ServiceDefaults `0.1.0-alpha.0.1070` restores from the feed with the new extension methods (Auth built
  clean against it — proves the published version carries Phase 1).
- Apply 429 + `Retry-After` integration test authored; runs in draft-PR CI (Docker/Testcontainers), not
  locally (remote-first).

## Reviews

- 2026-08-17 `/review` (medium) on Phase 1 → clean (`reviews/Feature-launch_rate-limiting.md`). The
  per-IP `ForwardedHeaders` note it pinned is now addressed (middleware wired before the limiter in both
  hosts; production trusted-proxy binding logged as config-gate debt).
- Phase 2 review pending (draft-PR CI + any `/review` before merge authorization).

## Decisions, discoveries, blockers, and deviations

- **`/connect/token` is NOT throttled with `Login`.** ROPC is registered only under `IsE2E()`
  (`Auth/Program.cs`), so production `/connect/token` carries service client-credentials (B2B/Customer/Auth
  all POST there) + auth-code exchange + refresh — no password entry. A tight per-IP Login cap there would
  throttle the platform's own service auth for zero brute-force benefit; the real interactive surface is
  the `Pages/Account` Razor Pages, which carry `Login`. The token endpoint keeps the generous global
  fallback limiter.
- **No user-facing message-compose surface exists.** `MessageController` is read/`mark-read`/`report`;
  `NotificationHub` has no client-invokable send method; the only caller of `IConversationsModule.SendAsync`
  is `ApplicationNotifier` (messages are lifecycle-generated, already bounded by `Apply`). So `Messaging`
  protects the one spammable conversations write, `Report`. A future direct-compose endpoint must carry
  `[EnableRateLimiting(RateLimitPolicies.Messaging)]`.
- **Per-IP correctness is config-gated.** Both hosts run `UseForwardedHeaders` before the limiter, but the
  production trusted-proxy `ForwardedHeadersOptions` binding is owned by `launch/config-and-deployment`
  (logged in `api/TECH_DEBT.md`). Until then, behind an ingress the per-IP policies collapse to the proxy IP
  (fails toward over-limiting — not a security hole).
- **Partition on `sub`, not tenant** (Phase 1 decision, unchanged). In-process limiter only (deferred).
- **Worktree recreated off `origin/main` for Phase 2** after #646 merged and its worktree was closed —
  the standard plans/AGENTS per-PR-slice lifecycle.
