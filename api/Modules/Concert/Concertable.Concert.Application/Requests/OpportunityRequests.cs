using Concertable.Application.Diffing;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Requests;

internal record OpportunityRequest : ISyncRequest
{
    public int? Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<int> GenreIds { get; init; } = [];
    public required IContract Contract { get; init; }
}
