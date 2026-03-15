namespace Core.Entities;

public class TicketEntity : BaseEntity
{
    public int UserId { get; set; }
    public int ConcertId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[]? QrCode { get; set; } // Has to be nullable as TicketEntity needs to be initially created without QRCode
    public Customer User { get; set; } = null!;
    public ConcertEntity Concert { get; set; } = null!;
    public ReviewEntity? Review { get; set; }
}
