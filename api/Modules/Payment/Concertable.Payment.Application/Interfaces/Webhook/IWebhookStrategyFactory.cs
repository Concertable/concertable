namespace Concertable.Payment.Application.Interfaces.Webhook;

internal interface IWebhookStrategyFactory
{
    IWebhookStrategy Create(WebhookType type);
}
