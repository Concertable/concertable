using Concertable.Identity.Contracts;
using Concertable.Payment.Application.Interfaces;
using Concertable.Payment.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Payment.Infrastructure;

internal class OffSessionManagerPaymentModule : IManagerPaymentModule
{
    private readonly IPaymentService paymentService;
    private readonly IStripeAccountService stripeAccountService;
    private readonly IPayoutAccountRepository payoutAccountRepository;
    private readonly IManagerModule managerModule;

    public OffSessionManagerPaymentModule(
        [FromKeyedServices(PaymentSession.OffSession)] IPaymentService paymentService,
        IStripeAccountService stripeAccountService,
        IPayoutAccountRepository payoutAccountRepository,
        IManagerModule managerModule)
    {
        this.paymentService = paymentService;
        this.stripeAccountService = stripeAccountService;
        this.payoutAccountRepository = payoutAccountRepository;
        this.managerModule = managerModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        Guid payerUserId,
        Guid payeeUserId,
        decimal amount,
        IDictionary<string, string>? metadata,
        string? paymentMethodId,
        CancellationToken ct = default)
    {
        var payer = await managerModule.GetByIdAsync(payerUserId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerUserId}");
        var payee = await managerModule.GetByIdAsync(payeeUserId)
            ?? throw new NotFoundException($"Payee manager not found for userId {payeeUserId}");

        var payerAccount = await payoutAccountRepository.GetByUserIdAsync(payerUserId)
            ?? throw new NotFoundException($"Payout account not found for payer {payerUserId}");
        var payeeAccount = await payoutAccountRepository.GetByUserIdAsync(payeeUserId)
            ?? throw new NotFoundException($"Payout account not found for payee {payeeUserId}");

        var payerStripeCustomerId = payerAccount.StripeCustomerId
            ?? throw new BadRequestException("Payer has no Stripe customer ID");
        var payeeStripeAccountId = payeeAccount.StripeAccountId
            ?? throw new BadRequestException("Payee has not completed Stripe verification");

        if (await stripeAccountService.GetAccountStatusAsync(payeeStripeAccountId) != PayoutAccountStatus.Verified)
            throw new BadRequestException("Payee has not completed Stripe verification");

        var resolvedPaymentMethodId = paymentMethodId
            ?? await stripeAccountService.GetPaymentMethodAsync(payerStripeCustomerId);

        var mergedMetadata = new Dictionary<string, string>
        {
            ["fromUserId"] = payerUserId.ToString(),
            ["fromUserEmail"] = payer.Email ?? string.Empty,
            ["toUserId"] = payeeUserId.ToString(),
            ["amount"] = ((long)(amount * 100)).ToString()
        }
        .Merge(metadata);

        return await paymentService.ProcessAsync(new TransactionRequest
        {
            PaymentMethodId = resolvedPaymentMethodId,
            FromUserEmail = payer.Email ?? string.Empty,
            StripeCustomerId = payerStripeCustomerId,
            DestinationStripeId = payeeStripeAccountId,
            Amount = amount,
            Metadata = mergedMetadata
        });
    }
}
