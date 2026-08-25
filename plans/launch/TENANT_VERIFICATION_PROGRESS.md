# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: none — Phase 2's worktree closed after merge; Phase 3 opens a fresh one
- Branch: next proposed `Feature/launch_tenant-verification` (Phase 2's branch merged and deleted)
- PR: [#784](https://github.com/Concertable/concertable/pull/784) — **MERGED**
  (`1867f0a7200b245ebd7b9b66662a144c606a028f`), `full-e2e` label (new HTTP endpoint — a positive trigger).
  Its causally-linked publish opened sync PR #787 (`0.1.0-alpha.0.1188`), which was itself superseded
  (closed, not merged) by PR #789 (`0.1.0-alpha.0.1189`) — a later, unrelated publish from this session's
  own docs PR #788 — before #787 could land; ownership transferred to that producer, matching the
  "only one platform-sync PR is ever live" pattern already seen in Phase 1.
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-25, Phase 2 fully terminal (merged, reviewed clean, sync resolved by
  supersession)

## Current state

Phase 1 and Phase 2 are both merged to `main` and fully terminal. No implementation started yet on Phase 3
(cross-module `IsVerifiedAsync` gate + enforcement at opportunity publication and settlement).

## Next Steps

1. `/open-worktree Feature/launch_tenant-verification` (branch off fetched `origin/main`) and start Phase 3
   of `TENANT_VERIFICATION_PLAN.md` (cross-module gate + enforcement):
   - Extend `ITenantModule` (Tenant.Contracts) with `Task<bool> IsVerifiedAsync(Guid tenantId,
     CancellationToken ct = default)`; implement in `TenantModule`/`TenantService` as
     `verification?.Status == Approved`, `false` when no row exists — fail-closed, matching
     `IsTaxComplianceCompleteAsync`'s posture.
   - **Opportunity publication gate**: inject `ITenantModule` into `OpportunityService`; in `CreateAsync`
     and `CreateMultipleAsync`, check `IsVerifiedAsync(tenantContext.GetTenantId())` before creating the
     `OpportunityEntity`. Add `OpportunityMutationError.VenueNotVerified` to the `[Union]` (Dunet) with
     `ErrorDefinition.Forbidden<VenueNotVerified>(...)` and error code `opportunity.venue_not_verified`,
     following `VenueNotFound`'s shape exactly.
   - **Settlement gate**: in `FinishExecutor.FinishAsync`, immediately after the existing tax-compliance
     pair check, add the same pattern for verification — `IsVerifiedAsync(supplierTenantId)` and
     `IsVerifiedAsync(customerTenantId)` — returning a new `SettlementOutcome.DeferredPendingVerification`
     on failure, with a matching `logger.SettlementDeferredPendingVerification(...)` `LoggerMessage`. No
     sweep changes needed: `ConcertCompletionRunner` already retries every non-`Settled` outcome hourly.
   - Integration tests: `TenantVerificationGateApiTests` (settlement defers/settles, mirroring
     `SelfBillingAgreementGateApiTests`/`ConcertPayoutComplianceGateApiTests`) and an opportunity-creation
     test proving an unverified tenant's `POST` is rejected.
   - Build + focused tests; commit; review (check for a stale security marker if the diff touches a
     `Controller[A-Za-z]*\.cs$` path or `.Contracts` — it will, here); push to a new PR.
2. Update this ledger **in the normal checkout** — never inside the delivery worktree.

## Completed work

- **Phase 1 — Domain** (PR #772, **merged** `5222bce51`, reviewed clean): `TenantVerificationEntity`
  (`Pending`/`Approved`/`Rejected`, transitions validated through `Concertable.Kernel.StateMachine<TState,
  TTrigger>` — the first real consumer of that shared abstraction in this codebase) and
  `VerificationDocumentEntity` (append-only evidence, `Licence`/`ProofOfAddress`/`CompanyRegistration`).
  EF configurations composed into `TenantDbContext` (confirmed no new tenancy stance needed —
  `TenantDbContext` is already unscoped). Migration re-scaffolded via `./initial-migrations.ps1`.
  19 unit tests. Skip-e2e tier (domain-only, no HTTP/UI/published-contract surface). Its version-sync
  (#778) was superseded by an unrelated later publish — Phase 1's own delivery obligation ended at its own
  successful publish.
- **Phase 2 — Tenant-facing submission API** (PR #784, **merged** `1867f0a72`, reviewed clean):
  `IVerificationService`/`VerificationService` (`GetStatusAsync`, `SubmitAsync` — create-or-resubmit,
  race-safe insert via `TryInsertAsync`), `VerificationController` (`api/organization/verification`: `GET`
  no special permission, `POST documents` gated on `TenantSettingsEdit` + `RateLimitPolicies.Upload`),
  evidence upload via `IBlobStorageService` with content-type + size + magic-byte validation
  (PDF/JPEG/PNG). `EvidenceUpload` keeps the service free of `IFormFile` so it stays callable outside an
  HTTP request; `VerificationDocumentEntity.Create(tenantId, documentType, fileExtension, uploadedAt)`
  derives its own blob name. 174 unit tests (service + validators + domain). Integration tests cover the
  submit/resubmit round-trip, the pending/approved eligibility gate, content-type/byte-mismatch rejection,
  and the `TenantSettingsEdit` permission boundary. `full-e2e` tier (new HTTP endpoint). Its version-sync
  (#787) was superseded by an unrelated later publish (#789, from this session's own docs PR #788) — Phase
  2's own delivery obligation ended at its own successful publish.
  Separately logged as tech debt (not fixed in this phase — pre-existing, unrelated to verification):
  `api/Concertable.B2B/src/Modules/Concert/TECH_DEBT.md` and `.../Artist/TECH_DEBT.md` — `Genres` should be
  set-shaped to prevent duplicate tags, pending EF Core `PrimitiveCollection`/`HashSet<T>` verification.

## Verification

- `dotnet test Concertable.B2B.Tenant.UnitTests` (2026-08-25, commit `ca7b8ba0d`): 174 passed, 0 failed.
- `dotnet build Concertable.B2B.Tenant.Api.csproj` and the full `Concertable.B2B.Tenant.IntegrationTests`
  graph (2026-08-25, commit `ca7b8ba0d`): 0 errors — pulls in the whole `Concertable.B2B.Web` service.
- PR #784's own CI (build, every carve, every unit/integration/architecture-tests project, `full-e2e`):
  all green before enqueueing; merge-queue `merge_group` run also green.

## Reviews

`reviews/Feature-launch_tenant-verification.md` — spent, deleted with this phase's close-out (per the
review-lifecycle standard: a review file is a work order for its branch, never an archive, and dies with
the thing it gates). Final state before deletion: reviewed up to `ecc99648b` (marker) / security-reviewed
up to `94720507` (re-run after later commits touched `VerificationController.cs` again). 10 findings across
two rounds (native+security tooling, then a manual user re-review), all fixed or `[wontfix]`-justified —
see git history (`reviews/Feature-launch_tenant-verification.md` at commit `ecc99648b`) for the full list.
The manual re-review's most load-bearing findings, kept here since the review file itself is gone:
`IReadOnlyList<T>` over `List<T>` on every collection return (`result-carriers`), C# 14 `extension()`
blocks over legacy `this`-parameter extension methods (`csharp-style`), a repository never keeps a redundant
concrete-context field when the inherited `Context.Query<T>()` already does the job (`persistence`), a
domain entity deriving its own naming convention rather than infrastructure building it ad hoc (DDD), a
service never taking an ASP.NET Core type (`IFormFile`) directly so it stays callable outside HTTP, and a
single-query method named for what it returns, not for the scope it already runs under by default
(`csharp-naming`).

## Decisions, discoveries, blockers, and deviations

- Verification is modeled on `Tenant` (new `TenantVerificationEntity`), not duplicated onto
  `Venue`/`Artist` — see plan §1.1 for the full rationale (mirrors `TenantEntity.TaxCompliance` +
  `ITenantModule.IsTaxComplianceCompleteAsync`, which `FinishExecutor` already consumes as a fail-closed
  gate). This is a load-bearing decision for every later phase — do not re-derive or re-litigate it.
- Only two enforcement points, exactly as scoped: opportunity publication and settlement. Artist
  Application/Apply is deliberately not gated — see plan §1.4.
- Phase 6 (removing `VenueEntity.Approved` and its admin surface) must not start before Phase 3's new
  gate is merged and green — the old signal cannot be dropped before the new one is proven.
- `TenantVerificationEntity` raises `TenantVerificationChangedDomainEvent` on every transition (Submit/
  Resubmit/Approve/Reject) with no handler yet — legal per the domain-events standard ("zero handlers for
  an event is valid"). No consumer is scoped in this plan; a future phase (or a separate one) may add a
  pre-commit handler if a real need arises (e.g. activity-feed integration). Do not add one speculatively.
- A cross-repo standards gap surfaced during Phase 2's review: `tomjseery/dotagents` PR #12 (framework
  types off service signatures; name a controller's lone query action `Get`, disambiguate only past one;
  a single-query service method named for what it returns) is open, not yet merged — needs manual merge
  (a Concertable merge-gate hook intercepts `gh pr merge` for unrelated repos in this session). A larger,
  deliberately-deferred item — an ArchUnit-based ban on `List<T>`/`Dictionary<K,V>`/`HashSet<T>` as a
  declared return type, covering `internal`/`private` members Meziantou's `MA0016` cannot reach — is
  written up in `plans/COLLECTION_ABSTRACTION_ARCHITECTURE_GATE.md`, merged with PR #784; not yet started.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
