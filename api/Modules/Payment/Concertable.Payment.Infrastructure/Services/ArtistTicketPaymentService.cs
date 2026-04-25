using Concertable.Contract.Abstractions;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Payment.Infrastructure.Services;

internal class ArtistTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly ICustomerModule customerModule;
    private readonly IManagerModule managerModule;

    public ArtistTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        ICustomerModule customerModule,
        IManagerModule managerModule)
    {
        this.customerPaymentService = customerPaymentService;
        this.customerModule = customerModule;
        this.managerModule = managerModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(
        int concertId, int quantity, string? paymentMethodId, decimal price,
        Guid customerUserId, Guid payeeUserId,
        IContract contract)
    {
        var payer = await customerModule.GetCustomerAsync(customerUserId)
            ?? throw new ForbiddenException("Only customers can purchase tickets");
        var payee = await managerModule.GetByIdAsync(payeeUserId)
            ?? throw new NotFoundException("Artist manager not found");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
