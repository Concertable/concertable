using Concertable.Core.Entities.Contracts;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.ValueObjects;

namespace Concertable.Core.Entities;

public class OpportunityEntity : IIdEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public DateRange Period { get; set; } = null!;
    public VenueEntity Venue { get; set; } = null!;
    public ContractEntity Contract { get; set; } = null!;
    public ICollection<OpportunityApplicationEntity> Applications { get; set; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
}
