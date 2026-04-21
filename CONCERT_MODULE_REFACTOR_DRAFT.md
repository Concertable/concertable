# Concert Module Refactor — Draft Notes

Running log of ideas, smells, and decisions to revisit when the Concert module is extracted.

---

## Smell: cross-module "lookup by foreign noun" on Identity repositories

Currently in `api/Modules/Identity/Concertable.Identity.Infrastructure/Repositories/ArtistManagerRepository.cs`:

```csharp
public async Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId)
{
    var userId = await readDb.Concerts
        .Where(c => c.Id == concertId)
        .Select(c => (Guid?)c.Booking.Application.Artist.UserId)
        .FirstOrDefaultAsync();
    return userId.HasValue
        ? await context.Users.OfType<ArtistManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
        : null;
}
```

Same shape for `GetByApplicationIdAsync`. The Identity module is reaching through `IReadDbContext` into `Concerts → Booking → Application → Artist` just to find one of its own users. This is a cross-module query wearing a repository's clothes.

### Rule of thumb

> If a repository method's name contains a noun owned by a different module, it shouldn't be on that repository.

`GetByConcertIdAsync` and `GetByApplicationIdAsync` fail that test.

### Why `IReadDbContext` is okay — for now

`IReadDbContext` was introduced as the *intentional* cross-module read seam while we're partially migrated. It's not hacky in itself — it's a pragmatic escape hatch. But using it from inside a module repository to resolve that module's own aggregate is the wrong shape. Reads that span modules belong in a query handler or in the caller, not buried behind an Identity repository method.

Long-term, once every module owns its own context and read surface, `IReadDbContext` usage should shrink to genuine cross-cutting query handlers only.

---

## Preferred fix (option 1): push the join to the caller's module

**Principle:** the module that *already owns* half the data does its half, then asks Identity only for what Identity owns (a user by id).

### What changes

1. **`IManagerRepository` loses the cross-module methods.** Delete `GetByConcertIdAsync` and `GetByApplicationIdAsync`. Keep only identity-scoped lookups (`GetByIdAsync(Guid userId)`, `GetByEmailAsync`, etc.).

2. **The caller resolves the Concert→UserId hop in its own module.** Today that caller lives in the legacy `Concertable.Infrastructure` workflows/services (FlatFee/VenueHire/Versus/DoorSplit workflows, `OpportunityService`, application services). Once the Concert module exists, the lookup lives there — it has first-class access to `ConcertEntity`, `BookingEntity`, `OpportunityApplicationEntity`.

3. **Two calls at the call site, one purpose each:**
   ```csharp
   // inside Concert module
   var artistUserId = await concertRepo.GetArtistUserIdForConcertAsync(concertId);
   if (artistUserId is null) return null;

   // Identity module — pure identity lookup
   var manager = await managerRepo.GetByIdAsync(artistUserId.Value);
   ```

### Why this isn't worse than the current code

- **Same round trips.** The current method is already two queries (one against read db, one against identity write db). Splitting the responsibility doesn't add a hop.
- **Each query touches only its own module's data.** Concert module queries concert data. Identity module queries identity data. No module reaches into another's tables through its repository.
- **The "ugly" disappears** because the Identity repo no longer has to know what a Concert is.

### When the single-query version is worth keeping

For hot read paths where the two-round-trip split is genuinely measurable, the right home is a **query handler** (e.g. `GetManagerForConcertQueryHandler`) living at the application/composition layer, using `IReadDbContext` to do the full join in one shot and returning a read DTO. That's explicitly what `IReadDbContext` is for — not repositories pretending to span modules.

---

## Action items for Concert module extraction

- [ ] Audit callers of `IManagerRepository.GetByConcertIdAsync` and `GetByApplicationIdAsync` (see grep: FlatFee/VenueHire/Versus/DoorSplit workflows, `OpportunityService`, Upfront/Deferred application services, `OpportunityApplicationValidator`).
- [ ] For each caller, decide: (a) resolve Concert→UserId locally then call `GetByIdAsync`, or (b) route through a dedicated query handler.
- [ ] Delete `GetByConcertIdAsync` and `GetByApplicationIdAsync` from `IManagerRepository` and `ArtistManagerRepository` once callers are migrated.
- [ ] Same pass on `VenueManagerRepository` — check for the mirror smell.
- [ ] Revisit `IReadDbContext` usage inside Identity repositories after migration; target is zero.

---

## Open questions to resolve during Concert extraction

- Does `OpportunityApplicationEntity` live in Concert or Artist module? It's the pivot point — Artist owns the application's author, Concert owns the opportunity being applied to.
- Where does `BookingEntity` live? Currently nested under Application; in Concert module it may become its own aggregate.
- Contract lifecycle — is that Concert-owned or a separate module later?

---

## Non-goals

- **Do not denormalize `ArtistUserId` / `ManagerUserId` onto `ConcertEntity`** just to avoid the join. Introduces a sync-via-events surface we don't need for a two-hop lookup. Revisit only if profiling demands it.
- **Do not introduce domain events for read convenience.** Events are for state changes across modules, not for materializing read models preemptively.
