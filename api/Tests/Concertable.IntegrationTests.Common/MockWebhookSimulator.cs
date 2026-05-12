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
            throw new InvalidOperationException("No payment intent from the last checkout; cannot simulate webhook.");

        var metadataJson = string.Join(",\n", stripeApiClient.LastMetadata.Select(kv => $"\"{kv.Key}\": \"{kv.Value}\""));

        var isVerify = stripeApiClient.LastMetadata.TryGetValue("type", out var txType) && txType == "verify";
        var eventType = isVerify ? "payment_intent.amount_capturable_updated" : "payment_intent.succeeded";
        var status = isVerify ? "requires_capture" : "succeeded";

        var json = $$"""
        {
            "id": "{{stripeApiClient.LastEventId}}",
            "object": "event",
            "type": "{{eventType}}",
            "data": {
                "object": {
                    "id": "{{stripeApiClient.LastPaymentIntentId}}",
                    "object": "payment_intent",
                    "status": "{{status}}",
                    "metadata": { {{metadataJson}} }
                }
            }
        }
        """;

        var client = httpClientFactory.CreateClient();
        await client.PostAsync("/api/Webhook", new StringContent(json, Encoding.UTF8, "application/json"));
    }
}
