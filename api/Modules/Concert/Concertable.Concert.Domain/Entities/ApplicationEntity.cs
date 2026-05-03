namespace Concertable.Concert.Domain;

public abstract class ApplicationEntity : IIdEntity
{
    public int Id { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Pending;
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistReadModel Artist { get; set; } = null!;
    public BookingEntity? Booking { get; set; }

    protected ApplicationEntity() { }

    protected ApplicationEntity(int artistId, int opportunityId)
    {
        ArtistId = artistId;
        OpportunityId = opportunityId;
    }

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

public sealed class StandardApplication : ApplicationEntity
{
    private StandardApplication() { }

    private StandardApplication(int artistId, int opportunityId)
        : base(artistId, opportunityId) { }

    public static StandardApplication Create(int artistId, int opportunityId) =>
        new(artistId, opportunityId);
}

public sealed class PrepaidApplication : ApplicationEntity
{
    public string PaymentMethodId { get; private set; } = null!;

    private PrepaidApplication() { }

    private PrepaidApplication(int artistId, int opportunityId, string paymentMethodId)
        : base(artistId, opportunityId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public static PrepaidApplication Create(int artistId, int opportunityId, string paymentMethodId) =>
        new(artistId, opportunityId, paymentMethodId);
}
