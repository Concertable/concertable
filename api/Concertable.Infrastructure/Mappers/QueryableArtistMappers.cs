using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;
using LinqKit;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableArtistMappers
{
    public static IQueryable<ArtistSummaryDto> ToSummaryDto(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query.AsExpandable()
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistSummaryDto
        {
            Id = a.Id,
            Name = a.Name,
            Avatar = a.Avatar,
            Rating = (double?)rating.AverageRating ?? 0.0,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };

    public static IQueryable<ArtistDto> ToDto(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query.AsExpandable()
        where a.Address != null
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistDto
        {
            Id = a.Id,
            Name = a.Name,
            About = a.About,
            BannerUrl = a.BannerUrl,
            Avatar = a.Avatar,
            County = a.Address!.County,
            Town = a.Address!.Town,
            Email = a.Email ?? string.Empty,
            Rating = (double?)rating.AverageRating ?? 0.0,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };
}
