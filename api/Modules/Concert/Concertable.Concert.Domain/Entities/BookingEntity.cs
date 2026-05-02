namespace Concertable.Concert.Domain;

public abstract class BookingEntity : IIdEntity
{
    public int Id { get; private set; }
    public int ApplicationId { get; private set; }
    public BookingStatus Status { get; private set; }
    public ApplicationEntity Application { get; set; } = null!;
    public ConcertEntity? Concert { get; private set; }

    protected BookingEntity() { }

    public void AwaitPayment()
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
            throw new DomainException("Only pending or confirmed bookings can await payment.");
        Status = BookingStatus.AwaitingPayment;
    }

    public void Confirm(ConcertEntity concert)
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.AwaitingPayment)
            throw new DomainException("Only pending or awaiting payment bookings can be confirmed.");
        Concert = concert;
        Status = BookingStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != BookingStatus.AwaitingPayment && Status != BookingStatus.Confirmed)
            throw new DomainException("Only awaiting payment or confirmed bookings can be completed.");
        if (DateTime.UtcNow < Application.Opportunity.Period.End)
            throw new DomainException("Booking cannot be completed before the concert has ended.");
        Status = BookingStatus.Complete;
    }

    public void FailPayment()
    {
        if (Status != BookingStatus.AwaitingPayment)
            throw new DomainException("Only bookings awaiting payment can fail.");
        Status = BookingStatus.PaymentFailed;
    }

    protected static T Init<T>(T booking, int applicationId) where T : BookingEntity
    {
        booking.ApplicationId = applicationId;
        booking.Status = BookingStatus.Pending;
        return booking;
    }
}

public sealed class StandardBooking : BookingEntity
{
    private StandardBooking() { }

    public static StandardBooking Create(int applicationId) =>
        Init(new StandardBooking(), applicationId);
}

public sealed class DeferredBooking : BookingEntity
{
    public string PaymentMethodId { get; private set; } = null!;

    private DeferredBooking() { }

    public static DeferredBooking Create(int applicationId, string paymentMethodId)
    {
        var booking = Init(new DeferredBooking(), applicationId);
        booking.PaymentMethodId = paymentMethodId;
        return booking;
    }
}
