namespace Application.DTOs
{
    public class EventDto : ItemDto
    {
        public decimal Price { get; set; }
        public int? TotalTickets { get; set; }
        public int? AvailableTickets { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DatePosted { get; set; }
        public VenueDto Venue { get; set; }
        public ArtistDto Artist { get; set; }

        public EventDto()
        {
            Type = "event";
        }
    }
}
