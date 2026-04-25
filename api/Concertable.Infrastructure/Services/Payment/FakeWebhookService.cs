using Concertable.Payment.Application.Interfaces.Webhook;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

internal class FakeWebhookService : IWebhookService
{
    private readonly IWebhookQueue webhookQueue;

    public FakeWebhookService(IWebhookQueue webhookQueue)
    {
        this.webhookQueue = webhookQueue;
    }

    public Task HandleAsync(string json, string stripeSignature)
    {
        var stripeEvent = EventUtility.ParseEvent(json);
        return webhookQueue.EnqueueAsync(stripeEvent);
    }
}
