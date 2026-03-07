using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record VenueDto : ItemDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public bool Approved { get; set; } = false;
        public string Email { get; set; }

        public VenueDto()
        {
            Type = "venue";
        }
    }

    public record VenueHeaderDto : ISearchHeader, IAddressHeader
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
