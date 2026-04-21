using Concertable.Artist.Domain;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class OpportunityApplicationEntity : IIdEntity
{
    public int Id { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertBookingEntity? Booking { get; set; }

    private OpportunityApplicationEntity() { }

    public static OpportunityApplicationEntity Create(int artistId, int opportunityId) => new()
    {
        ArtistId = artistId,
        OpportunityId = opportunityId
    };

    public void Accept(ConcertBookingEntity bookingConcert)
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be accepted.");
        Status = ApplicationStatus.Accepted;
        Booking = bookingConcert;
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
}
