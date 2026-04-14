using FluentResults;

namespace Concertable.Application.Interfaces;

public interface ITicketValidator
{
    Task<Result> CanPurchaseTicketAsync(int concertId, int? quantity = null);
}
