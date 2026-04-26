using System.Text;
using Concertable.Web.IntegrationTests.Infrastructure.Mocks;

namespace Concertable.Web.IntegrationTests.Infrastructure;

internal class MockStripeClient : IStripeClient
{
    private readonly IMockStripePaymentClient paymentClient;
    private readonly IHttpClientFactory httpClientFactory;

    public MockStripeClient(IMockStripePaymentClient paymentClient, IHttpClientFactory httpClientFactory)
    {
        this.paymentClient = paymentClient;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task SendWebhookAsync()
    {
        if (string.IsNullOrEmpty(paymentClient.LastPaymentIntentId))
            throw new InvalidOperationException("No payment intent from the last accept; cannot simulate webhook.");

        var metadataJson = string.Join(",\n", paymentClient.LastMetadata.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""));

        var json = $$"""
        {
            "id": "{{paymentClient.LastEventId}}",
            "object": "event",
            "type": "payment_intent.succeeded",
            "data": {
                "object": {
                    "id": "{{paymentClient.LastPaymentIntentId}}",
                    "object": "payment_intent",
                    "status": "succeeded",
                    "metadata": { {{metadataJson}} }
                }
            }
        }
        """;

        var client = httpClientFactory.CreateClient();
        await client.PostAsync("/api/Webhook", new StringContent(json, Encoding.UTF8, "application/json"));
    }
}
