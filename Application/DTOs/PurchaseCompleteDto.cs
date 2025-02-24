using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PurchaseCompleteDto
    {
        public int EntityId { get; set; } // EventId or ApplicationId
        public string TransactionId { get; set; }
        public int FromUserId { get; set; }
        public string FromEmail { get; set; }
        public int ToUserId { get; set; }
    }
}
