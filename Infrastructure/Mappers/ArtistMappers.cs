using Application.DTOs;
using Core.Entities;

namespace Infrastructure.Mappers
{
    public static class ArtistMappers
    {
        public static ArtistHeaderDto ToSearchHeader(this Artist artist) => new ArtistHeaderDto
        {
            Id = artist.Id,
            Name = artist.Name,
            ImageUrl = artist.ImageUrl,
            County = artist.User?.County,
            Town = artist.User?.Town,
            Latitude = artist.User?.Location?.Y,
            Longitude = artist.User?.Location?.X
        };

        public static ArtistDto ToDto(this Artist artist) => new ArtistDto
        {
            Id = artist.Id,
            Name = artist.Name,
            About = artist.About,
            ImageUrl = artist.ImageUrl,
            County = artist.User?.County,
            Town = artist.User?.Town,
            Latitude = artist.User?.Location?.Y,
            Longitude = artist.User?.Location?.X,
            Email = artist.User?.Email,
            Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto()).ToList()
        };

        /// <summary>
        /// Maps DTO fields that live directly on the Artist entity.
        /// ArtistGenres and User properties (County, Town, Location) are managed separately.
        /// </summary>
        public static Artist ToEntity(this ArtistDto dto) => new Artist
        {
            Id = dto.Id,
            Name = dto.Name,
            About = dto.About,
            ImageUrl = dto.ImageUrl
        };
    }
}
