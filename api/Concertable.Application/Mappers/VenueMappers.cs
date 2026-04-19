using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;
using Concertable.Application.Exceptions;

namespace Concertable.Application.Mappers;

public static class VenueMappers
{
    public static VenueDto ToDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        About = venue.About,
        BannerUrl = venue.BannerUrl,
        Avatar = venue.User.Avatar,
        Approved = venue.Approved,
        County = venue.User.Address?.County ?? string.Empty,
        Town = venue.User.Address?.Town ?? string.Empty,
        Email = venue.User.Email ?? string.Empty,
        Latitude = venue.User.Location?.Y ?? throw new InternalServerException($"Venue {venue.Id} user has no location set."),
        Longitude = venue.User.Location?.X ?? throw new InternalServerException($"Venue {venue.Id} user has no location set.")
    };

    public static VenueSummaryDto ToSummaryDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        Avatar = venue.User.Avatar,
    };

    public static IEnumerable<VenueDto> ToDtos(this IEnumerable<VenueEntity> venues) =>
        venues.Select(v => v.ToDto());
}
