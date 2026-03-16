using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IHeaderService
{
    Task<Pagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(int amount);
}
