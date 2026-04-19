
namespace Concertable.Core.Entities;

public class TicketEntity : IGuidEntity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int ConcertId { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public byte[] QrCode { get; private set; } = null!;
    public CustomerEntity User { get; set; } = null!;
    public ConcertEntity Concert { get; set; } = null!;
    public ReviewEntity? Review { get; set; }

    private TicketEntity() { }

    public static TicketEntity Create(Guid id, Guid userId, int concertId, byte[] qrCode, DateTime purchaseDate) => new()
    {
        Id = id,
        UserId = userId,
        ConcertId = concertId,
        QrCode = qrCode,
        PurchaseDate = purchaseDate
    };
}
