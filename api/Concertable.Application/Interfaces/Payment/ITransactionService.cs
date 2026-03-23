using Application.Responses;
using Core.Interfaces;
using Core.Parameters;

namespace Application.Interfaces.Payment;

public interface ITransactionService
{
    Task LogAsync(ITransaction dto);
    Task<Pagination<ITransaction>> GetAsync(IPageParams pageParams);
}
