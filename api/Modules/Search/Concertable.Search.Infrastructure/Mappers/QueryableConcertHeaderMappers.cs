using Concertable.Concert.Domain;
using Concertable.Infrastructure.Mappers;
using LinqKit;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class QueryableConcertHeaderMappers
{
    public static IQueryable<ConcertHeaderDto> ToHeaderDtos(
        this IQueryable<ConcertEntity> query,
        IQueryable<ConcertRatingProjection> ratings) =>
        from c in query.Where(c => c.Booking.Application.Opportunity.Venue.Location != null).AsExpandable()
        join r in ratings on c.Id equals r.ConcertId into rg
        from rating in rg.DefaultIfEmpty()
        select new ConcertHeaderDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Booking.Application.Artist.Avatar,
            Rating = rating != null ? rating.AverageRating : null,
            StartDate = c.Booking.Application.Opportunity.Period.Start,
            EndDate = c.Booking.Application.Opportunity.Period.End,
            DatePosted = c.DatePosted,
            County = c.Booking.Application.Opportunity.Venue.County ?? string.Empty,
            Town = c.Booking.Application.Opportunity.Venue.Town ?? string.Empty,
            Genres = GenreSelectors.FromConcert.Invoke(c)
        };
}
