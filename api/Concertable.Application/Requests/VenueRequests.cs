using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Requests;

public record CreateVenueRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required IFormFile Image { get; init; }
}

public record UpdateVenueRequest
{
    public required string Name { get; init; }
    public required string About { get; init; }
    public bool Approved { get; init; }
    public ImageDto? Image { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
