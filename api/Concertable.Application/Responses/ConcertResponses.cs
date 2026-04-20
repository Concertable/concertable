using Concertable.Application.DTOs;

namespace Concertable.Application.Responses;

public record ConcertPostResponse
{
    public required ConcertSnapshot ConcertHeader { get; set; }
    public IEnumerable<Guid> UserIds { get; set; } = [];
}

public record ConcertUpdateResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
}
