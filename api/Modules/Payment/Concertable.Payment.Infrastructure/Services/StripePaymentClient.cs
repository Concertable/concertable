using Concertable.Payment.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services;

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
