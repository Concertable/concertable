using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Parameters
{
    public class TicketPurchaseParams
    {
        public string PaymentMethodId { get; set; }
        public int EventId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
