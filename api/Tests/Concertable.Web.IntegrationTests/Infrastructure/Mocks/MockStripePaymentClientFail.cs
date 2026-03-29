using Concertable.Infrastructure.Interfaces;
using Stripe;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockStripePaymentClientFail : IStripePaymentClient
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
