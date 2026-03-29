using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;

namespace Concertable.Application.Mappers;

public static class ArtistMappers
{
    public static ArtistDto ToDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        About = artist.About,
        ImageUrl = artist.ImageUrl,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()),
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Email = artist.User.Email ?? string.Empty,
        Latitude = artist.User.Location?.Y ?? throw new InternalServerException($"Artist {artist.Id} user has no location set."),
        Longitude = artist.User.Location?.X ?? throw new InternalServerException($"Artist {artist.Id} user has no location set.")
    };

    public static ArtistHeaderDto ToHeaderDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        ImageUrl = artist.ImageUrl,
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Latitude = artist.User.Location?.Y ?? throw new InternalServerException($"Artist {artist.Id} user has no location set."),
        Longitude = artist.User.Location?.X ?? throw new InternalServerException($"Artist {artist.Id} user has no location set.")
    };

    public static ArtistHeaderDto ToHeaderDto(this ArtistDto artistDto) => new()
    {
        Id = artistDto.Id,
        Name = artistDto.Name,
        ImageUrl = artistDto.ImageUrl,
        County = artistDto.County,
        Town = artistDto.Town,
        Latitude = artistDto.Latitude,
        Longitude = artistDto.Longitude
    };

    public static ArtistEntity ToEntity(this CreateArtistRequest request) => new()
    {
        Name = request.Name,
        About = request.About,
        ImageUrl = string.Empty, // Set by caller after image upload
        ArtistGenres = request.Genres.Select(g => new ArtistGenreEntity { GenreId = g.Id }).ToList()
    };

    public static ArtistEntity ToEntity(this ArtistDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        ImageUrl = dto.ImageUrl,
        ArtistGenres = dto.Genres.Select(g => new ArtistGenreEntity { GenreId = g.Id }).ToList()
    };

    public static IEnumerable<ArtistDto> ToDtos(this IEnumerable<ArtistEntity> artists) =>
        artists.Select(a => a.ToDto());

    public static IEnumerable<ArtistHeaderDto> ToHeaderDtos(this IEnumerable<ArtistEntity> artists) =>
        artists.Select(a => a.ToHeaderDto());
}
