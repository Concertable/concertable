namespace Concertable.Customer.Application.Requests;

internal record CreatePreferenceRequest
{
    public int RadiusKm { get; set; }
    public IReadOnlyList<GenreDto> Genres { get; set; } = [];
}
