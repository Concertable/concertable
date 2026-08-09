# Code review — Bug/StripeCleanupIdempotency

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `67d958bbaae5a38da04d54b2a0ecbc94ee6ea585`  _(2026-08-09)_
**Security-reviewed up to commit:** `67d958bbaae5a38da04d54b2a0ecbc94ee6ea585`  _(2026-08-09)_

> Range reviewed: `dc0da93..abcc9be` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, security, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-09

> Range reviewed: `abcc9be..a9fb8dc` (36 commits).

No issues found. The branch-authored runtime change isolates every E2E fixture behind run-scoped
Stripe customers, filters account-wide webhook traffic before deduplication and handling, guarantees
owned-customer cleanup after partial fixture initialization, and surfaces Stripe confirmation
failures at the browser action that caused them. The B2B standalone AppHost adds Customer's message
topology only, not Customer's runtime, so the service boundary remains intact. The range also imports
the already-reviewed frontend boundary work from `origin/main`; its latest branch-authored tail is
documentation only. Checked correctness, security and secret handling, microservice isolation,
module boundaries, seeding, C# conventions, and test coverage. The full Payment unit suite passes
228/228.

## Incremental review — 2026-08-09 (remote reconciliation)

> Range reviewed: `a9fb8dc..67d958b`.

No issues found. The merge preserves the remote exact-PaymentIntent hold correlation and resolves
its sole overlap by removing the now-obsolete customer-and-amount lookup. The reconciled tree passes
the E2E helper tests 5/5, Payment unit tests 228/228, and the B2B UI E2E project build with zero
errors. No new secret, service-boundary, webhook, or runtime behavior was introduced by the merge.

## Incremental review — 2026-08-09 (merge-queue regression)

- [ ] **NAT1 — HIGH — correctness (expected card declines fail the scenario before its assertion)** — `api/Concertable.Payment/tests/E2ETests/Concertable.Payment.E2ETests.Helpers/Support/StripeCardEntry.cs:60`
  `ConfirmAsync` throws for every non-2xx Stripe confirmation response, but four negative scenarios intentionally submit a declined card and then assert the application's rejection UI. Merge-group run 31328570590 therefore failed all four at the `When` step on the expected Stripe 402, skipping `Then the payment is rejected`; 27 other B2B UI scenarios passed. Preserve the response wait, but let expected decline responses return to the scenario so its UI assertion remains the source of truth. Add focused coverage for successful and declined confirmation handling.
