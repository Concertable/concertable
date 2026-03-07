using Application.Interfaces.Search;

namespace Application.DTOs
{
    public class ArtistHeaderDto : ISearchHeader, IAddressHeader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public double? Rating { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
