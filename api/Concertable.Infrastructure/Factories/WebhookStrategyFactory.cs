using Concertable.Core.Enums;
using Concertable.Payment.Application.Interfaces.Webhook;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

internal class WebhookStrategyFactory : IWebhookStrategyFactory
{
    private readonly IServiceProvider serviceProvider;

    public WebhookStrategyFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IWebhookStrategy Create(WebhookType type)
        => serviceProvider.GetRequiredKeyedService<IWebhookStrategy>(type);
}
