using FluentResults;

namespace Concertable.Concert.Application.Interfaces;

internal interface ITicketValidator
{
    Task<Result> CanPurchaseTicketAsync(int concertId, int? quantity = null);
}
