using Application.DTOs;
using Core.Projections;
using Core.Entities;

namespace Infrastructure.Mappers;

public static class QueryableVenueMappers
{
    public static IQueryable<VenueHeaderDto> ToHeaderDtos(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query
        join r in ratings on v.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new VenueHeaderDto
        {
            Id = v.Id,
            Name = v.Name,
            ImageUrl = v.ImageUrl,
            Rating = rating.AverageRating,
            County = v.User.County ?? string.Empty,
            Town = v.User.Town ?? string.Empty,
            Latitude = v.User.Location != null ? v.User.Location.Y : 0.0,
            Longitude = v.User.Location != null ? v.User.Location.X : 0.0
        };
}
