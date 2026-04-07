using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;

namespace Concertable.Application.DTOs;

public record ArtistDto : IDetails, IAddress
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public double Rating { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
    public required string BannerUrl { get; set; }
    public string? Avatar { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public required string Email { get; set; }
}

public record ArtistSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Avatar { get; set; }
    public required string BannerUrl { get; set; }
    public double Rating { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}

public record ArtistHeaderDto : IHeader, IAddress
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public double? Rating { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
}
