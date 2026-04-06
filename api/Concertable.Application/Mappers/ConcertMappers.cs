using Concertable.Application.DTOs;
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
        Price = concert.Price,
        TotalTickets = concert.TotalTickets,
        AvailableTickets = concert.AvailableTickets,
        DatePosted = concert.DatePosted,
        StartDate = concert.Application.Opportunity.StartDate,
        EndDate = concert.Application.Opportunity.EndDate,
        Venue = concert.Application.Opportunity.Venue.ToDto(),
        Artist = concert.Application.Artist.ToDto(),
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
        ImageUrl = concert.Application.Artist.ImageUrl,
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
        ImageUrl = concertDto.Artist.ImageUrl,
        StartDate = concertDto.StartDate,
        EndDate = concertDto.EndDate,
        County = concertDto.Venue.County,
        Town = concertDto.Venue.Town,
        DatePosted = concertDto.DatePosted
    };

    public static IEnumerable<ConcertDto> ToDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(e => e.ToDto());

    public static IEnumerable<ConcertHeaderDto> ToHeaderDtos(this IEnumerable<ConcertEntity> concerts) =>
        concerts.Select(e => e.ToHeaderDto());
}
