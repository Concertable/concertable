# Multi-tenant data access: one context + `IgnoreQueryFilters` vs. a context-per-access-stance — second opinion wanted

## Context

- **Stack:** .NET 10, EF Core, SQL Server. Modular-monolith backend kept "microservice-ready" — each module owns its own `DbContext`/model.
- **Multi-tenancy:** rows carry a `TenantId`. Write-side safety is already handled by a `SaveChangesInterceptor` that stamps `TenantId` on insert and blocks cross-tenant modifications off the change tracker — **independent of any query filter**.
- **The entity in question** (e.g. `Venue`): **tenant-owned**, but with three distinct access patterns:
  1. **Owner** — a tenant manages *their own* venues (should be tenant-scoped).
  2. **Public browse** — anyone (other tenants, anonymous) views *any* venue's public page (cross-tenant **read**).
  3. **Platform admin** — a privileged operator approves a venue they don't own (cross-tenant **write**, and the write raises a domain event that must propagate through interceptors).

## The question

How should reads/writes with different tenant-visibility be served from the same entity? Is **Approach B** below a cleaner, more idiomatic industry .NET / EF Core pattern than **Approach A**, and should I keep it or switch back?

My bias, which I want pressure-tested: enabling a global query filter and then **conditionally disabling it per query** with `IgnoreQueryFilters()` feels like an anti-pattern — the visibility stance becomes invisible at the call site and easy to forget. But I want an outside opinion on whether that's actually the pragmatic industry norm.

## Approach A — one filtered context + `IgnoreQueryFilters()`

A single `VenueDbContext` applies a global tenant query filter. Cross-tenant reads/writes opt out per query.

```csharp
public class VenueDbContext : DbContext
{
    private readonly ITenantContext _tenant;
    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<Venue>().HasQueryFilter(v => v.TenantId == _tenant.TenantId);
}

// owner read — filtered implicitly
await _db.Venues.Where(v => v.UserId == userId).FirstOrDefaultAsync();

// public browse — must remember to opt out
await _db.Venues.IgnoreQueryFilters().Where(v => v.Id == id).FirstOrDefaultAsync();

// admin approve — opt out again, then write
var venue = await _db.Venues.IgnoreQueryFilters().FirstAsync(v => v.Id == id);
venue.Approve();
await _db.SaveChangesAsync();
```

## Approach B — one DbContext per access stance, zero `IgnoreQueryFilters`

Each visibility stance is its own `DbContext`, all composing the **same** entity configuration (so mappings can't drift), differing only in what they apply on top. Repositories are named by stance and injected explicitly.

```csharp
// shared, anemic mapping — zero tenancy
public sealed class VenueConfigurationProvider { public void Configure(ModelBuilder b) { /* table maps */ } }

// 1) filtered, writable — owner stance
public sealed class VenueDbContext : DbContext            // applies TenantId == current in OnModelCreating
public sealed class VenueRepository : IVenueRepository    // owner/tenant reads + writes

// 2) unfiltered, read-only — public browse stance
public sealed class PublicVenueDbContext : DbContext      // no filter; SaveChanges() throws
public sealed class PublicVenueRepository : IPublicVenueRepository

// 3) unfiltered, writable — platform-admin stance
public sealed class AdminVenueDbContext : DbContext       // no filter; full interceptor pipeline
public sealed class AdminVenueRepository : IAdminVenueRepository

// the service injects exactly the stances it needs:
public VenueService(IVenueRepository repo, IPublicVenueRepository publicRepo, IAdminVenueRepository adminRepo) { }

public Task<VenueDetails?> GetPublicPage(int id) => publicRepo.GetDetailsByIdAsync(id);   // cross-tenant by construction
public Task<VenueDetails?> GetMyVenue(Guid userId) => repo.GetByUserIdAsync(userId);      // tenant-scoped by construction
public async Task Approve(int id) { var v = await adminRepo.GetByIdAsync(id); v.Approve(); await adminRepo.SaveChangesAsync(); }
```

Rule of thumb used: an entity only gets a stance-qualified context/repo for the stances it *actually has*; a single-stance entity stays one plain `XDbContext`/`XRepository`, renamed only when a second stance appears.

## Tradeoffs I'm aware of (please correct / add)

**For B**
- Visibility stance is explicit at the injection/call site (`publicRepo` vs `repo`); no hidden per-query opt-outs.
- "Lift the filter" is *by construction* (a context with no filter), not an imperative escape hatch → auditable, impossible to forget.
- The read-only public context (`SaveChanges` throws) makes the browse path structurally incapable of writing.

**Against B**
- More types: up to 3 `DbContext`s + 3 repos per entity (extra DI registrations, boilerplate). Cheap at runtime (model cached per type, instances per-scope, pooled connections) but more code.
- Multiple `DbContext`s over the same tables have a real footgun: an entity tracked by context A isn't tracked by context B — load through one and `SaveChanges` through another and the write silently vanishes. (We've hit exactly this.) Requires the discipline: never cross a tracked entity between contexts.
- One context must own migrations for the table; the others are read/write views that must not scaffold.

**For A**
- Fewer types; the conventional EF approach in most tutorials/samples.
- Single context, single change tracker — no cross-context tracking footgun.

**Against A**
- Visibility is global-by-default then conditionally lifted; the stance isn't visible at the call site and is easy to forget — a missing `IgnoreQueryFilters` silently over-restricts; a stray one silently over-exposes.
- `IgnoreQueryFilters()` lifts **all** filters, not just tenancy.
- Mixed read/write on one context makes "this read is intentionally cross-tenant" invisible without a comment.

## What I want from you

1. Which is the cleaner, more **idiomatic industry .NET / EF Core** pattern for a tenant-owned entity that also has public + admin cross-tenant access?
2. Is `IgnoreQueryFilters()` as the *primary* cross-tenant mechanism genuinely an anti-pattern, or an accepted pragmatic norm?
3. Should I keep Approach B or switch — and is there a **third option** I'm missing (separate read models / CQRS read side, a query-side bypass abstraction, splitting commands from queries, etc.)?

**Please push back hard if B is over-engineered** — I don't want validation, I want the right call.
