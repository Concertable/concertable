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
            ImageUrl = a.User.Avatar,
            Rating = rating.AverageRating,
            County = a.User.County ?? string.Empty,
            Town = a.User.Town ?? string.Empty
        };

    public static IQueryable<ArtistDto> ToDto(
        this IQueryable<ArtistEntity> query,
        IQueryable<RatingAggregate> ratings) =>
        from a in query
        join r in ratings on a.Id equals r.EntityId into rg
        from rating in rg.DefaultIfEmpty()
        select new ArtistDto
        {
            Id = a.Id,
            Name = a.Name,
            About = a.About,
            BannerUrl = a.BannerUrl,
            Avatar = a.User.Avatar,
            County = a.User.County ?? string.Empty,
            Town = a.User.Town ?? string.Empty,
            Email = a.User.Email ?? string.Empty,
            Rating = (double?)rating.AverageRating ?? 0.0,
            Genres = a.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name))
        };
}
