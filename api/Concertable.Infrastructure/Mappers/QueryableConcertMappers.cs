using Application.DTOs;
using Core.Projections;
using Core.Entities;

namespace Infrastructure.Mappers;

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
            ImageUrl = c.Application.Artist.ImageUrl,
            Rating = rating.AverageRating,
            StartDate = c.Application.Opportunity.StartDate,
            EndDate = c.Application.Opportunity.EndDate,
            DatePosted = c.DatePosted,
            County = c.Application.Opportunity.Venue.User.County ?? string.Empty,
            Town = c.Application.Opportunity.Venue.User.Town ?? string.Empty,
            Latitude = c.Application.Opportunity.Venue.User.Location != null ? c.Application.Opportunity.Venue.User.Location.Y : 0.0,
            Longitude = c.Application.Opportunity.Venue.User.Location != null ? c.Application.Opportunity.Venue.User.Location.X : 0.0
        };
}
