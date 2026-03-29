using Concertable.Application.Responses;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IHeaderRepository<TEntity>
{
    Task<Pagination<TEntity>> SearchAsync(SearchParams searchParams);
}
