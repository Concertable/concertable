using NetTopologySuite.Geometries;

namespace Concertable.Shared;

public interface IHasLocation
{
    Point? Location { get; set; }
}
