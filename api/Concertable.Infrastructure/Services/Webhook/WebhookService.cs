using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services.Webhook;

public class WebhookService : IWebhookService
{
    private readonly IWebhookQueue webhookQueue;
    private readonly string webhookSecret;

    public WebhookService(IWebhookQueue webhookQueue, IConfiguration configuration)
    {
        this.webhookQueue = webhookQueue;
        webhookSecret = configuration["Stripe:WebhookSecret"]!;
    }

    public async Task HandleAsync(string json, string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        await webhookQueue.EnqueueAsync(stripeEvent);
    }

}
