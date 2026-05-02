using static Concertable.Seeding.Extensions.EntityReflectionExtensions;

namespace Concertable.Seeding.Factories;

public static class BookingFactory
{
    public static StandardBooking Confirmed(ConcertEntity concert)
        => New<StandardBooking>()
            .With(nameof(BookingEntity.Status), BookingStatus.Confirmed)
            .With(nameof(BookingEntity.Concert), concert);

    public static DeferredBooking ConfirmedDeferred(ConcertEntity concert, string paymentMethodId = "pm_test")
        => New<DeferredBooking>()
            .With(nameof(BookingEntity.Status), BookingStatus.Confirmed)
            .With(nameof(BookingEntity.Concert), concert)
            .With(nameof(DeferredBooking.PaymentMethodId), paymentMethodId);

    public static StandardBooking AwaitingPayment(ConcertEntity? concert = null)
    {
        var booking = New<StandardBooking>()
            .With(nameof(BookingEntity.Status), BookingStatus.AwaitingPayment);

        if (concert is not null)
            booking.With(nameof(BookingEntity.Concert), concert);

        return booking;
    }

    public static StandardBooking Complete(ConcertEntity concert)
        => New<StandardBooking>()
            .With(nameof(BookingEntity.Status), BookingStatus.Complete)
            .With(nameof(BookingEntity.Concert), concert);

    public static DeferredBooking CompleteDeferred(ConcertEntity concert, string paymentMethodId = "pm_test")
        => New<DeferredBooking>()
            .With(nameof(BookingEntity.Status), BookingStatus.Complete)
            .With(nameof(BookingEntity.Concert), concert)
            .With(nameof(DeferredBooking.PaymentMethodId), paymentMethodId);
}
