using Application.DTOs;
using Application.Requests;
using Application.Interfaces;
using Core.Exceptions;
using Application.Responses;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class PaymentService : IPaymentService
{
    private readonly StripeSettings stripeSettings;
    private readonly IStripeAccountService stripeAccountService;

    public PaymentService(IOptions<StripeSettings> stripeSettings, IStripeAccountService stripeAccountService)
    {
        this.stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
        this.stripeAccountService = stripeAccountService;
    }

    public async Task<PaymentResponse> ProcessAsync(TransactionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DestinationStripeId))
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Recipient does not have a Stripe account"
                };
            }

            if (!await stripeAccountService.IsUserVerifiedAsync(request.DestinationStripeId))
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Recipient is not eligible for payouts"
                };
            }

            long amount = (long)(request.Amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "GBP",
                PaymentMethod = request.PaymentMethodId,
                Confirm = true,
                ConfirmationMethod = "automatic",
                CaptureMethod = "automatic",
                PaymentMethodTypes = new List<string> { "card" },
                ReceiptEmail = request.FromUserEmail,
                Metadata = request.Metadata,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = request.DestinationStripeId
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

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
