using Concertable.User.Infrastructure.Data;
using Concertable.Shared;
using Concertable.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Concertable.User.Infrastructure.Events;

internal class VenueManagerSyncHandler(UserDbContext db)
    : IIntegrationEventHandler<VenueChangedEvent>
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task HandleAsync(VenueChangedEvent e, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == e.UserId, ct);
        if (user is null) return;

        user.SyncFromManager(
            e.Avatar,
            GeometryFactory.CreatePoint(new Coordinate(e.Longitude, e.Latitude)),
            new Address(e.County, e.Town));

        if (user is VenueManagerEntity vm)
            vm.AssignVenue(e.VenueId);

        await db.SaveChangesAsync(ct);
    }
}
