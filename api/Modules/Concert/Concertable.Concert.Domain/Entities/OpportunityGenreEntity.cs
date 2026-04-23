using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Concert.Domain;

[Table("OpportunityGenres")]
[PrimaryKey(nameof(OpportunityId), nameof(GenreId))]
public class OpportunityGenreEntity : IGenreJoin
{
    public int OpportunityId { get; set; }
    public int GenreId { get; set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;
}
