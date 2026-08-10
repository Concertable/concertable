# Big review — Refactor/B2BTypedResultMigration

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Plan anchored to commit:** `e229afb581c829279ca821b0a85729c4c4f0f441`  _(2026-08-10)_
**Reviewed up to commit:** `3d50d321c62fc7b9bc302aa9b2cbb93d77aa28b0`  _(2026-08-10)_
**Security-reviewed up to commit:** `3d50d321c62fc7b9bc302aa9b2cbb93d77aa28b0`  _(2026-08-10)_
Net diff reviewed: `1043a9178..e229afb58`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Summary

Big review complete: all 4 areas were reviewed over `1043a9178..e229afb58`. All 8 findings were
addressed: NAT1-NAT5, SEC2, and CV1 are fixed; SEC1 is consciously deferred to a human architecture
decision and tracked in the owning Concert `TECH_DEBT.md`. By review layer/lens, the findings comprise
5 native findings (2 test-coverage, 1 correctness, and 2 contract/documentation), 2 security findings,
and 1 C# convention/test-coverage finding; no microservice-isolation, module-boundary, or seeding
findings survived the confidence filter.

The incremental review of all 13 later commits is clean. SEC1 is the only outstanding finding; all
Cross-area notes are resolved.

## Coverage

- [x] Shared, B2B foundations, Deal — 64 files — `api/Concertable.Shared/**` `api/Concertable.B2B/Concertable.B2B.slnx` `api/Concertable.B2B/Directory.Packages.props` `api/Concertable.B2B/src/Modules/Conversations/**` `api/Concertable.B2B/src/Modules/Deal/**` `api/Concertable.B2B/src/Seed/**` `api/Concertable.B2B/tests/**` — reviewed 2026-08-10
- [x] Tenant, Artist, Venue, User — 91 files — `api/Concertable.B2B/src/Modules/Tenant/**` `api/Concertable.B2B/src/Modules/Artist/**` `api/Concertable.B2B/src/Modules/Venue/**` `api/Concertable.B2B/src/Modules/User/**` — reviewed 2026-08-10
- [x] Concert — 125 files — `api/Concertable.B2B/src/Modules/Concert/**` — reviewed 2026-08-10
- [x] Plan and progress — 2 files — `plans/typed-result/B2B_PLAN.md` `plans/typed-result/B2B_PROGRESS.md` — reviewed 2026-08-10

## Cross-area notes

- ~~Concert — `IDealModule` now exposes `Option` plus typed create/update failures; verify `DealAccessor`, `OpportunityService`, and `OpportunitySyncer` handle absence and failures without weakening transaction or invariant behavior.~~ — resolved: missing referenced deals remain invariant failures, opportunity mutations validate before entering the cross-module unit of work, and the syncer treats only impossible post-validation failures as invariants.
- ~~Tenant, Artist, Venue, User — `MessageService` now consumes option-valued organization and tenant lookups; verify the provider signatures and missing-value fallback behavior remain aligned.~~ — resolved: the Artist/Venue/Tenant providers return `Option`, and `MessageService` preserves the profile → tenant legal name → `UnknownOrg` fallback chain.
- ~~Concert — worker tests now require `ConcertCompletionRunner` to continue after expected finish refusals while propagating infrastructure exceptions; verify the changed runner implementation preserves both paths.~~ — resolved: typed finish refusals are logged and the loop continues, while unexpected infrastructure exceptions escape the runner unchanged.
- ~~Concert — `IArtistModule`, `IVenueModule`, `IUserModule`, and `ITenantModule` now expose `Option`/typed Results; verify `ApplicationValidator`, `ApplicationService`, `OpportunityService`, `SetupCheckoutStep`, `SelfBillingAgreementService`, and `InvoiceIssuer` preserve absence, domain-failure, and invariant behavior.~~ — resolved: each caller preserves its former absence response or invariant exception, including checkout's nullable manager email and invoice-time tenant/tax guarantees.

## Findings

## Shared, B2B foundations, Deal — reviewed 2026-08-10

No issues found in this area. Checked the native review catalog, security-sensitive controller and contract paths, correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths. The branch-wide Reunion carrier and terminal substitution was treated as the plan's explicit dependency-backed migration rather than a convention defect.

## Tenant, Artist, Venue, User — reviewed 2026-08-10

- [x] **NAT1 — LOW — test coverage** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Services/InvitationService.cs:91`
  Resolved: integration tests assert accepting an unknown invitation returns 404 without invitation or membership writes, and revoking an accepted invitation returns 409 without changing its status or membership.

- [x] **NAT2 — LOW — test coverage** — `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Services/UserService.cs:32`
  Resolved: an authenticated test principal absent from the User projection now receives the typed 401 ProblemDetails response, and the test verifies no user/location projection is written.

- [x] **NAT3 — LOW — native** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Contracts/ITenantModule.cs:20`
  Resolved: the summaries now distinguish `Option.None` for missing tax-compliance data, the typed `TenantNotFound` VAT failure, and the missing-compliance invariant exception.

## Concert — reviewed 2026-08-10

- [x] **NAT4 — HIGH — native/correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/Executors/EscrowExecutor.cs:40`
  Resolved: both callers now use typed transition effects, propagate refund failures without saving the lifecycle transition, and forward cancellation. Withdrawal returns the failure through its HTTP terminal; late-capture compensation throws at the worker terminal so the webhook is not acknowledged. Focused tests cover both failure paths.

- [-] **SEC1 — HIGH — security/correctness — DEFERRED** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Controllers/ApplicationController.cs:130`
  A request-independent token alone only narrows the failure window: B2B has no durable acceptance/cancellation intent, non-commission Payment operations are not booking-key idempotent, and a durable Payment event cannot reconstruct an application left `Applied` after a process failure. The correct fix is a separately planned B2B + Payment saga/package cut-over that persists the lifecycle intent and intermediate state before money moves, stages a transactional outbox command, makes Payment operations idempotent by booking, and reconciles pending work in a worker with cancellation-after-payment tests. Human decision required: authorize that cross-service plan or explicitly accept the unresolved financial/state inconsistency risk. Tracked in `api/Concertable.B2B/src/Modules/Concert/TECH_DEBT.md`.

- [x] **SEC2 — MEDIUM — security** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Controllers/DevController.cs:36`
  Resolved: the dev completion endpoint discards every successful `SettlementOutcome` and returns 204 No Content while retaining the typed ProblemDetails failure mapping.

- [x] **CV1 — LOW — C# conventions/test coverage** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/ErrorDefinitionContractTests.cs:13`
  Resolved: the definition inventory now hard-codes the code, message, and kind for every direct and wrapper case in `AcceptApplicationError`, `CancelApplicationError`, `CancelConcertError`, and `FinishConcertError`.

## Plan and progress — reviewed 2026-08-10

- [x] **NAT5 — MEDIUM — native/documentation** — `plans/typed-result/B2B_PROGRESS.md:10`
  Resolved: the ledger now records implementation `e229afb58`, review range and watermarks, every
  finding disposition and verification result, the mandatory incremental review, and both the
  publication and deferred SEC1 blockers without claiming terminal lifecycle state.

## Incremental review — 2026-08-10

> Range reviewed: `e229afb581c829279ca821b0a85729c4c4f0f441..3d50d321c62fc7b9bc302aa9b2cbb93d77aa28b0` (13 commits).

No new findings. Checked the native correctness and test-coverage catalog, security-sensitive
controller and Contracts changes, microservice isolation, module boundaries, seeding, C# conventions,
and documentation/ledger accuracy.
