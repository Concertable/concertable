using Concertable.Application.DTOs;
using Concertable.Core.Projections;
using Concertable.Core.Entities;
using LinqKit;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableArtistMappers
{
    public static IQueryable<ArtistHeaderDto> ToHeaderDtos(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query.AsExpandable()
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistHeaderDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = a.User.Avatar,
            Rating = rating.AverageRating,
            County = a.User.Address.County ?? string.Empty,
            Town = a.User.Address.Town ?? string.Empty,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };

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
            Avatar = a.User.Avatar,
            Rating = (double?)rating.AverageRating ?? 0.0,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };

    public static IQueryable<ArtistDto> ToDto(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query.AsExpandable()
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistDto
        {
            Id = a.Id,
            Name = a.Name,
            About = a.About,
            BannerUrl = a.BannerUrl,
            Avatar = a.User.Avatar,
            County = a.User.Address.County ?? string.Empty,
            Town = a.User.Address.Town ?? string.Empty,
            Email = a.User.Email ?? string.Empty,
            Rating = (double?)rating.AverageRating ?? 0.0,
            Genres = GenreSelectors.FromArtist.Invoke(a)
        };
}
