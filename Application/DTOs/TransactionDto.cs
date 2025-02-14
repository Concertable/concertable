using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionDto
    {
        public int FromUserId { get; set; }
        public string FromUserEmail { get; set; }
        public int ToUserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
    }
}
