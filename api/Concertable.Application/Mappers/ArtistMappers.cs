using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class ArtistMappers
{
    public static ArtistDto ToDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        About = artist.About,
        BannerUrl = artist.BannerUrl,
        Avatar = artist.User.Avatar,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()),
        County = artist.User.Address?.County ?? string.Empty,
        Town = artist.User.Address?.Town ?? string.Empty,
        Email = artist.User.Email ?? string.Empty
    };

    public static ArtistSummaryDto ToSummaryDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        Avatar = artist.User.Avatar,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()),
    };

    public static IEnumerable<ArtistDto> ToDtos(this IEnumerable<ArtistEntity> artists) =>
        artists.Select(a => a.ToDto());
}
