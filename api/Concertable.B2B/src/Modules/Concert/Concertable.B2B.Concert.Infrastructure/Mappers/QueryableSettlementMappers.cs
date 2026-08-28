using Concertable.B2B.Artist.Domain.ReadModels;
using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Domain.ReadModels;
using Concertable.B2B.Venue.Domain.ReadModels;

namespace Concertable.B2B.Concert.Infrastructure.Mappers;

internal static class QueryableSettlementMappers
{
    /// <summary>
    /// Manager-details projection in one round trip: the marketplace <see cref="ConcertDetails"/> shape
    /// joined to the concert's revenue-share settlement row (absent for a fixed-fee deal, or before the
    /// venue declares the door take).
    /// </summary>
    public static IQueryable<ManagerConcertDetailsProjection> ToManagerDetails(
        this IQueryable<ConcertEntity> query,
        IQueryable<ConcertRatingProjection> concertRatings,
        IQueryable<ArtistRatingProjection> artistRatings,
        IQueryable<VenueRatingProjection> venueRatings,
        IQueryable<RevenueShareSettlementEntity> settlements) =>
        from concert in query.ToDetails(concertRatings, artistRatings, venueRatings)
        select new ManagerConcertDetailsProjection
        {
            Concert = concert,
            Settlement = settlements
                .Where(s => s.ConcertId == concert.Id)
                .Select(s => new RevenueShareSettlementRowProjection(s.DoorRevenue, s.DeclaredAtUtc, s.Review))
                .FirstOrDefault()
        };
}
