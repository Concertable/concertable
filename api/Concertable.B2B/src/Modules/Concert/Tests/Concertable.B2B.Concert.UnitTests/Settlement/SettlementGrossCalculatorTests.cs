using Concertable.B2B.Concert.Infrastructure.Services.Settlement;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.UnitTests.Settlement;

public sealed class SettlementGrossCalculatorTests
{
    private readonly FlatFeeGrossCalculator flatFee = new();
    private readonly VenueHireGrossCalculator venueHire = new();
    private readonly DoorSplitGrossCalculator doorSplit = new();
    private readonly VersusGrossCalculator versus = new();

    [Fact]
    public void CalculateGross_FlatFee_ReturnsAgreedFee()
    {
        var deal = new FlatFeeDeal { PaymentMethod = PaymentMethod.Cash, Fee = 500m };

        var gross = flatFee.CalculateGross(deal, eligibleTakings: null);

        Assert.Equal(50_000, gross.ToMinorUnits());
        Assert.Equal(Currency.Gbp, gross.Currency);
    }

    [Fact]
    public void CalculateGross_FlatFee_IgnoresEligibleTakings()
    {
        var deal = new FlatFeeDeal { PaymentMethod = PaymentMethod.Cash, Fee = 200m };

        var gross = flatFee.CalculateGross(deal, eligibleTakings: 9_999m);

        Assert.Equal(20_000, gross.ToMinorUnits());
    }

    [Fact]
    public void RequiresEligibleTakings_FlatFee_IsFalse()
    {
        Assert.False(flatFee.RequiresEligibleTakings);
    }

    [Fact]
    public void CalculateGross_VenueHire_ReturnsHireFee()
    {
        var deal = new VenueHireDeal { PaymentMethod = PaymentMethod.Cash, HireFee = 400m };

        var gross = venueHire.CalculateGross(deal, eligibleTakings: null);

        Assert.Equal(40_000, gross.ToMinorUnits());
    }

    [Fact]
    public void RequiresEligibleTakings_VenueHire_IsFalse()
    {
        Assert.False(venueHire.RequiresEligibleTakings);
    }

    [Fact]
    public void CalculateGross_DoorSplit_ReturnsPercentageOfTakings()
    {
        var deal = new DoorSplitDeal { PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 70m };

        var gross = doorSplit.CalculateGross(deal, eligibleTakings: 1_000m);

        Assert.Equal(70_000, gross.ToMinorUnits());
    }

    [Fact]
    public void CalculateGross_DoorSplit_RoundsHalfPennyUp()
    {
        var deal = new DoorSplitDeal { PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 50m };

        var gross = doorSplit.CalculateGross(deal, eligibleTakings: 24.69m);

        Assert.Equal(1_235, gross.ToMinorUnits());
    }

    [Fact]
    public void CalculateGross_DoorSplit_NullTakings_Throws()
    {
        var deal = new DoorSplitDeal { PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 70m };

        Assert.Throws<InvalidOperationException>(() => doorSplit.CalculateGross(deal, eligibleTakings: null));
    }

    [Fact]
    public void RequiresEligibleTakings_DoorSplit_IsTrue()
    {
        Assert.True(doorSplit.RequiresEligibleTakings);
    }

    [Fact]
    public void CalculateGross_Versus_ReturnsGuaranteePlusPercentageOfTakings()
    {
        var deal = new VersusDeal
        {
            PaymentMethod = PaymentMethod.Cash,
            Guarantee = 100m,
            ArtistDoorPercent = 70m
        };

        var gross = versus.CalculateGross(deal, eligibleTakings: 1_000m);

        Assert.Equal(80_000, gross.ToMinorUnits());
    }

    [Fact]
    public void CalculateGross_Versus_RoundsCombinedGrossHalfPennyUp()
    {
        var deal = new VersusDeal
        {
            PaymentMethod = PaymentMethod.Cash,
            Guarantee = 100m,
            ArtistDoorPercent = 70m
        };

        var gross = versus.CalculateGross(deal, eligibleTakings: 1_000.01m);

        Assert.Equal(80_001, gross.ToMinorUnits());
    }

    [Fact]
    public void CalculateGross_Versus_NullTakings_Throws()
    {
        var deal = new VersusDeal
        {
            PaymentMethod = PaymentMethod.Cash,
            Guarantee = 100m,
            ArtistDoorPercent = 70m
        };

        Assert.Throws<InvalidOperationException>(() => versus.CalculateGross(deal, eligibleTakings: null));
    }

    [Fact]
    public void RequiresEligibleTakings_Versus_IsTrue()
    {
        Assert.True(versus.RequiresEligibleTakings);
    }
}
