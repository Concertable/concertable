using Core.Entities.Interfaces;

namespace Core.Entities;

public class TicketEntity : IEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int ConcertId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[]? QrCode { get; set; } // Has to be nullable as TicketEntity needs to be initially created without QRCode
    public CustomerEntity User { get; set; } = null!;
    public ConcertEntity Concert { get; set; } = null!;
    public ReviewEntity? Review { get; set; }
}
