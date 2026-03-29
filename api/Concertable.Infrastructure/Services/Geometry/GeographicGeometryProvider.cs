using Concertable.Application.Interfaces.Geometry;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Services.Geometry;

public class GeographicGeometryProvider : IGeometryProvider
{
    private readonly GeometryFactory geometryFactory;

    public GeographicGeometryProvider(GeometryFactory geometryFactory)
    {
        this.geometryFactory = geometryFactory;
    }

    public Point CreatePoint(double latitude, double longitude)
    {
        return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }

    public Point? CreatePoint(double? latitude, double? longitude)
    {
        return (latitude.HasValue && longitude.HasValue)
            ? CreatePoint(latitude, longitude)
            : null;
    }
}
