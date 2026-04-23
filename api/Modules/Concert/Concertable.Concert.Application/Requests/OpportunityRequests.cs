using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Requests;

internal record OpportunityRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<int> GenreIds { get; init; } = [];
    public required IContract Contract { get; init; }
}
