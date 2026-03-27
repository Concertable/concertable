using Application.DTOs;
using Core.Entities;

namespace Application.Mappers;

public static class TicketMappers
{
    public static TicketDto ToDto(this TicketEntity ticket) => new()
    {
        Id = ticket.Id,
        PurchaseDate = ticket.PurchaseDate,
        QrCode = ticket.QrCode,
        Concert = ToTicketConcertDto(ticket.Concert),
        User = ticket.User.ToDto()
    };

    private static TicketConcertDto ToTicketConcertDto(ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        Price = concert.Price,
        StartDate = concert.Application.Opportunity.StartDate,
        EndDate = concert.Application.Opportunity.EndDate,
        VenueName = concert.Application.Opportunity.Venue.Name,
        ArtistName = concert.Application.Artist.Name
    };

    public static IEnumerable<TicketDto> ToDtos(this IEnumerable<TicketEntity> tickets) =>
        tickets.Select(t => t.ToDto());
}
