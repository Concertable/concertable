using Concertable.Application.Results;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderService
{
    Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(int amount);
}
