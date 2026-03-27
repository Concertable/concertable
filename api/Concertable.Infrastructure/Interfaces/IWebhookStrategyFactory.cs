using Core.Enums;

namespace Infrastructure.Interfaces;

public interface IWebhookStrategyFactory
{
    IWebhookStrategy Create(WebhookType type);
}
