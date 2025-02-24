using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public bool RequiresAction { get; set; }
        public string Message { get; set; }
        public string ClientSecret { get; set; }
        public string TransactionId { get; set; }
    }
}
