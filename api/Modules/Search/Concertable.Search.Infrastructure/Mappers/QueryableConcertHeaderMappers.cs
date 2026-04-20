using Concertable.Core.Entities;
using Concertable.Core.Projections;
using Concertable.Infrastructure.Mappers;
using LinqKit;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class QueryableConcertHeaderMappers
{
    public static IQueryable<ConcertHeaderDto> ToHeaderDtos(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from c in query.Where(c => c.Booking.Application.Opportunity.Venue.User.Location != null
                                 && c.Booking.Application.Opportunity.Venue.User.Address != null).AsExpandable()
        join r in ratings on c.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ConcertHeaderDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Booking.Application.Artist.User.Avatar,
            Rating = rating.AverageRating,
            StartDate = c.Booking.Application.Opportunity.Period.Start,
            EndDate = c.Booking.Application.Opportunity.Period.End,
            DatePosted = c.DatePosted,
            County = c.Booking.Application.Opportunity.Venue.User.Address.County ?? string.Empty,
            Town = c.Booking.Application.Opportunity.Venue.User.Address.Town ?? string.Empty,
            Genres = GenreSelectors.FromConcert.Invoke(c)
        };
}
