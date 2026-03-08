using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record ArtistDto : ItemDto
    {
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();
        public required string ImageUrl { get; set; }
        public required string County { get; set; }
        public required string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public required string Email { get; set; }

        public ArtistDto()
        {
            Type = "artist";
        }
    }

    public record ArtistHeaderDto : IHeader, IAddressHeader
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ImageUrl { get; set; }
        public double? Rating { get; set; }
        public required string County { get; set; }
        public required string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
