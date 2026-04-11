using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Concertable.Core.Entities;

public class ConcertEntity : IIdEntity, IHasName, ILocatable<ConcertEntity>, IReviewable<ConcertEntity>
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public required string Name { get; set; }
    public required string About { get; set; }
    public string? BannerUrl { get; set; }
    public string? Avatar { get; set; }
    public decimal Price { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
    public DateTime? DatePosted { get; set; }
    public static Expression<Func<ConcertEntity, Point?>> LocationExpression => c => c.Application.Opportunity.Venue.User.Location;
    public static Expression<Func<ReviewEntity, int>> ReviewIdSelector => r => r.Ticket.ConcertId;
    public OpportunityApplicationEntity Application { get; set; } = null!;
    public ICollection<TicketEntity> Tickets { get; } = new List<TicketEntity>();
    public ICollection<ConcertGenreEntity> ConcertGenres { get; set; } = new List<ConcertGenreEntity>();
    public ICollection<ConcertImageEntity> Images { get; set; } = new List<ConcertImageEntity>();
}
