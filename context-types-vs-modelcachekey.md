# Design fork: separate DbContext *types* per visibility stance vs. ONE type with stance-keyed models via `IModelCacheKeyFactory`

## Scope — answer ONLY this

This is a **narrow design comparison**, not a code review. Do **not** audit any codebase, do **not** propose other architectures, do **not** branch off onto related topics. Two concrete options are on the table; I want a decisive verdict on **which is better, and under what conditions.**

**Settled premise (not up for debate here):** in a multi-tenant EF Core (v8/10) system, a given entity needs **multiple models** — a tenant-*filtered* one (a model-level `HasQueryFilter`, so the filter also applies through navigations/`Include`) and an *unfiltered* one (for legitimate cross-tenant reads like public marketplace browse). A single model can't serve both, and per-query `IgnoreQueryFilters()` is rejected as a forgettable opt-out. **Given that multiple models are required, there are exactly two ways to host them — A or B below.** Compare those.

## Option A — one DbContext *type* per stance

```csharp
// tenant-filtered, writable
internal sealed class VenueDbContext(DbContextOptions o, VenueConfig cfg, ITenant t) : DbContext(o)
{
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueImage> Images => Set<VenueImage>();
    public DbSet<VenueReview> Reviews => Set<VenueReview>();
    protected override void OnModelCreating(ModelBuilder mb)
    {
        cfg.Configure(mb);
        mb.Entity<Venue>().HasQueryFilter(v => v.TenantId == t.Current);
    }
}

// unfiltered, READ-ONLY — narrowed surface (only what browse needs)
internal sealed class PublicVenueDbContext(DbContextOptions o, VenueConfig cfg) : DbContext(o)
{
    public DbSet<Venue> Venues => Set<Venue>();                       // no Images, no Reviews
    protected override void OnModelCreating(ModelBuilder mb) => cfg.Configure(mb);   // no filter
    public override int SaveChanges() => throw new InvalidOperationException("read-only");
}

// unfiltered, WRITABLE — admin cross-tenant ops
internal sealed class AdminVenueDbContext(DbContextOptions o, VenueConfig cfg) : DbContext(o)
{
    public DbSet<Venue> Venues => Set<Venue>();
    protected override void OnModelCreating(ModelBuilder mb) => cfg.Configure(mb);   // no filter, writable
}
```
DI: three distinct registrations. A repository injects the stance it needs **by type** (`PublicVenueDbContext`), so the stance is a compile-time fact.

## Option B — one DbContext type, multiple stance-keyed models via `IModelCacheKeyFactory`

```csharp
enum Stance { Scoped, Public, Admin }

internal sealed class VenueDbContext(DbContextOptions o, VenueConfig cfg, ITenant t, Stance stance) : DbContext(o)
{
    public Stance Stance => stance;
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueImage> Images => Set<VenueImage>();
    public DbSet<VenueReview> Reviews => Set<VenueReview>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        cfg.Configure(mb);
        if (stance == Stance.Scoped)
            mb.Entity<Venue>().HasQueryFilter(v => v.TenantId == t.Current);
    }
    public override int SaveChanges() =>
        stance == Stance.Public ? throw new InvalidOperationException("read-only") : base.SaveChanges();
}

// fold the stance into the model cache key so EF caches one model per (type, stance):
internal sealed class StanceModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext c, bool designTime) => (c.GetType(), ((VenueDbContext)c).Stance, designTime);
}
```
DI: one type registered N times with different `Stance` (keyed services or a factory).

## Tradeoffs I've identified — correct, complete, or dispute these

| | A: separate types | B: one type + `IModelCacheKeyFactory` |
|---|---|---|
| Filter varies by stance | ✅ | ✅ |
| Filter covers navigations (model-level) | ✅ | ✅ |
| Read-only enforced **structurally** | ✅ `SaveChanges` throws *by type* | ⚠️ runtime `if (stance == Public)` |
| DbSet surface **narrowed** per stance | ✅ (Public has no `Reviews`) | ❌ all DbSets on every stance |
| Stance known at **compile time** | ✅ (it *is* the type) | ❌ runtime ctor field |
| Mis-wiring caught by | type system / DI resolution | only a correct registration (runtime) |
| Classes | 3 per entity | 1 + 1 shared factory |
| Cached models in memory | N (one per type) | N (one per stance key) — **same** |
| Per-request runtime cost | identical | identical |

So at runtime they're equivalent; the trade is **compile-time enforcement + narrowed surface (A)** vs **fewer classes (B)**.

## Decisive context

This is a **tenant-isolation security boundary** — a wrong stance means one tenant reading another tenant's private data. Stated priority: *"make it hard to violate permissions later."* Team-maintained codebase, heading toward microservices (per-module isolation is already a first-class value).

## Questions (stay scoped to these)

1. For **this** case — a security boundary with the "hard-to-misuse" priority — which is the better call, **A or B**, and why?
2. Is B's reliance on a **runtime stance field** (enforced by DI wiring, not the type system) a genuine weakness for a permission boundary, or am I overweighting it?
3. Are my EF claims about B correct? Specifically: the cache key must key on **stance only, never the tenant value** (else you cache a separate model per tenant — memory blow-up); `OnModelCreating` runs once per distinct key; and any **design-time / migrations** pitfalls with a stance-keyed model.
4. When is B clearly the **right** choice over A? (e.g. stance variation that *isn't* a security boundary — per-tenant model shapes, feature-flagged mappings?)
5. Is there a **third** way to host multiple models that I haven't named?

Be decisive and specific. Cite EF Core behavior where relevant. Do not review or assume anything about the surrounding codebase — adjudicate A vs B on the merits above.
