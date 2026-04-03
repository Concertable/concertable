using Concertable.Application.Interfaces;
using Concertable.Core.Enums;

namespace Concertable.Application.DTOs;

public record AdminDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
}

public record VenueManagerDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public int? VenueId { get; set; }
}

public record ArtistManagerDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public int? ArtistId { get; set; }
}

public record CustomerDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
}
