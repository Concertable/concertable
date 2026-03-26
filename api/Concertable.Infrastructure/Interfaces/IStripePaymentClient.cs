using Stripe;

namespace Infrastructure.Interfaces;

public interface IStripePaymentClient
{
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
}
