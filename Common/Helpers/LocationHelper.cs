using NetTopologySuite.Geometries;

namespace Common.Helpers;

public static class LocationHelper
{
    public static Point? CreatePoint(double? latitude, double? longitude)
        => (latitude.HasValue && longitude.HasValue)
            ? new Point(longitude.Value, latitude.Value) { SRID = 4326 }
            : null;
}
