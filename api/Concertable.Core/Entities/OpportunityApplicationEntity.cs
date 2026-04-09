using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Core.Entities;

public class OpportunityApplicationEntity : IIdEntity
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public int OpportunityId { get; set; }
    public int ArtistId { get; set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertEntity? Concert { get; set; }
}
