using Concertable.Concert.Application.Responses;
using Concertable.Concert.Infrastructure.Services.Completion;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Completion;

public class CompletionExecutorTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly CompletionExecutor sut;

    public CompletionExecutorTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new CompletionExecutor(contractLoader.Object, workflowFactory.Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var outcome = new Mock<IFinishOutcome>().Object;

        contractLoader.Setup(l => l.LoadByConcertIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);
        workflow.Setup(s => s.FinishAsync(1)).ReturnsAsync(outcome);

        var result = await sut.FinishAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(outcome, result.Value);
        workflow.Verify(s => s.FinishAsync(1), Times.Once);
    }
}
