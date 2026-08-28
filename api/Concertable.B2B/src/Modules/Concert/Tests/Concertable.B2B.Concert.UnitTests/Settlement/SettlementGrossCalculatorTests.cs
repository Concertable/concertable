using Concertable.B2B.Concert.Infrastructure.Services.Settlement;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.UnitTests.Settlement;

/// <summary>
/// Exhaustive formula and rounding coverage for the four pure settlement-gross calculators. Eligible
/// takings are supplied directly; loading them is <see cref="RevenueShareSettlementAmount"/>'s job, covered
/// by <see cref="SettlementAmountResolverTests"/>.
/// </summary>
public sealed class SettlementGrossCalculatorTests
{
    private readonly FlatFeeSettlementGrossCalculator flatFee = new();
    private readonly VenueHireSettlementGrossCalculator venueHire = new();
    private readonly DoorSplitSettlementGrossCalculator doorSplit = new();
    private readonly VersusSettlementGrossCalculator guaranteePlus = new();

    #region FlatFee

    [Fact]
    public void CalculateGross_FlatFee_ReturnsAgreedFee()
    {
        var deal = new FlatFeeDealDto { Fee = 500m };

        var gross = this.flatFee.CalculateGross(deal, Money.Zero(Currency.Gbp));

        Assert.Equal(Money.Gbp(500m), gross);
    }

    [Fact]
    public void CalculateGross_FlatFee_IgnoresEligibleTakings()
    {
        var deal = new FlatFeeDealDto { Fee = 500m };

        var gross = this.flatFee.CalculateGross(deal, Money.Gbp(9_999m));

        Assert.Equal(Money.Gbp(500m), gross);
    }

    #endregion

    #region VenueHire

    [Fact]
    public void CalculateGross_VenueHire_ReturnsAgreedHireFee()
    {
        var deal = new VenueHireDealDto { HireFee = 400m };

        var gross = this.venueHire.CalculateGross(deal, Money.Zero(Currency.Gbp));

        Assert.Equal(Money.Gbp(400m), gross);
    }

    [Fact]
    public void CalculateGross_VenueHire_IgnoresEligibleTakings()
    {
        var deal = new VenueHireDealDto { HireFee = 400m };

        var gross = this.venueHire.CalculateGross(deal, Money.Gbp(9_999m));

        Assert.Equal(Money.Gbp(400m), gross);
    }

    #endregion

    #region DoorSplit

    [Theory]
    [InlineData(70, 1_000, 700)]     // exact
    [InlineData(100, 1_000, 1_000)]  // whole takings
    [InlineData(0, 1_000, 0)]        // no share
    public void CalculateGross_DoorSplit_ReturnsArtistPercentageOfTakings(
        decimal percent,
        decimal takings,
        decimal expected)
    {
        var deal = new DoorSplitDealDto { ArtistDoorPercent = percent };

        var gross = this.doorSplit.CalculateGross(deal, Money.Gbp(takings));

        Assert.Equal(Money.Gbp(expected), gross);
    }

    [Fact]
    public void CalculateGross_DoorSplitHalfMinorUnit_RoundsHalfUp()
    {
        // 50% of £10.01 = 500.5 minor units -> rounds up to £5.01.
        var deal = new DoorSplitDealDto { ArtistDoorPercent = 50m };

        var gross = this.doorSplit.CalculateGross(deal, Money.Gbp(10.01m));

        Assert.Equal(Money.Gbp(5.01m), gross);
    }

    [Fact]
    public void CalculateGross_DoorSplitFractionalPercentage_RoundsOnce()
    {
        // 33.333% of £100.00 = 3333.3 minor units -> £33.33.
        var deal = new DoorSplitDealDto { ArtistDoorPercent = 33.333m };

        var gross = this.doorSplit.CalculateGross(deal, Money.Gbp(100m));

        Assert.Equal(Money.Gbp(33.33m), gross);
    }

    #endregion

    #region GuaranteePlus (Versus)

    [Fact]
    public void CalculateGross_GuaranteePlus_ReturnsGuaranteePlusArtistPercentageOfTakings()
    {
        var deal = new VersusDealDto { Guarantee = 100m, ArtistDoorPercent = 70m };

        var gross = this.guaranteePlus.CalculateGross(deal, Money.Gbp(1_000m));

        Assert.Equal(Money.Gbp(800m), gross);
    }

    [Fact]
    public void CalculateGross_GuaranteePlusLargeGuarantee_IsAdditiveNotWhicheverIsGreater()
    {
        // Guarantee £500 dwarfs the £100 share; the result is still the sum, £600.
        var deal = new VersusDealDto { Guarantee = 500m, ArtistDoorPercent = 10m };

        var gross = this.guaranteePlus.CalculateGross(deal, Money.Gbp(1_000m));

        Assert.Equal(Money.Gbp(600m), gross);
    }

    [Fact]
    public void CalculateGross_GuaranteePlusZeroPercentage_ReturnsGuaranteeAlone()
    {
        var deal = new VersusDealDto { Guarantee = 100m, ArtistDoorPercent = 0m };

        var gross = this.guaranteePlus.CalculateGross(deal, Money.Gbp(1_000m));

        Assert.Equal(Money.Gbp(100m), gross);
    }

    [Fact]
    public void CalculateGross_GuaranteePlusHalfMinorUnitShare_RoundsShareHalfUpThenAddsGuarantee()
    {
        // £100 + (50% of £10.01 = £5.01) = £105.01.
        var deal = new VersusDealDto { Guarantee = 100m, ArtistDoorPercent = 50m };

        var gross = this.guaranteePlus.CalculateGross(deal, Money.Gbp(10.01m));

        Assert.Equal(Money.Gbp(105.01m), gross);
    }

    #endregion
}
