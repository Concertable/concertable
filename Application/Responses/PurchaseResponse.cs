using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class PurchaseResponse
    {
        public bool Success { get; set; }
        public bool RequiresAction { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string TransactionId { get; set; }
        public string ClientSecret { get; set; }
        public string UserEmail { get; set; }
    }
}
