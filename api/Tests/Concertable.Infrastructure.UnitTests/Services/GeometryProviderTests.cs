using Infrastructure.Services.Geometry;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class GeometryProviderTests
{
    private readonly GeometryProvider sut;

    public GeometryProviderTests()
    {
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        sut = new GeometryProvider(geometryFactory);
    }

    [Fact]
    public void CreatePoint_ShouldUseSrid4326()
    {
        var result = sut.CreatePoint(51.5, -0.1);

        Assert.Equal(4326, result.SRID);
    }

    [Fact]
    public void CreatePoint_ShouldMapLatitudeToY_AndLongitudeToX()
    {
        var result = sut.CreatePoint(51.5, -0.1);

        Assert.Equal(51.5, result.Y);
        Assert.Equal(-0.1, result.X);
    }

    [Fact]
    public void CreatePoint_ShouldReturnNull_WhenLatitudeOrLongitudeIsNull()
    {
        Assert.Null(sut.CreatePoint(null, -0.1));
        Assert.Null(sut.CreatePoint(51.5, null));
        Assert.Null(sut.CreatePoint(null, null));
    }
}
