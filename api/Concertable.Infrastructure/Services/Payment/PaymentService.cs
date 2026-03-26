using Application.Interfaces.Payment;
using Application.Requests;
using Application.Responses;
using Infrastructure.Interfaces;
using Stripe;

namespace Infrastructure.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IStripePaymentClient stripeClient;
    private readonly IStripeAccountService stripeAccountService;

    public PaymentService(IStripePaymentClient stripeClient, IStripeAccountService stripeAccountService)
    {
        this.stripeClient = stripeClient;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<PaymentResponse> ProcessAsync(TransactionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DestinationStripeId))
                return new PaymentResponse { Success = false, Message = "Recipient does not have a Stripe account" };

            if (!await stripeAccountService.IsUserVerifiedAsync(request.DestinationStripeId))
                return new PaymentResponse { Success = false, Message = "Recipient is not eligible for payouts" };

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = "GBP",
                PaymentMethod = request.PaymentMethodId,
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

            return new PaymentResponse
            {
                Success = paymentIntent.Status == "succeeded",
                RequiresAction = paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_confirmation",
                ClientSecret = paymentIntent.ClientSecret,
                TransactionId = paymentIntent.Id,
                Message = paymentIntent.Status == "succeeded" ? "Payment successful" : "Additional authentication required"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResponse { Success = false, Message = $"Stripe Error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new PaymentResponse { Success = false, Message = $"General Error: {ex.Message}" };
        }
    }
}
