using Concertable.Contract.Abstractions;
using Concertable.Payment.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Payment.Infrastructure;

internal class PaymentModule : IPaymentModule
{
    private readonly ITicketPaymentStrategyFactory ticketPaymentStrategyFactory;
    private readonly IManagerPaymentService managerPaymentService;
    private readonly IManagerModule managerModule;
    private readonly IStripeAccountService stripeAccountService;

    public PaymentModule(
        ITicketPaymentStrategyFactory ticketPaymentStrategyFactory,
        IManagerPaymentService managerPaymentService,
        IManagerModule managerModule,
        IStripeAccountService stripeAccountService)
    {
        this.ticketPaymentStrategyFactory = ticketPaymentStrategyFactory;
        this.managerPaymentService = managerPaymentService;
        this.managerModule = managerModule;
        this.stripeAccountService = stripeAccountService;
    }

    public Task ProvisionCustomerAsync(Guid userId, string email, CancellationToken ct = default) =>
        stripeAccountService.ProvisionCustomerAsync(userId, email, ct);

    public Task ProvisionConnectAccountAsync(Guid userId, string email, CancellationToken ct = default) =>
        stripeAccountService.ProvisionConnectAccountAsync(userId, email, ct);

    public Task<Result<PaymentResponse>> PurchaseTicketsAsync(
        int concertId,
        int quantity,
        string? paymentMethodId,
        decimal price,
        Guid customerUserId,
        Guid payeeUserId,
        IContract contract,
        CancellationToken ct = default)
    {
        var strategy = ticketPaymentStrategyFactory.Create(contract.ContractType);
        return strategy.PayAsync(concertId, quantity, paymentMethodId, price, customerUserId, payeeUserId, contract);
    }

    public async Task<Result<PaymentResponse>> SettleBookingAsync(
        int bookingId,
        Guid payerUserId,
        Guid payeeUserId,
        decimal amount,
        IContract contract,
        CancellationToken ct = default)
    {
        var payer = await managerModule.GetByIdAsync(payerUserId)
            ?? throw new NotFoundException($"Payer manager not found for userId {payerUserId}");
        var payee = await managerModule.GetByIdAsync(payeeUserId)
            ?? throw new NotFoundException($"Payee manager not found for userId {payeeUserId}");

        var response = await managerPaymentService.PayAsync(payer, payee, amount, bookingId);
        return Result.Ok(response);
    }
}
