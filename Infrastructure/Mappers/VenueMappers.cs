using Application.DTOs;
using Core.Entities;

namespace Infrastructure.Mappers
{
    public static class VenueMappers
    {
        public static VenueHeaderDto ToSearchHeader(this Venue venue) => new VenueHeaderDto
        {
            Id = venue.Id,
            Name = venue.Name,
            ImageUrl = venue.ImageUrl,
            County = venue.User?.County,
            Town = venue.User?.Town,
            Latitude = venue.User?.Location?.Y,
            Longitude = venue.User?.Location?.X
        };

        public static VenueDto ToDto(this Venue venue) => new VenueDto
        {
            Id = venue.Id,
            Name = venue.Name,
            About = venue.About,
            ImageUrl = venue.ImageUrl,
            Approved = venue.Approved,
            County = venue.User?.County ?? string.Empty,
            Town = venue.User?.Town ?? string.Empty,
            Latitude = venue.User?.Location?.Y ?? 0,
            Longitude = venue.User?.Location?.X ?? 0,
            Email = venue.User?.Email ?? string.Empty
        };

        /// <summary>
        /// Maps DTO fields that live directly on the Venue entity.
        /// User properties (County, Town, Location, Email) are managed separately.
        /// </summary>
        public static Venue ToEntity(this VenueDto dto) => new Venue
        {
            Id = dto.Id,
            Name = dto.Name,
            About = dto.About,
            ImageUrl = dto.ImageUrl,
            Approved = dto.Approved
        };
    }
}
