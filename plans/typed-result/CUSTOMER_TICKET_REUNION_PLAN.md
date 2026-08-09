# Customer Ticket Reunion migration

> Next steps live in @plans/typed-result/CUSTOMER_TICKET_REUNION_PROGRESS.md → `## Next Steps`.

## Outcome

Recreate the unique Ticket, Concert purchase, checkout, and Customer Payment semantics from PR #282 on
current main using Reunion carriers and service-owned HTTP edges. Preserve observable behavior and
tests; do not rebase or revive the obsolete carrier/CFE implementation wholesale.

## Ownership and boundaries

- Branch: `Feature/typed-result_customer-ticket-reunion`.
- Customer owns Ticket, purchase, and checkout decisions; Payment remains an agnostic adapter reached
  only through its published Contracts/Client packages.
- PR #282 remains untouched until this replacement is locally complete, reviewed, and ready for an
  explicit supersession decision.
- No shared Kernel carrier, Shared.Api terminal, cross-service runtime reference, committed local feed,
  disposable version pin, or machine-specific configuration is introduced.

## Implementation and delivery DAGs

Implementation requires current main, the exact PR #282 semantic diff, and local
`Concertable.Payment.Contracts` / `Concertable.Payment.Client` `0.1.0-alpha.0.911` artifacts from
producer commit `a779fe041`. Delivery requires the Payment source PR, package publication, generated
platform sync, replacement revalidation against the published version, and then an explicit decision
before PR #282 is superseded.

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

## Definition of done

- Unique PR #282 behavior and coverage are preserved deliberately on current main.
- Ticket/checkout source uses Reunion directly with unchanged wire behavior and service isolation.
- No temporary package input is committed.
- Review is clean; published Payment revalidation, PR delivery, supersession, merge, publication, and
  generated sync are terminal before closeout.
