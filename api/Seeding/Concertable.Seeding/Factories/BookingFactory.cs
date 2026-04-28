using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class BookingFactory
{
    public static BookingEntity Confirmed(ConcertEntity concert)
        => New<BookingEntity>()
            .With(nameof(BookingEntity.Status), BookingStatus.Confirmed)
            .With(nameof(BookingEntity.Concert), concert);

    public static BookingEntity AwaitingPayment(ConcertEntity? concert = null)
    {
        var booking = New<BookingEntity>()
            .With(nameof(BookingEntity.Status), BookingStatus.AwaitingPayment);

        if (concert is not null)
            booking.With(nameof(BookingEntity.Concert), concert);

        return booking;
    }

    public static BookingEntity Complete(ConcertEntity concert)
        => New<BookingEntity>()
            .With(nameof(BookingEntity.Status), BookingStatus.Complete)
            .With(nameof(BookingEntity.Concert), concert);
}
