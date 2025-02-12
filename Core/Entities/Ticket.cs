
using Core.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Ticket : BaseEntity
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public Customer User { get; set; }
        public Event Event { get; set; }
        public Review? Review { get; set; }
    }
}
