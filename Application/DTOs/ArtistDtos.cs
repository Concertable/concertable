using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record ArtistDto : ItemDto
    {
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
        public string ImageUrl { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Email { get; set; }

        public ArtistDto()
        {
            Type = "artist";
        }
    }

    public record ArtistHeaderDto : ISearchHeader, IAddressHeader
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
