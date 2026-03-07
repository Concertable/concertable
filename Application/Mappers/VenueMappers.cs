using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Mappers
{
    public static class VenueMappers
    {
        public static VenueDto ToDto(this Venue venue) => new()
        {
            Id = venue.Id,
            Name = venue.Name,
            About = venue.About,
            ImageUrl = venue.ImageUrl,
            Approved = venue.Approved,
            County = venue.User?.County,
            Town = venue.User?.Town,
            Email = venue.User?.Email,
            Latitude = venue.User?.Location?.Y ?? 0,
            Longitude = venue.User?.Location?.X ?? 0
        };

        public static VenueHeaderDto ToHeaderDto(this Venue venue) => new()
        {
            Id = venue.Id,
            Name = venue.Name,
            ImageUrl = venue.ImageUrl,
            County = venue.User?.County,
            Town = venue.User?.Town,
            Latitude = venue.User?.Location?.Y,
            Longitude = venue.User?.Location?.X
        };

        public static VenueHeaderDto ToHeaderDto(this VenueDto venueDto) => new()
        {
            Id = venueDto.Id,
            Name = venueDto.Name,
            ImageUrl = venueDto.ImageUrl,
            County = venueDto.County,
            Town = venueDto.Town,
            Latitude = venueDto.Latitude,
            Longitude = venueDto.Longitude
        };

        public static Venue ToEntity(this CreateVenueRequest request) => new()
        {
            Name = request.Name,
            About = request.About
        };

public static IEnumerable<VenueDto> ToDtos(this IEnumerable<Venue> venues) =>
            venues.Select(v => v.ToDto());

        public static IEnumerable<VenueHeaderDto> ToHeaderDtos(this IEnumerable<Venue> venues) =>
            venues.Select(v => v.ToHeaderDto());
    }
}
