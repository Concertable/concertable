using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Domain.Errors;
using Concertable.Contracts;
using Concertable.Contracts.Enums;
using Concertable.Kernel.ValueObjects;

namespace Concertable.B2B.Concert.UnitTests.Domain;

public sealed class DoorRevenueDeclarationTests
{
    private static readonly DateTime DeclaredAt = new(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Declare_NegativeRevenue_ReturnsTypedFailureAndNoRecord()
    {
        var concert = CreateConcert();

        var result = RevenueShareSettlementEntity.Declare(concert, -0.01m, DeclaredAt);

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<DoorRevenueDeclarationError.NegativeRevenue>(error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200.50)]
    public void Declare_NonNegativeRevenue_OpensRecordForTheConcert(decimal doorRevenue)
    {
        var concert = CreateConcert();

        var result = RevenueShareSettlementEntity.Declare(concert, doorRevenue, DeclaredAt);

        Assert.True(result.TryGetValue(out var settlement));
        Assert.Equal(concert.Id, settlement!.ConcertId);
        Assert.Equal(concert.VenueTenantId, settlement.VenueTenantId);
        Assert.Equal(concert.ArtistTenantId, settlement.ArtistTenantId);
        Assert.Equal(doorRevenue, settlement.DoorRevenue);
        Assert.Equal(DeclaredAt, settlement.DeclaredAtUtc);
        Assert.Null(settlement.Review);
    }

    [Fact]
    public void Redeclare_NegativeRevenue_ReturnsTypedFailureWithoutMutation()
    {
        var settlement = Declared(200m);

        var result = settlement.Redeclare(-1m, DeclaredAt.AddHours(1));

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<DoorRevenueDeclarationError.NegativeRevenue>(error);
        Assert.Equal(200m, settlement.DoorRevenue);
    }

    [Fact]
    public void Redeclare_NonNegativeRevenue_RestatesTheTake()
    {
        var settlement = Declared(200m);
        var restatedAt = DeclaredAt.AddHours(2);

        var result = settlement.Redeclare(275m, restatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(275m, settlement.DoorRevenue);
        Assert.Equal(restatedAt, settlement.DeclaredAtUtc);
    }

    [Fact]
    public void Redeclare_AfterAReview_InvalidatesTheReviewedGross()
    {
        var settlement = Declared(200m);
        settlement.FreezeReviewedGross(70_000, DeclaredAt.AddHours(1));

        settlement.Redeclare(240m, DeclaredAt.AddHours(2));

        Assert.Null(settlement.Review);
    }

    [Fact]
    public void FreezeReviewedGross_NonNegativeAmount_RecordsTheSnapshot()
    {
        var settlement = Declared(200m);
        var reviewedAt = DeclaredAt.AddHours(1);

        settlement.FreezeReviewedGross(70_000, reviewedAt);

        Assert.Equal(new SettlementReview(70_000, reviewedAt), settlement.Review);
    }

    [Fact]
    public void FreezeReviewedGross_NegativeAmount_Throws()
    {
        var settlement = Declared(200m);

        Assert.Throws<InvalidOperationException>(() => settlement.FreezeReviewedGross(-1, DeclaredAt));
    }

    private static RevenueShareSettlementEntity Declared(decimal doorRevenue)
    {
        RevenueShareSettlementEntity.Declare(CreateConcert(), doorRevenue, DeclaredAt).TryGetValue(out var settlement);
        return settlement!;
    }

    private static ConcertEntity CreateConcert()
    {
        var application = StandardApplication.Create(1, 2, DealType.DoorSplit, Guid.NewGuid(), Guid.NewGuid());
        var booking = DeferredBooking.Create(application, "pm_test");
        return ConcertEntity.CreateDraft(
            booking,
            1,
            2,
            new DateRange(
                new DateTime(2026, 8, 10, 19, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 10, 22, 0, 0, DateTimeKind.Utc)),
            "Concert",
            "About",
            [Genre.Rock]);
    }
}
