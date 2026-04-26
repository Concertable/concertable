namespace Concertable.Customer.Application.Requests;

internal record CreatePreferenceRequest
{
    public int RadiusKm { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}
