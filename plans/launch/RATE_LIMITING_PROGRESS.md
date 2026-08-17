# API rate limiting progress

- Plan: `plans/launch/RATE_LIMITING_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/rate-limiting`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_rate-limiting`
- Branch: `Feature/launch_rate-limiting`
- PR: not opened
- Dependency/package gates: Phase 2 (Auth + B2B consumers) is delivery-gated on Phase 1's ServiceDefaults
  package publishing + the `chore/platform-sync-*` pin bump. No inbound blockers.
- Last reconciled: 2026-08-17, from `origin/main` (@bfbfd863c) + repository evidence.

## Current state

Plan authored. Design decided and verified against `origin/main`: the shared seam is
`Concertable.ServiceDefaults` (a feed package every web host consumes via `AddServiceDefaults`; Auth
references it but not `Concertable.Shared.Api`; it already carries `FrameworkReference
Microsoft.AspNetCore.App`, so no new package refs). Rate limiting ships as a **separate web-only** opt-in
pair `AddDefaultRateLimiting` / `UseDefaultRateLimiting` (not folded into `AddServiceDefaults`, which
non-web Workers/Simulator hosts also call). Policy definitions centralized in ServiceDefaults; policy
application per-endpoint in Auth (login) and B2B (apply/messaging/upload). No code written yet.

Claim confirmed: zero `AddRateLimiter`/`UseRateLimiter` in `api/` on current `origin/main`.

Confirmed endpoints: apply = `POST /api/Application/{opportunityId}` (`ApplicationController.Apply`);
upload = `POST /api/Blob/upload` (`BlobController.Upload`, anonymous today); login = Auth `/connect/token`
+ `Pages/Account` POSTs. Messaging send surface unconfirmed — `MessageController` is read + `mark-read`
only; send path (SignalR `NotificationHub` or event-generated) to be located in Phase 2.

## Next Steps

Implement **Phase 1 — rate-limiting seam in `Concertable.ServiceDefaults`** (all local; no blockers):

1. In `api/Concertable.ServiceDefaults/`, add `RateLimitingOptions` (sane hard-coded defaults per the
   plan's limits table), a public `RateLimitPolicies` constants class (`Login`/`Apply`/`Messaging`/`Upload`),
   `AddDefaultRateLimiting(this IHostApplicationBuilder)` registering `AddRateLimiter` with a global
   fallback (partition on `sub` claim else client IP), the four named fixed-window policies, and an
   `OnRejected` that sets 429 + `Retry-After` from `MetadataName.RetryAfter`; and
   `UseDefaultRateLimiting(this WebApplication)`. No new package references.
2. Add a focused test (new ServiceDefaults test project) booting a minimal in-memory `WebApplication` with
   a stub endpoint under `RateLimitPolicies.Apply`, asserting the over-limit request returns 429 with a
   `Retry-After` header.
3. Log the in-process-only / no-distributed-store deferral in `api/TECH_DEBT.md`.
4. Build `Concertable.ServiceDefaults` + the test project; run the focused test to green. Commit and push
   the coherent checkpoint to a draft PR (`gh pr create`, plain GitHub — personal repo).

Stop after pushing the draft PR. Delivery gates and the Phase 2 handoff are described in the plan's
Phase 1 delivery notes; they wait for explicit instruction and a `/review` pass.

## Completed work

- Plan + this ledger authored (`plans/launch/RATE_LIMITING_PLAN.md`, this file). Design verified against
  `origin/main` evidence; `plan_graph.py` run clean.

## Verification

- `grep -rn "AddRateLimiter|UseRateLimiter" api/` (worktree, `origin/main`) → zero matches. Claim holds.
- Seam facts on `origin/main`: ServiceDefaults `IsPackable`/`FrameworkReference Microsoft.AspNetCore.App`;
  B2B.Web + Auth `PackageReference Concertable.ServiceDefaults`; Auth has no `Shared.Api` ref; every web
  host calls `AddServiceDefaults`.
- No code/build verification yet (no implementation).

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
