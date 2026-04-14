using Concertable.Application.Responses;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionService
{
    Task LogAsync(ITransaction dto);
    Task CompleteAsync(string paymentIntentId);
    Task<IPagination<ITransaction>> GetAsync(IPageParams pageParams);
}
