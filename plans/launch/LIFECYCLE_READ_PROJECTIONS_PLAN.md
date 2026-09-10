# Lifecycle reporting read projections

> **Next steps live in @plans/launch/LIFECYCLE_READ_PROJECTIONS_PROGRESS.md -> `## Next Steps`.**

## 1. Decision

Adopt consumer-owned, event-fed read projections selectively for lifecycle reporting queries that
cross module boundaries. Do not denormalize every downstream-to-upstream read.

The authority and command direction remains:

```text
Opportunity -> Application -> Booking -> Concert
```

A downstream command may synchronously query authoritative upstream facts when the answer controls a
write or enforces an invariant. `Apply`, `Accept`, terms-fingerprint validation, and checkout creation
therefore continue to use the owning module facade. Eventual consistency is not acceptable at those
decision points.

A presentation or reporting query may instead use a projection owned by its consumer when all of the
following hold:

- a stale answer for one message-delivery interval is acceptable;
- the query repeatedly composes facts from more than one module;
- the projection removes runtime coupling or a shared scoped dependency graph;
- the producer can publish every required change durably and the consumer can rebuild or reconcile;
- the duplicated fields have one named owner and are never written through the projection.

The first implementation is deliberately narrow: Application owns an Opportunity availability
projection for its venue and artist pending-application counts. The Dashboard module remains a
stateless query composer. A Dashboard-wide materialized database is not justified by the current
three KPI endpoints and would multiply event, migration, replay, and consistency work.

## 2. Evidence and rationale

`VenueDashboardService.GetAsync` needs both the Application pending count and the Opportunity open
count. `ApplicationDashboardService.GetVenuePendingCountAsync` currently performs an Application
query and then calls `IOpportunityModule.GetUpcomingIdsAsync`. The direct Opportunity count and that
transitive call resolve the same scoped `OpportunityReadDbContext`; parallel composition therefore
caused EF Core's second-operation exception. The current serialization is correct and remains until
the dependency is removed.

Separate module contexts did not fail. The problem is that a facade hides a transitive query back into
another scoped module. Moving the availability fact into an Application-owned projection makes the
Application count a genuinely local read and restores an acyclic runtime query graph for this path.

There is already a local precedent: Application consumes Concert lifecycle events into its
concert-availability projection, and Concert consumes Artist and Venue events into local read models.
The new projection follows the same ownership shape while carrying only Opportunity availability
facts needed by Application reporting.

This is not objectively better for every read. Projections add eventual consistency, duplicate data,
event ordering, idempotency, backfill, replay, monitoring, and seeding obligations. For an infrequent
strongly consistent lookup inside one deployable, a query-only module facade remains the smaller and
better design.

## 3. Target boundary

Application owns an internal Infrastructure read model provisionally named
`OpportunityAvailabilityProjection`. Its minimum persisted shape is:

- `OpportunityId` as the key;
- the end timestamp used by the existing definition of upcoming;
- a monotonic producer revision or equivalent ordering token.

Do not copy Deal, Venue, genres, or the complete Opportunity DTO into this projection unless a proven
Application-owned query requires them. Do not add an EF navigation or foreign key to Opportunity.

Opportunity publishes a flat availability integration event after creation and whenever the projected
facts change. The outbox promise and Opportunity write must be atomic. The event contract must solve
database-generated Opportunity IDs explicitly; publishing a creation event after an already committed
save is not an acceptable lost-message window. The consumer records inbox identity and applies only a
newer producer revision, making duplicate and out-of-order delivery harmless.

`ApplicationDashboardService` then calculates both pending counts using only Application persistence.
Its `IOpportunityModule` dependency is removed. If `GetUpcomingIdsAsync` has no remaining legitimate
consumer, remove it from `IOpportunityModule`; otherwise retain it for the consumers that still need an
authoritative synchronous query.

## 4. Boundaries that remain synchronous

The following are outside the projection cutover:

- applying to an open Opportunity;
- acceptance eligibility and terms-fingerprint validation;
- apply and accept checkout construction;
- any mutation guard whose correctness depends on current Opportunity, Deal, Artist, or Venue facts;
- API mapping where the contract promises current authoritative data and no read-model requirement has
  been approved.

Those paths may be optimized independently, but they must not make business decisions from an
eventually consistent reporting projection.

## 5. Phases

### Phase 0 - lock semantics and the consistency budget

- [ ] Confirm that the existing pending-count meaning is preserved: an Application in the relevant
  state whose Opportunity end timestamp is in the future. Do not silently add Opportunity `Open`
  state to the predicate.
- [ ] Record the acceptable dashboard lag and the UI behavior while the projection is catching up.
- [ ] Inventory every Dashboard facade call and its transitive module dependencies. Classify each as
  authoritative command support, current-detail mapping, or eventually consistent reporting.
- [ ] Keep the current serialized Venue KPI implementation if the product does not accept eventual
  consistency; that is the stop gate for the rest of this plan.

### Phase 1 - publish durable Opportunity availability facts

- [ ] Add the smallest public integration-event contract that represents the projected availability
  facts and an ordering token.
- [ ] Make Opportunity creation and schedule changes emit the event through the transactional outbox.
- [ ] Prove that creation carries the final database-generated Opportunity ID without a post-commit
  publish gap. Use an explicit multi-flush transaction or another reviewed atomic mechanism supported
  by the existing unit-of-work infrastructure.
- [ ] Make every production mutation path that changes the projected facts emit exactly one final
  snapshot, including collection synchronization and event-driven Opportunity transitions if the
  selected projection includes state.
- [ ] Add focused Opportunity integration coverage for create, update, duplicate delivery inputs, and
  final event payloads.

### Phase 2 - build the Application-owned projection

- [ ] Add the internal Application Infrastructure projection, configuration, context surface, and
  repository/query abstraction. Keep it read-only outside its event handler.
- [ ] Handle the Opportunity availability event through the inbox and Application unit of work.
  Duplicate delivery is a no-op and an older revision cannot overwrite a newer row.
- [ ] Change the pending-count repository query to join Applications to the local projection and count
  in SQL rather than loading candidate rows and filtering in memory.
- [ ] Remove `IOpportunityModule` from `ApplicationDashboardService` and remove dead dashboard
  projection DTO/query code.
- [ ] Add Application integration coverage for initial projection, changed end date, duplicate and
  out-of-order events, tenant-specific counts, and the exact end-time boundary.

### Phase 3 - recover safe dashboard composition

- [ ] Prove from the resolved dependency graph that Venue pending Applications and open Opportunities
  no longer converge on one scoped context.
- [ ] Restore parallel KPI reads only for branches whose complete transitive graphs are disjoint.
  External Payment calls may run concurrently with local reads independently of this projection.
- [ ] Add a Dashboard API regression that forces overlapping query execution and proves the endpoint
  completes without EF Core context re-entry.
- [ ] Add a focused architecture assertion that `ApplicationDashboardService` has no Opportunity
  module dependency. Do not ban Application's authoritative Opportunity dependencies globally.

### Phase 4 - make projection data reproducible

- [ ] Re-scaffold the affected initial migrations through `api/initial-migrations.ps1` while the
  product remains pre-production.
- [ ] Drive dev and E2E projection rows through canonical Opportunity events; never insert the
  handler-owned projection directly.
- [ ] If an integration-test projection seeder is needed, derive it from the same canonical Opportunity
  seed catalog and map through the production creation path.
- [ ] Before executing this plan against any non-empty production database, add a bounded backfill or
  replay operation, a count/revision reconciliation check, and an operator-visible recovery procedure.
- [ ] Verify the smallest affected builds and focused integration suites locally, then rely on the
  draft-PR build, carve, unit, and integration gates and merge-queue E2E defined in
  `docs/REMOTE_VALIDATION.md`.

### Phase 5 - audit, do not generalize automatically

- [ ] Revisit Application response mapping and Opportunity dashboard enrichment using the Phase 0
  classification. Keep facade composition unless measured coupling, latency, or carve requirements
  justify another projection.
- [ ] Evaluate a Dashboard-owned materialized model only if several endpoints need the same
  cross-module snapshot or independent deployment/read scaling becomes a real requirement.
- [ ] Record each accepted projection as its own consumer-owned slice with event completeness,
  consistency, rebuild, and deletion criteria. Do not create a generic lifecycle read database.

## 6. Definition of done

- Venue and artist pending-application counts read only Application-owned persistence.
- Application dashboard reporting has no runtime dependency on `IOpportunityModule`.
- Apply, Accept, checkout, and invariant paths still use authoritative module facts.
- Opportunity availability publication is atomic, ordered, idempotently consumed, and reproducible from
  supported seed/backfill paths.
- The Venue KPI endpoint may safely parallelize only proven-disjoint branches and has a deterministic
  regression test for the former scoped-context collision.
- No projection becomes a command surface, aggregate authority, cross-module EF navigation, or second
  source of truth.

## 7. Explicit non-goals

- Replacing every query-only module facade with messaging.
- Giving the Dashboard module a database solely to avoid `Task.WhenAll` discipline.
- Using `IDbContextFactory`, transient contexts, or child service scopes to hide an overlapping
  dependency graph.
- Making eventually consistent data responsible for write eligibility or financial decisions.
- Changing lifecycle ownership or the accepted stage order.
