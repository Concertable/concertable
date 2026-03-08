using Application.DTOs;
using System.Text.Json.Serialization;

namespace Application.Interfaces.Search
{
    [JsonPolymorphic]
    [JsonDerivedType(typeof(ArtistHeaderDto), "artist")]
    [JsonDerivedType(typeof(VenueHeaderDto), "venue")]
    [JsonDerivedType(typeof(ConcertHeaderDto), "concert")]
    public interface ISearchHeader
    {
        int Id { get; set; }
        string Name { get; set; }
        string ImageUrl { get; set; }
        double? Rating { get; set; }
    }
}
