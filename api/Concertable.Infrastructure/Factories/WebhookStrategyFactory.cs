using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories;

public class WebhookStrategyFactory : IWebhookStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public WebhookStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IWebhookStrategy Create(string type)
        => serviceProvider.GetRequiredKeyedService<IWebhookStrategy>(type);
}
