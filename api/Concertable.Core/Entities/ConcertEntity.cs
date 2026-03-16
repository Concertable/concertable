
using Core.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class ConcertEntity : BaseEntity, IHasName, IHasLocation
{
    public int ApplicationId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public DateTime? DatePosted { get; set; }
    public Point? Location => Application.Opportunity.Venue.User.Location;
    public ConcertApplicationEntity Application { get; set; } = null!;
    public ICollection<TicketEntity> Tickets { get; } = new List<TicketEntity>();
    public ICollection<ConcertGenreEntity> ConcertGenres { get; set; } = new List<ConcertGenreEntity>();
    public ICollection<ConcertImageEntity> Images { get; set; } = new List<ConcertImageEntity>();
}
