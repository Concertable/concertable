using Concertable.Application.DTOs;

namespace Concertable.Application.Responses;

public record ConcertPostResponse
{
    public required ConcertDto Concert { get; set; }
    public required ConcertHeaderDto ConcertHeader { get; set; }
    public IEnumerable<Guid> UserIds { get; set; } = [];
}
