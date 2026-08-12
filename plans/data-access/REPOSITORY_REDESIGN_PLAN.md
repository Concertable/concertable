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

**Interfaces** — two, inheritance; fold away `IBaseRepository` (it's what forces the GetAll diamond):

```csharp
IReadRepository<T,K>                          // GetById, GetAll, Exists
IRepository<T,K> : IReadRepository<T,K>        // + Add, AddRange, Update, Remove, InsertAsync, SaveChanges  — no `new GetAllAsync`
```

No `IWriteOnlyRepository` — YAGNI in EF (you read before you write).

**Classes** — one query implementation, write extends read, no `BaseRepository`:

```csharp
ReadRepository<T,TContext,K> : IReadRepository<T,K>                     // GetById/GetAll/Exists over context.Set<T>(), ONCE
Repository<T,TContext,K> : ReadRepository<T,TContext,K>, IRepository<T,K>   // + writes + InsertAsync
```

`GetById`/`GetAll`/`Exists` live **once** (in `ReadOnlyRepository`); the write repo inherits them and
adds mutations. Works because tracking is on the context: a read repo binds a no-tracking read context
(untracked reads), a write repo binds the writable/tracking context (tracked fetch-to-mutate) — same
inherited code.

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

- **PR-A — Customer read contexts (boundary-safe, no publish).** Introduce `ConcertReadDbContext` /
  `VenueReadDbContext` / `ArtistReadDbContext` (Public-style: same provider, `SaveChanges` throws),
  register them `.UseQueryTrackingBehavior(NoTracking)`, and point the 3 read repos at them —
  dropping their use of `Query`/ad-hoc `.AsNoTracking()`. This is where enforced no-tracking lands.
  All Customer-internal source → one clean PR, no publish cycle.
- **PR-B — published base (publish-first).** Fold `IBaseRepository` into `IRepository` (kills the
  `new GetAllAsync` diamond); `Repository : ReadRepository`; delete `BaseRepository`; remove `Query`;
  add `InsertAsync`. **No renames** — `IReadRepository`/`IRepository`/`ReadRepository`/`Repository`
  keep their names, so consumers don't churn. Ships in `Concertable.DataAccess.Infrastructure` →
  publish → `platform-sync` bumps the pin (the fold is source-compatible for consumers that only use
  `IRepository`/`IReadRepository`).

PR-A first (no consumer of `Query` remains after it), then PR-B removes `Query` + unifies the bases.

## Open decisions / risks

- **Does anything depend on `IBaseRepository` as a standalone write-only surface?** The census shows it's
  consumed via `IRepository`; confirm zero standalone `IBaseRepository` references before folding it away
  (else keep it as a marker interface).
- **B2B/Payment dead `ReadRepository<T>` aliases** — unused (no concrete subclasses). Delete them in PR-B
  rather than carry them through the unify.
- **`InsertAsync` adoption is opportunistic** — new/changed call sites use it; not a forced migration of
  every existing `AddAsync` + `SaveChangesAsync`.
