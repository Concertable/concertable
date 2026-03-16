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
        Concert = ticket.Concert.ToDto(),
        User = ticket.User.ToDto()
    };

    public static IEnumerable<TicketDto> ToDtos(this IEnumerable<TicketEntity> tickets) =>
        tickets.Select(t => t.ToDto());
}
