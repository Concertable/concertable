using Application.DTOs;
using Application.Interfaces;
using Core.Exceptions;
using Core.Responses;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class PaymentService : IPaymentService
{
    private readonly StripeSettings stripeSettings;

    public PaymentService(IOptions<StripeSettings> stripeSettings)
    {
        this.stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
    }

    public async Task<PaymentResponse> ProcessAsync(TransactionRequestDto transactionRequesstDto)
    {
        try
        {
            long amount = (long)(transactionRequesstDto.Amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "GBP",
                PaymentMethod = transactionRequesstDto.PaymentMethodId,
                Confirm = true,
                ConfirmationMethod = "automatic",
                CaptureMethod = "automatic",
                PaymentMethodTypes = new List<string> { "card" },
                ReceiptEmail = transactionRequesstDto.FromUserEmail,
                Metadata = transactionRequesstDto.Metadata,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = transactionRequesstDto.StripeId
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            Debug.WriteLine("PaymentIntent created with ClientSecret: " + paymentIntent.ClientSecret);

            // Always return client_secret, no matter the status
            return new PaymentResponse
            {
                Success = paymentIntent.Status == "succeeded",
                RequiresAction = paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_confirmation",
                ClientSecret = paymentIntent.ClientSecret, // Always set this
                TransactionId = paymentIntent.Id,
                Message = paymentIntent.Status == "succeeded" ? "Payment successful" : "Additional authentication required"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResponse
            {
                Success = false,
                Message = $"Stripe Error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new PaymentResponse
            {
                Success = false,
                Message = $"General Error: {ex.Message}"
            };
        }
    }

}
