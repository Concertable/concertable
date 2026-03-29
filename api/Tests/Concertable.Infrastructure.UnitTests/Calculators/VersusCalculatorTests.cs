using Concertable.Infrastructure.Calculators;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Calculators;

public class VersusCalculatorTests
{
    [Theory]
    [InlineData(200, 1000, 50, 700)]
    [InlineData(200, 1000, 25, 450)]
    [InlineData(0, 1000, 50, 500)]
    [InlineData(200, 0, 50, 200)]
    [InlineData(0, 0, 50, 0)]
    public void ArtistShare_ShouldReturnCorrectAmount(decimal guarantee, decimal totalRevenue, decimal artistDoorPercent, decimal expected)
    {
        var result = VersusCalculator.ArtistShare(guarantee, totalRevenue, artistDoorPercent);

        Assert.Equal(expected, result);
    }
}
