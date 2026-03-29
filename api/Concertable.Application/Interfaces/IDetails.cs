using Concertable.Application.DTOs;
using System.Text.Json.Serialization;

namespace Concertable.Application.Interfaces;

[JsonDerivedType(typeof(ArtistDto), "artist")]
[JsonDerivedType(typeof(VenueDto), "venue")]
[JsonDerivedType(typeof(ConcertDto), "concert")]
public interface IDetails
{
    int Id { get; set; }
    string Name { get; set; }
    string About { get; set; }
    double Rating { get; set; }
}
