using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderService
{
    Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(int amount);
}
