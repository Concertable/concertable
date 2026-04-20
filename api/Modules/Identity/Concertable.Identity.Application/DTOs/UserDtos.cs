
namespace Concertable.Identity.Application.DTOs;

public record LoginResponse(IUser User, string AccessToken, string RefreshToken, int ExpiresInSeconds);

public record AdminDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role? Role { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public string BaseUrl { get; set; } = "/admin";
    public bool IsEmailVerified { get; set; }
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
    public string BaseUrl { get; set; } = "/venue";
    public bool IsEmailVerified { get; set; }
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
    public string BaseUrl { get; set; } = "/artist";
    public bool IsEmailVerified { get; set; }
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
    public string BaseUrl { get; set; } = "/";
    public bool IsEmailVerified { get; set; }
}
