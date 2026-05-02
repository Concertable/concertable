using Concertable.Concert.Infrastructure.Services.Acceptance;
using Concertable.Shared.Exceptions;
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
    public async Task AcceptAsync_WithPaymentMethod_DelegatesToPaidAcceptCapability()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var paid = workflow.As<IPaidAccept>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await sut.AcceptAsync(1, "pm_123");

        paid.Verify(s => s.AcceptAsync(1, "pm_123"), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_WithoutPaymentMethod_DelegatesToSimpleAcceptCapability()
    {
        var contract = new VenueHireContract { Id = 99, HireFee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var simple = workflow.As<ISimpleAccept>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.VenueHire)).Returns(workflow.Object);

        await sut.AcceptAsync(1);

        simple.Verify(s => s.AcceptAsync(1), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_WithPaymentMethod_Throws_WhenContractDoesNotAcceptPaymentMethod()
    {
        var contract = new VenueHireContract { Id = 99, HireFee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        workflow.As<ISimpleAccept>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.VenueHire)).Returns(workflow.Object);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.AcceptAsync(1, "pm_123"));
    }

    [Fact]
    public async Task AcceptAsync_WithoutPaymentMethod_Throws_WhenContractRequiresPaymentMethod()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        workflow.As<IPaidAccept>();

        contractLoader.Setup(l => l.LoadByApplicationIdAsync(1)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.AcceptAsync(1));
    }
}
