using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketValidator
{
    Result CanBePurchased(ConcertEntity concert);
    Task<Result> CanBePurchasedAsync(int concertId);
    Result CanPurchaseTickets(ConcertEntity concert, int quantity);
}
