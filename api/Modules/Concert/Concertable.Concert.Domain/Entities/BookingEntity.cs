namespace Concertable.Concert.Domain;

public abstract class BookingEntity : IIdEntity
{
    public int Id { get; private set; }
    public int ApplicationId { get; private set; }
    public BookingStatus Status { get; private set; }
    public ApplicationEntity Application { get; set; } = null!;
    public ConcertEntity? Concert { get; private set; }

    protected BookingEntity() { }

    protected BookingEntity(int applicationId)
    {
        ApplicationId = applicationId;
        Status = BookingStatus.Pending;
    }

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
}

public sealed class StandardBooking : BookingEntity
{
    private StandardBooking() { }

    private StandardBooking(int applicationId) : base(applicationId) { }

    public static StandardBooking Create(int applicationId) =>
        new(applicationId);
}

public sealed class DeferredBooking : BookingEntity
{
    public string PaymentMethodId { get; private set; } = null!;

    private DeferredBooking() { }

    private DeferredBooking(int applicationId, string paymentMethodId) : base(applicationId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public static DeferredBooking Create(int applicationId, string paymentMethodId) =>
        new(applicationId, paymentMethodId);
}
