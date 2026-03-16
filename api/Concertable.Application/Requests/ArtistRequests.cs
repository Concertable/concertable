using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests;

public record CreateArtistRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public IEnumerable<GenreDto> Genres { get; init; } = [];
    public required IFormFile Image { get; init; }
}

public record UpdateArtistRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public required string ImageUrl { get; init; }
    public IEnumerable<GenreDto> Genres { get; init; } = [];
    public IFormFile? Image { get; init; }
}
