using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public static class GeoHelper
    {
        public static bool HasValidCoordinates(IGeoParams geo)
            => geo.Latitude.HasValue && geo.Longitude.HasValue;
    }
}
