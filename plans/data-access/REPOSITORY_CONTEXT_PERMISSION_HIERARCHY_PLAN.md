# Repository and DbContext permission hierarchy

Consumer next steps live in
@plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PROGRESS.md -> `## Next Steps`.
Additive-producer next steps live in
@plans/data-access/REPOSITORY_CONTEXT_PERMISSION_HIERARCHY_PRODUCER_PROGRESS.md -> `## Next Steps`.

## Objective

Replace the asymmetric repository implementation and partially implicit `DbContext` permissions with a
single, mechanically consistent EF Core hierarchy:

```text
IReadDbContext  -> IReadRepository  -> ReadRepository
IWriteDbContext -> IWriteRepository -> WriteRepository
IDbContext      -> IRepository      -> Repository
```

Each repository implementation depends only on the matching context capability. Concrete module
constructors select the exact EF context. Dedicated read contexts remain separate EF instances so their
no-tracking and no-save guarantees cannot be weakened by a tracked projection or command context.

For B2B Artist, Venue, and Concert, the two concrete contexts also encode different tenancy stances.
`XDbContext` is the module's normal tracked, writable context carrying `ITenantContext` and selective
tenant query filters; `XReadDbContext` is the tenant-independent read-only context. The unqualified name
belongs to the normal aggregate unit of work, while `Read` names the alternate enforced capability.

The resulting shared API should be small and application-agnostic enough to extract into a reusable EF
Core package later. This plan makes the seam extraction-ready; it does not split packages or detach the
current `Concertable.Kernel`/messaging dependencies.

## Current state and problem

The public repository interfaces already have the right composition:

```text
IRepository<TEntity, TKey>
  : IReadRepository<TEntity, TKey>
  , IWriteRepository<TEntity>
```

The implementations do not mirror it cleanly:

- `ReadRepository<TEntity, TKey>` depends on `IReadDbContext`.
- `WriteRepository<TEntity, TContext>` depends on a concrete generic `DbContextBase` subtype.
- `Repository<TEntity, TContext, TKey>` constructs private `ReadFacet` and `WriteFacet` subclasses and
  stores the concrete facet types.
- `DbContextBase` implements `IReadDbContext`, but there is no matching write or combined context
  capability.
- Customer `Artist`/`Venue`/`Concert` correctly have two physical contexts each: `XReadDbContext` for
  customer queries and `XDbContext` for event-driven projection maintenance. They must not be collapsed.
- Customer `ReadDbContext` and B2B `PublicDbContext` duplicate generic schema/configuration-provider
  plumbing inside service-specific intermediary bases.
- The previous attempt to reparent the published `Repository` base atomically broke feed-compiled
  consumers at runtime. This redesign must expand the package first, migrate every consumer against the
  published expansion, and only then remove the legacy types.

## Locked design

### Context capabilities

`Concertable.DataAccess.Application` owns three interfaces:

```csharp
public interface IReadDbContext
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;
}

public interface IWriteDbContext
{
    Task AddAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        where TEntity : class;

    Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default)
        where TEntity : class;

    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IDbContext : IReadDbContext, IWriteDbContext;
```

The write interface exposes mutation and persistence operations, not `DbSet<TEntity>`, `Entry`,
`Database`, or `ChangeTracker`. Those would leak unrestricted EF access through the supposedly generic
permission seam. `DbContextBase` implements the mutation members explicitly where their signatures
overlap EF methods with different return types.

`IDbContext` is the exact context counterpart of `IRepository`: both are the composition of read and
write capabilities. Do not add `IRepositoryDbContext`, `IReadWriteDbContext`, command-service types, or
CQRS dispatch abstractions.

### EF context classes

`Concertable.DataAccess.Infrastructure` owns two independent abstract EF bases:

```text
Microsoft.EntityFrameworkCore.DbContext
|- ReadDbContext : IReadDbContext
`- DbContextBase : IDbContext
```

`DbContextBase` keeps the `Base` suffix because `DbContext` is already the EF Core type. It remains the
tracked, writable Concertable base and retains the existing inbox/outbox model integration. It adds the
explicit `IWriteDbContext` implementation and implements `IDbContext`.

The shared `ReadDbContext` derives directly from EF Core `DbContext`, implements only
`IReadDbContext`, composes the supplied module configuration provider and default schema, sets
`QueryTrackingBehavior.NoTracking` itself, and seals every synchronous and asynchronous `SaveChanges`
overload to throw. It does not inherit `DbContextBase` and therefore does not acquire `IDbContext`,
messaging mutation helpers, or the inbox/outbox model.

There are no generic service-specific read-context bases. Customer and B2B `ArtistReadDbContext`,
`VenueReadDbContext`, and `ConcertReadDbContext` derive the shared `ReadDbContext` directly and supply
their module configuration provider and schema.

- B2B `ArtistDbContext`, `VenueDbContext`, and `ConcertDbContext` compose their module
  configuration with `TenantScopedDbContext`/`VenueArtistTenantScopedDbContext`, remain writable stances over
  `DbContextBase`, and therefore implement `IDbContext`.
- `VenueAdminDbContext` remains the explicit tenant-independent tracked/write stance required by
  platform administration.

No `WriteDbContext` class is introduced: there is no physical write-only EF context today. A full
`DbContextBase` instance is exposed to `WriteRepository` through the narrower `IWriteDbContext` view.

### B2B tenancy stances and naming

Artist, Venue, and Concert each keep exactly two normal runtime contexts:

| Module | Tenant-bound tracked/write stance | Tenant-independent read-only stance | Additional stance |
|---|---|---|---|
| Artist | `ArtistDbContext` | `ArtistReadDbContext` | None |
| Venue | `VenueDbContext` | `VenueReadDbContext` | `VenueAdminDbContext` for tenant-independent administrative writes |
| Concert | `ConcertDbContext` | `ConcertReadDbContext` | None |

Their invariants are:

- `XReadDbContext` has no `ITenantContext` dependency, composes the full module configuration provider,
  applies no active-tenant query filters, defaults every query to no-tracking, implements only
  `IReadDbContext`, rejects every save overload, and excludes inbox/outbox configuration.
- `XDbContext` requires the scoped `ITenantContext`, composes the same module configuration plus
  the module-declared tenant filters, uses normal tracking, implements `IDbContext`, participates in the
  module unit of work and inbox/outbox behavior, and owns design-time migrations.
- `VenueAdminDbContext` composes the Venue model without active-tenant filters but remains tracked and
  writable. Its `Admin` qualifier describes authorization/use-case stance; it is not another spelling
  of tenant-independent reads.

The tenant context is not accurately described as filtering its entire model. Its selective filters
are exactly:

- Artist: `ArtistEntity`.
- Venue: `VenueEntity` and `VenueImageEntity`.
- Concert: `ApplicationEntity`, `BookingEntity`, `ContractEntity`, `InvoiceEntity`, and
  `SelfBillingAgreementEntity`.

Other configured entities remain unfiltered unless their module explicitly adds a filter. The
tenant-independent context therefore means "no active-tenant restriction", not "public data" and not
"a larger model".

Keep the stances as separate concrete EF types. Their model metadata, scoped dependencies, tracking,
save permissions, interceptors, and messaging configuration differ. A runtime mode flag would require
mode-aware EF model-cache keys and would make the write boundary conditional. `IgnoreQueryFilters`,
service location, and runtime stance switching are forbidden. DI registers both concrete types
explicitly; no unkeyed capability registration chooses between them. Renaming the migration-owning
context updates its design-time factory and `[DbContext]` metadata but does not create or re-scaffold a
schema migration.

The tenant-independent read context intentionally composes the full module model. Marketplace projections
and internal cross-tenant facts may share that physical read context; repository contracts, DTOs, and
module services determine which data may leave the module. A separate restricted context is warranted
only if a genuinely distinct read model/configuration is introduced. Audience names such as `Public`
do not create another persistence stance: `XReadDbContext` already expresses tenant-independent,
structurally read-only access.

### Repository contracts and implementations

The existing interface hierarchy remains:

```text
IReadRepository<TEntity, TKey>
IWriteRepository<TEntity>
IRepository<TEntity, TKey>
  : IReadRepository<TEntity, TKey>
  , IWriteRepository<TEntity>
```

The implementation hierarchy becomes three independent classes:

```csharp
public abstract class ReadRepository<TEntity, TKey>
    : IReadRepository<TEntity, TKey>
{
    protected IReadDbContext Context { get; }
}

public abstract class WriteRepository<TEntity>
    : IWriteRepository<TEntity>
{
    protected IWriteDbContext Context { get; }
}

public abstract class Repository<TEntity, TKey>
    : IRepository<TEntity, TKey>
{
    protected IDbContext Context { get; }
}
```

There is no inheritance between the implementation classes, no private facet subclasses, and no
implementation-to-implementation fields. `Repository` implements both read and write methods directly.
The few generic CRUD one-liners intentionally exist in the standalone and combined implementations;
that transparent duplication is smaller and safer than inheritance or composition machinery created
only to share them.

The reusable bases do not carry `TContext`. A concrete module repository binds its exact context in its
constructor and passes it to the matching capability constructor:

```csharp
internal sealed class ArtistReadRepository(ArtistReadDbContext context)
    : ReadRepository<ArtistEntity, int>(context), IArtistReadRepository;

internal sealed class ConcertRepository(ConcertDbContext context)
    : Repository<ConcertEntity, int>(context), IConcertRepository;
```

Each shared base exposes only its matching capability to subclasses through the protected `Context`
property. Specialized repositories can therefore compose domain queries without retaining the same
context twice. A custom repository that requires typed sets or EF-specific members beyond the
capability retains its concrete context in its own private field. This prevents a reusable base from
leaking every application's concrete context type while keeping advanced EF operations available where
they actually belong.

`ReadRepository<TEntity, TKey>` already has its final arity, so expansion adds the protected `Context`
property while temporarily retaining its published protected `context` field. Phase 2 migrates all
derived source to `Context`; Phase 3 removes the field only after source and package-consumer grep gates
are clean. The new write and combined arities need no equivalent member shim because they are new types.

Module-local `Repository<TEntity>` aliases may remain to fix the module's default key type, but their
constructor accepts the concrete module context and forwards it to `Repository<TEntity, TKey>`. Remove
unused module-local `WriteRepository<TEntity>` aliases; keep a local alias only where a real standalone
write repository derives from it.

### Tracking and unit of work

- `ReadRepository` runs on a dedicated `ReadDbContext`; its queries are no-tracking by context
  construction and it has no write API.
- `Repository` runs on a tracked `DbContextBase`; entities returned by its read methods participate in
  the same scoped unit of work as its mutations and `SaveChangesAsync`.
- `WriteRepository` stages or persists through `IWriteDbContext` and exposes no repository reads.
- `InsertAsync` remains add-plus-save; `AddAsync`/`AddRangeAsync` remain staging operations.
- `IUnitOfWork<TContext>` and `UnitOfWork<TContext>` retain their exact context generic. Transactions and
  EF execution strategies require the concrete `DbContextBase`; unit-of-work identity is not a
  repository permission surface.
- Dedicated read and tracked contexts remain distinct scoped EF instances even when they use the same
  connection string and tables. This preserves isolation from unsaved tracked changes and allows future
  read-replica routing without changing repositories.

### Dependency injection

Concrete constructors encode the context binding, and ordinary interface-to-implementation
registrations replace repository factory lambdas:

```csharp
services.AddScoped<IArtistReadRepository, ArtistReadRepository>();
services.AddScoped<IConcertRepository, ConcertRepository>();
```

Do not register a service-wide unkeyed `IReadDbContext`, `IWriteDbContext`, or `IDbContext`: every
service contains several module contexts and the final registration would silently win. DI resolves the
concrete context requested by the module repository; the base constructor narrows it to the capability.

## Service migration matrix

### Customer

| Modules | Context stance | Repository action |
|---|---|---|
| Artist, Venue, Concert | Keep `XReadDbContext` read-only and `XDbContext` tracked/writable for integration-event projections | Bind `XReadRepository` to `XReadDbContext`; keep projection handlers and their unit of work on `XDbContext`; never expose a Customer write repository for the replicated aggregate |
| Preference | `PreferenceDbContext : DbContextBase` (`IDbContext`) | Migrate the combined repository to `Repository<TEntity, int>` |
| Ticket | `TicketDbContext : DbContextBase` (`IDbContext`) | Migrate `TicketRepository` to the combined base; retain standalone write capability only where a real consumer requires it |
| Review | `ReviewDbContext : DbContextBase` (`IDbContext`) | Keep its aggregate-specific repositories; no forced generic-base conversion |
| User | `UserDbContext : DbContextBase` (`IDbContext`) | Keep its custom repository/event paths; no forced generic-base conversion |

Customer `XReadDbContext` names remain necessary because a separately tracked `XDbContext` exists for
projection maintenance. Event-driven projection writes do not grant Customer command ownership of B2B
Artist, Venue, or Concert data.

### B2B

| Context/repository stance | Action |
|---|---|
| `ArtistDbContext`, `VenueDbContext`, `ConcertDbContext` | Continue as the normal full `IDbContext` contexts with `ITenantContext`, selective filters, unit-of-work behavior, and migration ownership |
| `ArtistReadDbContext`, `VenueReadDbContext`, `ConcertReadDbContext` | Use the shared `ReadDbContext` for tenant-independent access with no tracking and save rejection |
| `DealDbContext`, `TenantDbContext`, `UserDbContext` | Continue as full `IDbContext` contexts; migrate standard and tenant repository bases to the new context-free arities |
| `VenueAdminDbContext` | Continue as full `IDbContext`; `VenueAdminRepository` remains combined read/write |
| `TenantScopedRepository` and `VenueArtistTenantScopedRepository` | Drop `TContext`; receive `IDbContext`; use protected generic query/mutation capabilities |
| `OpportunityRepository`/`OpportunityReadRepository` | Stop sharing a combined tenant repository base across tenant and marketplace contracts; share the active-opportunity predicate as a module-local query extension and keep the read implementation read-only |
| `ArtistReadRepository`/`VenueReadRepository` | Retain only marketplace-safe summary/details/genre operations; remove organisation-identity records, facade methods, and lookup abstractions because Tenant already owns the canonical business identity |
| Conversations sender display | Maintain a Conversations-owned `ParticipantProfile` projection keyed by `TenantId`, fed by `ArtistChangedEvent` and `VenueChangedEvent`; add `TenantId` to the Venue event additively and never synchronously query Artist or Venue for response rendering |
| Escrow payment booking lookup | Keep `GetApplicationIdByIdAsync` on `IBookingRepository`; remove the duplicate `IBookingExistence` abstraction and handle a missing event reference without throwing at the integration-event boundary |
| `ConcertReadRepository`/`OpportunityReadRepository` | Use `Read` because persistence names the capability; marketplace audience remains an API-contract concern |
| `SequenceRepository` | Stop inheriting `WriteRepository`: its contract is an allocator rather than `IWriteRepository`; implement it directly with `ConcertDbContext` for its read-before-stage algorithm |
| Conversations and bespoke dashboard repositories | Keep their domain-specific implementations; adopt capability bases only when their contract matches |

### Payment, Auth, Search, and Messaging

- Payment's `PaymentDbContext` remains full `IDbContext`. Standard repositories migrate to the new
  combined base. Repositories using `Entry`, bulk operations, or typed sets retain `PaymentDbContext`
  privately.
- Auth's `AuthDbContext` inherits `IDbContext` through `DbContextBase`, but Auth is not forced onto a
  generic repository where direct aggregate-specific persistence is clearer.
- Search's `SearchDbContext` remains full because projection handlers write it. Its specialized
  `ISearchDbContext` query view and header/autocomplete repositories remain separate; they are not
  replaced by entity CRUD repositories.
- Messaging `InboxDbContext`/`OutboxDbContext` and Duende's persisted-grant context remain direct EF or
  framework contexts. They are infrastructure stores, not domain repository contexts, and stay outside
  this hierarchy.

After unused module aliases and the mismatched Sequence inheritance are removed, Concertable may have
no concrete write-only repository. `IWriteRepository<TEntity>` and `WriteRepository<TEntity>` still
remain a complete, tested first-class pair for a real write-only contract; they are not deleted merely
because current modules consume writes through combined repositories.

## Reusability boundary

The target API is reusable because repository implementations depend on capability interfaces rather
than application context types, and context capabilities expose no module entities. Extraction into a
standalone package remains a separate decision because current packages still depend on
`Concertable.Kernel.IEntity<TKey>` and `DbContextBase` carries Concertable messaging integration.

Before any extraction, measure these remaining couplings and choose whether to move the entity marker
and messaging base into narrower packages. Do not broaden this migration into a speculative package
split; first land and exercise the permission hierarchy inside Concertable.

## EF Core basis

The context split follows EF Core's own behavior rather than treating the repositories as CQRS
dispatchers:

- [Tracking versus no-tracking queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)
  recommends no-tracking queries for read-only use and supports setting the default at context level.
- [Change tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/) states that query and
  update work best on the same `DbContext`; that is why the combined repository retains one tracked
  `IDbContext` instead of composing independent read/write objects.
- [DbContext lifetime and configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
  defines a `DbContext` as a short-lived unit of work and warns that it is not thread-safe; all three
  repository capabilities therefore keep the existing scoped context lifetime.
- EF Core exposes virtual synchronous and asynchronous `SaveChanges` overloads, allowing the shared
  read context to make save rejection a context-level boundary rather than a repository convention.
- [Dynamic models](https://learn.microsoft.com/en-us/ef/core/modeling/dynamic-model) documents that EF
  assumes one model per concrete context type unless a custom `IModelCacheKeyFactory` is supplied. The
  B2B stance split keeps that model identity structural and avoids a runtime tenancy-mode cache key.

## Implementation DAG

1. Add the new context interfaces, shared read context, new repository arities, and the additive
   `ReadRepository.Context` property alongside the legacy public types and field. Move generic read
   context plumbing into DataAccess and derive the six concrete Customer/B2B read contexts directly.
   Add contract and behavior tests.
2. After the additive package is available as an exact artifact, migrate every service consumer and
   context stance. Customer, B2B, and Payment migrations are independently implementable against that
   artifact but form one coordinated consumer checkpoint before contraction.
3. After repository/context grep gates prove no consumer uses the legacy arities or facets, remove the
   legacy public implementation types and stale module aliases.

## Delivery DAG

The published topology has one package layer: `Concertable.DataAccess.Application` and
`Concertable.DataAccess.Infrastructure` republish together. B2B, Customer, and Payment are direct
consumers of the new capability/repository surface; no consumer package re-exposes those types into a
further service layer. The cut-over therefore requires exactly three feature merges, with the normal
publication and generated platform-sync gate after each merge:

1. Merge the additive shared-package PR; wait for package publication and its platform-sync PR to merge
   green.
2. Build and merge the repo-wide consumer migration against that published platform version; follow its
   package publication/platform-sync result to green.
3. Merge the shared-package contraction that removes legacy arities/facets; follow publication and
   platform sync to green. Every consumer is already on the new surface before this breaking package is
   produced.
4. Revalidate all standalone service carves against the contracted published baseline, then close the
   plan and roadmap item.

This sequence is mandatory. Do not retry the previous atomic base reparent: feed-compiled consumers can
load the source-built shared assembly during CI, so a dangling inherited field fails before a
platform-sync can recompile them.

## Phases

### Phase 1 - Additive shared permission surface

- Add `IWriteDbContext` and `IDbContext` beside `IReadDbContext`.
- Make `DbContextBase` implement `IDbContext` and its mutation members explicitly.
- Add the shared `ReadDbContext` with configuration-provider/default-schema composition, built-in
  no-tracking, and sealed save rejection.
- Delete Customer's generic `ReadDbContext` and B2B's generic `PublicDbContext`; derive each concrete
  module read context from the shared DataAccess base directly.
- Rename B2B's tenant-independent concrete contexts to `ArtistReadDbContext`, `VenueReadDbContext`, and
  `ConcertReadDbContext`; retain `ArtistDbContext`, `VenueDbContext`, and `ConcertDbContext` for the
  normal tenant-aware tracked/write stance; rename the administrative exception to
  `VenueAdminDbContext`. Update DI, unit-of-work/interceptor bindings, design-time factories, migration
  metadata, repositories, services, tests, and documentation without re-scaffolding schema migrations.
- Separate genuine marketplace repository contracts from internal cross-tenant facts: remove the
  redundant Artist/Venue organisation-identity lookup path, add the Conversations-owned participant
  projection, and rename the booking-existence contract/implementation without `Public`.
- Split `OpportunityReadRepository` from the writable generic opportunity repository; share only the
  active-opportunity query predicate between the public read-only and regular writable implementations.
- Add `WriteRepository<TEntity>` and `Repository<TEntity, TKey>` beside the legacy generic-arity
  implementations; keep `ReadRepository<TEntity, TKey>` on `IReadDbContext`.
- Add `ReadRepository.Context` without removing its published protected `context` field.
- Add public-surface and focused EF behavior tests without modifying consumers.

Verification gate: smallest shared DataAccess builds and focused DataAccess unit tests; exact-head draft
PR CI owns the full build/carves/unit/integration matrix.

### Phase 2 - Migrate every context and repository consumer

- Migrate Customer read/full repository pairs and DI registrations without collapsing the physical
  contexts.
- Migrate B2B tenant/admin and tenant-independent repository paths; the tenant-independent contexts
  already use the shared read context from Phase 1.
- Correct the standalone Sequence write path.
- Migrate Payment standard repositories and retain concrete contexts only in bespoke implementations
  that require EF-specific APIs.
- Classify Auth, Search, Review, User, Conversations, Messaging, and framework contexts according to the
  matrix; do not force unsuitable generic repositories.
- Remove dead module-local aliases and repository factory lambdas.

Verification gate: affected service builds and focused repository/context unit tests locally; exact-head
draft PR CI owns standalone carves and full unit/integration coverage.

### Phase 3 - Contract the published API

- Prove zero source consumers of `WriteRepository<TEntity, TContext>`,
  `Repository<TEntity, TContext, TKey>`, `ReadFacet`, `WriteFacet`, and the protected
  `ReadRepository.context` compatibility field.
- Remove the legacy implementation arities, protected read-context field, and facet-composition tests.
- Update DataAccess and B2B convention documentation to the final permission hierarchy.
- Publish the contracted package and follow the generated platform-sync PR to green.

Verification gate: grep/invariant checks, shared package build and focused tests locally; exact-head PR CI
and the post-publication platform sync prove the contracted baseline.

### Phase 4 - Published-baseline closeout

- Rebuild all service carves against the final published platform version through remote CI.
- Confirm no red platform sync, no temporary compatibility types, and no stale package pin.
- Record whether the stabilized API is ready for a separate extraction plan; do not leave extraction
  work hidden in this completed hierarchy plan.
- Reconcile the roadmap, then delete this plan and ledger under the normal lifecycle.

## Verification invariants

- `IDbContext` inherits exactly `IReadDbContext` and `IWriteDbContext`.
- Each repository implementation implements exactly its corresponding repository interface.
- Shared repository implementations contain no concrete `DbContext` generic parameter, private facet
  implementation, or protected concrete EF context.
- `ReadDbContext` is no-tracking without DI configuration and every save overload throws.
- No service owns an intermediate generic read-context base; concrete module read contexts derive the
  shared DataAccess `ReadDbContext` directly.
- A read-only context model does not include inbox/outbox entities solely by inheriting the shared base.
- A dedicated read context cannot observe unsaved tracked changes from the paired full context.
- A combined repository read and subsequent save share one tracked context instance.
- Customer Artist/Venue/Concert application registrations expose read repositories only; their full
  contexts remain reachable by projection handlers, seed/test infrastructure, and unit-of-work code.
- B2B `XReadDbContext` contexts are tenant-independent, no-tracking, and read-only at the EF boundary;
  unqualified `XDbContext` types are the normal tracked/write contexts with explicitly declared tenant filters.
- No B2B `PublicXDbContext`, `GlobalXDbContext`, or audience-named read context remains.
- No B2B repository or context uses `Public` as a persistence stance; no `OrgIdentity` record, lookup
  abstraction, or Artist/Venue facade method remains.
- Conversations renders participant details from its local event-fed projection and has no synchronous
  Artist/Venue module dependency for sender identity.
- Final whole-repository grep has no legacy repository arities, facet types, stale factory registrations,
  or obsolete context inheritance.
- Every published transition ends with a green platform-sync PR and builds against the real feed version.

## Rejected designs

- Do not add `TContext` to `ReadRepository`; concrete module constructors already bind the exact read
  context and the reusable base needs only `IReadDbContext`.
- Do not collapse Customer read and projection contexts; they have different tracking/write semantics.
- Do not make `Repository` inherit either implementation base or construct nested facet subclasses.
- Do not introduce multiple concrete inheritance, command services, provider switching, or CQRS routing.
- Do not register unkeyed context capability interfaces globally in a service with multiple contexts.
- Do not collapse B2B's tenant and tenant-independent stances into one context through
  `IgnoreQueryFilters`, a runtime mode switch, service location, or a custom model-cache discriminator.
- Do not use `Public` or `Global` as a synonym for tenant-independent persistence access.
- Do not introduce another audience-specific B2B read context alongside `XReadDbContext` unless it owns
  a genuinely distinct restricted model or projection; `IReadDbContext` is already the capability boundary.
- Do not model Artist or Venue as organisation identities. Tenant is the canonical business identity;
  consumers that need profile display data own event-fed projections.
- Do not preserve the legacy public types as permanent shims after all consumers migrate.
- Do not split a future general-purpose package during this cutover; stabilize and measure first.

## Definition of done

- The target context and repository permission hierarchies are the only public shared shapes.
- Every applicable service context and repository is classified and migrated according to the matrix.
- All read-only stances enforce no tracking and reject saves; all combined repositories retain tracked
  unit-of-work behavior.
- The legacy facet implementation and context-generic shared repository arities are absent repo-wide.
- Additive publication, consumer migration, contraction publication, and both platform-sync gates are
  terminal and green.
- Review findings are resolved, the roadmap item is checked, and the plan/ledger are deleted only after
  final published-baseline evidence is recorded.
