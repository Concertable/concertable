using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Infrastructure.Interfaces;
using Stripe;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockStripePaymentClient : IMockStripePaymentClient
{
    public string LastPaymentIntentId { get; private set; } = string.Empty;
    public Dictionary<string, string> LastMetadata { get; private set; } = [];

    public void Reset()
    {
        LastPaymentIntentId = string.Empty;
        LastMetadata = [];
    }

    public Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options)
    {
        LastPaymentIntentId = $"pi_test_{Guid.NewGuid():N}";
        LastMetadata = options.Metadata ?? [];

        return Task.FromResult(new PaymentIntent
        {
            Id = LastPaymentIntentId,
            Status = "succeeded",
            AmountReceived = options.Amount ?? 0,
            Metadata = options.Metadata ?? []
        });
    }

}
