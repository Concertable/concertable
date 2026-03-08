using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record VenueDto : ItemDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public required string ImageUrl { get; set; }
        public required string County { get; set; }
        public required string Town { get; set; }
        public bool Approved { get; set; } = false;
        public required string Email { get; set; }

        public VenueDto()
        {
            Type = "venue";
        }
    }

    public record VenueHeaderDto : IHeader, IAddressHeader
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
