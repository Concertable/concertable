using Concertable.Concert.Infrastructure.Services.Acceptance;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Acceptance;
public class AcceptanceExecutorTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly AcceptanceExecutor sut;

    public AcceptanceExecutorTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new AcceptanceExecutor(contractLoader.Object, workflowFactory.Object);
    }

    [Fact]
    public async Task AcceptAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await sut.AcceptAsync(1);

        workflow.Verify(s => s.InitiateAsync(1, null), Times.Once);
    }
}
