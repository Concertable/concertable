using Application.DTOs;
using System.Text.Json.Serialization;

namespace Application.Interfaces.Search
{
    [JsonDerivedType(typeof(ArtistHeaderDto), "artist")]
    [JsonDerivedType(typeof(VenueHeaderDto), "venue")]
    [JsonDerivedType(typeof(ConcertHeaderDto), "concert")]
    public interface IHeader
    {
        int Id { get; set; }
        string Name { get; set; }
        string ImageUrl { get; set; }
        double? Rating { get; set; }
    }
}
