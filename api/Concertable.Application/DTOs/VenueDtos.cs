using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;

namespace Concertable.Application.DTOs;

public record VenueDto : IDetails, IAddress, ILatLong
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public double Rating { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public required string ImageUrl { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
    public bool Approved { get; set; } = false;
    public required string Email { get; set; }
}

public record VenueSummaryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public double Rating { get; set; }
}

public record VenueHeaderDto : IHeader, IAddress
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public double? Rating { get; set; }
    public required string County { get; set; }
    public required string Town { get; set; }
}
