# Code review — Feature/payments_provider-contract-baseline

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `01171e1b21b8a08a273eafb3d3f99859081756e2`  _(2026-08-16)_

**Security-reviewed up to commit:** `01171e1b21b8a08a273eafb3d3f99859081756e2`  _(2026-08-16)_

> Range reviewed: `e861f3642..85d85aab1` (22 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native/test coverage** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/ProviderContract/StripeOperationTransitionSpecificationTests.cs:14`
  The pinned-version test only compares one hard-coded `47.3.0` value with another, so changing the installed `Stripe.net` package in `Directory.Packages.props` leaves the executable status vocabulary green against the wrong SDK baseline. Bind this test to the resolved Stripe assembly/package version so every SDK upgrade requires an intentional baseline update.
  Resolved by comparing the committed baseline to Stripe.net's resolved assembly informational version.

- [x] **NAT2 — MEDIUM — native/correctness** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Architecture/ProviderContractInventoryTests.cs:125`
  Payment entry-point discovery runs only when a source file contains the exact text `using Stripe;`, so a fully-qualified `Stripe.PaymentIntentService`, a namespace-specific/global using, or an injected `StripeClient` can add real provider calls without changing the committed inventory. Replace the import-gated regex discovery with syntax/semantic detection of Stripe SDK receivers and cover fully-qualified and global-using forms.
  Resolved by binding invocation receivers with Roslyn per Payment project and covering fully-qualified, namespace-specific, global-using, and injected SDK forms.

- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Domain/ProviderContract/StripeOperationTransitionSpecification.cs:405`
  Every `requires_payment_method` observation is emitted as `PaymentMethodRequired`, but Stripe also returns that status after a decline and exposes the reason through `last_payment_error`; the observation type has no safe classified failure input, so the required `Declined` outcome is unreachable. Add a provider-internal failure classifier/input and make the transition emit the closed `Declined` code with a Concertable-authored message while keeping raw Stripe detail internal.
  Resolved with a provider-neutral internal decline classification, safe closed failure mapping, fail-closed applicability checks, and exhaustive status/session coverage.

- [x] **SEC1 — MEDIUM — security** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/PaymentOperationMappers.cs:40`
  The protobuf mapper fails closed on an unknown failure code but copies `failure.Message` verbatim for every known code, so provider exception text can cross the public client boundary under a valid enum value. Derive the published message from the closed code, or reject any wire message that does not match the central Concertable-authored definition.
  Resolved by deriving every known-code message from the central error definition and ignoring wire text while preserving unknown-code rejection.

## Incremental review — 2026-08-16

> Range reviewed: `85d85aab1..7b7561fa4` (16 commits).

- [x] **NAT3 — MEDIUM — native/correctness** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Architecture/ProviderContractInventoryTests.cs:258`
  The semantic detector still filters method names to `*Async`, but Stripe.net service types also expose synchronous provider operations. A new `service.Create(...)`, `Capture(...)`, or `Refund(...)` call therefore bypasses the exhaustive inventory while the guard remains green. Detect every invocation whose receiver binds to a Stripe SDK type and cover a synchronous form.
  Resolved by scanning every invocation on Stripe API client/service receivers regardless of method suffix and covering synchronous `RefundService.Create` discovery.

- [x] **NAT4 — MEDIUM — native/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Domain/ProviderContract/StripeOperationTransitionSpecification.cs:155`
  Same-state observations are classified as `Duplicate` solely from the normalized state. A decline commonly leaves an existing attempt in `RequiresPaymentMethod` while adding the new closed `Declined` failure, so the BUG1 path can still be discarded as a no-op. Treat an observation as duplicate only when the complete persisted projection is unchanged; apply same-state failure or capture-deadline changes and cover the decline regression.
  Resolved by comparing the complete mutable persisted projection before classifying a duplicate, with regressions for same-state decline and authorization capture-deadline changes.

## Incremental review — 2026-08-16 (NAT3/NAT4 resolutions)

> Range reviewed: `7b7561fa4..6cc1d59d5` (3 commits).

No additional findings survived the confidence filter. The synchronous Stripe-call inventory fix and
same-state persisted-projection fix preserve the provider boundary, terminal-state protection, safe
failure vocabulary, and focused regression coverage.

## Incremental review — 2026-08-16 (current-main reconciliation)

> Range reviewed: `6cc1d59d5..01171e1b2` (2 commits).

No additional findings survived the confidence filter. The merge was conflict-free; the Payment-side
upstream changes remove a dead transaction-mapper method and do not intersect the provider contract.
