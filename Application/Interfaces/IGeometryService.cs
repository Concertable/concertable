using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGeometryService
    {
        Point? CreatePoint(double? latitude, double? longitude);
        double? GetLatitude(Point? point);
        double? GetLongitude(Point? point);
    }
}
