using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PaymentDto
    {
        public decimal Amount { get; set; } 
        public string Currency { get; set; } = "GBP"; 
        public string PaymentMethodId { get; set; }
        public string Description { get; set; }
        public int EventId { get; set; } 
        public int UserId { get; set; } 
    }

}
