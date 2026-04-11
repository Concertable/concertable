using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Core.Entities;
using Concertable.Core.Exceptions;

namespace Concertable.Application.Mappers;

public static class ConcertMappers
{
    public static ConcertDto ToDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        About = concert.About,
        BannerUrl = concert.BannerUrl,
        Avatar = concert.Avatar,
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

    public static ConcertSummaryDto ToSummaryDto(this ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        Price = concert.Price,
        TotalTickets = concert.TotalTickets,
        AvailableTickets = concert.AvailableTickets,
        DatePosted = concert.DatePosted,
        StartDate = concert.Application.Opportunity.StartDate,
        EndDate = concert.Application.Opportunity.EndDate,
        Venue = concert.Application.Opportunity.Venue.ToSummaryDto(),
        Artist = concert.Application.Artist.ToSummaryDto(),
    };

    public static IEnumerable<ConcertSummaryDto> ToSummaryDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(c => c.ToSummaryDto());

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

    public static ConcertDetailsResponse ToDetailsResponse(this ConcertDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl ?? dto.Artist.BannerUrl ?? string.Empty,
        Avatar = dto.Avatar ?? dto.Artist.Avatar ?? string.Empty,
        Rating = dto.Rating,
        Price = dto.Price,
        TotalTickets = dto.TotalTickets,
        AvailableTickets = dto.AvailableTickets,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        DatePosted = dto.DatePosted,
        Genres = dto.Genres,
        Artist = new ConcertArtistResponse
        {
            Id = dto.Artist.Id,
            Name = dto.Artist.Name,
            About = dto.Artist.About,
            Rating = dto.Artist.Rating,
            County = dto.Artist.County,
            Town = dto.Artist.Town,
            Genres = dto.Artist.Genres
        },
        Venue = new ConcertVenueResponse
        {
            Id = dto.Venue.Id,
            Name = dto.Venue.Name,
            County = dto.Venue.County,
            Town = dto.Venue.Town,
            Latitude = dto.Venue.Latitude,
            Longitude = dto.Venue.Longitude
        }
    };

    public static IEnumerable<ConcertDto> ToDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(e => e.ToDto());

    public static IEnumerable<ConcertHeaderDto> ToHeaderDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(e => e.ToHeaderDto());
}
