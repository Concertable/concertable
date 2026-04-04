using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using System.Text.Json.Serialization;

namespace Concertable.Application.Interfaces;

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
}
