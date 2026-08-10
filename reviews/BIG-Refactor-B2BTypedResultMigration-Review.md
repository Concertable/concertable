# Big review — Refactor/B2BTypedResultMigration

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Plan anchored to commit:** `e229afb581c829279ca821b0a85729c4c4f0f441`  _(2026-08-10)_
**Reviewed up to commit:** `e229afb581c829279ca821b0a85729c4c4f0f441`  _(2026-08-10)_
**Security-reviewed up to commit:** `e229afb581c829279ca821b0a85729c4c4f0f441`  _(2026-08-10)_
Net diff reviewed: `1043a9178..e229afb58`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Summary

Big review complete: all 4 areas were reviewed over `1043a9178..e229afb58`. There are 8 open findings: 2 HIGH, 2 MEDIUM, and 4 LOW. By review layer/lens, these comprise 5 native findings (2 test-coverage, 1 correctness, and 2 contract/documentation), 2 security findings, and 1 C# convention/test-coverage finding; no microservice-isolation, module-boundary, or seeding findings survived the confidence filter.

The highest-priority fixes are NAT4 (propagate application-cancellation refund failures), SEC1 (make financial transitions durable across request cancellation), SEC2 (avoid exposing settlement-state detail from the dev endpoint), and NAT5 (make the plan ledger's operational state and next action current). All Cross-area notes are resolved.

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

- [ ] **NAT4 — HIGH — native/correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/Executors/EscrowExecutor.cs:40`
  `IApplicationCancelStep.ExecuteAsync` now returns a typed refund failure, but both this late-capture compensation path and `WithdrawExecutor.cs:22` await it as a plain `Task` and discard the result; a failed refund can therefore acknowledge the webhook or persist the Withdraw transition while money remains captured. Propagate the failure so the lifecycle transition is not saved, pass the cancellation token through, and add failure-path tests for both callers.

- [ ] **SEC1 — HIGH — security/correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Controllers/ApplicationController.cs:130`
  The migrated accept and application-cancel endpoints now carry `RequestAborted` through an irreversible capture/deposit/refund and then into the later lifecycle `SaveChangesAsync`; a disconnect after Payment succeeds can cancel the B2B state save and leave money moved against an unaccepted or uncancelled application. Establish a durable idempotent operation/reconciliation boundary and use a server-owned token once the financial operation begins, with cancellation-after-payment tests.

- [ ] **SEC2 — MEDIUM — security** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Controllers/DevController.cs:36`
  The changed success terminal serializes `SettlementOutcome`, allowing any authenticated user to probe arbitrary concert IDs and distinguish settled concerts from parties blocked on tax compliance or a self-billing agreement; preserve the endpoint's prior empty success response while still mapping typed failures.

- [ ] **CV1 — LOW — C# conventions/test coverage** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/ErrorDefinitionContractTests.cs:13`
  The explicit definition inventory omits every case from `AcceptApplicationError`, `CancelApplicationError`, `CancelConcertError`, and `FinishConcertError`, despite `CODE_CONVENTIONS.md` requiring an exact hard-coded code, message, and kind test for every error-union case. Add all omitted direct and wrapper cases to the contract theories.

## Plan and progress — reviewed 2026-08-10

- [ ] **NAT5 — MEDIUM — native/documentation** — `plans/typed-result/B2B_PROGRESS.md:10`
  The changed current summary still says the implementation head is pending, and `## Next Steps` tells the next agent to commit checkpoints 6-7 and start the full review, but HEAD `e229afb58` is already that implementation commit and this staged review has completed. Reconcile the summary and `## Next Steps`, and append the review range, watermark, and every finding disposition before handing off the Reunion wait, as required by `plans/agents/PLAN.md`.
