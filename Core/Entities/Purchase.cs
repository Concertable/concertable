using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Purchase : BaseEntity
    {
        public int FromUserId { get; set; }  
        public ApplicationUser FromUser { get; set; }
        public int ToUserId { get; set; }
        public ApplicationUser ToUser { get; set; }
        public string TransactionId { get; set; }
        public long Amount { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
