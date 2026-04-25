using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Interfaces.Webhook;

namespace Concertable.Infrastructure.Services.Payment;

internal class OnSessionPaymentService : PaymentService
{
    public OnSessionPaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
        : base(stripeClient, stripeAccountService) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.ConfirmationMethod = "automatic";
        options.CaptureMethod = "automatic";
    }
}
