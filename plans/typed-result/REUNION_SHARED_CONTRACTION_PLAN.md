# Reunion Shared contraction

> Next steps live in @plans/typed-result/REUNION_SHARED_CONTRACTION_PROGRESS.md → `## Next Steps`.

## Outcome

Remove the obsolete Concertable carrier/terminal surfaces and remaining third-party functional
dependencies only after every service consumer is prepared against Reunion. Enforce direct package
ownership and preserve all wire boundaries.

## Dependency model

Implementation requires a current, exact inventory from delivery-ready B2B, Auth, Customer
non-Payment, Customer Ticket, Payment, Search, messaging, and background paths. Delivery follows the
published-package cutover and generated sync topology discovered by that inventory. This plan does not
guess the contraction before consumers expose their final call sites.

## Phases

1. Inventory remaining old Kernel functional/error carriers, Shared.Api terminals, FluentResults/CFE
   dependencies, and package re-exposure after every consumer preparation branch is reviewed.
2. Design the smallest publish/sync contraction sequence and record both DAGs and exact owners.
3. Implement, verify, review, publish, migrate generated sync consumers, and enforce architecture gates.
4. Close the typed-result lifecycle only when every service and package gate is terminal.

## Definition of done

- Production source uses Reunion directly with no duplicate carrier or terminal surface.
- Every compiling project owns its direct Reunion package; no transport carries a functional carrier.
- All service builds, tests, carves, reviews, publications, generated syncs, and cleanup gates are green.
