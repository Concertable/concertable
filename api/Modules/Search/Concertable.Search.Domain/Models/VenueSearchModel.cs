using NetTopologySuite.Geometries;

namespace Concertable.Search.Domain.Models;

public sealed class VenueSearchModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Point? Location { get; set; }
}
