using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class StripeEvent
    {
        public string EventId { get; set; }
        public DateTime EventProcessedAt { get; set; }
    }
}
