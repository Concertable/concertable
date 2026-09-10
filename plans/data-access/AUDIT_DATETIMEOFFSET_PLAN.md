# Audit DateTimeOffset plan

## Outcome

Make the shared audit contract use `DateTimeOffset`, keep audit stamping centralized in
`Concertable.DataAccess`, and adopt it on Payment's mutable session operation and attempt without confusing
database audit time with provider-observation time.

## Package topology

`IAuditable` is public API in the packable `Concertable.Kernel` project. `AuditInterceptor` is shipped by
the packable `Concertable.DataAccess.Infrastructure` project and compiles against Kernel source. Every
service consumes the shared platform through its pinned `ConcertablePlatformVersion`; Payment is the only
current source consumer implementing `IAuditable` (`TransactionEntity` and `EscrowEntity`).

This requires two delivery merges:

1. producer expand: change Kernel and DataAccess source, then publish the next platform package set;
2. generated platform sync: bump consumers, migrate Payment's existing implementations, and re-scaffold
   Payment's initial migration.

The in-flight payment-session-state PR can prepare its own session-entity adoption, but cannot become
merge-ready until it consumes the published/synced audit contract.

## Design

- `IAuditable.CreatedAt` and `LastModifiedAt` use `DateTimeOffset`.
- `AuditInterceptor` stamps `TimeProvider.GetUtcNow()` directly and remains the sole generic audit writer.
- `CreatedBy` and `LastModifiedBy` retain the existing opaque actor string and `system` fallback.
- Audit timestamps describe local persistence. Payment's `LastAttemptedAt`, `LastObservedAt`, `TerminalAt`,
  provider-event time, and transition evidence remain domain-owned and are not interceptor-derived.
- Audit adoption is explicit for mutable persisted aggregates, not a mechanical marker on every entity.

## Phases

### Phase 1 — shared producer ✅

- Change `IAuditable` and `AuditInterceptor` to `DateTimeOffset`.
- Add focused tests proving added and modified entities receive the exact injected time and actor.
- Build and test Kernel and DataAccess from source.
- Open the producer PR and take it through publication. Source implementation and local verification are
  complete; review, merge, and publication remain.

### Phase 2 — published consumer sync

- On the generated platform-sync PR, migrate `TransactionEntity` and `EscrowEntity` audit properties and
  every affected caller to `DateTimeOffset`.
- Re-scaffold Payment's initial migration.
- Build the standalone service closures and merge the green sync.

### Phase 3 — payment session adoption

- Bring the published/synced platform baseline into the payment-session-state branch.
- Implement `IAuditable` on `PaymentSessionOperationEntity` and `PaymentSessionAttemptEntity`.
- Leave transition-specific timestamps in the aggregate and let the interceptor own generic audit fields.
- Re-scaffold Payment's initial migration and rerun focused session persistence/transition coverage.

## Verification

- Shared producer project builds and DataAccess unit tests pass.
- Generated output/package checks remain green.
- Consumer sync builds every service against the published package baseline.
- Payment's re-scaffolded model uses `datetimeoffset` for all four audit timestamps.
- The payment-session-state branch's existing unit and integration gates remain green.
