using Application.DTOs;
using Application.Interfaces;
using Core.Exceptions;
using Core.Responses;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings stripeSettings;
        private readonly IPurchaseService purchaseService;

        public PaymentService(IOptions<StripeSettings> stripeSettings, IPurchaseService purchaseService)
        {
            this.stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
            this.purchaseService = purchaseService;
        }

        public async Task<PaymentResponse> ProcessAsync(string paymentMethodId, TransactionDto transactionDto)
        {
            try
            {
                long amount = (long)(transactionDto.Amount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "GBP",
                    PaymentMethod = paymentMethodId,
                    ConfirmationMethod = "manual",
                    PaymentMethodTypes = new List<string> { "card" },
                    ReceiptEmail = transactionDto.FromUserEmail
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_confirmation")
                {
                    var purchaseDto = new PurchaseDto
                    {
                        FromUserId = transactionDto.FromUserId,
                        ToUserId = transactionDto.ToUserId,
                        TransactionId = paymentIntent.Id,
                        Type = transactionDto.PaymentType,
                        Amount = amount
                    };

                    await purchaseService.CreatePaidAsync(purchaseDto);

                    var confirmOptions = new PaymentIntentConfirmOptions();
                    var confirmedPaymentIntent = await service.ConfirmAsync(paymentIntent.Id, confirmOptions);

                    var succeeded = confirmedPaymentIntent.Status == "succeeded";
                    if (!succeeded)
                        throw new BadRequestException("Payment failed");

                    return new PaymentResponse
                    {
                        Success = succeeded,
                        ClientSecret = confirmedPaymentIntent.ClientSecret,
                        TransactionId = confirmedPaymentIntent.Id
                    };
                }
                else
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = $"Unexpected PaymentIntent status: {paymentIntent.Status}"
                    };
                }
            }
            catch (StripeException ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Stripe Error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"General Error: {ex.Message}"
                };
            }
        }
    }
}
