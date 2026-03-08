using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Transaction : BaseEntity
    {
        public int FromUserId { get; set; }
        public ApplicationUser FromUser { get; set; } = null!;
        public int ToUserId { get; set; }
        public ApplicationUser ToUser { get; set; } = null!;
        public required string TransactionId { get; set; }
        public long Amount { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
