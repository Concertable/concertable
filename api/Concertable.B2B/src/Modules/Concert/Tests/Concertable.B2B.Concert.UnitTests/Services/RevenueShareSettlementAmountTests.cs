using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Infrastructure.Services.Settlement;
using Concertable.B2B.Deal.Contracts;
using Concertable.B2B.Tenant.Contracts;
using Concertable.Kernel.ValueObjects;
using Moq;

namespace Concertable.B2B.Concert.UnitTests.Services;

public sealed class RevenueShareSettlementAmountTests
{
    private readonly Mock<IConcertRepository> repository = new();
    private readonly Mock<IArtistShareCalculator> calculator = new();
    private readonly Mock<ITenantModule> tenantModule = new();
    private readonly IDeal deal = Mock.Of<IDeal>();
    private readonly RevenueShareSettlementAmount resolver;

    public RevenueShareSettlementAmountTests()
    {
        this.resolver = new RevenueShareSettlementAmount(
            this.repository.Object,
            this.calculator.Object,
            this.tenantModule.Object);
    }

    [Fact]
    public async Task ResolveGrossAsync_DeductsRoundedVenuePrsBeforeArtistShare()
    {
        var venueTenantId = Guid.NewGuid();
        repository
            .Setup(r => r.GetRevenueSettlementByConcertIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TotalRevenue: (decimal?)99.99m, VenueTenantId: venueTenantId));
        tenantModule
            .Setup(t => t.GetPrsPassThroughRateAsync(venueTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.042m);
        calculator.Setup(c => c.Calculate(deal, 95.79m)).Returns(47.90m);
        var result = await resolver.ResolveGrossAsync(42, deal);

        Assert.Equal(Money.Gbp(47.90m), result);
        calculator.Verify(c => c.Calculate(deal, 95.79m), Times.Once);
    }
}
