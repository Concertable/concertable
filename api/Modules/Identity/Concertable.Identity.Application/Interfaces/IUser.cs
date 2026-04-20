using System.Text.Json.Serialization;
using Concertable.Identity.Application.DTOs;

namespace Concertable.Identity.Application.Interfaces;

[JsonDerivedType(typeof(VenueManagerDto), "venueManager")]
[JsonDerivedType(typeof(ArtistManagerDto), "artistManager")]
[JsonDerivedType(typeof(CustomerDto), "customer")]
[JsonDerivedType(typeof(AdminDto), "admin")]
public interface IUser
{
    Guid Id { get; set; }
    string Email { get; set; }
    Role? Role { get; set; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }
    string? County { get; set; }
    string? Town { get; set; }
    string BaseUrl { get; set; }
    bool IsEmailVerified { get; set; }
}
