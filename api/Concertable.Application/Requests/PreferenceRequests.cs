using Application.DTOs;

namespace Application.Requests;

public record CreatePreferenceRequest
{
    public int RadiusKm { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}
