

namespace Core.Entities;

public class ReviewEntity : BaseEntity
{
    public int TicketId { get; set; }
    public int Stars { get; set; }
    public string? Details { get; set; }
    public TicketEntity Ticket { get; set; } = null!;
}
