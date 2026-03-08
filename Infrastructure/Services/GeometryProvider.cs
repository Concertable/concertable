using Application.Interfaces;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class GeometryProvider : IGeometryProvider
{
    private readonly GeometryFactory geometryFactory;

    public GeometryProvider(GeometryFactory geometryFactory)
    {
        this.geometryFactory = geometryFactory;
    }

    public Point? CreatePoint(double? latitude, double? longitude)
    {
        return (latitude.HasValue && longitude.HasValue)
            ? geometryFactory.CreatePoint(new Coordinate(longitude.Value, latitude.Value))
            : null;
    }
}
