# Specification and Query Object boundary plan

## Outcome

Separate Concertable's overloaded current Specification family into three deliberately different tools:

1. caller-selected declarative Specifications for named eager-loading shape and optional projection;
2. Boolean predicate Specifications for reusable rules composed inside repositories and Query Objects;
3. infrastructure-only Query Objects for reusable `IQueryable` pipelines.

The B2B Concert repositories are the first shape-Specification proving ground. The shared contracts remain
general enough for later adoption, but this plan does not mechanically replace fixed repository queries or
introduce caller choice where none exists.

## Current baseline

- `Concertable.Kernel.Specifications.ISpecification<TEntity>` and its parameterized overload currently mean
  executable `IQueryable` transformations.
- `IPredicateSpecification<TEntity>` inherits that executable root. `And`, `Or`, and `Not` immediately
  flatten operands into an `ExpressionSpecification<TEntity>`.
- `INavigableSpecification` and `NavigablePredicateSpecification` apply predicates through navigation
  expressions while retaining `IQueryable` in the contract.
- Search's search, geometry, and sort Specifications are DI-injected executable query transformations and
  therefore are Query Objects under the final terminology.
- Booking, Application, Opportunity, CommissionBinding, and Escrow still contain graph-named repository
  methods. Current `main` no longer contains the plain `GetByIdAsync` graph-widening overrides described by
  the original design prompt, so implementation must not invent override-removal work that no longer exists.
- The in-flight B2B lifecycle refactor in [PR #633](https://github.com/Concertable/concertable/pull/633)
  changes the same Concert repository topology. Its terminal source is the baseline for the B2B
  proving-ground phase; producer and Search work do not wait for it.

## Final terminology and naming

Classes and interfaces use the full `Specification` word. Every local, field, and parameter holding a
Specification ends in `Spec`; Query Object names end in `Query`.

| Concern | Final contract | Meaning |
|---|---|---|
| Entity shape | `ISpecification<TEntity>` | Named include graph only; no predicate and no `IQueryable` |
| Projected shape | `ISpecification<TEntity, TResult>` | Entity shape plus a selector; the second generic always means projection |
| Include metadata | `IncludePath<TEntity>` | One typed include chain, step by step, consumed only by the DataAccess evaluator |
| Include continuation | `IIncludableSpecification<TEntity, TProperty>` and its `ThenInclude` extensions | Typed continuation of the current include chain, including through a collection navigation |
| Shape implementation | `Specification<TEntity>` | Include storage and protected typed include registration |
| Projected implementation | `Specification<TEntity, TResult>` | Shape base plus selector contract; rejects include registration |
| Ordered shape | `IOrderedSpecification<TEntity>` | Entity shape plus an ordered key-selector sequence |
| Ordered projected shape | `IOrderedSpecification<TEntity, TResult>` | Ordered shape plus projection |
| Order metadata | `SpecificationOrder<TEntity>` and `SpecificationOrderDirection` | Typed ordering consumed by collection evaluators |
| Predicate | `IPredicateSpecification<TEntity>` | `Expression<Func<TEntity, bool>> ToExpression()` |
| Parameterized predicate | `IPredicateSpecification<TEntity, TParameters>` | `ToExpression(TParameters parameters)` for a DI collaborator with runtime input |
| Predicate convenience | `PredicateSpecification<TEntity>` and `PredicateSpecification<TEntity, TParameters>` | Optional leaf-authoring bases, not the composition mechanism |
| Boolean nodes | `AndSpecification<TEntity>`, `OrSpecification<TEntity>`, `NotSpecification<TEntity>` and parameterized overloads | Internal composite nodes returned by predicate extensions |
| Navigation node | `NavigationSpecification<TSource, TNavigation>` and its parameterized counterpart | Internal predicate adaptation through a navigation expression, returned by `Via` and still composable |
| Expression leaf | `ExpressionPredicateSpecification<TEntity>` and its parameterized counterpart | Internal leaf lifting a raw lambda so `And`/`Or` never take a materialization shortcut |
| Executable query | `IQuery<TEntity, TParameters>` | Infrastructure-only `IQueryable<TEntity> Apply(...)` |
| Projecting executable query | `IQuery<TSource, TParameters, TResult>` | Infrastructure-only transformation to `IQueryable<TResult>` |

There is no empty Specification marker, `IEntityGraphSpecification`, `IProjectionSpecification`,
`FilteredSpecification`, `QueryShape`, `Specification.For(...)`, third-generic caller Specification, or
ambiguous Boolean include switch. The caller-facing and predicate families share no base type at all.

Runtime values normally enter a caller-created Specification through its constructor. The parameterized
predicate contract remains only for DI-owned rules such as date-range evaluation where the same collaborator
must accept a runtime value. Query Object parameters belong to the Query contract and do not change the
meaning of Specification generic arity.

## Caller-selected Specification contract

`ISpecification<TEntity>` directly owns typed include-chain metadata. It is not a marker and it exposes no
EF Core or `IQueryable` type. `Specification<TEntity>` owns the mutable construction mechanics behind
protected `Include` registration; callers only see the named fluent vocabulary exposed by a module
Specification. The public property is `IReadOnlyList<IncludePath<TEntity>> Includes`.

`protected Include(...)` returns `IIncludableSpecification<TEntity, TProperty>`, whose `ThenInclude`
extensions append further typed steps to the same `IncludePath`. Two overloads mirror EF Core's own
`IIncludableQueryable` shape — one continuing from a reference navigation, one continuing from
`IEnumerable<TElement>` — so a chain may pass through a collection. That is what makes the module
vocabulary a single continuous chain rather than one method per graph combination, and it is why the
contract carries per-step lambdas rather than a flattened path string: a later evaluator can replay real
`ThenInclude` calls without another package cutover.

`ISpecification<TEntity, TResult> : ISpecification<TEntity>` adds
`Expression<Func<TEntity, TResult>> Selector`. A projected Specification rejects include registration:
EF Core builds a projection's joins from its selector and discards includes, so a silently dead include is
a construction-time `InvalidOperationException` rather than a query that quietly loads nothing. The
projected repository overloads therefore never apply includes.

The shared repository surface is exactly:

```csharp
Task<TEntity?> GetByIdAsync(
    TKey id,
    ISpecification<TEntity> spec,
    CancellationToken ct = default);

Task<TResult?> GetByIdAsync<TResult>(
    TKey id,
    ISpecification<TEntity, TResult> spec,
    CancellationToken ct = default);

Task<IEnumerable<TEntity>> GetAllAsync(
    ISpecification<TEntity> spec,
    CancellationToken ct = default);

Task<IEnumerable<TEntity>> GetAllAsync(
    IOrderedSpecification<TEntity> spec,
    CancellationToken ct = default);

Task<IEnumerable<TResult>> GetAllAsync<TResult>(
    ISpecification<TEntity, TResult> spec,
    CancellationToken ct = default);

Task<IEnumerable<TResult>> GetAllAsync<TResult>(
    IOrderedSpecification<TEntity, TResult> spec,
    CancellationToken ct = default);
```

The repository always owns the ID predicate. There is no `FirstOrDefaultAsync(spec)`, no caller-supplied ID
Specification, and no overload accepting `IPredicateSpecification<TEntity>`. Custom repository methods may
accept a shape Specification while retaining their own fixed filter, but only when the caller genuinely
chooses the shape. A fixed graph or projection used by one operation stays inside that repository method.

The evaluator applies only the capability named by the overload. It must never inspect the runtime concrete
type and opportunistically apply `IPredicateSpecification<TEntity>` when the accepted parameter is
`ISpecification<TEntity>`.

## Module shape vocabulary

A module Specification is an empty subclass of `SpecificationBuilder<TEntity>`. It declares no methods at
all — the vocabulary is the fluent `Include`/`ThenInclude`/`OrderBy`/`Select` surface on the builder:

```csharp
internal sealed class BookingSpecification : SpecificationBuilder<BookingEntity>;
```

A caller composes the graph it needs without a class or a repository method per combination:

```csharp
var booking = await bookingRepository.GetByIdAsync(
    id,
    new BookingSpecification()
        .Include(booking => booking.Application.Artist.Genres)
        .Include(booking => booking.Application.Opportunity.Venue)
        .Include(booking => booking.Concert),
    ct);
```

An earlier draft of this plan specified a named per-graph vocabulary — `WithApplicationArtistGenres()`,
`WithApplicationOpportunityVenue()`, `WithConcert()` and the rest. That was rejected in implementation: it
needs a method per graph combination, which is the very duplication the Specification was introduced to
remove. `Include` expresses the same graphs with nothing to maintain. A named member is warranted only for a
projection genuinely reused by more than one lookup, and then it is an expression-bodied static on the module
Specification — never a field, because a builder instance is mutable and a shared one would accumulate
includes across callers.

`ThenInclude` continues the current path and is what a chain crossing a collection needs; a single
member-chain lambda covers a chain of reference navigations.

Fixed-filter methods such as `GetByConcertIdAsync` remain repository methods, and take a shape Specification
when a real caller-selected variation exists — as `GetByConcertIdAsync` and `GetSettlementByBookingIdAsync`
now do. Data access predicates, joins, tenant restrictions, and business-rule filtering do not move into
services. A fixed projection whose shape the repository owns — `GetOwnerByIdAsync`, the `ManagerConcertCard`
and `MonthlyPaymentTotal` projections, the aggregate queries — stays a named repository method; pushing its
navigation path out to a caller would leak schema knowledge into the service layer.

## Predicate algebra

`IPredicateSpecification<TEntity>` and `IPredicateSpecification<TEntity, TParameters>` do not inherit the
caller-facing `ISpecification` contracts. This separation is what makes it impossible to pass a predicate
Spec to the service-facing `GetByIdAsync` overload.

Keep optional `PredicateSpecification` bases for pure predicate leaves. A concrete infrastructure type that
genuinely has shape, projection, and predicate capabilities inherits the one appropriate `Specification`
base and implements the predicate interface; no combined abstract-class hierarchy is introduced.

`PredicateSpecificationExtensions` exposes `And`, `Or`, `Not`, `Via`, and `IsSatisfiedBy`. Boolean
composition returns explicit internal composite nodes rather than eagerly erasing the Specification tree
into an expression. Implementing `IPredicateSpecification<TEntity>` while containing predicate Spec
operands is the intentional Composite pattern. The nodes implement the interface directly rather than
inheriting a convenience base.

`And` and `Or` each take either another predicate Spec or a raw `Expression<Func<TEntity, bool>>`; the raw
overload lifts the lambda into an internal `ExpressionPredicateSpecification` leaf, so every operator is
symmetric and no overload forces an early `ToExpression()`. `ToExpression()` is the single terminal, called
once at the end of composition.

Parameterized composition uses the corresponding internal
`AndSpecification<TEntity, TParameters>`, `OrSpecification<TEntity, TParameters>`, and
`NotSpecification<TEntity, TParameters>` nodes. Extension overloads support two predicates sharing the same
runtime parameter type and a parameterized predicate combined with a nonparameterized predicate; callers do
not bind through a factory or introduce another public Specification arity.

`ToExpression()` recursively materializes the final expression through the existing parameter-rebinding
utilities. It must emit one parameter and no `InvocationExpression`, preserving EF Core translation.
`IsSatisfiedBy` is new behavior and belongs on the predicate extension surface so pure and multi-capability
implementations receive it equally. Parameterized predicates expose the equivalent overload.

Navigability becomes a universal operation on every predicate Specification. Remove
`INavigableSpecification` and `NavigablePredicateSpecification`; `Via` returns a `NavigationSpecification`
node — still an `IPredicateSpecification<TSource>` — that remains eligible for `And`, `Or`, and `Not`
before expression materialization. `Via` never materializes; substitution happens inside the node's own
`ToExpression()`. Its source type is inferred from an explicitly typed lambda parameter
(`spec.Via((ApplicationEntity a) => a.Opportunity)`) rather than an explicit type argument, because an
extension member's own type arguments cannot be supplied separately from its receiver's.

## Ordering contract

Include ordering in the initial breaking producer release so a later collection consumer does not require a
second Kernel/DataAccess package cutover.

`IOrderedSpecification<TEntity> : ISpecification<TEntity>` adds
`IReadOnlyList<SpecificationOrder<TEntity>> Orders`.
`IOrderedSpecification<TEntity, TResult> : ISpecification<TEntity, TResult>,
IOrderedSpecification<TEntity>` combines the ordered and projected contracts. Order sequence determines
`OrderBy` followed by `ThenBy`; `SpecificationOrderDirection` selects ascending or descending for each key.

Do not add an `OrderedSpecification` abstract-class family. `Specification<TEntity>` owns reusable protected
`OrderBy`, `OrderByDescending`, `ThenBy`, and `ThenByDescending` registration alongside its include state,
while implementing only the base shape contract. A concrete ordered Specification opts into the ordered
interface and exposes named fluent ordering vocabulary without reimplementing storage. This keeps ordering
composable with projection without creating bases for every capability combination.

The shared collection overloads explicitly accept ordered contracts and apply order metadata before
materialization. The evaluator exposes `Apply` (includes) and `ApplyOrders` separately rather than an
overload set keyed on the Specification interface, so no repository method can silently pick up or drop
ordering through overload resolution. The `GetByIdAsync` overloads do not apply ordering. Fixed repository list ordering remains
inside its named query, and Search's runtime sort parameters remain Query Objects rather than being
mechanically converted to caller Specifications. Paging is not added speculatively.

## Query Objects and the Search reclassification

Only Query Objects expose or return `IQueryable`. The reusable contracts live in
`Concertable.DataAccess.Infrastructure.Queries`; service-specific interfaces and implementations remain
internal to their service's Infrastructure project. Services receive materialized results. Query Objects may
inject predicate Specs and smaller Query Objects, then apply joins, predicates, geometry, ordering, paging,
and projection in one reusable pipeline.

The original plan assumed every Search Specification was an executable `IQueryable` transformation and
should therefore be renamed to `...Query`. Implementation found that most of them are not: search-term,
genre, geometry and the composed search rules are pure predicates, and sort produces order metadata. Only
the composed per-entity pipeline is genuinely executable. Search is therefore reclassified rather than
renamed wholesale:

| Concern | Final type | Kind |
|---|---|---|
| `INameSpecification<TEntity>` / `SearchTermSpecification<TEntity>` | `IPredicateSpecification<TEntity, string?>` | predicate Spec |
| `IGenreSpecification<TEntity>` / `GenreSpecification<TEntity>` | `IPredicateSpecification<TEntity, IGenreParams>` | predicate Spec |
| `IGeometrySpecification<TEntity>` / `GeometrySpecification<TEntity>` | `IPredicateSpecification<TEntity, IGeoParams>` | predicate Spec |
| `ISearchSpecification<TEntity>` / `SearchSpecification<TEntity>` / `VenueSearchSpecification` | `IPredicateSpecification<TEntity, SearchParams>` | predicate Spec composing the three leaves |
| `IConcertSearchSpecification` / `ConcertSearchSpecification` | `IPredicateSpecification<ConcertReadModel, SearchParams>` | predicate Spec adding the Concert posted/date/history/sold rules |
| `ISortSpecification<TEntity>` / `SortSpecification<TEntity>` / `ConcertSortSpecification` | `IReadOnlyList<SpecificationOrder<TEntity>> ToOrders(Sort?)` | order-metadata Spec |
| `IArtistSearchQuery` / `IVenueSearchQuery` / `IConcertSearchQuery` and their implementations | `IQuery<TEntity, SearchParams>` | the only executable pipeline |

Every one of these interfaces leaves Search Application and becomes an internal Search Infrastructure
contract. Fields and parameters holding a Specification end in `Spec` (`searchSpec`, `sortSpec`,
`geometrySpec`); fields holding a Query Object end in `Query` (`searchQuery`).

## Structural boundary enforcement

The boundary is enforced in code rather than left as guidance:

1. caller-facing and predicate Specification interfaces are unrelated;
2. shared and custom service-facing repository methods accept only the caller-facing contracts;
3. caller-facing Specification bases expose include/selector construction but no `.Where(...)`;
4. Query Object interfaces remain internal to Infrastructure;
5. architecture tests reject Application and service namespaces that depend on predicate Specification or
   Query Object contracts, while explicitly allowing repository, query, and infrastructure-Specification
   namespaces;
6. repository evaluators never discover hidden capabilities through runtime casts.

Predicate Specifications remain DI-friendly and are expected inside repositories and Query Objects:

```csharp
var allowedConcertSpec = endedAndBookedSpec
    .And(doorRevenueOutstandingSpec.Not());

query = query.Where(allowedConcertSpec.ToExpression());
```

## Published-package cutover

`Concertable.Kernel`, `Concertable.DataAccess.Application`, and
`Concertable.DataAccess.Infrastructure` are published packages. This refactor is breaking because the old
`ISpecification` meaning disappears, predicate inheritance changes, navigable types are removed, and shared
repository interfaces gain members. It therefore uses a producer publish followed by a generated consumer
sync rather than pretending the monorepo source graph can change atomically.

### Phase 1 — shared producer, and every in-repo consumer of it

- Replace the Kernel contract family with the final caller Specification and predicate families.
- Add explicit Boolean/navigation nodes, extension methods, parameterized predicate support, the
  `IncludePath`/`IIncludableSpecification` chain, order metadata, and the two Specification convenience bases.
- Add infrastructure Query Object contracts, the EF Specification evaluator, and both shared
  `GetByIdAsync` plus shape/projected/ordered `GetAllAsync` overloads to read and read/write repository bases.
- Migrate DataAccess's `UpcomingSpecification` and `DateRangeSpecification`; remove navigable inheritance
  and use `Via` at consumers. Remove `ExpressionSpecification` after explicit nodes cover every composition.
- Migrate every in-repo consumer in the same change, and reclassify Search per the table above. This is not
  the plan's original phase split: CI packs the platform from branch source through
  `scripts/local-platform.ps1` and restores services from that local feed, so the monorepo build is source
  coherent and a producer change that leaves consumers on the old API simply does not compile. The
  producer/consumer two-step is real only for the published feed, which pinned services adopt through the
  generated platform sync after this merges.
- Validate locally the way CI does: `./scripts/local-platform.ps1 prepare`, then `build`/`test` against that
  feed. A plain `dotnet build` of a service resolves the pinned published Kernel and reports the old API.
- Do not bump `ConcertablePlatformVersion` here; the generated sync PR owns that.

Consumption contract: repository callers hand `id + shapeSpec + CancellationToken` to the inherited ID
overloads, or one shape/projected/ordered Spec to the inherited collection overloads, and receive materialized
entities or projected results; infrastructure Query Objects receive an `IQueryable` and parameters and
return an unmaterialized query only to another infrastructure component.

Verification gate:

- Kernel predicate truth-table, `IsSatisfiedBy`, deep-composition, parameter-rebinding, navigation, and
  parameterized-predicate unit tests pass, including a navigated predicate that is still composed with `Or`
  and `And` before its single `ToExpression()`.
- A test proves a composed navigated tree materializes to one parameter and no `InvocationExpression`.
- Kernel unit tests prove an include chain records its steps through both a reference and a collection
  navigation, that fluent include vocabulary is additive, and that a projected Specification rejects includes.
- DataAccess integration tests against a relational provider prove plain ID loading stays unshaped, a
  reference chain and a collection `ThenInclude` chain each load their requested graph, an unrequested
  navigation stays unloaded, repeated fluent includes are idempotent, projection returns `TResult`, ordered
  collections preserve primary/secondary direction, ordered projection works, cancellation flows, and runtime
  predicate capabilities are ignored by the shape evaluator.
- The whole solution builds and every affected suite passes against the locally packed platform feed.

### Phase 2 — published consumer sync

- After the producer publishes, adopt the generated platform version in its platform-sync PR.
- Wait for [PR #633](https://github.com/Concertable/concertable/pull/633) to reach terminal before finalizing
  the B2B portion of that sync, then update the sync branch onto the terminal B2B source rather than migrating
  the superseded pre-carve repository topology.
- The Search reclassification and the B2B/Customer predicate migration land in Phase 1, so this phase is the
  version bump and its fallout only.
- Require package-mode service builds; no committed `UseLocalCore` setting or local feed survives.

Consumption contract: Search repositories inject internal Query Objects and materialize their returned
queries; Search Application and services never receive `IQueryable`.

Verification gate:

- Search query tests cover search-term, concert filters, geometry, ordering, and composed Query Objects.
- Kernel/DataAccess consumer grep gates find no old executable-Specification or navigable names outside an
  explicit package-compatibility allowlist.
- Search and every predicate consumer build against the published packages with service-boundary
  enforcement enabled.

### Phase 3 — Concert and Payment adoption

Delivered on `Refactor/RepositoryFinderSpecifications`, which stacks on the producer branch. It did not wait
for [PR #633](https://github.com/Concertable/concertable/pull/633): that PR touches the same Concert
repository files, so the adoption will need a merge from terminal main once #633 lands, but blocking on a
953-file refactor was a worse trade than resolving that merge.

- Module Specifications as empty `SpecificationBuilder<TEntity>` subclasses: `ConcertSpecification`,
  `BookingSpecification`, `ApplicationSpecification`, `OpportunitySpecification`, plus `EscrowSpecification`,
  `CommissionBindingSpecification` and `SettlementTransactionSpecification` in Payment.
- Every graph-named repository method removed — all eleven. `GetByIdWithArtistAndVenueAsync`,
  `GetByIdWithVenueAsync` and `GetByIdWithBookingAsync` collapse into the inherited
  `GetByIdAsync(id, spec, ct)`; `GetByConcertIdAsync` and `GetWithApplicationByConcertIdAsync` collapse into
  one spec-taking `GetByConcertIdAsync`; `GetSettlementWithRefundsByBookingIdAsync` becomes
  `GetSettlementByBookingIdAsync(bookingId, spec, ct)`.
- The nine interim class-per-graph Specifications from the branch's first pass are deleted; the fluent
  builder replaced them.
- Fixed repository projections stay where they are, per the rule above.
- Architecture rules prohibiting predicate and Query Object dependencies from service-facing namespaces are
  **not** written yet.

Consumption contract: a service may create only a module-owned shape/projection Specification and pass it to
an existing ID- or repository-owned-filter operation; it receives a materialized entity/result and cannot
supply a predicate.

Verification gate:

- Kernel unit tests prove include chains are additive, cross a collection through `ThenInclude`, and that
  `Select` carries orders while dropping includes.
- DataAccess integration tests against a relational provider prove the graph loads, an unrequested navigation
  stays unloaded, repeated includes are idempotent, a value projection returns null rather than default for a
  missing row, and a nullable column projects.
- Concert and Payment unit suites pass; the container-backed Concert, Payment and Search integration suites
  and the architecture tests have **not** run — the branch has no PR yet, so CI has never seen it.
- Still owed: a PR, its CI, and a review. The branch has 25 authored commits and no review work order at all.

### Phase 4 — known follow-up adoption

- Re-inventory graph-named methods after the B2B proof and migrate only genuine caller-selected graph cases,
  beginning with Payment's CommissionBinding and Escrow examples.
- Keep fixed graphs/projections in named repository methods and add no optional include Booleans.
- Complete the repository-wide old-name/GetWith grep and record deliberate fixed-query survivors.

Consumption contract: later services reuse the same shared shape/evaluator API; no service gains an
`IQueryable` or predicate parameter.

Verification gate:

- Each adopting service's focused repository integration tests and package-mode build pass.
- The final rename grep has zero obsolete executable-Specification/navigable identities, with any surviving
  `GetWith...` method explicitly justified as a fixed repository query rather than caller-selected shape.

## Definition of done

- Specification, predicate Specification, and Query Object have one unambiguous meaning each.
- Every agreed rename is complete across types, files, identifiers, tests, registrations, and documentation.
- Services can select only named shape/projection vocabulary and cannot pass predicate Specs to repository
  read overloads.
- The initial published contract includes ordered entity and projected collection Specifications, avoiding a
  second package cutover when the first caller-selected ordering use case arrives.
- Predicate `And`/`Or`/`Not`/`Via`/`IsSatisfiedBy` work for pure and multi-capability implementations,
  compose symmetrically over both a Spec and a raw lambda, keep `ToExpression()` as the single terminal, and
  remain EF-translatable.
- A caller-selected include chain can pass through a collection navigation, so the Phase 3 module vocabulary
  is one continuous typed chain and no later graph shape forces a second breaking package cutover.
- No caller-facing contract exposes `IQueryable`, EF Include APIs, arbitrary `.Where`, a shape Boolean, or a
  factory-style `Specification.For` entry point.
- Plain `GetByIdAsync(id, ct)` retains the shared unshaped behavior.
- Producer packages, generated platform sync, B2B proving ground, and selected follow-up adoption are merged
  and green against published package baselines.
