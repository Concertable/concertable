using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class ConcertMappers
{
    private static ConcertArtistDto ToConcertArtistDto(this ArtistEntity artist) => new()
    {
        Id = artist.Id,
        Name = artist.Name,
        Avatar = artist.User.Avatar,
        County = artist.User.Address?.County ?? string.Empty,
        Town = artist.User.Address?.Town ?? string.Empty,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto())
    };

    private static ConcertVenueDto ToConcertVenueDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        County = venue.User.Address?.County ?? string.Empty,
        Town = venue.User.Address?.Town ?? string.Empty,
        Latitude = venue.User.Location?.Y ?? 0.0,
        Longitude = venue.User.Location?.X ?? 0.0
    };

    public static ConcertDto ToDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        About = concert.About,
        BannerUrl = concert.BannerUrl ?? concert.Booking.Application.Artist.BannerUrl,
        Avatar = concert.Avatar ?? concert.Booking.Application.Artist.User.Avatar,
        Price = concert.Price,
        TotalTickets = concert.TotalTickets,
        AvailableTickets = concert.AvailableTickets,
        DatePosted = concert.DatePosted,
        StartDate = concert.Booking.Application.Opportunity.Period.Start,
        EndDate = concert.Booking.Application.Opportunity.Period.End,
        Venue = concert.Booking.Application.Opportunity.Venue.ToConcertVenueDto(),
        Artist = concert.Booking.Application.Artist.ToConcertArtistDto(),
        Genres = concert.ConcertGenres.Select(eg => eg.Genre.ToDto())
    };

    public static ConcertHeaderDto ToHeaderDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        ImageUrl = concert.Booking.Application.Artist.User.Avatar,
        StartDate = concert.Booking.Application.Opportunity.Period.Start,
        EndDate = concert.Booking.Application.Opportunity.Period.End,
        County = concert.Booking.Application.Opportunity.Venue.User.Address?.County ?? string.Empty,
        Town = concert.Booking.Application.Opportunity.Venue.User.Address?.Town ?? string.Empty,
        DatePosted = concert.DatePosted
    };

    public static ConcertHeaderDto ToHeaderDto(this ConcertDto concertDto) => new()
    {
        Id = concertDto.Id,
        Name = concertDto.Name,
        ImageUrl = concertDto.Artist.Avatar ?? string.Empty,
        StartDate = concertDto.StartDate,
        EndDate = concertDto.EndDate,
        County = concertDto.Venue.County,
        Town = concertDto.Venue.Town,
        DatePosted = concertDto.DatePosted
    };

    public static IEnumerable<ConcertDto> ToDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(e => e.ToDto());
}
