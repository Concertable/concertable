using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Responses
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string ClientSecret { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
