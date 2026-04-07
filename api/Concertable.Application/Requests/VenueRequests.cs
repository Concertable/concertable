using Concertable.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Concertable.Application.Requests;

public record CreateVenueRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required IFormFile Banner { get; init; }
}

public record UpdateVenueRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public bool Approved { get; init; }
    public ImageDto? Banner { get; init; }
    public IFormFile? Avatar { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
