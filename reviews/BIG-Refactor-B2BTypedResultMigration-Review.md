# Big review — Refactor/B2BTypedResultMigration

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Plan anchored to commit:** `e229afb581c829279ca821b0a85729c4c4f0f441`  _(2026-08-10)_
**Reviewed up to commit:** `f1468d83626f2e32e73bb4b76e19629ea20fa13c`  _(2026-08-14)_
**Security-reviewed up to commit:** `f1468d83626f2e32e73bb4b76e19629ea20fa13c`  _(2026-08-14)_
Net diff reviewed: `1043a9178..e229afb58`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Summary

Big review complete: all 4 areas were reviewed over `1043a9178..e229afb58`. All findings are
addressed: NAT1-NAT6, SEC1-SEC2, and CV1 are fixed. By review layer/lens, the findings comprise
5 native findings (2 test-coverage, 1 correctness, and 2 contract/documentation), 2 security findings,
and 1 C# convention/test-coverage finding; no microservice-isolation, module-boundary, or seeding
findings survived the confidence filter.

The earlier incremental review of 13 commits was clean. The 2026-08-12 incremental review found NAT6;
it is fixed in `eb8463469` and its follow-up is clean. The 2026-08-14 incremental review verified the
durable B2B/Payment saga cut-over, closed SEC1, and found no new issues. All findings and cross-area
notes are resolved. The follow-up through `35903d6e0` reviewed the branch-owned HTTP-terminal changes,
the Deal polymorphic-wire correction, and their tests; current-main merge content was inherited from
its landed PR. No new correctness, security, isolation, ownership, or coverage finding survived.

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

- [x] **SEC1 — HIGH — security/correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Controllers/ApplicationController.cs:130`
  Resolved: B2B persists stable acceptance/cancellation operation IDs, stages Payment commands in the
  same transaction through its outbox, and consumes typed Payment outcomes through its inbox. Payment
  owns idempotent operation execution and recovery; B2B remains Contracts-only and exposes the durable
  operation status without adding a cross-service runtime reference.

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

## Incremental review - 2026-08-12

> Range reviewed: `3d50d321c62fc7b9bc302aa9b2cbb93d77aa28b0..eb84634699fa643a072342cd196b9767a6694619` (332 commits).

- [x] **NAT6 - HIGH - native/architecture** - `api/Concertable.Shared/src/Concertable.Kernel/Errors/ValidationErrors.cs:1`
  Resolved in `eb8463469`: restored the shared compatibility carrier and its tests exactly from
  current main. Their eventual deletion remains owned by the downstream Shared-contraction plan.

No other issues found. Checked native correctness, security-sensitive controller changes,
microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review - 2026-08-14

> Range reviewed: `eb84634699fa643a072342cd196b9767a6694619..54b419b0153fe06bc2786db061a48bbbbecef41c`.

No new findings. Reviewed the B2B-owned net changes for the durable financial-operation saga,
outbound-only Messaging registration, Payment Contracts/Client boundary, inbox/outbox atomicity,
state-machine retry paths, typed HTTP mappings, implicit Reunion conversions, invariant exceptions,
EF migration, package ownership, and focused unit/integration/architecture coverage. SEC1 is closed.
The follow-up Option construction was superseded by `219b34b1e`, which restores target-typed `null`
for absence and direct reference payload returns.

## Incremental review - 2026-08-14

> Range reviewed: `54b419b0153fe06bc2786db061a48bbbbecef41c..219b34b1ef6152353212138e01f73a87120720ef` (3 commits).

No new findings. Checked native correctness, implicit Reunion construction, nullable value-type
conversion boundaries, microservice isolation, module boundaries, seeding, C# conventions, test
coverage, and documentation/ledger accuracy. Explicit `Result` factories remain only at generic
composition inference sites, `ToOption()` remains only for composition and nullable value types,
and interface-typed successes retain named cases because C# forbids their raw user-defined
conversion.

## Incremental review - 2026-08-14

> Range reviewed: `219b34b1ef6152353212138e01f73a87120720ef..506addfee9891d33a8e3a06c0297bb95d931fa1b` (66 commits).

No new findings. Reviewed the current-main reconciliation and its Artist/Venue dashboard conflict
resolutions through native correctness, Payment security, package ownership, microservice isolation,
module boundaries, seeding, C# conventions, and test coverage. The merge preserves main's published
Payment reporting client and MTD values while the typed identity Options short-circuit absence before
Concert or Payment calls. Upstream main changes outside the integration surface were already landed
and introduced no branch-specific interaction defect.

## Incremental review - 2026-08-14

> Range reviewed: `506addfee9891d33a8e3a06c0297bb95d931fa1b..85e84c7dcc9c6e81c0f34e627254b43cec6e9553` (4 commits).

No new findings. Reviewed the AppHost topology tech-debt entry and the standard HTTP-terminal cleanup
through native correctness, typed-result transport semantics, authorization preservation,
microservice isolation, module boundaries, C# conventions, and HTTP contract coverage. Created
responses retain their values and exact Location headers through `ToCreatedOrProblem`; Deal lookup
retains `200 OK` through `ToOkOrProblem`; the remaining custom callbacks are limited to file downloads
and bodyless `201 Created` responses without a dedicated Reunion unit-result terminal.

## Incremental review - 2026-08-14

> Range reviewed: `35903d6e02d750761f4d41a02d906a096c8f0fd2..9380696c208224e59ab77d09d8a72d00853e852f` (36 commits).

No new findings. Reviewed the module-facade delegation and current-main reconciliation through native
correctness, Payment security, microservice isolation, module boundaries, seeding, C# conventions,
and test coverage. The moved application-service operations preserve the former repository queries,
mapping, Options, and booleans; facade tests and the architecture guard pin the new wiring. Upstream
Payment reporting and shared integration-harness changes were already landed and introduce no
branch-specific interaction defect.

## Incremental review - 2026-08-14

> Range reviewed: `9380696c208224e59ab77d09d8a72d00853e852f..0f331b6a37cd7ffa4a746ce5e2dd96cf636109aa` (5 commits).

No new findings. Reviewed the verified push checkpoint and platform `0.1.0-alpha.0.988` reconciliation.
The source delta is limited to the five service package pins from terminal-green platform-sync PR
#566; restored B2B and Customer deployable closures, focused tests, architecture tests, and B2B User
integration tests remain green.

## Incremental review - 2026-08-14

> Range reviewed: `0f331b6a37cd7ffa4a746ce5e2dd96cf636109aa..f1468d83626f2e32e73bb4b76e19629ea20fa13c` (2 commits).

No new findings. The range contains only the reviewed platform-validation record and its plan-ledger
transport checkpoint; no runtime, package, contract, configuration, or test code changed.
