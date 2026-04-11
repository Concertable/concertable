using Concertable.Application.DTOs;

namespace Concertable.Application.Results;

public record ConcertPostResult
{
    public required ConcertDto Concert { get; set; }
    public required ConcertHeaderDto ConcertHeader { get; set; }
    public IEnumerable<Guid> UserIds { get; set; } = [];
}
