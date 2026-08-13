# Repository redesign — Cosmos-aligned facets + context-enforced no-tracking

Unify the DataAccess repository bases, make no-tracking **enforced by the context** (already shipped),
add `InsertAsync`, compose the shared read and write implementations behind `Repository`, and rename the
write-only facet `IBaseRepository`/`BaseRepository` → `IWriteRepository`/`WriteRepository`. The linchpin
enabling the published base-class changes is a **fix to the integration test seam** (a local platform-pack so consumers compile
against the source-built platform they run against) — see "The seam fix" below.

Branch/worktree: `Refactor/data-access_base-unify`, PR-B #530 (open). Read
[`plans/AGENTS.md`](../AGENTS.md) + [`plans/agents/PLAN.md`](../agents/PLAN.md) before executing.

Next steps live in @plans/data-access/REPOSITORY_REDESIGN_PROGRESS.md → `## Next Steps`.

---

## Why (the two problems)

1. **Duplicated CRUD.** `GetByIdAsync`/`GetAllAsync`/`Exists` are written in both `ReadRepository` and
   `Repository` (the latter via `BaseRepository` + re-declared read members). `IRepository` even carries
   a `new GetAllAsync` to resolve the GetAll diamond between `IReadRepository` and `IBaseRepository`.
2. **No-tracking is a bypassable convention.** The just-merged `Query => context.Set<T>().AsNoTracking()`
   root (#498 base / #503 consumers) is opt-in — a read repo can still call `context.Foo` and get a
   tracked query. Tracking is a *context* concern in EF; it should live there.

## Grounding facts (verified against `main` + the CosmosRepository source)

- **Current interfaces** (`Concertable.DataAccess.Application`): `IReadRepository<T,K>` = reads;
  `IBaseRepository<T>` = writes (+GetAll); `IRepository<T,K> : IBaseRepository<T>, IReadRepository<T,K>`
  with `new GetAllAsync`.
- **Current shared bases** (one file, `…Infrastructure/Repository.cs`): `BaseRepository<T,TContext>`
  implements writes, `ReadRepository<T,TContext,K>` implements reads, and the locally committed but
  rejected Phase 2 makes `Repository<T,TContext,K> : ReadRepository` and copies every write method.
- **Read-only context precedent:** `PublicDbContext : DbContextBase` composes the module's anemic
  `IEntityTypeConfigurationProvider` and **seals `SaveChanges`/`SaveChangesAsync` to throw**. It does
  **not** set `QueryTrackingBehavior` itself — no-tracking is applied at **DI** via
  `.UseQueryTrackingBehavior(NoTracking)` (B2B Venue/Artist/Concert public contexts).
- **Only Customer has concrete generic read repos** — `Concert`/`Venue`/`ArtistReadRepository`.
  Each directly inherits the Customer read base and DI supplies its matching
  `ConcertReadDbContext`/`VenueReadDbContext`/`ArtistReadDbContext`. Projection handlers separately use
  the tracked writable module context. B2B/Payment's dead `ReadRepository<T>` aliases are already gone.
- **Customer currently duplicates the generic read implementation.** Its package owns
  `IReadDbContext` plus a second `ReadRepository<TEntity>` implementation because the shared
  `ReadRepository<TEntity,TContext,TKey>` is tied to `DbContextBase`. The final design moves the
  queryable-only context contract to shared DataAccess so both tracked and read-only contexts can use
  the one shared read implementation.
- **CosmosRepository model** (`IEvangelist.Azure.CosmosRepository`, verified source): three facets —
  `IReadOnlyRepository<T>`, `IWriteOnlyRepository<T>`, and `IRepository<T> : IReadOnlyRepository<T>,
  IWriteOnlyRepository<T>, IBatchRepository<T>` (empty body, pure composition). Its `CreateAsync` = create
  **and** persist in one call — i.e. exactly our `InsertAsync`.

## Target design

**Naming:** keep the EF-idiomatic `IReadRepository`/`IRepository` (do NOT adopt Cosmos's
`IReadOnlyRepository`). **But DO rename the write-only facet** `IBaseRepository`/`BaseRepository` →
`IWriteRepository`/`WriteRepository`: "Base" is a dishonest name for what is purely the write-only side
(Add/AddRange/Insert/Update/Remove/SaveChanges, no reads, no key — Cosmos's `IWriteOnlyRepository`).

**Interfaces** — three facets:

```csharp
IReadRepository<T,K>                                        // GetById, GetAll, Exists
IWriteRepository<T>                                         // Add, AddRange, InsertAsync, Update, Remove, SaveChanges — write-only, keyless (renamed from IBaseRepository)
IRepository<T,K> : IWriteRepository<T>, IReadRepository<T,K> // no `new GetAllAsync`
```

The write-only facet is **kept, not folded away** (the plan's original "delete it" was wrong — it never
checked the keyless case): `SequenceRepository<TSequence>` operates on `ISequence` entities, which are
`ITenant` but deliberately **not** `IEntity<TKey>`, so they cannot satisfy `IReadRepository<T,K>`'s
`IEntity<TKey>` constraint and need a write-only, keyless base. `OpportunitySyncer` and
`CollectionSyncer<T>` also inject the write-only facet directly. The GetAll diamond is killed by
**removing `GetAllAsync` from the write facet** (no caller used it — verified).

**Classes — composition, not reparenting.** The two facet implementations own behavior once;
`Repository` is a flat-API facade which delegates to both. All three shared types use explicit
constructors — no primary constructors for captured state.

```csharp
IReadDbContext                         // IQueryable<T> Query<T>()
ReadRepository<T,K>                    // owns GetById/GetAll/Exists once
WriteRepository<T,TContext>            // owns Add/AddRange/Insert/Update/Remove/SaveChanges once
Repository<T,TContext,K>               // composes ReadRepository + WriteRepository; delegates the flat IRepository API
```

`DbContextBase` implements the shared `IReadDbContext` by returning `Set<TEntity>()` from `Query<TEntity>()`.
A combined `Repository<TEntity,TContext,TKey>` receives one tracked writable `TContext`, gives that same
scoped instance to its read and write components, and preserves one change tracker, transaction, and
`SaveChangesAsync` boundary. `TContext` is not claimed to be write-only; it is the context used by the
write facet and is also the correct tracked read context for a combined unit-of-work repository.

```csharp
public interface IReadDbContext
{
    IQueryable<TEntity> Query<TEntity>()
        where TEntity : class;
}

public abstract class DbContextBase : DbContext, IReadDbContext
{
    public IQueryable<TEntity> Query<TEntity>()
        where TEntity : class =>
        Set<TEntity>();
}
```

`DbSet<TEntity>` already implements `IQueryable<TEntity>`, so this is an implicit interface conversion;
do not add a redundant `.AsQueryable()` call. Do not introduce a `ReadDbContextView` wrapper either:
the `IReadDbContext`-typed field is the compile-time capability boundary, while dedicated
`ReadDbContext` implementations retain their sealed throwing `SaveChanges` overrides as the runtime
backstop. Because every repository-compatible context derives from `DbContextBase`, the shared
capability is implemented once rather than repeated in every module context.

```csharp
public Repository(TContext context)
{
    this.context = context;
    this.readRepository = new ReadRepository<TEntity, TKey>(context);
    this.writeRepository = new WriteRepository<TEntity, TContext>(context);
}
```

A dedicated read-only repository is a separate object graph and directly inherits the shared read
implementation. Customer Concert is the grounding example: `ConcertModule` consumes
`IConcertReadRepository`; `ConcertReadRepository` inherits `ReadRepository<ConcertEntity, int>`; DI passes
`ConcertReadDbContext` as `IReadDbContext`. It does not compose a writer and never receives
`ConcertDbContext`.

```csharp
internal sealed class ConcertReadRepository : ReadRepository<ConcertEntity, int>, IConcertReadRepository
{
    public ConcertReadRepository(IReadDbContext context)
        : base(context)
    {
    }
}
```

Do not silently combine a dedicated no-tracking context and a writable context behind one
`IRepository<TEntity>`: an entity read by the first is detached from the second, which breaks the
established read-mutate-save unit-of-work contract. Consumers that need only projections inject
`IReadRepository<TEntity>`; consumers that mutate aggregates inject `IRepository<TEntity>`.

**Context-architecture definition of done:** this phase is a full migration. Exactly one shared
`IReadDbContext`, one generic read implementation, and one generic write implementation remain;
Customer's duplicate context contract/read base are deleted; all dedicated read repositories use their
matching no-tracking `*ReadDbContext`; all combined repositories compose both facets over one tracked
module context; and architecture tests prevent duplicate abstractions or write capabilities from
returning to the read contract. No transitional parallel hierarchy is left for a later cleanup.

**`InsertAsync`** — on `IWriteRepository`, in the write repo: `AddAsync` + `SaveChangesAsync`, returns
the entity (Id populated). Faults propagate as exceptions (no bool). Cosmos calls this `CreateAsync`.

**Read contexts (the enforcement) — ✅ already shipped (PR-A/#526).** Customer read repos bind a
read-only, no-tracking `IReadDbContext`; `Query`/ad-hoc `.AsNoTracking()` are gone. No further work here.

## The seam fix — why it's the linchpin, and how

**The `FieldAccessException` is a test-harness artifact, not a real break.** In production every service
pins its own `<ConcertablePlatformVersion>` (per-service `Directory.Packages.props`, all at `0.955`) and
consumes DataAccess as a **PackageReference** (carve rule: `Deal.Infrastructure.csproj` — "never a
ProjectReference"); `platform-sync` **recompiles** each service against the new pin before the new base
ever reaches it. So compiled-version == runtime-version, always — a moved field or renamed type can never
dangle in prod.

The integration harness breaks that invariant: `Concertable.B2B.IntegrationTests.Fixtures` →
`Concertable.Shared/src/Seed/Concertable.Seed.Infrastructure` **→ source `ProjectReference` to
`Concertable.DataAccess.Infrastructure`**. That source DataAccess (higher MinVer) wins in the test output,
while the module repos (`Deal`/`Concert`/…) were compiled against the **feed** pin `0.955`. Source
platform + feed consumers = the false failure. It flags exactly the class of changes (reparent, rename)
that are production-safe, and it can never reach the platform-sync that would recompile consumers.

**Fix = validate PR-B's platform locally the way platform-sync validates it remotely: a local
platform-pack + pin override, so the integration build compiles AND runs every consumer against the ONE
source-built platform.** Carve-safe (consumers stay PackageReference; no source ProjectReference added to
production projects):

1. **Pack the source platform to a local feed.** `dotnet pack` every project that publishes a
   `Concertable.*` package referenced via `$(ConcertablePlatformVersion)` (enumerate them from the
   `<PackageVersion Include="Concertable.*" Version="$(ConcertablePlatformVersion)" />` lines in each
   service's `Directory.Packages.props` — DataAccess.Application/Infrastructure, Kernel, Contracts,
   Seed.*, Messaging.*, ServiceDefaults, Shared.*, …) into e.g. `artifacts/local-platform/`, all at a
   single `$(LocalPlatformVersion)` set **above** `0.955`. Use `-p:MinVerVersionOverride=$(LocalPlatformVersion)`
   so the pack **and** any source `ProjectReference` of those same projects (the Seed→DataAccess ref)
   build at the **same** version/assembly identity.
2. **Register the local feed** as a NuGet source with `packageSourceMapping` `Concertable.*` (keep the
   dependency-confusion guard) in a `nuget.config` on the integration build's path.
3. **Override the pin for the build under test:** `-p:ConcertablePlatformVersion=$(LocalPlatformVersion)`
   so every consumer restores the local-packed source platform.
4. **Consistency gate (the critical check):** exactly ONE `Concertable.DataAccess.Infrastructure.dll`
   (one version) may land in each integration test output folder. If the Seed source ProjectReference
   and the packed package resolve to two identities, the mismatch persists — that's the thing to verify,
   not assume.
5. **Wire it into CI and the local runner:** the merge-queue integration job (the one that produced run
   31636765379) and the local integration/e2e runner both do the pack + override as a pre-step.

**Unknowns the executor must resolve against the repo (do NOT assume):** the exact platform project set
to pack; the MinVer/`MinVerVersionOverride` interplay that guarantees single-assembly-identity; which CI
workflow runs the B2B/Customer integration suites and how it invokes build; whether the local feed goes
in a repo-root `nuget.config` or per-service. The local-pack is **test/CI-only** — it must not change
what the real `publish-packages` workflow emits, and the pin override must not be committed as a default.

## Execution plan (single PR — PR-B #530)

Everything below lands in the existing PR-B worktree/branch (`Refactor/data-access_base-unify`). Order
matters: the seam fix must precede the composition/rename or their builds/tests fail for the wrong reason.

- **Phase 1 — Seam fix. ✅ Complete.** Implement the local platform-pack + pin override (5 steps above). Land it
  first so Phases 2–3 are validated against a consistent platform.
- **Phase 2 — Compose the repository facets. ✅ Complete.** Replaced rejected local commit `d65293cc3`'s
  `Repository : ReadRepository` + copied writes with composition. Moved `IReadDbContext` to shared
  DataAccess, made `DbContextBase` implement it, centralized reads in `ReadRepository<TEntity,TKey>`,
  centralized writes in `BaseRepository<TEntity,TContext>`, and made `Repository<TEntity,TContext,TKey>`
  delegate its existing flat API to both using the same scoped `TContext`. Rebound Customer's dedicated
  read repositories to the shared read implementation and their existing read-only contexts. Preserved
  `IRepository<TEntity>`/`IReadRepository<TEntity>` consumer APIs, custom repository overrides, and the
  protected writable `context` used by module-specific queries. The 6 historical proof suites (B2B
  Artist/Concert/User/Venue, Customer User/Concert) are green.
- **Phase 3 — Rename `IBaseRepository`/`BaseRepository` → `IWriteRepository`/`WriteRepository`.** Full
  **grep-gate** rename (see [`plans/agents/PLAN.md`](../agents/PLAN.md) "grep gate"):
  `grep -rniE "ibaserepository|baserepository"` over the whole repo returns **zero**, every tier/casing —
  type names, the keyless `BaseRepository<T>` module alias behind `SequenceRepository`, every module's
  `Repository`/`TenantScopedRepository`, `OpportunitySyncer`/`CollectionSyncer`, DI registrations,
  identifiers (`baseRepository`→`writeRepository`), comments, docs. Allowlist: the historical mentions in
  this plan + its `_PROGRESS.md` (they narrate the old name) — update or list them explicitly, nothing
  else. Consumers compile because Phase 1 makes them build against the renamed source platform.
- **Phase 4 — Verify (build + Docker integration).** Pack locally, then
  `dotnet build api/Concertable.slnx -p:ConcertablePlatformVersion=$(LocalPlatformVersion)` → 0 errors.
  Run the integration suites via the `e2e-*` skills (mandatory `docker-health.ps1` pre-flight) → the 6
  formerly-red suites + all integration green; unit green. A red suite → the matching debug skill, not a
  status report.
- **Phase 5 — Deliver.** Push; the merge queue runs build + unit + integration (merge Step 4 tier: no
  positive E2E trigger → `skip-e2e`). On merge → `publish-packages` emits the renamed DataAccess →
  `platform-sync` bumps pins; because every consumer was migrated in-PR (grep gate = zero old refs), the
  `chore/platform-sync-*` PR builds green — **own it to merged**. Then close out per
  [`plans/AGENTS.md`](../AGENTS.md) (move recovery state to a `Docs/*_closeout` worktree, prune this one).

## Open decisions / risks

- **Facade forwarding is acceptable; behavior duplication is not.** `Repository` keeps one-line
  delegates so existing flat `IRepository<TEntity>` call sites remain unchanged. All EF read/write
  behavior lives only in the composed facet implementations.
- **Combined and read-only repositories have different context semantics.** A combined repository uses
  one tracked module context for both facets to preserve the unit of work. A dedicated read-only
  repository directly inherits `ReadRepository` and receives its module's no-tracking `ReadDbContext`.
- **`InsertAsync` adoption is opportunistic** — new/changed call sites use it; not a forced migration of
  every existing `AddAsync` + `SaveChangesAsync`.
- **B2B/Payment dead `ReadRepository<T>` aliases** — unused (no concrete subclasses); already deleted in
  the current PR-B diff. Keep them gone.
- **Local-pack scope creep** — packing the whole platform is safest but slower; if pack time hurts CI,
  narrow to the transitive platform closure the integration graph actually restores (still derived from
  Directory.Packages.props, not guessed).
