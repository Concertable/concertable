using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

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
            ImageUrl = v.User.Avatar,
            Rating = rating.AverageRating,
            County = v.User.County ?? string.Empty,
            Town = v.User.Town ?? string.Empty
        };

    public static IQueryable<VenueDto> ToDto(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query
        join r in ratings on v.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new VenueDto
        {
            Id = v.Id,
            Name = v.Name,
            About = v.About,
            BannerUrl = v.BannerUrl,
            Avatar = v.User.Avatar,
            Approved = v.Approved,
            County = v.User.County ?? string.Empty,
            Town = v.User.Town ?? string.Empty,
            Email = v.User.Email ?? string.Empty,
            Latitude = v.User.Location!.Y,
            Longitude = v.User.Location!.X,
            Rating = (double?)rating.AverageRating ?? 0.0
        };
}
