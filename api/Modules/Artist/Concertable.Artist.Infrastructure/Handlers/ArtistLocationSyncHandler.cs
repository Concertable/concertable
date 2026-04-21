using Concertable.Artist.Infrastructure.Data;
using Concertable.Identity.Contracts.Events;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Handlers;

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
