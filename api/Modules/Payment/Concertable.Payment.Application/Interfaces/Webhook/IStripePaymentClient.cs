using Stripe;

namespace Concertable.Payment.Application.Interfaces.Webhook;

internal interface IStripePaymentClient
{
    Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options);
}
