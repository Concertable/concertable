using System.Text;
using Concertable.IntegrationTests.Common.Mocks;

namespace Concertable.IntegrationTests.Common;

internal class MockWebhookSimulatorFail : IWebhookSimulator
{
    private readonly IMockStripeApiClient stripeApiClient;
    private readonly IHttpClientFactory httpClientFactory;

    public MockWebhookSimulatorFail(IMockStripeApiClient stripeApiClient, IHttpClientFactory httpClientFactory)
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
            "id": "evt_test_{{Guid.NewGuid():N}}",
            "object": "event",
            "type": "payment_intent.payment_failed",
            "data": {
                "object": {
                    "id": "{{stripeApiClient.LastPaymentIntentId}}",
                    "object": "payment_intent",
                    "status": "requires_payment_method",
                    "last_payment_error": {
                        "message": "Your card was declined."
                    },
                    "metadata": { {{metadataJson}} }
                }
            }
        }
        """;

        var client = httpClientFactory.CreateClient();
        await client.PostAsync("/api/Webhook", new StringContent(json, Encoding.UTF8, "application/json"));
    }
}
