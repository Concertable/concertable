# Repository redesign — Cosmos-aligned facets + context-enforced no-tracking

Unify the DataAccess repository bases, make no-tracking **enforced by the context** (already shipped),
add `InsertAsync`, reparent `Repository` onto `ReadRepository` (reads once), and rename the write-only
facet `IBaseRepository`/`BaseRepository` → `IWriteRepository`/`WriteRepository`. The linchpin enabling
the last two is a **fix to the integration test seam** (a local platform-pack so consumers compile
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
- **Current bases** (one file, `…Infrastructure/Repository.cs`): `BaseRepository<T,TContext>` (writes +
  GetAll), `ReadRepository<T,TContext,K>` (reads, + the new `Query` root), `Repository<T,TContext,K> :
  BaseRepository` (re-declares GetById/Exists). All constrain `TContext : DbContextBase`.
- **Read-only context precedent:** `PublicDbContext : DbContextBase` composes the module's anemic
  `IEntityTypeConfigurationProvider` and **seals `SaveChanges`/`SaveChangesAsync` to throw**. It does
  **not** set `QueryTrackingBehavior` itself — no-tracking is applied at **DI** via
  `.UseQueryTrackingBehavior(NoTracking)` (B2B Venue/Artist/Concert public contexts).
- **Only Customer has concrete read repos** — `Concert`/`Venue`/`Artist`ReadRepository. B2B/Payment
  declare the `ReadRepository<T>` alias but have **zero** concrete subclasses (dead aliases).
- **Customer `ConcertDbContext`** derives straight from `DbContextBase` (no tenancy/public split), is
  registered **tracked** (no `UseQueryTrackingBehavior`), and is shared by the read repo **and** the
  projection handlers (which need tracking to fetch-then-update read models).
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

**Classes** — `Repository` reparents onto `ReadRepository` (reads defined once):

```csharp
ReadRepository<T,TContext,K> : IReadRepository<T,K>                       // GetById/GetAll/Exists over context.Set<T>()
WriteRepository<T,TContext>  : IWriteRepository<T>                        // writes + InsertAsync, keyless (renamed from BaseRepository)
Repository<T,TContext,K> : ReadRepository<T,TContext,K>, IRepository<T,K> // inherits reads; adds writes + InsertAsync
```

Single inheritance means one facet's members are duplicated on `Repository`; we duplicate the **trivial
writes** (keeping reads defined once), and log it in `TECH_DEBT.md`. Reads-once was the original goal;
the only thing that ever blocked it — the reparent moving the inherited `context` field off the write
base, which the integration seam turned into a `FieldAccessException` (run 31636765379) — is removed by
the **seam fix below**, so the reparent is restored.

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
matters: the seam fix must precede the reparent/rename or their builds/tests fail for the wrong reason.

- **Phase 1 — Seam fix. ✅ Complete.** Implement the local platform-pack + pin override (5 steps above). Land it
  first so Phases 2–3 are validated against a consistent platform.
- **Phase 2 — Restore the reparent. ✅ Complete.** `Repository<T,TContext,TKey> : ReadRepository<T,TContext,TKey>`;
  move the write methods + `InsertAsync` onto `Repository`; drop its re-declared reads (inherited now).
  This is the exact change that failed as run 31636765379 — **it is the proof the seam fix works:** the
  6 suites (B2B Artist/Concert/User/Venue, Customer User/Concert) must go green.
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

- **Reparent duplicates the writes.** Single inheritance forces one facet's members onto `Repository`;
  we duplicate the trivial writes to keep reads-once. Logged in `TECH_DEBT.md`, not eliminable without
  default-interface-members or a mixin.
- **`InsertAsync` adoption is opportunistic** — new/changed call sites use it; not a forced migration of
  every existing `AddAsync` + `SaveChangesAsync`.
- **B2B/Payment dead `ReadRepository<T>` aliases** — unused (no concrete subclasses); already deleted in
  the current PR-B diff. Keep them gone.
- **Local-pack scope creep** — packing the whole platform is safest but slower; if pack time hurts CI,
  narrow to the transitive platform closure the integration graph actually restores (still derived from
  Directory.Packages.props, not guessed).
