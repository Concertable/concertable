using Application.DTOs;
using Core.Entities;

namespace Application.Mappers
{
    public static class ConcertMappers
    {
        public static ConcertDto ToDto(this Concert concert) => new()
        {
            Id = concert.Id,
            Name = concert.Name,
            About = concert.About,
            Price = concert.Price,
            TotalTickets = concert.TotalTickets,
            AvailableTickets = concert.AvailableTickets,
            DatePosted = concert.DatePosted,
            StartDate = concert.Application?.Listing?.StartDate ?? default,
            EndDate = concert.Application?.Listing?.EndDate ?? default,
            Venue = concert.Application?.Listing?.Venue?.ToDto(),
            Artist = concert.Application?.Artist?.ToDto(),
            Genres = concert.ConcertGenres?.Select(eg => eg.Genre.ToDto()) ?? Enumerable.Empty<GenreDto>()
        };

        public static ConcertHeaderDto ToHeaderDto(this Concert concert) => new()
        {
            Id = concert.Id,
            Name = concert.Name,
            ImageUrl = concert.Application?.Artist?.ImageUrl,
            StartDate = concert.Application?.Listing?.StartDate ?? default,
            EndDate = concert.Application?.Listing?.EndDate ?? default,
            County = concert.Application?.Listing?.Venue?.User?.County,
            Town = concert.Application?.Listing?.Venue?.User?.Town,
            Latitude = concert.Application?.Listing?.Venue?.User?.Location?.Y,
            Longitude = concert.Application?.Listing?.Venue?.User?.Location?.X,
            DatePosted = concert.DatePosted
        };

        public static ConcertHeaderDto ToHeaderDto(this ConcertDto concertDto) => new()
        {
            Id = concertDto.Id,
            Name = concertDto.Name,
            ImageUrl = concertDto.Artist?.ImageUrl,
            StartDate = concertDto.StartDate,
            EndDate = concertDto.EndDate,
            County = concertDto.Venue?.County,
            Town = concertDto.Venue?.Town,
            Latitude = concertDto.Venue?.Latitude,
            Longitude = concertDto.Venue?.Longitude,
            DatePosted = concertDto.DatePosted
        };

        public static IEnumerable<ConcertDto> ToDtos(this IEnumerable<Concert> concerts) =>
            concerts.Select(e => e.ToDto());

        public static IEnumerable<ConcertHeaderDto> ToHeaderDtos(this IEnumerable<Concert> concerts) =>
            concerts.Select(e => e.ToHeaderDto());
    }
}
