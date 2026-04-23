using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Payment;
using Concertable.Application.Responses;
using Concertable.Concert.Application.Interfaces;
using Concertable.Identity.Contracts;
using Concertable.Shared.Exceptions;
using FluentResults;

namespace Concertable.Infrastructure.Services.Payment;

internal class ArtistTicketPaymentService : ITicketPaymentStrategy
{
    private readonly ICustomerPaymentService customerPaymentService;
    private readonly IAuthModule authModule;
    private readonly ICurrentUser currentUser;
    private readonly IManagerModule managerModule;
    private readonly IConcertBookingRepository bookingRepository;

    public ArtistTicketPaymentService(
        ICustomerPaymentService customerPaymentService,
        IAuthModule authModule,
        ICurrentUser currentUser,
        IManagerModule managerModule,
        IConcertBookingRepository bookingRepository)
    {
        this.customerPaymentService = customerPaymentService;
        this.authModule = authModule;
        this.currentUser = currentUser;
        this.managerModule = managerModule;
        this.bookingRepository = bookingRepository;
    }

    public async Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price)
    {
        var payer = await authModule.GetCustomerAsync(currentUser.GetId())
            ?? throw new ForbiddenException("Only customers can purchase tickets");

        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found for concert");
        var payee = await managerModule.GetByIdAsync(booking.Application.Artist.UserId)
            ?? throw new NotFoundException("Artist manager not found");

        return await customerPaymentService.PayAsync(payer, payee, concertId, quantity, paymentMethodId, price);
    }
}
