using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;

namespace Concertable.Core.Entities;

public class OpportunityApplicationEntity : IIdEntity
{
    public int Id { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public string? PaymentMethodId { get; private set; }
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertEntity? Concert { get; set; }

    private OpportunityApplicationEntity() { }

    public static OpportunityApplicationEntity Create(int artistId, int opportunityId) => new()
    {
        ArtistId = artistId,
        OpportunityId = opportunityId
    };

    public void Accept(ConcertEntity concert)
    {
        if (Status != ApplicationStatus.Pending && Status != ApplicationStatus.AwaitingPayment)
            throw new DomainException("Only pending or awaiting payment applications can be accepted.");
        Status = ApplicationStatus.Accepted;
        Concert = concert;
    }

    public void Reject()
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be rejected.");
        Status = ApplicationStatus.Rejected;
    }

    public void Withdraw()
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be withdrawn.");
        Status = ApplicationStatus.Withdrawn;
    }

    public void StorePaymentMethod(string? paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public void AwaitPayment()
    {
        if (Status != ApplicationStatus.Pending && Status != ApplicationStatus.Accepted)
            throw new DomainException("Only pending or accepted applications can await payment.");
        Status = ApplicationStatus.AwaitingPayment;
    }

    public void FailPayment()
    {
        if (Status != ApplicationStatus.AwaitingPayment)
            throw new DomainException("Only applications awaiting payment can fail payment.");
        Status = ApplicationStatus.PaymentFailed;
    }

    public void Complete()
    {
        if (Status != ApplicationStatus.AwaitingPayment && Status != ApplicationStatus.Accepted)
            throw new DomainException("Only awaiting payment or accepted applications can be completed.");
        Status = ApplicationStatus.Complete;
    }
}
