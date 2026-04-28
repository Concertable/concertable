using Concertable.Concert.Application.Interfaces;
using Concertable.Concert.Application.Responses;
using Concertable.Concert.Infrastructure.Services.Completion;
using Concertable.Contract.Contracts;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Completion;

public class CompletionDispatcherTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowStrategyFactory> strategyFactory;
    private readonly CompletionDispatcher sut;

    public CompletionDispatcherTests()
    {
        contractLoader = new Mock<IContractLoader>();
        strategyFactory = new Mock<IConcertWorkflowStrategyFactory>();
        sut = new CompletionDispatcher(contractLoader.Object, strategyFactory.Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var strategy = new Mock<IConcertWorkflowStrategy>();
        var outcome = new Mock<IFinishOutcome>().Object;

        contractLoader.Setup(l => l.LoadByConcertIdAsync(1)).ReturnsAsync(contract);
        strategyFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(strategy.Object);
        strategy.Setup(s => s.FinishAsync(1)).ReturnsAsync(outcome);

        var result = await sut.FinishAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(outcome, result.Value);
        strategy.Verify(s => s.FinishAsync(1), Times.Once);
    }
}
