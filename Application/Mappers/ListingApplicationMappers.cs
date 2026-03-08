using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Application.Mappers;

public static class ListingApplicationMappers
{
    public static ListingApplicationDto ToDto(this ListingApplication application) => new(
        application.Id,
        application.Artist.ToDto(),
        application.Listing.ToDto(),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public static ArtistListingApplicationDto ToArtistListingApplicationDto(this ListingApplication application) => new(
        application.Id,
        application.Artist.ToDto(),
        application.Listing.ToWithVenueDto(),
        application.Concert != null ? ApplicationStatus.Accepted : ApplicationStatus.Pending
    );

    public static IEnumerable<ListingApplicationDto> ToDtos(this IEnumerable<ListingApplication> applications) =>
        applications.Select(a => a.ToDto());

    public static IEnumerable<ArtistListingApplicationDto> ToArtistListingApplicationDtos(this IEnumerable<ListingApplication> applications) =>
        applications.Select(a => a.ToArtistListingApplicationDto());
}
