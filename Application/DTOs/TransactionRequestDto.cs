using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionRequestDto
    {
        public string PaymentMethodId { get; set; }
        public string FromUserEmail { get; set; }
        public string? DestinationStripeId { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
