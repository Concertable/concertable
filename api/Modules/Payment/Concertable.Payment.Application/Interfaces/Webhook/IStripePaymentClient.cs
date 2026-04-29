using Stripe;

namespace Concertable.Payment.Application.Interfaces.Webhook;

public interface IStripePaymentClient
{
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
}
