using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Mappers;

public static class ArtistMappers
{
    public static ArtistDto ToDto(this Artist artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        About = artist.About,
        ImageUrl = artist.ImageUrl,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()),
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Email = artist.User.Email ?? string.Empty,
        Latitude = artist.User.Location?.Y,
        Longitude = artist.User.Location?.X
    };

    public static ArtistHeaderDto ToHeaderDto(this Artist artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        ImageUrl = artist.ImageUrl,
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Latitude = artist.User.Location?.Y,
        Longitude = artist.User.Location?.X
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

    public static Artist ToEntity(this CreateArtistRequest request) => new()
    {
        Name = request.Name,
        About = request.About,
        ImageUrl = string.Empty, // Set by caller after image upload
        ArtistGenres = request.Genres.Select(g => new ArtistGenre { GenreId = g.Id }).ToList()
    };

    public static Artist ToEntity(this ArtistDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        ImageUrl = dto.ImageUrl,
        ArtistGenres = dto.Genres.Select(g => new ArtistGenre { GenreId = g.Id }).ToList()
    };

    public static IEnumerable<ArtistDto> ToDtos(this IEnumerable<Artist> artists) =>
        artists.Select(a => a.ToDto());

    public static IEnumerable<ArtistHeaderDto> ToHeaderDtos(this IEnumerable<Artist> artists) =>
        artists.Select(a => a.ToHeaderDto());
}
