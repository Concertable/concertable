using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class ReviewEntity : IIdEntity
{
    public int Id { get; set; }
    public Guid TicketId { get; set; }
    public int Stars { get; set; }
    public string? Details { get; set; }
    public TicketEntity Ticket { get; set; } = null!;
}
