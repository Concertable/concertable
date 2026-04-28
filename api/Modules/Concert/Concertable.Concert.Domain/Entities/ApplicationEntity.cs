namespace Concertable.Concert.Domain;

public class ApplicationEntity : IIdEntity
{
    public int Id { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistReadModel Artist { get; set; } = null!;
    public BookingEntity? Booking { get; set; }

    private ApplicationEntity() { }

    public static ApplicationEntity Create(int artistId, int opportunityId) => new()
    {
        ArtistId = artistId,
        OpportunityId = opportunityId
    };

    public void Accept(BookingEntity bookingConcert)
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
