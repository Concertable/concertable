using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Interfaces;
using Concertable.Infrastructure.Services.Webhook;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;
using static Concertable.Infrastructure.UnitTests.Services.Webhook.WebhookProcessorBuilders;

namespace Concertable.Infrastructure.UnitTests.Services.Webhook;

public class WebhookProcessorTests
{
    private readonly Mock<IStripeEventRepository> stripeEventRepository;
    private readonly Mock<IWebhookStrategyFactory> strategyFactory;
    private readonly Mock<ILogger<WebhookProcessor>> logger;
    private readonly WebhookProcessor sut;

    public WebhookProcessorTests()
    {
        stripeEventRepository = new Mock<IStripeEventRepository>();
        strategyFactory = new Mock<IWebhookStrategyFactory>();
        logger = new Mock<ILogger<WebhookProcessor>>();
        sut = new WebhookProcessor(stripeEventRepository.Object, strategyFactory.Object, TimeProvider.System, logger.Object);
    }

    [Fact]
    public async Task ProcessAsync_ShouldDoNothing_WhenEventObjectIsNotPaymentIntent()
    {
        var stripeEvent = BuildEvent("evt_1", new Stripe.PaymentMethod());

        await sut.ProcessAsync(stripeEvent, CancellationToken.None);

        stripeEventRepository.Verify(r => r.EventExistsAsync(It.IsAny<string>()), Times.Never);
        strategyFactory.Verify(f => f.Create(It.IsAny<WebhookType>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldDoNothing_WhenEventAlreadyProcessed()
    {
        stripeEventRepository.Setup(r => r.EventExistsAsync("evt_1")).ReturnsAsync(true);
        var stripeEvent = BuildEvent("evt_1", BuildIntent("succeeded", WebhookType.Concert));

        await sut.ProcessAsync(stripeEvent, CancellationToken.None);

        stripeEventRepository.Verify(r => r.AddEventAsync(It.IsAny<StripeEventEntity>()), Times.Never);
        strategyFactory.Verify(f => f.Create(It.IsAny<WebhookType>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldSaveEvent_BeforeHandling()
    {
        stripeEventRepository.Setup(r => r.EventExistsAsync("evt_1")).ReturnsAsync(false);
        var strategy = new Mock<IWebhookStrategy>();
        strategyFactory.Setup(f => f.Create(WebhookType.Concert)).Returns(strategy.Object);
        var stripeEvent = BuildEvent("evt_1", BuildIntent("succeeded", WebhookType.Concert));

        await sut.ProcessAsync(stripeEvent, CancellationToken.None);

        stripeEventRepository.Verify(r => r.AddEventAsync(It.Is<StripeEventEntity>(e => e.EventId == "evt_1")), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldNotInvokeStrategy_WhenPaymentNotSucceeded()
    {
        stripeEventRepository.Setup(r => r.EventExistsAsync("evt_1")).ReturnsAsync(false);
        var stripeEvent = BuildEvent("evt_1", BuildIntent("requires_payment_method", WebhookType.Concert));

        await sut.ProcessAsync(stripeEvent, CancellationToken.None);

        strategyFactory.Verify(f => f.Create(It.IsAny<WebhookType>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldInvokeCorrectStrategy_WhenPaymentSucceeded()
    {
        stripeEventRepository.Setup(r => r.EventExistsAsync("evt_1")).ReturnsAsync(false);
        var strategy = new Mock<IWebhookStrategy>();
        strategyFactory.Setup(f => f.Create(WebhookType.Concert)).Returns(strategy.Object);
        var stripeEvent = BuildEvent("evt_1", BuildIntent("succeeded", WebhookType.Concert));

        await sut.ProcessAsync(stripeEvent, CancellationToken.None);

        strategy.Verify(s => s.HandleAsync(It.IsAny<PaymentIntent>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldLogError_WhenStrategyThrows()
    {
        stripeEventRepository.Setup(r => r.EventExistsAsync("evt_1")).ReturnsAsync(false);
        var strategy = new Mock<IWebhookStrategy>();
        strategy.Setup(s => s.HandleAsync(It.IsAny<PaymentIntent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("handler failure"));
        strategyFactory.Setup(f => f.Create(WebhookType.Concert)).Returns(strategy.Object);
        var stripeEvent = BuildEvent("evt_1", BuildIntent("succeeded", WebhookType.Concert));

        var exception = await Record.ExceptionAsync(() => sut.ProcessAsync(stripeEvent, CancellationToken.None));

        Assert.Null(exception);
        logger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
