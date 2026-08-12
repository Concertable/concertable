# Repository redesign — Cosmos-aligned facets + context-enforced no-tracking

Unify the DataAccess repository bases, make no-tracking **enforced by the context** (not a per-query
opt-in), and align the interface surface to the `Microsoft.Azure.CosmosRepository` facet split. Adds
`InsertAsync` (add + save) on the write side.

Branch/worktree when executing: `Refactor/data-access_repository-redesign` (fresh from `origin/main`).

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

**Decision: keep the EF-idiomatic names (`IReadRepository`/`IRepository`); do NOT rename to the Cosmos
`IReadOnlyRepository`/`IWriteOnlyRepository`.** That would be a repo-wide sweep across every service's
alias for cosmetic parity and buys nothing — Ardalis and this codebase already use `IReadRepository`/
`IRepository`. The value is context-enforced no-tracking + base unification + `InsertAsync`, not names.

**Interfaces** — three facets; `IBaseRepository` is **kept**, not folded away:

```csharp
IReadRepository<T,K>                                       // GetById, GetAll, Exists
IBaseRepository<T>                                         // Add, AddRange, InsertAsync, Update, Remove, SaveChanges — write-only, keyless
IRepository<T,K> : IBaseRepository<T>, IReadRepository<T,K> // no `new GetAllAsync`
```

**`IBaseRepository` cannot be deleted** (the earlier "fold it away" was wrong — it never checked the
keyless case): `SequenceRepository<TSequence>` operates on `ISequence` entities, which are `ITenant`
but deliberately **not** `IEntity<TKey>`, so they cannot satisfy `IReadRepository<T,K>`'s
`IEntity<TKey>` constraint and need a write-only, keyless base. `OpportunitySyncer` and
`CollectionSyncer<T>` also inject the write-only `IBaseRepository<T>` directly. The GetAll diamond is
killed instead by **removing `GetAllAsync` from `IBaseRepository`** (no caller used it). `IBaseRepository`
is a bad name for a write-only facet (Cosmos calls it `IWriteOnlyRepository`); renaming it to
`IWriteRepository` is deferred — it is binary-breaking on a published type and needs its own
publish-first migration (see "Open decisions").

**Classes** — `Repository` extends the write base; reads are re-declared (as `main` has them):

```csharp
ReadRepository<T,TContext,K> : IReadRepository<T,K>                     // GetById/GetAll/Exists over context.Set<T>()
Repository<T,TContext,K> : BaseRepository<T,TContext>, IRepository<T,K> // inherits writes + InsertAsync; re-declares the 3 reads
```

Reparenting `Repository` onto `ReadRepository` (to define reads once) was **reverted as
binary-breaking**: it moves the inherited `context` field off `BaseRepository`, and feed-compiled
consumers (`DealRepository → TenantScopedRepository → Repository`) emit `ldfld BaseRepository::context`
— the integration host loads the source-built new base and the field dangles (`FieldAccessException`,
run 31636765379). Keeping `Repository : BaseRepository` leaves `context` where compiled consumers
expect it. The read-member duplication that costs is cosmetic; the real win (context-enforced
no-tracking) already shipped in PR-A/#526.

**`InsertAsync`** — on `IWriteOnlyRepository`, in the write repo: `AddAsync` + `SaveChangesAsync`,
returns the entity (Id populated). Faults propagate as exceptions (no bool). Cosmos calls this
`CreateAsync`.

**Read contexts (the enforcement)** — Customer gets a read-only, no-tracking context per read-repo
module (`Concert`/`Venue`/`Artist`), mirroring B2B's `PublicDbContext`: composes the same
`IEntityTypeConfigurationProvider`, `SaveChanges` throws, and **registered with
`.UseQueryTrackingBehavior(NoTracking)`**. The read repos bind these; `context.Foo` is then no-tracking
by construction — `Query` is deleted. Projection handlers keep the existing tracked `ConcertDbContext`.

> Enforcement note: this is context/DI-enforced (read repos bind a no-tracking read-only context), not a
> generic `where TContext : ReadDbContext` constraint — the constraint would block `Repository :
> ReadOnlyRepository` (a writable context can't be a read-only one) and force composition boilerplate.
> Context-level enforcement is what B2B already does and keeps the clean inheritance.

## Delivery (2 PRs, 1 publish cycle)

Current `main` already has `Query` (from #498/#503), so:

- **PR-A — Customer read contexts (boundary-safe, no publish). ✅ DONE** (#522 merged, sync green).
  Plus a follow-up **#526 (merged, sync green)** put the read repos behind a queryable-only
  `IReadDbContext` interface. Operational truth in the `_PROGRESS.md` ledger.
- **PR-B — published base (publish-first). IN PROGRESS.** Remove `GetAllAsync` from `IBaseRepository`
  (that member forces the `new GetAllAsync` diamond) + add `InsertAsync`; `Repository : BaseRepository`
  (the `: ReadRepository` reparent was reverted — binary-breaking); remove `Query`.
  **`IBaseRepository`/`BaseRepository` are KEPT** (not deleted — see the resolved open-decision below).
  **No renames.** Ships in `Concertable.DataAccess.*` → publish → `platform-sync`.

## Open decisions / risks

- **RESOLVED — `IBaseRepository`/`BaseRepository` are KEPT (the write-only facet).** Census found real
  standalone consumers: `CollectionSyncer`, `OpportunitySyncer` (write-only `IBaseRepository`), and
  `SequenceRepository<TSequence>` which is **keyless** (`ISequence : ITenant`, not `IEntity<TKey>`) so it
  can only use the keyless `BaseRepository`, never the keyed `Repository`. So the facet cannot be folded
  away; the diamond is killed by removing `GetAllAsync` from `IBaseRepository` instead.
- **B2B/Payment dead `ReadRepository<T>` aliases** — unused (no concrete subclasses). Delete them in PR-B
  rather than carry them through the unify.
- **`InsertAsync` adoption is opportunistic** — new/changed call sites use it; not a forced migration of
  every existing `AddAsync` + `SaveChangesAsync`.
- **OPEN — rename `IBaseRepository`/`BaseRepository` → `IWriteRepository`/`WriteRepository`.** The honest
  name for the write-only facet. Deferred out of PR-B because it is **binary-breaking on a published
  type**: feed-compiled consumers reference the type by name (`OpportunitySyncer` ctor, `ldfld
  BaseRepository::context` in every module repo), so a rename fails the same integration seam that
  reverted the reparent. Lands cleanly only as either (a) a deprecate→migrate→remove publish-first
  sequence, or (b) after the integration test seam is fixed so a base-type change can be validated
  in-PR. The seam (feed-compiled consumers vs source-built DataAccess via `Seed.Infrastructure`'s source
  ProjectReference) is the real blocker on *any* binary-breaking base change — decide seam-fix vs
  deprecation-dance with Tommy.
