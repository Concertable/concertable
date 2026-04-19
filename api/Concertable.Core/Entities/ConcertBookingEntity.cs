using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Core.Entities;

public class ConcertBookingEntity : IIdEntity
{
    public int Id { get; private set; }
    public int ApplicationId { get; private set; }
    public string? PaymentMethodId { get; private set; }
    public BookingStatus Status { get; private set; }
    public OpportunityApplicationEntity Application { get; set; } = null!;
    public ConcertEntity Concert { get; private set; } = null!;

    private ConcertBookingEntity() { }

    public static ConcertBookingEntity Create(ConcertEntity concert, string? paymentMethodId = null) => new()
    {
        Concert = concert,
        PaymentMethodId = paymentMethodId,
        Status = BookingStatus.Confirmed
    };

    public void AwaitPayment()
    {
        if (Status != BookingStatus.Confirmed)
            throw new DomainException("Only confirmed bookings can await payment.");
        Status = BookingStatus.AwaitingPayment;
    }

    public void Confirm()
    {
        if (Status != BookingStatus.AwaitingPayment)
            throw new DomainException("Only bookings awaiting payment can be confirmed.");
        Status = BookingStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != BookingStatus.AwaitingPayment && Status != BookingStatus.Confirmed)
            throw new DomainException("Only awaiting payment or confirmed bookings can be completed.");
        Status = BookingStatus.Complete;
    }

    public void FailPayment()
    {
        if (Status != BookingStatus.AwaitingPayment)
            throw new DomainException("Only bookings awaiting payment can fail.");
        Status = BookingStatus.PaymentFailed;
    }
}
