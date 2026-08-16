# Code review — Feature/payments_provider-contract-baseline

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`  _(2026-08-16)_

**Security-reviewed up to commit:** `85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`  _(2026-08-16)_

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

- [ ] **SEC1 — MEDIUM — security** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/PaymentOperationMappers.cs:40`
  The protobuf mapper fails closed on an unknown failure code but copies `failure.Message` verbatim for every known code, so provider exception text can cross the public client boundary under a valid enum value. Derive the published message from the closed code, or reject any wire message that does not match the central Concertable-authored definition.
