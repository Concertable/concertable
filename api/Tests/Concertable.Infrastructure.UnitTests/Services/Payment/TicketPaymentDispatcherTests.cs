using Concertable.Application.Responses;
using Concertable.Concert.Application.Interfaces;
using Concertable.Contract.Abstractions;
using Concertable.Infrastructure.Factories;
using Concertable.Infrastructure.Services.Payment;
using FluentResults;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services.Payment;

public class TicketPaymentDispatcherTests
{
    private readonly Mock<IContractLookup> contractLookup;
    private readonly Mock<ITicketPaymentStrategyFactory> strategyFactory;
    private readonly TicketPaymentDispatcher sut;

    public TicketPaymentDispatcherTests()
    {
        contractLookup = new Mock<IContractLookup>();
        strategyFactory = new Mock<ITicketPaymentStrategyFactory>();
        sut = new TicketPaymentDispatcher(contractLookup.Object, strategyFactory.Object);
    }

    [Fact]
    public async Task PayAsync_ShouldResolveContractAndDelegate()
    {
        var contract = new FlatFeeContract { Id = 99, Fee = 500, PaymentMethod = PaymentMethod.Cash };
        var expected = Result.Ok(new PaymentResponse { TransactionId = "pi_test" });
        var strategy = new Mock<ITicketPaymentStrategy>();

        contractLookup.Setup(l => l.GetByConcertIdAsync(1)).ReturnsAsync(contract);
        strategyFactory.Setup(f => f.Create(ContractType.FlatFee)).Returns(strategy.Object);
        strategy.Setup(s => s.PayAsync(1, 2, "pm_test", 9.99m)).ReturnsAsync(expected);

        var result = await sut.PayAsync(1, 2, "pm_test", 9.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal("pi_test", result.Value.TransactionId);
    }
}
