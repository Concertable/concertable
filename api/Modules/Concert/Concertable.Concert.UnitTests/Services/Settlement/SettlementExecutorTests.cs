using Concertable.Concert.Infrastructure.Services.Settlement;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Settlement;
public class SettlementExecutorTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly SettlementExecutor sut;

    public SettlementExecutorTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new SettlementExecutor(contractLoader.Object, workflowFactory.Object);
    }

    [Fact]
    public async Task SettleAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();

        contractLoader.Setup(l => l.LoadByBookingIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await sut.SettleAsync(1);

        workflow.Verify(s => s.SettleAsync(1), Times.Once);
    }
}
