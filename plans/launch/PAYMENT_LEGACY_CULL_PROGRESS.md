# Payment legacy cull and vocabulary progress

- Plan: `plans/launch/PAYMENT_LEGACY_CULL_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/payment-legacy-cull`
- Worktree: not created (open as `Refactor/launch_payment-legacy-cull` when unblocked)
- Branch: not created
- PR: not opened
- Dependency/package gates: gated on BOTH consumer migrations being terminal — the B2B migration
  riding PR #633 and the Customer payment-reference migration.
- Last reconciled: 2026-09-04 at plan authoring

## Current state

Plan authored; deliberately dormant. The raw-identifier APIs exist solely for package-compatible
consumer migration (recorded in `PAYMENT_METHOD_COMMITMENTS_PROGRESS.md`); this plan deletes them
the moment nothing consumes them.

## Next Steps

Blocked: both consumer migrations off the raw-identifier Payment APIs are not terminal
Blocked by: `plans/launch/PAYMENT_METHOD_COMMITMENTS_PROGRESS.md` (which dispatches the B2B migration riding PR #633) and `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PROGRESS.md`
Unblock action: both consumer PRs merge and their platform pins advance past the reference migration
Resume when: no call to a raw-identifier Payment API exists outside `api/Concertable.Payment`, and both blocking ledgers are terminal

Then: open the worktree and execute Delivery items 1–4 in order, run the review workflow, publish
the breaking release, and follow both consumer pin bumps through revalidation to terminal.

## Completed work

- Authored the plan and ledger (2026-09-04).

## Verification

- None yet; the branch does not exist.

## Reviews

- No review yet; the branch does not exist.

## Decisions, discoveries, blockers, and deviations

- The legacy `StripeRequestOptions` single-attempt idempotency shim's `TECH_DEBT.md` entry resolves
  in Delivery item 2 (recorded there as "resolving with the §7 step 5 legacy cull").
- The vocabulary renames batch with the removal deliberately: both are breaking, and splitting them
  would cost two coordinated releases for no consumer benefit.
