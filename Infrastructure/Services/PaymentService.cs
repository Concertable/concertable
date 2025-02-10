using Application.DTOs;
using Application.Interfaces;
using Core.Enums;
using Core.Parameters;
using Core.Responses;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly StripeSettings stripeSettings;
        private readonly IPurchaseService purchaseService;
        private readonly IAuthService authService;

        public PaymentService(
            IOptions<StripeSettings> stripeSettings, 
            IPurchaseService purchaseService, 
            IAuthService authService 
            )
        {
            this.stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = this.stripeSettings.SecretKey;
            this.purchaseService = purchaseService;
            this.authService = authService;
        }

        public async Task<PaymentResponse> ProcessAsync(PaymentParams paymentParams, TransactionDto transactionDto)
        {
            try
            {
                long amount = (long)transactionDto.Amount * 100;
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = "GBP",
                    PaymentMethod = paymentParams.PaymentMethodId,
                    ConfirmationMethod = "manual",
                    ReceiptEmail = transactionDto.FromUserEmail
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                var succeeded = paymentIntent.Status == "succeeded";

                if (!succeeded) throw new StripeException("Payment confirmation failed");


                var purchaseDto = new PurchaseDto
                {
                    FromUserId = transactionDto.FromUserId,
                    ToUserId = transactionDto.ToUserId,
                    TransactionId = paymentIntent.Id,
                    Type = transactionDto.PaymentType,
                    Amount = amount
                };

                await purchaseService.CreatePaidAsync(purchaseDto);

                return new PaymentResponse
                {
                    Success = succeeded,
                    ClientSecret = paymentIntent.ClientSecret,
                    TransactionId = paymentIntent.Id
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
