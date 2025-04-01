using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PurchaseDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string TransactionId { get; set; }
        public long Amount { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
