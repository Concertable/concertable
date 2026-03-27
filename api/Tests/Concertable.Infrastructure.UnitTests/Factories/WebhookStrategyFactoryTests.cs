using Core.Enums;
using Infrastructure.Factories;
using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class WebhookStrategyFactoryTests
{
    private static readonly IWebhookStrategy concertStrategy = new Mock<IWebhookStrategy>().Object;
    private static readonly IWebhookStrategy settlementStrategy = new Mock<IWebhookStrategy>().Object;
    private readonly WebhookStrategyFactory sut;

    public WebhookStrategyFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWebhookStrategy>(WebhookType.Concert, concertStrategy);
        services.AddKeyedSingleton<IWebhookStrategy>(WebhookType.Settlement, settlementStrategy);

        sut = new WebhookStrategyFactory(services.BuildServiceProvider());
    }

    public static TheoryData<WebhookType, IWebhookStrategy> WebhookTypeStrategies => new()
    {
        { WebhookType.Concert, concertStrategy },
        { WebhookType.Settlement, settlementStrategy }
    };

    [Theory]
    [MemberData(nameof(WebhookTypeStrategies))]
    public void Create_ShouldReturnCorrectStrategy(WebhookType webhookType, IWebhookStrategy expected)
    {
        Assert.Same(expected, sut.Create(webhookType));
    }
}
