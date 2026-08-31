# Specification and Query Object boundary progress

- Plan: `plans/data-access/SPECIFICATION_QUERY_BOUNDARY_PLAN.md`
- Roadmap: `plans/data-access/DATA_ACCESS_ROADMAP.md`
- Roadmap item: `data-access/specification-query-boundary`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-data-access-specification-query-boundary`
- Branch: `Refactor/data-access-specification-query-boundary`
- PR: [#885](https://github.com/Concertable/concertable/pull/885)
- Dependency/package gates: the generated platform sync bumps `ConcertablePlatformVersion` after this
  merges; the B2B proving ground consumes the terminal source from
  [PR #633](https://github.com/Concertable/concertable/pull/633)
- Last reconciled: 2026-08-31 against the branch head and a full local-platform build

## Current state

Phase 1 is implemented and green: the Kernel contract family, the DataAccess evaluator and repository
overloads, the Search reclassification, and the B2B/Customer consumer migration all land together, because
CI packs the platform from branch source and restores services from that feed.

## Next Steps

Merge PR #885, let the platform publish and the generated sync land, then start Phase 3 (the B2B
`BookingSpecification`/`ApplicationSpecification` vocabulary and the removal of the graph-named methods)
once [PR #633](https://github.com/Concertable/concertable/pull/633) is terminal.

## Completed work

- Kernel: caller `ISpecification`/`IOrderedSpecification` family over `IncludePath<TEntity>`, the
  `IIncludableSpecification` + `ThenInclude` continuation (reference and collection overloads),
  `SpecificationOrder`, the `Specification` bases, the predicate family with `And`/`Or`/`Not`/`Via`/
  `IsSatisfiedBy`, and the internal `And`/`Or`/`Not`/`Navigation`/`Expression` nodes.
- DataAccess: `QueryableExtensions.Apply` (includes, deduplicated) and `ApplyOrders`, the `IQuery`
  contracts, the shared `GetByIdAsync`/`GetAllAsync` Specification overloads on both repository bases and on
  Customer's `QueryableReadRepository`, and `UpcomingSpecification`/`DateRangeSpecification` migrated off the
  navigable bases.
- Search: search-term, genre, geometry, per-entity search and Concert search are predicate Specs; sort
  returns order metadata; only `ArtistSearchQuery`/`VenueSearchQuery`/`ConcertSearchQuery` are executable.
  Every contract moved from Application to internal Infrastructure.
- B2B: `ConcertDashboardRepository` and `ConcertRepository` compose predicate Specs and call
  `ToExpression()` once at the end; `Via` carries the Opportunity navigation as a Spec.
- Removed: `ExpressionSpecification`, `INavigableSpecification`, `NavigablePredicateSpecification`, and the
  empty `ISpecification` marker.

## Verification

- `./scripts/local-platform.ps1 prepare` then `build api/Concertable.slnx`: 0 errors.
- `Concertable.Kernel.UnitTests` 259 passed, `Concertable.DataAccess.UnitTests` 19 passed,
  `Concertable.DataAccess.IntegrationTests` 11 passed, `Concertable.Search.UnitTests` 21 passed.
- Full CI on PR #885 green before this round of changes; re-run pending on the new push.

## Reviews

- One independent architecture review of the plan; one full code review at `5394617a0` recorded as approved
  in `reviews/Refactor-data-access-specification-query-boundary.md`. That review predates this round, which
  addressed six findings raised afterwards.

## Decisions, discoveries, blockers, and deviations

- The include contract carries per-step lambdas (`IncludePath.Steps`), not one flattened lambda. A single
  member-chain lambda cannot express a step through a collection, and the caller-facing contract is a
  published package, so the earlier flattened shape would have forced a second breaking cutover the first
  time a graph crossed a collection. The evaluator still renders a dotted path for `Include(string)`; the
  typed steps keep a real `ThenInclude` replay available later without a contract change.
- `ThenInclude` uses two extension overloads over a covariant `IIncludableSpecification<TEntity, out TProperty>`,
  the same mechanism EF Core uses for `IIncludableQueryable`.
- `Via` returns a composable `NavigationSpecification` node. `ToExpression()` is the only terminal, called
  once. Its source type comes from an explicitly typed lambda parameter, because a C# extension member's own
  type arguments cannot be supplied without also supplying the receiver's.
- `And` and `Or` are symmetric over a Spec and a raw lambda; the raw overload lifts into an internal
  `ExpressionPredicateSpecification` leaf rather than materializing early.
- A projected Specification throws on include registration: EF Core discards includes on a projecting query,
  so the combination was silently dead.
- The evaluator exposes `Apply` and `ApplyOrders` separately; the earlier `Apply(IOrderedSpecification<T>)`
  overload could silently drop ordering depending on a call site's static type.
- The Search rename table in the original plan was wrong — most Search "Specifications" are predicates, not
  executable transformations. The plan now records the reclassification instead.
- Phases 1 and 2 were re-cut: every in-repo consumer migrates with the producer, because the monorepo CI
  build is source coherent through the local platform feed.
- Search runtime sorting stays order metadata applied by its Query Object; `GetByIdAsync` never applies
  ordering; paging is not added speculatively.
