using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Core.Entities;

public class StripeEventEntity
{
    public required string EventId { get; set; }
    public DateTime EventProcessedAt { get; set; }
}
