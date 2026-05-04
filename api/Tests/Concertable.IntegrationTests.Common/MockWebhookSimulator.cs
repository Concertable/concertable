using System.Text;
using Concertable.IntegrationTests.Common.Mocks;

namespace Concertable.IntegrationTests.Common;

internal class MockWebhookSimulator : IWebhookSimulator
{
    private readonly IMockStripeApiClient stripeApiClient;
    private readonly IHttpClientFactory httpClientFactory;

    public MockWebhookSimulator(IMockStripeApiClient stripeApiClient, IHttpClientFactory httpClientFactory)
    {
        this.stripeApiClient = stripeApiClient;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task SendWebhookAsync()
    {
        if (string.IsNullOrEmpty(stripeApiClient.LastPaymentIntentId))
            throw new InvalidOperationException("No payment intent from the last accept; cannot simulate webhook.");

        var metadataJson = string.Join(",\n", stripeApiClient.LastMetadata.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""));

        var json = $$"""
        {
            "id": "{{stripeApiClient.LastEventId}}",
            "object": "event",
            "type": "payment_intent.succeeded",
            "data": {
                "object": {
                    "id": "{{stripeApiClient.LastPaymentIntentId}}",
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
