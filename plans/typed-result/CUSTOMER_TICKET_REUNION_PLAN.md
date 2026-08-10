# Customer Ticket Reunion migration

> Next steps live in @plans/typed-result/CUSTOMER_TICKET_REUNION_PROGRESS.md → `## Next Steps`.

## Outcome

Recreate the unique Ticket, Concert purchase, checkout, validation, and Customer Payment semantics
from PR #282 on current main using Reunion carriers and service-owned HTTP edges. Preserve observable
behavior and tests; do not rebase or revive the obsolete carrier/CFE implementation wholesale.

## Ownership and boundaries

- Branch: `Feature/typed-result_customer-ticket-reunion`.
- Customer owns Ticket, purchase, and checkout decisions; Payment remains an agnostic adapter reached
  only through its published Contracts/Client packages.
- PR #282 remains untouched until this replacement is locally complete, reviewed, and ready for an
  explicit supersession decision.
- No shared Kernel carrier, Shared.Api terminal, cross-service runtime reference, committed local feed,
  disposable version pin, or machine-specific configuration is introduced.
- `ConcertEntity.DecrementAvailability` remains an invariant exception when the paid-ticket background
  handler applies one event to insufficient stock; returning a Result there would hide a
  consistency/corruption fault from retry/dead-letter handling. `RestoreAvailability` has no
  production caller and its non-positive/over-capacity guards remain impossible-state exceptions.
  Review's star-range guard belongs to the Customer non-Payment plan, not this branch.

## Implementation and delivery DAGs

Implementation requires current main, the exact PR #282 semantic diff, local
`Concertable.Payment.Contracts` / `Concertable.Payment.Client` `0.1.0-alpha.0.915` artifacts from
producer commit `a2497e3e8`, and an exact locally packed `Reunion.Validation` artifact from merged
Reunion source `1500270`. Delivery requires publication of Reunion.Errors and Reunion.Validation,
the Payment source PR, Payment package publication, generated platform sync, replacement
revalidation against the published graph, and then an explicit decision before PR #282 is
superseded.

## Phases

1. [x] Audit PR #282 against current Customer Ticket/Concert/checkout code and record the unique behavior
   and test inventory. Recreate only still-valid semantics.
2. [x] Migrate Ticket and checkout application/module contracts to direct `Reunion` / `Reunion.Errors`
   ownership and use `Reunion.AspNetCore` only at actual Customer HTTP edges.
3. [x] Integrate the exact local Payment packages through temporary restore inputs, update Customer-owned
   Payment callers/mocks, and run affected unit/integration, Customer Release build, carve, and
   mechanical legacy-carrier gates.
4. [x] Restore all published-package configuration, commit and review the source as delivery-ready, then
   wait for published Payment revalidation before delivery or PR #282 supersession.
5. [x] Replace the Ticket DI validator's interim `bool` and `UnitResult<IReadOnlyList<string>>`
   contracts with `Reunion.Validation.ValidationResult`. Preserve the separate typed not-found
   outcome for asynchronous concert lookup, keep existing purchase/checkout ProblemDetails field
   contracts stable, update direct package ownership and tests, and rerun the complete Ticket,
   Customer Release, full-solution, carve, mechanical, and review gates.

## Definition of done

- Unique PR #282 behavior and coverage are preserved deliberately on current main.
- Ticket/checkout source uses Reunion directly with unchanged wire behavior and service isolation.
- Ticket validators return `ValidationResult` with structured `ValidationErrors`; general-purpose
  Result carriers remain only where a non-validation outcome such as missing concert must coexist.
- No temporary package input is committed.
- Review is clean; published Payment revalidation, PR delivery, supersession, merge, publication, and
  generated sync are terminal before closeout.
- Direct purchase/checkout validation stays typed and caller-actionable, while background consistency
  and malformed internal construction remain exceptions and are never exposed as public 4xx contracts.
