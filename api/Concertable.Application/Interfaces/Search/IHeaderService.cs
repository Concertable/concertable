using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IHeaderService
{
    Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(int amount);
}
