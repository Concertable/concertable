using Concertable.Core.Enums;
using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class ConcertBookingFactory
{
    public static ConcertBookingEntity Confirmed(ConcertEntity concert)
        => New<ConcertBookingEntity>()
            .With(nameof(ConcertBookingEntity.Status), BookingStatus.Confirmed)
            .With(nameof(ConcertBookingEntity.Concert), concert);

    public static ConcertBookingEntity AwaitingPayment(ConcertEntity? concert = null)
    {
        var booking = New<ConcertBookingEntity>()
            .With(nameof(ConcertBookingEntity.Status), BookingStatus.AwaitingPayment);

        if (concert is not null)
            booking.With(nameof(ConcertBookingEntity.Concert), concert);

        return booking;
    }

    public static ConcertBookingEntity Complete(ConcertEntity concert)
        => New<ConcertBookingEntity>()
            .With(nameof(ConcertBookingEntity.Status), BookingStatus.Complete)
            .With(nameof(ConcertBookingEntity.Concert), concert);
}
