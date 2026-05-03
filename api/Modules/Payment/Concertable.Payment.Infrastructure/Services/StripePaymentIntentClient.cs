using Concertable.Payment.Infrastructure.Mappers;
using FluentResults;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services;

internal abstract class StripePaymentIntentClient : IStripePaymentIntentClient
{
    private readonly IStripePaymentClient stripeClient;
    private readonly IStripeAccountClient stripeAccountClient;
    private readonly ILogger logger;

    protected StripePaymentIntentClient(IStripePaymentClient stripeClient, IStripeAccountClient stripeAccountClient, ILogger logger)
    {
        this.stripeClient = stripeClient;
        this.stripeAccountClient = stripeAccountClient;
        this.logger = logger;
    }

    public async Task<Result<PaymentResponse>> ChargeAsync(StripeChargeOptions opts)
    {
        try
        {
            if (string.IsNullOrEmpty(opts.DestinationStripeId))
                return Result.Fail("Recipient does not have a Stripe account");

            if (await stripeAccountClient.GetAccountStatusAsync(opts.DestinationStripeId) != PayoutAccountStatus.Verified)
                return Result.Fail("Recipient is not eligible for payouts");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(opts.Amount * 100),
                Currency = "GBP",
                PaymentMethod = opts.PaymentMethodId,
                Customer = opts.StripeCustomerId,
                Confirm = true,
                PaymentMethodTypes = ["card"],
                ReceiptEmail = opts.ReceiptEmail,
                Metadata = opts.Metadata,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = opts.DestinationStripeId
                }
            };

            Configure(options);

            var paymentIntent = await stripeClient.CreatePaymentIntentAsync(options);

            if (paymentIntent.Status == "succeeded")
                logger.LogInformation(
                    "Stripe payment intent {IntentId} succeeded: {AmountPence} pence to {Destination}",
                    paymentIntent.Id, paymentIntent.Amount, options.TransferData.Destination);
            else
                logger.LogWarning(
                    "Stripe payment intent {IntentId} returned non-succeeded status {Status}: {AmountPence} pence to {Destination}",
                    paymentIntent.Id, paymentIntent.Status, paymentIntent.Amount, options.TransferData.Destination);

            return paymentIntent.ToPaymentResult();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex,
                "Stripe charge failed for {AmountPence} pence to {Destination}: {StripeCode}",
                (long)(opts.Amount * 100), opts.DestinationStripeId, ex.StripeError?.Code);
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Payment processing failed for {AmountPence} pence to {Destination}",
                (long)(opts.Amount * 100), opts.DestinationStripeId);
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    protected abstract void Configure(PaymentIntentCreateOptions options);
}
