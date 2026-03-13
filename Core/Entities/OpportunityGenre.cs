using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey(nameof(OpportunityId), nameof(GenreId))]
public class OpportunityGenre
{
    public int OpportunityId { get; set; }
    public int GenreId { get; set; }
    public ConcertOpportunity Opportunity { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
