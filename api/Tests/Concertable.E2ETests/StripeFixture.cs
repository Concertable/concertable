using Stripe;

namespace Concertable.E2ETests;

public class StripeFixture
{
    private readonly PaymentIntentService paymentIntents;
    private readonly TransferService transfers;
    private readonly AppFixture app;

    internal StripeFixture(StripeClient client, AppFixture app)
    {
        paymentIntents = new PaymentIntentService(client);
        transfers = new TransferService(client);
        this.app = app;
    }

    public async Task<PaymentIntent?> FindCapturedHoldAsync(string stripeCustomerId, decimal amount)
    {
        var results = await paymentIntents.ListAsync(new PaymentIntentListOptions
        {
            Customer = stripeCustomerId,
            Created = new DateRangeOptions { GreaterThanOrEqual = app.LastReset }
        });
        return results.Data.SingleOrDefault(p =>
            p.Amount == (long)(amount * 100) && p.Status == "succeeded");
    }

    public async Task<Transfer?> FindTransferAsync(string stripeAccountId, decimal amount)
    {
        var results = await transfers.ListAsync(new TransferListOptions
        {
            Destination = stripeAccountId,
            Created = new DateRangeOptions { GreaterThanOrEqual = app.LastReset }
        });
        return results.Data.SingleOrDefault(t => t.Amount == (long)(amount * 100));
    }
}
