using System.Text.Json;
using Concertable.Payment.Application.Interfaces.Webhook;
using Stripe;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

internal class MockWebhookService : IWebhookService
{
    private readonly IWebhookProcessor webhookProcessor;

    public MockWebhookService(IWebhookProcessor webhookProcessor)
    {
        this.webhookProcessor = webhookProcessor;
    }

    public async Task HandleAsync(string json, string stripeSignature)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var eventId = root.GetProperty("id").GetString()!;
        var dataObject = root.GetProperty("data").GetProperty("object");
        var intentId = dataObject.GetProperty("id").GetString()!;
        var status = dataObject.GetProperty("status").GetString()!;

        var metadata = new Dictionary<string, string>();
        if (dataObject.TryGetProperty("metadata", out var metaEl))
        {
            foreach (var prop in metaEl.EnumerateObject())
                metadata[prop.Name] = prop.Value.GetString()!;
        }

        var intent = new PaymentIntent { Id = intentId, Status = status, Metadata = metadata };
        var stripeEvent = new Event
        {
            Id = eventId,
            Type = "payment_intent.succeeded",
            Data = new EventData { Object = intent }
        };

        await webhookProcessor.ProcessAsync(stripeEvent, CancellationToken.None);
    }
}
