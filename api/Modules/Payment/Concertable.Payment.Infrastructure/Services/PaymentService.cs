using Concertable.Payment.Infrastructure.Mappers;
using FluentResults;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Concertable.Payment.Infrastructure.Services;

internal abstract class PaymentService : IPaymentService
{
    private readonly IStripePaymentClient stripeClient;
    private readonly IStripeAccountService stripeAccountService;
    private readonly ILogger logger;

    protected PaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService, ILogger logger)
    {
        this.stripeClient = stripeClient;
        this.stripeAccountService = stripeAccountService;
        this.logger = logger;
    }

    public async Task<Result<PaymentResponse>> ProcessAsync(TransactionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DestinationStripeId))
                return Result.Fail("Recipient does not have a Stripe account");

            if (await stripeAccountService.GetAccountStatusAsync(request.DestinationStripeId) != PayoutAccountStatus.Verified)
                return Result.Fail("Recipient is not eligible for payouts");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = "GBP",
                PaymentMethod = request.PaymentMethodId,
                Customer = request.StripeCustomerId,
                Confirm = true,
                PaymentMethodTypes = ["card"],
                ReceiptEmail = request.FromUserEmail,
                Metadata = request.Metadata,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = request.DestinationStripeId
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
                (long)(request.Amount * 100), request.DestinationStripeId, ex.StripeError?.Code);
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Payment processing failed for {AmountPence} pence to {Destination}",
                (long)(request.Amount * 100), request.DestinationStripeId);
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    protected abstract void Configure(PaymentIntentCreateOptions options);
}
