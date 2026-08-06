using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Infrastructure.Services.Settlement;
using Concertable.B2B.Deal.Contracts;
using Concertable.B2B.Tenant.Contracts;
using Moq;

namespace Concertable.B2B.Concert.UnitTests.Services;

public sealed class RevenueShareSettlementAmountTests
{
    [Fact]
    public async Task ResolveGrossAsync_DeductsRoundedVenuePrsBeforeArtistShare()
    {
        var venueTenantId = Guid.NewGuid();
        var repository = new Mock<IConcertRepository>();
        var calculator = new Mock<IArtistShareCalculator>();
        var tenantModule = new Mock<ITenantModule>();
        var deal = Mock.Of<IDeal>();
        repository
            .Setup(r => r.GetRevenueSettlementByConcertIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TotalRevenue: (decimal?)99.99m, VenueTenantId: venueTenantId));
        tenantModule
            .Setup(t => t.GetPrsPassThroughRateAsync(venueTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.042m);
        calculator.Setup(c => c.Calculate(deal, 95.79m)).Returns(47.90m);
        var resolver = new RevenueShareSettlementAmount(repository.Object, calculator.Object, tenantModule.Object);

        var result = await resolver.ResolveGrossAsync(42, deal);

        Assert.Equal(47.90m, result);
        calculator.Verify(c => c.Calculate(deal, 95.79m), Times.Once);
    }
}
