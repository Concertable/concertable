using Concertable.Concert.Infrastructure.Services.Settlement;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Settlement;
public class SettlementDispatcherTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly SettlementDispatcher sut;

    public SettlementDispatcherTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new SettlementDispatcher(contractLoader.Object, workflowFactory.Object, NullLogger<SettlementDispatcher>.Instance);
    }

    [Fact]
    public async Task SettleAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();

        contractLoader.Setup(l => l.TryLoadByBookingIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await sut.SettleAsync(1);

        workflow.Verify(s => s.SettleAsync(1), Times.Once);
    }
}
