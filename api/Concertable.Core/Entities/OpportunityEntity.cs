using Concertable.Core.Entities.Contracts;
using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class OpportunityEntity : IEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VenueEntity Venue { get; set; } = null!;
    public ContractEntity Contract { get; set; } = null!;
    public ICollection<OpportunityApplicationEntity> Applications { get; set; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
}
