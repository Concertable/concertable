using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Mappers;
using Concertable.B2B.Concert.Application.Responses;
using Concertable.B2B.Concert.Domain.Lifecycle;
using Concertable.B2B.Concert.Infrastructure.Services.Settlement;
using Concertable.Contracts.Enums;
using Xunit;

namespace Concertable.B2B.Concert.UnitTests.Settlement;

/// <summary>
/// The keyed <see cref="ISettlementMapper"/> leaves turn a concert + its (optional) revenue-share
/// settlement row into the manager-facing <see cref="ISettlement"/> union. Which leaf resolves per
/// <see cref="DealType"/> is covered by <c>ConcertDealStrategyFactoryTests</c>; this is the shape each
/// leaf produces.
/// </summary>
public sealed class SettlementMapperTests
{
    private static readonly DateTime Now = new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    #region FixedSettlementMapper

    [Fact]
    public void ToSettlement_FlatFee_ReturnsFixedSettlementWithAgreedFeeInMinorUnits()
    {
        var mapper = new FixedSettlementMapper(new FlatFeeSettlementGrossCalculator());

        var settlement = mapper.ToSettlement(new FlatFeeDealDto { Fee = 500m }, Projection(), Now);

        Assert.Equal(new FixedSettlement(50_000), Assert.IsType<FixedSettlement>(settlement));
    }

    [Fact]
    public void ToSettlement_VenueHire_ReturnsFixedSettlementWithHireFeeInMinorUnits()
    {
        var mapper = new FixedSettlementMapper(new VenueHireSettlementGrossCalculator());

        var settlement = mapper.ToSettlement(new VenueHireDealDto { HireFee = 400m }, Projection(), Now);

        Assert.Equal(new FixedSettlement(40_000), Assert.IsType<FixedSettlement>(settlement));
    }

    #endregion

    #region RevenueShareSettlementMapper

    [Fact]
    public void ToSettlement_NoRow_EndedAndBooked_ReturnsUndeclaredWithOpenWindow()
    {
        var settlement = RevenueShare().ToSettlement(
            DoorSplitDeal, Projection(state: LifecycleState.Booked, endDate: Now.AddHours(-1)), Now);

        var declaration = Assert.IsType<Undeclared>(RevenueShareOf(settlement).Declaration);
        Assert.True(declaration.WindowOpen);
    }

    [Theory]
    [InlineData(LifecycleState.Booked, 1)]                // gig hasn't ended yet
    [InlineData(LifecycleState.AwaitingSettlement, -1)]   // no longer Booked
    public void ToSettlement_NoRow_OutsideDeclarationWindow_ReturnsUndeclaredWithClosedWindow(
        LifecycleState state,
        int endHoursFromNow)
    {
        var settlement = RevenueShare().ToSettlement(
            DoorSplitDeal, Projection(state: state, endDate: Now.AddHours(endHoursFromNow)), Now);

        var declaration = Assert.IsType<Undeclared>(RevenueShareOf(settlement).Declaration);
        Assert.False(declaration.WindowOpen);
    }

    [Fact]
    public void ToSettlement_RowWithoutReview_ReturnsDeclaredWithTakingsAndConcertableSales()
    {
        var declaredAt = Now.AddHours(-2);
        var projection = Projection(
            ticketsSold: 10,
            price: 25m,
            row: new RevenueShareSettlementRowProjection(200m, declaredAt, Review: null));

        var settlement = RevenueShare().ToSettlement(DoorSplitDeal, projection, Now);

        var declared = Assert.IsType<Declared>(RevenueShareOf(settlement).Declaration);
        Assert.Equal(200m, declared.DoorRevenue);
        Assert.Equal(25_000, declared.TicketSalesMinor); // 10 * £25
        Assert.Equal(declaredAt, declared.DeclaredAtUtc);
    }

    [Fact]
    public void ToSettlement_RowWithReview_ReturnsReviewedWithTheFrozenGross()
    {
        var declaredAt = Now.AddHours(-2);
        var reviewedAt = Now.AddHours(-1);
        var projection = Projection(
            ticketsSold: 10,
            price: 25m,
            row: new RevenueShareSettlementRowProjection(
                200m, declaredAt, new SettlementReview(70_000, reviewedAt)));

        var settlement = RevenueShare().ToSettlement(DoorSplitDeal, projection, Now);

        var reviewed = Assert.IsType<Reviewed>(RevenueShareOf(settlement).Declaration);
        Assert.Equal(200m, reviewed.DoorRevenue);
        Assert.Equal(25_000, reviewed.TicketSalesMinor);
        Assert.Equal(declaredAt, reviewed.DeclaredAtUtc);
        Assert.Equal(70_000, reviewed.GrossMinor);
        Assert.Equal(reviewedAt, reviewed.ReviewedAtUtc);
    }

    [Fact]
    public void ToSettlement_RevenueShare_CarriesTheDealFormulaFromThePaymentAmountMapper()
    {
        var settlement = RevenueShare().ToSettlement(DoorSplitDeal, Projection(), Now);

        var doorShare = Assert.IsType<DoorSharePayment>(RevenueShareOf(settlement).Formula);
        Assert.Equal(70m, doorShare.ArtistPercent);
    }

    #endregion

    private static readonly DoorSplitDealDto DoorSplitDeal = new() { ArtistDoorPercent = 70m };

    private static RevenueShareSettlementMapper RevenueShare() =>
        new(new DoorSplitPaymentAmountMapper());

    private static RevenueShareSettlement RevenueShareOf(ISettlement settlement) =>
        Assert.IsType<RevenueShareSettlement>(settlement);

    private static ManagerConcertDetailsProjection Projection(
        LifecycleState state = LifecycleState.Booked,
        DateTime? endDate = null,
        int ticketsSold = 0,
        decimal price = 0m,
        RevenueShareSettlementRowProjection? row = null) =>
        new()
        {
            Concert = new ConcertDetails
            {
                Id = 1,
                Name = "Concert",
                About = "About",
                Price = price,
                TicketsSold = ticketsSold,
                State = state,
                IsRevenueShare = true,
                StartDate = Now.AddDays(-1),
                EndDate = endDate ?? Now.AddHours(-1),
                Venue = new ConcertVenue { Name = "Venue", County = "County", Town = "Town" },
                Artist = new ConcertArtist { Name = "Artist", County = "County", Town = "Town" }
            },
            Settlement = row
        };
}
