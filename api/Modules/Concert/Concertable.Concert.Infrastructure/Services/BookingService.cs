using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal class BookingService : IBookingService
{
    private readonly IBookingRepository bookingRepository;
    private readonly IApplicationAcceptor applicationAcceptor;

    public BookingService(
        IBookingRepository bookingRepository,
        IApplicationAcceptor applicationAcceptor)
    {
        this.bookingRepository = bookingRepository;
        this.applicationAcceptor = applicationAcceptor;
    }

    public async Task<BookingEntity> CreateAsync(int applicationId)
    {
        var booking = BookingEntity.Create(applicationId);
        booking.AwaitPayment();
        await bookingRepository.AddAsync(booking);
        await applicationAcceptor.AcceptAsync(applicationId, booking);
        return booking;
    }

    public async Task<BookingEntity> CreateAsync(int applicationId, string? paymentMethodId)
    {
        var booking = BookingEntity.Create(applicationId);
        booking.StorePaymentMethod(paymentMethodId);
        await bookingRepository.AddAsync(booking);
        await applicationAcceptor.AcceptAsync(applicationId, booking);
        return booking;
    }

    public async Task<BookingEntity> MarkAwaitingPaymentByConcertIdAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");
        booking.AwaitPayment();
        await bookingRepository.SaveChangesAsync();
        return booking;
    }

    public async Task CompleteByConcertIdAsync(int concertId)
    {
        var booking = await bookingRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("Booking not found");
        booking.Complete();
        await bookingRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(int bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking not found");
        booking.Complete();
        await bookingRepository.SaveChangesAsync();
    }
}
