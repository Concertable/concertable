using Microsoft.Extensions.Logging;

namespace Concertable.Payment.Infrastructure.Services;

internal class OffSessionPaymentService : PaymentService
{
    public OffSessionPaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService, ILogger<OffSessionPaymentService> logger)
        : base(stripeClient, stripeAccountService, logger) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.OffSession = true;
    }
}
