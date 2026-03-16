using Application.DTOs;
using Application.Interfaces.Rating;
using System.Text.Json.Serialization;

namespace Application.Interfaces.Search;

[JsonDerivedType(typeof(ArtistHeaderDto), "artist")]
[JsonDerivedType(typeof(VenueHeaderDto), "venue")]
[JsonDerivedType(typeof(ConcertHeaderDto), "concert")]
public interface IHeader : IHasRating
{
    string Name { get; set; }
    string ImageUrl { get; set; }
}
