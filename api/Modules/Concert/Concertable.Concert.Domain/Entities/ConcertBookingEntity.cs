using System.ComponentModel.DataAnnotations.Schema;

namespace Concertable.Concert.Domain;

[Table("ConcertBookings")]
public class ConcertBookingEntity : IIdEntity
{
    public int Id { get; private set; }
    public int ApplicationId { get; private set; }
    public string? PaymentMethodId { get; private set; }
    public BookingStatus Status { get; private set; }
    public OpportunityApplicationEntity Application { get; set; } = null!;
    public ConcertEntity? Concert { get; private set; }

    private ConcertBookingEntity() { }

    public static ConcertBookingEntity Create(int applicationId) => new()
    {
        ApplicationId = applicationId,
        Status = BookingStatus.Pending
    };

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

    public void StorePaymentMethod(string? paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }
}
