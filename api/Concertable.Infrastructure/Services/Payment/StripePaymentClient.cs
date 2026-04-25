using Concertable.Payment.Application.Interfaces.Webhook;
using Concertable.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

internal class StripePaymentClient : IStripePaymentClient
{
    public StripePaymentClient(IOptions<StripeSettings> stripeSettings)
    {
        StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(PaymentIntentCreateOptions options)
    {
        var service = new PaymentIntentService();
        return await service.CreateAsync(options);
    }
}
