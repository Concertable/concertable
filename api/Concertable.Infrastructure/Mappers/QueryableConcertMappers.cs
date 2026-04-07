using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableConcertMappers
{
    public static IQueryable<ConcertHeaderDto> ToHeaderDtos(
        this IQueryable<ConcertEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from c in query
        join r in ratings on c.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ConcertHeaderDto
        {
            Id = c.Id,
            Name = c.Name,
            ImageUrl = c.Application.Artist.User.Avatar,
            Rating = rating.AverageRating,
            StartDate = c.Application.Opportunity.StartDate,
            EndDate = c.Application.Opportunity.EndDate,
            DatePosted = c.DatePosted,
            County = c.Application.Opportunity.Venue.User.County ?? string.Empty,
            Town = c.Application.Opportunity.Venue.User.Town ?? string.Empty
        };
}
