using Concertable.Core.Entities;
using Concertable.Core.Projections;
using Concertable.Infrastructure.Mappers;
using LinqKit;

namespace Concertable.Search.Infrastructure.Mappers;

internal static class QueryableArtistHeaderMappers
{
    public static IQueryable<ArtistHeaderDto> ToHeaderDtos(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query.AsExpandable()
        where a.User.Address != null
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistHeaderDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = a.User.Avatar,
            Rating = rating.AverageRating,
            County = a.User.Address!.County,
            Town = a.User.Address!.Town,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };
}
