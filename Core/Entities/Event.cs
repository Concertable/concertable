
namespace Core.Entities
{
    public class Event : BaseEntity
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        //public string About { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime? DatePosted { get; set; }
        public ListingApplication Application { get; set; }
        public ICollection<Ticket> Tickets { get; }
        public ICollection<EventImage> Images { get; }
        public ICollection<EventGenre> EventGenre { get; }
    }
}
