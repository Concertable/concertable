using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search
{
    public interface ISearchRepository<TEntity>
    {
        Task<Pagination<TEntity>> SearchAsync(SearchParams searchParams);
    }
}
