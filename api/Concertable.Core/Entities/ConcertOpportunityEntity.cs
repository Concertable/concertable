using Concertable.Core.Entities.Contracts;
using Core.Entities.Interfaces;

namespace Core.Entities;

public class ConcertOpportunityEntity : IEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VenueEntity Venue { get; set; } = null!;
    public ContractEntity Contract { get; set; } = null!;
    public ICollection<ConcertApplicationEntity> Applications { get; set; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
}
