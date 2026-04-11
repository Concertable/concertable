using Concertable.Application.Results;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderRepository<TEntity>
{
    Task<IPagination<TEntity>> SearchAsync(SearchParams searchParams);
}
