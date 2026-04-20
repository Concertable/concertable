using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

public class VenueTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly ICurrentUserResolver currentUserResolver;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;

    public VenueTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        ICurrentUserResolver currentUserResolver,
        IManagerRepository<VenueManagerEntity> venueManagerRepository)
    {
        this.customerPaymentService = customerPaymentService;
        this.currentUserResolver = currentUserResolver;
        this.venueManagerRepository = venueManagerRepository;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = await currentUserResolver.ResolveAsync() as CustomerEntity
            ?? throw new ForbiddenException("Only customers can purchase tickets");
        var payee = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
