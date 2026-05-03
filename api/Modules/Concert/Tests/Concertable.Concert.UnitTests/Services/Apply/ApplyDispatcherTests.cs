using Concertable.Concert.Infrastructure.Services.Apply;
using Concertable.Shared.Exceptions;
using Moq;

namespace Concertable.Concert.UnitTests.Services.Apply;
public class ApplyDispatcherTests
{
    private readonly Mock<IContractLoader> contractLoader;
    private readonly Mock<IConcertWorkflowFactory> workflowFactory;
    private readonly ApplyDispatcher sut;

    public ApplyDispatcherTests()
    {
        contractLoader = new Mock<IContractLoader>();
        workflowFactory = new Mock<IConcertWorkflowFactory>();
        sut = new ApplyDispatcher(contractLoader.Object, workflowFactory.Object);
    }

    [Fact]
    public async Task ApplyAsync_WithoutPaymentMethod_DelegatesToSimpleApplyCapability()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var simple = workflow.As<ISimpleApply>();
        var application = StandardApplication.Create(7, 11);
        simple.Setup(s => s.ApplyAsync(7, 11)).ReturnsAsync(application);

        contractLoader.Setup(l => l.LoadByOpportunityIdAsync(11)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        var result = await sut.ApplyAsync(11, 7);

        Assert.Same(application, result);
        simple.Verify(s => s.ApplyAsync(7, 11), Times.Once);
    }

    [Fact]
    public async Task ApplyAsync_WithPaymentMethod_DelegatesToPaidApplyCapability()
    {
        var contract = new VenueHireContract { Id = 99, HireFee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        var paid = workflow.As<IPaidApply>();
        var application = PrepaidApplication.Create(7, 11, "pm_123");
        paid.Setup(s => s.ApplyAsync(7, 11, "pm_123")).ReturnsAsync(application);

        contractLoader.Setup(l => l.LoadByOpportunityIdAsync(11)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.VenueHire)).Returns(workflow.Object);

        var result = await sut.ApplyAsync(11, 7, "pm_123");

        Assert.Same(application, result);
        paid.Verify(s => s.ApplyAsync(7, 11, "pm_123"), Times.Once);
    }

    [Fact]
    public async Task ApplyAsync_WithoutPaymentMethod_Throws_WhenContractRequiresPaymentMethod()
    {
        var contract = new VenueHireContract { Id = 99, HireFee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        workflow.As<IPaidApply>();

        contractLoader.Setup(l => l.LoadByOpportunityIdAsync(11)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.VenueHire)).Returns(workflow.Object);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.ApplyAsync(11, 7));
    }

    [Fact]
    public async Task ApplyAsync_WithPaymentMethod_Throws_WhenContractDoesNotAcceptPaymentMethod()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var workflow = new Mock<IConcertWorkflow>();
        workflow.As<ISimpleApply>();

        contractLoader.Setup(l => l.LoadByOpportunityIdAsync(11)).ReturnsAsync(contract);
        workflowFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(workflow.Object);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.ApplyAsync(11, 7, "pm_123"));
    }
}
