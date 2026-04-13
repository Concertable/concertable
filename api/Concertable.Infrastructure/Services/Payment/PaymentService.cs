using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Application.Results;
using Concertable.Infrastructure.Interfaces;
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

    public async Task<PaymentResult> ProcessAsync(TransactionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DestinationStripeId))
                return new PaymentResult { Success = false, Message = "Recipient does not have a Stripe account" };

            if (!await stripeAccountService.IsUserVerifiedAsync(request.DestinationStripeId))
                return new PaymentResult { Success = false, Message = "Recipient is not eligible for payouts" };

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

            return new PaymentResult
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
            return new PaymentResult { Success = false, Message = $"Stripe Error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new PaymentResult { Success = false, Message = $"General Error: {ex.Message}" };
        }
    }
}
