using NetTopologySuite.Geometries;

namespace Concertable.Kernel;

public interface IHasLocation
{
    Point? Location { get; set; }
}
