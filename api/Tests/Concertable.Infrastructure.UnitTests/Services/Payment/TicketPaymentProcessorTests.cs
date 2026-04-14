using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Services.Payment;
using FluentResults;
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
        var expected = Result.Ok(new PaymentResponse { TransactionId = "pi_test" });
        var strategy = new Mock<ITicketPaymentStrategy>();
        strategy.Setup(s => s.PayAsync(1, 2, "pm_test", 9.99m)).ReturnsAsync(expected);
        resolver.Setup(r => r.ResolveForConcertAsync(1)).ReturnsAsync(strategy.Object);

        var result = await sut.PayAsync(1, 2, "pm_test", 9.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal("pi_test", result.Value.TransactionId);
    }
}
