using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class TicketMappers
{
    public static TicketDto ToDto(this TicketEntity ticket, string email) => new()
    {
        Id = ticket.Id,
        PurchaseDate = ticket.PurchaseDate,
        QrCode = ticket.QrCode,
        Concert = ToTicketConcertDto(ticket.Concert),
        User = new UserDto
        {
            Id = ticket.UserId,
            Email = email
        }
    };

    private static TicketConcertDto ToTicketConcertDto(ConcertEntity concert) => new()
    {
        Id = concert.Id,
        Name = concert.Name,
        Price = concert.Price,
        StartDate = concert.Booking.Application.Opportunity.Period.Start,
        EndDate = concert.Booking.Application.Opportunity.Period.End,
        VenueName = concert.Booking.Application.Opportunity.Venue.Name,
        ArtistName = concert.Booking.Application.Artist.Name
    };

    public static IEnumerable<TicketDto> ToDtos(this IEnumerable<TicketEntity> tickets, string email) =>
        tickets.Select(t => t.ToDto(email));
}
