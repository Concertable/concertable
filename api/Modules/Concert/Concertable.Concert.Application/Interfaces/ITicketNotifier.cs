namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketNotifier
{
    Task TicketPurchasedAsync(string userId, object payload);
}
