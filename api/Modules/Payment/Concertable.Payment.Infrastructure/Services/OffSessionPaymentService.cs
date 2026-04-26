namespace Concertable.Payment.Infrastructure.Services;

internal class OffSessionPaymentService : PaymentService
{
    public OffSessionPaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
        : base(stripeClient, stripeAccountService) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.OffSession = true;
    }
}
