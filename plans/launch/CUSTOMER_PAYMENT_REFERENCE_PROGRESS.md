# Customer payment-reference migration progress

- Plan: `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/customer-payment-reference`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-launch_customer-payment-reference`
- Branch: `Feature/launch_customer-payment-reference`
- PR: not opened
- Dependency/package gates: met — PRs #933 and #937 are merged; `Concertable.Payment.*`
  `0.1.0-alpha.0.1322` is published and pinned independently while the platform remains at
  `0.1.0-alpha.0.1329`.
- Last reconciled: 2026-09-06 against `origin/main` at `ea33c48e`

## Current state

Implementation and focused verification are complete. Customer now creates Payment v1 sessions and
correlates purchase outcomes through the whole `PaymentOperationReference`; the old provider identifier,
Payment metadata, and legacy Customer payment-client surfaces are gone. Review and PR delivery remain.

## Next Steps

1. Run the review workflow, address findings, commit, push, and open the PR against `main`.

## Completed work

- Authored the plan and ledger (2026-09-04).
- Cleared the published-package gate and opened the delivery worktree from fresh `origin/main`.
- Pinned only `Concertable.Payment.*` to `0.1.0-alpha.0.1322`, including the local-platform override.
- Replaced the retired Customer payment client and raw payment-method request field with Payment v1
  session operations and whole-reference correlation.
- Added the Customer-owned checkout, payment-success, and payment-failure HTTP/notification shapes.
- Guarded and decoded Customer-minted ticket references before inbox processing; foreign and malformed
  references are skipped without recording an inbox row.
- Removed the unused Ticket dependency on `Concertable.B2B.User.Contracts`; Ticket now resolves receipt
  addresses through the Customer User module.
- Removed the mocked `TicketService` unit suite. Deterministic reference encoding is unit-tested; purchase
  and outcome orchestration are covered through HTTP and integration-event tests.
- Migrated customer shared, web, and mobile checkout correlation away from provider-derived ids.

## Verification

- `dotnet test` with `--no-build`: all seven Customer module integration suites passed, 71 tests total
  (Artist 2, Concert 11, Preference 7, Review 14, Ticket 29, User 6, Venue 2).
- Ticket unit suite passed, 32 tests; the focused Ticket integration suite passed again after the final
  inbox-ordering and reference-validation changes, 29 tests.
- Shared foundation tests passed, 26 tests; Customer shared tests passed, 3 tests. Customer shared, web
  shared, customer web, and mobile builds passed; mobile customer TypeScript checking passed.
- Case-insensitive forbidden-identifier sweep passed with zero matches in `api/Concertable.Customer`,
  `app/customer`, `app/web/customer`, and `app/mobile/customer`.
- The uniform local-platform Customer solution build compiled every Customer project and then failed only
  in the known pre-#633 B2B consumers: `ArtistDashboardService`, `VenueDashboardService`, `Checkout`, and
  `FinishConcertError` (six missing retired-contract symbols).
- The exact published-package Customer solution build additionally exposes an upstream main-branch source/
  package closure mismatch in `Concertable.Payment.Hosting/PaymentTopology.cs`: source Payment Hosting is
  compiled against published AppHost Shared 1329, whose fluent chain returns `AsbServiceTopology`. The
  uniform local-platform seam compiles Payment Hosting successfully. Customer and Payment sources were not
  changed to conceal either external failure.

## Reviews

- No review yet; the branch does not exist.

## Decisions, discoveries, blockers, and deviations

- On-session purchase collects no mandate; saved-card selection, if ever built, is Payment's
  surface per `PAYMENT_BOUNDARY_DECISION.md` §1 — Customer proxies at most an opaque token and
  persists nothing.
- Owner decision, 2026-09-04: PR #933 includes the legacy cull and breaking vocabulary pass, so
  Customer migrates directly from the raw-identifier client to the final reference surface after
  that package publishes; there is no later producer-cull dependency.
- Final producer contract, 2026-09-05: `PaymentSessionOperationRequest` carries one validated
  `PaymentOperationReference`; public payment outcomes and success/failure events expose no provider
  identifier. Customer correlates every result and event through its opaque reference or the
  Payment-owned operation id.
- Customer ticket references use operation type `ticket-purchase`; the opaque client reference encodes the
  buyer, concert, and quantity required to complete the purchase. Decoding occurs only after the exact
  operation-type guard and returns `false` for foreign or malformed references.
- Receipt delivery resolves the current email address from Customer's own User module when the success
  event arrives. Customer no longer trusts mutable user data carried through Payment metadata and no longer
  depends on B2B contracts.
- The fixed Stripe test payment method survives only inside the Customer E2E fixture's direct provider
  confirmation helper. It does not cross the Customer HTTP/event boundary or enter Customer state, DTOs,
  requests, or frontend code.
