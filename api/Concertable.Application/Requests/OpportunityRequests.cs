using Concertable.Application.Interfaces.Concert;

namespace Concertable.Application.Requests;

public record OpportunityRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<int> GenreIds { get; init; } = [];
    public required IContract Contract { get; init; }
}
