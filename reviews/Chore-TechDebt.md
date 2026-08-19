# Code review — Chore/TechDebt

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `4d8b02f777a060357b7d2e33bad9cca8207addbd`  _(2026-08-19)_

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

## Review — 2026-08-19

> Range reviewed: `29e7a1ad1..3264275d8` (2 commits, PR #659) — split the B2B Tenant module's fat
> `ITenantRepository` (mixing `TenantEntity`/`TenantMembershipEntity`/`TenantInvitationEntity` behind
> one interface) into `ITenantRepository`/`IMembershipRepository`/`IInvitationRepository`, one per
> entity, rewiring `TenantService`/`MembershipService`/`InvitationService`/`TenantContext` and their
> tests; collapse two stage-then-save call sites in `InvitationService` into the base repository's
> `InsertAsync`; factor the tenant/membership join out of `MembershipRepository` into a
> `QueryableXMappers`-shaped extension (`QueryableMembershipMappers.ToUserMemberships`); correct
> `api/agents/CODE_PATTERNS.md` (the "one repository per entity" section still named
> `ITenantRepository` as an open violation) and extend `api/agents/CODE_CONVENTIONS.md`'s Mappers
> section with the `QueryableXMappers` shape. No security-sensitive paths touched (no Auth/Payment/
> `*.Contracts`/Controller/workflow changes) — Step 1d skipped, no security marker.

Two layers (native + Concertable lenses). **No findings.**

- **Layer 1 (native, medium), two passes** — first pass (commit 1 only) and a second pass over the
  full range (both commits) after the mapper follow-up landed: no correctness/reuse/simplification/
  efficiency/error-handling defects. Verified `ToUserMemberships` is byte-for-byte equivalent to the
  private `Project` method it replaced (same join keys, same projection shape, same two call sites);
  the `InsertAsync` collapse in `InvitationService` preserves atomicity (all three repositories share
  one scoped `TenantDbContext`, so a `SaveChangesAsync` from any one flushes tracked mutations staged
  through the others — e.g. `AcceptInvitationAsync`'s membership insert still lands in the same save
  as the invitation's `Accept()` state change); DI registrations match every constructor; no stray
  references to removed members (`AddMembership`/`RemoveMembership`/`AddInvitation`/
  `RemoveInvitation`/`GetInvitationByIdAsync`) remain anywhere in the codebase; unit test mock
  rewires target the correct narrower interface per call site.
- **Layer 2 (Concertable lenses):** no findings.
  - *Correctness* — verified locally beyond the diff review: `dotnet build Concertable.B2B.slnx` 0
    errors; `Concertable.B2B.Tenant.UnitTests` 131/131; `Concertable.B2B.Tenant.IntegrationTests`
    58/58 (real SQL Server — exercises `MembershipService`, `InvitationService`, `TenantContext`, and
    `TenantService.DeleteAsync`'s new three-repository cascade via `DELETE /api/organization` in
    `MemberManagementTests.cs`/`InvitationTests.cs`).
  - *Microservice isolation* — n/a, entirely internal to B2B's Tenant module.
  - *Module boundaries* (`api/agents/CONVENTIONS.md`) — no module-facade change; new interfaces/impls
    correctly `internal`; visibility cascade unchanged.
  - *Conventions* (`api/agents/CODE_CONVENTIONS.md`) — explicit ctors + `this.field` (no primary
    constructors, no `_` prefix); `CancellationToken ct = default` on every new async method; new
    extension uses a C# 14 `extension()` block, not a legacy `this`-parameter method; `InsertAsync`
    used for the sole-staged-write case per the repository convention.
  - *Patterns* (`api/agents/CODE_PATTERNS.md`) — this diff **is** the reference example the "one
    repository per entity" section already pointed at (`ITenantRepository` was the named
    pre-existing violation); the doc is corrected in this same range.
  - *Test coverage* — the new/changed wiring (three-repository `DeleteAsync`, `InsertAsync` inserts,
    the mapper extraction) is exercised by the integration suite above; no added/altered behaviour
    lacks a covering test.

## Incremental review — 2026-08-19 (merge from origin/main)

Range `3264275d8..91c4a42df` — `main` moved (PR #651, `Refactor/b2b_admin-module`, extracting
`Concertable.B2B.Admin` out of `Concertable.B2B.User` + an `Admin`→`Privileged` repository/DbContext
rename) while this PR sat in the merge queue, DIRTY'd it, and disabled auto-merge. Updated the branch
with `git merge origin/main`. **Not a re-review of PR #651** — it is already reviewed in its own
`reviews/Refactor-b2b_admin-module.md`, already on `main`, and this merge doesn't touch any of it;
scoped to what this merge commit itself introduces.

One real conflict: `api/Concertable.B2B/src/Modules/Tenant/TECH_DEBT.md` — PR #651 edited the prose of
the `ITenantRepository` entry this PR's first commit already deleted (renaming its `AdminRepository`
reference to `Admin module's AdminRepository`). Resolved by keeping this branch's side (`No outstanding
debt.`) — verified byte-identical, pre- and post-merge, via `git show 3264275d8:…TECH_DEBT.md` vs
`git show 91c4a42df:…TECH_DEBT.md`: a pure "ours" resolution, zero new logic, nothing to review.
Every other changed path auto-merged cleanly (main-only or branch-only edits, no overlap).

**No findings.** Re-verified after the merge: `dotnet build api/Concertable.B2B/Concertable.B2B.slnx`
0 errors; `Concertable.B2B.Tenant.UnitTests` 131/131; `Concertable.B2B.Tenant.IntegrationTests` 58/58.

## Incremental review — 2026-08-19 (second merge from origin/main)

Range `91c4a42df..4d8b02f77` — `main` moved again (~122 commits: PR #637
`Docs/GuidanceDocsRestructure`, deleting `api/agents/` and `app/agents/` entirely in favour of
load-on-demand skills backed by an external `Concertable/agent-standards` repo, plus its own
platform-sync `chore/platform-sync-0.1.0-alpha.0.1073`) while this PR sat queued a second time,
DIRTY'ing it again. **Not a re-review of that restructure** — already reviewed in its own
`reviews/Docs-GuidanceDocsRestructure.md`, already on `main`.

Two conflicts:

- `api/Concertable.B2B/src/Modules/Tenant/TECH_DEBT.md` — same shape as the first merge (PR #651 had
  already updated the same deleted-entry's prose; this time the restructure updated it again to point
  at "the `persistence` skill" instead of the deleted `CODE_PATTERNS.md`). Resolved the same way: kept
  this branch's side (`No outstanding debt.`) — verified byte-identical pre/post-merge, zero new logic.
- `api/agents/CODE_CONVENTIONS.md` / `api/agents/CODE_PATTERNS.md` — modify/delete: this PR's earlier
  commits edited both (the `QueryableXMappers` clarification, correcting the stale `ITenantRepository`
  violation callout); the restructure deletes both files outright, moving their content to the external
  `agent-standards` repo. **Accepted the deletion** — resurrecting a deliberately, already-reviewed
  upstream-removed file to keep two local edits would be wrong. Those two doc corrections have no
  reachable home from this checkout (no local `agent-standards` clone) and are dropped; flagged to
  Tommy in the final chat summary rather than silently lost.

**No findings** in what this merge commit itself introduces (both conflict resolutions verified
pure/mechanical, no new logic). Re-verified after the merge: `dotnet build
api/Concertable.B2B/Concertable.B2B.slnx` 0 errors (now against platform pin `0.1.0-alpha.0.1073`);
`Concertable.B2B.Tenant.UnitTests` 131/131; `Concertable.B2B.Tenant.IntegrationTests` 58/58.
