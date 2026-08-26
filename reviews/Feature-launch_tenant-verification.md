# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `4d1111157dd817f26b2a97c93163800e89169f97`  _(2026-08-26)_
**Security-reviewed up to commit:** `4d1111157dd817f26b2a97c93163800e89169f97`  _(2026-08-26)_

> Range reviewed: `7d4dd12fb..a7bbbc47a` (4 commits) — Phase 4 of tenant verification (admin review surface).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — HIGH — Correctness** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/VerificationAdminService.cs:41` (original commit `30446a4e3`)
  `GetPendingAsync` enriched each pending row concurrently via `Task.WhenAll(pending.Data.Select(p => ToDtoAsync(p, ct)))`. `ToDtoAsync` calls `IVenueModule`/`IArtistModule.GetContactByTenantIdAsync`, backed by request-scoped `VenueReadDbContext`/`ArtistReadDbContext` (`AddDbContext`, default scoped lifetime). Two or more pending rows sharing a `TenantType` on one page ran concurrent EF Core operations against the *same* scoped `DbContext` instance, which EF Core rejects with `InvalidOperationException: a second operation was started on this context before a previous operation completed` — a guaranteed 500 on `GET /api/tenant/verification/pending` for any page with 2+ same-type pending rows. Independently confirmed by both the native review pass and my own re-inspection. Fixed by awaiting sequentially (`foreach` + `await`) in commit `4928e1647`. Regression test added: `TenantVerificationAdminApiTests.GetPending_ShouldReturn200_WhenTwoPendingRowsShareTenantType`. Follow-up (batch instead of per-row) logged as tech debt in `api/Concertable.B2B/TECH_DEBT.md` — "Admin verification queue enriches contact per row, not per page".

- [x] **BUG1 — MEDIUM — Correctness** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/VerificationAdminService.cs` (original commit `30446a4e3`, `ReviewAsync`)
  The approve/reject `notify(...)` call ran with no try/catch after `repository.SaveChangesAsync(ct)` already committed the domain transition. A transient email-transport failure would 500 an admin whose approve/reject had already durably succeeded, and a retry would then hit `VerificationReviewError.NotPending` against the decision that already landed. This codebase has an established, deliberate fix for exactly this shape: `ContentReportService` wraps its notifier call in try/catch with a dedicated `ContentReportNotificationFailed` log entry, specifically because "a transport failure must not fail a request whose write already committed, or the retry just files a duplicate." Fixed in commit `4928e1647` by mirroring that shape exactly (`VerificationReviewNotificationFailed` in `Log.cs`).

## Security review (Lens: auth gating, IDOR, data exposure)

No findings. `[Admin]` gating on `TenantVerificationAdminController` matches the established `VenueController`/`ModerationController` shape exactly, resolves through the real DB-backed admin-profile check (unmodified by this diff), and every action's tests assert 401/403 against real auth middleware. `tenantId` taken directly from the route is correct for an admin actor (no tenant-owner check needed — mirrors `VenueController.Approve(venueId)`). `GetContactByTenantIdAsync` is keyed by the row's own server-resolved `TenantId`, so no cross-tenant data mixing is possible, and the whole surface is reachable only through the `[Admin]` gate.

## Test coverage (Lens F)

Covered by `VerificationAdminApiTests.cs` (renamed from `TenantVerificationAdminApiTests.cs`): auth gates (401/403) on all three actions, 404/409 error paths, both `TenantType` branches of contact enrichment (venue and artist), notification-email capture on approve/reject, and the same-`TenantType` concurrency regression.

## Incremental review — 2026-08-26

Range: uncommitted redesign on top of `7fce9d54b`, driven by live design discussion, not a bug hunt — recorded here for the record since it changes shape substantially.

- **Controller/service consolidation**: `VerificationAdminController`/`IVerificationAdminService`/`VerificationAdminService` merged into `VerificationController`/`IVerificationService`/`VerificationService`, matching `VenueController`/`VenueService`'s existing shape (one controller/service per resource, per-action authorization, not per-actor-type). Route segment named once via `internal const string RouteSegment` and interpolated into every route (including the organization-prefixed absolute overrides), matching `VenueController`/`VenueReviewController`'s existing convention exactly — this pattern wasn't written down anywhere generic before; a doc PR was opened against `tomjseery/dotagents` (`HTTP_API.md`) to fix that gap.
- **`TenantContact` (Venue.Contracts, Artist.Contracts)** changed from `sealed record` to `readonly record struct` — fits the existing `csharp-style` standard's "small immutable value, identity is its fields, harmless default" rule, matching the `Money` precedent. Caused a real, correctly-scoped compile break in `VenueService.GetContactByTenantIdAsync`/`ArtistService.GetContactByTenantIdAsync`: `Nullable<T>` doesn't implicitly convert to `Option<T>` the way a reference-type `T?` does (confirmed by decompiling the actual published `Reunion 0.1.0-alpha.8` `Option<T>`'s operator table — only `op_Implicit(T)`/`Some<T>`/`None` exist, no `Nullable<T>` overload, and this is a hard C# limitation — an operator can't add an extra type-parameter constraint beyond what its containing generic type already declares). Fixed with the already-existing, already-documented `.ToOption()` extension at both call sites — the correct call per the `result-carriers` standard, not a workaround.
- **`Log.cs`** region renamed `VerificationAdminService` → `VerificationService` to match the merge (the `LoggerMessage` itself is unchanged, just relocated with its class).
- **`TECH_DEBT.md`**: the "enriches contact per row" entry's file reference updated post-merge; a new entry logs the `TenantType`-branch-in-`GetContactAsync` anti-pattern the `keyed-strategies` standard names directly, with a concrete design (`ITenantStrategy`/`ITenantStrategyFactory<TStrategy>` mirroring the existing `DealType`-keyed family, `ITenantContactResolver` as the first member) confirmed against the real `DealType` precedent rather than invented — and notes the `TenantContact`-per-module-duplication cleanup rides the same future PR, since the resolver needs a canonical return shape anyway.

No new findings beyond what's already fixed inline above (the `ToOption()` compile fix and the region rename) — this section is a record of a design change, not a defect list.

**Incremental security check (`4928e1647..392a5d782`)**: no findings. Confirmed every formerly-`[Admin]` action retains `[Admin]` directly on its method in the merged controller, `Get`/`SubmitDocuments` retain `[Authorize]`/`[HasPermission(TenantSettingsEdit)]`, no route collision or broadening, and the `TenantContact` struct change never lets a `default`/uninitialized value pass as present (`.ToOption()` maps absence to `None` exactly as before). Note: admin routes moved from `api/tenant/verification/*` to `api/verification/*` — a path change, not an authorization change; tests exercise the new paths.

## Incremental review — 2026-08-26 (merge-base)

Range `392a5d782..a7bbbc47a`: merged current `origin/main` in for base currency before enqueueing. Diff is `plans/launch/TENANT_VERIFICATION_PLAN.md`, `plans/launch/TENANT_VERIFICATION_PROGRESS.md`, `reviews/Docs-tv-p4-checkpoint.md`, `reviews/Feature-launch_tenant-verification.md` only — no code path touched. No findings; no security-sensitive path in range.

## Incremental review — 2026-08-26 (tech-debt log)

Range `a7bbbc47a..4d1111157`: `api/Concertable.B2B/TECH_DEBT.md` only, logging the `e2e-api-tests`
`IImageService`/`VenueService` startup flake investigated after two real merge_group failures (see PR
history: run `33014869553` for this PR, run `33008827828` for the unrelated PR #802 hitting the byte-
identical signature). No code path touched; no findings.

**Correction, same day:** the entry above was wrong. Static tracing found the real, deterministic cause
(below) and PR #802's identical failure was a merge-queue artifact (GitHub's merge queue tests stacked
diffs of PRs ahead in the queue; #802's first attempt ran while this PR was still ahead of it and
inherited its broken commit, then passed cleanly once this PR was dequeued) — not a second, coincidentally
identical bug. The wrong entry has been removed rather than left to mislead a future retry.

## Incremental review — 2026-08-26 (real root cause + fix)

Range `4d1111157..HEAD`: this PR's own `VerificationService` change — adding `IVenueModule`/`IArtistModule`
constructor dependencies for cross-module contact lookup — extends `ITenantModule`'s DI graph far enough
to reach `IImageService` (`ITenantModule` → `IVerificationService`/`VerificationService` → new
`IVenueModule`/`IArtistModule` → `VenueModule`/`ArtistModule` → `IVenueService`/`IArtistService` →
`VenueService`/`ArtistService` → `IImageService`). `Concert.Infrastructure/Data/Seeders/ConcertDevSeeder.cs`
already depends on `ITenantModule`, and `Concertable.B2B.E2ETests/AppFixture.cs` builds its own standalone
seed `Host` (~line 157) that never called `services.AddSharedImaging()` — unlike the real app host
(`B2BWebHostExtensions.cs`) and the Workers host, both of which do. Resolving `IDbInitializer` (which
depends on `IEnumerable<IDevSeeder>`, eagerly constructing every registered seeder including
`ConcertDevSeeder`) therefore threw deterministically once this PR's dependency addition landed — not an
environment race. Fixed by adding `services.AddSharedImaging()` to the seed host alongside the other
shared registrations it already carries (build verified locally). A structural note for the underlying
duplication-drift risk is logged in `api/Concertable.B2B/TECH_DEBT.md` (MED). No other findings in range.
