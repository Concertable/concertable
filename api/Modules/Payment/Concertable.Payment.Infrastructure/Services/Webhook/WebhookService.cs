using Concertable.Payment.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services.Webhook;

internal class WebhookService : IWebhookService
{
    private readonly IWebhookQueue webhookQueue;
    private readonly string webhookSecret;

    public WebhookService(IWebhookQueue webhookQueue, IOptions<StripeSettings> stripeSettings)
    {
        this.webhookQueue = webhookQueue;
        webhookSecret = stripeSettings.Value.WebhookSecret ?? string.Empty;
    }

    public async Task HandleAsync(string json, string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        await webhookQueue.EnqueueAsync(stripeEvent);
    }
}
