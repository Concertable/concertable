using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Concertable.Core.Parameters;

namespace Concertable.Search.Contracts;

public interface IHeaderModule
{
    Task<IPagination<IHeader>> SearchAsync(SearchParams searchParams);
    Task<IEnumerable<IHeader>> GetByAmountAsync(HeaderType type, int amount);
}
