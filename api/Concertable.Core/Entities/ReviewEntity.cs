using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class ReviewEntity : IEntity
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int Stars { get; set; }
    public string? Details { get; set; }
    public TicketEntity Ticket { get; set; } = null!;
}
