using Application.Interfaces;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class GeometryService : IGeometryService
    {
        private readonly GeometryFactory geometryFactory;

        public GeometryService(GeometryFactory geometryFactory)
        {
            this.geometryFactory = geometryFactory;
        }

        public Point? CreatePoint(double? latitude, double? longitude)
        {
            return (latitude.HasValue && longitude.HasValue)
            ? geometryFactory.CreatePoint(new Coordinate(longitude.Value, latitude.Value))
            : null;
        }

        public double? GetLatitude(Point? point) => point?.Y;

        public double? GetLongitude(Point? point) => point?.X;
    }
}
