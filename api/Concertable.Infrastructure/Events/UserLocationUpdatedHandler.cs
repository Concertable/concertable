using Concertable.Identity.Domain.Events;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Events;

public class UserLocationUpdatedHandler(ApplicationDbContext db) : IDomainEventHandler<UserLocationUpdatedEvent>
{
    public async Task HandleAsync(UserLocationUpdatedEvent e, CancellationToken ct = default)
    {
        var venue = await db.Venues.FirstOrDefaultAsync(v => v.UserId == e.UserId, ct);
        if (venue is not null)
        {
            venue.Location = e.Location;
            venue.Address = e.Address;
        }

        var artist = await db.Artists.FirstOrDefaultAsync(a => a.UserId == e.UserId, ct);
        if (artist is not null)
        {
            artist.Location = e.Location;
            artist.Address = e.Address;
        }

        if (venue is not null || artist is not null)
            await db.SaveChangesAsync(ct);
    }
}
