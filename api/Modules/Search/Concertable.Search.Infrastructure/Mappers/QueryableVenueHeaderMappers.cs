using Concertable.Core.Projections;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class QueryableVenueHeaderMappers
{
    public static IQueryable<VenueHeaderDto> ToHeaderDtos(
        this IQueryable<VenueEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from v in query.Where(v => v.Location != null && v.Address != null)
        join r in ratings on v.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new VenueHeaderDto
        {
            Id = v.Id,
            Name = v.Name,
            ImageUrl = v.Avatar,
            Rating = rating.AverageRating,
            County = v.Address!.County,
            Town = v.Address!.Town
        };
}
