using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Infrastructure.Services.Settlement;
using Concertable.Contract.Abstractions;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Settlement;

public class SettlementDispatcherTests
{
    private readonly Mock<IContractLookup> contractLookup;
    private readonly Mock<IConcertWorkflowStrategyFactory> strategyFactory;
    private readonly SettlementDispatcher sut;

    public SettlementDispatcherTests()
    {
        contractLookup = new Mock<IContractLookup>();
        strategyFactory = new Mock<IConcertWorkflowStrategyFactory>();
        sut = new SettlementDispatcher(contractLookup.Object, strategyFactory.Object);
    }

    [Fact]
    public async Task SettleAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var strategy = new Mock<IConcertWorkflowStrategy>();

        contractLookup.Setup(l => l.GetByBookingIdAsync(1)).ReturnsAsync(contract);
        strategyFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(strategy.Object);

        await sut.SettleAsync(1);

        strategy.Verify(s => s.SettleAsync(1), Times.Once);
    }
}
