using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class TicketPurchaseResponse : PurchaseResponse
    {
        public IEnumerable<int> TicketIds { get; set; }
        public int EventId { get; set; }
    }

}
