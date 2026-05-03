using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Services;

internal class OffSessionStripePaymentIntentClient : StripePaymentIntentClient
{
    public OffSessionStripePaymentIntentClient(IStripePaymentClient stripeClient, IStripeAccountClient stripeAccountService, ILogger<OffSessionStripePaymentIntentClient> logger)
        : base(stripeClient, stripeAccountService, logger) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.OffSession = true;
    }
}
