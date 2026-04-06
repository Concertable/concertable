using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Rating;
using System.Text.Json.Serialization;

namespace Concertable.Application.Interfaces.Search;

[JsonDerivedType(typeof(ArtistHeaderDto), "artist")]
[JsonDerivedType(typeof(VenueHeaderDto), "venue")]
[JsonDerivedType(typeof(ConcertHeaderDto), "concert")]
public interface IHeader : IHasRating, IAddress
{
    string Name { get; set; }
    string ImageUrl { get; set; }
}
