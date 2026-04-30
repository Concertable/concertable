using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Services;

internal class OnSessionPaymentService : PaymentService
{
    public OnSessionPaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService, ILogger<OnSessionPaymentService> logger)
        : base(stripeClient, stripeAccountService, logger) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.ConfirmationMethod = "automatic";
        options.CaptureMethod = "automatic";
    }
}
