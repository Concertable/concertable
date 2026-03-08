using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record ConcertDto : ItemDto
    {
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DatePosted { get; set; }
        public required VenueDto Venue { get; set; }
        public required ArtistDto Artist { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();

        public ConcertDto()
        {
            Type = "concert";
        }
    }

    public record ConcertHeaderDto : IHeader, IAddressHeader
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ImageUrl { get; set; }
        public double? Rating { get; set; }
        public required string County { get; set; }
        public required string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DatePosted { get; set; }
    }
}
