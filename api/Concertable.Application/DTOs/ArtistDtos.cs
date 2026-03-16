using Application.Interfaces;
using Application.Interfaces.Search;

namespace Application.DTOs;

public record ArtistDto : IDetails, IAddress, ILatLong
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public double Rating { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
    public required string ImageUrl { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public required string Email { get; set; }
}

public record ArtistHeaderDto : IHeader, IAddress, ILatLong
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public double? Rating { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}
