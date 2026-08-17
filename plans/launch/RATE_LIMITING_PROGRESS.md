# API rate limiting progress

- Plan: `plans/launch/RATE_LIMITING_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/rate-limiting`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting`
- Branch: `Feature/launch_rate-limiting`
- PR: #646 (Phase 1, draft — Concertable.ServiceDefaults rate-limiting seam)
- Dependency/package gates: Phase 2 (Auth + B2B consumers) is delivery-gated on Phase 1's ServiceDefaults
  package publishing + the `chore/platform-sync-*` pin bump. No inbound blockers.
- Last reconciled: 2026-08-17, from `origin/main` + repository evidence.

## Current state

**Phase 1 complete locally, pushed to a draft PR.** The shared web-only seam is implemented in
`Concertable.ServiceDefaults`: `RateLimitingOptions` (+ `RateLimitWindow`) with the plan's hard-coded
defaults, `RateLimitPolicies` public constants (`Login`/`Apply`/`Messaging`/`Upload`),
`AddDefaultRateLimiting(this IHostApplicationBuilder)` (global fallback keyed on `sub` else IP, four
named fixed-window policies, `OnRejected` → 429 + `Retry-After` + ProblemDetails when the host registered
it) and `UseDefaultRateLimiting(this WebApplication)`. No new package references on the shipping project
(framework `Microsoft.AspNetCore.App` already carries the rate-limiting APIs). Focused test project green.

Phase 2 (consumers) is unchanged and cannot be implemented/verified locally yet: Auth + B2B reference
ServiceDefaults as a **published feed package**, so they cannot compile against the new extension methods
until Phase 1 merges, `publish-packages` republishes, and `platform-sync` bumps the pins. Merge requires
Tommy's explicit go-ahead.

Confirmed endpoints (for Phase 2): apply = `POST /api/Application/{opportunityId}`
(`ApplicationController.Apply`); upload = `POST /api/Blob/upload` (`BlobController.Upload`, anonymous
today); login = Auth `/connect/token` + `Pages/Account` POSTs. Messaging send surface still unconfirmed —
`MessageController` is read + `mark-read` only; send path (SignalR `NotificationHub` or event-generated) to
be located in Phase 2.

## Next Steps

**Paused: Tommy — review the Phase 1 draft PR and authorize its merge.** Phase 1 is implemented, built,
and tested; nothing further is safely implementable locally.

- **Resume when:** the Phase 1 PR is merged, `publish-packages` has republished ServiceDefaults, and its
  `chore/platform-sync-*` PR is green/merged (the new `<ConcertablePlatformVersion>` pin is on the feed).
- **Then:** implement **Phase 2 — opt in and apply policies in Auth & B2B** per the plan. Auth:
  `AddDefaultRateLimiting()`/`UseDefaultRateLimiting()` + `Login` on the token endpoint and `Pages/Account`
  POSTs. B2B: the pair placed after auth/`TenantResolutionMiddleware`, before authorization/`MapControllers`;
  `Apply`→`ApplicationController.Apply`, `Upload`→`BlobController.Upload`, `Messaging`→the located
  message-send surface. Add a B2B integration test proving a throttled endpoint returns 429 + `Retry-After`.
- Route Phase 1 through `/review` before requesting merge.

## Completed work

- **Phase 1 implemented** in `api/Concertable.ServiceDefaults/`: `RateLimitPolicies.cs`,
  `RateLimitingOptions.cs`, `RateLimitingExtensions.cs`. Main csproj excludes `tests/**` from its default
  compile/pack globs (the test project nests under the package folder to inherit its props/nuget config).
- **Focused test project** `tests/Concertable.ServiceDefaults.Tests` — boots an in-memory `WebApplication`
  (TestHost) with a stub endpoint under `RateLimitPolicies.Apply`, drives it past a config-bound limit, and
  asserts 429 + a `Retry-After` header. Also proves config binding (limit supplied via in-memory config).
  Added to `api/Concertable.slnx`; test package versions added to ServiceDefaults `Directory.Packages.props`.
- **Distributed-store deferral** logged in `api/TECH_DEBT.md` (in-process limiter loosens per-replica under
  horizontal scale; acceptable at single-instance launch).
- Plan + this ledger authored earlier.

## Verification

- `dotnet build Concertable.ServiceDefaults.csproj` → 0 warnings, 0 errors.
- `dotnet test` (`Concertable.ServiceDefaults.Tests`) → 1 passed. Over-limit request returns
  `429 TooManyRequests` with a `Retry-After` header; limit driven from bound config.
- Full solution build / carve / integration matrix deferred to draft-PR CI (remote-first).

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **Seam = ServiceDefaults, web-only opt-in pair.** See plan "The seam" for the full justification.
- **Producer→consumer split is mandatory, not stylistic.** ServiceDefaults is consumed as a published
  feed package and is not in the `UseLocalCore` churny-core swap set, so consumers cannot compile against
  the new extension methods until it republishes and pins bump. Hence Phase 1 (publish) gates Phase 2.
- **Partition on `sub`, not tenant.** Tenant is B2B-specific and absent from the shared seam; `sub`
  partitioning stays agnostic and satisfies the roadmap. Tenant-level refinement noted as future, not built.
- **Deviation from the kickoff's `git checkout -b`:** the main checkout had unrelated dirty/untracked
  files, so the plain checkout aborted. Used an isolated worktree off `origin/main` instead (the
  plans/AGENTS-preferred path for plan work) — main checkout left untouched.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting
Read @plans/launch/RATE_LIMITING_PLAN.md and @plans/launch/RATE_LIMITING_PROGRESS.md and do what its `## Next Steps` says.
```
