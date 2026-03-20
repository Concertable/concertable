
using Core.Enums;

namespace Core.Entities;

public class ConcertApplicationEntity : BaseEntity
{
    public int OpportunityId { get; set; }
    public int ArtistId { get; set; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertEntity? Concert { get; set; }
}

public class FlatFeeApplicationEntity : ConcertApplicationEntity
{
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
}

public class DoorSplitApplicationEntity : ConcertApplicationEntity
{
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
}

public class VersusApplicationEntity : ConcertApplicationEntity
{
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
}

public class VenueHireApplicationEntity : ConcertApplicationEntity
{
    public VenueHireApplicationStatus Status { get; set; } = VenueHireApplicationStatus.Pending;
}
