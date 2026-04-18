using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Exceptions;
using Concertable.Core.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Core.Entities;

public class VenueEntity : IIdEntity, IHasName, ILocatable<VenueEntity>, IReviewable<VenueEntity>
{
    private VenueEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public bool Approved { get; private set; }
    public VenueManagerEntity User { get; set; } = null!;
    public static Expression<Func<VenueEntity, Point?>> LocationExpression => v => v.User.Location;
    public static Expression<Func<ReviewEntity, int>> ReviewIdSelector => r => r.Ticket.Concert.Application.Opportunity.VenueId;

    public ICollection<OpportunityEntity> Opportunities { get; private set; } = [];

    public static VenueEntity Create(Guid userId, string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        return new() { UserId = userId, Name = name, About = about, BannerUrl = bannerUrl };
    }

    public void Update(string name, string about, string bannerUrl)
    {
        ValidateFields(name, about, bannerUrl);
        Name = name;
        About = about;
        BannerUrl = bannerUrl;
    }

    public void Approve() => Approved = true;

    private static void ValidateFields(string name, string about, string bannerUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(about)) throw new DomainException("About is required.");
        if (string.IsNullOrWhiteSpace(bannerUrl)) throw new DomainException("Banner URL is required.");
    }
}
