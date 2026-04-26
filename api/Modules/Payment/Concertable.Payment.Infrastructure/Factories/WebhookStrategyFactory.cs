using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure.Factories;

internal class WebhookStrategyFactory : IWebhookStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public WebhookStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IWebhookStrategy Create(WebhookType type)
        => serviceProvider.GetRequiredKeyedService<IWebhookStrategy>(type);
}
