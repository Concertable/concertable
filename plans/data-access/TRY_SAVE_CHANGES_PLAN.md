# Shared TrySaveChanges plan

## Outcome

Provide one shared concurrency-save boundary in `Concertable.DataAccess`, clear the failed EF unit of work
safely, and replace Payment's manual aggregate detachment.

## Package topology

`Concertable.DataAccess.Infrastructure` is a published platform package. The new API is additive, but B2B
B2B and Payment compile against the published package rather than this source checkout. Delivery therefore has
three ordered slices:

1. add and publish the shared producer API;
2. migrate every existing direct implementation in the generated platform package sync and merge it green;
3. migrate the in-flight Payment reconciliation branch against the published API.

## Design

- `TrySaveChangesAsync` returns `true` after a successful save and `false` for
  `DbUpdateConcurrencyException`; every other exception propagates.
- A C# 14 extension on `DbContext` owns the EF save/clear behavior.
- `IUnitOfWork<TContext>` exposes the operation and `UnitOfWork<TContext>` delegates to the extension.
- Expected save failures clear every tracked entity from each originating context with
  `ChangeTracker.Clear()` because the failed transactional unit of work cannot be safely reused piecemeal.
- Duplicate-key handling remains separate debt rather than widening the helper with exception policy.

## Phases

### Phase 1 — shared producer

- Add the context extension and unit-of-work operation with focused success, concurrency, propagation,
  and tracker-clearing coverage.
- Replace per-entry failed-change detachment with whole-context clearing.
- Build and test DataAccess, then open the producer PR.

### Phase 2 — published package sync

- Merge the producer and verify the next published platform version contains the API.
- Treat `CS0535` from existing direct `IUnitOfWork<TContext>` implementations as the expected cut-over
  surface, implement the new member in each one, and merge the generated platform-sync PR green.

### Phase 3 — consumers

- Replace Payment reconciliation's `SaveAsync`/`Detach` recovery with the write-context extension exposed
  through its attempt repository, retaining canonical reload after a concurrency loss.
- Run Payment's concurrent duplicate-retry integration coverage.

## Verification

- DataAccess projects build and unit tests pass.
- The packed DataAccess artifact exposes `TrySaveChangesAsync` on `IUnitOfWork<TContext>`.
- Payment's concurrent duplicate-retry integration test passes against the published package baseline.
