# Code review — Bug/StripeCleanupIdempotency

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `42a9639c3208c2c05c5cff4989ba0205f5d28617`  _(2026-08-09)_
**Security-reviewed up to commit:** `42a9639c3208c2c05c5cff4989ba0205f5d28617`  _(2026-08-09)_

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

## Incremental review — 2026-08-09 (declined-card semantics)

> Range reviewed: `67d958b..a797e2b` (2 commits).

No issues found. Stripe confirmation transport is now synchronized for every card submission while
success validation remains limited to successful-flow operations. Expected-decline scenarios use a
named operation and retain their existing UI rejection assertions; the generic confirmation helper
contains no outcome enum, boolean mode, or status-code exception. Error extraction is centralized in
one E2E response extension and catches only malformed JSON using the repository's compact empty-block
form. The E2E helper tests pass 5/5 and both B2B and Customer UI E2E projects build with zero errors.

## Incremental review — 2026-08-09 (current-main sync)

> Range reviewed: `a797e2b..42a9639` (3 commits).

No issues found. The only imported changes are the separately reviewed and merged E2E diagnostic
guardrails from PR #454; they do not alter runtime behavior. After the merge, the helper tests pass
5/5 and both affected UI E2E projects build with zero errors.
