using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGeometryProvider
    {
        Point? CreatePoint(double? latitude, double? longitude);
    }
}
