using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class ConcertApplicationEntity : IEntity
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public int OpportunityId { get; set; }
    public int ArtistId { get; set; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertEntity? Concert { get; set; }
}
