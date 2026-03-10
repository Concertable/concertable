using NetTopologySuite.Geometries;

namespace Common.Helpers;

public static class PointExtensions
{
    public static double? ToLatitude(this Point? point) => point?.Y;
    public static double? ToLongitude(this Point? point) => point?.X;
}
