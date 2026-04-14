using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using FluentResults;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;

namespace Concertable.Infrastructure.Services.Payment;

public class ArtistTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly ICurrentUser currentUser;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;

    public ArtistTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        ICurrentUser currentUser,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository)
    {
        this.customerPaymentService = customerPaymentService;
        this.currentUser = currentUser;
        this.artistManagerRepository = artistManagerRepository;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = currentUser.GetEntity<CustomerEntity>();
        var payee = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found for this concert");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
