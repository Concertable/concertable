using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class GeoApproximatorHelper
    {
        private const double EarthRadiusKm = 6371.0;
        private const double KmPerDegreeLatitude = 111.0;

        public static (double minLat, double maxLat, double minLon, double maxLon) GetBoundingBox(
            double latitude, double longitude, int radiusKm)
        {
            double latDelta = radiusKm / KmPerDegreeLatitude;
            double lonDelta = radiusKm / (KmPerDegreeLatitude * Math.Cos(latitude * Math.PI / 180));

            return (
                minLat: latitude - latDelta,
                maxLat: latitude + latDelta,
                minLon: longitude - lonDelta,
                maxLon: longitude + lonDelta
            );
        }

        public static bool IsWithinBoundingBox(
            double targetLat, double targetLon,
            double minLat, double maxLat,
            double minLon, double maxLon)
        {
            return targetLat >= minLat && targetLat <= maxLat &&
                   targetLon >= minLon && targetLon <= maxLon;
        }
    }
}
