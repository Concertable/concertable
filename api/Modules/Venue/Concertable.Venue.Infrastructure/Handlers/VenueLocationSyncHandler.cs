using Concertable.Identity.Contracts.Events;
using Concertable.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Venue.Infrastructure.Handlers;

internal class VenueLocationSyncHandler(VenueDbContext db)
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
