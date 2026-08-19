# API rate limiting progress

- Plan: `plans/launch/RATE_LIMITING_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/rate-limiting`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting`
- Branch: `Feature/launch_rate-limiting`
- PRs: #646 (superseded seam v1, **MERGED**) · #655 (**producer v2 — seam refactor**, this branch) · consumer PR (all 5 services, **not yet created** — gated on #655 publish)
- Last reconciled: 2026-08-19, from `origin/main` + full endpoint sweep (3 subagents) + repository evidence.

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
test overrides). Unit tests: partition trips 429 + `Retry-After`, and per-user vs per-IP key resolution — **4 pass**.

**PR #655 repurposed to producer-only.** The obsolete Phase-2 consumer wiring it carried (Auth + B2B
tagging, built for the abandoned global design, referencing the now-removed `RateLimitPolicies`) was
reverted to `origin/main`; the old `ApplicationRateLimitApiTests` deleted. #655 now diffs only the
ServiceDefaults refactor + these plan docs.

## Next Steps

**Producer refactor pushed to draft PR #655; `/review` complete and clean (see `## Reviews`). Awaiting
Tommy's merge authorization so the new seam publishes.** Local state is committed and green; branch is
current with `origin/main`.

- **Merge #655 (needs Tommy's go-ahead — review gate satisfied):** on merge of this `api/**` change, `publish-packages`
  republishes `Concertable.ServiceDefaults` and `platform-sync` opens a `chore/platform-sync-*` pin bump.
  Non-breaking (no consumer on `main` uses the rate-limit API yet) → follow it to green/merged.
- **Then create the consumer PR** (Phase 2, new branch off the new pin): opt all five web hosts into the
  seam and apply the named policies per the plan's surface table, lift the integration-fixture disable
  step into `Concertable.Testing.Integration`, add 429 + `Retry-After` integration tests (one per
  partition kind), and tick roadmap line 44 + §7 in the shipping commit.

## Completed work

- **Design decision** — opt-in, no global fallback; shared mechanism + per-service policies; all 5
  throttles one endpoint, no global cap.
- **Phase 1 producer refactor (this branch)** — the ServiceDefaults seam above; unit tests rewritten and
  green; obsolete consumer wiring stripped from #655.
- **#646** — merged seam v1 (global + central policies); superseded by this refactor.

## Verification

- `dotnet build` + `dotnet test` → 0/0, **4/4 pass** for `Concertable.ServiceDefaults.UnitTests`.
- Full build/carve/unit/integration matrix owned by draft-PR CI (remote-first).

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
