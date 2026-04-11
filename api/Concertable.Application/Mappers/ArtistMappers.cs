using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
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
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Email = artist.User.Email ?? string.Empty
    };

    public static ConcertArtistDto ToConcertArtistDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        About = artist.About,
        Avatar = artist.User.Avatar,
        BannerUrl = artist.BannerUrl,
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto())
    };

    public static ArtistSummaryDto ToSummaryDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        Avatar = artist.User.Avatar,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()),
    };

    public static ArtistHeaderDto ToHeaderDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        ImageUrl = artist.User.Avatar,
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty
    };

    public static ArtistHeaderDto ToHeaderDto(this ArtistDto artistDto) => new()
    {
        Id = artistDto.Id,
        Name = artistDto.Name,
        ImageUrl = artistDto.Avatar ?? string.Empty,
        County = artistDto.County,
        Town = artistDto.Town
    };

    public static ArtistEntity ToEntity(this CreateArtistRequest request) => new()
    {
        Name = request.Name,
        About = request.About,
        BannerUrl = string.Empty,
        ArtistGenres = request.Genres.Select(g => new ArtistGenreEntity { GenreId = g.Id }).ToList()
    };

    public static ArtistEntity ToEntity(this ArtistDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        ArtistGenres = dto.Genres.Select(g => new ArtistGenreEntity { GenreId = g.Id }).ToList()
    };

    public static ArtistDetailsResponse ToDetailsResponse(this ArtistDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        Avatar = dto.Avatar,
        Rating = dto.Rating,
        Genres = dto.Genres,
        County = dto.County,
        Town = dto.Town,
        Email = dto.Email
    };

    public static IEnumerable<ArtistDto> ToDtos(this IEnumerable<ArtistEntity> artists) =>
        artists.Select(a => a.ToDto());

    public static IEnumerable<ArtistHeaderDto> ToHeaderDtos(this IEnumerable<ArtistEntity> artists) =>
        artists.Select(a => a.ToHeaderDto());
}
