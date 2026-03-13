

namespace Core.Entities;

public class ConcertOpportunity : BaseEntity
{
    public int VenueId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Pay { get; set; }
    public Venue Venue { get; set; } = null!;
    public ICollection<ConcertApplication> Applications { get; set; } = [];
    public ICollection<OpportunityGenre> OpportunityGenres { get; set; } = [];
}
