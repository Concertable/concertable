

using Concertable.Core.Entities.BookingContracts;

namespace Core.Entities;

public class ConcertOpportunityEntity : BaseEntity
{
    public int VenueId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VenueEntity Venue { get; set; } = null!;
    public BookingContractEntity Contract { get; set; } = null!;
    public ICollection<ConcertApplicationEntity> Applications { get; set; } = [];
    public ICollection<OpportunityGenreEntity> OpportunityGenres { get; set; } = [];
}
