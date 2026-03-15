using Application.DTOs;
using Application.Requests;
using Core.Entities;
using Core.Exceptions;

namespace Application.Mappers;

public static class VenueMappers
{
    public static VenueDto ToDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        About = venue.About,
        ImageUrl = venue.ImageUrl,
        Approved = venue.Approved,
        County = venue.User.County ?? string.Empty,
        Town = venue.User.Town ?? string.Empty,
        Email = venue.User.Email ?? string.Empty,
        Latitude = venue.User.Location?.Y ?? throw new InternalServerException($"Venue {venue.Id} user has no location set."),
        Longitude = venue.User.Location?.X ?? throw new InternalServerException($"Venue {venue.Id} user has no location set.")
    };

    public static VenueHeaderDto ToHeaderDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        ImageUrl = venue.ImageUrl,
        County = venue.User.County ?? string.Empty,
        Town = venue.User.Town ?? string.Empty,
        Latitude = venue.User.Location?.Y ?? throw new InternalServerException($"Venue {venue.Id} user has no location set."),
        Longitude = venue.User.Location?.X ?? throw new InternalServerException($"Venue {venue.Id} user has no location set.")
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

    public static VenueEntity ToEntity(this CreateVenueRequest request) => new()
    {
        Name = request.Name,
        About = request.About,
        ImageUrl = string.Empty
    };

    public static IEnumerable<VenueDto> ToDtos(this IEnumerable<VenueEntity> venues) =>
        venues.Select(v => v.ToDto());

    public static IEnumerable<VenueHeaderDto> ToHeaderDtos(this IEnumerable<VenueEntity> venues) =>
        venues.Select(v => v.ToHeaderDto());
}
