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

    public async Task<Result<PaymentResponse>> HoldAsync(StripeHoldOptions opts)
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
                OnBehalfOf = opts.DestinationStripeId
            };

            Configure(options);

            var paymentIntent = await stripeClient.CreatePaymentIntentAsync(options);

            if (paymentIntent.Status == "succeeded")
                logger.LogInformation(
                    "Stripe escrow hold {IntentId} succeeded: {AmountPence} pence held in platform balance on behalf of {Destination}",
                    paymentIntent.Id, paymentIntent.Amount, options.OnBehalfOf);
            else
                logger.LogWarning(
                    "Stripe escrow hold {IntentId} returned non-succeeded status {Status}: {AmountPence} pence on behalf of {Destination}",
                    paymentIntent.Id, paymentIntent.Status, paymentIntent.Amount, options.OnBehalfOf);

            return paymentIntent.ToPaymentResult();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex,
                "Stripe hold failed for {AmountPence} pence on behalf of {Destination}: {StripeCode}",
                (long)(opts.Amount * 100), opts.DestinationStripeId, ex.StripeError?.Code);
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Hold processing failed for {AmountPence} pence on behalf of {Destination}",
                (long)(opts.Amount * 100), opts.DestinationStripeId);
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    public async Task<Result<TransferResponse>> ReleaseAsync(StripeReleaseOptions opts)
    {
        try
        {
            if (string.IsNullOrEmpty(opts.DestinationStripeId))
                return Result.Fail("Recipient does not have a Stripe account");

            var options = new TransferCreateOptions
            {
                Amount = (long)(opts.Amount * 100),
                Currency = "GBP",
                Destination = opts.DestinationStripeId,
                SourceTransaction = opts.ChargeId,
                Metadata = opts.Metadata
            };

            var transfer = await stripeClient.CreateTransferAsync(options);

            logger.LogInformation(
                "Stripe escrow release {TransferId} succeeded: {AmountPence} pence to {Destination} from charge {ChargeId}",
                transfer.Id, transfer.Amount, options.Destination, options.SourceTransaction);

            return Result.Ok(new TransferResponse(transfer.Id));
        }
        catch (StripeException ex)
        {
            logger.LogError(ex,
                "Stripe release failed for {AmountPence} pence to {Destination} from charge {ChargeId}: {StripeCode}",
                (long)(opts.Amount * 100), opts.DestinationStripeId, opts.ChargeId, ex.StripeError?.Code);
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Release processing failed for {AmountPence} pence to {Destination}",
                (long)(opts.Amount * 100), opts.DestinationStripeId);
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    public async Task<Result<RefundResponse>> RefundAsync(StripeRefundOptions opts)
    {
        try
        {
            if (!string.IsNullOrEmpty(opts.TransferId))
            {
                await stripeClient.CreateTransferReversalAsync(opts.TransferId, new TransferReversalCreateOptions
                {
                    Amount = (long)(opts.Amount * 100),
                    Metadata = opts.Metadata
                });

                logger.LogInformation(
                    "Stripe transfer reversal succeeded for transfer {TransferId}: {AmountPence} pence",
                    opts.TransferId, (long)(opts.Amount * 100));
            }

            var refund = await stripeClient.CreateRefundAsync(new RefundCreateOptions
            {
                PaymentIntent = opts.PaymentIntentId,
                Amount = (long)(opts.Amount * 100),
                Reason = opts.Reason,
                Metadata = opts.Metadata
            });

            logger.LogInformation(
                "Stripe refund {RefundId} succeeded for payment intent {IntentId}: {AmountPence} pence",
                refund.Id, opts.PaymentIntentId, refund.Amount);

            return Result.Ok(new RefundResponse(refund.Id));
        }
        catch (StripeException ex)
        {
            logger.LogError(ex,
                "Stripe refund failed for payment intent {IntentId}: {StripeCode}",
                opts.PaymentIntentId, ex.StripeError?.Code);
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Refund processing failed for payment intent {IntentId}",
                opts.PaymentIntentId);
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    protected abstract void Configure(PaymentIntentCreateOptions options);
}
