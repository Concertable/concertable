using Concertable.Application.Interfaces.Payment;
using Concertable.Infrastructure.Interfaces;

namespace Concertable.Infrastructure.Services.Payment;

public class OffSessionPaymentService : PaymentService
{
    public OffSessionPaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
        : base(stripeClient, stripeAccountService) { }

    protected override void Configure(Stripe.PaymentIntentCreateOptions options)
    {
        options.OffSession = true;
    }
}
