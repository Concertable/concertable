using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Interfaces;
using Concertable.Infrastructure.Mappers;
using FluentResults;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

public abstract class PaymentService : IPaymentService
{
    private readonly IStripePaymentClient stripeClient;
    private readonly IStripeAccountService stripeAccountService;

    protected PaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
    {
        this.stripeClient = stripeClient;
        this.stripeAccountService = stripeAccountService;
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

            return paymentIntent.ToPaymentResult();
        }
        catch (StripeException ex)
        {
            return Result.Fail($"Stripe Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"General Error: {ex.Message}");
        }
    }

    protected abstract void Configure(PaymentIntentCreateOptions options);
}
