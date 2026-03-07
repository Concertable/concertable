using Application.DTOs;
using Core.Entities;

namespace Application.Mappers
{
    public static class ListingApplicationMappers
    {
        public static ListingApplicationDto ToDto(this ListingApplication application) => new()
        {
            Id = application.Id,
            Artist = application.Artist.ToDto(),
            Listing = application.Listing.ToDto()
        };

        public static ArtistListingApplicationDto ToArtistListingApplicationDto(this ListingApplication application) => new()
        {
            Id = application.Id,
            Artist = application.Artist.ToDto(),
            ListingWithVenue = application.Listing.ToWithVenueDto()
        };

        public static IEnumerable<ListingApplicationDto> ToDtos(this IEnumerable<ListingApplication> applications) =>
            applications.Select(a => a.ToDto());

        public static IEnumerable<ArtistListingApplicationDto> ToArtistListingApplicationDtos(this IEnumerable<ListingApplication> applications) =>
            applications.Select(a => a.ToArtistListingApplicationDto());
    }
}
