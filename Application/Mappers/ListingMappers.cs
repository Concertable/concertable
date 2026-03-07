using Application.DTOs;
using Core.Entities;

namespace Application.Mappers
{
    public static class ListingMappers
    {
        public static ListingDto ToDto(this Listing listing) => new()
        {
            Id = listing.Id,
            StartDate = listing.StartDate,
            EndDate = listing.EndDate,
            Pay = listing.Pay,
            Genres = listing.ListingGenres.Select(lg => lg.Genre.ToDto())
        };

        public static Listing ToEntity(this ListingDto dto) => new()
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Pay = dto.Pay,
            ListingGenres = dto.Genres.Select(g => new ListingGenre { GenreId = g.Id }).ToList()
        };

        public static ListingWithVenueDto ToWithVenueDto(this Listing listing) => new()
        {
            Listing = listing.ToDto(),
            Venue = listing.Venue.ToDto()
        };

        public static IEnumerable<ListingDto> ToDtos(this IEnumerable<Listing> listings) =>
            listings.Select(l => l.ToDto());
    }
}
