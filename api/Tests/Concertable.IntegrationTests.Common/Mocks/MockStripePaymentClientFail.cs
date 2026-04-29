using Concertable.Payment.Application.Interfaces.Webhook;
using Stripe;

namespace Concertable.IntegrationTests.Common.Mocks;

internal class MockStripePaymentClientFail : IStripePaymentClient
{
    public Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options)
    {
        return Task.FromResult(new PaymentIntent
        {
            Id = $"pi_test_{Guid.NewGuid():N}",
            Status = "requires_payment_method",
            Metadata = options.Metadata ?? []
        });
    }
}
