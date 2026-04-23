using System.ComponentModel.DataAnnotations.Schema;
using Concertable.Concert.Domain.Events;
using Concertable.Shared;

namespace Concertable.Concert.Domain;

[Table("Reviews")]
public class ReviewEntity : IIdEntity, IEventRaiser
{
    public int Id { get; private set; }
    public Guid TicketId { get; private set; }
    public byte Stars { get; private set; }
    public string? Details { get; private set; }
    public TicketEntity Ticket { get; set; } = null!;

    private readonly EventRaiser _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.DomainEvents;
    public void ClearDomainEvents() => _events.Clear();

    private ReviewEntity() { }

    public static ReviewEntity Create(
        Guid ticketId,
        byte stars,
        string? details,
        int artistId,
        int venueId,
        int concertId)
    {
        ValidateStars(stars);
        var review = new ReviewEntity { TicketId = ticketId, Stars = stars, Details = details };
        review._events.Raise(new ReviewCreatedDomainEvent(ticketId, artistId, venueId, concertId, stars));
        return review;
    }

    private static void ValidateStars(byte stars)
    {
        if (stars is < 1 or > 5)
            throw new DomainException("Stars must be between 1 and 5.");
    }
}
