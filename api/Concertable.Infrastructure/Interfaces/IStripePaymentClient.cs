using Stripe;

namespace Concertable.Infrastructure.Interfaces;

public interface IStripePaymentClient
{
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
}
