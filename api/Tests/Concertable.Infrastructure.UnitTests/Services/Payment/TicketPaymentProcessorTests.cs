using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Services.Payment;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Payment;

public class TicketPaymentProcessorTests
{
    private readonly Mock<IContractStrategyResolver<ITicketPaymentStrategy>> resolver;
    private readonly TicketPaymentProcessor sut;

    public TicketPaymentProcessorTests()
    {
        resolver = new Mock<IContractStrategyResolver<ITicketPaymentStrategy>>();
        sut = new TicketPaymentProcessor(resolver.Object);
    }

    [Fact]
    public async Task PayAsync_ShouldResolveStrategyAndDelegate()
    {
        var expected = new PaymentResponse { Success = true, TransactionId = "pi_test", Message = "OK" };
        var strategy = new Mock<ITicketPaymentStrategy>();
        strategy.Setup(s => s.PayAsync(1, 2, "pm_test", 9.99m)).ReturnsAsync(expected);
        resolver.Setup(r => r.ResolveForConcertAsync(1)).ReturnsAsync(strategy.Object);

        var result = await sut.PayAsync(1, 2, "pm_test", 9.99m);

        Assert.Same(expected, result);
    }
}
