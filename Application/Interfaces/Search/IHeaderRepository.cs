using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search
{
    public interface IHeaderRepository<TEntity>
    {
        Task<Pagination<TEntity>> SearchAsync(SearchParams searchParams);
    }
}
