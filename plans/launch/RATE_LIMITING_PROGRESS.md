# API rate limiting progress

- Plan: `plans/launch/RATE_LIMITING_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/rate-limiting`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting`
- Branch: `Feature/launch_rate-limiting`
- PRs: #646 (superseded seam v1, **MERGED**) · #655 (**producer v2 — seam refactor**, **MERGED** 2026-08-19T20:34Z) · #663 (`chore/platform-sync-0.1.0-alpha.0.1078` — sync from #655, **MERGED** 2026-08-19T21:25Z) · **#670 (Phase 2 consumers — all 5 services, DRAFT, open)**
- Last reconciled: 2026-08-19, from `origin/main` + repository evidence. #655 + #663 both merged; all five services pin `0.1.0-alpha.0.1078` (the opt-in seam). Spent #655 worktree closed; fresh Phase-2 worktree recreated off `origin/main` (`7f782a237`).

## Current state

**Design changed mid-flight (with Tommy): from a global-fallback + central-policy model to opt-in
named policies, shared mechanism + per-service ownership, across all five services.** Rationale and the
endpoint evidence (36 of 146 HTTP endpoints — ~25% — are real abuse surfaces) are in the plan's "Why
opt-in" section.

**Producer (Phase 1, #655) implemented and green locally.** `Concertable.ServiceDefaults` rebuilt to the
opt-in seam: `AddDefaultRateLimiting` (plumbing + 429/`Retry-After` `OnRejected` only), `AddRateLimitPolicy`
(one named fixed-window policy, lazy `IOptionsMonitor<RateLimitWindow>` config binding), `RateLimitWindow`,
`UseDefaultRateLimiting`. Removed the old global limiter, `RateLimitPolicies` constants, and
`RateLimitingOptions`. The lazy binding fixes the earlier eager-bind wart (which had forced env-var-only
test overrides). Unit tests: partition trips 429 + `Retry-After`, and per-user vs per-IP key resolution — **5 pass**.

**Producer seam now live on `main`.** All five services pin `0.1.0-alpha.0.1078`, which carries the opt-in
seam (`AddDefaultRateLimiting`, `AddRateLimitPolicy(name, RateLimitWindow, perUser)`, `UseDefaultRateLimiting`).
Phase 2 (consumer wiring) is unblocked and in progress on this branch.

**Phase-2 surface reconciled against live code (subagent sweep, 2026-08-19).** No service currently references
any rate-limiting API — greenfield. `UseForwardedHeaders` is present only in Auth (line 179); B2B, Customer,
Search, Payment have none and need it added before the limiter for the per-IP policies. No explicit
`UseRouting` in B2B/Customer/Search/Payment (implicit routing) — the limiter goes after `UseAuthentication`.
No integration fixture disables rate limiting today (the old env-var seam is gone) — the shared helper is a
net-new `IntegrationTestHostExtensions` step; each service owns its own `ApiFixture`, there is no shared base.

## Next Steps

**Phase 2 consumer rollout implemented on this branch and building 0/0 (full `api/Concertable.slnx` + each
touched test project).** Remaining is delivery only:

- **Draft PR #670 open**; PR CI owns build/carve/unit/integration (no positive E2E trigger — see plan's Validation posture).
- **`/review`** the branch; address findings (also produces the review file the merge gate requires).
- **`/merge`** once green; then follow the `api/**` platform-sync PR to green/merged (this touches `api/**`,
  so it republishes ServiceDefaults + all consumers and re-bumps the pin — non-breaking, should auto-merge).
- On merge, the plan is terminal → delete `RATE_LIMITING_PLAN.md` + this ledger (roadmap already ticked, not deleted).

## Completed work

- **Design decision** — opt-in, no global fallback; shared mechanism + per-service policies; all 5
  throttles one endpoint, no global cap.
- **Phase 1 producer refactor** — the ServiceDefaults seam above; unit tests rewritten and green; obsolete
  consumer wiring stripped. Shipped as **#655, MERGED**; published pin `0.1.0-alpha.0.1078`.
- **Platform sync #663 MERGED** — bumped every service's `<ConcertablePlatformVersion>` to `0.1.0-alpha.0.1078`
  (non-breaking; no consumer used the API yet). Spent #655 worktree closed (`-PlanManaged`); Phase-2 worktree
  recreated off `origin/main`.
- **#646** — merged seam v1 (global + central policies); superseded by this refactor.
- **Phase 2 consumer rollout (this branch)** — all five web hosts opted into the seam with `UseForwardedHeaders`
  before `UseDefaultRateLimiting` (placed after auth); named policies + `[EnableRateLimiting]` across the surface
  table. Policy-name constants per service (`RateLimitPolicies` in Search.Api / Payment.Api / Auth / B2B
  Tenant.Contracts; Customer host + matching literals, no service-wide assembly). Shared
  `RateLimitingTestConfig.RelaxRateLimiting`/`ConstrainRateLimiting` in `Concertable.Testing.Integration`; every
  web `ApiFixture` relaxes via one line (`ApiFixture` made a virtual `RateLimitPermit` hook so the trip fixture
  is a 4-line subclass, not a copy). Two integration trip tests (per-IP + per-user, 429 + `Retry-After`) in the
  Customer Review module (`RateLimitApiTests`). Adjacent auth gaps logged in `api/TECH_DEBT.md`; roadmap line 44 + §7 ticked.

## Verification

- Producer unit tests: `Concertable.ServiceDefaults.UnitTests` — 5 pass (from #655).
- Phase 2: `dotnet build api/Concertable.slnx` → **0 errors**; each touched test project builds clean
  (Customer Review / Auth / Search integration test projects).
- Integration trip tests + full build/carve/unit/integration matrix owned by draft-PR CI (remote-first; no local E2E).

## Reviews

- Producer PR #655 (seam refactor) — **`/review` complete 2026-08-19 (medium), both layers clean.** One
  Lens F gap (lazy config binding unpinned) fixed in-pass (test added, commit `9d2921e8f`); no open
  findings. See `reviews/Feature-launch_rate-limiting.md` (2026-08-19 section). The prior 2026-08-17 pass
  covered the now-superseded seam v1.

## Decisions, discoveries, blockers, and deviations

- **Opt-in, no global fallback** (this session, with Tommy). ~25% of endpoints need a limit; two-thirds
  of those are anonymous (per-IP), the rest authenticated writes (per-user). A global per-user cap helps
  the ~75% workflow-bounded endpoints not at all, can't partition the anonymous majority, and risks
  false-positives on legit burst traffic.
- **Shared mechanism, per-service policies** — ServiceDefaults owns plumbing + `AddRateLimitPolicy` only;
  each service names and owns its policies. Honors "shared is the intersection, never the union".
- **Lazy config binding** — named `IOptionsMonitor<RateLimitWindow>` resolved per request, replacing the
  eager `Bind()` at builder time that had forced integration tests to disable the limiter via env vars.
  Phase 2's shared test helper can now use ordinary config.
- **Forced publish-first split** — `Concertable.ServiceDefaults` is a feed package, not in the
  `UseLocalCore` swap set, so consumers can't compile against the new API until it republishes; a combined
  producer+consumer PR can't pass CI. #655 = producer; a follow-up branch = consumers.
- **`/connect/token` not throttled; Payment webhook never throttled; `Messaging` protects `report`** —
  carried over from the sweep (no user-facing message-compose endpoint exists; `report` is the spammable
  conversations write).
- **Adjacent auth gaps found by the sweep (to log/fix in the consumer PR, not this producer PR):** Auth
  `POST /Account/ChangePassword` unthrottled (Phase 2 adds a per-user policy); B2B anon `DELETE
  api/blob/{fileName}` + `GET download`; Payment `GET /api/Transaction` missing `[Authorize]` — the last
  two logged in `api/TECH_DEBT.md` alongside the Phase 2 wiring.
- **Partition on `sub`, not tenant**; in-process limiter only (distributed store deferred).
