
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class ConcertApplication : BaseEntity
{
    public int OpportunityId { get; set; }
    public int ArtistId { get; set; }
    public ConcertOpportunity Opportunity { get; set; } = null!;
    public Artist Artist { get; set; } = null!;
    public Concert? Concert { get; set; }
}
