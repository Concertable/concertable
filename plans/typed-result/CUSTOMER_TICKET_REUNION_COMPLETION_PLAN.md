# Customer Ticket Reunion migration completion

Next steps live in @plans/typed-result/CUSTOMER_TICKET_REUNION_COMPLETION_PROGRESS.md → `## Next Steps`.

## Outcome

Complete the Customer Ticket Reunion migration after the post-merge audit of PR #475 found nullable
application/module lookup boundaries and unnecessarily expanded Result construction. Keep repository
lookups nullable as persistence concerns, expose expected absence as `Option<T>`, and preserve every
existing HTTP, payment, validation, and background consistency behavior.

## Ownership and boundaries

- Branch: `Fix/typed-result_customer-ticket-reunion-completion`.
- Customer owns Concert and Ticket application/module contracts and their HTTP termination.
- Payment remains an agnostic published client; this correction maps its typed error without changing
  any Payment contract.
- Repository single-item lookups remain nullable and convert to Reunion at the service/module boundary.

## Phases

1. [x] Reconcile PR #475 and its closed plan against the roadmap's Result/Option rules.
2. [x] Convert the remaining Customer Concert/Ticket service and module lookups to `Option<T>`, carry
   cancellation through the Concert repository query, normalize Ticket Result construction and payment
   mapping, and update affected consumers, package ownership, and tests.
3. [ ] Complete the full solution, affected Customer unit/integration, standalone carve, mechanical
   inventory, and review gates; deliver the correction PR through merge, publication, platform sync,
   and terminal docs closeout.

## Definition of done

- Customer has no nullable non-persistence single-item application, service, or module lookup contract.
- Nullable repository lookups convert with Reunion's published `ToOption()` operation.
- Ticket uses target-typed Result conversions and `Map`/`MapError` where only success/error projection
  is required.
- Concert lookup cancellation reaches the EF query.
- Existing purchase, checkout, eligibility, review, Concert details, and background completion behavior
  remains covered and unchanged.
- Build, affected unit/integration tests, carve, inventories, review, PR, publication, and platform sync
  are terminal before the plan pair is closed.
