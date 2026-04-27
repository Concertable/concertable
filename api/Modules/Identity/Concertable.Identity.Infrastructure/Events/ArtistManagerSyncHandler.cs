using Concertable.Artist.Contracts.Events;
using Concertable.Identity.Infrastructure.Data;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Concertable.Identity.Infrastructure.Events;

internal class ArtistManagerSyncHandler(IdentityDbContext db)
    : IIntegrationEventHandler<ArtistChangedEvent>
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task HandleAsync(ArtistChangedEvent e, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == e.UserId, ct);
        if (user is null) return;

        user.SyncFromManager(
            e.Avatar,
            GeometryFactory.CreatePoint(new Coordinate(e.Longitude, e.Latitude)),
            new Address(e.County, e.Town));

        await db.SaveChangesAsync(ct);
    }
}
