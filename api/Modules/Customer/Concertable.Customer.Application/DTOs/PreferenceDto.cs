namespace Concertable.Customer.Application.DTOs;

internal record PreferenceDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int RadiusKm { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}
