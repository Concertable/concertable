# Concert tech debt

## Durable financial lifecycle operations

Accepting, withdrawing, or cancelling an application carries the HTTP request cancellation token through Payment's irreversible capture, deposit, or refund and into the later B2B lifecycle save. A disconnect or process failure after Payment succeeds can therefore leave money moved while the application remains in its previous state. A request-independent token does not close the process-failure window, and the existing Payment event cannot reconstruct the missing B2B transition.

Owner decision: authorize a separate cross-service B2B + Payment saga/package cut-over, or explicitly accept the unresolved financial/state inconsistency risk. The durable design must persist the lifecycle intent and intermediate state before the remote financial operation, stage a transactional outbox command, make Payment operations idempotent by booking, and reconcile pending work in a worker. It must cover cancellation after Payment succeeds.

Resolves when: the cross-service saga is implemented and verified with cancellation-after-payment and process-recovery tests, and application financial state can no longer diverge from Payment after request cancellation or service failure.

## Cross-tenant read-check abstractions are fragmented into one-method services

`IConcertAvailability`, `ISelfBillingAgreementGate`, and `ObligationChecker` each answer a
boolean/read question over the tenant-independent `IConcertReadDbContext`, and each is its own single-purpose
service. `ObligationChecker` follows the `XChecker` naming convention; `IConcertAvailability` (a state-noun
that says nothing about what it does) and `ISelfBillingAgreementGate` (a `Gate` coinage — and `Gate`/`Guard`
connote *throwing*, which these do not) still need renaming to it. Two of them also overlap:
`ISelfBillingAgreementGate` and `ObligationChecker` both query
`SelfBillingAgreements` for a current agreement (`ExpiresAtUtc > now`). The module runs two competing
conventions for the same thing — per-entity `XReadRepository` finders (what `persistence` prescribes: "don't
wrap a single query already owned by a repository in a one-method interface") versus purpose-named capability
services (what `multitenancy` blessed for `IConcertAvailability`).

Owner decision: settle on one convention. The read queries move to per-entity read repositories
(`IConcertReadRepository`, `ISelfBillingAgreementReadRepository`, an application read finder), and the
decisions that compose them stay with their consumers — apply/accept rules in `ApplicationValidator`,
settlement deferral in `FinishExecutor`, and the GDPR obligation check + export behind the `ConcertModule`
facade. `IConcertAvailability` and `ISelfBillingAgreementGate` are then deleted, and the self-billing overlap
collapses to a single finder. Also remove the dead `IApplicationValidator` dependency that `ConcertService`
injects but never calls.

Resolves when: the bespoke `*Gate`/`*Availability` read-check services are gone, their queries live on read
repositories, the self-billing overlap is a single finder, and `ConcertService` no longer injects an unused
validator.
