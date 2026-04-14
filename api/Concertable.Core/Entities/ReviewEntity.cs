using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Exceptions;

namespace Concertable.Core.Entities;

public class ReviewEntity : IIdEntity
{
    public int Id { get; private set; }
    public Guid TicketId { get; private set; }
    public byte Stars { get; private set; }
    public string? Details { get; private set; }
    public TicketEntity Ticket { get; set; } = null!;

    private ReviewEntity() { }

    public static ReviewEntity Create(Guid ticketId, byte stars, string? details)
    {
        ValidateStars(stars);
        return new() { TicketId = ticketId, Stars = stars, Details = details };
    }

    private static void ValidateStars(byte stars)
    {
        if (stars is < 1 or > 5)
            throw new DomainException("Stars must be between 1 and 5.");
    }
}
