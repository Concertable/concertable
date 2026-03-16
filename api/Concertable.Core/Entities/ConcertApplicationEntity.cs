
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class ConcertApplicationEntity : BaseEntity
{
    public int OpportunityId { get; set; }
    public int ArtistId { get; set; }
    public ConcertOpportunityEntity Opportunity { get; set; } = null!;
    public ArtistEntity Artist { get; set; } = null!;
    public ConcertEntity? Concert { get; set; }
}
