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
        County = artist.User.County ?? string.Empty,
        Town = artist.User.Town ?? string.Empty,
        Genres = artist.ArtistGenres.Select(ag => ag.Genre.ToDto())
    };

    private static ConcertVenueDto ToConcertVenueDto(this VenueEntity venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        County = venue.User.County ?? string.Empty,
        Town = venue.User.Town ?? string.Empty,
        Latitude = venue.User.Location?.Y ?? 0.0,
        Longitude = venue.User.Location?.X ?? 0.0
    };

    public static ConcertDto ToDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        About = concert.About,
        BannerUrl = concert.BannerUrl ?? concert.Application.Artist.BannerUrl,
        Avatar = concert.Avatar ?? concert.Application.Artist.User.Avatar,
        Price = concert.Price,
        TotalTickets = concert.TotalTickets,
        AvailableTickets = concert.AvailableTickets,
        DatePosted = concert.DatePosted,
        StartDate = concert.Application.Opportunity.StartDate,
        EndDate = concert.Application.Opportunity.EndDate,
        Venue = concert.Application.Opportunity.Venue.ToConcertVenueDto(),
        Artist = concert.Application.Artist.ToConcertArtistDto(),
        Genres = concert.ConcertGenres.Select(eg => eg.Genre.ToDto())
    };

    public static ConcertHeaderDto ToHeaderDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        ImageUrl = concert.Application.Artist.User.Avatar,
        StartDate = concert.Application.Opportunity.StartDate,
        EndDate = concert.Application.Opportunity.EndDate,
        County = concert.Application.Opportunity.Venue.User.County ?? string.Empty,
        Town = concert.Application.Opportunity.Venue.User.Town ?? string.Empty,
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
