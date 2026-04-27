using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Infrastructure.Services.Acceptance;
using Concertable.Contract.Contracts;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Acceptance;

public class AcceptanceDispatcherTests
{
    private readonly Mock<IContractLookup> contractLookup;
    private readonly Mock<IConcertWorkflowStrategyFactory> strategyFactory;
    private readonly AcceptanceDispatcher sut;

    public AcceptanceDispatcherTests()
    {
        contractLookup = new Mock<IContractLookup>();
        strategyFactory = new Mock<IConcertWorkflowStrategyFactory>();
        sut = new AcceptanceDispatcher(contractLookup.Object, strategyFactory.Object);
    }

    [Fact]
    public async Task AcceptAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var strategy = new Mock<IConcertWorkflowStrategy>();

        contractLookup.Setup(l => l.GetByApplicationIdAsync(1)).ReturnsAsync(contract);
        strategyFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(strategy.Object);

        await sut.AcceptAsync(1);

        strategy.Verify(s => s.InitiateAsync(1, null), Times.Once);
    }
}
