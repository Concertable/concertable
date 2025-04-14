using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class LocationHelper
    {
        public static Point? CreatePoint(double? latitude, double? longitude)
            => (latitude.HasValue && longitude.HasValue)
                ? new Point(longitude.Value, latitude.Value) { SRID = 4326 }
                : null;

        public static double? GetLatitude(Point? point)
            => point is not null
            ? point.Y : null;

        public static double? GetLongitude(Point? point)
            => point is not null
            ? point.X : null;
    }
}
