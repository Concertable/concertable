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

    public async Task<Transfer> CreateTransferAsync(TransferCreateOptions options)
    {
        var service = new TransferService();
        return await service.CreateAsync(options);
    }

    public async Task<Refund> CreateRefundAsync(RefundCreateOptions options)
    {
        var service = new RefundService();
        return await service.CreateAsync(options);
    }

    public async Task<TransferReversal> CreateTransferReversalAsync(string transferId, TransferReversalCreateOptions options)
    {
        var service = new TransferReversalService();
        return await service.CreateAsync(transferId, options);
    }
}
