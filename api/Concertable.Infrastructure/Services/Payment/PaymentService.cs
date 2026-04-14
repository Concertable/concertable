using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Infrastructure.Interfaces;
using FluentResults;
using Stripe;

namespace Concertable.Infrastructure.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IStripePaymentClient stripeClient;
    private readonly IStripeAccountService stripeAccountService;

    public PaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
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

            if (!await stripeAccountService.IsUserVerifiedAsync(request.DestinationStripeId))
                return Result.Fail("Recipient is not eligible for payouts");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = "GBP",
                PaymentMethod = request.PaymentMethodId,
                Customer = request.StripeCustomerId,
                Confirm = true,
                ConfirmationMethod = "automatic",
                CaptureMethod = "automatic",
                PaymentMethodTypes = ["card"],
                ReceiptEmail = request.FromUserEmail,
                Metadata = request.Metadata,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = request.DestinationStripeId
                }
            };

            var paymentIntent = await stripeClient.CreatePaymentIntentAsync(options);

            if (paymentIntent.Status != "succeeded" && paymentIntent.Status != "requires_action" && paymentIntent.Status != "requires_confirmation")
                return Result.Fail($"Payment failed with status: {paymentIntent.Status}");

            return Result.Ok(new PaymentResponse
            {
                RequiresAction = paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_confirmation",
                ClientSecret = paymentIntent.ClientSecret,
                TransactionId = paymentIntent.Id
            });
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
}
