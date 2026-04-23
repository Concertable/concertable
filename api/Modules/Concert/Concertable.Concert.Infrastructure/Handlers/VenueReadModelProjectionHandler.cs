using Concertable.Concert.Domain;
using Concertable.Concert.Infrastructure.Data;
using Concertable.Shared;
using Concertable.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Concertable.Concert.Infrastructure.Handlers;

internal class VenueReadModelProjectionHandler(ConcertDbContext db)
    : IIntegrationEventHandler<VenueChangedEvent>
{
    private static readonly GeometryFactory GeometryFactory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task HandleAsync(VenueChangedEvent e, CancellationToken ct = default)
    {
        var venue = await db.VenueReadModels.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);

        var location = e.Latitude.HasValue && e.Longitude.HasValue
            ? GeometryFactory.CreatePoint(new Coordinate(e.Longitude.Value, e.Latitude.Value))
            : null;

        if (venue is null)
        {
            db.VenueReadModels.Add(new VenueReadModel
            {
                Id = e.VenueId,
                UserId = e.UserId,
                Name = e.Name,
                About = e.About,
                County = e.County,
                Town = e.Town,
                Location = location
            });
        }
        else
        {
            venue.UserId = e.UserId;
            venue.Name = e.Name;
            venue.About = e.About;
            venue.County = e.County;
            venue.Town = e.Town;
            venue.Location = location;
        }

        await db.SaveChangesAsync(ct);
    }
}
