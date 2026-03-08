using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests;

public record CreateArtistRequest
{
    public required string Name { get; set; }
    public required string About { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record ArtistCreateRequest(CreateArtistRequest Artist, IFormFile Image);

public record ArtistUpdateRequest
{
    public required ArtistDto Artist { get; set; }
    public IFormFile? Image { get; set; }
}
