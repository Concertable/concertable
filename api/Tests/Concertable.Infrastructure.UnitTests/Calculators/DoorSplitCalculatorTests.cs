using Infrastructure.Calculators;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Calculators;

public class DoorSplitCalculatorTests
{
    [Theory]
    [InlineData(1000, 50, 500)]
    [InlineData(1000, 25, 250)]
    [InlineData(1000, 100, 1000)]
    [InlineData(0, 50, 0)]
    [InlineData(1000, 0, 0)]
    public void ArtistShare_ShouldReturnCorrectAmount(decimal totalRevenue, decimal artistDoorPercent, decimal expected)
    {
        var result = DoorSplitCalculator.ArtistShare(totalRevenue, artistDoorPercent);

        Assert.Equal(expected, result);
    }
}
