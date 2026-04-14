using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Results;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class VenueTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly ICurrentUser currentUser;
    private readonly IManagerRepository<VenueManagerEntity> venueManagerRepository;

    public VenueTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        ICurrentUser currentUser,
        IManagerRepository<VenueManagerEntity> venueManagerRepository)
    {
        this.customerPaymentService = customerPaymentService;
        this.currentUser = currentUser;
        this.venueManagerRepository = venueManagerRepository;
    }

    public async Task<PaymentResult> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = currentUser.GetEntity<CustomerEntity>();
        var payee = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Venue manager not found for this concert");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
