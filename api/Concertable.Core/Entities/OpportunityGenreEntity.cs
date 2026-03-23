using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Entities;

[PrimaryKey(nameof(OpportunityId), nameof(GenreId))]
public class OpportunityGenreEntity : IGenreJoin
{
    public int OpportunityId { get; set; }
    public int GenreId { get; set; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
