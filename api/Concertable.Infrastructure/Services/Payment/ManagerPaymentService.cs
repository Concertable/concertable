using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class ManagerPaymentService : IManagerPaymentService
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPaymentService paymentService;
    private readonly ITransactionService transactionService;
    private readonly TimeProvider timeProvider;

    public ManagerPaymentService(
        IStripeAccountService stripeAccountService,
        IPaymentService paymentService,
        ITransactionService transactionService,
        TimeProvider timeProvider)
    {
        this.stripeAccountService = stripeAccountService;
        this.paymentService = paymentService;
        this.transactionService = transactionService;
        this.timeProvider = timeProvider;
    }

    public async Task PayAsync(ManagerEntity payer, ManagerEntity payee, decimal amount, int applicationId)
    {
        if (await stripeAccountService.GetAccountStatusAsync(payee.StripeAccountId) != PayoutAccountStatus.Verified)
            throw new BadRequestException("Payee has not completed Stripe verification");

        var paymentMethodId = await stripeAccountService.GetPaymentMethodAsync(payer.StripeCustomerId);

        var response = await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = paymentMethodId,
            FromUserEmail = payer.Email,
            StripeCustomerId = payer.StripeCustomerId,
            Amount = amount,
            DestinationStripeId = payee.StripeAccountId,
            Metadata = new Dictionary<string, string>
            {
                { "fromUserId", payer.Id.ToString() },
                { "toUserId", payee.Id.ToString() },
                { "type", "settlement" },
                { "applicationId", applicationId.ToString() }
            }
        });

        if (response.IsFailed)
            throw new InternalServerException(string.Join(", ", response.Errors.Select(e => e.Message)));

        if (response.Value.TransactionId is null)
            throw new InternalServerException("Payment did not return a valid PaymentIntent ID");

        await transactionService.LogAsync(new SettlementTransactionDto
        {
            ApplicationId = applicationId,
            FromUserId = payer.Id,
            ToUserId = payee.Id,
            PaymentIntentId = response.Value.TransactionId,
            Amount = (long)(amount * 100),
            Status = TransactionStatus.Pending,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        });
    }
}
