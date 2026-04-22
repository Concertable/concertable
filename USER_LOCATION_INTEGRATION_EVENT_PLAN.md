# User Location → Integration Event Refactor

## Context

When a user updates their location via `UserService.SaveLocationAsync`, the change must
propagate to the `Artist` row (if they own one) and the `Venue` row (if they own one) so
both denormalised copies stay in sync with the user's profile.

Current implementation:

- `UserEntity.UpdateLocation` raises `UserLocationUpdatedEvent : IDomainEvent`
  (`api/Modules/Identity/Concertable.Identity.Domain/Events/UserLocationUpdatedEvent.cs`).
- `UserLocationUpdatedHandler` in legacy `api/Concertable.Infrastructure/Events/` uses
  `ApplicationDbContext.Venues` / `.Artists` directly.
- This violates module boundaries: a domain event is crossing modules, and the handler
  reaches into `ApplicationDbContext` for tables the Artist module (and soon Venue) owns.

The integration-event infrastructure now exists in `Concertable.Shared.Domain` /
`Concertable.Shared.Infrastructure`:

- `IIntegrationEvent`, `IIntegrationEventBus`, `IIntegrationEventHandler<TEvent>`
- `InProcessIntegrationEventBus` registered via `AddSharedInfrastructure()`.

Reference implementation already shipping: `ReviewCreatedDomainEvent` → translated by
`ReviewCreatedDomainEventHandler` → `ReviewSubmittedEvent : IIntegrationEvent` in
`Concert.Contracts` → consumed by `ArtistReviewProjectionHandler` in Artist.Infrastructure.

## Shape decision — Shape A (direct publish)

Location has **no in-module reaction** inside Identity. The two-hop domain-event →
translator → integration-event pattern only earns its keep when the owning module also
reacts. For location we publish the integration event **directly** from `UserService`
after `SaveChangesAsync`:

- Delete `UserLocationUpdatedEvent : IDomainEvent` (Identity.Domain/Events).
- `UserEntity.UpdateLocation` no longer raises anything; it just mutates state.
- `UserService.SaveLocationAsync` calls `integrationEventBus.PublishAsync(...)` after
  the save succeeds. Publishing *after* save is mandatory — publishing before means
  subscribers could react to a change that rolls back.

If Identity later grows an in-module reaction, promote back to Shape B (keep domain
event + translator) — cheap reversal.

## Naming

- Event: `UserLocationUpdatedEvent` in `Concertable.Identity.Contracts.Events`.
  Record shape: `(Guid UserId, Point Location, Address? Address) : IIntegrationEvent`.
- Location + Address are safe payload types: `Point` is NetTopologySuite (already a
  transitive dep of Shared.Domain), `Address` lives in `Concertable.Shared.Domain` as a
  cross-module primitive value object.

## Wrinkle: Venue isn't extracted yet

Venue still lives in `Concertable.Core` / `Concertable.Infrastructure`. That's fine —
the Venue-side handler can live in legacy `Concertable.Infrastructure` for now and
relocate to `Venue.Infrastructure` when the Venue module gets extracted. The handler
only references `Identity.Contracts` (for the event) + its own DbContext, so the move
is mechanical later.

---

## Stage A — Publisher (Identity)

1. **Create contracts event.** New file
   `api/Modules/Identity/Concertable.Identity.Contracts/Events/UserLocationUpdatedEvent.cs`:
   ```csharp
   using Concertable.Shared;
   using NetTopologySuite.Geometries;

   namespace Concertable.Identity.Contracts.Events;

   public record UserLocationUpdatedEvent(
       Guid UserId,
       Point Location,
       Address? Address) : IIntegrationEvent;
   ```
   Ensure `Concertable.Identity.Contracts.csproj` references `Concertable.Shared.Domain`
   and has `NetTopologySuite` as a package reference (or pulls it via Shared.Domain — it
   already does).

2. **Strip the domain event from `UserEntity`.** In
   `api/Modules/Identity/Concertable.Identity.Domain/UserEntity.cs`:
   - Remove the `_events.Raise(new UserLocationUpdatedEvent(...))` call from
     `UpdateLocation`.
   - Leave `IEventRaiser`/`EventRaiser` in place — other events may land here later, and
     removal is not part of this plan.

3. **Delete** `api/Modules/Identity/Concertable.Identity.Domain/Events/UserLocationUpdatedEvent.cs`
   (the `IDomainEvent` version). Grep for any remaining references and clean up.

4. **Publish from `UserService.SaveLocationAsync`.** In
   `api/Modules/Identity/Concertable.Identity.Infrastructure/Services/UserService.cs`:
   - Inject `IIntegrationEventBus`.
   - After `await userRepsitory.SaveChangesAsync();`, publish:
     ```csharp
     await integrationEventBus.PublishAsync(
         new UserLocationUpdatedEvent(user.Id, user.Location!, user.Address),
         ct);
     ```
   - `UpdateLocationAsync(UserEntity user, ...)` — the overload that takes an entity and
     doesn't save — should **not** publish. It's a builder-style helper used during
     seeding/creation flows; the caller owns the save + publish. If any caller of this
     overload ends up saving afterwards and the propagation matters, publish at that
     call site. Verify callers during implementation.

## Stage B — Artist subscriber

1. **New handler** in
   `api/Modules/Artist/Concertable.Artist.Infrastructure/Handlers/ArtistLocationSyncHandler.cs`
   (mirror `ArtistReviewProjectionHandler` layout):
   ```csharp
   internal class ArtistLocationSyncHandler(ArtistDbContext db)
       : IIntegrationEventHandler<UserLocationUpdatedEvent>
   {
       public async Task HandleAsync(UserLocationUpdatedEvent e, CancellationToken ct = default)
       {
           var artist = await db.Artists.FirstOrDefaultAsync(a => a.UserId == e.UserId, ct);
           if (artist is null) return;

           artist.Location = e.Location;
           artist.Address = e.Address;
           await db.SaveChangesAsync(ct);
       }
   }
   ```
2. **Register** in `Artist.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
   alongside the existing `ReviewSubmittedEvent` registration:
   ```csharp
   services.AddScoped<IIntegrationEventHandler<UserLocationUpdatedEvent>, ArtistLocationSyncHandler>();
   ```
3. **Reference check.** `Concertable.Artist.Infrastructure.csproj` must reference
   `Concertable.Identity.Contracts`. It may already — if not, add it. No reference to
   `Identity.Application`/`Identity.Infrastructure`/`Identity.Domain` should be introduced.

## Stage C — Venue subscriber (legacy home)

1. **New handler** in
   `api/Concertable.Infrastructure/Events/VenueLocationSyncHandler.cs` (temporary home,
   moves when Venue is extracted):
   ```csharp
   internal class VenueLocationSyncHandler(ApplicationDbContext db)
       : IIntegrationEventHandler<UserLocationUpdatedEvent>
   {
       public async Task HandleAsync(UserLocationUpdatedEvent e, CancellationToken ct = default)
       {
           var venue = await db.Venues.FirstOrDefaultAsync(v => v.UserId == e.UserId, ct);
           if (venue is null) return;

           venue.Location = e.Location;
           venue.Address = e.Address;
           await db.SaveChangesAsync(ct);
       }
   }
   ```
2. **Register** in whichever DI extension wires legacy Venue services today
   (`Concertable.Web/Extensions/ServiceCollectionExtensions.cs` is the current home):
   ```csharp
   services.AddScoped<IIntegrationEventHandler<UserLocationUpdatedEvent>, VenueLocationSyncHandler>();
   ```
3. **Delete** `api/Concertable.Infrastructure/Events/UserLocationUpdatedHandler.cs` and
   remove its DI registration. This is the legacy monolithic handler the refactor
   replaces. Grep-verify no other consumers of the old `IDomainEvent` exist.

## Stage D — Verification

1. `dotnet build` across the solution — zero warnings/errors.
2. Manual smoke: run the API, call `PUT /api/users/location` (or whichever endpoint
   drives `SaveLocationAsync`) for a user that owns both an Artist and a Venue. Confirm
   both rows' `Location` + `Address` columns updated. Repeat for a user that owns
   only one, and one that owns neither (neither handler should throw).
3. Integration tests: `api/Tests/Concertable.Web.IntegrationTests/Controllers/User/UserApiTests.cs`
   — update/extend the location-update test to assert both Artist and Venue rows sync.
4. Grep for `UserLocationUpdatedEvent` — only hits should be:
   - `Identity.Contracts/Events/UserLocationUpdatedEvent.cs` (definition)
   - `UserService.cs` (publish)
   - `ArtistLocationSyncHandler.cs` + its DI registration
   - `VenueLocationSyncHandler.cs` + its DI registration
   - Tests

## Out of scope

- Moving `VenueLocationSyncHandler` into a `Venue.Infrastructure` module. That belongs
  with the Venue extraction (separate backlog item, see `MM_NORTH_STAR.md`).
- Publishing integration events from `Artist.UpdateAsync` / `Venue.UpdateAsync` when
  *those* entities' locations change. Current direction is only User → Artist/Venue;
  reverse direction has no consumers today.
- Outbox / durable delivery. In-process bus is fine for Stage 1 modular-monolith — if we
  ever go cross-process, an outbox pattern slots in behind `IIntegrationEventBus`
  without touching publishers or handlers.

## After-merge memory update

Update `MEMORY.md`:

- New entry: `project_integration_events.md` — "Location sync is first integration-event
  consumer beyond Review. User → Artist/Venue propagation via Shape A (direct publish
  from UserService, no intermediate domain event)."
- If `MM_NORTH_STAR.md` lists this cleanup, tick it off there too.
