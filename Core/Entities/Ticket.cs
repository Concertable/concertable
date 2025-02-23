
using Core.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Ticket : BaseEntity
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public byte[]? QrCode { get; set; } // Has to be nullable as Ticket needs to be initially created without QRCode
        public Customer User { get; set; }
        public Event Event { get; set; }
        public Review? Review { get; set; }
    }
}
