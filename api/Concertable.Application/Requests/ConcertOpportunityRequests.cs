using Application.DTOs;
using Application.Interfaces.Concert;

namespace Application.Requests;

public record ConcertOpportunityRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<GenreDto> Genres { get; init; } = [];
    public required IContract Contract { get; init; }
}
