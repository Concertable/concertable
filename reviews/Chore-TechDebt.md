# Code review — Chore/TechDebt

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `67d863427ec904c596a858deeffa1e6a8d0bf7ed`  _(2026-08-13)_

**Security-reviewed up to commit:** `67d863427ec904c596a858deeffa1e6a8d0bf7ed`  _(2026-08-13)_

> Range reviewed: `1c88858f9..f3ad7c718` — move Payment E2E Stripe adapter out of production hosts (relocate `Payment.Seed` → `E2ETests.Stripe`, extract Web/Workers bootstrap, add E2E host entry points, harness `LaunchAs` swap). Two layers (native + Concertable lenses) + security layer (Payment paths).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MAJOR — correctness** — `.github/workflows/test.yml:301` — the `carve-payment` gate's `dotnet sln add` still listed the now-deleted `src/Seed/Concertable.Payment.Seed/…csproj`, which would hard-fail the carve job (project-not-found) in the merge queue. **Fixed** in `e0e57c3c5`: removed the line; `Concertable.Payment.Contracts.csproj` is now the last arg. Thematically correct — the E2E adapter is no longer part of Payment's deployable closure.
- [x] **NAT2 — LOW — stale reference** — three comments/docs still named the renamed E2E types after the rename. **Fixed** in `e0e57c3c5`: `FakeStripeAccountClient.cs:9` and `IDbSeeder.cs:18` now say "the E2E Stripe adapter"; `.agents/skills/e2e-api-debug/SKILL.md:58` drops `StripeE2EAccountResolver` from the shared-infra list (it's Payment-owned, never lived in the shared harness).
- [x] **CI1 — MAJOR — arch-test regression** (caught by the CI run, not the diff review — the test lives in `Shared.Api.UnitTests`, outside the diff) — `TypedResultArchitectureTests.ServiceHosts_RegisterProblemDetailsBeforeMvc` scanned only `Program.cs` in `.Web` dirs; extracting `Payment.Web`'s composition into `HostExtensions.cs` (and adding the `.E2ETests.Web` host dir) tripped it. **Fixed** in `f3ad7c718`: scan `Program.cs` + sibling `HostExtensions.cs`, and scope to production hosts via `IsProductionSource` (excludes the `tests/` E2E host). Invariant intact — `AddProblemDetails` still precedes the controller registration in `AddWebHost`; verified locally (`Shared.Api.UnitTests` 24/24 green).

### Clean layers

- **Layer 1 (native, medium):** confirmed the `HostExtensions` extraction in `Payment.Web`/`Workers` is byte-for-byte behaviour-preserving (same registrations, order, migration gating; only the intentional E2E branch dropped); the moved adapter files are identical apart from namespace/type rename; the `LaunchAs` annotation swap is correct. Only NAT1/NAT2 above.
- **Layer 2 (Concertable lenses):** no findings. Microservice isolation intact — the harness referencing the Payment E2E hosts is allowed (Payment is an adapter; the harness already pins Payment); no data-service→data-service reference introduced. Conventions (field naming, source-gen logging) preserved; the mislabeled "Seed" project removed; adapter tests moved intact; extracted bootstrap stays covered by `Payment.IntegrationTests`.
- **Layer 1d (security, Payment paths):** no findings — net security improvement. Auth/JWT config (`MapInboundClaims`, `Authority`, `ClockSkew`, `ValidateIssuer`, `ValidAudiences`, `ServiceToken` scope), CORS, Kestrel, middleware order, and the migration gate are byte-for-byte preserved in `HostExtensions`. No new secrets (the `acct_…` IDs are pre-existing test-mode fixtures moved verbatim). Production `src/` no longer compiles/ships E2E code; `InternalsVisibleTo` tightened (friend access moved to a test-only assembly).

## Incremental review — 2026-08-13

Range `f3ad7c718..67d863427` (fix `cd429ec9f` + merge `67d863427`). Authored delta reviewed in isolation from origin/main's already-reviewed merge content; scoped to the host files this branch owns. **No findings.**

- **FIX (correctness) — E2E host content root** — `Concertable.Payment.E2ETests.Web/Program.cs`, `...Workers/Program.cs`: `ContentRootPath = AppContext.BaseDirectory`. Fixes the startup DI-validation crash (`Stripe.SetupIntentService` unresolved) that timed out the merge-queue `e2e-api-tests` on all 10 `B2B.E2ETests.Payments.*`: the `<Content Link>` appsettings land in the output dir but Aspire's content root is the project dir, so `appsettings.E2E.json` (`UseRealStripe=true`) never loaded → Stripe SDK singletons unregistered → the swapped adapter failed `ValidateOnBuild`. Verified end to end (healthy Docker): scoped `ConcertDraftTests.ShouldCreateDraft_WhenDoorSplitApplicationAccepted` now passes (health green, accept→draft→settlement→payout, real Stripe). Canonical config precedence preserved; the linked files remain the single source (no duplication).
- **Merge conflict resolution** — `Payment.Web/HostExtensions.cs`: ported origin/main's escrow ASB registrations (7 `Publishes` + 3 `HandleCommand` + `using Concertable.Payment.Contracts`) from the production `Program.cs` block this branch extracted into `AddWebHost`; mirrors origin/main's own reviewed registration. origin/main's new `IStripeAccountClient.CreateBoundCommissionHoldSessionAsync` carried onto the relocated E2E adapter via git rename detection (override ignores `commissionBindingId` — correct E2E stub). `Payment.Web` + both E2E hosts build 0 errors.
- Lenses: correctness ✓ (verified E2E), microservice isolation/boundaries ✓ (test-only host + Payment-owned registration, no new cross-service ref), conventions ✓, security ✓ (no auth/secret/CORS/middleware/migration-gate change; content root = the host's own output dir), coverage ✓ (Payment.IntegrationTests + the API E2E run). The single 2-line comment is a footgun/invariant warning at the exact site a revert to `CreateBuilder(args)` would silently re-break config loading.
