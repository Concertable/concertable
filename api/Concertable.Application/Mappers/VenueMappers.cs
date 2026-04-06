using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;

namespace Concertable.Application.Mappers;

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

    public static VenueSummaryDto ToSummaryDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        ImageUrl = venue.ImageUrl,
    };

    public static VenueHeaderDto ToHeaderDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        ImageUrl = venue.ImageUrl,
        County = venue.User.County ?? string.Empty,
        Town = venue.User.Town ?? string.Empty
    };

    public static VenueHeaderDto ToHeaderDto(this VenueDto venueDto) => new()
    {
        Id = venueDto.Id,
        Name = venueDto.Name,
        ImageUrl = venueDto.ImageUrl,
        County = venueDto.County,
        Town = venueDto.Town
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
