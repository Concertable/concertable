using Concertable.Core.Enums;

namespace Concertable.Infrastructure.Interfaces;

public interface IWebhookStrategyFactory
{
    IWebhookStrategy Create(WebhookType type);
}
