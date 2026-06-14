# Verify a claim: can multi-tenant "filtered-by-default + a clean cross-tenant view" be done with ONE EF Core DbContext?

## What I need from you

Investigate and **verify or refute** the findings below. Be adversarial — I want holes found, not agreement. Cite current EF Core behavior (version + mechanism + docs). **If a single-DbContext approach satisfies the constraints, prove it in code.**

## The setup

- .NET 10, EF Core, SQL Server, multi-tenant.
- Entity `Application` is **private to two tenants** (`VenueTenantId`, `ArtistTenantId`) and must be invisible to any other tenant.
- Critically, `Application` is **read through navigations** from other entities — not only from its own `DbSet`:

```csharp
class Application { public int Id; public Guid VenueTenantId; public Guid ArtistTenantId; public string Secret; }
class Booking     { public int Id; public Application Application; }   // navigation
class Concert     { public int Id; public Booking Booking; }          // navigation chain
```
```csharp
_db.Concerts.Where(c => c.Booking.Application.VenueTenantId == x);   // reaches Application THROUGH the chain
_db.Bookings.Include(b => b.Application);                            // and via Include
```

- Separately there is a **legitimate cross-tenant read**: a public marketplace browse that genuinely must see across all tenants.

## What "clean" means (the constraints)

1. **Default + transitive:** tenant scoping must apply automatically and **through navigations** — a query that reaches `Application` via `Include`/join is scoped without the query author doing anything per-query.
2. **No forgettable opt-out:** the cross-tenant view must NOT be obtained via a per-call-site flag that's easy to forget (e.g. an `IgnoreQueryFilters()` sprinkled per query). The visibility stance should be enforced by construction, not by remembering to set/unset something.
3. Prefer **one DbContext** if it can satisfy 1 + 2.

## My findings (verify or refute each)

- **F1.** A repository-level scoped queryable — `context.Set<Application>().Where(tenant)`, exposed as a `CurrentTenant` property or a virtual `IQueryable<T> Query` overridden per repo — only scopes queries **rooted** at `Application`. It does **not** apply when another query roots elsewhere and navigates in (`_db.Concerts.Where(c => c.Booking.Application…)`, or `.Include(b => b.Application)`). → repo-level scoping **leaks through navigations**.
- **F2.** The only mechanism that scopes an entity *through navigations* is the **model-level global query filter** (`HasQueryFilter`), because navigation resolution happens in EF's query compiler, below the repository layer.
- **F3.** Once the filter is in the model, the only ways to read unfiltered are: (a) `IgnoreQueryFilters()` per query, or (b) a **separate DbContext** whose model never declared the filter.
- **F4 (the claim I'm least sure of — challenge it hardest).** For an entity that is *both* navigated *and* needs a clean unfiltered view, you need **two different *models*** (one with the filter, one without) — a single model cannot satisfy constraints 1 + 2. **Open question:** must those two models be two separate context *types*, or can one type hold both via `IModelCacheKeyFactory` (see C6) and still count as "clean"? My original claim was "separate context is the only way"; C6 makes me think the honest claim is only "separate *models* are required," and that separate *types* are a stronger-enforcement *choice*, not a necessity. Confirm or correct.

## Candidate single-context approaches I considered — confirm or break my assessment, and add any I missed

- **C1 — repo `.Where` / `CurrentTenant` / virtual `Query` override.** Fails F1 (navigation leak).
- **C2 — `IgnoreQueryFilters()` per cross-tenant query.** One context, covers navigations (the model filter stays), but **violates constraint 2** — per-call-site opt-out, easy to forget, and lifts *all* filters, not just tenancy.
- **C3 — toggleable global filter (the strongest single-context counter — please scrutinize this hardest).** A mutable flag on the context instance that the filter reads, flipped before a cross-tenant query:
  ```csharp
  public bool SeeAllTenants;   // instance field on the context
  modelBuilder.Entity<Application>().HasQueryFilter(
      a => SeeAllTenants || a.VenueTenantId == _t.Current || a.ArtistTenantId == _t.Current);

  // cross-tenant read:
  _db.SeeAllTenants = true;
  var rows = await _db.Concerts.Where(...).ToListAsync();
  ```
  One context, **covers navigations** (it's a model filter). **My assessment: still violates constraint 2** — it's "enable globally, conditionally disable" via a mutable, request-scoped side-effect; the stance becomes a flag on the shared context that a stray/forgotten toggle turns into a silent cross-tenant leak (or silent over-restriction), and it's not visible at the call site. Is that assessment fair, or is C3 an accepted, clean, pragmatic norm I'm wrongly dismissing? (Note also any EF caveats: filter is compiled into the cached model once; does referencing a mutable instance field behave per-instance correctly, and any pitfalls?)
- **C4 — `IQueryExpressionInterceptor` / query interceptor (EF Core 7+).** Could it inject tenant predicates into navigations generically as a *cleaner* single-context mechanism — or is that just hand-rolling `HasQueryFilter`, worse?
- **C5 — separate read model / CQRS read side (Dapper, projection, read replica) for the cross-tenant browse.** Not "one EF context," but is it the better answer than multiple EF contexts?
- **C6 — ONE context *type*, multiple stance-keyed models via `IModelCacheKeyFactory` (scrutinize this hardest alongside C3).** A custom `IModelCacheKeyFactory` folds a `stance` field into the model cache key, so the same type caches a filtered and an unfiltered model; `OnModelCreating` applies the filter only for the scoped stance; the stance is fixed at construction (immutable per instance):
  ```csharp
  class VenueDbContext(DbContextOptions o, ITenant t, Stance stance) : DbContext(o) {
      public Stance Stance => stance;
      protected override void OnModelCreating(ModelBuilder mb) {
          cfg.Configure(mb);
          if (stance == Stance.Scoped)
              mb.Entity<Booking>().HasQueryFilter(b => b.VTenant == t.Current || b.ATenant == t.Current);
      }
  }
  class StanceModelCacheKeyFactory : IModelCacheKeyFactory {
      public object Create(DbContext c, bool designTime) => (c.GetType(), ((VenueDbContext)c).Stance, designTime);
  }
  ```
  **My assessment: this is the strongest refutation of "separate context *types* are required" — what's actually required is a separate *model*, and one type can hold several. It covers navigations (real model filter), gives an unfiltered view by construction (no `IgnoreQueryFilters`), and — unlike C3 — has NO per-query toggle (stance is immutable per instance), so it appears to satisfy constraint 2.** The price: the stance moves from a compile-time *type* to a runtime ctor field, so the permission is enforced by DI wiring rather than the type system; read-only-for-public becomes a runtime check rather than `SaveChanges` throwing; and the type must be registered N times (keyed services / factory). **Questions: (a) does C6 genuinely satisfy constraints 1 + 2? (b) any EF caveats — model-cache memory, the cache key MUST key on stance only (never the tenant value, or you cache a model per tenant), `OnModelCreating` running once per distinct key, design-time/migrations behavior? (c) is C6's loss of type-level enforcement a fair price for fewer classes, vs separate context types?**

## The actual questions

1. Are **F1–F4** factually correct for current EF Core? Any error?
2. Is there a **single-DbContext** approach satisfying constraints 1 + 2 that I missed?
3. Is my rejection of **C2/C3** (per-query / toggle opt-outs as "unclean") sound, or dogmatic — are they the accepted pragmatic industry norm?
4. Given all this, is **"separate DbContext per visibility stance"** the right call here, or over-engineering?

Be specific, cite EF Core behavior, and **if I'm wrong, show the code that proves it.**
