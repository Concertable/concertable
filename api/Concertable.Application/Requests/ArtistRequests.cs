using Concertable.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Concertable.Application.Requests;

public record CreateArtistRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public IEnumerable<GenreDto> Genres { get; init; } = [];
    public required IFormFile Banner { get; init; }
}

public record UpdateArtistRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public ImageDto? Banner { get; init; }
    public IEnumerable<GenreDto> Genres { get; init; } = [];
}
