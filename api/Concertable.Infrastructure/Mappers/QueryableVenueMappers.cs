using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableVenueMappers
{
    public static IQueryable<VenueHeaderDto> ToHeaderDtos(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query.Where(v => v.User.Location != null && v.User.County != null && v.User.Town != null)
        join r in ratings on v.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new VenueHeaderDto
        {
            Id = v.Id,
            Name = v.Name,
            ImageUrl = v.User.Avatar,
            Rating = rating.AverageRating,
            County = v.User.County!,
            Town = v.User.Town!
        };

    public static IQueryable<VenueSummaryDto> ToSummaryDto(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query.Where(v => v.User.Location != null && v.User.County != null && v.User.Town != null)
        join r in ratings on v.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new VenueSummaryDto
        {
            Id = v.Id,
            Name = v.Name,
            Avatar = v.User.Avatar,
            Rating = (double?)rating.AverageRating ?? 0.0
        };

    public static IQueryable<VenueDto> ToDto(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query.Where(v => v.User.Location != null && v.User.County != null && v.User.Town != null)
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
            County = v.User.County!,
            Town = v.User.Town!,
            Email = v.User.Email ?? string.Empty,
            Latitude = v.User.Location!.Y,
            Longitude = v.User.Location!.X,
            Rating = (double?)rating.AverageRating ?? 0.0
        };
}
