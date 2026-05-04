using Concertable.Concert.Infrastructure.Services.Completion;
using Microsoft.Extensions.Logging;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Completion;
public class CompletionDispatcherTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly Mock<ILogger<CompletionDispatcher>> logger;
    private readonly CompletionDispatcher sut;

    public CompletionDispatcherTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        logger = new Mock<ILogger<CompletionDispatcher>>();
        sut = new CompletionDispatcher(contractLoader.Object, workflowFactory.Object, logger.Object);
    }

    [Fact]
    public async Task FinishAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();

        contractLoader.Setup(l => l.LoadByConcertIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);
        workflow.Setup(s => s.FinishAsync(1)).Returns(Task.CompletedTask);

        var result = await sut.FinishAsync(1);

        Assert.True(result.IsSuccess);
        workflow.Verify(s => s.FinishAsync(1), Times.Once);
    }
}
