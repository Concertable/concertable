
using Core.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Ticket : BaseEntity
    {
        public int UserId { get; set; }
        public int ConcertId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public byte[]? QrCode { get; set; } // Has to be nullable as Ticket needs to be initially created without QRCode
        public Customer User { get; set; } = null!;
        public Concert Concert { get; set; } = null!;
        public Review? Review { get; set; }
    }
}
