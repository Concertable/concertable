using Concertable.Application.Results;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Payment;

public interface ITransactionService
{
    Task LogAsync(ITransaction dto);
    Task CompleteAsync(string paymentIntentId);
    Task<Pagination<ITransaction>> GetAsync(IPageParams pageParams);
}
