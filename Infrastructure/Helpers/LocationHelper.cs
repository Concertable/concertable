using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class LocationHelper
    {
        public static Point? CreatePoint(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return null;

            return new Point(longitude.Value, latitude.Value) { SRID = 4326 };
        }
    }
}
