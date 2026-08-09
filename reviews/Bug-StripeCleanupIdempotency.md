# Code review — Bug/StripeCleanupIdempotency

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `05ca100f19c74494bf25d4850a4842b2642d0e61`  _(2026-08-10)_
**Security-reviewed up to commit:** `b376131f73933849f6af44111d84a1b69eaf9c78`  _(2026-08-09)_

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

## Incremental review — 2026-08-09 (convention wording sync)

> Range reviewed: `42a9639..ff7be0f` (3 commits).

No issues found. The imported PR #456 change only simplifies the already-reviewed empty-block
convention wording; the runtime tree and verification results are unchanged.

## Incremental review — 2026-08-09 (E2E isolation correction)

> Range reviewed: `ff7be0f..b376131` (10 commits; 4 first-parent commits).

One architecture issue was found and fixed in `b376131`: the standalone B2B AppHost imported
`Concertable.Customer.AppHost.Extensions` only to provision Customer's unused ASB subscriptions.
The reference and `AddCustomerTopology()` call are removed; Search topology already provisions the
B2B event topics used by this stack.

No open issues remain. `3f9d954` removes the E2E filter seam from production `WebhookProcessor`,
places account-wide Stripe event isolation in a `Payment.Seed` decorator, restores the ordinary
declined-card operation plus downstream UI assertion, renames the harness resource to
`StripeCustomerResolver`, and exposes Payment's partial owner mappings as `Option<string>`. This
supersedes the earlier declined-card review description above. Fixture teardown remains unchanged and
its duplication is recorded in the shared E2E technical-debt file instead of introducing a cleanup
callback registry.

Verified after merging current `origin/main`: Payment unit tests 232/232, E2E Stripe helper tests 5/5,
and both B2B and Customer UI E2E project builds succeed with zero errors. Checked correctness,
security and secret handling, microservice isolation, module boundaries, seeding, C# conventions,
and changed-path test coverage.

## Incremental review — 2026-08-09 (fixture contract and naming)

> Range reviewed: `b376131..ccb4013` (7 commits).

No issues found. The attempted Payment resolver rename is fully reversed in the final tree, retaining
`StripeE2EAccountResolver` because `Concertable.Payment.Seed` is not an E2E-specific namespace. The
fixture-facing property now matches its `StripeCustomerResolver` type, and B2B `UiFixture` restores
the established non-null Playwright lifecycle. Failure-continuing fixture teardown remains separately
recorded in shared E2E technical debt. Payment unit tests pass 232/232 and the B2B UI E2E project
builds with zero errors. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and changed-path test coverage; the net range introduces no security-sensitive change.

## Incremental review — 2026-08-10 (Customer card-tab navigation)

> Range reviewed: `ccb4013..05ca100` (3 commits).

No issues found. The merge-group trace proves the Customer Payment Element already had Card selected,
then the old nested-text click landed on the sticky application header's Find link at the same page
coordinates. The helper now targets Stripe's accessible tab, preserves an already-selected tab, and
uses keyboard activation only when selection is required. Saved-card confirmation is unchanged, and
all new-card success, decline, and 3DS callers retain their existing outcome semantics. The shared
helper tests pass 5/5 and both Customer and B2B UI E2E projects build with zero errors. The local
browser rerun was discarded after Docker developed SQL pre-login resets during stack startup; full
runtime verification remains with the merge queue. Checked correctness, shared-caller behavior, C#
conventions, and changed-path coverage; the range introduces no security-sensitive change.
