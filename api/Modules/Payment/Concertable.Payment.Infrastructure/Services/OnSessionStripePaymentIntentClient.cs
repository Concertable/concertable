using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Services;

internal class OnSessionStripePaymentIntentClient : StripePaymentIntentClient
{
    public OnSessionStripePaymentIntentClient(IStripePaymentClient stripeClient, IStripeAccountClient stripeAccountService, ILogger<OnSessionStripePaymentIntentClient> logger)
        : base(stripeClient, stripeAccountService, logger) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.ConfirmationMethod = "automatic";
        options.CaptureMethod = "automatic";
    }
}
