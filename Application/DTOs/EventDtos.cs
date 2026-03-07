using Application.Interfaces.Search;

namespace Application.DTOs
{
    public record EventDto : ItemDto
    {
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DatePosted { get; set; }
        public VenueDto Venue { get; set; }
        public ArtistDto Artist { get; set; }
        public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();

        public EventDto()
        {
            Type = "event";
        }
    }

    public record EventHeaderDto : ISearchHeader, IAddressHeader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public double? Rating { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DatePosted { get; set; }
    }
}
