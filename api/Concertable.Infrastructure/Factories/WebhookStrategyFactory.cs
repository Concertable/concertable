using Concertable.Core.Enums;
using Concertable.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

public class WebhookStrategyFactory : IWebhookStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public WebhookStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IWebhookStrategy Create(WebhookType type)
        => serviceProvider.GetRequiredKeyedService<IWebhookStrategy>(type);
}
