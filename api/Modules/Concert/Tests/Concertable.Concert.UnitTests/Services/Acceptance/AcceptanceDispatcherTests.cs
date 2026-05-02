using Concertable.Concert.Infrastructure.Services.Acceptance;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Acceptance;
public class AcceptanceDispatcherTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly AcceptanceDispatcher sut;

    public AcceptanceDispatcherTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new AcceptanceDispatcher(contractLoader.Object, workflowFactory.Object);
    }

    [Fact]
    public async Task AcceptAsync_WithPaymentMethod_DelegatesToAcceptWithPaymentMethodCapability()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var withPm = workflow.As<IAcceptWithPaymentMethod>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await sut.AcceptAsync(1, "pm_123");

        withPm.Verify(s => s.AcceptAsync(1, "pm_123"), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_WithoutPaymentMethod_DelegatesToAcceptByConfirmationCapability()
    {
        var contract = new VenueHireContract { Id = 99, HireFee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var byConfirm = workflow.As<IAcceptByConfirmation>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.VenueHire)).Returns(workflow.Object);

        await sut.AcceptAsync(1);

        byConfirm.Verify(s => s.AcceptAsync(1), Times.Once);
    }
}
