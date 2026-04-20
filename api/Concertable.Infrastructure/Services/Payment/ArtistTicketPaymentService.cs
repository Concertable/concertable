using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

public class ArtistTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly ICurrentUserResolver currentUserResolver;
    private readonly IManagerRepository<ArtistManagerEntity> artistManagerRepository;

    public ArtistTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        ICurrentUserResolver currentUserResolver,
        IManagerRepository<ArtistManagerEntity> artistManagerRepository)
    {
        this.customerPaymentService = customerPaymentService;
        this.currentUserResolver = currentUserResolver;
        this.artistManagerRepository = artistManagerRepository;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = await currentUserResolver.ResolveAsync() as CustomerEntity
            ?? throw new ForbiddenException("Only customers can purchase tickets");
        var payee = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Artist manager not found for this concert");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
