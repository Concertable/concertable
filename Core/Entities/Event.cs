
using Core.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Event : BaseEntity, ILocation
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime? DatePosted { get; set; }
        public ListingApplication Application { get; set; }
        public ICollection<Ticket> Tickets { get; } = new List<Ticket>();
        public ICollection<EventImage> Images { get; } = new List<EventImage>();
        public ICollection<EventGenre> EventGenres { get; set; } = new List<EventGenre>();
        [NotMapped]
        public Point? Location => Application.Listing.Venue.User.Location;
    }
}
