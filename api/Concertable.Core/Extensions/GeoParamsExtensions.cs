using Concertable.Core.Interfaces;

namespace Concertable.Core.Extensions;

public static class GeoParamsExtensions
{
    public static bool HasValidCoordinates(this IGeoParams geo)
        => geo.Latitude.HasValue && geo.Longitude.HasValue;
}
