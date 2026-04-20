using Concertable.Application.Interfaces;
using Concertable.Core.Parameters;
using Concertable.Search.Contracts;

namespace Concertable.Search.Application.Interfaces;

public interface IHeaderService
{
    Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(int amount);
}
