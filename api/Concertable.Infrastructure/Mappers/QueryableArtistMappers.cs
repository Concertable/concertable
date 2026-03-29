using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableArtistMappers
{
    public static IQueryable<ArtistHeaderDto> ToHeaderDtos(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistHeaderDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = a.ImageUrl,
            Rating = rating.AverageRating,
            County = a.User.County ?? string.Empty,
            Town = a.User.Town ?? string.Empty,
            Latitude = a.User.Location != null ? a.User.Location.Y : 0.0,
            Longitude = a.User.Location != null ? a.User.Location.X : 0.0
        };
}
