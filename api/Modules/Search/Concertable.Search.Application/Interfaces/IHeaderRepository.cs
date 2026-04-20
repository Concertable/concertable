using Concertable.Application.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

internal interface IHeaderRepository<TEntity>
{
    Task<IPagination<TEntity>> SearchAsync(SearchParams searchParams);
}
