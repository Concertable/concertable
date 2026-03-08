
using Core.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Concert : BaseEntity, IHasName, IHasLocation
    {
        public int ApplicationId { get; set; }
        public required string Name { get; set; }
        public required string About { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public DateTime? DatePosted { get; set; }
        public Point? Location => Application.Listing.Venue.User.Location;
        public ListingApplication Application { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; } = new List<Ticket>();
        public ICollection<ConcertGenre> ConcertGenres { get; set; } = new List<ConcertGenre>();
        public ICollection<ConcertImage> Images { get; set; } = new List<ConcertImage>();
    }
}
