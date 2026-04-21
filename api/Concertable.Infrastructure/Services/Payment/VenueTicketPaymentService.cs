using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

public class VenueTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly IAuthModule authModule;
    private readonly ICurrentUser currentUser;
    private readonly IManagerModule managerModule;

    public VenueTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        IAuthModule authModule,
        ICurrentUser currentUser,
        IManagerModule managerModule)
    {
        this.customerPaymentService = customerPaymentService;
        this.authModule = authModule;
        this.currentUser = currentUser;
        this.managerModule = managerModule;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = await authModule.GetCustomerAsync(currentUser.GetId())
            ?? throw new ForbiddenException("Only customers can purchase tickets");
        var payee = await managerModule.GetVenueManagerByConcertIdAsync(concertId);

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
